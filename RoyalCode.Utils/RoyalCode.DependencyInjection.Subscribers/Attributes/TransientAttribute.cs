namespace RoyalCode.DependencyInjection.Subscribers.Attributes;

/// <summary>
/// Specifies the lifetime of a service as transient in an Microsoft.Extensions.DependencyInjection.IServiceCollection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TransientAttribute { }