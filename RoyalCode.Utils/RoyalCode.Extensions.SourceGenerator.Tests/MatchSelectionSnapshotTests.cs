using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Snapshots;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

public class MatchSelectionSnapshotTests
{
    [Fact]
    public void Snapshot_should_not_retain_Roslyn_objects_and_should_inline_new_instance_parent_path()
    {
        const string source =
            """
            public class Address { public string City { get; set; } }
            public class Entity { public Address Address { get; set; } }
            public class AddressDto { public string City { get; set; } }
            public class Dto { public AddressDto Address { get; set; } }
            """;
        var compilation = CSharpCompilation.Create(
            "Snapshot",
            [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var entity = compilation.GetTypeByMetadataName("Entity")!;
        var dto = compilation.GetTypeByMetadataName("Dto")!;
        var model = compilation.GetSemanticModel(compilation.SyntaxTrees.Single());

        var selection = MatchSelection.Create(
            TypeDescriptor.Create(dto),
            TypeDescriptor.Create(entity),
            model);
        var snapshot = MatchSelectionSnapshotFactory.Create(selection);

        var address = snapshot.PropertyMatches.Single();
        var city = address.Assignment!.InnerSelection!.PropertyMatches.Single();
        Assert.Equal("Address.City", city.Target!.Path);
        Assert.DoesNotContain(Traverse(snapshot), value =>
            value is ISymbol or SyntaxNode or SyntaxTree or SemanticModel or Compilation or Diagnostic or Location);
    }

    private static IEnumerable<object> Traverse(object root)
    {
        var queue = new Queue<object>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var value = queue.Dequeue();
            if (!visited.Add(value))
                continue;
            yield return value;
            if (value is string || value.GetType().IsPrimitive || value.GetType().IsEnum)
                continue;
            if (value is System.Collections.IEnumerable enumerable)
                foreach (var item in enumerable)
                    if (item is not null) queue.Enqueue(item);
            foreach (var field in value.GetType().GetFields(
                         System.Reflection.BindingFlags.Instance |
                         System.Reflection.BindingFlags.Public |
                         System.Reflection.BindingFlags.NonPublic))
                if (field.GetValue(value) is { } child) queue.Enqueue(child);
        }
    }
}
