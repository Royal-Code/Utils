using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators.Commands;

public class AssignValueCommand : GeneratorNode
{
    public static AssignValueCommand CreateParameterAssignField(string parameterName, bool useThis = true)
    {
        return useThis
            ? new AssignValueCommand($"this.{parameterName}", parameterName)
            : new AssignValueCommand($"_{parameterName}", parameterName);
    }

    private readonly ValueNode left;
    private readonly ValueNode right;

    public bool AppendLine { get; set; } = false;

    public AssignValueCommand(ValueNode left, ValueNode right)
    {
        this.left = left;
        this.right = right;
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.Ident(indent).Append(left.GetValue(indent)).Append(" = ").Append(right.GetValue(indent)).AppendLine(";");

        if (AppendLine)
            sb.AppendLine();
    }
}
