namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

/// <summary>
/// Represent a type and its properties that can be matched to another type.
/// </summary>
public ref struct MatchTypeInfo
{
    public static MatchTypeInfo Create(
        TypeDescriptor type, MatchOptions options)
    {
        var properties = options.TargetPropertiesRetriever.GetProperties(type);
        return new MatchTypeInfo(type, properties, options);
    }

    /// <summary>
    /// Creates a new instance of <see cref="MatchTypeInfo"/>.
    /// </summary>
    /// <param name="type">The type descriptor.</param>
    /// <param name="properties">The properties of the type.</param>
    /// <param name="options">The match options used to retrieve the properties.</param>
    public MatchTypeInfo(
        TypeDescriptor type, 
        IReadOnlyList<PropertyDescriptor> properties,
        MatchOptions options)
    {
        Type = type;
        Properties = properties;
        Options = options;
    }

    /// <summary>
    /// The type descriptor of the current type.
    /// </summary>
    public TypeDescriptor Type { get; }

    /// <summary>
    /// The properties of the current type.
    /// </summary>
    public IReadOnlyList<PropertyDescriptor> Properties { get; }

    public MatchOptions Options { get; }
}