using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

/// <summary>
/// <para>
///     Describes how to assign a property, as resolved during matching. Mutable, and part of a tree that
///     holds Roslyn symbols (through <see cref="InnerSelection"/>).
/// </para>
/// <para>
///     Do not retain it in an incremental generator pipeline, and do not use it as a cache key: it has no
///     value equality. Convert the matching result with
///     <see cref="Snapshots.MatchSelectionSnapshotFactory.Create(PropertySelection.MatchSelection)"/> and
///     feed the pipeline with the resulting snapshot instead.
/// </para>
/// </summary>
public class AssignDescriptor
{
    public AssignType AssignType { get; set; }

    /// <summary>
    /// How the enumerable expression must be materialized for the target property.
    /// </summary>
    public CollectionMaterialization Materialization { get; set; }

    /// <summary>
    /// Whether the enumerable expression requires <c>.ToList()</c>.
    /// </summary>
    public bool RequireToList => Materialization == CollectionMaterialization.List;

    public MatchSelection? InnerSelection { get; set; }

    /// <summary>
    /// <para>
    ///     For <see cref="AssignType.Select"/>, how each element of the enumerable must be assigned.
    /// </para>
    /// <para>
    ///     When the elements are objects to be mapped, this is a <see cref="AssignType.NewInstance"/>
    ///     carrying the <see cref="InnerSelection"/>. When they only need a conversion
    ///     (e.g. <c>List&lt;int&gt;</c> to <c>List&lt;long&gt;</c>), this is the conversion of the element
    ///     (<see cref="AssignType.SimpleCast"/>, <see cref="AssignType.Direct"/>...) and there is no inner selection.
    /// </para>
    /// </summary>
    public AssignDescriptor? ElementAssignment { get; set; }
}
