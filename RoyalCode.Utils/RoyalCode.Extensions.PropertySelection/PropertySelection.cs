using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.Extensions.PropertySelection;

/// <summary>
/// <para>
///     Selection of a property.
/// </para>
/// <para>
///     It contains the selection of a property and the selection of the parent property,
///     and there may be a chaining of properties.
/// </para>
/// <para>
///     The responsibility of this class is to identify class properties using a string
///     containing the names of the properties to be selected separated by dots or trying to select by PascalCase.
/// </para>
/// <para>
///     With a property selected, you can take all the <see cref="PropertyInfo"/> of the selection,
///     generate a <see cref="Expression"/> to access the member,
///     check if it is possible to assign the value to the property at all its levels.
/// </para>
/// <para>
///     This functionality is useful for creating lambda expressions
///     where members are accessed and need to be found by property names.
/// </para>
/// <para>
///     See the facilitator <see cref="PropertySelectionExtensions.SelectProperty(Type, string)"/>.
/// </para>
/// </summary>
public class PropertySelection
{
    private readonly Type declarationType;
    private readonly PropertyInfo info;

    /// <summary>
    /// The current selected property type.
    /// </summary>
    public Type PropertyType => info.PropertyType;

    /// <summary>
    /// The declaring class type of the root selected property.
    /// if this selection does not have a parent, this selection will be the root.
    /// </summary>
    public Type RootDeclaringType => Parent != null ? Parent.RootDeclaringType : info.DeclaringType!;

    /// <summary>
    /// The parent <see cref="PropertySelection"/>. Can be null.
    /// </summary>
    public PropertySelection? Parent { get; private set; }

    /// <summary>
    /// The current selected property name.
    /// </summary>
    public string PropertyName => info.Name;

    /// <summary>
    /// The current selected property info.
    /// </summary>
    public PropertyInfo Info => info;

    /// <summary>
    /// <para>
    ///     Creates a new property selection from a <see cref="PropertyInfo"/>.
    /// </para>
    /// <para>
    ///     It is useful for creating a root selection and then selecting one or more child properties, forming a chain.
    /// </para>
    /// </summary>
    /// <param name="info">The <see cref="PropertyInfo"/>.</param>
    public PropertySelection(PropertyInfo info)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        Parent = null;
        declarationType = info.DeclaringType 
            ?? throw new InvalidOperationException("The property info does not hava a DeclaringType");
        this.info = info;
    }

    /// <summary>
    /// <para>
    ///     Factory method to create a property selection from property names.
    /// </para>
    /// <para>
    ///     PascalCase is considered for property selection.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>
    ///     Starting from a class called Affiliate and wanting to select the Id property of the affiliate's company,
    ///     where the affiliate has a property called Company,
    ///     of type Company, we can use the string: <code>CompanyId</code>.
    /// </para>
    /// <para>
    ///     The code in C# to access the property mentioned above would look like this:
    ///     <code>affiliate.Company.Id</code>.
    /// </para>
    /// </example>
    /// <param name="type">The class type containing the property.</param>
    /// <param name="property">The property selection, the names to search the properties, separating by dots and searching by PascalCase.</param>
    /// <param name="required">If the existence of the property is required. Optional, default true.</param>
    /// <returns>The selection of the property, or null if it is not required and not found.</returns>
    /// <exception cref="ArgumentException">
    ///     If the property cannot be selected and is required.
    /// </exception>
    public static PropertySelection? Select(Type type, string property, bool required = true)
    {
        PropertySelection? ps = null;

        if (property.Contains('.'))
            return Select(type, property.Split('.'), required);

        if (property.Contains('-'))
            return Select(type, property.Split('-'), required);

        var info = type.GetProperty(property);
        if (info is not null)
            return new PropertySelection(info);
        
        var parts = property.SplitUpperCase();
        if (parts is not null)
        {
            ps = SelectByParts(parts, type, 0, null);
        }

        if (ps is not null)
            return ps;

        if (required)
            throw CreateException(type.Name, property);

        return null;
    }

    private static PropertySelection? Select(Type type, string[] properties, bool required)
    {
        PropertySelection? ps = null;
        foreach (var property in properties)
        {
            ps = ps is null 
                ? Select(type, property, required) 
                : ps.SelectChild(property, required);

            if (ps is not null) 
                continue;
                
            if (required)
                throw CreateException(type.Name, property);
                
            return null;
        }

        return ps;
    }

    // Novo método estático recursivo que substitui PropertySelectionPart
    private static PropertySelection? SelectByParts(string[] parts, Type currentType, int position, PropertySelection? parent)
    {
        var currentProperty = string.Empty;
        for (int i = position; i < parts.Length; i++)
        {
            currentProperty += parts[i];
            var info = currentType.GetTypeInfo().PropertyLookup(currentProperty);
            if (info is not null)
            {
                var ps = parent == null ? new PropertySelection(info) : parent.SelectChild(info);
                if (i + 1 < parts.Length)
                {
                    var nextPs = SelectByParts(parts, info.PropertyType, i + 1, ps);
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

    private static ArgumentException CreateException(string typeName, string propertyName)
    {
        return new ArgumentException($"The class '{typeName}' does not have the property '{propertyName}'");
    }

    /// <summary>
    /// <para>
    ///     Creates a new <see cref="PropertySelection"/> from the current selection as a child selection.
    /// </para>
    /// <para>
    ///     This selection will be the parent of the new selection.
    /// </para>
    /// </summary>
    /// <param name="property">The property selection, the names to search the properties, separating by dots and searching by PascalCase.</param>
    /// <param name="required">If the existence of the property is required. Optional, default true.</param>
    /// <returns>
    ///     A new instance of <see cref="PropertySelection"/>
    ///     or null if the property does not exists and is not required.
    /// </returns>
    public PropertySelection? SelectChild(string property, bool required = true)
    {
        if (string.IsNullOrEmpty(property))
            throw new ArgumentNullException(nameof(property));

        var newSelection = Select(info.PropertyType, property, required);
        if (newSelection is null)
            return null;
        
        newSelection.SetParent(this);

        return newSelection;
    }

    private void SetParent(PropertySelection parent)
    {
        if (Parent is not null)
            Parent.SetParent(parent);
        else
            Parent = parent;
    }

    /// <summary>
    /// <para>
    ///     Creates a new <see cref="PropertySelection"/> from the current selection.
    /// </para>
    /// <para>
    ///     This selection will be the parent of the new selection.
    /// </para>
    /// </summary>
    /// <param name="property">
    ///     Some <see cref="PropertyInfo"/> of the current selected type (<see cref="PropertyType"/>).
    /// </param>
    /// <returns>A new instance of <see cref="PropertySelection"/>.</returns>
    /// <exception cref="ArgumentException">
    ///     If the <paramref name="property"/> declaring type are not equal to current select property type
    ///     (<see cref="PropertyType"/>).
    /// </exception>
    public PropertySelection SelectChild(PropertyInfo property)
    {
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        if (PropertyType != property.DeclaringType &&
            !property.DeclaringType.IsAssignableFrom(PropertyType))
            throw new ArgumentException("The property type is different from the current selection type. "
                + $"{declarationType.Name} was expected but was found {property.DeclaringType!.Name}");

        var newSelection = new PropertySelection(property)
        {
            Parent = this
        };

        return newSelection;
    }

    /// <summary>
    /// Builds an expression to access a property according to the selection path.
    /// </summary>
    /// <param name="expression">Expression for the start of access to property.</param>
    /// <returns>The expression of access to property.</returns>
    public MemberExpression GetAccessExpression(Expression expression)
    {
        var fromExpression = Parent?.GetAccessExpression(expression) ?? expression;

        if (!declarationType.IsAssignableFrom(fromExpression.Type))
        {
            throw new ArgumentException("The expression type is different from the property class type.\n"
                                        + $"The class type of property '{info.Name}' is '{declarationType.Name}', "
                                        + $"and expression class type is '{expression.Type.Name}'.");
        }

        return Expression.Property(fromExpression, info);
    }

    /// <summary>
    /// Array with the properties of the selection.
    /// </summary>
    /// <remarks>
    /// Parents come first in the array.
    /// </remarks>
    /// <returns>Array of properties, with at least one.</returns>
    public PropertyInfo[] PropertyPath()
    {
        var stack = new Stack<PropertyInfo>();
        CreatePropertyPath(stack);
        return stack.ToArray();
    }

    /// <summary>
    /// Determines whether the entire path can be assigned values.
    /// </summary>
    /// <returns>True if every path can have its members assigned.</returns>
    public bool CanSetValue()
    {
        return info.CanWrite && (Parent?.CanSetValue() ?? true);
    }

    /// <summary>
    /// Build the path of the properties.
    /// </summary>
    /// <returns>String representing the path separated by dots.</returns>
    public override string ToString()
        => Parent is null
            ? info.Name
            : $"{Parent}.{info.Name}";

    private void CreatePropertyPath(Stack<PropertyInfo> stack)
    {
        stack.Push(info);
        Parent?.CreatePropertyPath(stack);
    }
}