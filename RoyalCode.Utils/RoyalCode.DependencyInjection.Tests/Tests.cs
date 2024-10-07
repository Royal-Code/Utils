using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace RoyalCode.DependencyInjection.Tests;

public class Tests
{
    [Fact]
    public void TestAll()
    {
        Util.Compile(Code, out var output, out var diagnostics);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

        var generatedMap = output.SyntaxTrees.Skip(1).FirstOrDefault()?.ToString();
        generatedMap.Should().Be(Expected);
    }

    private const string Code =
"""
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.DependencyInjection.Tests;

public static partial class MyExtensions
{
    [AddServices]
    public static partial void AddServices(this IServiceCollection services);
}

[Service, Singleton]
public sealed class SimpleService { }

[Service, Singleton]
public sealed class SimpleServiceGene<T> { }

[Service, Singleton]
public sealed class SimpleServiceGene2<T1, T2> { }

public interface ICommonService { }

[Service, Scoped]
public class CommonService : ICommonService { }

public interface ICommonServiceGene<T> { }

[Service, Scoped]
public class CommonServiceGene<T> : ICommonServiceGene<T> { }

public interface ICommonServiceGene2<T1, T2> { }

[Service, Scoped]
public class CommonServiceGene2<T1, T2> : ICommonServiceGene2<T1, T2> { }

public interface IMultiServiceA { }

public interface IMultiServiceB { }

[Service]
public class MultiService : IMultiServiceA, IMultiServiceB { }
""";

    private const string Expected =
"""
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.DependencyInjection.Tests;

public static partial class MyExtensions
{
    public static partial void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<SimpleService>();
        services.AddSingleton(typeof(SimpleServiceGene<>));
        services.AddSingleton(typeof(SimpleServiceGene2<,>));
        services.AddScoped<ICommonService, CommonService>();
        services.AddScoped(typeof(ICommonServiceGene<>), typeof(CommonServiceGene<>));
        services.AddScoped(typeof(ICommonServiceGene2<,>), typeof(CommonServiceGene2<,>));
        services.AddTransient<MultiService>();
        services.AddTransient<IMultiServiceA>(sp => sp.GetService<MultiService>());
        services.AddTransient<IMultiServiceB>(sp => sp.GetService<MultiService>());
    }
}

""";
}



