using System;

namespace RoyalCode.Extensions.PropertySelection.Tests.Models;

public class TargetFoo
{
    public string Name { get; set; }

    public ITargetBar Bar { get; set; }
}

public interface ITargetBar
{
    public string Name { get; set; }
}

public class TargetBar : ITargetBar
{
    public string Name { get; set; }
    
    public DateTime Date { get; set; }
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