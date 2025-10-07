using System.Text;
using Microsoft.CodeAnalysis;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Tests.PropertySelectionTests;

public class MatchSelectionTests
{
    /// <summary>
    /// Helper que compila o código de apoio e cria a seleção de matching entre dois tipos declarados no snippet.
    /// </summary>
    private static MatchSelection BuildMatchSelection(string source, string originTypeName, string targetTypeName)
    {
        // Arrange (compilação)
        Util.Compile(source, out var compilation, out var diagnostics);
        Assert.True(diagnostics.IsDefaultOrEmpty, "A compilação do código de teste gerou diagnostics inesperados: " + string.Join("\n", diagnostics.Select(d => d.ToString())));

        var originSymbol = compilation.GetSymbolsWithName(originTypeName, SymbolFilter.Type).OfType<INamedTypeSymbol>().First();
        var targetSymbol = compilation.GetSymbolsWithName(targetTypeName, SymbolFilter.Type).OfType<INamedTypeSymbol>().First();

        var originDescriptor = TypeDescriptor.Create(originSymbol);
        var targetDescriptor = TypeDescriptor.Create(targetSymbol);

        var model = compilation.GetSemanticModel(compilation.SyntaxTrees.First());

        // Act (criação do MatchSelection)
        return MatchSelection.Create(originDescriptor, targetDescriptor, model);
    }

    [Fact]
    public void SinglePropertyMatch()
    {
        // Arrange + Act
        var match = BuildMatchSelection(Code.BaseSource, "OriginFoo", "TargetFoo");
        var propertyMatch = match.PropertyMatches.FirstOrDefault(p => p.Origin.Name == "Name");

        // Assert
        Assert.NotNull(propertyMatch);
        Assert.False(propertyMatch!.IsMissing);
        Assert.True(propertyMatch.CanAssign); // string -> string
    }

    [Fact]
    public void PathPropertyMatch_BarName()
    {
        // Arrange + Act
        var match = BuildMatchSelection(Code.BaseSource, "OriginFoo", "TargetFoo");
        var propertyMatch = match.PropertyMatches.FirstOrDefault(p => p.Origin.Name == "BarName");

        // Assert
        Assert.NotNull(propertyMatch);
        Assert.False(propertyMatch!.IsMissing);

        // Caminho montado a partir de partes do nome PascalCase BarName => Bar.Name
        var sb = new StringBuilder();
        propertyMatch.Target!.WritePropertyPath(sb);
        Assert.Equal("Bar.Name", sb.ToString());
    }

    [Fact]
    public void FailurePropertyMatch_Missing()
    {
        // Arrange + Act
        var match = BuildMatchSelection(Code.BaseSource, "OriginFailureMatch", "TargetFoo");
        var propertyMatch = match.PropertyMatches.FirstOrDefault(p => p.Origin.Name == "Hello");

        // Assert
        Assert.NotNull(propertyMatch);
        Assert.True(propertyMatch!.IsMissing);
        Assert.False(propertyMatch.CanAssign);
    }

    [Fact]
    public void FailureTypePropertyMatch_NotAssignable()
    {
        // Arrange + Act
        var match = BuildMatchSelection(Code.BaseSource, "OriginFailureMatch", "TargetBar");
        var propertyMatch = match.PropertyMatches.FirstOrDefault(p => p.Origin.Name == "Date");

        // Assert
        Assert.NotNull(propertyMatch);
        Assert.False(propertyMatch!.IsMissing); // encontrou a propriedade
        Assert.False(propertyMatch.CanAssign);  // tipos incompatíveis (DateTimeOffset -> DateTime)
    }

    [Fact]
    public void HasMissingProperties_ReturnsTrue()
    {
        // Arrange + Act
        var match = BuildMatchSelection(Code.BaseSource, "OriginFailureMatch", "TargetBar");
        var hasMissing = match.HasMissingProperties(out var missing);

        // Assert
        Assert.True(hasMissing);
        Assert.Contains(missing, p => p.Name == "Hello");
    }

    [Fact]
    public void HasNotAssignableProperties_ReturnsTrue()
    {
        // Arrange + Act
        var match = BuildMatchSelection(Code.BaseSource, "OriginFailureMatch", "TargetBar");
        var hasNotAssignable = match.HasNotAssignableProperties(out var notAssignable);

        // Assert
        Assert.True(hasNotAssignable);
        Assert.Contains(notAssignable, m => m.Origin.Name == "Date");
    }

    [Fact]
    public void ExtendedTypesMatch_NullableToNonNullable()
    {
        // Arrange + Act
        var match = BuildMatchSelection(Code.BaseSource, "OriginQux", "TargetQux");
        var idMatch = match.PropertyMatches.First(p => p.Origin.Name == "Id");
        var nameMatch = match.PropertyMatches.First(p => p.Origin.Name == "Name");

        // Assert
        Assert.False(match.HasMissingProperties(out _));
        Assert.False(idMatch.IsMissing);
        Assert.False(nameMatch.IsMissing);
    }

    [Fact]
    public void ComplexMatch_WithPathId()
    {
        // Arrange + Act
        var match = BuildMatchSelection(Code.BaseSource, "ComplexFilter", "ComplexFoo");
        var barIdMatch = match.PropertyMatches.First(p => p.Origin.Name == "BarId");

        // Assert
        Assert.False(barIdMatch.IsMissing);

        // BarId => Bar.Id
        var sb = new StringBuilder();
        barIdMatch.Target!.WritePropertyPath(sb);
        Assert.Equal("Bar.Id", sb.ToString());
    }

    [Fact]
    public void DeepPathMatch_Level1Level2Name()
    {
        // Arrange + Act
        var match = BuildMatchSelection(Code.BaseSource, "DeepOrigin", "DeepRoot");
        var deepMatch = match.PropertyMatches.First(p => p.Origin.Name == "Level1Level2Name");

        // Assert
        Assert.False(deepMatch.IsMissing);

        // Level1Level2Name => Level1.Level2.Name
        var sb = new StringBuilder();
        deepMatch.Target!.WritePropertyPath(sb);
        Assert.Equal("Level1.Level2.Name", sb.ToString());
    }
}


// Parte da classe auxiliar para armazenar códigos-fonte usados nos testes.
internal static partial class Code
{
    public const string BaseSource =
        """
        using System;
        namespace TestModels
        {
            public class TargetBar
            {
                public string Name { get; set; }
                public DateTime Date { get; set; }
            }

            public class TargetFoo
            {
                public string Name { get; set; }
                public TargetBar Bar { get; set; }
            }

            public class OriginFoo
            {
                public string Name { get; set; }
                public TargetBar Bar { get; set; }
                public string BarName { get; set; }
            }

            public class OriginFailureMatch
            {
                public string Hello { get; set; }
                public DateTimeOffset Date { get; set; }
            }

            public interface ITargetBar
            {
                string Name { get; set; }
            }

            public class TargetBar2 : ITargetBar
            {
                public string Name { get; set; }
                public DateTime Date { get; set; }
            }

            public class TargetQuxBase
            {
                public int Id { get; set; }
            }

            public class TargetQux : TargetQuxBase
            {
                public string Name { get; set; }
            }

            public class OriginQux
            {
                public int? Id { get; set; }
                public string? Name { get; set; }
            }

            public class ComplexBase
            {
                public int Id { get; set; }
            }

            public class ComplexBar : ComplexBase
            {
                public string Name { get; set; }
            }

            public class ComplexFoo : ComplexBase
            {
                public string Name { get; set; }
                public ComplexBar Bar { get; set; }
            }

            public class ComplexFilter
            {
                public int? Id { get; set; }
                public int? BarId { get; set; }
            }

            public class DeepLevel2
            {
                public string Name { get; set; }
            }

            public class DeepLevel1
            {
                public DeepLevel2 Level2 { get; set; }
            }

            public class DeepRoot
            {
                public DeepLevel1 Level1 { get; set; }
            }

            public class DeepOrigin
            {
                public string Level1Level2Name { get; set; }
            }
        }
        """;
}