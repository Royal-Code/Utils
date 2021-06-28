using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.Tasks
{
    /// <summary>
    /// Utilitário para executar <see cref="Task"/> e <see cref="Task{TResult}"/> de forma síncrona.
    /// </summary>
    public static class TaskRunner
    {
        private static readonly ObjectPool<InternalSynchronizationContext> synchrorizationContextPool
            = new DefaultObjectPool<InternalSynchronizationContext>(new InternalPooledObjectPolicy(), 10);

        /// <summary>
        /// Executa sincronamente a Task.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="continueOnCapturedContext">Aplicado no <see cref="Task.ConfigureAwait(bool)"/>.</param>
        public static void RunSync(this Task task, bool continueOnCapturedContext = false)
        {
            var originalContext = SynchronizationContext.Current;

            if (originalContext == null)
            {
                task.GetAwaiter().GetResult();
                return;
            }

            var internalContext = synchrorizationContextPool.Get();

            RunSync(internalContext, originalContext, task, continueOnCapturedContext);

            synchrorizationContextPool.Return(internalContext);
        }

        /// <summary>
        /// Executa sincronamente a Task.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="continueOnCapturedContext">Aplicado no <see cref="Task.ConfigureAwait(bool)"/>.</param>
        public static void RunSyncOnNewThread(this Task task, bool continueOnCapturedContext = false)
        {
            var originalContext = SynchronizationContext.Current;

            if (originalContext == null)
            {
                task.GetAwaiter().GetResult();
                return;
            }

            var internalContext = synchrorizationContextPool.Get();
            var mre = new ManualResetEvent(false);

            var executionThread = new Thread(state =>
            {
                RunSync(internalContext, originalContext, task, continueOnCapturedContext, mre);
            });

            executionThread.Start();
            mre.WaitOne();

            synchrorizationContextPool.Return(internalContext);
        }

        /// <summary>
        /// Executa sincronamente a Task.
        /// </summary>
        /// <typeparam name="T">Tipo do valor retornado pela task.</typeparam>
        /// <param name="task">Task.</param>
        /// <param name="continueOnCapturedContext">Aplicado no <see cref="Task.ConfigureAwait(bool)"/>.</param>
        /// <returns>O resultado da Task.</returns>
        public static T RunSync<T>(this Task<T> task, bool continueOnCapturedContext = false)
        {
            var originalContext = SynchronizationContext.Current;

            if (originalContext is null)
                return task.GetAwaiter().GetResult();

            var state = new TaskResultState<T>();

            var internalContext = synchrorizationContextPool.Get();

            RunSync(internalContext, originalContext, task, continueOnCapturedContext, state);

            synchrorizationContextPool.Return(internalContext);

            return state.Result;
        }

        /// <summary>
        /// Executa sincronamente a Task.
        /// </summary>
        /// <typeparam name="T">Tipo do valor retornado pela task.</typeparam>
        /// <param name="task">Task.</param>
        /// <param name="continueOnCapturedContext">Aplicado no <see cref="Task.ConfigureAwait(bool)"/>.</param>
        /// <returns>O resultado da Task.</returns>
        public static T RunSyncOnNewThread<T>(this Task<T> task, bool continueOnCapturedContext = false)
        {
            var originalContext = SynchronizationContext.Current;

            if (originalContext is null)
                return task.GetAwaiter().GetResult();

            var state = new TaskResultState<T>();

            var internalContext = synchrorizationContextPool.Get();
            var mre = new ManualResetEvent(false);

            var executionThread = new Thread(obj =>
            {
                var threadState = (TaskResultState<T>)obj;
                RunSync(internalContext, originalContext, task, continueOnCapturedContext, threadState, mre);
            });

            executionThread.Start();
            mre.WaitOne();

            synchrorizationContextPool.Return(internalContext);

            return state.Result;
        }

        /// <summary>
        /// Executa sincronamente a Task.
        /// </summary>
        /// <param name="delegate">Function thats produces the Task.</param>
        /// <param name="continueOnCapturedContext">Aplicado no <see cref="Task.ConfigureAwait(bool)"/>.</param>
        public static void RunSync(Func<Task> @delegate, bool continueOnCapturedContext = false)
        {
            var originalContext = SynchronizationContext.Current;

            if (originalContext == null)
            {
                @delegate().GetAwaiter().GetResult();
                return;
            }

            var internalContext = synchrorizationContextPool.Get();

            RunSync(internalContext, originalContext, @delegate(), continueOnCapturedContext);

            synchrorizationContextPool.Return(internalContext);
        }

        /// <summary>
        /// Executa sincronamente a Task.
        /// </summary>
        /// <param name="delegate">Function thats produces the Task.</param>
        /// <param name="continueOnCapturedContext">Aplicado no <see cref="Task.ConfigureAwait(bool)"/>.</param>
        public static void RunSyncOnNewThread(Func<Task> @delegate, bool continueOnCapturedContext = false)
        {
            var originalContext = SynchronizationContext.Current;

            if (originalContext == null)
            {
                @delegate().GetAwaiter().GetResult();
                return;
            }

            var internalContext = synchrorizationContextPool.Get();
            var mre = new ManualResetEvent(false);

            var executionThread = new Thread(state =>
            {
                RunSync(internalContext, originalContext, @delegate(), continueOnCapturedContext, mre);
            });

            executionThread.Start();
            mre.WaitOne();

            synchrorizationContextPool.Return(internalContext);
        }

        /// <summary>
        /// Executa sincronamente a Task.
        /// </summary>
        /// <typeparam name="T">Tipo do valor retornado pela task.</typeparam>
        /// <param name="delegate">Function thats produces the Task.</param>
        /// <param name="continueOnCapturedContext">Aplicado no <see cref="Task.ConfigureAwait(bool)"/>.</param>
        /// <returns>O resultado da Task.</returns>
        public static T RunSync<T>(Func<Task<T>> @delegate, bool continueOnCapturedContext = false)
        {
            var originalContext = SynchronizationContext.Current;

            if (originalContext is null)
                return @delegate().GetAwaiter().GetResult();

            var state = new TaskResultState<T>();

            var internalContext = synchrorizationContextPool.Get();

            RunSync(internalContext, originalContext, @delegate(), continueOnCapturedContext, state);

            synchrorizationContextPool.Return(internalContext);

            return state.Result;
        }

        /// <summary>
        /// Executa sincronamente a Task.
        /// </summary>
        /// <typeparam name="T">Tipo do valor retornado pela task.</typeparam>
        /// <param name="delegate">Function thats produces the Task.</param>
        /// <param name="continueOnCapturedContext">Aplicado no <see cref="Task.ConfigureAwait(bool)"/>.</param>
        /// <returns>O resultado da Task.</returns>
        public static T RunSyncOnNewThread<T>(Func<Task<T>> @delegate, bool continueOnCapturedContext = false)
        {
            var originalContext = SynchronizationContext.Current;

            if (originalContext is null)
                return @delegate().GetAwaiter().GetResult();

            var state = new TaskResultState<T>();

            var internalContext = synchrorizationContextPool.Get();
            var mre = new ManualResetEvent(false);

            var executionThread = new Thread(obj =>
            {
                var threadState = (TaskResultState<T>)obj;
                RunSync(internalContext, originalContext, @delegate(), continueOnCapturedContext, threadState, mre);
            });

            executionThread.Start();
            mre.WaitOne();

            synchrorizationContextPool.Return(internalContext);

            return state.Result;
        }

        private static void RunSync(
            InternalSynchronizationContext internalContext,
            SynchronizationContext originalContext,
            Task task, 
            bool continueOnCapturedContext,
            ManualResetEvent mre = null)
        {
            SynchronizationContext.SetSynchronizationContext(internalContext);
            try
            {
                internalContext.Post(async _ =>
                {
                    try
                    {
                        await task.ConfigureAwait(continueOnCapturedContext);
                    }
                    catch (Exception ex)
                    {
                        internalContext.InnerException = ex;
                        throw;
                    }
                    finally
                    {
                        internalContext.CompleteExecution();
                    }
                }, null);

                if (mre is not null)
                    internalContext.Post(_ => mre.Set(), null);

                internalContext.WaitForCompletion();
            }
            catch (AggregateException ex)
            {
                var exception = ex.TryGetSingleInnerException();

                if (exception is OperationCanceledException)
                    throw CreateTimeoutException(internalContext.Stopwatch.Elapsed, ex);

                ExceptionDispatchInfo.Capture(exception).Throw();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(originalContext);
            }
        }

        private static void RunSync<T>(
            InternalSynchronizationContext internalContext,
            SynchronizationContext originalContext,
            Task<T> task,
            bool continueOnCapturedContext,
            TaskResultState<T> threadState,
            ManualResetEvent mre = null)
        {
            RunSync(internalContext, originalContext, task, continueOnCapturedContext);
            SynchronizationContext.SetSynchronizationContext(internalContext);
            try
            {
                internalContext.Post(async _ =>
                {
                    try
                    {
                        threadState.Result = await task.ConfigureAwait(continueOnCapturedContext);
                    }
                    catch (Exception ex)
                    {
                        internalContext.InnerException = ex;
                        throw;
                    }
                    finally
                    {
                        internalContext.CompleteExecution();
                    }
                }, null);

                internalContext.Post(_ => mre.Set(), null);

                internalContext.WaitForCompletion();
            }
            catch (AggregateException ex)
            {
                var exception = ex.TryGetSingleInnerException();

                if (exception is OperationCanceledException)
                    throw CreateTimeoutException(internalContext.Stopwatch.Elapsed, ex);

                ExceptionDispatchInfo.Capture(exception).Throw();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(originalContext);
            }
        }

        private class InternalPooledObjectPolicy : IPooledObjectPolicy<InternalSynchronizationContext>
        {
            public InternalSynchronizationContext Create()
            {
                return new InternalSynchronizationContext();
            }

            public bool Return(InternalSynchronizationContext value)
            {
                value.Reset();
                return true;
            }
        }

        private class InternalSynchronizationContext : SynchronizationContext
        {
            private readonly AutoResetEvent autoReset = new(false);
            private readonly ConcurrentQueue<Tuple<SendOrPostCallback, object>> queue = new();

            private bool completed;

            public Exception InnerException { get; set; }

            public Stopwatch Stopwatch { get; } = new Stopwatch();

            public override void Send(SendOrPostCallback callback, object state)
            {
                throw new NotSupportedException("Send is not supported, only Post.");
            }

            public override void Post(SendOrPostCallback callback, object state)
            {
                queue.Enqueue(new(callback, state));
                autoReset.Set();
            }

            public void Reset()
            {
                autoReset.Reset();
                completed = false;
                InnerException = null;
                Stopwatch.Reset();
                while (queue.TryDequeue(out _)) ; // clear
            }

            public void CompleteExecution()
            {
                Post(_ =>
                {
                    completed = true;
                    Stopwatch.Stop();
                }, null);
            }

            public void WaitForCompletion()
            {
                Stopwatch.Start();

                OperationStarted();

                while (!completed)
                {
                    if (queue.IsEmpty)
                        autoReset.WaitOne();

                    while (queue.TryDequeue(out var task))
                    {
                        // Executa the callback (Item1) passing the state (Item2).
                        task.Item1(task.Item2);

                        if (InnerException is not null)
                        {
                            throw new AggregateException(InnerException);
                        }
                    }
                }

                OperationCompleted();
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }

        private class TaskResultState<T>
        {
            public T Result { get; set; }
        }

        /// <summary>
        /// Tenta obtém uma única exception interna, caso haja mais de uma, retorna <paramref name="ex"/>.
        /// </summary>
        /// <param name="ex">Aggregado de exceptions.</param>
        /// <returns>A exception mais interna.</returns>
        private static Exception TryGetSingleInnerException(this AggregateException ex)
        {
            while (true)
            {
                if (ex.InnerExceptions.Count is not 1)
                    return ex;

                if (ex.InnerExceptions[0] is not AggregateException aggregateException)
                    break;

                ex = aggregateException;
            }

            return ex.InnerExceptions[0];
        }

        private static TimeoutException CreateTimeoutException(TimeSpan elapsed, Exception innerException)
        {
            return new TimeoutException("Timeout after: " + elapsed, innerException);
        }
    }
}
