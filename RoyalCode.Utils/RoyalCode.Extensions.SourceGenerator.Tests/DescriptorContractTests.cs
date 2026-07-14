using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;
using RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

public class DescriptorContractTests
{
    [Fact]
    public void TypeDescriptor_equal_values_should_have_equal_hash_codes()
    {
        var left = CreateTypeDescriptor(isNullable: true);
        var right = CreateTypeDescriptor(isNullable: true);

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
        Assert.NotEqual(left, CreateTypeDescriptor(isNullable: false));
    }

    [Fact]
    public void TypeDescriptor_equality_should_handle_only_one_defined_property_list_being_null()
    {
        var withoutProperties = new TypeDescriptor("Widget", ["Domain"]);
        var withProperties = new TypeDescriptor("Widget", ["Domain"])
        {
            DefinedProperties = [],
        };

        Assert.NotEqual(withoutProperties, withProperties);
        Assert.NotEqual(withProperties, withoutProperties);
    }

    [Fact]
    public void PropertyDescriptor_object_equality_should_use_PropertyDescriptor()
    {
        object left = new PropertyDescriptor(new TypeDescriptor("int", ["System"]), "Id", null);
        object right = new PropertyDescriptor(new TypeDescriptor("int", ["System"]), "Id", null);

        Assert.True(left.Equals(right));
    }

    /// <summary>
    /// <para>
    ///     O modelo de trabalho do matching (<see cref="MatchSelection"/> e a árvore que ele carrega) não tem
    ///     igualdade de valor, e isso é deliberado: ele retém símbolos do Roslyn e tem estado mutável
    ///     (o <c>Parent</c> de um <see cref="PropertySelection"/> é atribuído depois da construção), então um
    ///     <c>GetHashCode</c> baseado nesse estado mudaria depois do objeto já ter sido usado como chave.
    /// </para>
    /// <para>
    ///     Quem precisa comparar entre builds — o pipeline incremental — deve usar
    ///     <c>MatchSelectionSnapshotFactory.Create</c>. Se este teste falhar, alguém devolveu
    ///     <see cref="IEquatable{T}"/> a esses tipos: reveja a decisão antes de mexer no teste.
    /// </para>
    /// </summary>
    [Fact]
    public void Matching_working_model_should_not_have_value_equality()
    {
        Assert.DoesNotContain(
            typeof(MatchSelection).GetInterfaces(),
            i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>));
        Assert.DoesNotContain(
            typeof(PropertyMatch).GetInterfaces(),
            i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>));
        Assert.DoesNotContain(
            typeof(PropertySelection).GetInterfaces(),
            i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>));
        Assert.DoesNotContain(
            typeof(AssignDescriptor).GetInterfaces(),
            i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>));

        var left = new MatchSelection(
            new TypeDescriptor("Origin", ["Domain"]),
            new TypeDescriptor("Target", ["Domain"]),
            new List<PropertyMatch>());
        var right = new MatchSelection(
            new TypeDescriptor("Origin", ["Domain"]),
            new TypeDescriptor("Target", ["Domain"]),
            new List<PropertyMatch>());

        Assert.NotEqual(left, right);
    }

    [Fact]
    public void Descriptor_factories_should_not_cache_compilation_symbols_statically()
    {
        var typeFields = typeof(TypeDescriptor).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
        var parameterFields = typeof(ParameterDescriptor).GetFields(BindingFlags.Static | BindingFlags.NonPublic);

        Assert.DoesNotContain(typeFields, field =>
            field.Name is "voidTypeDescriptor" or "cancellationToken");
        Assert.DoesNotContain(parameterFields, field => field.Name == "cancellationToken");

        var firstModel = CreateSemanticModel("FirstCompilation");
        var secondModel = CreateSemanticModel("SecondCompilation");

        Assert.NotSame(
            TypeDescriptor.Void(firstModel).Symbol?.ContainingAssembly,
            TypeDescriptor.Void(secondModel).Symbol?.ContainingAssembly);
        Assert.NotSame(
            TypeDescriptor.CancellationToken(firstModel).Symbol?.ContainingAssembly,
            TypeDescriptor.CancellationToken(secondModel).Symbol?.ContainingAssembly);
        Assert.NotSame(
            ParameterDescriptor.CancellationToken(firstModel).Type.Symbol?.ContainingAssembly,
            ParameterDescriptor.CancellationToken(secondModel).Type.Symbol?.ContainingAssembly);
    }

    private static TypeDescriptor CreateTypeDescriptor(bool isNullable)
    {
        return new TypeDescriptor("Widget", ["Domain", "Shared"], isNullable: isNullable)
        {
            DefinedProperties =
            [
                new PropertyDescriptor(new TypeDescriptor("int", ["System"]), "Id", null),
            ],
        };
    }

    private static SemanticModel CreateSemanticModel(string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("public class Marker { }");
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        return compilation.GetSemanticModel(syntaxTree);
    }
}
