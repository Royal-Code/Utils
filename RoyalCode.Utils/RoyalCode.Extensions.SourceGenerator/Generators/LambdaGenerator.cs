using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class LambdaGenerator : GeneratorNode
{
    private GeneratorNodeList? commands;

    public ArgumentsGenerator Parameters { get; } = new();

    public GeneratorNodeList Commands => commands ??= new();

    public bool Async { get; set; }

    public bool Block { get; set; }

    public bool InLine { get; set; }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        if (Async)
            sb.Append("async ");

        Parameters.Write(sb, indent);

        sb.Append(" => ");

        if (commands is null)
        {
            sb.Append("{ }");
            return;
        }

        if (Block)
        {
            if (InLine)
            {
                sb.Append("{ ");
                commands.InLine = true;
                commands.Write(sb);
                sb.Append(" }");
            }
            else
            {
                sb.AppendLine();
                sb.Indent(indent).AppendLine("{");
                commands.Write(sb, indent + 1);
                sb.Indent(indent).AppendLine("}");
            }
        }
        else
        {
            commands.Write(sb, indent);
        }
    }
}