﻿using RoyalCode.Extensions.SourceGenerator.Descriptors;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Generators;

public class MethodGenerator : GeneratorNode, IWithNamespaces
{
    public static MethodGenerator CreateImplementation(MethodGenerator abstractMethod)
    {
        var impl = new MethodGenerator(abstractMethod.Name, abstractMethod.ReturnType);
        
        if (abstractMethod.modifiers is not null)
            impl.modifiers = abstractMethod.modifiers.CloneForMethodImplementation();

        if (abstractMethod.parameters is not null)
            impl.parameters = abstractMethod.Parameters.Clone();

        return impl;
    }

    private GeneratorNodeList? attributes;
    private ModifiersGenerator? modifiers;
    private ParametersGenerator? parameters;
    private GeneratorNodeList? commands;
    private BaseParametersGenerator? baseParameters;

    public MethodGenerator(string name, TypeDescriptor returnType)
    {
        Name = name;
        ReturnType = returnType;
    }

    public GeneratorNodeList Attributes => attributes ??= new();
    
    public ModifiersGenerator Modifiers => modifiers ??= new();

    public ParametersGenerator Parameters => parameters ??= new();

    public BaseParametersGenerator BaseParameters => baseParameters ??= new();

    public GeneratorNodeList Commands => commands ??= new();

    public string Name { get; set; }

    public TypeDescriptor ReturnType { get; set; }

    public bool IsAbstract { get; set; }

    public bool UseArrow { get; set; }

    public IEnumerable<string> GetNamespaces()
    {
        foreach (var ns in ReturnType.Namespaces)
            yield return ns;
        if (attributes is not null)
            foreach (var ns in attributes.GetNamespaces())
                yield return ns;
        if (parameters is not null)
            foreach (var ns in parameters.GetNamespaces())
                yield return ns;
        if (commands is not null)
            foreach (var ns in commands.GetNamespaces())
                yield return ns;
    }

    public override void Write(StringBuilder sb, int ident = 0)
    {
        sb.AppendLine();
        
        attributes?.Write(sb, ident);
        
        sb.Ident(ident);
        modifiers?.Write(sb);
        
        sb.Append(ReturnType.Name).Append(' ');
        
        sb.Append(Name);
        if (parameters is null)
            sb.Append("()");
        else
            parameters.Write(sb, ident);
        
        if (IsAbstract)
        {
            sb.AppendLine(";");
            return;
        }

        baseParameters?.Write(sb, ident);

        if (commands is null)
        {
            sb.AppendLine(" { }");
            return;
        }

        if (UseArrow)
        {
            sb.Append(" => ");
            commands.Write(sb, ident + 1);
        }
        else
        {
            sb.AppendLine();
            sb.Ident(ident).Append('{');
            sb.AppendLine();

            int commandsIdent = ident + 1;
            commands.Write(sb, commandsIdent);

            sb.Ident(ident).Append('}');
            sb.AppendLine();
        }
    }
}