using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoyalCode.DependencyInjection.Generators;

internal static class Extenstions
{
    public static IEnumerable<string> GetNamespaces(this TypeSyntax typeSyntax, SemanticModel semanticModel)
    {
        // namespace do tipo
        var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
        return typeInfo.Type?.GetNamespaces() ?? [];
    }

    public static IEnumerable<string> GetNamespaces(this TypeDeclarationSyntax typeSyntax, SemanticModel semanticModel)
    {
        // namespace do tipo
        var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
        return typeInfo.Type?.GetNamespaces() ?? [];
    }

    public static IEnumerable<string> GetNamespaces(this ITypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace.ToDisplayString();
        yield return ns;

        // se for generic, namespace dos tipos genéricos
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            yield break;

        foreach (var typeArgument in namedTypeSymbol.TypeArguments)
        {
            if (typeArgument is INamedTypeSymbol namedTypeArgument)
            {
                var namespaces = GetNamespaces(namedTypeArgument);
                foreach (var n in namespaces)
                    yield return n;
            }
        }
    }
}