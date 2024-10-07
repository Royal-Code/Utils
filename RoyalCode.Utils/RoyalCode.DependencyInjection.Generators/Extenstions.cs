using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.CompilerServices;
using System.Text;

namespace RoyalCode.DependencyInjection.Generators;

internal static class Extenstions
{
    public static IEnumerable<string> GetNamespaces(this TypeSyntax typeSyntax, SemanticModel semanticModel)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetNamespace(this MemberDeclarationSyntax node)
    {
        var parent = node.Parent;
        while (parent != null)
        {
            if (parent is BaseNamespaceDeclarationSyntax namespaceDeclaration)
                return namespaceDeclaration.Name.ToString();

            parent = parent.Parent;
        }

        return string.Empty;
    }

    public static StringBuilder GenericCommas(this StringBuilder sb, int genericParameters)
    {
        sb.Append('<');
        
        var qtd = genericParameters - 1;
        for (int i = 0; i < qtd; i++)
        {
            sb.Append(',');
        }

        sb.Append('>');

        return sb;
    }

    public static StringBuilder Lifetime(this StringBuilder sb, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                sb.Append(".AddTransient");
                break;
            case ServiceLifetime.Scoped:
                sb.Append(".AddScoped");
                break;
            case ServiceLifetime.Singleton:
                sb.Append(".AddSingleton");
                break;
        }

        return sb;
    }
}