using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace RoyalCode.Tasks.Tests.StarvationApi2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationController : ControllerBase
    {
        private static int MultiThreadExecutionCounter = 0;

        [HttpGet]
        public Task<int> Get()
        {
            return Task.Run(() =>
            {
                Task.Delay(15);
                return Interlocked.Increment(ref MultiThreadExecutionCounter);
            });
        }
    }
}
