using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

/// <summary>
/// <para>
///     Represents a node for code generation.
/// </para>
/// <para>
///     A node can be a class, namespace, property, method, command, etc.
///     <br />
///     A node is a complete piece of code. For divisions in snippets, there is the <see cref="ValueNode"/>.
/// </para>
/// </summary>
public abstract class GeneratorNode
{
    public abstract void Write(StringBuilder sb, int indent = 0);
}
