using RoyalCode.Extensions.SourceGenerator.Collections;
using RoyalCode.Extensions.SourceGenerator.Diagnostics;

namespace RoyalCode.Extensions.SourceGenerator.Generation;

/// <summary>
/// <para>
///     A value-equatable transport for the result of a transform: an optional model plus the diagnostics
///     produced while analysing the input. It keeps invalid input from reaching emitters or aggregations.
/// </para>
/// <para>
///     When <see cref="IsValid"/> is <see langword="false"/> there is no <see cref="Model"/> to emit. Diagnostics
///     may explain a semantic rejection, but an empty collection is also valid for input that is incomplete or
///     not applicable and should be ignored without noise. This lets a single semantic read validate and collect
///     diagnostics without a second analysis pass and without letting a broken model flow downstream.
/// </para>
/// </summary>
/// <typeparam name="TModel">The immutable, value-equatable pipeline model.</typeparam>
public readonly struct GenerationCandidate<TModel> : IEquatable<GenerationCandidate<TModel>>
    where TModel : class, IEquatable<TModel>
{
    private GenerationCandidate(TModel? model, EquatableArray<DiagnosticInfo> diagnostics)
    {
        Model = model;
        Diagnostics = diagnostics;
    }

    /// <summary>The model to emit, or <see langword="null"/> when the input was invalid.</summary>
    public TModel? Model { get; }

    /// <summary>The diagnostics gathered during analysis; may be present even for a valid model (warnings).</summary>
    public EquatableArray<DiagnosticInfo> Diagnostics { get; }

    /// <summary>Whether a model is present and may drive generation.</summary>
    public bool IsValid => Model is not null;

    /// <summary>Creates a valid candidate carrying a model and optional diagnostics.</summary>
    /// <param name="model">The model that may flow to emitters.</param>
    /// <param name="diagnostics">Optional warnings or informational diagnostics associated with the model.</param>
    /// <returns>A valid generation candidate.</returns>
    public static GenerationCandidate<TModel> Valid(TModel model, EquatableArray<DiagnosticInfo> diagnostics = default)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model));
        return new GenerationCandidate<TModel>(model, diagnostics);
    }

    /// <summary>
    /// Creates a candidate with no model. Diagnostics may explain a rejection; an empty collection represents
    /// incomplete or non-applicable input that should not emit a model or diagnostic.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to report, or an empty collection for a silent rejection.</param>
    /// <returns>An invalid generation candidate.</returns>
    public static GenerationCandidate<TModel> Invalid(EquatableArray<DiagnosticInfo> diagnostics) =>
        new(null, diagnostics);

    /// <summary>Creates an invalid candidate from a single diagnostic.</summary>
    /// <param name="diagnostic">The non-null diagnostic that explains the rejection.</param>
    /// <returns>An invalid generation candidate.</returns>
    public static GenerationCandidate<TModel> Invalid(DiagnosticInfo diagnostic)
    {
        if (diagnostic is null)
            throw new ArgumentNullException(nameof(diagnostic));
        return new GenerationCandidate<TModel>(null, new EquatableArray<DiagnosticInfo>(new[] { diagnostic }));
    }

    /// <inheritdoc />
    public bool Equals(GenerationCandidate<TModel> other) =>
        EqualityComparer<TModel?>.Default.Equals(Model, other.Model) &&
        Diagnostics.Equals(other.Diagnostics);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GenerationCandidate<TModel> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Model?.GetHashCode() ?? 0;
            return (hash * 397) ^ Diagnostics.GetHashCode();
        }
    }

    /// <summary>Determines whether two candidates carry equal models and diagnostics.</summary>
    /// <param name="left">The first candidate.</param>
    /// <param name="right">The second candidate.</param>
    /// <returns><see langword="true"/> when models and diagnostics are equal.</returns>
    public static bool operator ==(GenerationCandidate<TModel> left, GenerationCandidate<TModel> right) =>
        left.Equals(right);

    /// <summary>Determines whether two candidates differ by model or diagnostics.</summary>
    /// <param name="left">The first candidate.</param>
    /// <param name="right">The second candidate.</param>
    /// <returns><see langword="true"/> when models or diagnostics differ.</returns>
    public static bool operator !=(GenerationCandidate<TModel> left, GenerationCandidate<TModel> right) =>
        !left.Equals(right);
}
