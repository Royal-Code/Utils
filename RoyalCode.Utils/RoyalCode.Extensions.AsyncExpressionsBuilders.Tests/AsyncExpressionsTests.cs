using System.Linq.Expressions;

namespace RoyalCode.Extensions.AsyncExpressionsBuilders.Tests;

public class AsyncExpressionsTests
{
    [Fact]
    public async Task ManualAwait()
    {
        var repo = new Repo();
        var text = await repo.GetAsync();
        Assert.Equal("Hello World!", text);
    }

    [Fact]
    public void ManualTaskContinuation()
    {
        var repo = new Repo();
        string? text = null;

        var task = repo.GetAsync()
            .ContinueWith(t =>
            {
                text = t.Result;
            });

        var task2 = repo.GetAsync()
            .ContinueWith(t =>
            {
                text = t.Result;
                return text;
            });

        task.Wait();
        Assert.Equal("Hello World!", text);
    }

    [Fact]
    public void ManualTaskContinuationExtended()
    {
        var repo = new Repo();
        string? text1 = null;
        string? text2 = null;
        string? text3 = null;

        var task = repo.GetAsync()
            .ContinueWith(t =>
            {
                text1 = t.Result;
                return repo.GetAsync();
            })
            .Unwrap().ContinueWith(t =>
            {
                text2 = t.Result;
                return repo.GetAsync();
            })
            .Unwrap().ContinueWith(t =>
            {
                text3 = t.Result;
                return new Values()
                {
                    First = text1,
                    Second = text2,
                    Third = text3
                };
            });

        var result = task.Result;
        Assert.Equal("Hello World!", result.First);
        Assert.Equal("Hello World!", result.Second);
        Assert.Equal("Hello World!", result.Third);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(1, 2, null)]
    [InlineData(1, null, 3)]
    [InlineData(1, null, null)]
    [InlineData(null, 2, 3)]
    [InlineData(null, 2, null)]
    [InlineData(null, null, 3)]
    [InlineData(null, null, null)]
    public void ManualConditionalTaskContinuation(int? id1, int? id2, int? id3)
    {
        var repo = new Repo();
        string? text1 = null;
        string? text2 = null;
        string? text3 = null;
        string? expected1 = id1.HasValue ? "Hello World!" : null;
        string? expected2 = id2.HasValue ? "Hello World!" : null;
        string? expected3 = id3.HasValue ? "Hello World!" : null;

        Task? task = null;
        if (id1.HasValue)
        {
            var getAsyncTask = task is null
                ? repo.GetAsync()
                : task.ContinueWith(t => repo.GetAsync()).Unwrap();

            task = getAsyncTask.ContinueWith(t =>
            {
                text1 = t.Result;
            });
        }
        if (id2.HasValue)
        {
            var getAsyncTask = task is null
                ? repo.GetAsync()
                : task.ContinueWith(t => repo.GetAsync()).Unwrap();

            task = getAsyncTask.ContinueWith(t =>
            {
                text2 = t.Result;
            });
        }
        if (id3.HasValue)
        {
            var getAsyncTask = task is null
                ? repo.GetAsync()
                : task.ContinueWith(t => repo.GetAsync()).Unwrap();

            task = getAsyncTask.ContinueWith(t =>
            {
                text3 = t.Result;
            });
        }

        var finalTask = task is null
            ? Task.FromResult(new Values()
            {
                First = text1,
                Second = text2,
                Third = text3
            })
            : task.ContinueWith(t => new Values()
            {
                First = text1,
                Second = text2,
                Third = text3
            });

        var result = finalTask.Result;
        Assert.Equal(expected1, result.First);
        Assert.Equal(expected2, result.Second);
        Assert.Equal(expected3, result.Third);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(1, 2, null)]
    [InlineData(1, null, 3)]
    [InlineData(1, null, null)]
    [InlineData(null, 2, 3)]
    [InlineData(null, 2, null)]
    [InlineData(null, null, 3)]
    [InlineData(null, null, null)]
    public void ManualConditionalTaskContinuation2(int? id1, int? id2, int? id3)
    {
        var repo = new Repo();
        string? text1 = null;
        string? text2 = null;
        string? text3 = null;
        string? expected1 = id1.HasValue ? "Hello World!" : null;
        string? expected2 = id2.HasValue ? "Hello World!" : null;
        string? expected3 = id3.HasValue ? "Hello World!" : null;

        Task? task = null;

        task = Task.Run(() =>
        {
            if (id1.HasValue)
            {
                var getAsyncTask = repo.GetAsync();
                return getAsyncTask.ContinueWith(t =>
                {
                    text1 = t.Result;
                });
            }
            return Task.CompletedTask;
        });

        task = task.ContinueWith(t =>
        {
            if (id2.HasValue)
            {
                var getAsyncTask = repo.GetAsync();
                return getAsyncTask.ContinueWith(t =>
                {
                    text2 = t.Result;
                });
            }
            return Task.CompletedTask;
        }).Unwrap();

        task = task.ContinueWith(t =>
        {
            if (id3.HasValue)
            {
                var getAsyncTask = repo.GetAsync();
                return getAsyncTask.ContinueWith(t =>
                {
                    text3 = t.Result;
                });
            }
            return Task.CompletedTask;
        }).Unwrap();

        var finalTask = task.ContinueWith(t => new Values()
        {
            First = text1,
            Second = text2,
            Third = text3
        });

        var result = finalTask.Result;
        Assert.Equal(expected1, result.First);
        Assert.Equal(expected2, result.Second);
        Assert.Equal(expected3, result.Third);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(1, 2, null)]
    [InlineData(1, null, 3)]
    [InlineData(1, null, null)]
    [InlineData(null, 2, 3)]
    [InlineData(null, 2, null)]
    [InlineData(null, null, 3)]
    [InlineData(null, null, null)]
    public void ManualConditionalTaskContinuation3(int? id1, int? id2, int? id3)
    {
        var repo = new Repo();
        string? text1 = null;
        string? text2 = null;
        string? text3 = null;
        string? expected1 = id1.HasValue ? "Hello World!" : null;
        string? expected2 = id2.HasValue ? "Hello World!" : null;
        string? expected3 = id3.HasValue ? "Hello World!" : null;

        Task? task = Task.CompletedTask;

        if (id1.HasValue)
        {
            task = task.ContinueWith(t =>
            {
                var getAsyncTask = repo.GetAsync();
                return getAsyncTask.ContinueWith(t =>
                {
                    text1 = t.Result;
                });
            }).Unwrap();
        }

        if (id2.HasValue)
        {
            task = task.ContinueWith(t =>
            {
                var getAsyncTask = repo.GetAsync();
                return getAsyncTask.ContinueWith(t =>
                {
                    text2 = t.Result;
                });
            }).Unwrap();
        }

        if (id3.HasValue)
        {
            task = task.ContinueWith(t =>
            {
                var getAsyncTask = repo.GetAsync();
                return getAsyncTask.ContinueWith(t =>
                {
                    text3 = t.Result;
                });
            }).Unwrap();
        }

        var finalTask = task.ContinueWith(t => new Values()
        {
            First = text1,
            Second = text2,
            Third = text3
        });

        var result = finalTask.Result;
        Assert.Equal(expected1, result.First);
        Assert.Equal(expected2, result.Second);
        Assert.Equal(expected3, result.Third);
    }

    [Fact]
    public void Task_WithoutResult_NoContinuation()
    {
        var asyncBuilder = new AsyncBlockBuilder();

        var textVar = Expression.Variable(typeof(string), "text");
        var assign = Expression.Assign(textVar, Expression.Constant("Hello", typeof(string)));

        asyncBuilder.AddVariable(textVar);
        asyncBuilder.AddCommand(assign);

        var taskBlock = asyncBuilder.Build();

        var lambda = Expression.Lambda<Func<Task>>(taskBlock);
        var func = lambda.Compile();
        var task = func();
        task.GetAwaiter().GetResult();
    }

    [Fact]
    public void Task_Result_NoContinuation()
    {
        var asyncBuilder = new AsyncBlockBuilder();

        var textVar = Expression.Variable(typeof(string), "text");
        var assign = Expression.Assign(textVar, Expression.Constant("Hello", typeof(string)));

        asyncBuilder.AddVariable(textVar);
        asyncBuilder.AddCommand(assign);

        var taskBlock = asyncBuilder.Build(textVar, typeof(string));

        var lambda = Expression.Lambda<Func<Task<string>>>(taskBlock);
        var func = lambda.Compile();
        var task = func();
        var text = task.GetAwaiter().GetResult();
        Assert.Equal("Hello", text);
    }

    [Fact]
    public void Task_WithoutResult_OneContinuation()
    {
        var asyncBuilder = new AsyncBlockBuilder();

        var repoVar = Expression.Variable(typeof(Repo), "repo");
        var assignRepo = Expression.Assign(repoVar, Expression.New(typeof(Repo)));
        asyncBuilder.AddVariable(repoVar);
        asyncBuilder.AddCommand(assignRepo);

        var textVar = Expression.Variable(typeof(string), "text");
        asyncBuilder.AddVariable(textVar);

        var getAsyncMethod = typeof(Repo).GetMethod(nameof(Repo.GetAsync))!;
        var getAsyncCall = Expression.Call(repoVar, getAsyncMethod);
        var getResult = asyncBuilder.AwaitResult(getAsyncCall, typeof(string));

        var assignText = Expression.Assign(textVar, getResult);
        asyncBuilder.AddCommand(assignText);

        var taskBlock = asyncBuilder.Build();

        var lambda = Expression.Lambda<Func<Task>>(taskBlock);
        var func = lambda.Compile();
        var task = func();
        task.GetAwaiter().GetResult();
    }

    [Fact]
    public void Task_Result_OneContinuation()
    {
        var asyncBuilder = new AsyncBlockBuilder();

        var repoVar = Expression.Variable(typeof(Repo), "repo");
        var assignRepo = Expression.Assign(repoVar, Expression.New(typeof(Repo)));
        asyncBuilder.AddVariable(repoVar);
        asyncBuilder.AddCommand(assignRepo);

        var textVar = Expression.Variable(typeof(string), "text");
        asyncBuilder.AddVariable(textVar);

        var getAsyncMethod = typeof(Repo).GetMethod(nameof(Repo.GetAsync))!;
        var getAsyncCall = Expression.Call(repoVar, getAsyncMethod);
        var getResult = asyncBuilder.AwaitResult(getAsyncCall, typeof(string));

        var assignText = Expression.Assign(textVar, getResult);
        asyncBuilder.AddCommand(assignText);

        var taskBlock = asyncBuilder.Build(textVar, typeof(string));

        var lambda = Expression.Lambda<Func<Task<string>>>(taskBlock);
        var func = lambda.Compile();
        var task = func();
        var text = task.GetAwaiter().GetResult();
        Assert.Equal("Hello World!", text);
    }

    [Fact]
    public void Task_Result_ThreeContinuation()
    {
        var asyncBuilder = new AsyncBlockBuilder();

        var repoVar = Expression.Variable(typeof(Repo), "repo");
        var assignRepo = Expression.Assign(repoVar, Expression.New(typeof(Repo)));
        asyncBuilder.AddVariable(repoVar);
        asyncBuilder.AddCommand(assignRepo);

        var textVar1 = Expression.Variable(typeof(string), "text1");
        var textVar2 = Expression.Variable(typeof(string), "text2");
        var textVar3 = Expression.Variable(typeof(string), "text3");
        asyncBuilder.AddVariable(textVar1);
        asyncBuilder.AddVariable(textVar2);
        asyncBuilder.AddVariable(textVar3);

        var getAsyncMethod = typeof(Repo).GetMethod(nameof(Repo.GetAsync))!;

        var getAsyncCall = Expression.Call(repoVar, getAsyncMethod);
        var getResult = asyncBuilder.AwaitResult(getAsyncCall, typeof(string));
        var assignText1 = Expression.Assign(textVar1, getResult);
        asyncBuilder.AddCommand(assignText1);

        getAsyncCall = Expression.Call(repoVar, getAsyncMethod);
        getResult = asyncBuilder.AwaitResult(getAsyncCall, typeof(string));
        var assignText2 = Expression.Assign(textVar2, getResult);
        asyncBuilder.AddCommand(assignText2);

        getAsyncCall = Expression.Call(repoVar, getAsyncMethod);
        getResult = asyncBuilder.AwaitResult(getAsyncCall, typeof(string));
        var assignText3 = Expression.Assign(textVar3, getResult);
        asyncBuilder.AddCommand(assignText3);

        var valuesVar = Expression.Variable(typeof(Values), "values");
        var valuesCtor = typeof(Values).GetConstructor(new[] { typeof(string), typeof(string), typeof(string) })!;
        var valuesNew = Expression.New(valuesCtor, textVar1, textVar2, textVar3);
        var valuesAssign = Expression.Assign(valuesVar, valuesNew);
        asyncBuilder.AddVariable(valuesVar);
        asyncBuilder.AddCommand(valuesAssign);

        var taskBlock = asyncBuilder.Build(valuesVar, typeof(Values));

        var lambda = Expression.Lambda<Func<Task<Values>>>(taskBlock);
        var func = lambda.Compile();
        var task = func();
        var values = task.GetAwaiter().GetResult();
        Assert.Equal("Hello World!", values.First);
        Assert.Equal("Hello World!", values.Second);
        Assert.Equal("Hello World!", values.Third);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(1, 2, null)]
    [InlineData(1, null, 3)]
    [InlineData(1, null, null)]
    [InlineData(null, 2, 3)]
    [InlineData(null, 2, null)]
    [InlineData(null, null, 3)]
    [InlineData(null, null, null)]
    public void Task_ConditionalTaskContinuation(int? id1, int? id2, int? id3)
    {
        var scopeBuilder = new AsyncScopeBuilder();

        var repoVar = Expression.Variable(typeof(Repo), "repo");
        var assignRepo = Expression.Assign(repoVar, Expression.New(typeof(Repo)));
        scopeBuilder.AddVariable(repoVar);
        scopeBuilder.AddCommand(assignRepo);

        var textVar1 = Expression.Variable(typeof(string), "text1");
        var textVar2 = Expression.Variable(typeof(string), "text2");
        var textVar3 = Expression.Variable(typeof(string), "text3");
        scopeBuilder.AddVariable(textVar1);
        scopeBuilder.AddVariable(textVar2);
        scopeBuilder.AddVariable(textVar3);

        var getAsyncMethod = typeof(Repo).GetMethod(nameof(Repo.GetAsync))!;


        var idVar1 = Expression.Variable(typeof(int?), "id1");
        var assignId = Expression.Assign(idVar1, Expression.Constant(id1, typeof(int?)));
        scopeBuilder.AddVariable(idVar1);
        scopeBuilder.AddCommand(assignId);

        var asyncBuilder = new AsyncBlockBuilder();
        var getAsyncCall = Expression.Call(repoVar, getAsyncMethod);
        var getResult = asyncBuilder.AwaitResult(getAsyncCall, typeof(string));
        var assignText1 = Expression.Assign(textVar1, getResult);
        asyncBuilder.AddCommand(assignText1);

        var condition1 = Expression.IfThen(
            Expression.NotEqual(idVar1, Expression.Constant(null)),
            scopeBuilder.AwaitScopedBlock(asyncBuilder));
        scopeBuilder.AddCommand(condition1);


        var idVar2 = Expression.Variable(typeof(int?), "id2");
        assignId = Expression.Assign(idVar2, Expression.Constant(id2, typeof(int?)));
        scopeBuilder.AddVariable(idVar2);
        scopeBuilder.AddCommand(assignId);

        asyncBuilder = new AsyncBlockBuilder();
        getAsyncCall = Expression.Call(repoVar, getAsyncMethod);
        getResult = asyncBuilder.AwaitResult(getAsyncCall, typeof(string));
        var assignText2 = Expression.Assign(textVar2, getResult);
        asyncBuilder.AddCommand(assignText2);

        var condition2 = Expression.IfThen(
            Expression.NotEqual(idVar2, Expression.Constant(null)),
            scopeBuilder.AwaitScopedBlock(asyncBuilder));
        scopeBuilder.AddCommand(condition2);


        var idVar3 = Expression.Variable(typeof(int?), "id3");
        assignId = Expression.Assign(idVar3, Expression.Constant(id3, typeof(int?)));
        scopeBuilder.AddVariable(idVar3);
        scopeBuilder.AddCommand(assignId);

        asyncBuilder = new AsyncBlockBuilder();
        getAsyncCall = Expression.Call(repoVar, getAsyncMethod);
        getResult = asyncBuilder.AwaitResult(getAsyncCall, typeof(string));
        var assignText3 = Expression.Assign(textVar3, getResult);
        asyncBuilder.AddCommand(assignText3);

        var condition3 = Expression.IfThen(
            Expression.NotEqual(idVar3, Expression.Constant(null)),
            scopeBuilder.AwaitScopedBlock(asyncBuilder));
        scopeBuilder.AddCommand(condition3);


        var valuesCtor = typeof(Values).GetConstructor(new[] { typeof(string), typeof(string), typeof(string) })!;
        var valuesNew = Expression.New(valuesCtor, textVar1, textVar2, textVar3);

        var taskBlock = scopeBuilder.Build(valuesNew, typeof(Values));

        var lambda = Expression.Lambda<Func<Task<Values>>>(taskBlock);
        var func = lambda.Compile();
        var task = func();
        var values = task.GetAwaiter().GetResult();

        string? expected1 = id1.HasValue ? "Hello World!" : null;
        string? expected2 = id2.HasValue ? "Hello World!" : null;
        string? expected3 = id3.HasValue ? "Hello World!" : null;
        Assert.Equal(expected1, values.First);
        Assert.Equal(expected2, values.Second);
        Assert.Equal(expected3, values.Third);
    }
}

public class Repo
{
    public async Task<string> GetAsync()
    {
        await Task.Delay(1000);
        return "Hello World!";
    }
}

public class Values
{
    public Values() { }

    public Values(string first, string second, string third)
    {
        First = first;
        Second = second;
        Third = third;
    }

    public string First { get; set; }

    public string Second { get; set; }

    public string Third { get; set; }
}