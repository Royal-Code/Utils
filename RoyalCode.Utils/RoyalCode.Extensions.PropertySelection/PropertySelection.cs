using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.Extensions.PropertySelection
{
    /// <summary>
    /// Seleção de propriedades.
    /// Contém a seleção de uma propriedade e a seleção da propriedade pai.
    /// Util para o acesso de várias propriedades encadeadas.
    /// Veja o facilitador <see cref="PropertySelectionExtensions.SelectProperty(Type, string)"/>.
    /// </summary>
    public class PropertySelection
    {
        private readonly Type declarationType;

        private readonly PropertyInfo info;

        private Stack<string>? _addOns;

        /// <summary>
        /// Tipo da propriedade;
        /// </summary>
        public Type PropertyType => info.PropertyType;

        /// <summary>
        /// Tipo da propriedade raiz da seleção.
        /// </summary>
        public Type RootDeclaringType => Parent != null ? Parent.RootDeclaringType : info.DeclaringType;

        /// <summary>
        /// <see cref="PropertySelection"/> pai desta. Pode ser nulo.
        /// </summary>
        public PropertySelection? Parent { get; private set; }

        /// <summary>
        /// Nome da propriedade.
        /// </summary>
        public string PropertyName => info.Name;

        /// <summary>
        /// Se possui algum add-on, complemento.
        /// </summary>
        public bool HasAddOn => _addOns != null;

        /// <summary>
        /// Adicionais, complementos, da seleção da propriedade.
        /// </summary>
        public IEnumerable<string> AddOns => _addOns?.AsEnumerable() ?? Array.Empty<string>();

        /// <summary>
        /// Cria uma nova seleção para uma propriedade.
        /// </summary>
        /// <param name="info">Informações da propriedade.</param>
        public PropertySelection(PropertyInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            Parent = null;
            declarationType = info.DeclaringType;
            this.info = info;
        }

        /// <summary>
        /// Método para criar uma seleção de propriedade a partir da nomenclatura das propriedades.
        /// É considerado o PascalCase para seleção das propriedades.
        /// </summary>
        /// <example>
        /// <para>
        /// Tendo de partida uma classe chamada Filial e querendo selecionar a propriedade Id da empresa da filial,
        /// onde a filial possuí uma propriedade chamada Empresa, do tipo empresa, podemos usar a string:
        /// <code>EmpresaId</code>.
        /// </para>
        /// <para>
        /// O código em c# para acessar a propriedade mensionada acima seria assim:
        /// <code>filial.Empresa.Id</code>
        /// </para>
        /// </example>
        /// <param name="type">Tipo de dado que contém a propriedade.</param>
        /// <param name="property">Nome da propriedade, onde é procurado por pascal case.</param>
        /// <param name="required">Se a existência da propriedade é requerida. Opcional, padrão true</param>
        /// <returns>A seleção da propriedade, ou nulo se ela não é requerida e não for encontrada.</returns>
        /// <exception cref="ArgumentException">
        ///     Caso não seja possível selecionar a propriedade e ela seja requerida.
        /// </exception>
        public static PropertySelection? Select(Type type, string property, bool required = true)
        {
            var info = type.GetProperty(property);
            if (info != null)
                return new PropertySelection(info);

            PropertySelection? ps = null;
            var pascalCase = property.SplitPascalCase();
            if (pascalCase.Contains(' '))
            {
                var partSelector = new PropertySelectionPart(pascalCase.Split(' '), type);
                ps = partSelector.Select();
            }

            if (ps is null)
            {
                if (required)
                    throw CreateException(type.Name, property);
                else
                    return null;
            }

            return ps;
        }

        private static ArgumentException CreateException(string typeName, string propertyName)
        {
            return new ArgumentException($"The class '{typeName}' does not have the property '{propertyName}'");
        }

        /// <summary>
        /// Cria uma nova seleção de propriedade a partir da propriedade selecionada.
        /// A seleção atual será pai da nova seleção.
        /// </summary>
        /// <param name="property">Nome da propriedade.</param>
        /// <returns>Nova instância de PropertySelection.</returns>
        public PropertySelection Push(string property)
        {
            if (string.IsNullOrEmpty(property))
                throw new ArgumentNullException(nameof(property));

            var newSelection = Select(info.PropertyType, property)!;
            newSelection.Parent = this;
            newSelection._addOns = _addOns;

            return newSelection;
        }

        /// <summary>
        /// Cria uma nova seleção de propriedade a partir da propriedade selecionada.
        /// A seleção atual será pai da nova seleção.
        /// </summary>
        /// <param name="property">Nome da propriedade.</param>
        /// <returns>Nova instância de PropertySelection.</returns>
        public PropertySelection Push(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (property.DeclaringType != PropertyType)
                throw new ArgumentException("The property type is different from the current selection type. "
                    + $"{declarationType.Name} was expected but was found {property.DeclaringType.Name}");

            var newSelection = new PropertySelection(property)
            {
                Parent = this,
                _addOns = _addOns
            };

            return newSelection;
        }

        /// <summary>
        /// Adiciona um complemento.
        /// </summary>
        /// <param name="addOn">Complemento.</param>
        public void AddOn(string addOn)
        {
            if (_addOns == null)
                _addOns = new Stack<string>();

            _addOns.Push(addOn);
        }

        /// <summary>
        /// Monta uma expressão para acessar uma propriedade segundo o path da seleção.
        /// </summary>
        /// <param name="expression">Expressão de partida.</param>
        /// <returns>A expressão de acesso a propriedade.</returns>
        public MemberExpression GetAccessExpression(Expression expression)
        {
            var fromExpression = Parent != null
                ? Parent.GetAccessExpression(expression)
                : expression;

            if (!declarationType.IsAssignableFrom(fromExpression.Type))
            {
                throw new ArgumentException("The expression type is different from the property class type.\n"
                    + $"The class type of property '{info.Name}' is '{declarationType.Name}', " 
                    + $"and expression class type is '{expression.Type.Name}'.");
            }

            return Expression.Property(fromExpression, info);
        }

        /// <summary>
        /// Array com as propriedades da seleção.
        /// </summary>
        /// <remarks>
        /// Os pais vem por primeiro no array.
        /// </remarks>
        /// <returns>Array de propriedades, com ao menos uma.</returns>
        public PropertyInfo[] PropertyPath()
        {
            var stack = new Stack<PropertyInfo>();
            CreatePropertyPath(stack);
            return stack.ToArray();
        }

        /// <summary>
        /// Determina se todo o path pode ter valores atribuídos.
        /// </summary>
        /// <returns>Verdadeiro se todo path pode ter os membros atribuídos.</returns>
        public bool CanSetValue()
        {
            return info.CanWrite && (Parent?.CanSetValue() ?? true);
        }

        /// <summary>
        /// Monta o path das propriedades.
        /// </summary>
        /// <returns>String que representa o path separados por pontos.</returns>
        public override string ToString()
            => Parent == null
                ? info.Name
                : Parent.ToString() + "." + info.Name;

        private void CreatePropertyPath(Stack<PropertyInfo> stack)
        {
            stack.Push(info);
            if (Parent != null)
                Parent.CreatePropertyPath(stack);
        }
    }

    internal class PropertySelectionPart
    {
        private readonly string[] parts;
        private readonly Type currentType;
        private readonly int position;
        private readonly PropertySelection? parent;

        internal PropertySelectionPart(string[] parts, Type currentType, int position = 0, PropertySelection? parent = null)
        {
            this.parts = parts;
            this.currentType = currentType;
            this.position = position;
            this.parent = parent;
        }

        internal PropertySelection? Select()
        {
            var currentProperty = string.Empty;

            for (int i = position; i < parts.Length; i++)
            {
                currentProperty += parts[i];
                var info = currentType.GetTypeInfo().PropertyLookup(currentProperty);
                if (info is not null)
                {
                    var ps = parent == null ? new PropertySelection(info) : parent.Push(info);
                    if (i + 1 < parts.Length)
                    {
                        var nextPart = new PropertySelectionPart(parts, info.PropertyType, i + 1, ps);
                        var nextPs = nextPart.Select();
                        if (nextPs is not null)
                            return nextPs;
                    }
                    else
                    {
                        return ps;
                    }
                }
            }

            return null;
        }
    }
}
