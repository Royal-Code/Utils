using RoyalCode.Extensions.SourceGenerator.Descriptors;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

#pragma warning disable S2376 // Set method

public class HierarchyGenerator : GeneratorNode, IWithNamespaces
{
    private TypeDescriptor? extends;
    private List<TypeDescriptor>? implements;

    public TypeDescriptor Extends { set => extends = value; }

    public void AddImplements(TypeDescriptor type)
    {
        implements ??= [];

        if (implements.Contains(type))
            return;

        implements.Add(type);
    }

    public IEnumerable<string> GetNamespaces()
    {
        if (extends is not null)
            foreach (var ns in extends.Namespaces)
                yield return ns;

        if (implements is not null)
            foreach (var ns in implements.SelectMany(t => t.Namespaces))
                yield return ns;
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        if (extends is null && implements is null)
            return;

        sb.Append(" : ");

        if (extends is not null)
            sb.Append(extends.Name);

        if (implements is null)
            return;

        if (extends is not null)
            sb.Append(", ");

        sb.Append(string.Join(", ", implements.Select(t => t.Name)));
    }
}