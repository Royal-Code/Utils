using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.DependencyInjection.Generators;
using System.Collections.Immutable;

namespace RoyalCode.DependencyInjection.Tests;

internal static class Util
{
    internal static void Compile(
        string sourceCode,
        out Compilation outputCompilation,
        out ImmutableArray<Diagnostic> diagnostics)
    {
        // the source code to be compiled
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // assemblies references requered to compile the source code
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(Util).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICollection<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(CancellationToken).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ServiceAttribute).Assembly.Location),
        };

        // create a compilation for the source code.
        var compilation = CSharpCompilation.Create("SourceGeneratorTests", [syntaxTree], references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // apply the source generator and collect the output
        var driver = CSharpGeneratorDriver.Create(new IncrementalGenerator());

        driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out diagnostics);
    }
}
