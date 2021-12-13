using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace RoyalCode.DependencyInjection.Subscribers
{
    /// <summary>
    /// Subscription handler to process som action over a method.
    /// </summary>
    public interface IMethodSubscriptionHandler : ISubscriptionHandler
    {
        /// <summary>
        /// Check if the handler accpets the method.
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns>True if the method must be processed, false otherwise.</returns>
        bool Accept(MethodInfo method);

        /// <summary>
        /// Process the method and register what is needed.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="method">The method to be processed.</param>
        void Proccess(IServiceCollection services, Type serviceType, MethodInfo method);
    }
}
