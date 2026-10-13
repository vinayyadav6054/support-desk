using Microsoft.AspNetCore.Mvc;

namespace SupportDesk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "ok" });
    }
}
