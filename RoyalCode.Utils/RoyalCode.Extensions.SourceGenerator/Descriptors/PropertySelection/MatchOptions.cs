using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

/// <summary>
/// Provides configuration options for controlling how property values are retrieved and compared during a match
/// operation.
/// </summary>
public class MatchOptions
{
    /// <summary>
    /// Internal default instance of <see cref="MatchOptions"/>.
    /// </summary>
    internal static MatchOptions InternalDefault { get; } = new();

    /// <summary>
    /// Creates a new instance of <see cref="MatchOptions"/> with default settings.
    /// </summary>
    public static MatchOptions Default => new();

    /// <summary>
    /// Default method to retrieve properties from the origin type.
    /// Util when overriding the default retriever is not necessary.
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    public static IReadOnlyList<PropertyDescriptor> GetOriginProperties(TypeDescriptor origin)
        => InternalDefault.OriginPropertiesRetriever.GetProperties(origin);

    /// <summary>
    /// Default method to retrieve properties from the target type.
    /// Util when overriding the default retriever is not necessary.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static IReadOnlyList<PropertyDescriptor> GetTargetProperties(TypeDescriptor target)
        => InternalDefault.TargetPropertiesRetriever.GetProperties(target);

    /// <summary>
    /// The retriever used to obtain properties from the origin type.
    /// </summary>
    public IOriginPropertiesRetriever OriginPropertiesRetriever { get; set; } = new DefaultOriginPropertiesRetriever();

    /// <summary>
    /// The retriever used to obtain properties from the target type.
    /// </summary>
    public ITargetPropertiesRetriever TargetPropertiesRetriever { get; set; } = new DefaultTargetPropertiesRetriever();

    /// <summary>
    /// Additional property name resolvers to be used during property matching.
    /// </summary>
    public IPropertyNameResolver[]? PropertyNameResolvers { get; set; }

    /// <summary>
    /// Gets or sets the collection of additional assign descriptor resolvers to be used during assignment operations.
    /// <br />
    /// Optional; if not set, only the default resolvers will be used.
    /// </summary>
    /// <remarks>
    ///     Use this property to specify custom resolvers that extend or override the default assignment behavior.
    ///     The order of resolvers in the array may affect resolution precedence.
    /// </remarks>
    public IAssignDescriptorResolver[]? AdditionalAssignDescriptorResolvers { get; set; }
}
