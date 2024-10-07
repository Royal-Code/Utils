using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace RoyalCode.DependencyInjection.Generators;

internal class AddServicesGenerator
{
    private readonly SourceProductionContext spc;
    private readonly AddServicesInformation addServices;
    private readonly ImmutableArray<ServiceInformation> services;

    internal AddServicesGenerator(
        SourceProductionContext spc,
        AddServicesInformation addServices,
        ImmutableArray<ServiceInformation> services)
    {
        this.spc = spc;
        this.addServices = addServices;
        this.services = services;
    }

    internal void Generate()
    {
        var builder = new StringBuilder();

        // usings
        var ns = addServices.ClassDescriptor.Namespaces[0];
        AddUsings(builder, ns);

        // namespace
        builder.Append("namespace ").Append(ns).AppendLine(";").AppendLine();

        // declaração da classe e método
        DeclareClass(builder);

        // adiciona serviços
        AddServices(builder);

        // finaliza a classe
        FinalizeClass(builder);

        var fileName = $"{addServices.ClassDescriptor.Name}.g.cs";
        spc.AddSource(fileName, builder.ToString());
    }

    private void AddUsings(StringBuilder builder, string ns)
    {
        List<string> usings = ["Microsoft.Extensions.DependencyInjection"];

        for (int i = 0; i < services.Length; i++)
        {
            var serv = services[i];
            usings.AddRange(serv.ImplementationType.Namespaces);
            for (int j = 0; j < serv.ServicesTypes.Length; j++)
                usings.AddRange(serv.ServicesTypes[j].Namespaces);
        }

        var required = usings.Distinct()
            .Where(u => u != ns)
            .OrderBy(u => u)
            .ToList();

        foreach (var u in required)
        {
            builder.Append("using ").Append(u).AppendLine(";");
        }

        builder.AppendLine();
    }

    private void DeclareClass(StringBuilder builder)
    {
        builder.Append("public static partial class ").AppendLine(addServices.ClassDescriptor.Name);
        builder.AppendLine("{");

        builder.Append("    ")
            .Append(addServices.MethodIsPublic ? "public" : "internal")
            .Append(" static partial ")
            .Append(addServices.ReturnType.Name)
            .Append(' ')
            .Append(addServices.MethodName)
            .AppendLine("(this IServiceCollection services)")
            .AppendLine("    {");
    }

    private void AddServices(StringBuilder builder)
    {
        foreach (var serv in services)
        {
            builder.Append("        services").Lifetime(serv.Lifetime);

            if (serv.ServicesTypes.Length == 0)
            {
                if (serv.GenericParameters == 0)
                {
                    builder.Append('<').Append(serv.ImplementationType.Name).Append(">()");
                }
                else
                {
                    builder.Append("(typeof(")
                        .Append(serv.ImplementationType.Name)
                        .GenericCommas(serv.GenericParameters)
                        .Append("))");
                }
            }
            else if (serv.ServicesTypes.Length == 1)
            {
                if (serv.GenericParameters == 0)
                {
                    builder.Append('<')
                        .Append(serv.ServicesTypes[0].Name)
                        .Append(", ")
                        .Append(serv.ImplementationType.Name)
                        .Append(">()");
                }
                else
                {
                    builder.Append("(typeof(")
                        .Append(serv.ServicesTypes[0].Name)
                        .GenericCommas(serv.GenericParameters)
                        .Append("), typeof(")
                        .Append(serv.ImplementationType.Name)
                        .GenericCommas(serv.GenericParameters)
                        .Append("))");
                }
            }
            else
            {
                // caso há várias interfaces, então é adicionado elas como factories, mais abaixo.
                builder.Append('<').Append(serv.ImplementationType.Name).Append(">()");
            }

            builder.AppendLine(";");

            if (serv.ServicesTypes.Length > 1)
            {
                foreach (var servType in serv.ServicesTypes)
                {
                    //services.AddTransient<IService>(sp => sp.GetService<Service>())

                    builder.Append("        services")
                        .Lifetime(serv.Lifetime)
                        .Append('<')
                        .Append(servType.Name)
                        .Append(">(sp => sp.GetService<")
                        .Append(serv.ImplementationType.Name)
                        .AppendLine(">());");
                }
            }
        }
    }

    private static void FinalizeClass(StringBuilder builder)
    {
        builder.AppendLine("    }");
        builder.AppendLine("}");
    }
}