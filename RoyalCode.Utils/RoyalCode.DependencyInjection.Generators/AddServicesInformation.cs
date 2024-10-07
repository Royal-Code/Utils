namespace RoyalCode.DependencyInjection.Generators;

public sealed class AddServicesInformation
{
    public AddServicesInformation(TypeDescriptor serviceType, string methodName)
    {
        ServiceType = serviceType;
        MethodName = methodName;
    }

    public TypeDescriptor ServiceType { get; }

    public string MethodName { get; }
}