using RoyalCode.Extensions.SourceGenerator.Descriptors;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators.Commands;

public class ThrowCommand : GeneratorNode, IWithNamespaces
{
    public static ThrowCommand NotImplementedException()
    {
        return new ThrowCommand(new TypeDescriptor("NotImplementedException", []));
    }

    private readonly TypeDescriptor exceptionType;
    private ParametersGenerator? parameters;

    public ThrowCommand(TypeDescriptor exceptionType)
    {
        this.exceptionType = exceptionType;
    }

    public ParametersGenerator Parameters => parameters ??= new ParametersGenerator();

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.Indent(indent);
        sb.Append("throw new ").Append(exceptionType.Name);
        if (parameters is null)
            sb.Append("()");
        else
            parameters.Write(sb, indent);
        sb.AppendLine(";");
    }

    public IEnumerable<string> GetNamespaces()
    {
        if (exceptionType is not null)
            foreach (var ns in exceptionType.Namespaces)
                yield return ns;
        if (parameters is not null)
            foreach (var ns in parameters.GetNamespaces())
                yield return ns;
    }
}
