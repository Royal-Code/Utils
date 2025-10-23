using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators.Commands;

public class ReturnCommand : GeneratorNode
{
    private readonly ValueNode valueNode;

    public bool AppendLine { get; set; } = true;

    public ReturnCommand(ValueNode valueNode)
    {
        this.valueNode = valueNode ?? throw new ArgumentNullException(nameof(valueNode));
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.Indent(indent).Append("return ").Append(valueNode.GetValue(indent)).Append(";");

        if (AppendLine)
            sb.AppendLine();
    }
}
