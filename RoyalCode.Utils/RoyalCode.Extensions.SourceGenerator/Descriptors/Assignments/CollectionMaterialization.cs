namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

/// <summary>
/// How an enumerable expression must be materialized to be assignable to the target property.
/// </summary>
public enum CollectionMaterialization
{
    /// <summary>
    /// No materialization required: the target accepts the enumerable as is (e.g. <c>IEnumerable&lt;T&gt;</c>).
    /// </summary>
    None,

    /// <summary>
    /// Requires <c>.ToList()</c> (e.g. <c>List&lt;T&gt;</c>, <c>IReadOnlyList&lt;T&gt;</c>, <c>ICollection&lt;T&gt;</c>).
    /// </summary>
    List,

    /// <summary>
    /// Requires <c>.ToArray()</c> (e.g. <c>T[]</c>).
    /// </summary>
    Array,

    /// <summary>
    /// Requires <c>.ToHashSet()</c> (e.g. <c>HashSet&lt;T&gt;</c>, <c>ISet&lt;T&gt;</c>).
    /// </summary>
    HashSet,
}
