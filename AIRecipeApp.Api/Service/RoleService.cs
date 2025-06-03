using AIRecipeApp.Api.Context;
using AIRecipeApp.Api.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIRecipeApp.Api.Service
{
    public class RoleService
    {
        private readonly IMongoCollection<Role> _roles;
        private readonly IMongoCollection<UserRole> _userRoles;

        public RoleService(MongoDbContext context)
        {
            _roles = context.GetCollection<Role>("roles");
            _userRoles = context.GetCollection<UserRole>("userRoles");
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _roles.Find(_ => true).ToListAsync();
        }

        public async Task<Role> GetRoleByIdAsync(string id)
        {
            return await _roles.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            await _roles.InsertOneAsync(role);
            return role;
        }

        public async Task<bool> UpdateRoleAsync(string id, Role role)
        {
            var result = await _roles.ReplaceOneAsync(r => r.Id == id, role);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteRoleAsync(string id)
        {
            var result = await _roles.DeleteOneAsync(r => r.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var userRole = await _userRoles.Find(ur => ur.UserId == userId).FirstOrDefaultAsync();
            if (userRole == null)
                return new List<string>();

            var roles = await _roles.Find(r => userRole.RoleIds.Contains(r.Id)).ToListAsync();
            return roles.Select(r => r.Name).ToList();
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleId)
        {
            var userRole = await _userRoles.Find(ur => ur.UserId == userId).FirstOrDefaultAsync();
            if (userRole == null)
            {
                userRole = new UserRole { UserId = userId, RoleIds = new List<string> { roleId } };
                await _userRoles.InsertOneAsync(userRole);
                return true;
            }

            if (!userRole.RoleIds.Contains(roleId))
            {
                userRole.RoleIds.Add(roleId);
                var result = await _userRoles.ReplaceOneAsync(ur => ur.UserId == userId, userRole);
                return result.ModifiedCount > 0;
            }

            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleId)
        {
            var userRole = await _userRoles.Find(ur => ur.UserId == userId).FirstOrDefaultAsync();
            if (userRole == null)
                return false;

            if (userRole.RoleIds.Contains(roleId))
            {
                userRole.RoleIds.Remove(roleId);
                var result = await _userRoles.ReplaceOneAsync(ur => ur.UserId == userId, userRole);
                return result.ModifiedCount > 0;
            }

            return true;
        }
    }
}