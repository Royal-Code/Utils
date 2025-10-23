using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators.Commands;

public class ValidateHasProblemsCommand : GeneratorNode
{
    private readonly string identifier;

    public ValidateHasProblemsCommand(string identifier)
    {
        this.identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.Ident(indent);
        sb.Append("if (");
        sb.Append(identifier).Append(".HasProblems(out var validationProblems)");
        sb.AppendLine(")");
        sb.IdentPlus(indent);
        sb.AppendLine("return validationProblems;");
        sb.AppendLine();
    }
}
