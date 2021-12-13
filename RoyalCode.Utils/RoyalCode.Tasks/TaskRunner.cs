using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.Tasks
{
    /// <summary>
    /// Utility class to run <see cref="Task"/> and <see cref="Task{TResult}"/> synchronously.
    /// </summary>
    public static class TaskRunner
    {
        private static readonly ObjectPool<InternalSynchronizationContext> synchrorizationContextPool
            = new DefaultObjectPool<InternalSynchronizationContext>(new InternalPooledObjectPolicy(), 100);

        /// <summary>
        /// Execute the Task Synchronously.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="continueOnCapturedContext">Applied on <see cref="Task.ConfigureAwait(bool)"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteSynchronously(this Task task, bool continueOnCapturedContext = false)
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
        /// Execute the Task Synchronously.
        /// </summary>
        /// <typeparam name="T">Type of task result.</typeparam>
        /// <param name="task">Task.</param>
        /// <param name="continueOnCapturedContext">Applied on <see cref="Task.ConfigureAwait(bool)"/>.</param>
        /// <returns>O resultado da Task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetResultSynchronously<T>(this Task<T> task, bool continueOnCapturedContext = false)
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
        /// Execute the Task Synchronously.
        /// </summary>
        /// <param name="delegate">Function thats produces the Task.</param>
        /// <param name="continueOnCapturedContext">Applied on <see cref="Task.ConfigureAwait(bool)"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Synchronously(Func<Task> @delegate, bool continueOnCapturedContext = false)
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
        /// Execute the Task Synchronously.
        /// </summary>
        /// <typeparam name="T">Type of task result.</typeparam>
        /// <param name="delegate">Function thats produces the Task.</param>
        /// <param name="continueOnCapturedContext">Applied on <see cref="Task.ConfigureAwait(bool)"/>.</param>
        /// <returns>O resultado da Task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Synchronously<T>(Func<Task<T>> @delegate, bool continueOnCapturedContext = false)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RunSync(
            InternalSynchronizationContext internalContext,
            SynchronizationContext originalContext,
            Task task, 
            bool continueOnCapturedContext)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RunSync<T>(
            InternalSynchronizationContext internalContext,
            SynchronizationContext originalContext,
            Task<T> task,
            bool continueOnCapturedContext,
            TaskResultState<T> resultState)
        {
            SynchronizationContext.SetSynchronizationContext(internalContext);
            try
            {
                internalContext.Post(async _ =>
                {
                    try
                    {
                        resultState.Result = await task.ConfigureAwait(continueOnCapturedContext);
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
        /// Try get deep single inner exception from AggregateException.
        /// </summary>
        /// <param name="ex">Some AggregateException.</param>
        /// <returns>The deep single inner exception from AggregateException or the AggregateException.</returns>
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
            return new TimeoutException($"Synchronously task execution canceled after: {elapsed}.", innerException);
        }
    }
}
