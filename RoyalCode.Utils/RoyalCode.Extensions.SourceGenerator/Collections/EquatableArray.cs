using System.Collections;
using System.Collections.Immutable;

namespace RoyalCode.Extensions.SourceGenerator.Collections;

/// <summary>
/// <para>
///     An immutable array with value equality by content and order, safe to retain in an incremental
///     generator pipeline (unlike <see cref="ImmutableArray{T}"/>, whose equality is by reference).
/// </para>
/// <para>
///     A <see langword="default"/> value and an empty array represent the same empty sequence: they compare
///     equal and share the same hash code. Absence with domain meaning must be modelled explicitly outside the
///     collection, not as <see langword="default"/> versus empty.
/// </para>
/// </summary>
/// <typeparam name="T">The element type, which must have value equality.</typeparam>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    /// <summary>
    /// The shared empty instance. Equivalent to <see langword="default"/>.
    /// </summary>
    public static readonly EquatableArray<T> Empty = default;

    // empty is always normalized to null so that default, an empty array and an empty ImmutableArray coincide.
    private readonly T[]? items;

    /// <summary>
    /// Creates an <see cref="EquatableArray{T}"/> from an <see cref="ImmutableArray{T}"/>.
    /// A default or empty source becomes the empty sequence.
    /// </summary>
    /// <param name="items">The items to copy into the immutable sequence.</param>
    public EquatableArray(ImmutableArray<T> items)
    {
        this.items = items.IsDefaultOrEmpty ? null : System.Linq.Enumerable.ToArray(items);
    }

    /// <summary>
    /// Creates an <see cref="EquatableArray{T}"/> from an array. A <see langword="null"/> or empty source
    /// becomes the empty sequence.
    /// </summary>
    /// <param name="items">The items to copy into the immutable sequence.</param>
    public EquatableArray(T[]? items)
    {
        this.items = items is { Length: > 0 } ? items.ToArray() : null;
    }

    /// <summary>
    /// Creates an <see cref="EquatableArray{T}"/> from a sequence. A <see langword="null"/> or empty source
    /// becomes the empty sequence.
    /// </summary>
    /// <param name="items">The items to copy into the immutable sequence.</param>
    public EquatableArray(IEnumerable<T>? items)
        : this(items is null ? null : System.Linq.Enumerable.ToArray(items))
    { }

    /// <summary>Whether this is the empty sequence.</summary>
    public bool IsEmpty => items is null || items.Length == 0;

    /// <inheritdoc />
    public int Count => items?.Length ?? 0;

    /// <inheritdoc />
    public T this[int index] => (items ?? throw new IndexOutOfRangeException())[index];

    /// <summary>
    /// Returns the content as an <see cref="ImmutableArray{T}"/> (empty when this is the empty sequence).
    /// </summary>
    /// <returns>An immutable copy of the sequence.</returns>
    public ImmutableArray<T> AsImmutableArray() =>
        items is null ? ImmutableArray<T>.Empty : ImmutableArray.Create(items);

    /// <inheritdoc />
    public bool Equals(EquatableArray<T> other)
    {
        var a = items;
        var b = other.items;

        if (a is null || a.Length == 0)
            return b is null || b.Length == 0;
        if (b is null || a.Length != b.Length)
            return false;

        for (var i = 0; i < a.Length; i++)
            if (!EqualityComparer<T>.Default.Equals(a[i], b[i]))
                return false;

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (items is null || items.Length == 0)
            return 0;

        unchecked
        {
            var hash = 17;
            foreach (var item in items)
                hash = (hash * 31) + (item?.GetHashCode() ?? 0);
            return hash;
        }
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        var source = items ?? Array.Empty<T>();
        return ((IEnumerable<T>)source).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Determines whether two arrays contain the same items in the same order.</summary>
    /// <param name="left">The first array.</param>
    /// <param name="right">The second array.</param>
    /// <returns><see langword="true"/> when both arrays have equal content and order.</returns>
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    /// <summary>Determines whether two arrays differ by content or order.</summary>
    /// <param name="left">The first array.</param>
    /// <param name="right">The second array.</param>
    /// <returns><see langword="true"/> when content or order differs.</returns>
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}

/// <summary>
/// Helpers to build <see cref="EquatableArray{T}"/> values.
/// </summary>
public static class EquatableArray
{
    /// <summary>Creates an <see cref="EquatableArray{T}"/> by copying a sequence.</summary>
    /// <param name="source">The source sequence; <see langword="null"/> represents an empty sequence.</param>
    /// <typeparam name="T">The value-equatable item type.</typeparam>
    /// <returns>An immutable sequence with value equality.</returns>
    public static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T>? source)
        where T : IEquatable<T> => new(source);
}
