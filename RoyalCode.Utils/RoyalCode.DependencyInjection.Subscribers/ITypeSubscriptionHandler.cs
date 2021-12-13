using Microsoft.Extensions.DependencyInjection;
using System;

namespace RoyalCode.DependencyInjection.Subscribers
{
    /// <summary>
    /// Subscription handler to process some action over a type.
    /// </summary>
    public interface ITypeSubscriptionHandler : ISubscriptionHandler
    {
        /// <summary>
        /// Process the type and add some services if required.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="type">The type to be processed.</param>
        void Proccess(IServiceCollection services, Type type);
    }
}
