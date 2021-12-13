using System;

namespace RoyalCode.DependencyInjection.Subscribers
{
    /// <summary>
    /// An generic base interface for subscritions handlers.
    /// </summary>
    public interface ISubscriptionHandler
    {
        /// <summary>
        /// Check if the handlers accepts the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True if it can be proccessed, false otherwise.</returns>
        bool Accept(Type type);
    }
}
