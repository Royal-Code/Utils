using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoyalCode.DependencyInjection.Generators;

public static class ServiceTransformer
{
    public const string ServiceAttributeName = "RoyalCode.DependencyInjection.ServiceAttribute";

    public static bool Predicate(SyntaxNode node, CancellationToken _) => node is ClassDeclarationSyntax;

    public static ServiceInformation Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken _)
    {
        var model = context.SemanticModel;
        var classDeclaration = (ClassDeclarationSyntax)context.TargetNode;
        var classSymbol = context.TargetSymbol as ITypeSymbol;
        var errors = new List<Diagnostic>();

        // verifica se na classe há o attr Scoped, Transient ou Singleton
        var lifetime = ServiceLifetime.Transient;
        var lifetimeAttribute = classDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString() == "Scoped" || a.Name.ToString() == "Transient" || a.Name.ToString() == "Singleton");

        if (lifetimeAttribute is not null)
        {
            var name = lifetimeAttribute.Name.ToString();
            lifetime = name switch
            {
                "Scoped" => ServiceLifetime.Scoped,
                "Singleton" => ServiceLifetime.Singleton,
                _ => lifetime
            };
        }

        // Obtém o type descriptor da classe, a qual é a implementação
        var implementationType = TypeDescriptor.Create(classSymbol!, model);

        // Verificar se a classe tem parâmetros genéricos
        SeparatedSyntaxList<TypeParameterSyntax> genericParams = default;
        bool hasGenericParameters = false;
        if (classDeclaration.TypeParameterList is not null && classDeclaration.TypeParameterList.Parameters.Count > 0)
        {
            hasGenericParameters = true;
            genericParams = classDeclaration.TypeParameterList.Parameters;
        }

        // obtém as interfaces que a classe implementa
        var servicesTypes = new List<TypeDescriptor>();
        foreach (var interfaceType in classSymbol!.AllInterfaces)
        {
            // se tem parâmetros genéricos, só adiciona as interfaces com a mesma quantidade de parâmetros
            if (hasGenericParameters && interfaceType.TypeParameters.Length != genericParams.Count)
                continue;

            var interfaceDescriptor = TypeDescriptor.Create(interfaceType, model);
            servicesTypes.Add(interfaceDescriptor);
        }

        // a classe não pode ser abstrata
        var isAbstract = classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword);
        if (isAbstract)
        {
            errors.Add(Diagnostic.Create(
                Diagnostics.InvalidServiceUsage,
                classDeclaration.GetLocation(),
                "The service can not be abstract"));
        }

        // não é possível registrar mais de uma internface quando há parâmetros genéricos
        if (hasGenericParameters && servicesTypes.Count > 1)
        {
            errors.Add(Diagnostic.Create(
                Diagnostics.InvalidServiceUsage,
                classDeclaration.GetLocation(),
                "An implementation with generic arguments can only implement one interface with generic arguments"));
        }

        return new ServiceInformation(implementationType, servicesTypes.ToArray(), lifetime, genericParams.Count)
            .WithErrors(errors);
    }
}