using System.Text;
using RoyalCode.Extensions.SourceGenerator.Generators;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

public class ClassGeneratorTests
{
    [Fact]
    public void Writes_containing_types_from_outermost_to_innermost()
    {
        var generator = new TestClassGenerator("EntityDetails", "Dtos");
        generator.Modifiers.Public();
        generator.Modifiers.Partial();

        var outer = new ContainingTypeGenerator("Outer");
        outer.Modifiers.Public();
        outer.Modifiers.Partial();
        generator.ContainingTypes.Add(outer);

        var inner = new ContainingTypeGenerator("Container");
        inner.Modifiers.Internal();
        inner.Modifiers.Partial();
        generator.ContainingTypes.Add(inner);

        var source = generator.Render().Replace("\r\n", "\n");

        Assert.Contains(
            "public partial class Outer\n{\n    internal partial class Container\n    {\n        public partial class EntityDetails",
            source);
    }

    private sealed class TestClassGenerator(string name, string ns) : ClassGenerator(name, ns)
    {
        public string Render()
        {
            var builder = new StringBuilder();
            Write(builder);
            return builder.ToString();
        }
    }
}
