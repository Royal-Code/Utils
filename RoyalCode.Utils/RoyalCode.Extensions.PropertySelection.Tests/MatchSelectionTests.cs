using System;
using System.Linq;
using RoyalCode.Extensions.PropertySelection.Tests.Models;
using Xunit;

namespace RoyalCode.Extensions.PropertySelection.Tests;

public class MatchSelectionTests
{
    [Fact]
    public void SinglePropertyMatch()
    {
        var match = typeof(OriginFoo).MatchProperties<TargetFoo>();

        var propertyMatch = match.PropertyMatches
            .FirstOrDefault(p => p.OriginProperty.Name == nameof(OriginFoo.Name));
        
        Assert.NotNull(propertyMatch);
        Assert.True(propertyMatch!.Match);
    }
    
    [Fact]
    public void TypePropertyMatch()
    {
        var match = typeof(OriginFoo).MatchProperties<TargetFoo>();

        var propertyMatch = match.PropertyMatches
            .FirstOrDefault(p => p.OriginProperty.Name == nameof(OriginFoo.Bar));
        
        Assert.NotNull(propertyMatch);
        Assert.True(propertyMatch!.Match);
        Assert.True(propertyMatch.TypeMatch);
    }
    
    [Fact]
    public void PathPropertyMatch()
    {
        var match = typeof(OriginFoo).MatchProperties<TargetFoo>();

        var propertyMatch = match.PropertyMatches
            .FirstOrDefault(p => p.OriginProperty.Name == nameof(OriginFoo.BarName));
        
        Assert.NotNull(propertyMatch);
        Assert.True(propertyMatch!.Match);
    }

    [Fact]
    public void EnsureAllMatchedOk()
    {
        var match = typeof(OriginFoo).MatchProperties<TargetFoo>();
        match.EnsureAllMatched();
    }
    
    [Fact]
    public void FailurePropertyMatch()
    {
        var match = typeof(OriginFailureMatch).MatchProperties<TargetBar>();

        var propertyMatch = match.PropertyMatches
            .FirstOrDefault(p => p.OriginProperty.Name == nameof(OriginFailureMatch.Hello));
        
        Assert.NotNull(propertyMatch);
        Assert.False(propertyMatch!.Match);
    }
    
    [Fact]
    public void FailureTypePropertyMatch()
    {
        var match = typeof(OriginFailureMatch).MatchProperties<TargetBar>();

        var propertyMatch = match.PropertyMatches
            .FirstOrDefault(p => p.OriginProperty.Name == nameof(OriginFailureMatch.Date));
        
        Assert.NotNull(propertyMatch);
        Assert.True(propertyMatch!.Match);
        Assert.False(propertyMatch.TypeMatch);
    }
    
    [Fact]
    public void EnsureAllMatchedFail()
    {
        var match = typeof(OriginFailureMatch).MatchProperties<TargetBar>();
        
        var exception = Assert.Throws<InvalidOperationException>(() => match.EnsureAllMatched());

        var message =
            "The property 'Hello' of type 'OriginFailureMatch' has no-match to a property of type 'TargetBar'.";
        
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ExtendedTypesMacth()
    {
        var match = typeof(OriginQux).MatchProperties<TargetQux>();

        match.EnsureAllMatched();


    }

    [Fact]
    public void ComplexMacth()
    {
        var match = typeof(ComplexFilter).MatchProperties<ComplexFoo>();

        match.EnsureAllMatched();
    }
}