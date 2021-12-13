using Microsoft.Extensions.DependencyInjection;
using System;

namespace RoyalCode.DependencyInjection.Subscribers.Attributes
{
    /// <summary>
    /// Specifies the lifetime of a service in an Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class ServiceLifetimeAttribute : Attribute
    {
        /// <summary>
        /// Create a new attribute with the <see cref="ServiceLifetime"/>.
        /// </summary>
        /// <param name="lifetime">The lifetime of a service.</param>
        public ServiceLifetimeAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        /// <summary>
        /// The lifetime of a service.
        /// </summary>
        public ServiceLifetime Lifetime { get; }
    }
}
