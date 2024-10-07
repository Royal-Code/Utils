using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.DependencyInjection.Tests;

public static partial class MyExtensions
{
    [AddServices]
    public static partial void AddServices(this IServiceCollection services);
}

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
        services.AddTransient<IMultiServiceA>(sp => sp.GetService<MultiService>()!);
        services.AddTransient<IMultiServiceB>(sp => sp.GetService<MultiService>()!);
    }
}