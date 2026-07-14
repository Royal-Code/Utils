using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors;

/// <summary>
/// <para>
///     Describes a property during matching and code generation.
/// </para>
/// <para>
///     It implements <see cref="IEquatable{T}"/> for the matching logic (comparing name and type, never the
///     symbol), but it holds a <see cref="IPropertySymbol"/> and therefore must not be retained by an
///     incremental generator pipeline. Use
///     <see cref="Snapshots.MatchSelectionSnapshotFactory.Create(PropertySelection.MatchSelection)"/> and pass
///     the resulting symbol-free snapshot along instead.
/// </para>
/// </summary>
public sealed class PropertyDescriptor : IEquatable<PropertyDescriptor>
{
    public static PropertyDescriptor Create(PropertyDeclarationSyntax syntax, SemanticModel model)
    {
        var symbol = model.GetDeclaredSymbol(syntax) as IPropertySymbol;

        return new(TypeDescriptor.Create(syntax.Type!, model), syntax.Identifier.Text, symbol);
    }

    public static PropertyDescriptor Create(IPropertySymbol symbol)
        => new(TypeDescriptor.Create(symbol.Type!), symbol.Name, symbol);

    public PropertyDescriptor(TypeDescriptor type, string name, IPropertySymbol? symbol)
    {
        Type = type;
        Name = name;
        Symbol = symbol;
    }

    public TypeDescriptor Type { get; }

    public string Name { get; }

    public IPropertySymbol? Symbol { get; }

    public bool Equals(PropertyDescriptor? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Name == other.Name &&
            Equals(Type, other.Type);
    }

    public override bool Equals(object? obj)
    {
        return obj is PropertyDescriptor other && Equals(other);

    }

    public override int GetHashCode()
    {
        int hashCode = -1979447941;
        hashCode = hashCode * -1521134295 + Type.GetHashCode();
        hashCode = hashCode * -1521134295 + Name.GetHashCode();
        return hashCode;
    }
}
