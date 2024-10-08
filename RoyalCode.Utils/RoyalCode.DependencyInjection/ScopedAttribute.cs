﻿namespace RoyalCode.DependencyInjection;

/// <summary>
/// Specifies the lifetime of a service as scoped in a Microsoft.Extensions.DependencyInjection.IServiceCollection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ScopedAttribute : Attribute { }