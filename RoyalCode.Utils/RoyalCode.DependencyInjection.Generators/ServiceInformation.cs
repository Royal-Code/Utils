using Microsoft.CodeAnalysis;

namespace RoyalCode.DependencyInjection.Generators;

public sealed class ServiceInformation : TransformationBase, IEquatable<ServiceInformation>
{
    public ServiceInformation(
        TypeDescriptor implementationType,
        TypeDescriptor[] servicesTypes,
        ServiceLifetime lifetime,
        int genericParameters)
    {
        ImplementationType = implementationType;
        ServicesTypes = servicesTypes;
        Lifetime = lifetime;
        GenericParameters = genericParameters;
    }

    internal ServiceInformation WithErrors(List<Diagnostic> errors)
    {
        if (errors.Count > 0)
            AddErrors(errors);
        return this;
    }

    public TypeDescriptor ImplementationType { get; }

    public TypeDescriptor[] ServicesTypes { get; }

    public ServiceLifetime Lifetime { get; }

    public int GenericParameters { get; }

    public bool Equals(ServiceInformation other)
    {
        return other is not null &&
            Equals(ImplementationType, other.ImplementationType) &&
            EqualityComparer<TypeDescriptor[]>.Default.Equals(ServicesTypes, other.ServicesTypes) &&
            Lifetime == other.Lifetime &&
            GenericParameters == other.GenericParameters &&
            EqualErrors(other);

    }

    public override bool Equals(object? obj)
    {
        return obj is ServiceInformation information && Equals(information);
    }

    public override int GetHashCode()
    {
        int hashCode = 676100879;
        hashCode = hashCode * -1521134295 + EqualityComparer<List<Diagnostic>?>.Default.GetHashCode(Errors);
        hashCode = hashCode * -1521134295 + EqualityComparer<TypeDescriptor>.Default.GetHashCode(ImplementationType);
        hashCode = hashCode * -1521134295 + EqualityComparer<TypeDescriptor[]>.Default.GetHashCode(ServicesTypes);
        hashCode = hashCode * -1521134295 + Lifetime.GetHashCode();
        hashCode = hashCode * -1521134295 + GenericParameters.GetHashCode();
        return hashCode;
    }

}