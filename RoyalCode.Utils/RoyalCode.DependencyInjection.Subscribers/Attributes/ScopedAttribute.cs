using Microsoft.Extensions.DependencyInjection;
using System;

namespace RoyalCode.DependencyInjection.Subscribers.Attributes
{
    /// <summary>
    /// Specifies the lifetime of a service as scoped in an Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ScopedAttribute : ServiceLifetimeAttribute
    {
        /// <summary>
        /// Create a new attribute.
        /// </summary>
        public ScopedAttribute() : base(ServiceLifetime.Scoped) { }
    }
}
