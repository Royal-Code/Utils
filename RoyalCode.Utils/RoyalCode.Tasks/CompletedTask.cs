using System;
using System.Threading.Tasks;

namespace RoyalCode.Tasks
{
    /// <summary>
    /// Utility class to get a task completed after an action or function has been executed.
    /// </summary>
    public static class CompletedTask
    {
        /// <summary>
        /// <para>
        ///     Get a task completed after the action has been executed.
        /// </para>
        /// <para>
        ///     The execution is surrounded by try/catch and when an exception occurs, a task from exception is returned.
        /// </para>
        /// </summary>
        /// <param name="action">Action to be executed.</param>
        /// <returns><see cref="Task.CompletedTask"/>.</returns>
        public static Task After(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// <para>
        ///     Get a task completed after the function has been executed.
        /// </para>
        /// <para>
        ///     The execution is surrounded by try/catch and when an exception occurs, a task from exception is returned.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="function">Func to be executed.</param>
        /// <returns><see cref="Task.FromResult{TResult}(TResult)"/>.</returns>
        public static Task<T> After<T>(Func<T> function)
        {
            function();

            try
            {
                var result = function();
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return Task.FromException<T>(ex);
            }
        }
    }
}
