﻿using System.Diagnostics;

namespace RoyalCode.DependencyInjection;

/// <summary>
/// Specifies the lifetime of a service as transient in an Microsoft.Extensions.DependencyInjection.IServiceCollection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
[Conditional("COMPILE_TIME_ONLY")]
public class TransientAttribute : Attribute { }