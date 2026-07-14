using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

internal class EnumerableAssignDescriptorResolver : IAssignDescriptorResolver
{
    public bool TryCreateAssignDescriptor(
        TypeDescriptor leftType,
        TypeDescriptor rightType,
        SemanticModel model,
        MatchOptions options,
        out AssignDescriptor? descriptor)
    {
        if (leftType.Symbol is null || rightType.Symbol is null ||
            leftType.Symbol.TryGetEnumerableGenericType(out var leftGenericSymbol) is false ||
            rightType.Symbol.TryGetEnumerableGenericType(out var rightGenericSymbol) is false)
        {
            descriptor = null;
            return false;
        }

        TypeDescriptor leftGenericType = TypeDescriptor.Create(leftGenericSymbol!);
        TypeDescriptor rightGenericType = TypeDescriptor.Create(rightGenericSymbol!);

        bool requireSelect = false;
        AssignDescriptor? genericAssignment = null;

        // se os tipos genéricos são iguais, não é necessário fazer a conversão
        // cas contrário, é necessário fazer a conversão
        if (!leftGenericType.Equals(rightGenericType))
        {
            requireSelect = true;
            genericAssignment = AssignDescriptorFactory.Create(leftGenericType, rightGenericType, model, options);

            // se a conversão dos tipos genéricos não for possível, não é possível fazer a conversão desta propriedade
            if (genericAssignment is null)
            {
                descriptor = null;
                return false;
            }
        }

        // determina como o enumerável deve ser materializado para caber no tipo de destino
        if (!TryResolveMaterialization(leftType.Symbol, leftGenericSymbol!, model, out var materialization))
        {
            descriptor = null;
            return false;
        }

        descriptor = new AssignDescriptor()
            {
                AssignType = requireSelect ? AssignType.Select : AssignType.Direct,
                Materialization = materialization,
                InnerSelection = genericAssignment?.InnerSelection,
                ElementAssignment = genericAssignment,
            };
        return true;
    }

    /// <summary>
    /// Descobre qual materialização (<c>ToList</c>, <c>ToArray</c>, <c>ToHashSet</c> ou nenhuma) produz um valor
    /// assinável ao tipo de destino, testando a conversão implícita a partir do tipo concreto construído.
    /// </summary>
    private static bool TryResolveMaterialization(
        ITypeSymbol leftSymbol,
        ITypeSymbol leftGenericSymbol,
        SemanticModel model,
        out CollectionMaterialization materialization)
    {
        // o destino é o próprio IEnumerable<T>: aceita o enumerável como está.
        if (leftSymbol.IsEnumerableInterface())
        {
            materialization = CollectionMaterialization.None;
            return true;
        }

        var compilation = model.Compilation;

        // array de destino (T[]): materializa com ToArray.
        if (leftSymbol is IArrayTypeSymbol)
        {
            var arrayType = compilation.CreateArrayTypeSymbol(leftGenericSymbol);
            if (compilation.ClassifyConversion(arrayType, leftSymbol).IsImplicit)
            {
                materialization = CollectionMaterialization.Array;
                return true;
            }

            materialization = default;
            return false;
        }

        // demais destinos: o primeiro tipo concreto que converte implicitamente vence.
        // List<T> cobre List<T>, IList<T>, ICollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T>;
        // HashSet<T> cobre HashSet<T> e ISet<T>.
        if (IsAssignableFromConstructed(compilation, "System.Collections.Generic.List`1", leftGenericSymbol, leftSymbol))
        {
            materialization = CollectionMaterialization.List;
            return true;
        }

        if (IsAssignableFromConstructed(compilation, "System.Collections.Generic.HashSet`1", leftGenericSymbol, leftSymbol))
        {
            materialization = CollectionMaterialization.HashSet;
            return true;
        }

        materialization = default;
        return false;
    }

    private static bool IsAssignableFromConstructed(
        Compilation compilation,
        string metadataName,
        ITypeSymbol genericArgument,
        ITypeSymbol destination)
    {
        var definition = compilation.GetTypeByMetadataName(metadataName);
        if (definition is null)
            return false;

        var constructed = definition.Construct(genericArgument);
        return compilation.ClassifyConversion(constructed, destination).IsImplicit;
    }
}
