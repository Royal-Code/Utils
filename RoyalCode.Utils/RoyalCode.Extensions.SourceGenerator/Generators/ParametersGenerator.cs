using RoyalCode.Extensions.SourceGenerator.Descriptors;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class ParametersGenerator : GeneratorNode, IWithNamespaces
{
    private List<ParameterGenerator>? parameters;

    public bool InLine { get; set; } = true;

    public void Add(ParameterGenerator parameter)
    {
        parameters ??= [];
        parameters.Add(parameter);
    }

    public void Add(IEnumerable<ParameterGenerator> parameters)
    {
        this.parameters ??= [];
        foreach (var p in parameters)
            this.parameters.Add(p);
    }

    public bool Any() => parameters?.Count > 0;

    public IEnumerable<ParameterDescriptor> GetDescriptors()
    {
        return parameters is null
            ? []
            : parameters.Select(p => p.ParameterDescriptor);
    }

    public ParametersGenerator Clone()
    {
        var newParameters = new ParametersGenerator()
        {
            InLine = InLine
        };

        if (parameters is null)
            return newParameters;

        newParameters.parameters = [];
        foreach (var p in parameters)
            newParameters.parameters.Add(p.Clone());

        return newParameters;
    }

    public IEnumerable<string> GetNamespaces()
    {
        if (parameters is not null)
            foreach (var p in parameters)
                foreach (var ns in p.GetNamespaces())
                    yield return ns;
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.Append('(');
        if (parameters is not null)
        {
            if (!InLine)
                sb.AppendLine().IdentPlus(indent);

            var first = true;
            foreach(var p in parameters)
            {
                if (first)
                    first = false;
                else
                {
                    sb.Append(", ");
                    if (!InLine)
                        sb.AppendLine().IdentPlus(indent);
                }

                p.Write(sb);
            }
        }
        sb.Append(')');
    }
}