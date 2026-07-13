using System.Collections.ObjectModel;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Snapshots;

/// <summary>
/// Creates immutable, symbol-free snapshots of property matching results for safe
/// retention by incremental generator pipelines.
/// </summary>
public static class MatchSelectionSnapshotFactory
{
    public static MatchSelectionSnapshot Create(MatchSelection selection) =>
        Create(selection, Array.Empty<PropertySnapshot>());

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
            {
                var innerParent = descriptor.AssignType == AssignType.NewInstance && target is not null
                    ? target.Properties
                    : Array.Empty<PropertySnapshot>();
                assignment = new AssignmentSnapshot(
                    descriptor.AssignType,
                    descriptor.RequireToList,
                    descriptor.InnerSelection is null
                        ? null
                        : Create(descriptor.InnerSelection, innerParent));
            }

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
}

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
        TypeDeclarationSnapshot? declaration)
    {
        Name = name;
        this.namespaces = Array.AsReadOnly(namespaces.ToArray());
        IsNullable = isNullable;
        IsNullableReference = isNullableReference;
        IsNonNullableReference = isNonNullableReference;
        IsValueType = isValueType;
        IsNullableValueType = isNullableValueType;
        Declaration = declaration;
    }

    public string Name { get; }

    public IReadOnlyList<string> Namespaces => namespaces;

    public bool IsNullable { get; }

    public bool IsNullableReference { get; }

    public bool IsNonNullableReference { get; }

    public bool IsValueType { get; }

    public bool IsNullableValueType { get; }

    public TypeDeclarationSnapshot? Declaration { get; }

    public bool MayBeNull => IsNullable || IsNullableReference || Name.EndsWith("?", StringComparison.Ordinal);

    public string UnderlyingType => Name.EndsWith("?", StringComparison.Ordinal)
        ? Name.Substring(0, Name.Length - 1)
        : Name;

    public static TypeSnapshot Create(TypeDescriptor descriptor)
    {
        var nullableValueType = descriptor.IsNullable &&
                                descriptor.Symbol is Microsoft.CodeAnalysis.INamedTypeSymbol namedType &&
                                namedType.TypeArguments.Length == 1 &&
                                namedType.TypeArguments[0].IsValueType;
        return new TypeSnapshot(
            descriptor.Name,
            descriptor.Namespaces,
            descriptor.IsNullable,
            descriptor.NullableAnnotation == Microsoft.CodeAnalysis.NullableAnnotation.Annotated &&
            descriptor.Symbol is { IsValueType: false },
            descriptor.NullableAnnotation == Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated &&
            descriptor.Symbol is { IsValueType: false },
            descriptor.Symbol?.IsValueType == true,
            nullableValueType,
            descriptor.Symbol is Microsoft.CodeAnalysis.INamedTypeSymbol declarationSymbol
                ? TypeDeclarationSnapshot.Create(declarationSymbol)
                : null);
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
            return (hash * 397) ^ (Declaration?.GetHashCode() ?? 0);
        }
    }
}

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

    public string Name { get; }
    public string MetadataName { get; }
    public string NamespaceName { get; }
    public string QualifiedName { get; }
    public string IdentityIdentifier { get; }
    public string Accessibility { get; }
    public string DeclarationKeyword { get; }
    public bool IsError { get; }
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

public sealed class ContainingTypeSnapshot : IEquatable<ContainingTypeSnapshot>
{
    private ContainingTypeSnapshot(string name, string accessibility, string declarationKeyword)
    {
        Name = name;
        Accessibility = accessibility;
        DeclarationKeyword = declarationKeyword;
    }

    public string Name { get; }
    public string Accessibility { get; }
    public string DeclarationKeyword { get; }

    internal static ContainingTypeSnapshot Create(Microsoft.CodeAnalysis.INamedTypeSymbol symbol) =>
        new(symbol.Name, TypeDeclarationSnapshot.AccessibilityName(symbol.DeclaredAccessibility),
            TypeDeclarationSnapshot.GetDeclarationKeyword(symbol));

    public bool Equals(ContainingTypeSnapshot? other) => other is not null &&
        Name == other.Name && Accessibility == other.Accessibility && DeclarationKeyword == other.DeclarationKeyword;

    public override bool Equals(object? obj) => obj is ContainingTypeSnapshot other && Equals(other);

    public override int GetHashCode() => ((Name.GetHashCode() * 397) ^ Accessibility.GetHashCode()) * 397 ^ DeclarationKeyword.GetHashCode();
}

public sealed class PropertySnapshot : IEquatable<PropertySnapshot>
{
    public PropertySnapshot(TypeSnapshot type, string name)
    {
        Type = type;
        Name = name;
    }

    public TypeSnapshot Type { get; }

    public string Name { get; }

    public static PropertySnapshot Create(PropertyDescriptor descriptor) =>
        new(TypeSnapshot.Create(descriptor.Type), descriptor.Name);

    public bool Equals(PropertySnapshot? other) =>
        other is not null && Name == other.Name && Type.Equals(other.Type);

    public override bool Equals(object? obj) => obj is PropertySnapshot other && Equals(other);

    public override int GetHashCode() => (Type.GetHashCode() * 397) ^ Name.GetHashCode();
}

public sealed class PropertyPathSnapshot : IEquatable<PropertyPathSnapshot>
{
    private readonly ReadOnlyCollection<PropertySnapshot> properties;

    public PropertyPathSnapshot(IEnumerable<PropertySnapshot> properties)
    {
        this.properties = Array.AsReadOnly(properties.ToArray());
        if (this.properties.Count == 0)
            throw new ArgumentException("A property path cannot be empty.", nameof(properties));
    }

    public IReadOnlyList<PropertySnapshot> Properties => properties;

    public PropertySnapshot PropertyType => properties[properties.Count - 1];

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

public sealed class AssignmentSnapshot : IEquatable<AssignmentSnapshot>
{
    public AssignmentSnapshot(
        AssignType assignType,
        bool requireToList,
        MatchSelectionSnapshot? innerSelection)
    {
        AssignType = assignType;
        RequireToList = requireToList;
        InnerSelection = innerSelection;
    }

    public AssignType AssignType { get; }

    public bool RequireToList { get; }

    public MatchSelectionSnapshot? InnerSelection { get; }

    public bool Equals(AssignmentSnapshot? other) =>
        other is not null &&
        AssignType == other.AssignType &&
        RequireToList == other.RequireToList &&
        Equals(InnerSelection, other.InnerSelection);

    public override bool Equals(object? obj) => obj is AssignmentSnapshot other && Equals(other);

    public override int GetHashCode() =>
        (((int)AssignType * 397) ^ RequireToList.GetHashCode()) * 397 ^
        (InnerSelection?.GetHashCode() ?? 0);
}

public sealed class PropertyMatchSnapshot : IEquatable<PropertyMatchSnapshot>
{
    public PropertyMatchSnapshot(
        PropertySnapshot origin,
        PropertyPathSnapshot? target,
        AssignmentSnapshot? assignment)
    {
        Origin = origin;
        Target = target;
        Assignment = assignment;
    }

    public PropertySnapshot Origin { get; }

    public PropertyPathSnapshot? Target { get; }

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

public sealed class MatchSelectionSnapshot : IEquatable<MatchSelectionSnapshot>
{
    private readonly ReadOnlyCollection<PropertyMatchSnapshot> propertyMatches;

    public MatchSelectionSnapshot(
        TypeSnapshot originType,
        TypeSnapshot targetType,
        IEnumerable<PropertyMatchSnapshot> propertyMatches)
    {
        OriginType = originType;
        TargetType = targetType;
        this.propertyMatches = Array.AsReadOnly(propertyMatches.ToArray());
    }

    public TypeSnapshot OriginType { get; }

    public TypeSnapshot TargetType { get; }

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
