using System.Diagnostics;

namespace RoyalCode.DependencyInjection;

/// <summary>
/// Specifies the lifetime of a service as singleton in a Microsoft.Extensions.DependencyInjection.IServiceCollection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
[Conditional("COMPILE_TIME_ONLY")]
public class SingletonAttribute : Attribute { }