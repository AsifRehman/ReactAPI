using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthTest.Manager;
using AuthTest.Data.Models;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LoginController : ControllerBase
    {
        private readonly JwtAuthenticationManager jwtAuthenticationManager;
        public LoginController(JwtAuthenticationManager jwtAuthenticationManager)
        {
            this.jwtAuthenticationManager = jwtAuthenticationManager;
        }


        [HttpPost]
        [AllowAnonymous]
        public IActionResult Authorize([FromBody] User usr)
        {
            var token = jwtAuthenticationManager.Authenticate(usr.UserName, usr.Password);
            if (string.IsNullOrEmpty(token))
                return Unauthorized();
            return Ok(token);
        }

        [HttpGet]
        public IActionResult TestRoute()
        {
            return Ok("Authorized");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok("Anonymous");
        }
    }
}
