using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators.Commands;

public sealed class IfCommand : GeneratorNode
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

    public override void Write(StringBuilder sb, int ident = 0)
    {
        int idented = Idented ? ident : 0;

        sb.Append("if (").Append(condition.GetValue(idented)).Append(")");

        if (commands is null || commands.Count is 0)
        {
            sb.AppendLine(" { }");
        }
        else if (commands.Count is 1)
        {
            sb.AppendLine();
            commands.Write(sb, idented + 1);
        }
        else
        {
            sb.AppendLine().Ident(idented).AppendLine("{");
            commands.Write(sb, idented + 1);
            sb.Ident(idented).AppendLine("}");
        }

        if (elseCommands is not null && elseCommands.Count > 0)
        {
            sb.Ident(idented);

            sb.AppendLine("else");

            if (elseCommands.Count is 1)
            {
                elseCommands.Write(sb, idented + 1);
            }
            else
            {
                sb.Ident(idented).AppendLine("{");
                elseCommands.Write(sb, idented + 1);
                sb.Ident(idented).AppendLine("}");
            }
        }

        if (NewLine)
            sb.AppendLine();
    }
}
