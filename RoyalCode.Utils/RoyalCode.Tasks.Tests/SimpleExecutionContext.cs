namespace RoyalCode.Tasks.Tests
{
    class SimpleExecutionContext
    {
        public const int ExecutionCount = 1000;

        public static int WithoutResultAsyncExecutionCount = 0;
        public static int WithoutResultSyncExecutionCount = 0;

        public static int WithResultAsyncExecutionCount = 0;
        public static int WithResultSyncExecutionCount = 0;

    }
}
