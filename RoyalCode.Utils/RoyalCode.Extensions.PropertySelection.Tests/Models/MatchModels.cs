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

public class TargetQuxBase
{
    public int Id { get; set; }
}

public class TargetQux : TargetQuxBase
{
    public string Name { get; set; } = null!;
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
    public string Name { get; set; } = null!;
}

public class ComplexFoo : ComplexBase
{
    public string Name { get; set; } = null!;

    public ComplexBar Bar { get; set; } = null!;
}

public class ComplexFilter
{
    public int? Id { get; set; }

    public int? BarId { get; set; }
}