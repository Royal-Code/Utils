using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

public class AssignDescriptor : IEquatable<AssignDescriptor>
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

    public bool Equals(AssignDescriptor other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return AssignType == other.AssignType &&
            Materialization == other.Materialization &&
            Equals(InnerSelection, other.InnerSelection);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AssignDescriptor other)
            return false;

        return Equals(other);
    }

    public override int GetHashCode()
    {
        int hashCode = -2066519001;
        hashCode = hashCode * -1521134295 + AssignType.GetHashCode();
        hashCode = hashCode * -1521134295 + Materialization.GetHashCode();
        hashCode = hashCode * -1521134295 + (InnerSelection?.GetHashCode() ?? 0);
        return hashCode;
    }
}
