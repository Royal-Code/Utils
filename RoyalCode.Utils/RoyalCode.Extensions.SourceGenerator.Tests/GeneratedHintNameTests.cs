using System.Text.RegularExpressions;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

public class GeneratedHintNameTests
{
    [Fact]
    public void Create_matches_the_stable_sha256_base32_vector()
    {
        var hintName = GeneratedHintName.Create(
            "RoyalCode.SmartCommands.Demo.Commands.Pedidos.ItemPedidoPorSku|AutoSelect",
            "ItemPedidoPorSku_AutoSelect");

        Assert.Equal("ItemPedidoPorSku_AutoSelect.DWHMMPCV.g.cs", hintName);
    }

    [Fact]
    public void Create_is_deterministic_for_the_same_inputs()
    {
        var first = GeneratedHintName.Create("Domain.Container`1.Details|Extensions", "Details_Extensions");
        var second = GeneratedHintName.Create("Domain.Container`1.Details|Extensions", "Details_Extensions");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Complete_identity_changes_the_hash_without_changing_the_readable_prefix()
    {
        var first = GeneratedHintName.Create("First.Namespace.Details|AutoSelect", "Details_AutoSelect");
        var second = GeneratedHintName.Create("Second.Namespace.Details|AutoSelect", "Details_AutoSelect");

        Assert.NotEqual(first, second);
        Assert.StartsWith("Details_AutoSelect.", first, StringComparison.Ordinal);
        Assert.StartsWith("Details_AutoSelect.", second, StringComparison.Ordinal);
    }

    [Fact]
    public void Hash_depends_only_on_identity()
    {
        var first = GeneratedHintName.Create("Domain.Details|AutoSelect", "Details_AutoSelect");
        var second = GeneratedHintName.Create("Domain.Details|AutoSelect", "Renamed_AutoSelect");

        Assert.Equal(HashOf(first), HashOf(second));
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Readable_name_is_sanitized_and_hash_is_file_name_safe_base32()
    {
        var hintName = GeneratedHintName.Create("Domain.Map Things/Api:*?", "Map Things/Api:*?");

        Assert.StartsWith("Map_Things_Api___.", hintName, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"^[\p{L}\p{Nd}_-]+\.[A-Z2-7]{8}\.g\.cs$"), hintName);
    }

    [Fact]
    public void Readable_name_and_total_file_name_have_bounded_length()
    {
        var hintName = GeneratedHintName.Create("Domain." + new string('X', 200), new string('X', 200));

        Assert.StartsWith(new string('X', 32) + ".", hintName, StringComparison.Ordinal);
        Assert.Equal(46, hintName.Length);
    }

    [Fact]
    public void Representative_distinct_identities_do_not_collide()
    {
        var hintNames = Enumerable.Range(0, 1_000)
            .Select(index => GeneratedHintName.Create($"Domain.Feature{index}.Details|AutoSelect", "Details_AutoSelect"))
            .ToArray();

        Assert.Equal(hintNames.Length, hintNames.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_invalid_identity(string? identity)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            GeneratedHintName.Create(identity!, "Details_AutoSelect"));

        Assert.Equal("identity", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_invalid_readable_name(string? readableName)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            GeneratedHintName.Create("Domain.Details|AutoSelect", readableName!));

        Assert.Equal("readableName", exception.ParamName);
    }

    private static string HashOf(string hintName)
    {
        var suffixLength = ".g.cs".Length;
        return hintName.Substring(hintName.Length - suffixLength - 8, 8);
    }
}
