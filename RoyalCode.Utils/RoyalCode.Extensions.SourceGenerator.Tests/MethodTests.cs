using RoyalCode.Extensions.SourceGenerator.Descriptors;
using RoyalCode.Extensions.SourceGenerator.Generators;
using RoyalCode.Extensions.SourceGenerator.Generators.Commands;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Tests;

public class MethodTests
{
    [Fact]
    public void Method_WithOneGenericAndOneParameter()
    {
        // Arrange
        var sb = new StringBuilder();
        var expected =
            """
            
            public void Method<T>(T value)
                where T : notnull
            {
                throw new NotImplementedException();
            }
            
            """;

        // Act
        var method = new MethodGenerator("Method", TypeDescriptor.Void());
        method.Modifiers.Public();
        method.Generics.AddGeneric("T");
        method.Where.Add(new WhereGenerator("T", "notnull"));
        method.Parameters.Add(new ParameterGenerator(new ParameterDescriptor(new TypeDescriptor("T", []), "value")));
        method .Commands.Add(ThrowCommand.NotImplementedException());

        method.Write(sb);
        var generated = sb.ToString();
        
        // Assert
        Assert.Equal(expected, generated);
    }
}
