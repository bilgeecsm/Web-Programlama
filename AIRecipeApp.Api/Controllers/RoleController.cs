using AIRecipeApp.Api.Entities;
using AIRecipeApp.Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AIRecipeApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Role>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRoleById(string id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound();

            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole([FromBody] Role role)
        {
            var createdRole = await _roleService.CreateRoleAsync(role);
            return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] Role role)
        {
            var success = await _roleService.UpdateRoleAsync(id, role);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var success = await _roleService.DeleteRoleAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] RoleAssignmentRequest request)
        {
            var success = await _roleService.AssignRoleToUserAsync(request.UserId, request.RoleId);
            if (!success)
                return BadRequest("Rol atama iþlemi baþarýsýz oldu.");

            return Ok(new { message = "Rol baþarýyla atandý." });
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] RoleAssignmentRequest request)
        {
            var success = await _roleService.RemoveRoleFromUserAsync(request.UserId, request.RoleId);
            if (!success)
                return BadRequest("Rol kaldýrma iþlemi baþarýsýz oldu.");

            return Ok(new { message = "Rol baþarýyla kaldýrýldý." });
        }
    }

    public class RoleAssignmentRequest
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
}