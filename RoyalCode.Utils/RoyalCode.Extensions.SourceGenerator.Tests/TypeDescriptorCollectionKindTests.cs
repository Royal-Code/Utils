using Microsoft.CodeAnalysis;
using RoyalCode.Extensions.SourceGenerator.Descriptors;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

/// <summary>
///     Cobre <see cref="TypeDescriptor.IsArray"/>, <see cref="TypeDescriptor.IsEnumerable"/> e
///     <see cref="TypeDescriptor.IsCollection"/>, que agora respondem pelo símbolo (e não pelo texto do nome).
///     <c>IsEnumerable</c> é o <c>IEnumerable&lt;T&gt;</c> em si; <c>IsCollection</c> é qualquer coisa que o
///     implemente, exceto <c>string</c>.
/// </summary>
public class TypeDescriptorCollectionKindTests
{
    private const string Source =
        """
        using System.Collections;
        using System.Collections.Generic;

        namespace Tests.CollectionKind
        {
            public class Item { }

            public class Holder
            {
                public IEnumerable<Item> Enumerable { get; set; }
                public IEnumerable NonGenericEnumerable { get; set; }
                public List<Item> List { get; set; }
                public Item[] Array { get; set; }
                public HashSet<Item> Set { get; set; }
                public string Text { get; set; }
                public int Number { get; set; }
                public Item Single { get; set; }
            }
        }
        """;

    private static TypeDescriptor PropertyType(string propertyName)
    {
        Util.Compile(Source, out var compilation, out var diagnostics);
        Assert.True(
            diagnostics.IsDefaultOrEmpty,
            "A compilação do código de teste gerou diagnostics inesperados: "
                + string.Join("\n", diagnostics.Select(d => d.ToString())));

        var holder = compilation.GetSymbolsWithName("Holder", SymbolFilter.Type)
            .OfType<INamedTypeSymbol>().First();

        var property = holder.GetMembers(propertyName).OfType<IPropertySymbol>().Single();
        return TypeDescriptor.Create(property.Type);
    }

    [Theory]
    [InlineData("Enumerable", false, true, true)]
    [InlineData("NonGenericEnumerable", false, true, false)] // IEnumerable não genérico não tem elemento tipado
    [InlineData("List", false, false, true)]
    [InlineData("Array", true, false, true)]
    [InlineData("Set", false, false, true)]
    [InlineData("Text", false, false, false)] // string implementa IEnumerable<char>, mas não é coleção aqui
    [InlineData("Number", false, false, false)]
    [InlineData("Single", false, false, false)]
    public void Collection_kinds_are_resolved_from_the_symbol(
        string propertyName, bool isArray, bool isEnumerable, bool isCollection)
    {
        var type = PropertyType(propertyName);

        Assert.Equal(isArray, type.IsArray);
        Assert.Equal(isEnumerable, type.IsEnumerable);
        Assert.Equal(isCollection, type.IsCollection);
    }
}
