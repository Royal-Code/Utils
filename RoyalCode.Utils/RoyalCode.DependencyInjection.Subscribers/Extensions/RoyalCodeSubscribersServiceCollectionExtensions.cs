using RoyalCode.DependencyInjection.Subscribers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class RoyalCodeSubscribersServiceCollectionExtensions
    {
        /// <summary>
        /// Utilitary for register one concrete type and multiples interfaces with a factory.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="implementationType">The service, a concrete type that can be instanciated.</param>
        /// <param name="serviceLifetime">The service lifetime scope.</param>
        /// <returns>
        ///     A new instance of <see cref="RegistrationBuilder"/> to complete the service registration.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="services"/> or <paramref name="implementationType"/> was null.
        /// </exception>
        public static RegistrationBuilder Register(
            this IServiceCollection services,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
#else
            Type implementationType,
#endif
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (implementationType is null)
                throw new ArgumentNullException(nameof(implementationType));

            return RegistrationBuilder.Create(services, implementationType, serviceLifetime);
        }

        /// <summary>
        /// <para>
        ///     Utilitary for register one concrete type and multiples interfaces with a factory.
        /// </para>
        /// <para>
        ///     The service lifetime scope will be transient.
        /// </para>
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="implementationType">The service, a concrete type that can be instanciated.</param>
        /// <returns>
        ///     A new instance of <see cref="RegistrationBuilder"/> to complete the service registration.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="services"/> or <paramref name="implementationType"/> was null.
        /// </exception>
        public static RegistrationBuilder RegisterTransient(
            this IServiceCollection services,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
#else
            Type implementationType)
#endif
            => services.Register(implementationType, ServiceLifetime.Transient);

        /// <summary>
        /// <para>
        ///     Utilitary for register one concrete type and multiples interfaces with a factory.
        /// </para>
        /// <para>
        ///     The service lifetime scope will be transient.
        /// </para>
        /// </summary>
        /// <typeparam name="TImplementationType">The service, a concrete type that can be instanciated.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>
        ///     A new instance of <see cref="RegistrationBuilder"/> to complete the service registration.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="services"/> was null.
        /// </exception>
        public static RegistrationBuilder RegisterTransient<TImplementationType>(this IServiceCollection services)
            => services.Register(typeof(TImplementationType), ServiceLifetime.Transient);

        /// <summary>
        /// <para>
        ///     Utilitary for register one concrete type and multiples interfaces with a factory.
        /// </para>
        /// <para>
        ///     The service lifetime scope will be scoped.
        /// </para>
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="implementationType">The service, a concrete type that can be instanciated.</param>
        /// <returns>
        ///     A new instance of <see cref="RegistrationBuilder"/> to complete the service registration.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="services"/> or <paramref name="implementationType"/> was null.
        /// </exception>
        public static RegistrationBuilder RegisterScoped(
            this IServiceCollection services,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
#else
            Type implementationType)
#endif
            => services.Register(implementationType, ServiceLifetime.Scoped);

        /// <summary>
        /// <para>
        ///     Utilitary for register one concrete type and multiples interfaces with a factory.
        /// </para>
        /// <para>
        ///     The service lifetime scope will be scoped.
        /// </para>
        /// </summary>
        /// <typeparam name="TImplementationType">The service, a concrete type that can be instanciated.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>
        ///     A new instance of <see cref="RegistrationBuilder"/> to complete the service registration.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="services"/> was null.
        /// </exception>
        public static RegistrationBuilder RegisterScoped<TImplementationType>(this IServiceCollection services)
            => services.Register(typeof(TImplementationType), ServiceLifetime.Scoped);

        /// <summary>
        /// <para>
        ///     Utilitary for register one concrete type and multiples interfaces with a factory.
        /// </para>
        /// <para>
        ///     The service lifetime scope will be singleton.
        /// </para>
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="implementationType">The service, a concrete type that can be instanciated.</param>
        /// <returns>
        ///     A new instance of <see cref="RegistrationBuilder"/> to complete the service registration.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="services"/> or <paramref name="implementationType"/> was null.
        /// </exception>
        public static RegistrationBuilder RegisterSingleton(
            this IServiceCollection services,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
#else
            Type implementationType)
#endif
            => services.Register(implementationType, ServiceLifetime.Singleton);

        /// <summary>
        /// <para>
        ///     Utilitary for register one concrete type and multiples interfaces with a factory.
        /// </para>
        /// <para>
        ///     The service lifetime scope will be singleton.
        /// </para>
        /// </summary>
        /// <typeparam name="TImplementationType">The service, a concrete type that can be instanciated.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>
        ///     A new instance of <see cref="RegistrationBuilder"/> to complete the service registration.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="services"/> was null.
        /// </exception>
        public static RegistrationBuilder RegisterSingleton<TImplementationType>(this IServiceCollection services)
            => services.Register(typeof(TImplementationType), ServiceLifetime.Singleton);

        /// <summary>
        /// Executes various handlers to register services and other components from a type.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="type">The type to be processed.</param>
        /// <returns>The same instance of <paramref name="services"/> for chain calls.</returns>
        public static IServiceCollection Subscribes(this IServiceCollection services,
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
#else
            Type type)
#endif
        {
            foreach (var action in SubscriptionHandlers.LookupHandlers(type))
            {
                action(services);
            }
            return services;
        }

        /// <summary>
        /// Executes various handlers to register services and other components from a type.
        /// </summary>
        /// <typeparam name="T">The type to be processed.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The same instance of <paramref name="services"/> for chain calls.</returns>
        public static IServiceCollection Subscribes<T>(this IServiceCollection services)
            => services.Subscribes(typeof(T));

        /// <summary>
        /// Apply the subscribes to all types from the assembly.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The same instance of <paramref name="services"/> for chain calls.</returns>
        public static IServiceCollection Subscribes(this IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                services.Subscribes(type);
            }
            return services;
        }

        /// <summary>
        /// Apply the subscribes to all types from the assembly of defined type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the assembly</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The same instance of <paramref name="services"/> for chain calls.</returns>
        public static IServiceCollection SubscribesAssemblyOfType<T>(this IServiceCollection services)
            => services.Subscribes(typeof(T).Assembly);
    }
}
