using Microsoft.AspNetCore.Mvc;
using RoyalCode.Tasks.Tests.StarvationApi1.Services;
using System.Threading.Tasks;

namespace RoyalCode.Tasks.Tests.StarvationApi1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsyncController : ControllerBase
    {
        private readonly App2Service service;

        public AsyncController(App2Service service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<int> Get()
        {
            return await service.GetNextValue();
        }
    }
}
