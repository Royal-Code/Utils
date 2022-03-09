namespace RoyalCode.Extensions.PropertySelection.Tests.Models;

public class Alpha
{
    public Betha Betha { get; set; }
}

public class Betha
{
    public Gamma Gamma { get; set; }
}

public class Gamma
{
    public Delta Delta { get; set; }
}

public class Delta
{
    public string Name { get; set; }

    public string GetOnly => "Get Only";
}