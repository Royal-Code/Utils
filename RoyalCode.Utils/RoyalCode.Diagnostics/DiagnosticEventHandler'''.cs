using System;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// <para>
    ///     Implementação padrão de <see cref="IDiagnosticEventHandler"/>.
    /// </para>
    /// <para>
    ///     Este componente irá obter três argumentos específicos das propriedades do objeto de argumentos do evento
    ///     de diagnóstico através de um delegate <see cref="EventArgumentGetter{TArgument}"/> e executar
    ///     uma delegate manipulador.
    /// </para>
    /// <para>
    ///     Para facilitar a criação deste objeto use 
    ///     <see cref="DiagnosticEventHandlers.For{TArgument1, TArgument2, TArgument3}(string, Action{TArgument1, TArgument2, TArgument3}, string, string, string)"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TArgument1">Tipo do argumento 1 requerido.</typeparam>
    /// <typeparam name="TArgument2">Tipo do argumento 2 requerido.</typeparam>
    /// <typeparam name="TArgument3">Tipo do argumento 3 requerido.</typeparam>
    public class DiagnosticEventHandler<TArgument1, TArgument2, TArgument3> : IDiagnosticEventHandler
    {
        private readonly Action<TArgument1, TArgument2, TArgument3> handle;
        private readonly string?[] propertyNames;
        private EventArgumentGetter<TArgument1>? getter1;
        private EventArgumentGetter<TArgument2>? getter2;
        private EventArgumentGetter<TArgument3>? getter3;

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

            if (getter1 == null)
                getter1 = EventArgumentGetterFactory.Get<TArgument1>(eventArgs.GetType(), propertyNames[0]);

            if (getter2 == null)
                getter2 = EventArgumentGetterFactory.Get<TArgument2>(eventArgs.GetType(), propertyNames[1]);

            if (getter3 == null)
                getter3 = EventArgumentGetterFactory.Get<TArgument3>(eventArgs.GetType(), propertyNames[2]);

            var argument1 = getter1(eventArgs);
            var argument2 = getter2(eventArgs);
            var argument3 = getter3(eventArgs);
            if (argument1 != null || argument2 != null || argument3 != null)
                handle(argument1, argument2, argument3);
        }

        /// <summary>
        /// Cria novo manipulador com o nome do evento e o delegate de manipulação.
        /// </summary>
        /// <param name="eventName">O nome do evento.</param>
        /// <param name="handle">O delegate de manipulação.</param>
        public DiagnosticEventHandler(string eventName, Action<TArgument1, TArgument2, TArgument3> handle)
        {
            this.handle = handle ?? throw new ArgumentNullException(nameof(handle));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            propertyNames = new string[3];
        }

        /// <summary>
        /// Cria novo manipulador com o nome do evento, o delegate de manipulação e nome da propriedade do argumento.
        /// </summary>
        /// <param name="eventName">O nome do evento.</param>
        /// <param name="handle">O delegate de manipulação.</param>
        /// <param name="propertyName1">O nome da propriedade do argumento 1.</param>
        /// <param name="propertyName2">O nome da propriedade do argumento 2.</param>
        /// <param name="propertyName3">O nome da propriedade do argumento 3.</param>
        public DiagnosticEventHandler(string eventName, Action<TArgument1, TArgument2, TArgument3> handle,
            string? propertyName1, string? propertyName2, string? propertyName3)
        {
            this.handle = handle ?? throw new ArgumentNullException(nameof(handle));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            propertyNames = new string?[] { propertyName1, propertyName2, propertyName3 };
        }
    }
}
