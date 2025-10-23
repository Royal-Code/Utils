using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class ArgumentsGenerator : GeneratorNode, IWithNamespaces
{
    private List<ValueNode>? arguments;

    public bool InLine { get; set; } = true;

    public void AddArgument(ValueNode argument)
    {
        arguments ??= [];
        arguments.Add(argument);
    }
    
    public void AddArguments(ValueNode[] arguments)
    {
        this.arguments ??= [];
        this.arguments.AddRange(arguments);
    }

    public IEnumerable<string> GetNamespaces()
    {
        if (arguments is null)
            yield break;
        foreach (var arg in arguments)
            if (arg is IWithNamespaces withNamespaces)
                foreach (var ns in withNamespaces.GetNamespaces())
                    yield return ns;
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.Append("(");
        if (arguments is null)
        {
            sb.Append(")");
            return;
        }

        bool first = true;
        foreach (var arg in arguments)
        {
            if (first)
            {
                if (!InLine)
                    sb.AppendLine().IdentPlus(indent);
                first = false;
            }
            else
            {
                if (!InLine)
                    sb.AppendLine(",").IdentPlus(indent);
                else
                    sb.Append(", ");
            }

            sb.Append(arg.GetValue(indent));
        }

        sb.Append(")");
    }
}