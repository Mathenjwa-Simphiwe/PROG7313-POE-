using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PROGPOE.Models;

namespace PROGPOE.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class ApiAccountController : ControllerBase
    {
        private readonly UserManager<Client> _cm;
        private readonly UserManager<Admin> _am;
        private readonly IConfiguration _cfg;
        public ApiAccountController(UserManager<Client> cm, UserManager<Admin> am, IConfiguration cfg) { _cm = cm; _am = am; _cfg = cfg; }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto d)
        {
            var admin = await _am.FindByEmailAsync(d.Email);
            if (admin != null && await _am.CheckPasswordAsync(admin, d.Password))
                return Ok(new { Token = Token(admin.Id, "Admin"), Role = "Admin", admin.FullName });
            var client = await _cm.FindByEmailAsync(d.Email);
            if (client != null && await _cm.CheckPasswordAsync(client, d.Password))
                return Ok(new { Token = Token(client.Id, "Client"), Role = "Client", client.FullName });
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        private string Token(string uid, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, uid), new Claim(ClaimTypes.Role, role) };
            var token = new JwtSecurityToken(_cfg["Jwt:Issuer"], _cfg["Jwt:Audience"], claims, expires: DateTime.Now.AddHours(3), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    public class LoginDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
}