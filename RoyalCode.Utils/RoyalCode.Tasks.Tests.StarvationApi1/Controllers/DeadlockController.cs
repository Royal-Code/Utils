using Microsoft.AspNetCore.Mvc;
using RoyalCode.Tasks.Tests.StarvationApi1.Services;
using System.Threading.Tasks;

namespace RoyalCode.Tasks.Tests.StarvationApi1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeadlockController : ControllerBase
    {
        private readonly App2Service service;

        public DeadlockController(App2Service service)
        {
            this.service = service;
        }

        [HttpGet]
        public int Get()
        {
            return Task.Run(async () => await service.GetNextValue()).Result;
        }
    }
}
