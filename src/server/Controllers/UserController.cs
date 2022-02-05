using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public string GetUser()
        {
            return "Roger";
        }

        [HttpPost]
        public IActionResult Login()
        {
            return Ok();
        }
    }
}
