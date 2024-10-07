using Microsoft.CodeAnalysis;

namespace RoyalCode.DependencyInjection.Generators;

public static class Diagnostics
{
    private const string Category = "Usage";

    public static readonly DiagnosticDescriptor InvalidAddServicesUsage = new(
        id: "RCDI000",
        title: "Invalid AddServicesAttribute usage",
        messageFormat: "Invalid use of AddServicesAttribute: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidServiceUsage = new(
        id: "RCDI001",
        title: "Invalid ServiceAttribute usage",
        messageFormat: "Invalid use of ServiceAttribute: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

}