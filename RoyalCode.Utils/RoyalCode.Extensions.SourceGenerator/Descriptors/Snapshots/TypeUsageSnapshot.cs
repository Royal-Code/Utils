namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Snapshots;

/// <summary>
/// The contextual roles a type can play at a particular use site (a parameter, a service, ...).
/// These are usage facts, not structural facts, so they never belong on the structural
/// <see cref="TypeSnapshot"/>: the same type may play different roles at different sites.
/// </summary>
[Flags]
public enum TypeUsageRoles
{
    /// <summary>No contextual role.</summary>
    None = 0,

    /// <summary>The type is a domain entity loaded by the handler.</summary>
    Entity = 1,

    /// <summary>The type is the unit-of-work / persistence context.</summary>
    Context = 2,

    /// <summary>The type is an externally-bound handler parameter (e.g. <c>WithParameter</c>).</summary>
    HandlerParameter = 4,

    /// <summary>The type is a collection of domain entities loaded by the handler.</summary>
    CollectionOfEntities = 8,
}

/// <summary>
/// <para>
///     A symbol-free, value-equatable snapshot of a type <em>as used</em> at a specific site: the structural
///     <see cref="TypeSnapshot"/> together with the contextual <see cref="TypeUsageRoles"/> resolved for that site.
/// </para>
/// <para>
///     The roles are frozen here at transform time and never mutate the structural snapshot, so two uses of the
///     same type that differ only by role produce different <see cref="TypeUsageSnapshot"/> values while sharing
///     an equal <see cref="TypeSnapshot"/>.
/// </para>
/// </summary>
public sealed class TypeUsageSnapshot : IEquatable<TypeUsageSnapshot>
{
    /// <summary>Creates a usage snapshot from a structural snapshot and its roles.</summary>
    /// <param name="type">The immutable structural type snapshot.</param>
    /// <param name="roles">The contextual roles resolved for this use site.</param>
    public TypeUsageSnapshot(TypeSnapshot type, TypeUsageRoles roles)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Roles = roles;
    }

    /// <summary>The structural snapshot of the type (never carries the role).</summary>
    public TypeSnapshot Type { get; }

    /// <summary>The contextual roles resolved for this use site.</summary>
    public TypeUsageRoles Roles { get; }

    /// <summary>Whether the type is used as a domain entity here.</summary>
    public bool IsEntity => (Roles & TypeUsageRoles.Entity) != 0;

    /// <summary>Whether the type is used as the persistence context here.</summary>
    public bool IsContext => (Roles & TypeUsageRoles.Context) != 0;

    /// <summary>Whether the type is used as an externally-bound handler parameter here.</summary>
    public bool IsHandlerParameter => (Roles & TypeUsageRoles.HandlerParameter) != 0;

    /// <summary>Whether the type is used as a collection of domain entities here.</summary>
    public bool IsCollectionOfEntities => (Roles & TypeUsageRoles.CollectionOfEntities) != 0;

    /// <summary>Creates a usage snapshot from a descriptor and explicit roles.</summary>
    /// <param name="descriptor">The type descriptor to snapshot.</param>
    /// <param name="roles">The contextual roles resolved for this use site.</param>
    /// <returns>An immutable, symbol-free usage snapshot.</returns>
    public static TypeUsageSnapshot Create(TypeDescriptor descriptor, TypeUsageRoles roles) =>
        new(TypeSnapshot.Create(descriptor), roles);

    /// <summary>
    /// Creates a usage snapshot from a descriptor, reading its mutable hints
    /// (<c>IsEntity</c>, <c>IsContext</c>, <c>IsHandlerParameter</c>, <c>IsCollectionOfEntities</c>) into
    /// frozen <see cref="TypeUsageRoles"/>. Use this to cross the boundary from the working model to the pipeline.
    /// </summary>
    /// <param name="descriptor">The descriptor whose structural type and mutable role hints will be frozen.</param>
    /// <returns>An immutable, symbol-free usage snapshot.</returns>
    public static TypeUsageSnapshot CreateFromHints(TypeDescriptor descriptor)
    {
        if (descriptor is null)
            throw new ArgumentNullException(nameof(descriptor));

        var roles = TypeUsageRoles.None;
        if (descriptor.IsEntity) roles |= TypeUsageRoles.Entity;
        if (descriptor.IsContext) roles |= TypeUsageRoles.Context;
        if (descriptor.IsHandlerParameter) roles |= TypeUsageRoles.HandlerParameter;
        if (descriptor.IsCollectionOfEntities) roles |= TypeUsageRoles.CollectionOfEntities;
        return Create(descriptor, roles);
    }

    /// <inheritdoc />
    public bool Equals(TypeUsageSnapshot? other) =>
        other is not null && Roles == other.Roles && Type.Equals(other.Type);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TypeUsageSnapshot other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (Type.GetHashCode() * 397) ^ (int)Roles;
        }
    }
}
