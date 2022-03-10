using System.Reflection;

namespace RoyalCode.Extensions.PropertySelection;

/// <summary>
/// Component that brings the result of the selection of a property of a class
/// from a property of another class.
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
    /// If the selection success.
    /// </summary>
    public bool Match => TargetSelection is not null;

    internal string GetNoMatchMessage()
    {
        return $"The property '{OriginProperty.Name}' of type '{OriginProperty.DeclaringType!.Name}' has no-match to a property of type '{targetType.Name}'.";
    }
}