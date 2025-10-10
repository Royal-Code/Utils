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
    /// Initializes a new instance of the MatchTypeInfo class for the specified target type descriptor.
    /// </summary>
    /// <remarks>
    ///     The constructed instance uses the default match options and retrieves the target type's
    ///     properties using the default property retriever.
    ///     This ensures consistent matching behavior across instances.
    /// </remarks>
    /// <param name="targetType">
    ///     The type descriptor representing the target type for which matching information will be constructed.
    ///     Cannot be null.
    /// </param>
    public MatchTypeInfo(TypeDescriptor targetType)
    {
        Type = targetType;
        Properties = MatchOptions.Default.TargetPropertiesRetriever.GetProperties(targetType);
        Options = MatchOptions.Default;
    }

    /// <summary>
    /// Initializes a new instance of the MatchTypeInfo class with the specified type descriptor and property
    /// descriptors.
    /// </summary>
    /// <param name="type">The type descriptor representing the type to be matched.</param>
    /// <param name="properties">A read-only list of property descriptors that define the properties associated with the type.</param>
    public MatchTypeInfo(TypeDescriptor type, IReadOnlyList<PropertyDescriptor> properties)
    {
        Type = type;
        Properties = properties;
        Options = MatchOptions.Default;
    }

    /// <summary>
    /// Initializes a new instance of the MatchTypeInfo class using the specified type descriptor and matching options.
    /// </summary>
    /// <param name="targetType">The type descriptor representing the target type for which property matching information will be generated.
    /// Cannot be null.</param>
    /// <param name="options">The options that configure property matching behavior, including how target properties are retrieved. Cannot be
    /// null.</param>
    public MatchTypeInfo(TypeDescriptor targetType, MatchOptions options)
    {
        Type = targetType;
        Properties = options.TargetPropertiesRetriever.GetProperties(targetType);
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