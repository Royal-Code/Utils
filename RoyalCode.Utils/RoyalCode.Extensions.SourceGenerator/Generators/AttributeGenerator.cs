using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class AttributeGenerator : GeneratorNode, IWithNamespaces
{
    private readonly string name;
    private readonly string[] namespaces;
    private ArgumentsGenerator? arguments;

    public AttributeGenerator(string name, string[] namespaces)
    {
        this.name = name;
        this.namespaces = namespaces;
    }
    
    public AttributeGenerator(string name, ValueNode argument, string[] namespaces)
    {
        this.name = name;
        this.namespaces = namespaces;
        Arguments.AddArgument(argument);
    }
    
    public AttributeGenerator(string name, string[] namespaces, params ValueNode[] arguments)
    {
        this.name = name;
        this.namespaces = namespaces;
        Arguments.AddArguments(arguments);
    }

    public ArgumentsGenerator Arguments => arguments ??= new();

    public IEnumerable<string> GetNamespaces()
    {
        for (var i = 0; i < namespaces.Length; i++)
            yield return namespaces[i];
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.Ident(indent)
            .Append('[')
            .Append(name);
        
        arguments?.Write(sb);
        
        sb.Append(']').AppendLine();
    }
}