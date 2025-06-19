using System.Text;
using Microsoft.CodeAnalysis;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

/// <summary>
/// <para>
///     Represents a generator for creating C# class definitions, including support for namespaces,
///     attributes, modifiers, generics, hierarchy, and members such as fields, constructors, properties, and methods.
/// </para>
/// <para>
///     This class provides functionality to construct and generate source code for a C# class dynamically.
/// </para> 
/// <para>
///     It implements <see cref="ITransformationGenerator"/>,
///     which causes this node to generate a .cs file of the class, using the nodes added to the class.
/// </para>
/// </summary>
public class ClassGenerator : ITransformationGenerator, IWithNamespaces
{
    private GeneratorNodeList? attributes;
    private ModifiersGenerator? modifiers;
    private GenericsGenerator? generics;
    private HierarchyGenerator? hierarchy;
    private GeneratorNodeList? where;
    private GeneratorNodeList? fields;
    private GeneratorNodeList? constructors;
    private GeneratorNodeList? properties;
    private GeneratorNodeList? methods;

    public ClassGenerator(string name, string ns, string typeType = "class")
    {
        Name = name;
        Namespace = ns;
        TypeType = typeType;
    }

    public string Namespace { get; set; }

    public GeneratorNodeList Attributes => attributes ??= new();

    public ModifiersGenerator Modifiers => modifiers ??= new();

    public string TypeType { get; }

    public string Name { get; set; }

    public GenericsGenerator Generics => generics ??= new();

    public HierarchyGenerator Hierarchy => hierarchy ??= new();

    public GeneratorNodeList Where => where ??= new();

    public GeneratorNodeList Fields => fields ??= new();

    public GeneratorNodeList Constructors => constructors ??= new();

    public GeneratorNodeList Properties => properties ??= new();

    public GeneratorNodeList Methods => methods ??= new();

    /// <summary>
    /// Nome do arquivo que será gerado, opcional, será utilizado o padrão: <c>"{Name}.g.cs"</c>.
    /// </summary>
    public string? FileName { get; set; }

    public event Action<SourceProductionContext, StringBuilder>? Generating;
    
    public event Action<SourceProductionContext, StringBuilder>? Generated;

    public MethodGenerator CreateImplementation(MethodGenerator abstractMethod)
    {
        var impl = MethodGenerator.CreateImplementation(abstractMethod);
        Methods.Add(impl);
        return impl;
    }

    public void Generate(SourceProductionContext spc)
    {
        var builder = new StringBuilder();

        Generating?.Invoke(spc, builder);

        Write(builder);

        Generated?.Invoke(spc, builder);

        var source = builder.ToString();

        spc.AddSource(FileName ?? $"{Name}.g.cs", source);
    }

    public IEnumerable<string> GetNamespaces()
    {
        if (attributes is not null)
            foreach (var ns in attributes.GetNamespaces())
                yield return ns;
        if (generics is not null)
            foreach (var ns in generics.GetNamespaces())
                yield return ns;
        if (hierarchy is not null)
            foreach (var ns in hierarchy.GetNamespaces())
                yield return ns;
        if (where is not null)
            foreach (var ns in where.GetNamespaces())
                yield return ns;
        if (fields is not null)
            foreach (var field in fields.GetNamespaces())
                yield return field;
        if (constructors is not null)
            foreach (var constructor in constructors.GetNamespaces())
                yield return constructor;
        if (properties is not null)
            foreach (var property in properties.GetNamespaces())
                yield return property;
        if (methods is not null)
            foreach (var method in methods.GetNamespaces())
                yield return method;
    }

    public bool HasErrors(SourceProductionContext spc, SyntaxToken mainToken) => false;

    protected void Write(StringBuilder sb)
    {
        var usings = new UsingsGenerator();
        usings.AddNamespaces(GetNamespaces());
        usings.ExcludeNamespace(Namespace);
        usings.Write(sb);

        sb.AppendLine();
        sb.Append("namespace ").Append(Namespace).AppendLine(";");
        sb.AppendLine();

        attributes?.Write(sb);
        modifiers?.Write(sb);
        sb.Append(TypeType).Append(" ").Append(Name);

        generics?.Write(sb);
        hierarchy?.Write(sb);
        where?.Write(sb, 1);

        sb.AppendLine();
        sb.Append("{");

        if (fields is not null)
        {
            sb.AppendLine();
            fields.Write(sb, 1);
        }

        constructors?.Write(sb, 1);
        properties?.Write(sb, 1);
        methods?.Write(sb, 1);

        sb.AppendLine("}");
    }
}