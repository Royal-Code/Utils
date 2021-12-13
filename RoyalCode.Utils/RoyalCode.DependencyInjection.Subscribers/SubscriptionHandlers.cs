using Microsoft.Extensions.DependencyInjection;
using RoyalCode.DependencyInjection.Subscribers.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoyalCode.DependencyInjection.Subscribers
{
    /// <summary>
    /// <para>
    ///     A Global class for handle the services subscriptions.
    /// </para>
    /// <para>
    ///     All the handlers must be added here before start subscribing the services.
    /// </para>
    /// </summary>
    public static class SubscriptionHandlers
    {
        private static readonly ICollection<ISubscriptionHandler> subscriptionHandlers =
            new List<ISubscriptionHandler>()
            {
                new ServiceAttributeHandler(),
                new SubscribesAttributeHandler()
            };

        /// <summary>
        /// Add an subcription handler.
        /// </summary>
        /// <param name="handler">The subscription handler.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="handler"/> was null.</exception>
        public static void AddSubscriptionHandler(ISubscriptionHandler handler)
        {
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            subscriptionHandlers.Add(handler);
        }

        /// <summary>
        /// Add a processor for some method decorated with the attribute <paramref name="attributeType"/>.
        /// </summary>
        /// <param name="attributeType">The attribute type that the method must contains.</param>
        /// <param name="processor">The action to process the registrations for the method.</param>
        public static void AddMethodProcessor(Type attributeType, Action<IServiceCollection, Type, MethodInfo> processor)
        {
            SubscribesAttributeHandler.SubscribesMethodProcessor[attributeType] = processor;
        }

        /// <summary>
        /// Add a processor for some method decorated with the attribute <typeparamref name="TAttribute"/>.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type that the method must contains.</typeparam>
        /// <param name="processor">The action to process the registrations for the method.</param>
        public static void AddMethodProcessor<TAttribute>(Action<IServiceCollection, Type, MethodInfo> processor)
            where TAttribute : Attribute
        {
            AddMethodProcessor(typeof(TAttribute), processor);
        }

        internal static IEnumerable<Action<IServiceCollection>> LookupHandlers(Type type)
        {
            foreach (var handler in subscriptionHandlers.Where(h => h.Accept(type)))
            {
                if (handler is ITypeSubscriptionHandler typeHandler)
                {
                    Action<IServiceCollection> action = srv => typeHandler.Proccess(srv, type);
                    yield return action;
                }
                if (handler is IMethodSubscriptionHandler methodHandler)
                {

                    var methods = type.GetAllPublicMethods()
                        .Where(rm => methodHandler.Accept(rm));
                    foreach (var method in methods)
                    {
                        Action<IServiceCollection> action = srv => methodHandler.Proccess(srv, type, method);
                        yield return action;
                    }
                }
            }
        }

        internal static IEnumerable<MethodInfo> GetAllPublicMethods(this Type type)
        {
            Type? currentType = type;
            while (currentType is not null)
            {
                foreach (var method in currentType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    yield return method;
                }
                currentType = currentType.BaseType;
                if (currentType == typeof(object))
                    currentType = null;

            }
        }
    }
}
