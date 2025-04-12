using System.Diagnostics;

namespace RoyalCode.DependencyInjection;

/// <summary>
/// Attribute to mark a method as a service factory, where the return type is the service type produced.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("COMPILE_TIME_ONLY")]
public class ServiceFactoryAttribute : Attribute { }