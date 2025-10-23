using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class MethodInvokeGenerator : GeneratorNode, IWithNamespaces
{
    private readonly ValueNode identifier;
    private readonly string method;
    private readonly ArgumentsGenerator arguments;

    public MethodInvokeGenerator(ValueNode identifier, string method, ArgumentsGenerator? arguments = null)
    {
        this.identifier = identifier;
        this.method = method;
        this.arguments = arguments ?? new();
    }

    public MethodInvokeGenerator(ValueNode identifier, string method, ValueNode argument)
    {
        this.identifier = identifier;
        this.method = method;
        
        arguments = new();
        arguments.AddArgument(argument);
    }

    public bool Await { get; set; }

    public bool LineIdent { get; set; }

    public void AddArgument(ValueNode arg) => arguments.AddArgument(arg);

    public IEnumerable<string> GetNamespaces()
    {
        if (identifier is IWithNamespaces withNamespaces)
            foreach (var ns in withNamespaces.GetNamespaces())
                yield return ns;
        foreach (var ns in arguments.GetNamespaces())
            yield return ns;
    }

    public void UseArgumentPerLine() => arguments.InLine = false;

    public override void Write(StringBuilder sb, int indent = 0)
    {
        if (Await)
            sb.Append("await ");

        sb.Append(identifier.GetValue(indent));

        if (LineIdent)
            sb.AppendLine().IdentPlus(indent);

        sb.Append(".").Append(method);

        arguments.Write(sb, indent + (LineIdent ? 1: 0));
    }
}