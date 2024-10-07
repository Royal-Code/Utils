namespace RoyalCode.DependencyInjection.Generators;

public sealed class ServiceInformation
{
    public ServiceInformation(
        TypeDescriptor implementationType,
        TypeDescriptor[] servicesTypes,
        ServiceLifetime lifetime)
    {
        ImplementationType = implementationType;
        ServicesTypes = servicesTypes;
        Lifetime = lifetime;
    }

    public TypeDescriptor ImplementationType { get; }

    public TypeDescriptor[] ServicesTypes { get; }

    public ServiceLifetime Lifetime { get; }
}