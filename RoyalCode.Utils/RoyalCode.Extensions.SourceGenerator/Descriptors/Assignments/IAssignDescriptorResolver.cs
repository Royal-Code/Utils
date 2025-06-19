using Microsoft.CodeAnalysis;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

internal interface IAssignDescriptorResolver
{
    public bool TryCreateAssignDescriptor(
        TypeDescriptor leftType,
        TypeDescriptor rightType,
        SemanticModel model,
        out AssignDescriptor? descriptor);
}
