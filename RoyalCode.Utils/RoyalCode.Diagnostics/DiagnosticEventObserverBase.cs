using System;
using System.Collections.Generic;
using System.Linq;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// <para>
    ///     Implementação abstrata de <see cref="IDiagnosticEventObserver"/>.
    /// </para>
    /// <para>
    ///     Implemente o método <see cref="CreateHandlers"/> para criar vários <see cref="IDiagnosticEventHandler"/>.
    /// </para>
    /// <para>
    ///     Cada handler trata um evento diferente.
    /// </para>
    /// <para>
    ///     Veja também <see cref="DiagnosticEventHandlers"/> e <see cref="DiagnosticEventHandler{TArgument}"/>
    ///     para criação facilitada dos handlers.
    /// </para>
    /// </summary>
    public abstract class DiagnosticEventObserverBase : IDiagnosticEventObserver
    {
        private readonly IEnumerable<IDiagnosticEventHandler> eventHandlers;
        private readonly string[] enabledOperationsNames;
        private readonly string[] ignoreOperationNames;

        /// <summary>
        /// Inicializa observador.
        /// </summary>
        protected DiagnosticEventObserverBase()
        {
            eventHandlers = CreateHandlers()?.ToArray()
                ?? throw new InvalidOperationException($"{nameof(CreateHandlers)} returns null");

            enabledOperationsNames = GetEnabledOperationsNames()?.ToArray()
                ?? throw new InvalidOperationException($"{nameof(GetEnabledOperationsNames)} returns null");

            ignoreOperationNames = GetIgnoreOperationNames()?.ToArray()
                ?? throw new InvalidOperationException($"{nameof(GetIgnoreOperationNames)} returns null");
        }

        /// <summary>
        /// <para>
        ///     Nome do listener e de eventos.
        /// </para>
        /// <para>
        ///     Um listener pode emitir vários eventos.
        /// </para>
        /// </summary>
        public abstract string DiagnosticListenerName { get; }

        /// <summary>
        /// <para>
        ///     Se está ativado a observações de um determinado evento.
        /// </para>
        /// <para>
        ///     Este método pode ser sobrescrito. Por padrão, se existir um handler para o evento, será verdadeiro,
        ///     caso contrário falso.
        /// </para>
        /// </summary>
        /// <param name="eventName">Nome do evento.</param>
        /// <returns>Verdadeiro se ativado, falso caso contrário.</returns>
        public virtual bool IsEnabled(string eventName)
        {
            if (ignoreOperationNames.Length != 0)
            {
                for (int i = 0; i < ignoreOperationNames.Length; i++)
                {
                    if (ignoreOperationNames[i] == eventName)
                        return false;
                }
            }

            if (enabledOperationsNames.Length == 0)
                return true;

            for (int i = 0; i < enabledOperationsNames.Length; i++)
            {
                if (enabledOperationsNames[i] == eventName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Nada.
        /// </summary>
        public void OnCompleted() { }

        /// <summary>
        /// Nada.
        /// </summary>
        /// <param name="error"></param>
        public void OnError(Exception error) { }

        /// <summary>
        /// Executa <see cref="OnNext(string, object)"/>.
        /// </summary>
        /// <param name="value">Valores do evento de diagnóstico ocorrido.</param>
        public virtual void OnNext(KeyValuePair<string, object?> value) => OnNext(value.Key, value.Value);

        /// <summary>
        /// Percorre os handlers e delega o evento ao qual o nome atenda.
        /// </summary>
        /// <param name="eventName">Nome do evento.</param>
        /// <param name="eventArgs">Argumentos do evento.</param>
        protected virtual void OnNext(string eventName, object? eventArgs)
        {
            if (eventArgs is null)
                return;
            foreach (var handler in eventHandlers)
            {
                if (handler.EventName == eventName)
                    handler.Handle(eventArgs);
            }
        }

        /// <summary>
        /// <para>
        ///     Cria os handlers de eventos deste observer.
        /// </para>
        /// <para>
        ///     Utilize <c>yield</c> junto com
        ///     <see cref="DiagnosticEventHandlers.For{TArgument}(string, Action{TArgument}, string)"/>.
        /// </para>
        /// <para>
        ///     Crie métodos na classe que herda <see cref="DiagnosticEventObserverBase"/> para serem
        ///     usados como delegate na criação do handler de eventos.
        /// </para>
        /// </summary>
        /// <returns>Uma coleção de handlers.</returns>
        protected abstract IEnumerable<IDiagnosticEventHandler> CreateHandlers();

        /// <summary>
        /// Nomes das operações (eventos) que estarão ativas.
        /// </summary>
        /// <returns>Uma coleção com os nomes das operações que devem ser observadas.</returns>
        protected virtual IEnumerable<string> GetEnabledOperationsNames() => Array.Empty<string>();

        /// <summary>
        /// Nomes das operações (eventos) que serão ignorados.
        /// </summary>
        /// <returns>Uma coleção com os nomes das operações que devem ser ignoradas.</returns>
        protected virtual IEnumerable<string> GetIgnoreOperationNames() => Array.Empty<string>();
    }
}
