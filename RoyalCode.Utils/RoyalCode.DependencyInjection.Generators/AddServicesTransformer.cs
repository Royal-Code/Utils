using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace RoyalCode.DependencyInjection.Generators;

public static class AddServicesTransformer
{
    public const string AddServicesAttributeName = "RoyalCode.DependencyInjection.AddServicesAttribute";

    public static bool Predicate(SyntaxNode node, CancellationToken _) => node is MethodDeclarationSyntax;

    public static AddServicesInformation Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken _)
    {
        // obtém o método
        var method = (MethodDeclarationSyntax)context.TargetNode;

        // obtém a classe que contém o método
        if (method.Parent is not ClassDeclarationSyntax classDeclaration)
        {
            return new AddServicesInformation(Diagnostic.Create(
                Diagnostics.InvalidAddServicesUsage,
                method.GetLocation(),
                "The method does not have a class declaration"));
        }

        // o método deve ser public, partial e static
        var isPrivate = method.Modifiers.Any(SyntaxKind.PrivateKeyword);
        var isPublic = method.Modifiers.Any(SyntaxKind.PublicKeyword);
        var notStatic = !method.Modifiers.Any(SyntaxKind.StaticKeyword);
        var notPartial = !method.Modifiers.Any(SyntaxKind.PartialKeyword);

        if (isPrivate || notStatic || notPartial)
        {
            return new AddServicesInformation(Diagnostic.Create(
                Diagnostics.InvalidAddServicesUsage,
                method.GetLocation(),
                "The method must be public or internal and static and partial"));
        }

        // obtém o retorno do método
        var methodReturnType = TypeDescriptor.Create(method.ReturnType, context.SemanticModel);

        // valida o tipo de retorno, deve ser void ou IServiceCollection
        if (!IsValidReturnType(method, context.SemanticModel))
        {
            return new AddServicesInformation(Diagnostic.Create(
                Diagnostics.InvalidAddServicesUsage,
                method.GetLocation(),
                "The method must return void or IServiceCollection"));
        }

        // valida o parâmetro, deve ser (this IServiceCollection services)
        if (!HasValidParameter(method, context.SemanticModel))
        {
            return new AddServicesInformation(Diagnostic.Create(
                Diagnostics.InvalidAddServicesUsage,
                method.GetLocation(),
                "The method must have a single parameter of type IServiceCollection and modify 'this'"));
        }

        var classDescriptor = new TypeDescriptor(classDeclaration.Identifier.Text, [classDeclaration.GetNamespace()]);

        return new AddServicesInformation(methodReturnType, method.Identifier.Text, isPublic, classDescriptor);
    }

    public static bool IsValidReturnType(MethodDeclarationSyntax method, SemanticModel semanticModel)
    {
        // Obtenha o tipo de retorno do método
        var returnTypeSyntax = method.ReturnType;

        // Obtenha o símbolo do tipo de retorno usando o SemanticModel
        var returnTypeSymbol = semanticModel.GetSymbolInfo(returnTypeSyntax).Symbol as ITypeSymbol;

        // Se não conseguiu obter o símbolo, retorne false
        if (returnTypeSymbol == null)
            return false;

        // Verifique se o tipo de retorno é void
        if (returnTypeSymbol.SpecialType == SpecialType.System_Void)
            return true;

        // Verifique se o tipo de retorno é IServiceCollection do namespace Microsoft.Extensions.DependencyInjection
        if (returnTypeSymbol.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.IServiceCollection")
            return true;

        // Se não é nem void nem IServiceCollection, retorne false
        return false;
    }

    public static bool HasValidParameter(MethodDeclarationSyntax method, SemanticModel semanticModel)
    {
        if (method.ParameterList.Parameters.Count != 1)
            return false;

        // Percorre os parâmetros do método
        var parameter = method.ParameterList.Parameters[0];

        // Verifica se o parâmetro tem o modificador 'this'
        bool isExtensionMethodParameter = parameter.Modifiers.Any(SyntaxKind.ThisKeyword);

        if (!isExtensionMethodParameter)
            return false;

        // Obtenha o tipo do parâmetro
        var parameterType = parameter.Type;
        if (parameterType == null)
            return false;

        // Usa o SemanticModel para obter informações sobre o tipo do parâmetro
        var parameterTypeSymbol = semanticModel.GetTypeInfo(parameterType).Type as INamedTypeSymbol;

        // Verifique se o tipo do parâmetro é IServiceCollection do namespace Microsoft.Extensions.DependencyInjection
        if (parameterTypeSymbol is null
            || parameterTypeSymbol.Name != "IServiceCollection"
            || parameterTypeSymbol.ContainingNamespace.ToString() != "Microsoft.Extensions.DependencyInjection")
        {
            return false;
        }
        
        return true; // Encontrou o parâmetro correto
    }
}
