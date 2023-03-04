namespace RoyalCode.Extensions.PropertySelection.Tests.Models;

public class Alpha
{
    public Betha Betha { get; set; } = null!;
}

public class Betha
{
    public Gamma Gamma { get; set; } = null!;
}

public class Gamma
{
    public Delta Delta { get; set; } = null!;
}

public class Delta
{
    public string Name { get; set; } = null!;

    public string GetOnly => "Get Only";
}

public class AlphaExtended : Alpha { }

