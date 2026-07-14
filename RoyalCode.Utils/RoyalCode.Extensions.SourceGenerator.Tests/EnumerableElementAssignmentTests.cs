using Microsoft.CodeAnalysis;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Snapshots;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

/// <summary>
///     Quando os elementos de uma coleção precisam apenas de conversão (e não de um novo objeto), o
///     <see cref="AssignDescriptor"/> precisa dizer <em>como</em> converter cada elemento. Antes, o resolver
///     copiava só a <c>InnerSelection</c> do elemento — que é nula num cast — e produzia um
///     <see cref="AssignType.Select"/> sem nada dentro, deixando o gerador sem como emitir o corpo do lambda.
///     O <see cref="AssignDescriptor.ElementAssignment"/> carrega o assignment completo do elemento.
/// </summary>
public class EnumerableElementAssignmentTests
{
    private static AssignDescriptor ItemsAssignment(string entityCollection, string dtoCollection)
    {
        var source =
            $$"""
            using System.Collections.Generic;

            namespace Tests.ElementAssignment
            {
                public enum Status { Draft, Done }

                public enum StatusDto { Draft, Done }

                public class Item
                {
                    public string Name { get; set; }
                }

                public class ItemDetails
                {
                    public string Name { get; set; }
                }

                public class Order
                {
                    public {{entityCollection}} Items { get; set; }
                }

                public class OrderDetails
                {
                    public {{dtoCollection}} Items { get; set; }
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

        var match = MatchSelection.Create(
            TypeDescriptor.Create(dto),
            TypeDescriptor.Create(entity),
            model);

        var items = match.PropertyMatches.First(p => p.Origin.Name == "Items");
        Assert.NotNull(items.AssignDescriptor);
        return items.AssignDescriptor!;
    }

    /// <summary>
    /// Elementos que só precisam de conversão: o assignment do elemento é o cast, sem seleção interna.
    /// Uma coleção de enums equivalentes (<c>List&lt;Status&gt;</c> na entidade, <c>List&lt;StatusDto&gt;</c>
    /// no DTO) resolve o elemento como <see cref="AssignType.SimpleCast"/> — o gerador precisa do assignment
    /// do elemento para emitir <c>Items.Select(i => (StatusDto)i).ToList()</c>.
    /// </summary>
    [Fact]
    public void Converted_elements_should_carry_the_element_assignment()
    {
        var assignment = ItemsAssignment("List<Status>", "List<StatusDto>");

        Assert.Equal(AssignType.Select, assignment.AssignType);
        Assert.Equal(CollectionMaterialization.List, assignment.Materialization);
        Assert.Null(assignment.InnerSelection);

        Assert.NotNull(assignment.ElementAssignment);
        Assert.Equal(AssignType.SimpleCast, assignment.ElementAssignment!.AssignType);
        Assert.Null(assignment.ElementAssignment.InnerSelection);
    }

    /// <summary>
    /// Elementos que são objetos mapeados: o assignment do elemento é um NewInstance com a seleção interna,
    /// que continua exposta em <see cref="AssignDescriptor.InnerSelection"/> como antes.
    /// </summary>
    [Fact]
    public void Mapped_elements_should_carry_a_NewInstance_element_assignment()
    {
        var assignment = ItemsAssignment("List<Item>", "List<ItemDetails>");

        Assert.Equal(AssignType.Select, assignment.AssignType);
        Assert.NotNull(assignment.InnerSelection);

        Assert.NotNull(assignment.ElementAssignment);
        Assert.Equal(AssignType.NewInstance, assignment.ElementAssignment!.AssignType);
        Assert.Same(assignment.InnerSelection, assignment.ElementAssignment.InnerSelection);
    }

    /// <summary>
    /// O snapshot (o que o pipeline incremental realmente enxerga) precisa carregar o mesmo dado,
    /// senão o gerador continua sem saber converter os elementos.
    /// </summary>
    [Fact]
    public void Snapshot_should_carry_the_element_assignment()
    {
        var source =
            """
            using System.Collections.Generic;

            namespace Tests.ElementAssignment.Snapshot
            {
                public enum Status { Draft, Done }

                public enum StatusDto { Draft, Done }

                public class Order
                {
                    public List<Status> Items { get; set; }
                }

                public class OrderDetails
                {
                    public List<StatusDto> Items { get; set; }
                }
            }
            """;

        Util.Compile(source, out var compilation, out _);

        var dto = compilation.GetSymbolsWithName("OrderDetails", SymbolFilter.Type)
            .OfType<INamedTypeSymbol>().First();
        var entity = compilation.GetSymbolsWithName("Order", SymbolFilter.Type)
            .OfType<INamedTypeSymbol>().First();
        var model = compilation.GetSemanticModel(compilation.SyntaxTrees.First());

        var selection = MatchSelection.Create(
            TypeDescriptor.Create(dto),
            TypeDescriptor.Create(entity),
            model);

        var snapshot = MatchSelectionSnapshotFactory.Create(selection);
        var assignment = snapshot.PropertyMatches.First(m => m.Origin.Name == "Items").Assignment;

        Assert.NotNull(assignment);
        Assert.Equal(AssignType.Select, assignment!.AssignType);
        Assert.Equal(CollectionMaterialization.List, assignment.Materialization);
        Assert.NotNull(assignment.ElementAssignment);
        Assert.Equal(AssignType.SimpleCast, assignment.ElementAssignment!.AssignType);
    }
}
