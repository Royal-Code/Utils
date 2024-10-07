using Microsoft.CodeAnalysis;

namespace RoyalCode.DependencyInjection.Generators;

public sealed class ServiceInformation : TransformationBase
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
}