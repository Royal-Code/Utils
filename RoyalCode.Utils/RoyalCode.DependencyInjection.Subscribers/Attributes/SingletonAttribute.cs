using Microsoft.Extensions.DependencyInjection;
using System;

namespace RoyalCode.DependencyInjection.Subscribers.Attributes
{
    /// <summary>
    /// Specifies the lifetime of a service as singleton in an Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SingletonAttribute : ServiceLifetimeAttribute
    {
        /// <summary>
        /// Create a new attribute.
        /// </summary>
        public SingletonAttribute() : base(ServiceLifetime.Singleton) { }
    }
}
