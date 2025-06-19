using Microsoft.CodeAnalysis;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

internal class AssignDescriptorFactory
{
    private static IAssignDescriptorResolver[] resolvers =
        [
            new DirectAssignDescriptorResolver(),
            new CastAssignDescriptorResolver(),
            new NullableAssignDescriptorResolver(),
            new EnumerableAssignDescriptorResolver(),
            new InnerTypeAssignDescriptorResolver(),
        ];

    public static AssignDescriptor? Create(TypeDescriptor leftType, TypeDescriptor rightType, SemanticModel model)
    {
        foreach (var analyzer in resolvers)
        {
            if (analyzer.TryCreateAssignDescriptor(
                leftType,
                rightType,
                model,
                out var descriptor))
            {
                return descriptor;
            }
        }

        return null;
    }
}
