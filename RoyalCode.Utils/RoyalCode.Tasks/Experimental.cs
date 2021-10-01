using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RoyalCode.Tasks
{

    public static class NoSynchronizationContextScope
    {
        public static Disposable Enter()
        {
            var context = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            return new Disposable(context);
        }

        public struct Disposable : IDisposable
        {
            private readonly SynchronizationContext _synchronizationContext;
            public Disposable(SynchronizationContext synchronizationContext)
            {
                _synchronizationContext = synchronizationContext;
            }
            public void Dispose() => SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
        }

        public static T GetResultSafe<T>(this Task<T> task)
        {
            if (SynchronizationContext.Current == null)
                return task.Result;

            if (task.IsCompleted)
                return task.Result;

            var tcs = new TaskResultState<T>();

            task.ContinueWith(t =>
            {
                var ex = t.Exception;
                if (ex != null)
                    tcs.SetException(ex);
                else
                    tcs.SetResult(t.Result);
            }, TaskScheduler.Default);

            using (Enter())
            {
                task.Wait();
            }

            return tcs.Task.Result;
        }

        private class TaskResultState<T>
        {
            private Task<T>? _task;

            public Task<T> Task => _task ?? System.Threading.Tasks.Task.FromException<T>(new InvalidOperationException());

            internal void SetException(AggregateException ex)
            {
                _task = System.Threading.Tasks.Task.FromException<T>(ex);
            }

            internal void SetResult(T result)
            {
                _task = System.Threading.Tasks.Task.FromResult<T>(result);
            }
        }
    }


    public class Uso
    {
        private Task MyAsyncMethod() => Task.CompletedTask;

        private void MySynchronousMethodLikeDisposeForExample1()
        {
            // MyAsyncMethod will get queued to the thread pool 
            // so it shouldn't deadlock with the Wait() ??
            Task.Run(MyAsyncMethod).Wait();
        }

        private void MySynchronousMethodLikeDisposeForExample2()
        {
            using (NoSynchronizationContextScope.Enter())
            {
                MyAsyncMethod().Wait();
            }
        }
    }
}
