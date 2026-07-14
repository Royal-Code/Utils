using Microsoft.CodeAnalysis;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

/// <summary>
///     Regressão de <see cref="EnumerableAssignDescriptorResolver"/>: quando a coleção de destino
///     (lado origem/DTO) é declarada com o tipo concreto <c>List&lt;T&gt;</c> (em vez de uma interface como
///     <c>IReadOnlyList&lt;T&gt;</c>/<c>ICollection&lt;T&gt;</c>), ela deve ser projetada com
///     <c>Select(...).ToList()</c>, e não tratada como um objeto mapeável (casando <c>Capacity</c> e o
///     indexador <c>this[]</c> da própria lista).
/// </summary>
public class EnumerableListDestinationBugTests
{
    private const string Source =
        """
        using System.Collections.Generic;

        namespace Tests.ListDestination
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

            // DTO: a coleção aninhada é declarada com o tipo concreto List<T>.
            public class OrderDetails
            {
                public List<OrderItemDetails> Items { get; set; }
            }
        }
        """;

    private static AssignDescriptor ItemsAssignment()
    {
        Util.Compile(Source, out var compilation, out var diagnostics);
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
        Assert.NotNull(items.AssignDescriptor);
        return items.AssignDescriptor!;
    }

    /// <summary>
    /// Um destino <c>List&lt;T&gt;</c> aninhado deve ser projetado com <c>Select(...).ToList()</c> —
    /// <see cref="AssignType.Select"/> com <c>RequireToList = true</c> e uma seleção interna sobre os
    /// elementos (<c>ProductName</c>/<c>Quantity</c>).
    /// </summary>
    [Fact]
    public void ListDestination_should_project_elements_with_Select_ToList()
    {
        var assignment = ItemsAssignment();

        Assert.Equal(AssignType.Select, assignment.AssignType);
        Assert.True(assignment.RequireToList, "Um destino List<T> deve exigir .ToList() após o Select.");
        Assert.NotNull(assignment.InnerSelection);

        var innerOriginNames = assignment.InnerSelection!.PropertyMatches
            .Select(m => m.Origin.Name)
            .ToArray();

        Assert.Contains("ProductName", innerOriginNames);
        Assert.Contains("Quantity", innerOriginNames);
        Assert.DoesNotContain("Capacity", innerOriginNames);
    }
}
