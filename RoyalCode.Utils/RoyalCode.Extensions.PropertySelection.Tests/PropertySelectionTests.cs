using System;
using System.Linq.Expressions;
using RoyalCode.Extensions.PropertySelection.Tests.Models;
using Xunit;

namespace RoyalCode.Extensions.PropertySelection.Tests;

public class PropertySelectionTests
{
    [Fact]
    public void T01_SelectSingleProperty()
    {
        var selected = typeof(Alpha).SelectProperty("Betha");
        Assert.NotNull(selected);
        Assert.Equal("Betha", selected.ToString());
    }

    [Fact]
    public void T02_SelectTwoPropertiesWithDots()
    {
        var selected = typeof(Alpha).SelectProperty("Betha.Gamma");
        Assert.NotNull(selected);
        Assert.Equal("Betha.Gamma", selected.ToString());
    }

    [Fact]
    public void T03_SelectTwoPropertiesWithPascalCase()
    {
        var selected = typeof(Alpha).SelectProperty("BethaGamma");
        Assert.NotNull(selected);
        Assert.Equal("Betha.Gamma", selected.ToString());
    }

    [Fact]
    public void T04_SelectPropertiesWithPascalCaseAndDots()
    {
        var selected = typeof(Alpha).SelectProperty("BethaGamma.DeltaName");
        Assert.NotNull(selected);
        Assert.Equal("Betha.Gamma.Delta.Name", selected.ToString());
    }

    [Fact]
    public void T05_TrySelectPropertyThatDoesNotExist()
    {
        var selection = PropertySelection.Select(typeof(Alpha), "DoesNotExists", false);
        Assert.Null(selection);
    }

    [Fact]
    public void T06_ThrowWhenSelectPropertyThatDoesNotExist()
    {
        Assert.Throws<ArgumentException>(() => PropertySelection.Select(typeof(Alpha), "DoesNotExists"));
    }

    [Fact]
    public void T07_SelectChild()
    {
        var selection = typeof(Alpha).SelectProperty("BethaGamma");
        selection = selection.SelectChild("DeltaName")!;
        Assert.Equal("Betha.Gamma.Delta.Name", selection.ToString());
    }

    [Theory]
    [InlineData("BethaGammaDeltaName", true)]
    [InlineData("BethaGammaDeltaGetOnly", false)]
    public void T08_CanRead(string path, bool canSetExtected)
    {
        var selected = typeof(Alpha).SelectProperty(path);
        var canSet = selected.CanSetValue();
        Assert.Equal(canSetExtected, canSet);
    }

    [Fact]
    public void T09_GetAccessExpression()
    {
        var selected = typeof(Alpha).SelectProperty("BethaGammaDeltaName");
        var root = Expression.Variable(typeof(Alpha), "root");
        var expr = selected.GetAccessExpression(root);
        
        Assert.NotNull(expr);
        Assert.Equal("root.Betha.Gamma.Delta.Name", expr.ToString());
    }
    
    [Fact]
    public void T10_GetAccessExpressionFromExtendedRootType()
    {
        var selected = typeof(Alpha).SelectProperty("BethaGammaDeltaName");
        var root = Expression.Variable(typeof(AlphaExtended), "root");
        var expr = selected.GetAccessExpression(root);
        
        Assert.NotNull(expr);
        Assert.Equal("root.Betha.Gamma.Delta.Name", expr.ToString());
    }
}