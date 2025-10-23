using RoyalCode.Extensions.SourceGenerator.Descriptors;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class ParameterGenerator : GeneratorNode, IWithNamespaces
{
    private readonly ParameterDescriptor parameter;
    private readonly ValueNode? defaultValue;
    private GeneratorNodeList? attributes;

    public ParameterGenerator(ParameterDescriptor parameter, ValueNode? defaultValue = null)
    {
        this.parameter = parameter;
        this.defaultValue = defaultValue;
    }

    public bool ThisModifier { get; set; }

    public GeneratorNodeList Attributes => attributes ??= new();

    public ParameterDescriptor ParameterDescriptor => parameter;

    public ParameterGenerator Clone()
    {
        return new ParameterGenerator(parameter, defaultValue)
        {
            ThisModifier = ThisModifier
        };
    }

    public IEnumerable<string> GetNamespaces()
    {
        foreach (var ns in parameter.Type.Namespaces)
            yield return ns;
        if (defaultValue is IWithNamespaces wns)
            foreach (var ns in wns.GetNamespaces())
                yield return ns;
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        if (attributes is not null && attributes.Count > 0)
        {
            attributes.Write(sb, indent);
            sb.Append(' ');
        }

        if (ThisModifier)
            sb.Append("this ");

        sb.Append(parameter.Type.Name).Append(' ').Append(parameter.Name);
        if (defaultValue is not null)
            sb.Append(" = ").Append(defaultValue.GetValue(indent));
    }
}