using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class BaseParametersGenerator : GeneratorNode
{
    private List<string>? parameters;

    public void AddParamter(string parameter)
    {
        parameters ??= [];
        parameters.Add(parameter);
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        if (parameters is null)
            return;

        sb.AppendLine()
            .IndentPlus(indent)
            .Append(" : base (");

        bool first = true;
        foreach(var p in parameters)
        {
            if (first)
                first = false;
            else
                sb.Append(", ");
            sb.Append(p);
        }

        sb.Append(")");
    }
}