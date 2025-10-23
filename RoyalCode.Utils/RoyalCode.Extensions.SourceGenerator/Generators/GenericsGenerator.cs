using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class GenericsGenerator : GeneratorNode, IWithNamespaces
{
    private List<string>? generics;
    private List<string>? namespaces;

    public void AddGeneric(string typeName)
    {
        generics ??= [];
        generics.Add(typeName);
    }

    public void AddGeneric(string typeName, string[] typeNamespace)
    {
        generics ??= [];
        generics.Add(typeName);

        namespaces ??= [];
        namespaces.AddRange(typeNamespace);
    }

    public IEnumerable<string> GetNamespaces()
    {
        if (namespaces is not null)
        {
            foreach (var ns in namespaces)
                yield return ns;
        }
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        if (generics is null)
            return;

        sb.Append("<");
        sb.Append(string.Join(", ", generics));
        sb.Append(">");
    }
}