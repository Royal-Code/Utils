using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoyalCode.DependencyInjection.Generators;

public sealed class TypeDescriptor : IEquatable<TypeDescriptor>
{
    public static TypeDescriptor Create(TypeSyntax typeSyntax, SemanticModel model)
    {
        var name = typeSyntax.ToString();
        return new(name, typeSyntax.GetNamespaces(model).ToArray());
    }

    public static TypeDescriptor Create(ITypeSymbol classSymbol, SemanticModel model)
    {
        return new(classSymbol.Name, classSymbol.GetNamespaces().ToArray());
    }

    public TypeDescriptor(string name, string[] namespaces)
    {
        Name = name;
        Namespaces = namespaces;
    }

    public string Name { get; }

    public string[] Namespaces { get; }

    public bool Equals(TypeDescriptor other)
    {
        return other is not null &&
            Equals(Name, other.Name) &&
            EqualityComparer<string[]>.Default.Equals(Namespaces, other.Namespaces);
    }

    public override bool Equals(object? obj)
    {
        return obj is TypeDescriptor descriptor && Equals(descriptor);
    }

    public override int GetHashCode()
    {
        int hashCode = -353132481;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Namespaces);
        return hashCode;
    }

    
}