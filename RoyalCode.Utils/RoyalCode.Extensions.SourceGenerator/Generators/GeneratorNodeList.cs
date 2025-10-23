using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class GeneratorNodeList : GeneratorNode, IWithNamespaces
{
    private List<GeneratorNode>? nodes;

    public bool InLine { get; set; }

    public void Add(GeneratorNode generator)
    {
        nodes ??= [];
        nodes.Add(generator);
    }

    public IEnumerable<T> Enumerate<T>() => nodes?.OfType<T>() ?? [];

    public int Count => nodes?.Count ?? 0;

    public IEnumerable<string> GetNamespaces()
    {
        if (nodes is null)
            yield break;
        foreach (var node in nodes)
            if (node is IWithNamespaces withNamespaces)
                foreach (var ns in withNamespaces.GetNamespaces())
                    yield return ns;
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        if (nodes is null)
            return;

        bool first = true;

        foreach (var node in nodes)
        {
            if (InLine)
            {
                if (!first)
                    sb.Append(" ");
                first = false;

                node.Write(sb, 0);
            }
            else
            {
                node.Write(sb, indent);
            }
        }
    }
}
