using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Snapshots;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

public class TypeSnapshotExtensionTests
{
    private const string Source =
        """
        #nullable enable

        namespace N
        {
            public class Box<T> { }
            public class Task<T> { }
            public class Outer<T> { public class Inner<U> { } }
            public class Item { }
            public class Store { }
            public class Holder
            {
                public Box<Item> Generic;
                public Box<Store> OtherGeneric;
                public Item[] Array;
                public Item[,] Matrix;
                public Item[][] Jagged;
                public Task<Item> CustomTask;
                public System.Threading.Tasks.Task<Item> FrameworkTask;
                public Outer<Item>.Inner<Store> Nested;
                public Outer<Store>.Inner<Store> OtherNested;
            }
        }
        """;

    private static ITypeSymbol FieldType(string field)
    {
        var compilation = CSharpCompilation.Create(
            "S",
            [CSharpSyntaxTree.ParseText(Source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var holder = compilation.GetTypeByMetadataName("N.Holder")!;
        return holder.GetMembers(field).OfType<IFieldSymbol>().Single().Type;
    }

    [Fact]
    public void Captures_array_element()
    {
        var snapshot = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("Array")));

        Assert.True(snapshot.IsArray);
        Assert.Equal(1, snapshot.ArrayRank);
        Assert.NotNull(snapshot.ElementType);
        Assert.Equal("Item", snapshot.ElementType!.Name);
        Assert.True(snapshot.TypeArguments.IsEmpty);
        Assert.True(snapshot.HasCompleteShape);
    }

    [Fact]
    public void Captures_generic_arguments_and_original_definition()
    {
        var snapshot = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("Generic")));

        Assert.False(snapshot.IsArray);
        Assert.Single(snapshot.TypeArguments);
        Assert.Equal("Item", snapshot.TypeArguments[0].Name);
        Assert.Equal("Box`1", snapshot.OriginalDefinitionMetadataName);
        Assert.Equal("N.Box`1", snapshot.OriginalDefinitionQualifiedMetadataName);
        Assert.True(snapshot.HasCompleteShape);
    }

    [Fact]
    public void Different_type_arguments_are_not_equal_but_share_original_definition()
    {
        var withItem = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("Generic")));
        var withStore = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("OtherGeneric")));

        Assert.Equal("Box`1", withItem.OriginalDefinitionMetadataName);
        Assert.Equal("Box`1", withStore.OriginalDefinitionMetadataName);
        Assert.NotEqual(withItem, withStore);
    }

    [Fact]
    public void Void_is_flagged()
    {
        var snapshot = TypeSnapshot.Create(TypeDescriptor.Void());

        Assert.True(snapshot.IsVoid);
        Assert.False(snapshot.IsArray);
    }

    [Fact]
    public void Captures_multidimensional_and_jagged_array_shapes()
    {
        var matrix = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("Matrix")));
        var jagged = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("Jagged")));

        Assert.Equal(2, matrix.ArrayRank);
        Assert.Equal("Item", matrix.ElementType!.Name);
        Assert.Equal(1, jagged.ArrayRank);
        Assert.True(jagged.ElementType!.IsArray);
        Assert.Equal(1, jagged.ElementType.ArrayRank);
        Assert.Equal("Item", jagged.ElementType.ElementType!.Name);
    }

    [Fact]
    public void Qualified_metadata_identity_distinguishes_same_short_name()
    {
        var custom = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("CustomTask")));
        var framework = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("FrameworkTask")));

        Assert.Equal("Task`1", custom.OriginalDefinitionMetadataName);
        Assert.Equal("Task`1", framework.OriginalDefinitionMetadataName);
        Assert.Equal("N.Task`1", custom.OriginalDefinitionQualifiedMetadataName);
        Assert.Equal("System.Threading.Tasks.Task`1", framework.OriginalDefinitionQualifiedMetadataName);
        Assert.NotEqual(custom, framework);
    }

    [Fact]
    public void Captures_nested_generic_metadata_identity()
    {
        var snapshot = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("Nested")));

        Assert.Equal("N.Outer`1+Inner`1", snapshot.OriginalDefinitionQualifiedMetadataName);
        Assert.Single(snapshot.TypeArguments);
        Assert.Equal("Store", snapshot.TypeArguments[0].Name);
        Assert.NotNull(snapshot.ContainingType);
        Assert.Equal("N.Outer`1", snapshot.ContainingType!.OriginalDefinitionQualifiedMetadataName);
        Assert.Single(snapshot.ContainingType.TypeArguments);
        Assert.Equal("Item", snapshot.ContainingType.TypeArguments[0].Name);

        var other = TypeSnapshot.Create(TypeDescriptor.Create(FieldType("OtherNested")));
        Assert.NotEqual(snapshot, other);
    }

    [Fact]
    public void Symbol_less_complex_display_names_are_explicitly_incomplete()
    {
        var array = TypeSnapshot.Create(new TypeDescriptor("Sorting[]", ["N"]));
        var generic = TypeSnapshot.Create(new TypeDescriptor("Page<Sorting>", ["N"]));

        Assert.True(array.IsArray);
        Assert.Equal(0, array.ArrayRank);
        Assert.Null(array.ElementType);
        Assert.False(array.HasCompleteShape);
        Assert.Empty(generic.TypeArguments);
        Assert.False(generic.HasCompleteShape);
    }

    [Fact]
    public void Create_rejects_null_descriptor()
    {
        Assert.Throws<ArgumentNullException>(() => TypeSnapshot.Create(null!));
    }
}
