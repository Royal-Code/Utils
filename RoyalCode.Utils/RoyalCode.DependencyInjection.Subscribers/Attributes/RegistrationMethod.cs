using System;

namespace RoyalCode.DependencyInjection.Subscribers.Attributes
{
    /// <summary>
    /// The method used when register the service.
    /// </summary>
    public enum RegistrationMethod
    {
        /// <summary>
        /// It register the class (the concrete type) as a service and register the interfaces
        /// with a factory that resolve the class (concrete type) as the instance.
        /// </summary>
        RegisterTheClassAsAServiceAndCreateFactoryForTheInterfaces,

        /// <summary>
        /// It registers the class (the concrete type) as a service and the interfaces, but all independently.
        /// </summary>
        RegisterTheClassAsAServiceAndTheInterfacesIndependently,

        /// <summary>
        /// It registers all the interfaces as a service.
        /// </summary>
        RegisterAllTheInterfacesIndependently,

        /// <summary>
        /// It register the class (the concrete type) as a service and register the interfaces
        /// annotated with the <see cref="SerializableAttribute"/>
        /// with a factory that resolve the class (concrete type) as the instance.
        /// </summary>
        RegisterTheClassAsAServiceAndTheAnnotatedIntefaces,

        /// <summary>
        /// Only registers the interfaces annotated with the same attribute
        /// (<see cref="SerializableAttribute"/>) as a service.
        /// </summary>
        RegisterOnlyTheAnnotatedInterfaces,

        /// <summary>
        /// Register only the concrete type (the class) as a service.
        /// </summary>
        RegisterTheClassOnly,

        /// <summary>
        /// It register the class as a service and create a factory for the specified interfaces.
        /// </summary>
        RegisterTheClassAsAServiceAndTheSpecifiedInterfaces,

        /// <summary>
        /// It register only the specified interfaces independently.
        /// </summary>
        RegisterOnlyTheSpecifiedInterfaces,
    }
}
