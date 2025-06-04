using AIRecipeApp.Api.Context;
using AIRecipeApp.Api.Entities;
using AIRecipeApp.Api.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AIRecipeApp.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _config;
        private readonly RoleService _roleService;

        public AuthController(MongoDbContext context, IConfiguration config, RoleService roleService)
        {
            _context = context;
            _config = config;
            _roleService = roleService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var existingUser = await _context.Users.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
            if (existingUser != null)
                return BadRequest("Bu kullanıcı adı zaten alınmış.");

            await _context.Users.InsertOneAsync(user);

            // Varsayılan olarak "User" rolünü ata
            var defaultRole = await _context.Roles.Find(r => r.Name == "User").FirstOrDefaultAsync();
            if (defaultRole != null)
            {
                await _roleService.AssignRoleToUserAsync(user.Id, defaultRole.Id);
            }

            return Ok(new { message = "Kullanıcı başarıyla kaydedildi!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var existingUser = await _context.Users.Find(u => u.Username == user.Username && u.Password == user.Password).FirstOrDefaultAsync();
            if (existingUser == null)
                return Unauthorized("Kullanıcı adı veya şifre hatalı!");

            // Kullanıcının rollerini al
            var userRoles = await _roleService.GetUserRolesAsync(existingUser.Id);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, existingUser.Username)
            };

            // Rolleri claims'e ekle
            foreach (var role in userRoles)
            {
                claims.Add(new Claim("role", role));
            }

            var token = GenerateJwtToken(claims);
            return Ok(new { token });
        }

        private string GenerateJwtToken(List<Claim> claims)
        {
            var keyString = _config["Jwt:Key"];

            if (string.IsNullOrEmpty(keyString) || keyString.Length < 32)
            {
                throw new ArgumentException("JWT key must be at least 32 characters long.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}