using System.Linq.Expressions;

namespace RoyalCode.Extensions.AsyncExpressionsBuilders;

public class AsyncScopeBuilder
{
    private readonly List<ParameterExpression> variables = new();
    private readonly List<Expression> commands = new();

    private readonly ParameterExpression taskExpression;

    public AsyncScopeBuilder()
    {
        taskExpression = Expression.Variable(typeof(Task), "task");
        variables.Add(taskExpression);
        commands.Add(Expression.Assign(taskExpression, Expression.Constant(Task.CompletedTask)));
    }

    public void AddVariable(ParameterExpression variable)
    {
        variables.Add(variable);
    }

    public Expression AwaitScopedBlock(AsyncBlockBuilder blockBuilder)
    {
        var scopedTask = blockBuilder.Build(); // -> o tipo do bloco será Task

        // deve ser criado uma função lambda Func<Task, Task> retornando o scopedTask
        var lambda = Expression.Lambda<Func<Task, Task>>(scopedTask, Expression.Parameter(typeof(Task), "t"));

        // obter método ContinueWith da taskExpression
        var continueWithMethod = taskExpression.Type.GetMethods()
                    .Where(m => m.Name == "ContinueWith" && m.GetParameters().Length == 1 && m.GetGenericArguments().Length == 1)
                    .Select(m => m.MakeGenericMethod(typeof(Task)))
                    .First(m => m.GetParameters()[0].ParameterType == typeof(Func<Task, Task>));

        // chamar método
        var call = Expression.Call(taskExpression, continueWithMethod, lambda);

        // obtém o método Unwrap
        var unwrapMethod = typeof(TaskExtensions)
                .GetMethods()
                .First(m => m.Name == "Unwrap" && m.GetGenericArguments().Length == 0);

        // chamar unwrap em seguida
        call = Expression.Call(null, unwrapMethod, call);

        // atribuir a chamada a taskExpression
        var assign = Expression.Assign(taskExpression, call);

        return assign;
    }

    public void AddCommand(Expression expression)
    {
        commands.Add(expression);
    }

    public Expression Build()
    {
        commands.Add(taskExpression);
        return Expression.Block(typeof(Task), variables, commands);
    }

    public Expression Build(Expression resultExpression, Type resultType)
    {
        // deve ser criado uma função lambda Func<Task, TResult> retornando o scopedTask
        var functionType = typeof(Func<,>).MakeGenericType(typeof(Task), resultType);
        var lambda = Expression.Lambda(functionType, resultExpression, Expression.Parameter(typeof(Task), "t"));

        // obter método ContinueWith da taskExpression
        var continueWithMethod = taskExpression.Type.GetMethods()
                    .Where(m => m.Name == "ContinueWith" && m.GetParameters().Length == 1 && m.GetGenericArguments().Length == 1)
                    .Select(m => m.MakeGenericMethod(resultType))
                    .First(m => m.GetParameters()[0].ParameterType == functionType);

        // chamar método
        var call = Expression.Call(taskExpression, continueWithMethod, lambda);

        // a task<TResult> retornada pela camada será retornada no block, então adiciona a call como comando
        commands.Add(call);

        return Expression.Block(call.Type, variables, commands);
    }
}
