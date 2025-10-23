using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators.Commands;

public sealed class IfCommand : GeneratorNode, IWithNamespaces
{
    private readonly ValueNode condition;
    private GeneratorNodeList? commands;
    private GeneratorNodeList? elseCommands;

    public IfCommand(ValueNode condition)
    {
        this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
    }

    public bool Idented { get; set; } = true;

    public bool NewLine { get; set; } = true;

    public void AddCommand(GeneratorNode command)
    {
        commands ??= new GeneratorNodeList();
        commands.Add(command);
    }

    public void AddElseCommand(GeneratorNode command)
    {
        elseCommands ??= new GeneratorNodeList();
        elseCommands.Add(command);
    }

    public IEnumerable<string> GetNamespaces()
    {
        if (commands is not null)
        {
            foreach (var ns in commands.GetNamespaces())
                yield return ns;
        }
        if (elseCommands is not null)
        {
            foreach (var ns in elseCommands.GetNamespaces())
                yield return ns;
        }
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        int localIdent = Idented ? indent : 0;

        sb.Indent(localIdent);
        sb.Append("if (").Append(condition.GetValue(localIdent)).Append(")");

        if (commands is null || commands.Count is 0)
        {
            sb.AppendLine(" { }");
        }
        else if (commands.Count is 1)
        {
            sb.AppendLine();
            commands.Write(sb, localIdent + 1);
        }
        else
        {
            sb.AppendLine().Indent(localIdent).AppendLine("{");
            commands.Write(sb, localIdent + 1);
            sb.Indent(localIdent).AppendLine("}");
        }

        if (elseCommands is not null && elseCommands.Count > 0)
        {
            sb.Indent(localIdent);

            sb.AppendLine("else");

            if (elseCommands.Count is 1)
            {
                elseCommands.Write(sb, localIdent + 1);
            }
            else
            {
                sb.Indent(localIdent).AppendLine("{");
                elseCommands.Write(sb, localIdent + 1);
                sb.Indent(localIdent).AppendLine("}");
            }
        }

        if (NewLine)
            sb.AppendLine();
    }
}
