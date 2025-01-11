using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController() : ControllerBase
    {
        [HttpGet]
        [Route("api/health/liveness")]
        public IActionResult GetLiveness()
        {
            return Ok("Service is healthy");
        }

        [HttpGet]
        [Route("api/health/readiness")]
        public IActionResult GetReadiness()
        {
            return Ok("Service is healthy");
        }
    }
}
