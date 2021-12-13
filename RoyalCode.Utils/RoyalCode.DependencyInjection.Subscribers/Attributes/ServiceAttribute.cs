using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace RoyalCode.DependencyInjection.Subscribers.Attributes
{
    /// <summary>
    /// The Service sttribute is used in classes that must be registered in the <see cref="IServiceCollection"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// <para>
        ///     Interfaces that are not elegible as a service.
        /// </para>
        /// <para>
        ///     By default, <see cref="IDisposable"/> are not elegible.
        /// </para>
        /// </summary>
        public static readonly ICollection<Type> IneligibleServicesInterfaces = new List<Type>()
        {
            typeof(IDisposable)
        };

        /// <summary>
        /// Create as Default.
        /// </summary>
        public ServiceAttribute() { }

        /// <summary>
        /// Create with method <see cref="RegistrationMethod.RegisterTheClassAsAServiceAndTheSpecifiedInterfaces"/>
        /// and set the specified services.
        /// </summary>
        /// <param name="specificServices">The specified services.</param>
        public ServiceAttribute(params Type[] specificServices)
        {
            Method = RegistrationMethod.RegisterTheClassAsAServiceAndTheSpecifiedInterfaces;
            SpecificServices = specificServices;
        }

        /// <summary>
        /// Create by defining the method and specific services.
        /// </summary>
        /// <param name="method">The registration method.</param>
        /// <param name="specificServices">The specified services.</param>
        public ServiceAttribute(RegistrationMethod method, params Type[] specificServices)
        {
            Method = method;
            SpecificServices = specificServices;
        }

        /// <summary>
        /// It defines the method, the form, in which the class will be registered,
        /// and the interfaces that the class implements, as a service.
        /// </summary>
        public RegistrationMethod Method { get; set; } = 
            RegistrationMethod.RegisterTheClassAsAServiceAndCreateFactoryForTheInterfaces;

        /// <summary>
        /// <para>
        ///     The Specific serviçoes.
        /// </para>
        /// <para>
        ///     Used when <see cref="Method"/> 
        ///     is <see cref="RegistrationMethod.RegisterTheClassAsAServiceAndTheSpecifiedInterfaces"/>
        ///     or <see cref="RegistrationMethod.RegisterOnlyTheSpecifiedInterfaces"/>.
        /// </para>
        /// </summary>
        public IEnumerable<Type>? SpecificServices { get; }
    }
}
