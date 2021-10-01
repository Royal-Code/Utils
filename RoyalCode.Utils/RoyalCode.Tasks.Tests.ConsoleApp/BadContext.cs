using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RoyalCode.Tasks.Tests.ConsoleApp
{
    public class BadContextService
    {
        private readonly TheSyncronousService theSyncronousService= new TheSyncronousService();

        public async Task<bool> Something(string input)
        {
            var result = theSyncronousService.SomethingSyncThatCallAsync(input);

            await Task.Delay(1);

            return true;
        }
    }

    public class TheSyncronousService
    {
        private readonly HttpClient client = new();

        public string SomethingSyncThatCallAsync(string input)
        {
            return TheSyncronizer(input);
        }

        private string TheSyncronizer(string input)
        {
            return TheAsyncCall(input).GetResultSynchronously();
        }

        private async Task<string> TheAsyncCall(string input)
        {
            var response = await client.GetAsync($"https://www.google.com/search?q=starvation+number+{input}");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
