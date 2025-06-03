using AIRecipeApp.Api.Authorization;
using AIRecipeApp.Api.Context;
using AIRecipeApp.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace AIRecipeApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.AdminOnly)]
    public class AdminController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public AdminController(MongoDbContext context)
        {
            _context = context;
        }

        // Sadece Admin kullanıcılar tüm kullanıcıları görebilir
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.Find(_ => true).ToListAsync();
            var userList = users.Select(u => new 
            { 
                Id = u.Id, 
                Username = u.Username, 
                Role = u.Role.ToString() 
            });
            
            return Ok(userList);
        }

        // Sadece Admin kullanıcılar başka kullanıcılara rol atayabilir
        [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] Role newRole)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.Role, newRole);
            
            var result = await _context.Users.UpdateOneAsync(filter, update);
            
            if (result.ModifiedCount == 0)
                return NotFound("Kullanıcı bulunamadı.");
                
            return Ok(new { message = $"Kullanıcı rolü {newRole} olarak güncellendi." });
        }

        // Sadece Admin kullanıcılar başka kullanıcıları silebilir
        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == currentUserId)
                return BadRequest("Kendi hesabınızı silemezsiniz.");
                
            var result = await _context.Users.DeleteOneAsync(u => u.Id == userId);
            
            if (result.DeletedCount == 0)
                return NotFound("Kullanıcı bulunamadı.");
                
            return Ok(new { message = "Kullanıcı başarıyla silindi." });
        }

        // Admin paneli için istatistikler
        [HttpGet("stats")]
        public async Task<IActionResult> GetStatistics()
        {
            var totalUsers = await _context.Users.CountDocumentsAsync(_ => true);
            var totalRecipes = await _context.Recipes.CountDocumentsAsync(_ => true);
            var adminCount = await _context.Users.CountDocumentsAsync(u => u.Role == Role.Admin);
            var moderatorCount = await _context.Users.CountDocumentsAsync(u => u.Role == Role.Moderator);
            var userCount = await _context.Users.CountDocumentsAsync(u => u.Role == Role.User);

            return Ok(new 
            { 
                TotalUsers = totalUsers,
                TotalRecipes = totalRecipes,
                AdminCount = adminCount,
                ModeratorCount = moderatorCount,
                UserCount = userCount
            });
        }
    }
} 