using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiplomBackend.Controllers.Admin
{
    public class RoleController : AdminControllerBase
    {
        public RoleController(DiplomContext context) : base(context)
        {
        }

        // Получение всех ролей
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _context.Roles.ToListAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // Добавление новой роли
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] Role role)
        {
            try
            {
                if (string.IsNullOrEmpty(role.Name))
                {
                    return BadRequest("Role name is required.");
                }

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Role added successfully.", roleId = role.RoleId });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // Обновление роли по ID
        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRole(int roleId, [FromBody] Role updatedRole)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    return NotFound("Role not found.");
                }

                role.Name = updatedRole.Name;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Role updated successfully." });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // Удаление роли по ID
        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    return NotFound("Role not found.");
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Role deleted successfully." });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
