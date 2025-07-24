using RoyalCode.Extensions.SourceGenerator.Generators.Commands;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator.Tests.Commands;

public class IfTests
{
    [Fact]
    public void If_WithoutCommands()
    {
        // Arrange
        var sb = new StringBuilder();
        var expected =
            """
            if (1 == 1) { }


            """;

        // Act
        var command = new IfCommand("1 == 1");
        command.Write(sb);
        var generated = sb.ToString();

        // Assert
        Assert.Equal(expected, generated);
    }

    [Fact]
    public void If_WithoutCommands_WithoutNewLine()
    {
        // Arrange
        var sb = new StringBuilder();
        var expected =
            """
            if (1 == 1) { }

            """;

        // Act
        var command = new IfCommand("1 == 1") { NewLine = false };
        command.Write(sb);
        var generated = sb.ToString();

        // Assert
        Assert.Equal(expected, generated);
    }

    [Fact]
    public void If_WithSingleCommand()
    {
        // Arrange
        var sb = new StringBuilder();
        var expected =
            """
            if (1 == 1)
                return 0;


            """;

        // Act
        var command = new IfCommand("1 == 1");
        command.AddCommand(new ReturnCommand("0"));
        command.Write(sb);
        var generated = sb.ToString();

        // Assert
        Assert.Equal(expected, generated);
    }

    [Fact]
    public void If_WithCommands()
    {
        // Arrange
        var sb = new StringBuilder();
        var expected =
            """
            if (1 == 1)
            {
                var i = 0;
                return i;
            }


            """;

        // Act
        var command = new IfCommand("1 == 1");
        command.AddCommand(new AssignValueCommand("var i", "0"));
        command.AddCommand(new ReturnCommand("i"));
        command.Write(sb);
        var generated = sb.ToString();

        // Assert
        Assert.Equal(expected, generated);
    }

    [Fact]
    public void If_Else_WithoutCommands()
    {

        var sb = new StringBuilder();
        var expected =
            """
            if (1 == 1) { }
            else
                return 0;


            """;


        var command = new IfCommand("1 == 1");
        command.AddElseCommand(new ReturnCommand("0"));
        command.Write(sb);
        var generated = sb.ToString();

        Assert.Equal(expected, generated);
    }

    [Fact]
    public void If_Else_WithSingleCommand()
    {
        // Arrange
        var sb = new StringBuilder();
        var expected =
            """
            if (1 == 1)
                return 0;
            else
                return 1;


            """;

        // Act
        var command = new IfCommand("1 == 1");
        command.AddCommand(new ReturnCommand("0"));
        command.AddElseCommand(new ReturnCommand("1"));
        command.Write(sb);
        var generated = sb.ToString();

        // Assert
        Assert.Equal(expected, generated);
    }

    [Fact]
    public void If_Else_WithCommands()
    {
        // Arrange
        var sb = new StringBuilder();
        var expected =
            """
            if (1 == 1)
            {
                var i = 0;
                return i;
            }
            else
            {
                var j = 1;
                return j;
            }


            """;

        // Act
        var command = new IfCommand("1 == 1");
        command.AddCommand(new AssignValueCommand("var i", "0"));
        command.AddCommand(new ReturnCommand("i"));
        command.AddElseCommand(new AssignValueCommand("var j", "1"));
        command.AddElseCommand(new ReturnCommand("j"));
        command.Write(sb);
        var generated = sb.ToString();

        // Assert
        Assert.Equal(expected, generated);
    }
}
