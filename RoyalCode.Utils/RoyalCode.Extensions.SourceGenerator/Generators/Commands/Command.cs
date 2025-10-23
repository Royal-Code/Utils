using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators.Commands;

public sealed class Command : GeneratorNode
{
    private readonly GeneratorNode generatorNode;

    public Command(GeneratorNode generatorNode)
    {
        this.generatorNode = generatorNode;
    }

    public bool Await { get; set; } = false;

    public bool NewLine { get; set; } = true;

    public bool InLine { get; set; }

    public bool Idented { get; set; } = true;

    public override void Write(StringBuilder sb, int indent = 0)
    {
        if (Idented)
            sb.Ident(indent);

        if (Await)
            sb.Append("await ");

        generatorNode.Write(sb, indent);

        if (InLine)
        {
            sb.Append(";");
        }
        else
        {
            sb.AppendLine(";");
            if (NewLine)
                sb.AppendLine();
        }
    }
}