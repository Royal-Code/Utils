namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// Handler para manipular um evento de diagnóstico.
    /// </summary>
    public interface IDiagnosticEventHandler
    {
        /// <summary>
        /// Nome do evento manipulado.
        /// </summary>
        string EventName { get; }

        /// <summary>
        /// Manipulador do evento.
        /// </summary>
        /// <param name="eventArgs">Argumentos do evento.</param>
        void Handle(object eventArgs);
    }
}
