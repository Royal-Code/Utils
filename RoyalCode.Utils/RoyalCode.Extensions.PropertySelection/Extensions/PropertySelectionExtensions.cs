using RoyalCode.Extensions.PropertySelection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    /// <summary>
    /// Extensions methods used internaly by the <see cref="PropertySelection"/>.
    /// </summary>
    public static class PropertySelectionExtensions
    {
        /// <summary>
        /// Obtém uma seleção de propriedade a partir dos nomes das propriedades separadas por ponto (.).
        /// </summary>
        /// <example>
        /// Para obter o Cep do cliente de uma venda se usaria algo como:
        /// typeof(Venda).SelectProperty("Cliente.Endereco.Cep");
        /// </example>
        /// <param name="type">Tipo de dado que contém a propriedade.</param>
        /// <param name="propertyPath">Caminho de propriedades que precisam ser acessadas.</param>
        /// <returns>A seleção de propriedades</returns>
        public static PropertySelection SelectProperty(this Type type, string propertyPath)
        {
            string path = propertyPath;
            string[]? addOns = null;
            if (propertyPath.Contains(' '))
            {
                addOns = propertyPath.Split(' ');
                path = addOns[0];
            }

            var selections = path.Contains('.') ? path.Split('.') : new string[] { path };

            var ps = PropertySelection.Select(type, selections[0])!;
            for (int i = 1; i < selections.Length; i++)
            {
                ps = ps.SelectChild(selections[i]);
            }

            if (addOns is not null)
                addOns.Skip(1).Each(ps.AddOn);

            return ps;
        }

        /// <summary>
        /// Função 'Para Cada'. Semelhando ao ForEach das listas. 
        /// </summary>
        /// <typeparam name="T">Tipo de dado do enumerado.</typeparam>
        /// <param name="enumerable">Enumerado</param>
        /// <param name="action">Função.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable is null)
                return;

            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
		/// Splits pascal case, so "FooBar" would become "Foo Bar"
		/// </summary>
        /// <param name="name">A string, thats represents a name of something, to be splited.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string SplitPascalCase(this string name)
        {
            return SplitUpperCase(name, ' ', false);
        }

        /// <summary>
		/// Splits pascal case, so "FooBar" would become "Foo Bar" if separetor was ' ',
        /// and would become "Foo-Bar" if separetor was '-'. The separetor default is ' '.
		/// </summary>
        /// <param name="name">A string, thats represents a name of something, to be splited.</param>
        /// <param name="separetor">Caracter separador, por padrão é ' '.</param>
        /// <param name="lower">Se devem ser convertidos para minúsculo os caracteres, padrão falso.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string SplitUpperCase(this string name, char separetor = ' ', bool lower = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            var sb = new StringBuilder(name.Length + 5);

            for (int i = 0; i < name.Length; ++i)
            {
                var currentChar = name[i];
                if (char.IsUpper(currentChar) && i > 1
                    && (!char.IsUpper(name[i - 1]) || (i + 1 < name.Length && !char.IsUpper(name[i + 1]))))
                {
                    sb.Append(separetor);
                }
                if (lower)
                    currentChar = char.ToLowerInvariant(currentChar);
                sb.Append(currentChar);
            }

            return sb.ToString();
        }

        internal static IEnumerable<string[]> SplitPropertySelection(this string propertySelection)
        {
            return propertySelection.Split('.')
                .Select(part => part.SplitPascalCase().Split(' '));
        }

        /// <summary>
        /// Semelhante ao <see cref="Type.GetProperty(string)"/>, porém olhando as propriedades
        /// declaradas no tipo atual, e caso não encontrado, pesquisa nos tipos superiores (herdados).
        /// </summary>
        /// <param name="type">Tipo com propriedades.</param>
        /// <param name="name">Nome da propriedade pesquisada.</param>
        /// <returns>Uma <see cref="PropertyInfo"/> ou nulo.</returns>
        public static PropertyInfo? PropertyLookup(this TypeInfo type, string name)
        {
            return type.DeclaredProperties.FirstOrDefault(p => p.Name == name)
                ?? (type.BaseType != typeof(object) ? type.BaseType.GetTypeInfo().PropertyLookup(name) : null);
        }
    }
}
