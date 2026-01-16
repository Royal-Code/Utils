using Microsoft.CodeAnalysis;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

/// <summary>
/// Resolves property names for given property symbols.
/// </summary>
public interface IPropertyNameResolver
{
    /// <summary>
    /// Tries to resolve the property name for the given property symbol.
    /// </summary>
    /// <param name="symbol">The property symbol to resolve the name for.</param>
    /// <param name="propertyName">The resolved property name, if successful; otherwise, null.</param>
    /// <returns>True if the property name was successfully resolved; otherwise, false.</returns>
    public bool TryResolvePropertyName(IPropertySymbol symbol, out string? propertyName);
}
