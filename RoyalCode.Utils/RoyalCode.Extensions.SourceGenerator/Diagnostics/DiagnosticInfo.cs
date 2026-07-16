using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoyalCode.Extensions.SourceGenerator.Collections;

namespace RoyalCode.Extensions.SourceGenerator.Diagnostics;

/// <summary>
/// <para>
///     An immutable, symbol-free and value-equatable description of a diagnostic, safe to retain in an
///     incremental generator pipeline. It stores the diagnostic id, its arguments as strings and the
///     location decomposed into file path, source span and line span.
/// </para>
/// <para>
///     The pipeline carries this DTO; the <see cref="Location"/> and <see cref="Diagnostic"/> are reconstructed
///     only at <c>RegisterSourceOutput</c> via <see cref="ToDiagnostic"/>, whose resolver maps an id back to its
///     <see cref="DiagnosticDescriptor"/>. The catalog of descriptors belongs to the consuming generator, so this
///     type does not reference any specific catalog.
/// </para>
/// </summary>
public sealed class DiagnosticInfo : IEquatable<DiagnosticInfo>
{
    private DiagnosticInfo(
        string id,
        EquatableArray<string> arguments,
        string? filePath,
        TextSpan sourceSpan,
        LinePositionSpan lineSpan,
        bool hasLocation)
    {
        Id = id;
        Arguments = arguments;
        FilePath = filePath;
        SourceSpan = sourceSpan;
        LineSpan = lineSpan;
        HasLocation = hasLocation;
    }

    /// <summary>The diagnostic id (e.g. <c>RCCMD026</c>).</summary>
    public string Id { get; }

    /// <summary>The message arguments, already reduced to strings.</summary>
    public EquatableArray<string> Arguments { get; }

    /// <summary>The source file path, or <see langword="null"/> when there is no in-source location.</summary>
    public string? FilePath { get; }

    /// <summary>The source span of the location.</summary>
    public TextSpan SourceSpan { get; }

    /// <summary>The line/character span of the location.</summary>
    public LinePositionSpan LineSpan { get; }

    /// <summary>Whether an in-source location was captured.</summary>
    public bool HasLocation { get; }

    /// <summary>
    /// Creates a <see cref="DiagnosticInfo"/> from a descriptor, an optional location and message arguments.
    /// The location is decomposed into equatable parts; when it is not in source it is dropped.
    /// </summary>
    /// <param name="descriptor">The diagnostic descriptor whose id identifies the diagnostic.</param>
    /// <param name="location">The optional source location to decompose.</param>
    /// <param name="arguments">The values used to format the diagnostic message.</param>
    /// <returns>An immutable, symbol-free diagnostic description.</returns>
    public static DiagnosticInfo Create(
        DiagnosticDescriptor descriptor,
        Location? location,
        params object?[] arguments)
    {
        if (descriptor is null)
            throw new ArgumentNullException(nameof(descriptor));

        var values = new EquatableArray<string>(
            (arguments ?? Array.Empty<object?>()).Select(value => value?.ToString() ?? string.Empty));

        if (location is null || location == Location.None || !location.IsInSource)
            return new DiagnosticInfo(descriptor.Id, values, null, default, default, false);

        var lineSpan = location.GetLineSpan();
        return new DiagnosticInfo(descriptor.Id, values, lineSpan.Path, location.SourceSpan, lineSpan.Span, true);
    }

    /// <summary>
    /// Reconstructs the <see cref="Diagnostic"/>. The <paramref name="descriptorResolver"/> maps this
    /// diagnostic's <see cref="Id"/> back to its <see cref="DiagnosticDescriptor"/>; the catalog is owned by
    /// the consuming generator, so this type never depends on a specific catalog.
    /// </summary>
    /// <param name="descriptorResolver">Resolves a diagnostic id to the consuming generator's descriptor.</param>
    /// <returns>The reconstructed Roslyn diagnostic.</returns>
    public Diagnostic ToDiagnostic(Func<string, DiagnosticDescriptor> descriptorResolver)
    {
        if (descriptorResolver is null)
            throw new ArgumentNullException(nameof(descriptorResolver));

        var location = HasLocation
            ? Location.Create(FilePath ?? string.Empty, SourceSpan, LineSpan)
            : Location.None;

        return Diagnostic.Create(
            descriptorResolver(Id),
            location,
            Arguments.Cast<object>().ToArray());
    }

    /// <inheritdoc />
    public bool Equals(DiagnosticInfo? other) =>
        other is not null &&
        Id == other.Id &&
        Arguments.Equals(other.Arguments) &&
        FilePath == other.FilePath &&
        SourceSpan.Equals(other.SourceSpan) &&
        LineSpan.Equals(other.LineSpan) &&
        HasLocation == other.HasLocation;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DiagnosticInfo other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Id.GetHashCode();
            hash = (hash * 397) ^ Arguments.GetHashCode();
            hash = (hash * 397) ^ (FilePath?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ SourceSpan.GetHashCode();
            hash = (hash * 397) ^ LineSpan.GetHashCode();
            return (hash * 397) ^ HasLocation.GetHashCode();
        }
    }
}
