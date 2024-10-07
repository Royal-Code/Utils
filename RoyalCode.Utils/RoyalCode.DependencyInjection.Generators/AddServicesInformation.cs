using Microsoft.CodeAnalysis;

namespace RoyalCode.DependencyInjection.Generators;

/// <summary>
/// Descriptions for generating the method that will add services.
/// </summary>
public sealed class AddServicesInformation : TransformationBase, IEquatable<AddServicesInformation>
{
    public AddServicesInformation(
        TypeDescriptor returnType,
        string methodName,
        bool methodIsPublic,
        TypeDescriptor classDescriptor)
    {
        ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
        MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        MethodIsPublic = methodIsPublic;
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

    public bool MethodIsPublic { get; }

    public TypeDescriptor ClassDescriptor { get; }

    public bool Equals(AddServicesInformation? other)
    {
        return other is not null &&
            Equals(ReturnType, other.ReturnType) &&
            Equals(MethodName, other.MethodName) &&
            MethodIsPublic == other.MethodIsPublic &&
            Equals(ClassDescriptor, other.ClassDescriptor) &&
            EqualErrors(other);
    }

    public override bool Equals(object? obj)
    {
        return obj is AddServicesInformation information && Equals(information);
    }

    public override int GetHashCode()
    {
        int hashCode = -218655081;
        hashCode = hashCode * -1521134295 + EqualityComparer<List<Diagnostic>?>.Default.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<TypeDescriptor>.Default.GetHashCode(ReturnType);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MethodName);
        hashCode = hashCode * -1521134295 + MethodIsPublic.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<TypeDescriptor>.Default.GetHashCode(ClassDescriptor);
        return hashCode;
    }
}