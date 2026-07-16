using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Snapshots;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

public class DescriptorSnapshotsTests
{
    private const string Source =
        """
        namespace Shop
        {
            public class Product { public int Id { get; set; } }
            public class Store { }
        }
        """;

    private static (INamedTypeSymbol product, IPropertySymbol id) GetSymbols()
    {
        var compilation = CSharpCompilation.Create(
            "S",
            [CSharpSyntaxTree.ParseText(Source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var product = compilation.GetTypeByMetadataName("Shop.Product")!;
        var id = product.GetMembers("Id").OfType<IPropertySymbol>().Single();
        return (product, id);
    }

    [Fact]
    public void ParameterSnapshot_reads_hints_and_drops_symbols()
    {
        var (product, _) = GetSymbols();
        var type = TypeDescriptor.Create(product);
        type.MarkAsEntity();
        var descriptor = new ParameterDescriptor(type, "product");

        var snapshot = ParameterSnapshot.CreateFromHints(descriptor);

        Assert.Equal("product", snapshot.Name);
        Assert.True(snapshot.TypeUsage.IsEntity);
        Assert.False(snapshot.TypeUsage.IsContext);
        AssertSymbolFree(snapshot);
    }

    [Fact]
    public void ParameterSnapshot_equality_by_name_and_usage()
    {
        var type = new TypeDescriptor("Product", new[] { "Shop" });
        var a = ParameterSnapshot.Create(new ParameterDescriptor(type, "p"), TypeUsageRoles.Entity);
        var b = ParameterSnapshot.Create(new ParameterDescriptor(type, "p"), TypeUsageRoles.Entity);
        var differentRole = ParameterSnapshot.Create(new ParameterDescriptor(type, "p"), TypeUsageRoles.Context);
        var differentName = ParameterSnapshot.Create(new ParameterDescriptor(type, "q"), TypeUsageRoles.Entity);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, differentRole);
        Assert.NotEqual(a, differentName);
    }

    [Fact]
    public void ServiceTypeSnapshot_equality_and_symbol_free()
    {
        var (product, _) = GetSymbols();
        var descriptor = new ServiceTypeDescriptor(
            TypeDescriptor.Create(product),
            TypeDescriptor.Create(product));

        var a = ServiceTypeSnapshot.Create(descriptor);
        var b = ServiceTypeSnapshot.Create(descriptor);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        AssertSymbolFree(a);
    }

    [Fact]
    public void EditTypeSnapshot_considers_parameter_and_is_symbol_free()
    {
        var (product, _) = GetSymbols();
        var entity = TypeDescriptor.Create(product);
        var id = new TypeDescriptor("int", new[] { "System" });

        var withoutParameter = new EditTypeDescriptor(entity, id);
        var withParameter = new EditTypeDescriptor(entity, id)
        {
            Parameter = new ParameterDescriptor(entity, "product"),
        };

        var snapshotWithout = EditTypeSnapshot.Create(withoutParameter);
        var snapshotWith = EditTypeSnapshot.Create(withParameter);

        Assert.Null(snapshotWithout.Parameter);
        Assert.NotNull(snapshotWith.Parameter);
        Assert.NotEqual(snapshotWithout, snapshotWith);
        AssertSymbolFree(snapshotWith);
    }

    [Fact]
    public void IdPropertyBindingSnapshot_from_symbols_is_symbol_free()
    {
        var (product, id) = GetSymbols();
        var type = TypeDescriptor.Create(product);
        type.MarkAsEntity();
        var binding = new IdPropertyBoundToEntityParameter(
            new ParameterDescriptor(type, "product"),
            PropertyDescriptor.Create(id));

        var snapshot = IdPropertyBindingSnapshot.Create(binding);

        Assert.Equal("Id", snapshot.Property.Name);
        Assert.True(snapshot.Parameter.TypeUsage.IsEntity);
        AssertSymbolFree(snapshot);
    }

    private static void AssertSymbolFree(object root) =>
        Assert.DoesNotContain(Traverse(root), value =>
            value is ISymbol or SyntaxNode or SyntaxTree or SemanticModel or Compilation or Diagnostic or Location);

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
