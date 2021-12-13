using Microsoft.Extensions.DependencyInjection;
using RoyalCode.DependencyInjection.Subscribers.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace RoyalCode.DependencyInjection.Subscribers.Handlers
{
    internal class ServiceAttributeHandler : ITypeSubscriptionHandler
    {
        public bool Accept(Type type) => type.IsDefined(typeof(ServiceAttribute), false);

        public void Proccess(IServiceCollection services, Type type)
        {
            var attr = type.GetCustomAttribute<ServiceAttribute>()!;
            var lifetime = type.GetServiceLifetime();
            switch (attr.Method)
            {
                case RegistrationMethod.RegisterTheClassAsAServiceAndCreateFactoryForTheInterfaces:
                    services.Register(type, lifetime).As(type.GetElegibleInterfaces());

                    break;
                case RegistrationMethod.RegisterTheClassAsAServiceAndTheInterfacesIndependently:
                    services.Add(ServiceDescriptor.Describe(type, type, lifetime));
                    type.GetElegibleInterfaces().Each(t => ServiceDescriptor.Describe(t, type, lifetime));

                    break;
                case RegistrationMethod.RegisterAllTheInterfacesIndependently:
                    type.GetElegibleInterfaces().Each(t => ServiceDescriptor.Describe(t, type, lifetime));

                    break;
                case RegistrationMethod.RegisterTheClassAsAServiceAndTheAnnotatedIntefaces:
                    services.Register(type, lifetime).As(type.GetAnnotatedInterfaces());

                    break;
                case RegistrationMethod.RegisterOnlyTheAnnotatedInterfaces:
                    type.GetAnnotatedInterfaces().Each(t => ServiceDescriptor.Describe(t, type, lifetime));

                    break;
                case RegistrationMethod.RegisterTheClassOnly:
                    services.Add(ServiceDescriptor.Describe(type, type, lifetime));

                    break;
                case RegistrationMethod.RegisterTheClassAsAServiceAndTheSpecifiedInterfaces:
                    services.Register(type, lifetime).As(attr.SpecificServices ?? Enumerable.Empty<Type>());

                    break;
                case RegistrationMethod.RegisterOnlyTheSpecifiedInterfaces:
                    attr.SpecificServices?.Each(t => ServiceDescriptor.Describe(t, type, lifetime));

                    break;
                default:
                    throw new NotSupportedException($"The registration method is not supported ('{attr.Method}')");
            }
        }
    }
}
