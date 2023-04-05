using System.Linq.Expressions;

namespace RoyalCode.Extensions.AsyncExpressionsBuilders;

internal class AsyncChainedBlock
{
    private readonly Expression taskExpression;
    private readonly Type? taskResultType;
    private readonly AsyncChainedBlock? parent;
    private readonly ParameterExpression continuationParameter;

    private readonly List<Expression> expressions = new();

    public AsyncChainedBlock(Expression taskExpression, Type? taskResultType = null, AsyncChainedBlock? parent = null)
    {
        this.taskExpression = taskExpression;
        this.taskResultType = taskResultType;
        this.parent = parent;

        if (taskResultType is null)
        {
            continuationParameter = Expression.Parameter(typeof(Task), "t");
        }
        else
        {
            continuationParameter = Expression.Parameter(typeof(Task<>).MakeGenericType(taskResultType), "t");
            AwaitedResultExpression = Expression.Property(continuationParameter, "Result");
        }
    }

    public Expression? AwaitedResultExpression { get; }

    public Type? ReturnType { get; set; }

    public void Add(Expression expression)
    {
        expressions.Add(expression);
    }

    public AsyncChainedBlock Await(Expression taskExpression, Type? taskResultType = null)
    {
        if (taskResultType is null)
            ReturnType = typeof(Task);
        else
            ReturnType = typeof(Task<>).MakeGenericType(taskResultType);

        Add(taskExpression);

        return new AsyncChainedBlock(taskExpression, taskResultType, this);
    }

    public Expression CreateTask()
    {
        var task = GetTaskExpressionForBuild();

        var continuationParameterType = ReturnType is null
            ? typeof(Action<>).MakeGenericType(continuationParameter.Type)
            : typeof(Func<,>).MakeGenericType(continuationParameter.Type, ReturnType);

        var continuationMethod = ReturnType is null
                ? task.Type.GetMethod("ContinueWith", new Type[] { continuationParameterType })
                : task.Type.GetMethods()
                    .Where(m => m.Name == "ContinueWith" && m.GetParameters().Length == 1 && m.GetGenericArguments().Length == 1)
                    .Select(m => m.MakeGenericMethod(ReturnType))
                    .First(m => m.GetParameters()[0].ParameterType == continuationParameterType);

        var continuationBlock = ReturnType is null
            ? Expression.Block(expressions)
            : Expression.Block(ReturnType, expressions);

        var continuationLambda = Expression.Lambda(continuationParameterType, continuationBlock, continuationParameter);

        return Expression.Call(task, continuationMethod!, continuationLambda);
    }

    public Expression CreateTask(Expression resultExpression, Type resultType)
    {
        Add(resultExpression);
        ReturnType = resultType;
        return CreateTask();
    }

    private Expression GetTaskExpressionForBuild()
    {
        if (parent is null)
        {
            return taskExpression;
        }
        else
        {
            var parentTask = parent.CreateTask();

            var unwrapMethod = typeof(TaskExtensions)
                .GetMethods()
                .First(m => m.Name == "Unwrap" && m.GetGenericArguments().Length == (taskResultType is null ? 0 : 1));

            if (taskResultType is not null)
                unwrapMethod = unwrapMethod.MakeGenericMethod(taskResultType);

            return Expression.Call(unwrapMethod!, parentTask);
        }
    }
}
