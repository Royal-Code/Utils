using Microsoft.CodeAnalysis;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

/// <summary>
///     Cobre a materialização de coleções aninhadas em <see cref="EnumerableAssignDescriptorResolver"/>:
///     conforme o tipo declarado no destino, a projeção precisa de <c>ToList</c>, <c>ToArray</c>,
///     <c>ToHashSet</c> ou de nada. Destinos que não sabemos materializar devem ficar sem assignment
///     (não-assináveis), e nunca ser mapeados como objeto — o que casaria os membros da própria coleção
///     (<c>Capacity</c>, <c>Length</c>, o indexador) em vez dos elementos.
/// </summary>
public class EnumerableDestinationMaterializationTests
{
    /// <summary>
    /// Monta a compilação com a coleção do DTO declarada como <paramref name="dtoCollectionType"/>
    /// e devolve o assignment da propriedade 'Items' (ou <c>null</c>, se não for assinável).
    /// </summary>
    private static AssignDescriptor? ItemsAssignment(string dtoCollectionType)
    {
        var source =
            $$"""
            using System.Collections.Generic;

            namespace Tests.Materialization
            {
                public class OrderItem
                {
                    public string ProductName { get; set; }
                    public int Quantity { get; set; }
                }

                public class Order
                {
                    public int Id { get; set; }
                    public List<OrderItem> Items { get; set; }
                }

                public class OrderItemDetails
                {
                    public string ProductName { get; set; }
                    public int Quantity { get; set; }
                }

                public class OrderDetails
                {
                    public {{dtoCollectionType}} Items { get; set; }
                }
            }
            """;

        Util.Compile(source, out var compilation, out var diagnostics);
        Assert.True(
            diagnostics.IsDefaultOrEmpty,
            "A compilação do código de teste gerou diagnostics inesperados: "
                + string.Join("\n", diagnostics.Select(d => d.ToString())));

        var dto = compilation.GetSymbolsWithName("OrderDetails", SymbolFilter.Type)
            .OfType<INamedTypeSymbol>().First();
        var entity = compilation.GetSymbolsWithName("Order", SymbolFilter.Type)
            .OfType<INamedTypeSymbol>().First();
        var model = compilation.GetSemanticModel(compilation.SyntaxTrees.First());

        // origem = DTO (OrderDetails), destino/target = entidade (Order), como o SmartSelector faz.
        var match = MatchSelection.Create(
            TypeDescriptor.Create(dto),
            TypeDescriptor.Create(entity),
            model);

        var items = match.PropertyMatches.First(p => p.Origin.Name == "Items");
        Assert.False(items.IsMissing, "A propriedade 'Items' deveria ser encontrada no tipo de destino.");
        return items.AssignDescriptor;
    }

    private static void AssertProjectsElements(AssignDescriptor? assignment, CollectionMaterialization expected)
    {
        Assert.NotNull(assignment);
        Assert.Equal(AssignType.Select, assignment!.AssignType);
        Assert.Equal(expected, assignment.Materialization);
        Assert.NotNull(assignment.InnerSelection);

        var innerOriginNames = assignment.InnerSelection!.PropertyMatches
            .Select(m => m.Origin.Name)
            .ToArray();

        Assert.Contains("ProductName", innerOriginNames);
        Assert.Contains("Quantity", innerOriginNames);
    }

    [Theory]
    [InlineData("List<OrderItemDetails>")]
    [InlineData("IList<OrderItemDetails>")]
    [InlineData("ICollection<OrderItemDetails>")]
    [InlineData("IReadOnlyList<OrderItemDetails>")]
    [InlineData("IReadOnlyCollection<OrderItemDetails>")]
    public void ListLike_destination_should_require_ToList(string dtoCollectionType)
    {
        AssertProjectsElements(ItemsAssignment(dtoCollectionType), CollectionMaterialization.List);
    }

    [Fact]
    public void Array_destination_should_require_ToArray()
    {
        AssertProjectsElements(ItemsAssignment("OrderItemDetails[]"), CollectionMaterialization.Array);
    }

    [Theory]
    [InlineData("HashSet<OrderItemDetails>")]
    [InlineData("ISet<OrderItemDetails>")]
    public void Set_destination_should_require_ToHashSet(string dtoCollectionType)
    {
        AssertProjectsElements(ItemsAssignment(dtoCollectionType), CollectionMaterialization.HashSet);
    }

    [Fact]
    public void Enumerable_destination_should_not_require_materialization()
    {
        AssertProjectsElements(ItemsAssignment("IEnumerable<OrderItemDetails>"), CollectionMaterialization.None);
    }

    /// <summary>
    /// Um destino que não sabemos materializar (aqui, um dicionário, cujo elemento é um
    /// <c>KeyValuePair</c>) deve ficar sem assignment. Antes, o resolver desistia e a factory caía no
    /// <see cref="InnerTypeAssignDescriptorResolver"/>, que mapeava a coleção como objeto.
    /// </summary>
    [Fact]
    public void Unsupported_collection_destination_should_not_be_mapped_as_object()
    {
        var assignment = ItemsAssignment("Dictionary<string, OrderItemDetails>");

        Assert.Null(assignment);
    }
}
