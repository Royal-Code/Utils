using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.Tasks.Tests.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Tests!");

            Console.WriteLine("1: Google do not block");
            Console.WriteLine("2: local deadlock");
            Console.WriteLine("3: local task runner");
            Console.WriteLine("4: local async do not block");
            Console.Write("Your choise: ");
            var choise = Console.ReadLine();

            if (choise == "1")
                ThreadPoolStarvation_Google_Do_Not_Block();
            else if (choise == "2")
                ThreadPoolStarvation_Deadlock();
            else if (choise == "3")
                ThreadPoolStarvation_TaskRunner();
            else if (choise == "4")
                ThreadPoolStarvation_Async_Do_Not_Block();
            else
                Console.WriteLine("Value not accepted");

            Console.WriteLine("ThreadPoolStarvation finished!");

            //Console.ReadKey();
        }

        static void ThreadPoolStarvation_Google_Do_Not_Block()
        {
            ThreadPoolStarvation_HttpCall("https://www.google.com/search?q=starvation+number+");
        }

        static void ThreadPoolStarvation_Deadlock()
        {
            ThreadPoolStarvation_HttpCall("http://localhost:2001/api/Deadlock?q=starvation+number+");
        }

        static void ThreadPoolStarvation_TaskRunner()
        {
            ThreadPoolStarvation_HttpCall("http://localhost:2001/api/TaskRunner?q=starvation+number+");
        }

        static void ThreadPoolStarvation_Async_Do_Not_Block()
        {
            ThreadPoolStarvation_HttpCall("http://localhost:2001/api/Async?q=starvation+number+");
        }

        static void ThreadPoolStarvation_HttpCall(string baseEndpoint)
        {
            var watch = new Stopwatch();
            watch.Start();

            List<Task<int>> tasks = new(ThreadsExecutionContext.MultiThreadExecutionCount);

            for (int j = 0; j < ThreadsExecutionContext.MultiThreadExecutionCount; j++)
            {
                var task = Task.Run(async () =>
                {
                    using var client = new HttpClient();
                    var response = await client.GetAsync($"{baseEndpoint}{ThreadsExecutionContext.MultiThreadExecutionCount}");
                    _ = await response.Content.ReadAsStringAsync();
                    return Interlocked.Increment(ref ThreadsExecutionContext.MultiThreadExecutionCounter);
                });

                tasks.Add(task);
            }

            foreach (var task in tasks)
            {
                var value = task.GetAwaiter().GetResult();
            }

            Console.WriteLine($"Total executions: {ThreadsExecutionContext.MultiThreadExecutionCounter}, in: {watch.Elapsed}.");
        }
    }
}
