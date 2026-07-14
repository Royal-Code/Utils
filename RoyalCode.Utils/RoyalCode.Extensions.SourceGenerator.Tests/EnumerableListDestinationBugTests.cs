using Microsoft.CodeAnalysis;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

/// <summary>
/// <para>
///     Evidencia um bug em <see cref="EnumerableAssignDescriptorResolver"/>: quando a coleção de destino
///     (lado origem/DTO) é declarada com o tipo concreto <c>List&lt;T&gt;</c> (em vez de uma interface como
///     <c>IReadOnlyList&lt;T&gt;</c>/<c>ICollection&lt;T&gt;</c>), o resolver de enumeráveis desiste e a factory
///     cai no <see cref="InnerTypeAssignDescriptorResolver"/>, que trata o próprio <c>List&lt;T&gt;</c> como um
///     objeto mapeável — casando os membros da lista (<c>Capacity</c> e o indexador <c>this[]</c>) em vez de
///     projetar os elementos via <c>Select(...).ToList()</c>.
/// </para>
/// <para>
///     Causa raiz em <c>EnumerableAssignDescriptorResolver</c>: a checagem de conversão usa o
///     <em>generic definition aberto</em> <c>List&lt;T&gt;</c> em
///     <c>ClassifyConversion(listType, leftType.Symbol)</c>, que retorna <c>NoConversion</c> para um destino
///     <c>List&lt;algo&gt;</c> construído (deveria construir <c>List&lt;leftGeneric&gt;</c> antes de classificar
///     e exigir conversão implícita). As interfaces passam por acidente, pois o definition aberto tem conversão
///     <c>ExplicitReference</c> (Exists=true) para elas e o código só testa <c>.Exists</c>.
/// </para>
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
    /// Caracterização (canário verde): documenta o comportamento bugado atual. Enquanto o bug existir,
    /// o destino <c>List&lt;T&gt;</c> vira <see cref="AssignType.NewInstance"/> mapeando os membros da própria
    /// lista (<c>Capacity</c> e o indexador <c>this[]</c>). Quando o bug for corrigido, este teste passará a
    /// falhar — sinal de que o teste com <c>Skip</c> abaixo deve ser reativado e este, removido.
    /// </summary>
    [Fact]
    public void ListDestination_currently_maps_List_own_members_instead_of_projecting_elements()
    {
        var assignment = ItemsAssignment();

        // Comportamento bugado: trata List<T> como objeto (NewInstance) em vez de Select.
        Assert.Equal(AssignType.NewInstance, assignment.AssignType);
        Assert.NotNull(assignment.InnerSelection);

        var innerOriginNames = assignment.InnerSelection!.PropertyMatches
            .Select(m => m.Origin.Name)
            .ToArray();

        // Prova de que está casando os membros do próprio List<T>, não os elementos do DTO.
        Assert.Contains("Capacity", innerOriginNames);
        Assert.DoesNotContain("ProductName", innerOriginNames);
        Assert.DoesNotContain("Quantity", innerOriginNames);
    }

    /// <summary>
    /// Comportamento correto esperado: um destino <c>List&lt;T&gt;</c> aninhado deve ser projetado com
    /// <c>Select(...).ToList()</c> — <see cref="AssignType.Select"/> com <c>RequireToList = true</c> e uma
    /// seleção interna sobre os elementos (<c>ProductName</c>/<c>Quantity</c>). Reative (remova o <c>Skip</c>)
    /// quando <see cref="EnumerableAssignDescriptorResolver"/> for corrigido para construir
    /// <c>List&lt;leftGeneric&gt;</c> antes de classificar a conversão (e exigir conversão implícita).
    /// </summary>
    [Fact(Skip =
        "Bug conhecido: destino de coleção aninhada declarado como List<T> vira NewInstance mapeando "
        + "Capacity/this[] em vez de Select(...).ToList(). Reative quando EnumerableAssignDescriptorResolver "
        + "construir List<leftGeneric> antes de ClassifyConversion e exigir conversão implícita.")]
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
