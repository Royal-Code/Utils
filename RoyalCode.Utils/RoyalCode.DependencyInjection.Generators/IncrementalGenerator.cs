using Microsoft.CodeAnalysis;

namespace RoyalCode.DependencyInjection.Generators;

[Generator]
public class IncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipelineService = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: ServiceTransformer.ServiceAttributeName,
            predicate: ServiceTransformer.Predicate,
            transform: ServiceTransformer.Transform);

        var pipelineAddServices = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: AddServicesTransformer.AddServicesAttributeName,
            predicate: AddServicesTransformer.Predicate,
            transform: AddServicesTransformer.Transform);

        var combine = pipelineAddServices.Combine(pipelineService.Collect());

        context.RegisterSourceOutput(combine, (spc, source) =>
        {
            var (addServices, services) = source;

            addServices.ReportDiagnostic(spc);
            foreach (var service in services)
                service.ReportDiagnostic(spc);

            if (!addServices.CanGenerate)
                return;

            var generator = new AddServicesGenerator(spc, addServices, services);
            generator.Generate();
        });
    }
}