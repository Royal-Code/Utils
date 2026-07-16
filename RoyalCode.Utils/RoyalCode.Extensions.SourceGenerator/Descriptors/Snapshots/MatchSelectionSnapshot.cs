using System.Collections.ObjectModel;
using RoyalCode.Extensions.SourceGenerator.Collections;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Snapshots;

/// <summary>
/// <para>
///     Creates immutable, symbol-free snapshots of property matching results for safe
///     retention by incremental generator pipelines.
/// </para>
/// <para>
///     This is the boundary between the two models. <see cref="MatchSelection"/> and the descriptors it
///     carries (<see cref="TypeDescriptor"/>, <see cref="PropertyDescriptor"/>, <c>AssignDescriptor</c>) are
///     the working model of the matching: they hold Roslyn symbols and mutable state, so retaining them in a
///     pipeline keeps the whole <c>Compilation</c> alive and makes caching unreliable. The snapshots are the
///     pipeline model: immutable, symbol-free, with value equality all the way down.
/// </para>
/// <para>
///     So resolve the match inside the transform, snapshot it, and let only the snapshot escape:
/// </para>
/// <code>
///     var provider = context.SyntaxProvider
///         .ForAttributeWithMetadataName(
///             "Some.AttributeName",
///             static (node, _) => node is TypeDeclarationSyntax,
///             static (ctx, _) =>
///             {
///                 // modelo de trabalho: símbolos, vivo apenas dentro do transform
///                 var selection = MatchSelection.Create(origin, target, ctx.SemanticModel);
///
///                 // modelo do pipeline: sem símbolos, comparável entre builds
///                 return MatchSelectionSnapshotFactory.Create(selection);
///             });
///
///     context.RegisterSourceOutput(provider, static (spc, snapshot) => Generate(spc, snapshot));
/// </code>
/// </summary>
public static class MatchSelectionSnapshotFactory
{
    /// <summary>
    /// Creates the snapshot of a matching result, to be retained by an incremental generator pipeline.
    /// </summary>
    /// <param name="selection">The matching result, which holds symbols and must not escape the transform.</param>
    /// <returns>An immutable, symbol-free snapshot with value equality.</returns>
    public static MatchSelectionSnapshot Create(MatchSelection selection) =>
        selection is null
            ? throw new ArgumentNullException(nameof(selection))
            : Create(selection, Array.Empty<PropertySnapshot>());

    private static MatchSelectionSnapshot Create(
        MatchSelection selection,
        IReadOnlyList<PropertySnapshot> parentPath)
    {
        var matches = selection.PropertyMatches.Select(match =>
        {
            var target = match.Target is null
                ? null
                : new PropertyPathSnapshot(parentPath.Concat(
                    match.Target.ToEnumerablePath().Select(item =>
                        PropertySnapshot.Create(item.PropertyType))));

            AssignmentSnapshot? assignment = null;
            if (match.AssignDescriptor is { } descriptor)
                assignment = CreateAssignment(descriptor, target);

            return new PropertyMatchSnapshot(
                PropertySnapshot.Create(match.Origin),
                target,
                assignment);
        });

        return new MatchSelectionSnapshot(
            TypeSnapshot.Create(selection.OriginType),
            TypeSnapshot.Create(selection.TargetType),
            matches);
    }

    private static AssignmentSnapshot CreateAssignment(
        AssignDescriptor descriptor,
        PropertyPathSnapshot? target)
    {
        // apenas o NewInstance projeta sobre o caminho da propriedade de destino;
        // os demais (inclusive o assignment do elemento de um Select) partem da raiz.
        var innerParent = descriptor.AssignType == AssignType.NewInstance && target is not null
            ? target.Properties
            : Array.Empty<PropertySnapshot>();

        return new AssignmentSnapshot(
            descriptor.AssignType,
            descriptor.Materialization,
            descriptor.InnerSelection is null
                ? null
                : Create(descriptor.InnerSelection, innerParent),
            // o assignment do elemento é relativo ao parâmetro do lambda do Select,
            // e não ao caminho da propriedade de destino: por isso não herda o target.
            descriptor.ElementAssignment is null
                ? null
                : CreateAssignment(descriptor.ElementAssignment, null));
    }
}

/// <summary>An immutable, symbol-free structural description of a type.</summary>
public sealed class TypeSnapshot : IEquatable<TypeSnapshot>
{
    private readonly ReadOnlyCollection<string> namespaces;

    private TypeSnapshot(
        string name,
        IEnumerable<string> namespaces,
        bool isNullable,
        bool isNullableReference,
        bool isNonNullableReference,
        bool isValueType,
        bool isNullableValueType,
        TypeDeclarationSnapshot? declaration,
        bool isVoid,
        bool isNamedType,
        bool isArray,
        int arrayRank,
        TypeSnapshot? elementType,
        TypeSnapshot? containingType,
        EquatableArray<TypeSnapshot> typeArguments,
        string? originalDefinitionMetadataName,
        string? originalDefinitionQualifiedMetadataName,
        bool hasCompleteShape)
    {
        Name = name;
        this.namespaces = Array.AsReadOnly(namespaces.ToArray());
        IsNullable = isNullable;
        IsNullableReference = isNullableReference;
        IsNonNullableReference = isNonNullableReference;
        IsValueType = isValueType;
        IsNullableValueType = isNullableValueType;
        Declaration = declaration;
        IsVoid = isVoid;
        IsNamedType = isNamedType;
        IsArray = isArray;
        ArrayRank = arrayRank;
        ElementType = elementType;
        ContainingType = containingType;
        TypeArguments = typeArguments;
        OriginalDefinitionMetadataName = originalDefinitionMetadataName;
        OriginalDefinitionQualifiedMetadataName = originalDefinitionQualifiedMetadataName;
        HasCompleteShape = hasCompleteShape;
    }

    /// <summary>The source-facing type name.</summary>
    public string Name { get; }

    /// <summary>The namespaces required to reference the type.</summary>
    public IReadOnlyList<string> Namespaces => namespaces;

    /// <summary>Whether the descriptor represents a nullable type.</summary>
    public bool IsNullable { get; }

    /// <summary>Whether the symbol is an annotated nullable reference type.</summary>
    public bool IsNullableReference { get; }

    /// <summary>Whether the symbol is a non-nullable reference type.</summary>
    public bool IsNonNullableReference { get; }

    /// <summary>Whether the type is a value type.</summary>
    public bool IsValueType { get; }

    /// <summary>Whether the type is a nullable value type.</summary>
    public bool IsNullableValueType { get; }

    /// <summary>The named type declaration, when one was resolved.</summary>
    public TypeDeclarationSnapshot? Declaration { get; }

    /// <summary>Whether the type is <c>void</c>.</summary>
    public bool IsVoid { get; }

    /// <summary>Whether the type is a named type.</summary>
    public bool IsNamedType { get; }

    /// <summary>Whether the type is an array (<c>T[]</c>).</summary>
    public bool IsArray { get; }

    /// <summary>
    /// The array rank, or zero when the type is not an array or its shape was not resolved semantically.
    /// </summary>
    public int ArrayRank { get; }

    /// <summary>For an array type, the snapshot of its element type; otherwise <see langword="null"/>.</summary>
    public TypeSnapshot? ElementType { get; }

    /// <summary>
    /// The constructed containing type for a nested named type; otherwise <see langword="null"/>. This preserves
    /// outer generic arguments, which are not included in the nested symbol's own <see cref="TypeArguments"/>.
    /// </summary>
    public TypeSnapshot? ContainingType { get; }

    /// <summary>
    /// The generic type arguments (empty when the type is not a constructed generic). Nested types are
    /// snapshotted recursively so the whole shape has value equality.
    /// </summary>
    public EquatableArray<TypeSnapshot> TypeArguments { get; }

    /// <summary>
    /// The metadata name of the original (unbound) definition when the type is a named type
    /// (e.g. <c>Task`1</c> for <c>Task&lt;T&gt;</c>), used to classify Task/ValueTask/Result and similar
    /// without depending on type arguments. <see langword="null"/> for arrays, type parameters and
    /// symbol-less snapshots.
    /// </summary>
    public string? OriginalDefinitionMetadataName { get; }

    /// <summary>
    /// The namespace- and containing-type-qualified metadata identity of the original named definition
    /// (e.g. <c>System.Threading.Tasks.Task`1</c>). Consumers should use this value when classifying known types.
    /// </summary>
    public string? OriginalDefinitionQualifiedMetadataName { get; }

    /// <summary>
    /// Whether all structural facts required for semantic classification were available. Symbol-less descriptors
    /// are deliberately marked incomplete instead of having their display names parsed heuristically.
    /// </summary>
    public bool HasCompleteShape { get; }

    /// <summary>
    /// The metadata name of the type (e.g. <c>List`1</c>), falling back to <see cref="Name"/> when there is
    /// no named declaration. It is derived from other fields, so it is not part of equality.
    /// </summary>
    public string MetadataName => Declaration?.MetadataName ?? Name;

    /// <summary>Whether values of the type may be null.</summary>
    public bool MayBeNull => IsNullable || IsNullableReference || Name.EndsWith("?", StringComparison.Ordinal);

    /// <summary>The source-facing type name without a trailing nullable marker.</summary>
    public string UnderlyingType => Name.EndsWith("?", StringComparison.Ordinal)
        ? Name.Substring(0, Name.Length - 1)
        : Name;

    /// <summary>Creates an immutable structural snapshot from a descriptor.</summary>
    /// <param name="descriptor">The descriptor to snapshot while its Roslyn symbol is available.</param>
    /// <returns>A symbol-free structural snapshot.</returns>
    public static TypeSnapshot Create(TypeDescriptor descriptor)
    {
        if (descriptor is null)
            throw new ArgumentNullException(nameof(descriptor));

        var symbol = descriptor.Symbol;

        var nullableValueType = descriptor.IsNullable &&
                                symbol is Microsoft.CodeAnalysis.INamedTypeSymbol nullableNamed &&
                                nullableNamed.TypeArguments.Length == 1 &&
                                nullableNamed.TypeArguments[0].IsValueType;

        var isVoid = descriptor.IsVoid ||
                     symbol?.SpecialType == Microsoft.CodeAnalysis.SpecialType.System_Void;

        var arraySymbol = symbol as Microsoft.CodeAnalysis.IArrayTypeSymbol;
        var isArray = arraySymbol is not null ||
                      (symbol is null && descriptor.IsArray);
        var arrayRank = arraySymbol?.Rank ?? 0;

        TypeSnapshot? elementType = null;
        if (arraySymbol is not null)
            elementType = Create(TypeDescriptor.Create(arraySymbol.ElementType));

        TypeSnapshot? containingType = null;
        if (symbol is Microsoft.CodeAnalysis.INamedTypeSymbol { ContainingType: { } containingTypeSymbol })
            containingType = Create(TypeDescriptor.Create(containingTypeSymbol));

        var typeArguments = EquatableArray<TypeSnapshot>.Empty;
        string? originalDefinitionMetadataName = null;
        string? originalDefinitionQualifiedMetadataName = null;
        if (symbol is Microsoft.CodeAnalysis.INamedTypeSymbol namedSymbol)
        {
            var originalDefinition = namedSymbol.OriginalDefinition;
            originalDefinitionMetadataName = originalDefinition.MetadataName;
            originalDefinitionQualifiedMetadataName = GetQualifiedMetadataName(originalDefinition);
            if (namedSymbol.TypeArguments.Length > 0)
                typeArguments = new EquatableArray<TypeSnapshot>(
                    namedSymbol.TypeArguments.Select(argument => Create(TypeDescriptor.Create(argument))));
        }

        var hasCompleteShape = symbol switch
        {
            null => false,
            Microsoft.CodeAnalysis.IPointerTypeSymbol => false,
            Microsoft.CodeAnalysis.IFunctionPointerTypeSymbol => false,
            _ => true,
        };

        return new TypeSnapshot(
            descriptor.Name,
            descriptor.Namespaces,
            descriptor.IsNullable,
            descriptor.NullableAnnotation == Microsoft.CodeAnalysis.NullableAnnotation.Annotated &&
            symbol is { IsValueType: false },
            descriptor.NullableAnnotation == Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated &&
            symbol is { IsValueType: false },
            symbol?.IsValueType == true,
            nullableValueType,
            symbol is Microsoft.CodeAnalysis.INamedTypeSymbol declarationSymbol
                ? TypeDeclarationSnapshot.Create(declarationSymbol)
                : null,
            isVoid,
            symbol is Microsoft.CodeAnalysis.INamedTypeSymbol,
            isArray,
            arrayRank,
            elementType,
            containingType,
            typeArguments,
            originalDefinitionMetadataName,
            originalDefinitionQualifiedMetadataName,
            hasCompleteShape);
    }

    private static string GetQualifiedMetadataName(Microsoft.CodeAnalysis.INamedTypeSymbol symbol)
    {
        var typeNames = new Stack<string>();
        for (var current = symbol; current is not null; current = current.ContainingType)
            typeNames.Push(current.MetadataName);

        var typeName = string.Join("+", typeNames);
        var namespaceName = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();
        return string.IsNullOrEmpty(namespaceName) ? typeName : $"{namespaceName}.{typeName}";
    }

    public bool Equals(TypeSnapshot? other) =>
        other is not null &&
        Name == other.Name &&
        namespaces.SequenceEqual(other.namespaces) &&
        IsNullable == other.IsNullable &&
        IsNullableReference == other.IsNullableReference &&
        IsNonNullableReference == other.IsNonNullableReference &&
        IsValueType == other.IsValueType &&
        IsNullableValueType == other.IsNullableValueType &&
        IsVoid == other.IsVoid &&
        IsNamedType == other.IsNamedType &&
        IsArray == other.IsArray &&
        ArrayRank == other.ArrayRank &&
        Equals(ElementType, other.ElementType) &&
        Equals(ContainingType, other.ContainingType) &&
        TypeArguments.Equals(other.TypeArguments) &&
        OriginalDefinitionMetadataName == other.OriginalDefinitionMetadataName &&
        OriginalDefinitionQualifiedMetadataName == other.OriginalDefinitionQualifiedMetadataName &&
        HasCompleteShape == other.HasCompleteShape &&
        Equals(Declaration, other.Declaration);

    public override bool Equals(object? obj) => obj is TypeSnapshot other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Name.GetHashCode();
            foreach (var item in namespaces)
                hash = (hash * 397) ^ item.GetHashCode();
            hash = (hash * 397) ^ IsNullable.GetHashCode();
            hash = (hash * 397) ^ IsNullableReference.GetHashCode();
            hash = (hash * 397) ^ IsNonNullableReference.GetHashCode();
            hash = (hash * 397) ^ IsValueType.GetHashCode();
            hash = (hash * 397) ^ IsNullableValueType.GetHashCode();
            hash = (hash * 397) ^ IsVoid.GetHashCode();
            hash = (hash * 397) ^ IsNamedType.GetHashCode();
            hash = (hash * 397) ^ IsArray.GetHashCode();
            hash = (hash * 397) ^ ArrayRank;
            hash = (hash * 397) ^ (ElementType?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ (ContainingType?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ TypeArguments.GetHashCode();
            hash = (hash * 397) ^ (OriginalDefinitionMetadataName?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ (OriginalDefinitionQualifiedMetadataName?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ HasCompleteShape.GetHashCode();
            return (hash * 397) ^ (Declaration?.GetHashCode() ?? 0);
        }
    }
}

/// <summary>An immutable description of a named type declaration and its containing declarations.</summary>
public sealed class TypeDeclarationSnapshot : IEquatable<TypeDeclarationSnapshot>
{
    private readonly ReadOnlyCollection<ContainingTypeSnapshot> containingTypes;

    private TypeDeclarationSnapshot(
        string name,
        string metadataName,
        string namespaceName,
        string qualifiedName,
        string identityIdentifier,
        string accessibility,
        string declarationKeyword,
        bool isError,
        IEnumerable<ContainingTypeSnapshot> containingTypes)
    {
        Name = name;
        MetadataName = metadataName;
        NamespaceName = namespaceName;
        QualifiedName = qualifiedName;
        IdentityIdentifier = identityIdentifier;
        Accessibility = accessibility;
        DeclarationKeyword = declarationKeyword;
        IsError = isError;
        this.containingTypes = Array.AsReadOnly(containingTypes.ToArray());
    }

    /// <summary>The declared source name.</summary>
    public string Name { get; }
    /// <summary>The metadata name, including generic arity.</summary>
    public string MetadataName { get; }
    /// <summary>The containing namespace.</summary>
    public string NamespaceName { get; }
    /// <summary>The source-qualified name within its containing types.</summary>
    public string QualifiedName { get; }
    /// <summary>A stable identifier suitable for generated member names.</summary>
    public string IdentityIdentifier { get; }
    /// <summary>The C# accessibility keyword.</summary>
    public string Accessibility { get; }
    /// <summary>The C# declaration keyword.</summary>
    public string DeclarationKeyword { get; }
    /// <summary>Whether Roslyn resolved the declaration as an error type.</summary>
    public bool IsError { get; }
    /// <summary>The containing type declarations, from outermost to innermost.</summary>
    public IReadOnlyList<ContainingTypeSnapshot> ContainingTypes => containingTypes;

    internal static TypeDeclarationSnapshot Create(Microsoft.CodeAnalysis.INamedTypeSymbol symbol)
    {
        var symbols = new Stack<Microsoft.CodeAnalysis.INamedTypeSymbol>();
        for (var current = symbol.ContainingType; current is not null; current = current.ContainingType)
            symbols.Push(current);
        var containers = symbols.Select(ContainingTypeSnapshot.Create).ToArray();
        var names = containers.Select(item => item.Name).Concat([symbol.Name]).ToArray();
        return new TypeDeclarationSnapshot(
            symbol.Name,
            symbol.MetadataName,
            symbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : symbol.ContainingNamespace.ToDisplayString(),
            string.Join(".", names),
            string.Join("_", names),
            AccessibilityName(symbol.DeclaredAccessibility),
            GetDeclarationKeyword(symbol),
            symbol.TypeKind == Microsoft.CodeAnalysis.TypeKind.Error,
            containers);
    }

    internal static string AccessibilityName(Microsoft.CodeAnalysis.Accessibility accessibility) => accessibility switch
    {
        Microsoft.CodeAnalysis.Accessibility.Internal => "internal",
        Microsoft.CodeAnalysis.Accessibility.Private => "private",
        Microsoft.CodeAnalysis.Accessibility.Protected => "protected",
        Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => "protected internal",
        Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal => "private protected",
        _ => "public",
    };

    internal static string GetDeclarationKeyword(Microsoft.CodeAnalysis.INamedTypeSymbol symbol) => symbol switch
    {
        { IsRecord: true, TypeKind: Microsoft.CodeAnalysis.TypeKind.Struct } => "record struct",
        { IsRecord: true } => "record",
        { TypeKind: Microsoft.CodeAnalysis.TypeKind.Struct } => "struct",
        _ => "class",
    };

    public bool Equals(TypeDeclarationSnapshot? other) => other is not null &&
        Name == other.Name && MetadataName == other.MetadataName && NamespaceName == other.NamespaceName &&
        QualifiedName == other.QualifiedName && IdentityIdentifier == other.IdentityIdentifier &&
        Accessibility == other.Accessibility && DeclarationKeyword == other.DeclarationKeyword &&
        IsError == other.IsError && containingTypes.SequenceEqual(other.containingTypes);

    public override bool Equals(object? obj) => obj is TypeDeclarationSnapshot other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Name.GetHashCode();
            hash = (hash * 397) ^ MetadataName.GetHashCode();
            hash = (hash * 397) ^ NamespaceName.GetHashCode();
            hash = (hash * 397) ^ Accessibility.GetHashCode();
            foreach (var item in containingTypes) hash = (hash * 397) ^ item.GetHashCode();
            return hash;
        }
    }
}

/// <summary>An immutable description of a containing type declaration.</summary>
public sealed class ContainingTypeSnapshot : IEquatable<ContainingTypeSnapshot>
{
    private ContainingTypeSnapshot(string name, string accessibility, string declarationKeyword)
    {
        Name = name;
        Accessibility = accessibility;
        DeclarationKeyword = declarationKeyword;
    }

    /// <summary>The declared type name.</summary>
    public string Name { get; }
    /// <summary>The C# accessibility keyword.</summary>
    public string Accessibility { get; }
    /// <summary>The C# declaration keyword.</summary>
    public string DeclarationKeyword { get; }

    internal static ContainingTypeSnapshot Create(Microsoft.CodeAnalysis.INamedTypeSymbol symbol) =>
        new(symbol.Name, TypeDeclarationSnapshot.AccessibilityName(symbol.DeclaredAccessibility),
            TypeDeclarationSnapshot.GetDeclarationKeyword(symbol));

    public bool Equals(ContainingTypeSnapshot? other) => other is not null &&
        Name == other.Name && Accessibility == other.Accessibility && DeclarationKeyword == other.DeclarationKeyword;

    public override bool Equals(object? obj) => obj is ContainingTypeSnapshot other && Equals(other);

    public override int GetHashCode() => ((Name.GetHashCode() * 397) ^ Accessibility.GetHashCode()) * 397 ^ DeclarationKeyword.GetHashCode();
}

/// <summary>An immutable, symbol-free property description.</summary>
public sealed class PropertySnapshot : IEquatable<PropertySnapshot>
{
    /// <summary>Creates a property snapshot.</summary>
    /// <param name="type">The property type.</param>
    /// <param name="name">The non-empty property name.</param>
    public PropertySnapshot(TypeSnapshot type, string name)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Property name cannot be null, empty, or whitespace.", nameof(name));
    }

    /// <summary>The property type.</summary>
    public TypeSnapshot Type { get; }

    /// <summary>The property name.</summary>
    public string Name { get; }

    /// <summary>Creates a property snapshot from a descriptor.</summary>
    /// <param name="descriptor">The property descriptor to snapshot.</param>
    /// <returns>An immutable, symbol-free property snapshot.</returns>
    public static PropertySnapshot Create(PropertyDescriptor descriptor) =>
        descriptor is null
            ? throw new ArgumentNullException(nameof(descriptor))
            : new(TypeSnapshot.Create(descriptor.Type), descriptor.Name);

    public bool Equals(PropertySnapshot? other) =>
        other is not null && Name == other.Name && Type.Equals(other.Type);

    public override bool Equals(object? obj) => obj is PropertySnapshot other && Equals(other);

    public override int GetHashCode() => (Type.GetHashCode() * 397) ^ Name.GetHashCode();
}

/// <summary>An immutable, non-empty path of properties.</summary>
public sealed class PropertyPathSnapshot : IEquatable<PropertyPathSnapshot>
{
    private readonly ReadOnlyCollection<PropertySnapshot> properties;

    /// <summary>Creates a property path.</summary>
    /// <param name="properties">The non-null properties, ordered from root to leaf.</param>
    public PropertyPathSnapshot(IEnumerable<PropertySnapshot> properties)
    {
        if (properties is null)
            throw new ArgumentNullException(nameof(properties));

        var values = properties.ToArray();
        if (values.Any(property => property is null))
            throw new ArgumentException("A property path cannot contain null items.", nameof(properties));

        this.properties = Array.AsReadOnly(values);
        if (this.properties.Count == 0)
            throw new ArgumentException("A property path cannot be empty.", nameof(properties));
    }

    /// <summary>The properties ordered from root to leaf.</summary>
    public IReadOnlyList<PropertySnapshot> Properties => properties;

    /// <summary>The leaf property.</summary>
    public PropertySnapshot PropertyType => properties[properties.Count - 1];

    /// <summary>The dot-separated property path.</summary>
    public string Path => string.Join(".", properties.Select(item => item.Name));

    public bool Equals(PropertyPathSnapshot? other) =>
        other is not null && properties.SequenceEqual(other.properties);

    public override bool Equals(object? obj) => obj is PropertyPathSnapshot other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (var item in properties)
                hash = (hash * 31) + item.GetHashCode();
            return hash;
        }
    }
}

/// <summary>An immutable description of an assignment selected during property matching.</summary>
public sealed class AssignmentSnapshot : IEquatable<AssignmentSnapshot>
{
    /// <summary>Creates an assignment snapshot.</summary>
    /// <param name="assignType">The assignment strategy.</param>
    /// <param name="materialization">The collection materialization strategy.</param>
    /// <param name="innerSelection">The nested property selection, when required.</param>
    /// <param name="elementAssignment">The element assignment for a select operation, when required.</param>
    public AssignmentSnapshot(
        AssignType assignType,
        CollectionMaterialization materialization,
        MatchSelectionSnapshot? innerSelection,
        AssignmentSnapshot? elementAssignment = null)
    {
        AssignType = assignType;
        Materialization = materialization;
        InnerSelection = innerSelection;
        ElementAssignment = elementAssignment;
    }

    /// <summary>The assignment strategy.</summary>
    public AssignType AssignType { get; }

    /// <summary>The collection materialization strategy.</summary>
    public CollectionMaterialization Materialization { get; }

    /// <summary>Whether the generated assignment must materialize a list.</summary>
    public bool RequireToList => Materialization == CollectionMaterialization.List;

    /// <summary>The nested property selection, when required.</summary>
    public MatchSelectionSnapshot? InnerSelection { get; }

    /// <summary>
    /// For <see cref="AssignType.Select"/>, how each element must be assigned. See
    /// <see cref="Assignments.AssignDescriptor.ElementAssignment"/>.
    /// </summary>
    public AssignmentSnapshot? ElementAssignment { get; }

    public bool Equals(AssignmentSnapshot? other) =>
        other is not null &&
        AssignType == other.AssignType &&
        Materialization == other.Materialization &&
        Equals(InnerSelection, other.InnerSelection) &&
        Equals(ElementAssignment, other.ElementAssignment);

    public override bool Equals(object? obj) => obj is AssignmentSnapshot other && Equals(other);

    public override int GetHashCode() =>
        ((((int)AssignType * 397) ^ (int)Materialization) * 397 ^
        (InnerSelection?.GetHashCode() ?? 0)) * 397 ^
        (ElementAssignment?.GetHashCode() ?? 0);
}

/// <summary>An immutable match between an origin property and an optional target path.</summary>
public sealed class PropertyMatchSnapshot : IEquatable<PropertyMatchSnapshot>
{
    /// <summary>Creates a property match snapshot.</summary>
    /// <param name="origin">The non-null origin property.</param>
    /// <param name="target">The matched target path, when found.</param>
    /// <param name="assignment">The assignment strategy, when required.</param>
    public PropertyMatchSnapshot(
        PropertySnapshot origin,
        PropertyPathSnapshot? target,
        AssignmentSnapshot? assignment)
    {
        Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        Target = target;
        Assignment = assignment;
    }

    /// <summary>The origin property.</summary>
    public PropertySnapshot Origin { get; }

    /// <summary>The matched target path, when found.</summary>
    public PropertyPathSnapshot? Target { get; }

    /// <summary>The assignment strategy, when required.</summary>
    public AssignmentSnapshot? Assignment { get; }

    public bool Equals(PropertyMatchSnapshot? other) =>
        other is not null &&
        Origin.Equals(other.Origin) &&
        Equals(Target, other.Target) &&
        Equals(Assignment, other.Assignment);

    public override bool Equals(object? obj) => obj is PropertyMatchSnapshot other && Equals(other);

    public override int GetHashCode() =>
        ((Origin.GetHashCode() * 397) ^ (Target?.GetHashCode() ?? 0)) * 397 ^
        (Assignment?.GetHashCode() ?? 0);
}

/// <summary>An immutable, symbol-free snapshot of a complete property matching result.</summary>
public sealed class MatchSelectionSnapshot : IEquatable<MatchSelectionSnapshot>
{
    private readonly ReadOnlyCollection<PropertyMatchSnapshot> propertyMatches;

    /// <summary>Creates a matching result snapshot.</summary>
    /// <param name="originType">The origin type.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="propertyMatches">The property matches.</param>
    public MatchSelectionSnapshot(
        TypeSnapshot originType,
        TypeSnapshot targetType,
        IEnumerable<PropertyMatchSnapshot> propertyMatches)
    {
        OriginType = originType ?? throw new ArgumentNullException(nameof(originType));
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        if (propertyMatches is null)
            throw new ArgumentNullException(nameof(propertyMatches));

        var matches = propertyMatches.ToArray();
        if (matches.Any(match => match is null))
            throw new ArgumentException("Property matches cannot contain null items.", nameof(propertyMatches));
        this.propertyMatches = Array.AsReadOnly(matches);
    }

    /// <summary>The origin type.</summary>
    public TypeSnapshot OriginType { get; }

    /// <summary>The target type.</summary>
    public TypeSnapshot TargetType { get; }

    /// <summary>The frozen property matches.</summary>
    public IReadOnlyList<PropertyMatchSnapshot> PropertyMatches => propertyMatches;

    public bool Equals(MatchSelectionSnapshot? other) =>
        other is not null &&
        OriginType.Equals(other.OriginType) &&
        TargetType.Equals(other.TargetType) &&
        propertyMatches.SequenceEqual(other.propertyMatches);

    public override bool Equals(object? obj) => obj is MatchSelectionSnapshot other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = (OriginType.GetHashCode() * 397) ^ TargetType.GetHashCode();
            foreach (var item in propertyMatches)
                hash = (hash * 397) ^ item.GetHashCode();
            return hash;
        }
    }
}
