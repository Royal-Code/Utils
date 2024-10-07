using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoyalCode.DependencyInjection.Generators;

public sealed class TypeDescriptor
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


}