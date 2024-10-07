
using Microsoft.CodeAnalysis;

namespace RoyalCode.DependencyInjection.Generators;

/// <summary>
/// Descriptions for generating the method that will add services.
/// </summary>
public sealed class AddServicesInformation : TransformationBase
{
    public AddServicesInformation(TypeDescriptor returnType, string methodName, TypeDescriptor classDescriptor)
    {
        ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
        MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        ClassDescriptor = classDescriptor ?? throw new ArgumentNullException(nameof(classDescriptor));
        CanGenerate = true;
    }

    public AddServicesInformation(Diagnostic diagnostic)
    {
        CanGenerate = false;
        AddError(diagnostic);
    }

    public bool CanGenerate { get; }

    public TypeDescriptor ReturnType { get; }

    public string MethodName { get; }

    public TypeDescriptor ClassDescriptor { get; }
}