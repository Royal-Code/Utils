using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoyalCode.DependencyInjection.Generators;

public static class AddServicesTransformer
{
    public const string AddServicesAttributeName = "RoyalCode.DependencyInjection.Subscribers.Attributes.AddServicesAttribute";

    public static bool Predicate(SyntaxNode node, CancellationToken _) => node is ClassDeclarationSyntax;

    public static AddServicesInformation Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken _)
    {
        // obtém o nome do método que será chamado para adicionar os serviços
        var attrCtorArg = context.Attributes[0].ConstructorArguments[0];
        var methodName = attrCtorArg.Value as string;

        // obtém o type descriptor da classe, a qual é a implementação
        var classSymbol = context.TargetSymbol as ITypeSymbol;
        var implementationType = TypeDescriptor.Create(classSymbol!, context.SemanticModel);

        return new AddServicesInformation(implementationType, methodName!);
    }
}