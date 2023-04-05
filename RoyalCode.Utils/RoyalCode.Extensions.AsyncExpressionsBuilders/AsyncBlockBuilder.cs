using System.Linq.Expressions;

namespace RoyalCode.Extensions.AsyncExpressionsBuilders;

/// <summary>
/// Classe para criar System.Linq.Expression de blocos assíncronos, 
/// onde existem task que precisam ser completadas outros comandos possam ser executados.
/// </summary>
public class AsyncBlockBuilder
{
    private readonly List<Expression> commands = new();
    private readonly List<ParameterExpression> variables = new();
    private AsyncChainedBlock current;

    public void AddVariable(ParameterExpression variable)
    {
        variables.Add(variable);
    }

    public void AddCommand(Expression expression)
    {
        if (current is null)
            commands.Add(expression);
        else
            current.Add(expression);
    }

    public void AwaitVoid(Expression taskExpression)
    {
        if (current is null)
            current = new AsyncChainedBlock(taskExpression);
        else
            current = current.Await(taskExpression);
    }

    public Expression AwaitResult(Expression taskExpression, Type resultType)
    {
        if (current is null)
            current = new AsyncChainedBlock(taskExpression, resultType);
        else
            current = current.Await(taskExpression, resultType);

        return current.AwaitedResultExpression!;
    }

    public Expression Build()
    {
        var taskExpression = current?.CreateTask() ?? Expression.Constant(Task.CompletedTask);

        commands.Add(taskExpression);
        return Expression.Block(typeof(Task), variables, commands);
    }

    public Expression Build(Expression resultExpression, Type resultType)
    {
        var taskExpression = current?.CreateTask(resultExpression, resultType)
            ?? Expression.Call(
                typeof(Task).GetMethods().First(m => m.Name == "FromResult").MakeGenericMethod(resultType),
                resultExpression);

        commands.Add(taskExpression);
        return Expression.Block(typeof(Task<>).MakeGenericType(resultType), variables, commands);
    }
}