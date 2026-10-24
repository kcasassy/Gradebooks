using Gradebooks.Gradebook;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Gradebook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly List<User> _users = new();
        private static int _nextId = 1;
        private static readonly PasswordHasher<User> _hasher = new();
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config) => _config = config;
        public record AuthRequest(string Username, string Password);
        [HttpPost("register")]
        public IActionResult Register(AuthRequest req)
        {
            if (_users.Any(u => u.Username == req.Username))
                return BadRequest("Username already taken.");
            var user = new User
            {
                Id = _nextId++,
                Username = req.Username,
                Role = req.Username == "prof" ? "Instructor" : "Student"   // demo shortcut
            };
            user.PasswordHash = _hasher.HashPassword(user, req.Password);
            _users.Add(user);
            return Created("", new { user.Id, user.Username, user.Role });
        }
        [HttpPost("login")]
        public IActionResult Login(AuthRequest req)
        {
            var user = _users.FirstOrDefault(u => u.Username == req.Username);
            if (user is null) return Unauthorized("Invalid credentials.");
            if (_hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password)
                    == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials.");
            return Ok(new { token = CreateToken(user) });
        }
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),   // identity -> used by /me
            new Claim(ClaimTypes.Role, user.Role)        // role     -> used by [Authorize(Roles)]
        };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"], audience: _config["Jwt:Audience"],
                claims: claims, expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}