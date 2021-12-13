using System;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// Classe estática para criação de <see cref="IDiagnosticEventHandler"/>.
    /// </summary>
    public static class DiagnosticEventHandlers
    {
        /// <summary>
        /// <para>
        ///     Cria um novo <see cref="IDiagnosticEventHandler"/> 
        ///     a partir do tipo <see cref="DiagnosticEventHandler"/>.
        /// </para>
        /// <para>
        ///     Para manipular um evento é utilizado um delegate, que já receberá o tipo de dado desejado.
        /// </para>
        /// </summary>
        /// <param name="eventName">Nome do evento.</param>
        /// <param name="handler">Delegate manipulador de eventos.</param>
        /// <returns>Uma nova instância de <see cref="DiagnosticEventHandler{TArgument}"/>.</returns>
        public static DiagnosticEventHandler For(string eventName,Action handler)
        {
            return new DiagnosticEventHandler(eventName, handler);
        }

        /// <summary>
        /// <para>
        ///     Cria um novo <see cref="IDiagnosticEventHandler"/> 
        ///     a partir do tipo <see cref="DiagnosticEventHandler{TArgument}"/>.
        /// </para>
        /// <para>
        ///     É informado um tipo de argumento (<typeparamref name="TArgument"/>) 
        ///     que será obtido de uma das propriedades do objeto de argumentos do
        ///     evento. Ainda, é possível específicar o nome da propriedade.
        /// </para>
        /// <para>
        ///     Para manipular um evento é utilizado um delegate, que já receberá o tipo de dado desejado.
        /// </para>
        /// </summary>
        /// <typeparam name="TArgument">Tipo do argumento.</typeparam>
        /// <param name="eventName">Nome do evento.</param>
        /// <param name="handler">Delegate manipulador de eventos.</param>
        /// <param name="propertyName">Nome da propriedade para extração do argumento, opcional.</param>
        /// <returns>Uma nova instância de <see cref="DiagnosticEventHandler{TArgument}"/>.</returns>
        public static DiagnosticEventHandler<TArgument> For<TArgument>(
            string eventName,
            Action<TArgument> handler, 
            string? propertyName = null)
        {
            return propertyName == null
                ? new DiagnosticEventHandler<TArgument>(eventName, handler)
                : new DiagnosticEventHandler<TArgument>(eventName, handler, propertyName);
        }

        /// <summary>
        /// <para>
        ///     Cria um novo <see cref="IDiagnosticEventHandler"/> 
        ///     a partir do tipo <see cref="DiagnosticEventHandler{TArgument1, TArgument2}"/>.
        /// </para>
        /// <para>
        ///     São informados os tipos de argumentos (<typeparamref name="TArgument1"/> e <typeparamref name="TArgument2"/>) 
        ///     que serão obtidos das propriedades do objeto de argumentos do
        ///     evento. Ainda, é possível específicar os nomes das propriedades.
        /// </para>
        /// <para>
        ///     Para manipular um evento é utilizado um delegate, que já receberá o tipo de dado desejado.
        /// </para>
        /// </summary>
        /// <typeparam name="TArgument1">Tipo do argumento 1.</typeparam>
        /// <typeparam name="TArgument2">Tipo do argumento 2.</typeparam>
        /// <param name="eventName">Nome do evento.</param>
        /// <param name="handler">Delegate manipulador de eventos.</param>
        /// <param name="propertyName1">Nome da propriedade para extração do argumento 1, opcional.</param>
        /// <param name="propertyName2">Nome da propriedade para extração do argumento 2, opcional.</param>
        /// <returns>Uma nova instância de <see cref="DiagnosticEventHandler{TArgument1, TArgument2}"/>.</returns>
        public static DiagnosticEventHandler<TArgument1, TArgument2> For<TArgument1, TArgument2>(
            string eventName,
            Action<TArgument1, TArgument2> handler,
            string? propertyName1 = null,
            string? propertyName2 = null)
        {
            return new DiagnosticEventHandler<TArgument1, TArgument2>(eventName, handler, propertyName1, propertyName2);
        }

        /// <summary>
        /// <para>
        ///     Cria um novo <see cref="IDiagnosticEventHandler"/> 
        ///     a partir do tipo <see cref="DiagnosticEventHandler{TArgument1, TArgument2, TArgument3}"/>.
        /// </para>
        /// <para>
        ///     São informados os tipos de argumentos 
        ///     (<typeparamref name="TArgument1"/>, <typeparamref name="TArgument2"/> e <typeparamref name="TArgument3"/>) 
        ///     que serão obtidos das propriedades do objeto de argumentos do
        ///     evento. Ainda, é possível específicar os nomes das propriedades.
        /// </para>
        /// <para>
        ///     Para manipular um evento é utilizado um delegate, que já receberá o tipo de dado desejado.
        /// </para>
        /// </summary>
        /// <typeparam name="TArgument1">Tipo do argumento 1.</typeparam>
        /// <typeparam name="TArgument2">Tipo do argumento 2.</typeparam>
        /// <typeparam name="TArgument3">Tipo do argumento 3.</typeparam>
        /// <param name="eventName">Nome do evento.</param>
        /// <param name="handler">Delegate manipulador de eventos.</param>
        /// <param name="propertyName1">Nome da propriedade para extração do argumento 1, opcional.</param>
        /// <param name="propertyName2">Nome da propriedade para extração do argumento 2, opcional.</param>
        /// <param name="propertyName3">Nome da propriedade para extração do argumento 3, opcional.</param>
        /// <returns>Uma nova instância de <see cref="DiagnosticEventHandler{TArgument1, TArgument2}"/>.</returns>
        public static DiagnosticEventHandler<TArgument1, TArgument2, TArgument3> For<TArgument1, TArgument2, TArgument3>(
            string eventName,
            Action<TArgument1, TArgument2, TArgument3> handler,
            string? propertyName1 = null,
            string? propertyName2 = null,
            string? propertyName3 = null)
        {
            return new DiagnosticEventHandler<TArgument1, TArgument2, TArgument3>(
                eventName, handler, propertyName1, propertyName2, propertyName3);
        }
    }
}
