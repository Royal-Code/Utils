namespace RoyalCode.Extensions.SourceGenerator.Generators;

/// <summary>
///     Represents an abstract base class for nodes that encapsulate a value.
/// </summary>
/// <remarks>
///     The <see cref="ValueNode"/> class provides a common interface for derived classes that represent
///     specific types of values. It supports implicit conversion from <see langword="string"/> and 
///     <see cref="GeneratorNode"/> to their respective derived types.
/// </remarks>
public abstract class ValueNode
{
    public static implicit operator ValueNode(string v) => new StringValueNode(v);
    public static implicit operator ValueNode(GeneratorNode g) => new GeneratorValueNode(g);

    public abstract string GetValue(int ident);
}