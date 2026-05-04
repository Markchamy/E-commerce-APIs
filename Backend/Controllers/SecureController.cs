using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class SecureController : ControllerBase
    {
        [Authorize]
        [HttpGet("secure-endpoint")]
        public IActionResult SecureEndpoint()
        {
            return Ok("This is a secure endpoint, accessible only to authenticated users.");
        }

        [AllowAnonymous]
        [HttpGet("public-endpoint")]
        public IActionResult PublicEndpoint()
        {
            return Ok("This is a public endpoint, accessible to everyone.");
        }
    }
}
