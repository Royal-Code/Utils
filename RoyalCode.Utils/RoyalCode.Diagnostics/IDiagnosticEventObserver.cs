using System;
using System.Collections.Generic;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// <para>
    ///     Observador de eventos de diagnóstico.
    /// </para>
    /// <para>
    ///     Este componente é utilizado pelo <see cref="DiagnosticListenerObserver"/>.
    /// </para>
    /// <para>
    ///     Veja também a implementação abstrata <see cref="DiagnosticEventObserverBase"/>.
    /// </para>
    /// </summary>
    public interface IDiagnosticEventObserver : IObserver<KeyValuePair<string, object?>>
    {
        /// <summary>
        /// <para>
        ///     Nome do listener e de eventos.
        /// </para>
        /// <para>
        ///     Um listener pode emitir vários eventos.
        /// </para>
        /// </summary>
        string DiagnosticListenerName { get; }

        /// <summary>
        /// Se está ativado a observações de um determinado evento.
        /// </summary>
        /// <param name="eventName">Nome do evento.</param>
        /// <returns>Verdadeiro se ativado, falso caso contrário.</returns>
        bool IsEnabled(string eventName);
    }
}
