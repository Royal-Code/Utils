using RoyalCode.Extensions.SourceGenerator.Descriptors;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators.Commands;

public class AddServiceCommand : GeneratorNode, IWithNamespaces
{
    private readonly ServiceTypeDescriptor serviceTypeDescriptor;
    private readonly string servicesVarName;

    public AddServiceCommand(ServiceTypeDescriptor serviceTypeDescriptor, string servicesVarName)
    {
        this.serviceTypeDescriptor = serviceTypeDescriptor;
        this.servicesVarName = servicesVarName;
    }

    public IEnumerable<string> GetNamespaces()
    {
        foreach(var ns in serviceTypeDescriptor.HandlerType.Namespaces)
            yield return ns;
        foreach(var ns in serviceTypeDescriptor.InterfaceType.Namespaces)
            yield return ns;
        yield return "Microsoft.Extensions.DependencyInjection";
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.Ident(indent);

        sb.Append(servicesVarName).Append(".AddTransient<")
            .Append(serviceTypeDescriptor.InterfaceType.Name).Append(", ")
            .Append(serviceTypeDescriptor.HandlerType.Name).Append(">();");

        sb.AppendLine();
    }
}
