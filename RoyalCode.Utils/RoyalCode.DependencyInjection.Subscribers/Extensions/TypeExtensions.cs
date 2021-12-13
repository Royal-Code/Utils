using Microsoft.Extensions.DependencyInjection;
using RoyalCode.DependencyInjection.Subscribers.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// <para>
        ///     Get the <see cref="ServiceLifetime"/> for the <paramref name="type"/>.
        /// </para>
        /// <para>
        ///     To define the <see cref="ServiceLifetime"/>, some <see cref="ServiceLifetimeAttribute"/>
        ///     must be used, like <see cref="SingletonAttribute"/>, <see cref="ScopedAttribute"/> or
        ///     <see cref="TransientAttribute"/>.
        /// </para>
        /// <para>
        ///     If one of these attributes was not found, so the default value will be used, 
        ///     that are <see cref="ServiceLifetime.Transient"/>.
        /// </para>
        /// </summary>
        /// <param name="type">Some <see cref="Type"/>.</param>
        /// <returns>The <see cref="ServiceLifetime"/> for the <paramref name="type"/>.</returns>
        internal static ServiceLifetime GetServiceLifetime(this MemberInfo type)
        {
            return type.GetCustomAttributes(true)
                .OfType<ServiceLifetimeAttribute>()
                .FirstOrDefault()
                ?.Lifetime ?? ServiceLifetime.Transient;
        }

        /// <summary>
        /// Get all elegible interfaces of a service type.
        /// </summary>
        /// <param name="type">The service type.</param>
        /// <returns>All elegible interfaces.</returns>
        internal static IEnumerable<Type> GetElegibleInterfaces(this Type type)
        {
            return type.GetInterfaces()
                .Where(t => !ServiceAttribute.IneligibleServicesInterfaces.Contains(t));
        }

        /// <summary>
        /// Get all interfaces that have <see cref="ServiceAttribute"/> defined.
        /// </summary>
        /// <param name="type">The service type.</param>
        /// <returns>All annotated interfaces.</returns>
        internal static IEnumerable<Type> GetAnnotatedInterfaces(this Type type)
        {
            return type.GetInterfaces()
                .Where(t => t.IsDefined(typeof(ServiceAttribute), false));
        }

        /// <summary>
        /// A foreach.
        /// </summary>
        /// <param name="types">Types to iterate.</param>
        /// <param name="action">The iterator.</param>
        internal static void Each(this IEnumerable<Type> types, Action<Type> action)
        {
            foreach (var type in types)
                action(type);
        }
    }
}
