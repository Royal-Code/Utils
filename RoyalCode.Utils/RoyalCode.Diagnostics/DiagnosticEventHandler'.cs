using System;

namespace RoyalCode.Diagnostics
{

    /// <summary>
    /// <para>
    ///     Implementação padrão de <see cref="IDiagnosticEventHandler"/>.
    /// </para>
    /// <para>
    ///     Este componente irá obter um argumento específico das propriedades do objeto de argumentos do evento
    ///     de diagnóstico através de um delegate <see cref="EventArgumentGetter{TArgument}"/> e executar
    ///     uma delegate manipulador.
    /// </para>
    /// <para>
    ///     Para facilitar a criação deste objeto use 
    ///     <see cref="DiagnosticEventHandlers.For{TArgument}(string, Action{TArgument}, string)"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TArgument">Tipo do argumento requerido.</typeparam>
    public class DiagnosticEventHandler<TArgument> : IDiagnosticEventHandler
    {
        private readonly Action<TArgument> handle;
        private readonly string propertyName;
        private EventArgumentGetter<TArgument> getter;

        /// <summary>
        /// Nome do evento manipulado.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Aplica a manipulação utilizando os delegates.
        /// </summary>
        /// <param name="eventArgs">Objeto de argumentos do evento.</param>
        public void Handle(object eventArgs)
        {
            if (eventArgs is null)
                throw new ArgumentNullException(nameof(eventArgs));
            
            if (getter == null)
                getter = EventArgumentGetterFactory.Get<TArgument>(eventArgs.GetType(), propertyName);

            var argument = getter(eventArgs);
            if (argument != null)
                handle(argument);
        }

        /// <summary>
        /// Cria novo manipulador com o nome do evento e o delegate de manipulação.
        /// </summary>
        /// <param name="eventName">O nome do evento.</param>
        /// <param name="handle">O delegate de manipulação.</param>
        public DiagnosticEventHandler(string eventName, Action<TArgument> handle)
        {
            this.handle = handle ?? throw new ArgumentNullException(nameof(handle));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        }

        /// <summary>
        /// Cria novo manipulador com o nome do evento, o delegate de manipulação e nome da propriedade do argumento.
        /// </summary>
        /// <param name="eventName">O nome do evento.</param>
        /// <param name="handle">O delegate de manipulação.</param>
        /// <param name="propertyName">O nome da propriedade do argumento.</param>
        public DiagnosticEventHandler(string eventName, Action<TArgument> handle, string propertyName)
        {
            this.handle = handle ?? throw new ArgumentNullException(nameof(handle));
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        }
    }
}
