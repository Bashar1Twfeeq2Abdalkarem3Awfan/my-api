using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;
using MyAPIv3.DTOs;

namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserRoleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserRole
        // عرض ربط المستخدمين بالأدوار يتطلب على الأقل صلاحية عرض المستخدمين
        [HttpGet]
        [AllowAnonymous] // TEMPORARY - Remove after setup!
        //[MyAPIv3.Attributes.RequirePermission("view_users")]
        public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUserRoles()
        {
            var userRoles = await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Select(ur => new UserRoleDto
                {
                    UserId = ur.UserId,
                    RoleId = ur.RoleId,
                    AssignedAt = ur.AssignedAt,
                    User = ur.User != null ? new UserDto
                    {
                        Id = ur.User.Id,
                        PersonId = ur.User.PersonId,
                        Username = ur.User.Username,
                        LoginName = ur.User.LoginName,
                        LastLogin = ur.User.LastLogin,
                        CreatedAt = ur.User.CreatedAt,
                        UpdatedAt = ur.User.UpdatedAt
                    } : null,
                    Role = ur.Role != null ? new RoleDto
                    {
                        Id = ur.Role.Id,
                        RoleName = ur.Role.RoleName,
                        Description = ur.Role.Description,
                        IsActive = ur.Role.IsActive,
                        CreatedAt = ur.Role.CreatedAt
                    } : null
                })
                .ToListAsync();

            return Ok(userRoles);
        }

        // POST: api/UserRole
        // تعيين دور لمستخدم يتطلب صلاحية خاصة 'assign_roles'
        [HttpPost]
        [AllowAnonymous] // TEMPORARY - Remove after setup!
        //[MyAPIv3.Attributes.RequirePermission("assign_roles")]
        public async Task<ActionResult<UserRoleDto>> PostUserRole(CreateUserRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.UserRoles.AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId))
            {
                return BadRequest("This user already has this role.");
            }

            var userRole = new UserRole
            {
                UserId = dto.UserId,
                RoleId = dto.RoleId,
                AssignedAt = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            var result = await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId)
                .Select(ur => new UserRoleDto
                {
                    UserId = ur.UserId,
                    RoleId = ur.RoleId,
                    AssignedAt = ur.AssignedAt,
                    User = ur.User != null ? new UserDto
                    {
                        Id = ur.User.Id,
                        PersonId = ur.User.PersonId,
                        Username = ur.User.Username,
                        LoginName = ur.User.LoginName,
                        LastLogin = ur.User.LastLogin,
                        CreatedAt = ur.User.CreatedAt,
                        UpdatedAt = ur.User.UpdatedAt
                    } : null,
                    Role = ur.Role != null ? new RoleDto
                    {
                        Id = ur.Role.Id,
                        RoleName = ur.Role.RoleName,
                        Description = ur.Role.Description,
                        IsActive = ur.Role.IsActive,
                        CreatedAt = ur.Role.CreatedAt
                    } : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetUserRoles), new { userId = dto.UserId, roleId = dto.RoleId }, result);
        }

        // DELETE: api/UserRole?userId=1&roleId=2
        // إزالة دور من مستخدم يتطلب أيضاً 'assign_roles'
        [HttpDelete]
        [AllowAnonymous] // TEMPORARY - Remove after setup!
        //[MyAPIv3.Attributes.RequirePermission("assign_roles")]
        public async Task<IActionResult> DeleteUserRole(long userId, long roleId)
        {
            var userRole = await _context.UserRoles.FindAsync(userId, roleId);
            if (userRole == null)
                return NotFound();

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}


