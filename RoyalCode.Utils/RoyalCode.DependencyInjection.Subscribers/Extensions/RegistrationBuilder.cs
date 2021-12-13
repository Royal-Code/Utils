using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Utilitary for register one concrete type and multiples interfaces with a factory.
    /// </summary>
    public class RegistrationBuilder
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="implementationType">The service, a concrete type that can be instanciated.</param>
        /// <param name="serviceLifetime">The service lifetime scope.</param>
        /// <returns></returns>
        internal static RegistrationBuilder Create(
            IServiceCollection services,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
#else
            Type implementationType,
#endif
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            return new RegistrationBuilder(services, implementationType, serviceLifetime);
        }

        private readonly IServiceCollection services;
        private readonly Type implementationType;
        private readonly ServiceLifetime lifetime;

        private RegistrationBuilder(IServiceCollection services, Type implementationType, ServiceLifetime lifetime)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.implementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            this.lifetime = lifetime;

            services.Add(ServiceDescriptor.Describe(implementationType, implementationType, lifetime));
        }

        /// <summary>
        /// Adds an interface that can be resolved from the service registered as a concrete type.
        /// </summary>
        /// <param name="serviceType">The service type, some interface.</param>
        /// <returns>The same instance for chain calls.</returns>
        public RegistrationBuilder As(Type serviceType)
        {
            services.Add(ServiceDescriptor.Describe(serviceType, sp => sp.GetService(implementationType)!, lifetime));
            return this;
        }

        /// <summary>
        /// Adds a collection of interfaces that each one can be resolved from the service registered as a concrete type.
        /// </summary>
        /// <param name="servicesTypes">The collection of services types.</param>
        /// <returns>The same instance for chain calls.</returns>
        public RegistrationBuilder As(IEnumerable<Type> servicesTypes)
        {
            foreach (var serviceType in servicesTypes)
            {
                As(serviceType);
            }
            return this;
        }
    }
}
