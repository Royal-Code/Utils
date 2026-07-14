using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

/// <summary>
/// <para>
///     A result of matching a property from the origin type to a property in the target type.
/// </para>
/// <para>
///     Holds Roslyn symbols and mutable state. Do not retain it in an incremental generator pipeline, and do
///     not use it as a cache key: it has no value equality. Convert the matching result with
///     <see cref="Snapshots.MatchSelectionSnapshotFactory.Create(MatchSelection)"/> and feed the pipeline with
///     the resulting snapshot instead.
/// </para>
/// </summary>
public class PropertyMatch(PropertyDescriptor origin, PropertySelection? target, AssignDescriptor? assignDescriptor)
{
    /// <summary>
    /// The origin property type descriptor. (DTO property)
    /// </summary>
    public PropertyDescriptor Origin { get; } = origin;

    /// <summary>
    /// The target property selection. (Entity property)
    /// </summary>
    public PropertySelection? Target { get; } = target;

    /// <summary>
    /// The assign descriptor that describes how to assign the origin property to the target property.
    /// </summary>
    public AssignDescriptor? AssignDescriptor { get; } = assignDescriptor;

    /// <summary>
    /// Determines if the target property selection is missing.
    /// </summary>
    public bool IsMissing => Target is null;

    /// <summary>
    /// Determine if the properties are compatible and has a valid assign descriptor.
    /// </summary>
    public bool CanAssign => AssignDescriptor is not null && !IsMissing;
}
