using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class GeneratorValueNode : ValueNode, IWithNamespaces
{
    private readonly GeneratorNode generatorNode;
    private string? cachedValue;

    public GeneratorValueNode(GeneratorNode generatorNode)
    {
        this.generatorNode = generatorNode;
    }

    public IEnumerable<string> GetNamespaces()
    {
        if (generatorNode is IWithNamespaces withNamespaces)
            foreach (var ns in withNamespaces.GetNamespaces())
                yield return ns;
    }

    public override string GetValue(int ident)
    {
        if (cachedValue is not null)
            return cachedValue;

        var sb = new StringBuilder();
        generatorNode.Write(sb, ident);
        cachedValue = sb.ToString();

        return cachedValue;
    }
}