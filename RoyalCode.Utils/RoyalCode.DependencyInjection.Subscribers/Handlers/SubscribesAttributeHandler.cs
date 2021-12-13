using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RoyalCode.DependencyInjection.Subscribers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.DependencyInjection.Subscribers.Handlers
{
    internal class SubscribesAttributeHandler : IMethodSubscriptionHandler
    {
        private static readonly MethodInfo GetServiceMethod = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService))!;

        internal static readonly Dictionary<Type, Action<IServiceCollection, Type, MethodInfo>> SubscribesMethodProcessor =
            new()
            {
                { typeof(ProducesAttribute), ProducesAttributeProcessor }
            };

        public bool Accept(Type type) => type.IsDefined(typeof(SubscribesAttribute), true);

        public bool Accept(MethodInfo method) => method.GetCustomAttributes()
            .Any(a => SubscribesMethodProcessor.ContainsKey(a.GetType()));

        public void Proccess(IServiceCollection services, Type serviceType, MethodInfo method)
        {
            if(!serviceType.IsDefined(typeof(ServiceAttribute), false))
            {
                var lifetime = serviceType.GetServiceLifetime();
                services.TryAdd(ServiceDescriptor.Describe(serviceType, serviceType, lifetime));
            }

            foreach(var attr in method.GetCustomAttributes())
            {
                if (SubscribesMethodProcessor.TryGetValue(attr.GetType(), out var processor))
                    processor(services, serviceType, method);
            }
        }

        private static void ProducesAttributeProcessor(IServiceCollection services, Type type, MethodInfo method)
        {
            var spVariable = Expression.Parameter(typeof(IServiceProvider), "sp");
            
            var getServiceCall = Expression.Call(
                spVariable,
                GetServiceMethod,
                Expression.Constant(type));

            List<Expression> arguments = new();
            foreach (var parameter in method.GetParameters())
            {
                arguments.Add(Expression.Call(
                spVariable,
                GetServiceMethod,
                Expression.Constant(parameter.ParameterType)));
            }

            var producerCall = Expression.Call(
                getServiceCall,
                method,
                arguments);

            var lambda = Expression.Lambda<Func<IServiceProvider, object>>(producerCall, spVariable);
            var factory = lambda.Compile();

            var lifetime = method.GetServiceLifetime();

            services.Add(ServiceDescriptor.Describe(method.ReturnType, factory, lifetime));
        }
    }
}
