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
                }).RunSync();
            }

            Assert.Equal(SimpleExecutionContext.ExecutionCount, SimpleExecutionContext.WithoutResultSyncExecutionCount);
        }

        [Fact]
        public void Test_03_SyncExecution_Without_Result_OnNewThread()
        {
            for (int i = 0; i < SimpleExecutionContext.ExecutionCount; i++)
            {
                Task.Run(() =>
                {
                    SimpleExecutionContext.WithoutResultSyncExecutionOnNewThreadCount++;
                }).RunSyncOnNewThread();
            }

            Assert.Equal(SimpleExecutionContext.ExecutionCount, SimpleExecutionContext.WithoutResultSyncExecutionOnNewThreadCount);
        }

        [Fact]
        public void Test_04_AsyncExecution_With_Result()
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
        public void Test_05_SyncExecution_With_Result()
        {
            var max = 0;
            for (int i = 0; i < SimpleExecutionContext.ExecutionCount; i++)
            {
                max = Math.Max(max, Task.Run(() =>
                {
                    return ++SimpleExecutionContext.WithResultSyncExecutionCount;
                }).RunSync());
            }

            Assert.Equal(SimpleExecutionContext.ExecutionCount, SimpleExecutionContext.WithResultSyncExecutionCount);
            Assert.Equal(SimpleExecutionContext.WithResultSyncExecutionCount, max);
        }

        [Fact]
        public void Test_06_SyncExecution_With_Result_OnNewThread()
        {
            var max = 0;
            for (int i = 0; i < SimpleExecutionContext.ExecutionCount; i++)
            {
                max = Math.Max(max, Task.Run(() =>
                {
                    return ++SimpleExecutionContext.WithResultSyncExecutionOnNewThreadCount;
                }).RunSyncOnNewThread());
            }

            Assert.Equal(SimpleExecutionContext.ExecutionCount, SimpleExecutionContext.WithResultSyncExecutionOnNewThreadCount);
            Assert.Equal(SimpleExecutionContext.WithResultSyncExecutionOnNewThreadCount, max);
        }
    }
}
