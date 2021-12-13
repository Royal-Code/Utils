using System;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// <para>
    ///     Implementação padrão de <see cref="IDiagnosticEventHandler"/>.
    /// </para>
    /// <para>
    ///     Este componente irá obter dois argumentos específicos das propriedades do objeto de argumentos do evento
    ///     de diagnóstico através de um delegate <see cref="EventArgumentGetter{TArgument}"/> e executar
    ///     uma delegate manipulador.
    /// </para>
    /// <para>
    ///     Para facilitar a criação deste objeto use 
    ///     <see cref="DiagnosticEventHandlers.For{TArgument1, TArgument2}(string, Action{TArgument1, TArgument2}, string, string)"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TArgument1">Tipo do argumento 1 requerido.</typeparam>
    /// <typeparam name="TArgument2">Tipo do argumento 2 requerido.</typeparam>
    public class DiagnosticEventHandler<TArgument1, TArgument2> : IDiagnosticEventHandler
    {
        private readonly Action<TArgument1, TArgument2> handle;
        private readonly string?[] propertyNames;
        private EventArgumentGetter<TArgument1>? getter1;
        private EventArgumentGetter<TArgument2>? getter2;

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

            if (getter1 is null)
                getter1 = EventArgumentGetterFactory.Get<TArgument1>(eventArgs.GetType(), propertyNames[0]);

            if (getter2 is null)
                getter2 = EventArgumentGetterFactory.Get<TArgument2>(eventArgs.GetType(), propertyNames[1]);

            var argument1 = getter1(eventArgs);
            var argument2 = getter2(eventArgs);
            if (argument1 != null || argument2 != null)
                handle(argument1, argument2);
        }

        /// <summary>
        /// Cria novo manipulador com o nome do evento e o delegate de manipulação.
        /// </summary>
        /// <param name="eventName">O nome do evento.</param>
        /// <param name="handle">O delegate de manipulação.</param>
        public DiagnosticEventHandler(string eventName, Action<TArgument1, TArgument2> handle)
        {
            this.handle = handle ?? throw new ArgumentNullException(nameof(handle));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            propertyNames = new string[2];
        }

        /// <summary>
        /// Cria novo manipulador com o nome do evento, o delegate de manipulação e nome da propriedade do argumento.
        /// </summary>
        /// <param name="eventName">O nome do evento.</param>
        /// <param name="handle">O delegate de manipulação.</param>
        /// <param name="propertyName1">O nome da propriedade do argumento 1.</param>
        /// <param name="propertyName2">O nome da propriedade do argumento 2.</param>
        public DiagnosticEventHandler(string eventName, Action<TArgument1, TArgument2> handle,
            string? propertyName1, string? propertyName2)
        {
            this.handle = handle ?? throw new ArgumentNullException(nameof(handle));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            propertyNames = new string?[] { propertyName1, propertyName2 };
        }
    }
}
