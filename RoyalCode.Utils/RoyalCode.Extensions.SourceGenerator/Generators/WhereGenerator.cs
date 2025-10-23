using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class WhereGenerator : GeneratorNode
{
    public WhereGenerator(string argument, string type)
    {
        Argument = argument;
        Type = type;
    }

    public string Argument { get; set; }

    public string Type { get; set; }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.AppendLine()
            .Indent(indent)
            .Append("where ").Append(Argument).Append(" : ").Append(Type);
    }
}