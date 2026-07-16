using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RoyalCode.Extensions.SourceGenerator.Collections;
using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Snapshots;
using RoyalCode.Extensions.SourceGenerator.Diagnostics;
using RoyalCode.Extensions.SourceGenerator.Generation;

#pragma warning disable RS2008 // Test-only descriptors are not analyzer rules and need no release tracking.

namespace RoyalCode.Extensions.SourceGenerator.Tests;

public class EquatableArrayTests
{
    [Fact]
    public void Default_and_empty_are_equal_and_share_hash()
    {
        EquatableArray<string> byDefault = default;
        var fromEmptyArray = new EquatableArray<string>(Array.Empty<string>());
        var fromNull = new EquatableArray<string>((string[]?)null);

        Assert.True(byDefault.IsEmpty);
        Assert.Empty(byDefault);
        Assert.Equal(byDefault, EquatableArray<string>.Empty);
        Assert.Equal(byDefault, fromEmptyArray);
        Assert.Equal(byDefault, fromNull);
        Assert.Equal(byDefault.GetHashCode(), fromEmptyArray.GetHashCode());
        Assert.Equal(byDefault.GetHashCode(), fromNull.GetHashCode());
        Assert.Equal(0, byDefault.GetHashCode());
    }

    [Fact]
    public void Equality_is_by_content_and_order()
    {
        var a = new EquatableArray<string>(new[] { "a", "b", "c" });
        var b = new EquatableArray<string>(new[] { "a", "b", "c" });
        var reordered = new EquatableArray<string>(new[] { "a", "c", "b" });
        var shorter = new EquatableArray<string>(new[] { "a", "b" });

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.True(a == b);
        Assert.NotEqual(a, reordered);
        Assert.NotEqual(a, shorter);
        Assert.True(a != shorter);
    }

    [Fact]
    public void Exposes_count_indexer_and_enumeration()
    {
        var a = new EquatableArray<string>(new[] { "x", "y" });

        Assert.Equal(2, a.Count);
        Assert.Equal("y", a[1]);
        Assert.Equal(new[] { "x", "y" }, a.ToArray());
        Assert.Equal(2, a.AsImmutableArray().Length);
    }

    [Fact]
    public void Copies_source_array_so_content_and_hash_cannot_be_mutated()
    {
        var source = new[] { "a", "b" };
        var value = new EquatableArray<string>(source);
        var hash = value.GetHashCode();

        source[0] = "changed";

        Assert.Equal(new[] { "a", "b" }, value.ToArray());
        Assert.Equal(hash, value.GetHashCode());
        Assert.Equal(new EquatableArray<string>(new[] { "a", "b" }), value);
    }
}

public class GenerationCandidateTests
{
    private sealed record Model(string Value) : IEquatable<Model>;

    [Fact]
    public void Valid_carries_model_and_is_valid()
    {
        var candidate = GenerationCandidate<Model>.Valid(new Model("a"));

        Assert.True(candidate.IsValid);
        Assert.Equal(new Model("a"), candidate.Model);
        Assert.True(candidate.Diagnostics.IsEmpty);
    }

    [Fact]
    public void Invalid_has_no_model_but_keeps_diagnostics()
    {
        var diagnostic = DiagnosticInfo.Create(Descriptor, Location.None);
        var candidate = GenerationCandidate<Model>.Invalid(diagnostic);

        Assert.False(candidate.IsValid);
        Assert.Null(candidate.Model);
        Assert.Single(candidate.Diagnostics);
    }

    [Fact]
    public void Default_and_empty_invalid_candidates_are_silent_and_do_not_emit()
    {
        GenerationCandidate<Model> byDefault = default;
        var explicitInvalid = GenerationCandidate<Model>.Invalid(default(EquatableArray<DiagnosticInfo>));

        Assert.False(byDefault.IsValid);
        Assert.Null(byDefault.Model);
        Assert.True(byDefault.Diagnostics.IsEmpty);
        Assert.Equal(byDefault, explicitInvalid);
    }

    [Fact]
    public void Invalid_single_diagnostic_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => GenerationCandidate<Model>.Invalid(null!));
    }

    [Fact]
    public void Equality_considers_model_and_diagnostics()
    {
        var a = GenerationCandidate<Model>.Valid(new Model("a"));
        var b = GenerationCandidate<Model>.Valid(new Model("a"));
        var c = GenerationCandidate<Model>.Valid(new Model("b"));

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, c);
    }

    private static readonly DiagnosticDescriptor Descriptor = new(
        "TST001", "t", "t", "t", DiagnosticSeverity.Error, true);
}

public class DiagnosticInfoTests
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        "TST002", "title", "message {0}", "cat", DiagnosticSeverity.Error, true);

    [Fact]
    public void Roundtrips_through_resolver_without_location()
    {
        var info = DiagnosticInfo.Create(Descriptor, Location.None, "arg");

        Assert.False(info.HasLocation);
        Assert.Equal("TST002", info.Id);
        Assert.Equal(new[] { "arg" }, info.Arguments.ToArray());

        var diagnostic = info.ToDiagnostic(_ => Descriptor);
        Assert.Equal("TST002", diagnostic.Id);
        Assert.Equal(Location.None, diagnostic.Location);
    }

    [Fact]
    public void Preserves_location_span_and_path_through_roundtrip()
    {
        var tree = CSharpSyntaxTree.ParseText("class C { }", path: "C.cs");
        var location = Location.Create(tree, new TextSpan(6, 1));

        var info = DiagnosticInfo.Create(Descriptor, location);

        Assert.True(info.HasLocation);
        Assert.Equal(new TextSpan(6, 1), info.SourceSpan);
        Assert.Equal("C.cs", info.FilePath);

        // A localização é reconstruída como ExternalFileLocation (não vinculada à SyntaxTree),
        // preservando arquivo e span — que é o contrato deste DTO symbol-free.
        var diagnostic = info.ToDiagnostic(_ => Descriptor);
        Assert.Equal(new TextSpan(6, 1), diagnostic.Location.SourceSpan);
        Assert.Equal("C.cs", diagnostic.Location.GetLineSpan().Path);
    }

    [Fact]
    public void Equality_by_id_arguments_and_location()
    {
        var a = DiagnosticInfo.Create(Descriptor, Location.None, "x");
        var b = DiagnosticInfo.Create(Descriptor, Location.None, "x");
        var differentArg = DiagnosticInfo.Create(Descriptor, Location.None, "y");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, differentArg);
    }

    [Fact]
    public void ToDiagnostic_requires_resolver()
    {
        var info = DiagnosticInfo.Create(Descriptor, Location.None);
        Assert.Throws<ArgumentNullException>(() => info.ToDiagnostic(null!));
    }

    [Fact]
    public void Create_rejects_null_descriptor()
    {
        Assert.Throws<ArgumentNullException>(() => DiagnosticInfo.Create(null!, Location.None));
    }
}

public class TypeUsageSnapshotTests
{
    [Fact]
    public void Same_type_different_roles_are_not_equal_but_share_structural_snapshot()
    {
        var descriptor = new TypeDescriptor("Product", new[] { "Shop" });

        var asEntity = TypeUsageSnapshot.Create(descriptor, TypeUsageRoles.Entity);
        var asContext = TypeUsageSnapshot.Create(descriptor, TypeUsageRoles.Context);

        Assert.True(asEntity.Type.Equals(asContext.Type)); // estrutural igual
        Assert.NotEqual(asEntity, asContext);               // uso diferente
        Assert.True(asEntity.IsEntity);
        Assert.False(asEntity.IsContext);
    }

    [Fact]
    public void Same_type_same_roles_are_equal()
    {
        var descriptor = new TypeDescriptor("Product", new[] { "Shop" });

        var a = TypeUsageSnapshot.Create(descriptor, TypeUsageRoles.Entity | TypeUsageRoles.HandlerParameter);
        var b = TypeUsageSnapshot.Create(descriptor, TypeUsageRoles.HandlerParameter | TypeUsageRoles.Entity);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void CreateFromHints_reads_descriptor_hints_into_roles()
    {
        var descriptor = new TypeDescriptor("Product", new[] { "Shop" });
        descriptor.MarkAsEntity();
        descriptor.MarkAsHandlerParameter();

        var usage = TypeUsageSnapshot.CreateFromHints(descriptor);

        Assert.True(usage.IsEntity);
        Assert.True(usage.IsHandlerParameter);
        Assert.False(usage.IsContext);
        Assert.False(usage.IsCollectionOfEntities);
    }

    [Fact]
    public void Factories_reject_null_descriptor()
    {
        Assert.Throws<ArgumentNullException>(() => TypeUsageSnapshot.Create(null!, TypeUsageRoles.None));
        Assert.Throws<ArgumentNullException>(() => TypeUsageSnapshot.CreateFromHints(null!));
    }
}

public class DescriptorSnapshotValidationTests
{
    private static TypeUsageSnapshot Usage() =>
        TypeUsageSnapshot.Create(new TypeDescriptor("Product", ["Shop"]), TypeUsageRoles.None);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParameterSnapshot_rejects_invalid_name(string? name)
    {
        Assert.Throws<ArgumentException>(() => new ParameterSnapshot(Usage(), name!));
    }

    [Fact]
    public void ParameterSnapshot_rejects_null_type_usage()
    {
        Assert.Throws<ArgumentNullException>(() => new ParameterSnapshot(null!, "product"));
    }

    [Fact]
    public void PropertySnapshot_rejects_null_type_and_invalid_name()
    {
        Assert.Throws<ArgumentNullException>(() => new PropertySnapshot(null!, "Name"));
        Assert.Throws<ArgumentException>(() => new PropertySnapshot(Usage().Type, " "));
    }

    [Fact]
    public void PropertyPath_rejects_null_empty_and_null_items()
    {
        Assert.Throws<ArgumentNullException>(() => new PropertyPathSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new PropertyPathSnapshot([]));
        Assert.Throws<ArgumentException>(() => new PropertyPathSnapshot([null!]));
    }

    [Fact]
    public void MatchSelectionSnapshot_rejects_null_contract_members()
    {
        var type = Usage().Type;

        Assert.Throws<ArgumentNullException>(() => new MatchSelectionSnapshot(null!, type, []));
        Assert.Throws<ArgumentNullException>(() => new MatchSelectionSnapshot(type, null!, []));
        Assert.Throws<ArgumentNullException>(() => new MatchSelectionSnapshot(type, type, null!));
        Assert.Throws<ArgumentException>(() => new MatchSelectionSnapshot(type, type, [null!]));
        Assert.Throws<ArgumentNullException>(() => MatchSelectionSnapshotFactory.Create(null!));
    }
}
