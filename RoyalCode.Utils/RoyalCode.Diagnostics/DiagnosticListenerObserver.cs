using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// <para>
    ///     Um <see cref="IObserver{T}"/> de <see cref="DiagnosticListener"/>, registrado através do
    ///     <see cref="DiagnosticListener.AllListeners"/> para inscrever observadores de eventos de diagnóstisco.
    /// </para>
    /// <para>
    ///     Este componente utiliza todos os <see cref="IDiagnosticEventObserver"/> para registrá-los
    ///     como observadores de eventos.
    /// </para>
    /// <para>
    ///     Implemente e registre <see cref="IDiagnosticEventObserver"/> para escutar os eventos.
    /// </para>
    /// <para>
    ///     A inicialização é feita pelo <see cref="DiagnosticListenerObserverBackgroundProcessStartup"/>.
    /// </para>
    /// </summary>
    public class DiagnosticListenerObserver : IObserver<DiagnosticListener>
    {
        private readonly IEnumerable<IDiagnosticEventObserver> eventObservers;
        private readonly DiagnosticsOptions options;

        /// <summary>
        /// Cria novo observador.
        /// </summary>
        /// <param name="eventObservers">Observadores de eventos.</param>
        /// <param name="options">Opções de diagnóstico.</param>
        public DiagnosticListenerObserver(
            IEnumerable<IDiagnosticEventObserver> eventObservers,
            IOptions<DiagnosticsOptions> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.eventObservers = eventObservers ?? throw new ArgumentNullException(nameof(eventObservers));
            this.options = options.Value;
        }

        /// <summary>
        /// Não usado.
        /// </summary>
        public void OnCompleted() { }

        /// <summary>
        /// Não usado.
        /// </summary>
        /// <param name="error"></param>
        public void OnError(Exception error) { }

        /// <summary>
        /// Se o tracing estiver habilitado, os observadores de eventos (<see cref="IDiagnosticEventObserver"/>)
        /// serão registraos.
        /// </summary>
        /// <param name="listener">Um <see cref="DiagnosticListener"/>.</param>
        public void OnNext(DiagnosticListener listener)
        {
            if (options.Enabled)
                foreach (var observer in eventObservers)
                {
                    if (observer.DiagnosticListenerName.Equals(listener.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        listener.Subscribe(observer, observer.IsEnabled);
                    }
                }
        }
    }
}
