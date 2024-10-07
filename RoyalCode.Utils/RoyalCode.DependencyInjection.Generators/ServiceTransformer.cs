using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoyalCode.DependencyInjection.Generators;

public static class ServiceTransformer
{
    public const string ServiceAttributeName = "RoyalCode.DependencyInjection.Subscribers.Attributes.ServiceAttribute";

    public static bool Predicate(SyntaxNode node, CancellationToken _) => node is ClassDeclarationSyntax;

    public static ServiceInformation Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken _)
    {
        var model = context.SemanticModel;
        var classDeclaration = (ClassDeclarationSyntax)context.TargetNode;
        var classSymbol = context.TargetSymbol as ITypeSymbol;

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

        // obtém as interfaces que a classe implementa
        var servicesTypes = new List<TypeDescriptor>();
        foreach (var interfaceType in classSymbol!.AllInterfaces)
        {
            var interfaceDescriptor = TypeDescriptor.Create(interfaceType, model);
            servicesTypes.Add(interfaceDescriptor);
        }

        return new ServiceInformation(implementationType, servicesTypes.ToArray(), lifetime);
    }
}