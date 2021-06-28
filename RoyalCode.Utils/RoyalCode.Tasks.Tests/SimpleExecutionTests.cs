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
    }
}
