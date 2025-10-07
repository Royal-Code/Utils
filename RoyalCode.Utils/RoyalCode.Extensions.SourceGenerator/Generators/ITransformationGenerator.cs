using Microsoft.CodeAnalysis;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

/// <summary>
/// <para>
///     Defines a contract for generating transformations within a source production context.
/// </para>
/// </summary>
/// <remarks>
///     Implementations of this interface are responsible for producing transformations or outputs based on
///     the provided <see cref="SourceProductionContext"/>.
///     This is typically used in source generators to contribute additional code or artifacts during compilation.
/// </remarks>
public interface ITransformationGenerator
{
    public void Generate(SourceProductionContext spc);
}

/// <summary>
/// <para>
///     Defines a contract for generating transformations within a source production context.
/// </para>
/// </summary>
/// <remarks>
///     Implementations of this interface are responsible for producing transformations or outputs based on
///     the provided <see cref="SourceProductionContext"/>.
///     This is typically used in source generators to contribute additional code or artifacts during compilation.
/// </remarks>
/// <typeparam name="TModel">Type of models used to generate sources </typeparam>
public interface ITransformationGenerator<in TModel>
{
    public void Generate(SourceProductionContext spc, IEnumerable<TModel> models);
}