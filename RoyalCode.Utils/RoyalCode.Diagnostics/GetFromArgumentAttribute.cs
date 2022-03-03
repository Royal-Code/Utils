using System;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// <para>
    ///     Atributo para propriedades de classes com <see cref="ArgumentAdapterAttribute"/>.
    /// </para>
    /// <para>
    ///     Somente as propriedades declaradas com este atributo serão atribuídas.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class GetFromArgumentAttribute : Attribute
    {
        /// <summary>
        /// <para>
        ///     Nome da propriedade, ou Path (CamelCase) para acessá-la.
        /// </para>
        /// <para>
        ///     Define de onde virá o valor que será atribuído a propriedade decorada com este atributo.
        /// </para>
        /// <para>
        ///     Este valor é opcional, quando não informado, 
        ///     será usado o mesmo nome da propriedade decorada com este atributo.
        /// </para>
        /// </summary>
        public string? PropertyName { get; set; }

        /// <summary>
        /// <para>
        ///     Se é propriedade é requerida, deve existir.
        /// </para>
        /// <para>
        ///     Quando for requerida e a propriedade não for encontrada, disparará uma exception.
        ///     Caso a propriedade não for requerida, somente não haverá atribuição da propriedade decorada com
        ///     este atributo.
        /// </para>
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// Cria novo atributo, sem informar a propriedade de onde virá o valor.
        /// </summary>
        public GetFromArgumentAttribute() { }

        /// <summary>
        /// Cria novo atributo, informando a propriedade de onde virá o valor.
        /// </summary>
        /// <param name="propertyName">Nome da propriedade, ou Path (CamelCase) para acessá-la.</param>
        public GetFromArgumentAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
