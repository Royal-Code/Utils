using System;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// <para>
    ///     Implementação padrão de <see cref="IDiagnosticEventHandler"/>.
    /// </para>
    /// <para>
    ///     Para facilitar a criação deste objeto use 
    ///     <see cref="DiagnosticEventHandlers.For(string, Action)"/>.
    /// </para>
    /// </summary>
    public class DiagnosticEventHandler : IDiagnosticEventHandler
    {
        private readonly Action handle;

        /// <summary>
        /// Nome do evento manipulado.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Aplica a manipulação utilizando os delegates.
        /// </summary>
        /// <param name="eventArgs">Não usado.</param>
        public void Handle(object eventArgs) => handle();

        /// <summary>
        /// Cria novo manipulador com o nome do evento e o delegate de manipulação.
        /// </summary>
        /// <param name="eventName">O nome do evento.</param>
        /// <param name="handle">O delegate de manipulação.</param>
        public DiagnosticEventHandler(string eventName, Action handle)
        {
            this.handle = handle ?? throw new ArgumentNullException(nameof(handle));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        }
    }
}
