namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Snapshots;

/// <summary>
/// A symbol-free, value-equatable snapshot of a <see cref="ParameterDescriptor"/>: the parameter name plus
/// the type <em>as used</em> (structure + contextual roles) via <see cref="TypeUsageSnapshot"/>.
/// </summary>
public sealed class ParameterSnapshot : IEquatable<ParameterSnapshot>
{
    /// <summary>Creates a parameter snapshot.</summary>
    /// <param name="typeUsage">The frozen structural type and contextual roles.</param>
    /// <param name="name">The non-empty parameter name.</param>
    public ParameterSnapshot(TypeUsageSnapshot typeUsage, string name)
    {
        TypeUsage = typeUsage ?? throw new ArgumentNullException(nameof(typeUsage));
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Parameter name cannot be null, empty, or whitespace.", nameof(name));
    }

    /// <summary>The type of the parameter, with its contextual roles frozen.</summary>
    public TypeUsageSnapshot TypeUsage { get; }

    /// <summary>The parameter name.</summary>
    public string Name { get; }

    /// <summary>Creates a snapshot reading the descriptor's type hints into frozen roles.</summary>
    /// <param name="descriptor">The parameter descriptor to snapshot.</param>
    /// <returns>An immutable, symbol-free parameter snapshot.</returns>
    public static ParameterSnapshot CreateFromHints(ParameterDescriptor descriptor) =>
        descriptor is null
            ? throw new ArgumentNullException(nameof(descriptor))
            : new(TypeUsageSnapshot.CreateFromHints(descriptor.Type), descriptor.Name);

    /// <summary>Creates a snapshot with explicit roles for the parameter's type.</summary>
    /// <param name="descriptor">The parameter descriptor to snapshot.</param>
    /// <param name="roles">The contextual roles resolved for this parameter.</param>
    /// <returns>An immutable, symbol-free parameter snapshot.</returns>
    public static ParameterSnapshot Create(ParameterDescriptor descriptor, TypeUsageRoles roles) =>
        descriptor is null
            ? throw new ArgumentNullException(nameof(descriptor))
            : new(TypeUsageSnapshot.Create(descriptor.Type, roles), descriptor.Name);

    /// <inheritdoc />
    public bool Equals(ParameterSnapshot? other) =>
        other is not null && Name == other.Name && TypeUsage.Equals(other.TypeUsage);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ParameterSnapshot other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => (TypeUsage.GetHashCode() * 397) ^ Name.GetHashCode();
}

/// <summary>
/// A symbol-free, value-equatable snapshot of a <see cref="ServiceTypeDescriptor"/> (a DI registration pair).
/// A service registration has no contextual role, so both members are structural <see cref="TypeSnapshot"/>s.
/// </summary>
public sealed class ServiceTypeSnapshot : IEquatable<ServiceTypeSnapshot>
{
    /// <summary>Creates a service registration snapshot.</summary>
    /// <param name="interfaceType">The registered service type.</param>
    /// <param name="handlerType">The service implementation type.</param>
    public ServiceTypeSnapshot(TypeSnapshot interfaceType, TypeSnapshot handlerType)
    {
        InterfaceType = interfaceType ?? throw new ArgumentNullException(nameof(interfaceType));
        HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
    }

    /// <summary>The service interface type.</summary>
    public TypeSnapshot InterfaceType { get; }

    /// <summary>The service implementation (handler) type.</summary>
    public TypeSnapshot HandlerType { get; }

    /// <summary>Creates a snapshot from a descriptor.</summary>
    /// <param name="descriptor">The service registration descriptor to snapshot.</param>
    /// <returns>An immutable, symbol-free service registration snapshot.</returns>
    public static ServiceTypeSnapshot Create(ServiceTypeDescriptor descriptor) =>
        descriptor is null
            ? throw new ArgumentNullException(nameof(descriptor))
            : new(TypeSnapshot.Create(descriptor.InterfaceType), TypeSnapshot.Create(descriptor.HandlerType));

    /// <inheritdoc />
    public bool Equals(ServiceTypeSnapshot? other) =>
        other is not null && InterfaceType.Equals(other.InterfaceType) && HandlerType.Equals(other.HandlerType);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ServiceTypeSnapshot other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => (InterfaceType.GetHashCode() * 397) ^ HandlerType.GetHashCode();
}

/// <summary>
/// A symbol-free, value-equatable snapshot of an <see cref="EditTypeDescriptor"/>: the edited entity type,
/// its id type, and the resolved entity parameter (when already bound during the transform).
/// </summary>
public sealed class EditTypeSnapshot : IEquatable<EditTypeSnapshot>
{
    /// <summary>Creates an edited-entity snapshot.</summary>
    /// <param name="entityType">The entity type being edited.</param>
    /// <param name="idType">The entity identifier type.</param>
    /// <param name="parameter">The bound handler parameter, when resolved.</param>
    public EditTypeSnapshot(TypeSnapshot entityType, TypeSnapshot idType, ParameterSnapshot? parameter)
    {
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        IdType = idType ?? throw new ArgumentNullException(nameof(idType));
        Parameter = parameter;
    }

    /// <summary>The entity type being edited.</summary>
    public TypeSnapshot EntityType { get; }

    /// <summary>The id type of the edited entity.</summary>
    public TypeSnapshot IdType { get; }

    /// <summary>The command parameter that receives the loaded entity, when resolved.</summary>
    public ParameterSnapshot? Parameter { get; }

    /// <summary>Creates a snapshot from a descriptor, snapshotting its bound parameter when present.</summary>
    /// <param name="descriptor">The edited-entity descriptor to snapshot.</param>
    /// <returns>An immutable, symbol-free edited-entity snapshot.</returns>
    public static EditTypeSnapshot Create(EditTypeDescriptor descriptor) =>
        descriptor is null
            ? throw new ArgumentNullException(nameof(descriptor))
            : new(
            TypeSnapshot.Create(descriptor.EntityType),
            TypeSnapshot.Create(descriptor.IdType),
            descriptor.Parameter is null ? null : ParameterSnapshot.CreateFromHints(descriptor.Parameter));

    /// <inheritdoc />
    public bool Equals(EditTypeSnapshot? other) =>
        other is not null &&
        EntityType.Equals(other.EntityType) &&
        IdType.Equals(other.IdType) &&
        Equals(Parameter, other.Parameter);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EditTypeSnapshot other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = EntityType.GetHashCode();
            hash = (hash * 397) ^ IdType.GetHashCode();
            return (hash * 397) ^ (Parameter?.GetHashCode() ?? 0);
        }
    }
}

/// <summary>
/// A symbol-free, value-equatable snapshot of an <see cref="IdPropertyBoundToEntityParameter"/>: the binding
/// between an entity (or collection-of-entities) parameter and the command property that carries its id(s).
/// </summary>
public sealed class IdPropertyBindingSnapshot : IEquatable<IdPropertyBindingSnapshot>
{
    /// <summary>Creates an entity-id binding snapshot.</summary>
    /// <param name="parameter">The bound entity parameter.</param>
    /// <param name="property">The command property carrying the identifier.</param>
    public IdPropertyBindingSnapshot(ParameterSnapshot parameter, PropertySnapshot property)
    {
        Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        Property = property ?? throw new ArgumentNullException(nameof(property));
    }

    /// <summary>The entity (or collection-of-entities) parameter.</summary>
    public ParameterSnapshot Parameter { get; }

    /// <summary>The command property holding the id (or ids) used to load the entity.</summary>
    public PropertySnapshot Property { get; }

    /// <summary>Creates a snapshot from a descriptor.</summary>
    /// <param name="descriptor">The entity-id binding descriptor to snapshot.</param>
    /// <returns>An immutable, symbol-free binding snapshot.</returns>
    public static IdPropertyBindingSnapshot Create(IdPropertyBoundToEntityParameter descriptor) =>
        descriptor is null
            ? throw new ArgumentNullException(nameof(descriptor))
            : new(ParameterSnapshot.CreateFromHints(descriptor.Parameter), PropertySnapshot.Create(descriptor.Property));

    /// <inheritdoc />
    public bool Equals(IdPropertyBindingSnapshot? other) =>
        other is not null && Parameter.Equals(other.Parameter) && Property.Equals(other.Property);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is IdPropertyBindingSnapshot other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => (Parameter.GetHashCode() * 397) ^ Property.GetHashCode();
}
