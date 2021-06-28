using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.Tasks.Tests.StarvationApi1.Services
{
    public class App2Service
    {
        private static int Counter;

        private readonly IHttpClientFactory factory;
        private readonly ILoggerFactory loggerFactory;

        public App2Service(IHttpClientFactory factory, ILoggerFactory loggerFactory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task<int> GetNextValue()
        {
            var client = factory.CreateClient("app2");
            var response = await client.GetAsync($"api/Operation?number={Interlocked.Increment(ref Counter)}");
            var content = await response.Content.ReadAsStringAsync();
            var parsed = int.TryParse(content, out int value);

            if (parsed)
                return value;

            loggerFactory.CreateLogger<App2Service>()
                .LogError("Can't parse the content '{0}'", content);

            return 0;
        }
    }
}
