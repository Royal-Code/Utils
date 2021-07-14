namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// Delegate para acessar uma propriedade de um objeto de argumentos de eventos de diagnóstico e obter
    /// um determinado argumento para manipulação do evento de diagnóstico.
    /// </summary>
    /// <typeparam name="TArgument">Tipo do argumento requirido.</typeparam>
    /// <param name="eventArgs">Objeto de argumentos do evento.</param>
    /// <returns>O argumento extraído de uma propriedade do objeto de argumentos.</returns>
    public delegate TArgument EventArgumentGetter<TArgument>(object eventArgs);
}
