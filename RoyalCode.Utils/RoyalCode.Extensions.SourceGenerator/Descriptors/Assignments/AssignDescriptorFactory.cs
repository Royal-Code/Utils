using Microsoft.CodeAnalysis;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

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

    public static AssignDescriptor? Create(
        TypeDescriptor leftType, TypeDescriptor rightType, SemanticModel model, MatchOptions options)
    {
        if (options.AdditionalAssignDescriptorResolvers is not null 
            && options.AdditionalAssignDescriptorResolvers.Length is not 0)
        {
            foreach (var analyzer in options.AdditionalAssignDescriptorResolvers)
            {
                if (analyzer.TryCreateAssignDescriptor(
                    leftType,
                    rightType,
                    model,
                    options,
                    out var descriptor))
                {
                    return descriptor;
                }
            }
        }

        foreach (var analyzer in resolvers)
        {
            if (analyzer.TryCreateAssignDescriptor(
                leftType,
                rightType,
                model,
                options,
                out var descriptor))
            {
                return descriptor;
            }
        }

        return null;
    }
}
