using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

internal sealed class DirectAssignDescriptorResolver : IAssignDescriptorResolver
{
    public bool TryCreateAssignDescriptor(
        TypeDescriptor leftType,
        TypeDescriptor rightType,
        SemanticModel model,
        MatchOptions options,
        out AssignDescriptor? descriptor)
    {
        if (!CanBeAssigned(leftType, rightType, model))
        {
            descriptor = null;
            return false;
        }

        descriptor = new AssignDescriptor
        {
            AssignType = AssignType.Direct,
        };
        return true;
    }

    private bool CanBeAssigned(TypeDescriptor leftType, TypeDescriptor rightType, SemanticModel model)
    {
        if (leftType.Equals(rightType))
            return true;

        if (leftType.Symbol is null || rightType.Symbol is null)
            return false;

        var conversion = model.Compilation.ClassifyConversion(
                    leftType.Symbol!,
                    rightType.Symbol!);

        return conversion.Exists && conversion.IsImplicit;
    }
}