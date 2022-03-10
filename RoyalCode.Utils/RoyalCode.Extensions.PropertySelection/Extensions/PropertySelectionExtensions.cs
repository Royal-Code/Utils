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
        /// Gets a property selection from the property names separated by dot (.).
        /// </summary>
        /// <example>
        /// To get the customer's ZIP code from a sale you would use something like:
        /// typeof(Sale).SelectProperty("Customer.Address.ZipCode");
        /// </example>
        /// <param name="type">Data type containing the property.</param>
        /// <param name="propertyPath">Path of properties that need to be accessed.</param>
        /// <returns>The selection of properties.</returns>
        public static PropertySelection SelectProperty(this Type type, string propertyPath)
        {
            return PropertySelection.Select(type, propertyPath)!;
        }

        /// <summary>
        /// Creates a new selection of properties where properties of the source type
        /// serve to select the properties of the target type.
        /// </summary>
        /// <param name="origin">The source type.</param>
        /// <param name="target">The target type.</param>
        /// <returns>A new instance of <see cref="MatchSelection"/>.</returns>
        public static MatchSelection MatchProperties(this Type origin, Type target)
        {
            return new MatchSelection(origin, target);
        }

        /// <summary>
        /// 'For Each' function. Similar to the ForEach of the lists. 
        /// </summary>
        /// <typeparam name="T">Data type of the enumerate.</typeparam>
        /// <param name="enumerable">Enumerated.</param>
        /// <param name="action">Function.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Each<T>(this IEnumerable<T>? enumerable, Action<T> action)
        {
            if (enumerable is null)
                return;

            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
		/// Splits pascal case, so "FooBar" would become "Foo Bar".
		/// </summary>
        /// <param name="name">A string, thats represents a name of something, to be splited.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string SplitPascalCase(this string name)
        {
            return SplitUpperCase(name);
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
        /// Similar to <see cref="Type.GetProperty(string)"/>,
        /// but looking at the properties declared in the current type,
        /// and if not found, searches the higher (inherited) types.
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
