using RoyalCode.Extensions.SourceGenerator.Descriptors;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class PropertyGenerator : GeneratorNode, IWithNamespaces
{
    private ModifiersGenerator? modifiers;

    public PropertyGenerator(TypeDescriptor type, string name, bool canGet = true, bool canSet = true)
    {
        Type = type;
        Name = name;
        CanGet = canGet;
        CanSet = canSet;
    }

    public ModifiersGenerator Modifiers => modifiers ??= new();

    public TypeDescriptor Type { get; set; }

    public string Name { get; set; }

    public bool CanGet { get; set; } = true;

    public bool CanSet { get; set; } = true;

    public ValueNode? Value { get; set; }

    public IEnumerable<string> GetNamespaces()
    {
        foreach (var ns in Type.Namespaces)
            yield return ns;
        if (Value is IWithNamespaces wns)
            foreach (var ns in wns.GetNamespaces())
                yield return ns;
    }

    public override void Write(StringBuilder sb, int indent = 0)
    {
        sb.AppendLine();
        sb.Ident(indent);
        
        modifiers?.Write(sb);
        sb.Append(Type.Name).Append(' ').Append(Name).Append(" { ");
        
        if (CanGet)
            sb.Append("get; ");
        
        if (CanSet)
            sb.Append("set; ");
        
        sb.Append("}");

        if (Value is not null)
        {
            sb.Append(" = ").Append(Value.GetValue(indent));
            sb.AppendLine(";");
        }
        else
        {
            sb.AppendLine();
        }
    }
}