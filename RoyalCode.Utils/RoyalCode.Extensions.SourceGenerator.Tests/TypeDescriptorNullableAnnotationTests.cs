using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoyalCode.Extensions.SourceGenerator.Descriptors;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

public class TypeDescriptorNullableAnnotationTests
{
    private const string Source =
        """
        #nullable enable

        namespace Domain;

        public class Person
        {
            public string Name { get; set; } = string.Empty;
            public string? Nick { get; set; }
            public int? Age { get; set; }
        }
        """;

    [Fact]
    public void Create_from_symbol_should_capture_the_nullable_annotation()
    {
        var person = GetPersonSymbol();

        var name = CreatePropertyType(person, "Name");
        var nick = CreatePropertyType(person, "Nick");

        // tipos especiais já carregavam o '?' no nome; a anotação agora é dado do descritor
        Assert.Equal("string", name.Name);
        Assert.Equal(NullableAnnotation.NotAnnotated, name.NullableAnnotation);

        Assert.Equal("string?", nick.Name);
        Assert.Equal(NullableAnnotation.Annotated, nick.NullableAnnotation);
        Assert.False(nick.IsNullable);
    }

    [Fact]
    public void Create_from_symbol_should_keep_nullable_value_type_behavior()
    {
        var person = GetPersonSymbol();

        var age = CreatePropertyType(person, "Age");

        Assert.Equal("Int32?", age.Name);
        Assert.True(age.IsNullable);
        Assert.Equal(NullableAnnotation.Annotated, age.NullableAnnotation);
    }

    [Fact]
    public void Descriptors_differing_only_by_nullable_annotation_should_not_be_equal()
    {
        var annotated = new TypeDescriptor("string", ["System"], nullableAnnotation: NullableAnnotation.Annotated);
        var notAnnotated = new TypeDescriptor("string", ["System"], nullableAnnotation: NullableAnnotation.NotAnnotated);
        var annotatedAgain = new TypeDescriptor("string", ["System"], nullableAnnotation: NullableAnnotation.Annotated);

        Assert.NotEqual(annotated, notAnnotated);
        Assert.Equal(annotated, annotatedAgain);
        Assert.Equal(annotated.GetHashCode(), annotatedAgain.GetHashCode());
    }

    [Fact]
    public void Create_from_syntax_should_derive_the_annotation_from_the_type_text()
    {
        Util.Compile(Source, out var compilation, out _);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var properties = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

        var nick = TypeDescriptor.Create(properties.Single(p => p.Identifier.Text == "Nick").Type!, model);
        var name = TypeDescriptor.Create(properties.Single(p => p.Identifier.Text == "Name").Type!, model);

        Assert.Equal(NullableAnnotation.Annotated, nick.NullableAnnotation);
        Assert.Equal(NullableAnnotation.None, name.NullableAnnotation);
    }

    private static INamedTypeSymbol GetPersonSymbol()
    {
        Util.Compile(Source, out var compilation, out _);
        return compilation.GetTypeByMetadataName("Domain.Person")
            ?? throw new InvalidOperationException("Person type not found.");
    }

    private static TypeDescriptor CreatePropertyType(INamedTypeSymbol type, string propertyName)
    {
        var property = type.GetMembers(propertyName).OfType<IPropertySymbol>().Single();
        return TypeDescriptor.Create(property.Type);
    }
}
