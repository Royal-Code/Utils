using System.Text;
using Microsoft.CodeAnalysis;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Tests.PropertySelectionTests;

public class PropertySelectionTests
{
    /// <summary>
    /// Helper que compila o código e executa a mesma lógica que o algoritmo de matching usa
    /// (PropertySelection.Select para um PropertyDescriptor específico dentro do mesmo tipo).
    /// </summary>
    private static (PropertySelection? selection, PropertyDescriptor originProperty) Select(
        string source,
        string originTypeName,
        string propertyName)
    {
        // Arrange
        Util.Compile(source, out var compilation, out var diagnostics);
        Assert.True(diagnostics.IsDefaultOrEmpty, "Diagnostics inesperados: " + string.Join("\n", diagnostics.Select(d => d.ToString())));

        var originSymbol = compilation
            .GetSymbolsWithName(originTypeName, SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .First();

        var originDescriptor = TypeDescriptor.Create(originSymbol);
        var originProperties = originDescriptor.CreateProperties(null);
        var originProp = originProperties.First(p => p.Name == propertyName);

        // Act
        // Construímos um MatchTypeInfo com o próprio tipo para reaproveitar o algoritmo de partição PascalCase.
        var targetTypeInfo = MatchTypeInfo.Create(originDescriptor, MatchOptions.Default);
        var selection = PropertySelection.Select(originProp, targetTypeInfo);

        // Return para asserções no teste chamador.
        return (selection, originProp);
    }

    [Fact]
    public void Select_SimpleProperty()
    {
        // Arrange + Act
        var (selection, _) = Select(Code.Source, "Alpha", "Betha");

        // Assert
        Assert.NotNull(selection);
        var sb = new StringBuilder();
        selection!.WritePropertyPath(sb);
        Assert.Equal("Betha", sb.ToString());
    }

    [Fact]
    public void Select_PascalCaseTwoLevels()
    {
        // Arrange
        // Propriedade sintetizada BethaGammaDeltaName deve resultar no caminho Betha.Gamma.Delta.Name
        Util.Compile(Code.Source, out var compilation, out var diagnostics);
        Assert.True(diagnostics.IsDefaultOrEmpty, "Diagnostics inesperados: " + string.Join("\n", diagnostics.Select(d => d.ToString())));

        var complexSymbol = compilation
            .GetSymbolsWithName("Complex", SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .First();

        var complexDescriptor = TypeDescriptor.Create(complexSymbol);
        var prop = complexDescriptor.CreateProperties(null).First(p => p.Name == "BethaGammaDeltaName");

        var alphaSymbol = compilation
            .GetSymbolsWithName("Alpha", SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .First();

        var alphaDescriptor = TypeDescriptor.Create(alphaSymbol);

        // Act
        var targetInfo = MatchTypeInfo.Create(alphaDescriptor, MatchOptions.Default);
        var selection = PropertySelection.Select(prop, targetInfo);

        // Assert
        Assert.NotNull(selection);
        
        var sb = new StringBuilder();
        selection!.WritePropertyPath(sb);
        Assert.Equal("Betha.Gamma.Delta.Name", sb.ToString());
    }

    [Fact]
    public void Select_FailsWhenNoParts()
    {
        // Arrange
        Util.Compile(Code.Source, out var compilation, out var diagnostics);
        Assert.True(diagnostics.IsDefaultOrEmpty);

        var alphaSymbol = compilation
            .GetSymbolsWithName("Alpha", SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .First();

        var typeDesc = TypeDescriptor.Create(alphaSymbol);
        // PropertyDescriptor sintético para nome inexistente
        var propMissing = new PropertyDescriptor(TypeDescriptor.Create(alphaSymbol), "DoesNotExist", null);

        // Act
        var matchInfo = MatchTypeInfo.Create(typeDesc, MatchOptions.Default);
        var selection = PropertySelection.Select(propMissing, matchInfo);

        // Assert
        Assert.Null(selection);
    }

    [Fact]
    public void Select_ChainParentSet_RootHasNoParent()
    {
        // Arrange
        Util.Compile(Code.Source, out var compilation, out var diagnostics);
        Assert.True(diagnostics.IsDefaultOrEmpty);

        var alpha = compilation
            .GetSymbolsWithName("Alpha", SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .First();

        var alphaDesc = TypeDescriptor.Create(alpha);
        var bethaProp = alphaDesc.CreateProperties(null).First(p => p.Name == "Betha");

        // Act
        var matchInfo = MatchTypeInfo.Create(alphaDesc, MatchOptions.Default);
        var bethaSel = PropertySelection.Select(bethaProp, matchInfo)!;

        // Assert
        Assert.NotNull(bethaSel);
        Assert.Null(bethaSel.Parent); // Seleção raiz não possui Parent
    }

    [Fact]
    public void Select_ParentRootDeclaringType_EqualsSelfOnRoot()
    {
        // Arrange + Act
        var (selection, _) = Select(Code.Source, "Alpha", "Betha");

        // Assert
        Assert.NotNull(selection);
        Assert.Equal(selection!.RootDeclaringType, selection.PropertyType);
    }
}

// Parte da classe auxiliar para armazenar códigos-fonte usados nos testes.
internal static partial class Code
{
    public const string Source =
        """
        namespace TestPS
        {
            public class Delta
            {
                public string Name { get; set; }
                public string GetOnly { get; }
            }

            public class Gamma
            {
                public Delta Delta { get; set; }
            }

            public class Betha
            {
                public Gamma Gamma { get; set; }
            }

            public class Alpha
            {
                public Betha Betha { get; set; }
            }

            public class AlphaExtended : Alpha { }

            public class Complex
            {
                public Betha Betha { get; set; }
                public string BethaGammaDeltaName { get; set; }
            }
        }
        """;
}