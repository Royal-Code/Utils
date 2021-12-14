using System;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// <para>
    ///     Atributo para tipos (classes) adaptador dos argumentos de eventos de diagnóstico.
    /// </para>
    /// <para>
    ///     Usado pelo componente <see cref="EventArgumentGetterFactory"/> para adaptar um objeto de
    ///     eventos de diagnóstico para outro, onde será instanciado a classe decorada com este atributo
    ///     e atribuído as propriedades do objeto de evento para o objeto com este atributo.
    /// </para>
    /// <para>
    ///     Somente as propriedades decaradas com <see cref="GetFromArgumentAttribute"/> serão atribuídas.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ArgumentAdapterAttribute : Attribute
    {
        /// <summary>
        /// <para>
        ///     Vários nomes de tipos 'Assembly Qualified' que poderão existir como argumento de eventos de diagnóstico.
        /// </para>
        /// <para>
        ///     Opcional.
        /// </para>
        /// </summary>
        public string[]? GetFromTypes { get; set; }
    }
}
