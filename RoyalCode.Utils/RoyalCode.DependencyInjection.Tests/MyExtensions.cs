using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.DependencyInjection.Tests;

public static partial class MyExtensions
{
    [AddServices]
    internal static partial void AddServices(this IServiceCollection services);
}
