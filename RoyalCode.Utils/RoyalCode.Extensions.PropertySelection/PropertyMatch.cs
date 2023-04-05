using System.Reflection;

namespace RoyalCode.Extensions.PropertySelection;

/// <summary>
/// <para>
///     Component that brings the result of the selection of a property of a class
///     from a property of another class.
/// </para>
/// </summary>
public class PropertyMatch
{
    private readonly Type targetType;

    /// <summary>
    /// Creates a new result of the selection.
    /// </summary>
    /// <param name="originProperty">The origin property.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="targetSelection">The target selection, if it was selected, or null if the selection failed.</param>
    public PropertyMatch(
        PropertyInfo originProperty,
        Type targetType,
        PropertySelection? targetSelection)
    {
        this.targetType = targetType;
        OriginProperty = originProperty;
        TargetSelection = targetSelection;
    }

    /// <summary>
    /// The origin property.
    /// </summary>
    public PropertyInfo OriginProperty { get; }

    /// <summary>
    /// The target selection, if it was selected, or null if the selection failed.
    /// </summary>
    public PropertySelection? TargetSelection { get; }

    /// <summary>
    /// <para>
    ///     If the selection is successful, check that the selected property matches the source property type.
    /// </para>
    /// <para>
    ///     if the selection failed, the value will be 'false'.
    /// </para>
    /// </summary>
    public bool TypeMatch => TargetSelection?.PropertyType.IsAssignableFrom(OriginProperty.PropertyType) ?? false;

    /// <summary>
    /// <para>
    ///     If the selection is successful, check that the source property type matches the selected property type.
    /// </para>
    /// <para>
    ///     if the selection failed, the value will be 'false'.
    /// </para>
    /// </summary>
    public bool InvetedTypeMatch => TargetSelection is not null && 
        OriginProperty.PropertyType.IsAssignableFrom(TargetSelection.PropertyType);

    /// <summary>
    /// If the selection is successful.
    /// </summary>
    public bool Match => TargetSelection is not null;

    internal string GetNoMatchMessage()
    {
        return $"The property '{OriginProperty.Name}' of type '{OriginProperty.DeclaringType!.Name}' has no-match to a property of type '{targetType.Name}'.";
    }
}