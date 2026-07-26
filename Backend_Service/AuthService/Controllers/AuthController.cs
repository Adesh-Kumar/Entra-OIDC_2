using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("login")]
        public IActionResult Login([FromQuery] string? returnUrl = null)
        {
            var frontendUrl = _configuration["FrontendUrl"] ?? "https://localhost:5173";
            var redirectUrl = string.IsNullOrEmpty(returnUrl) ? frontendUrl : returnUrl;

            //RedirectUri is the URL to which the user will be redirected after a successful login.
            // If returnUrl is provided, it will be used; otherwise, the frontendUrl will be used as 
            // the default redirect URL.
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token", new CookieOptions { SameSite = SameSiteMode.None, Secure = true });

            var frontendUrl = _configuration["FrontendUrl"] ?? "https://localhost:5173";
            return SignOut(new AuthenticationProperties { RedirectUri = frontendUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpGet("user")]
        public IActionResult GetUser()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new
                {
                    Name = User.Identity.Name,
                    Claims = User.Claims.Select(c => new { c.Type, c.Value })
                });
            }
            return Unauthorized();
        }
    }
}
