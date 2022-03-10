using System.Reflection;

namespace RoyalCode.Extensions.PropertySelection;

/// <summary>
/// This class contains the result of selecting properties of a class from another class.
/// </summary>
public class MatchSelection
{
    /// <summary>
    /// Creates a new selection of properties where properties of the source type
    /// serve to select the properties of the target type.
    /// </summary>
    /// <param name="originType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    public MatchSelection(Type originType, Type targetType)
    {
        OriginType = originType;
        TargetType = targetType;
        PropertyMatches = ProcessPropertiesMatch();
    }

    /// <summary>
    /// The source type.
    /// </summary>
    public Type OriginType { get; }

    /// <summary>
    /// The target type.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// A collection of results. See more <see cref="PropertyMatch"/>.
    /// </summary>
    public IEnumerable<PropertyMatch> PropertyMatches { get; }

    /// <summary>
    /// Ensures that all properties of the source type have been selected in the target type,
    /// otherwise an exception will be thrown.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     When an property was not selected.
    /// </exception>
    public void EnsureAllMatched()
    {
        if (!PropertyMatches.All(m => m.Match))
            throw new InvalidOperationException(string.Join(". ",
                PropertyMatches.Where(m => !m.Match).Select(m => m.GetNoMatchMessage())));
    }

    /// <summary>
    /// Process properties selection.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<PropertyMatch> ProcessPropertiesMatch()
    {
        var properties = OriginType.GetTypeInfo()
            .GetRuntimeProperties()
            .Where(p => p.CanRead);

        LinkedList<PropertyMatch> result = new();
        foreach (var property in properties)
        {
            var selection = PropertySelection.Select(TargetType, property.Name, false);
            result.AddLast(new PropertyMatch(property, TargetType, selection));
        }

        return result;
    }
}