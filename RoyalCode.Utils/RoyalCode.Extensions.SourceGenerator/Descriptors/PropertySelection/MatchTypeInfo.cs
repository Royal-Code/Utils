﻿namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

/// <summary>
/// Represent a type and its properties that can be matched to another type.
/// </summary>
public ref struct MatchTypeInfo
{
    /// <summary>
    /// Creates a new instance of <see cref="MatchTypeInfo"/>.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="properties"></param>
    public MatchTypeInfo(TypeDescriptor type, IReadOnlyList<PropertyDescriptor> properties)
    {
        Type = type;
        Properties = properties;
    }

    /// <summary>
    /// The type descriptor of the current type.
    /// </summary>
    public TypeDescriptor Type { get; }

    /// <summary>
    /// The properties of the current type.
    /// </summary>
    public IReadOnlyList<PropertyDescriptor> Properties { get; }
}