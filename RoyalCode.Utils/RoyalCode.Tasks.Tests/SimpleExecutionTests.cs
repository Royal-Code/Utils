using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RoyalCode.Tasks.Tests
{
    public class SimpleExecutionTests
    {
        [Fact]
        public void Test_01_AsyncExecution_Without_Result()
        {
            for (int i = 0; i < SimpleExecutionContext.ExecutionCount; i++)
            {
                Task.Run(() =>
                {
                    Interlocked.Increment(ref SimpleExecutionContext.WithoutResultAsyncExecutionCount);
                });
            }

            Assert.NotEqual(SimpleExecutionContext.ExecutionCount, SimpleExecutionContext.WithoutResultAsyncExecutionCount);
        }

        [Fact]
        public void Test_02_SyncExecution_Without_Result()
        {
            for (int i = 0; i < SimpleExecutionContext.ExecutionCount; i++)
            {
                Task.Run(() =>
                {
                    SimpleExecutionContext.WithoutResultSyncExecutionCount++;
                }).ExecuteSynchronously();
            }

            Assert.Equal(SimpleExecutionContext.ExecutionCount, SimpleExecutionContext.WithoutResultSyncExecutionCount);
        }

        [Fact]
        public void Test_03_AsyncExecution_With_Result()
        {
            for (int i = 0; i < SimpleExecutionContext.ExecutionCount; i++)
            {
                Task.Run(() =>
                {
                    return Interlocked.Increment(ref SimpleExecutionContext.WithResultAsyncExecutionCount);
                });
            }

            Assert.NotEqual(SimpleExecutionContext.ExecutionCount, SimpleExecutionContext.WithResultAsyncExecutionCount);
        }

        [Fact]
        public void Test_04_SyncExecution_With_Result()
        {
            var max = 0;
            for (int i = 0; i < SimpleExecutionContext.ExecutionCount; i++)
            {
                max = Math.Max(max, Task.Run(() =>
                {
                    return ++SimpleExecutionContext.WithResultSyncExecutionCount;
                }).GetResultSynchronously());
            }

            Assert.Equal(SimpleExecutionContext.ExecutionCount, SimpleExecutionContext.WithResultSyncExecutionCount);
            Assert.Equal(SimpleExecutionContext.WithResultSyncExecutionCount, max);
        }
    }
}
