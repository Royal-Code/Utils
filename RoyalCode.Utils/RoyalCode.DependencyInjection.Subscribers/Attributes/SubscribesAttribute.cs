using System;

namespace RoyalCode.DependencyInjection.Subscribers.Attributes
{
    /// <summary>
    /// <para>
    ///     Default attribute to processes the methods of annotated services.
    /// </para>
    /// <para>
    ///     To include handlers to process some method, use the methods
    ///     <see cref="SubscriptionHandlers.AddMethodProcessor(Type, Action{Microsoft.Extensions.DependencyInjection.IServiceCollection, Type, System.Reflection.MethodInfo})"/>
    ///     or
    ///     <see cref="SubscriptionHandlers.AddMethodProcessor{TAttribute}(Action{Microsoft.Extensions.DependencyInjection.IServiceCollection, Type, System.Reflection.MethodInfo})"/>.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SubscribesAttribute : Attribute { }
}
