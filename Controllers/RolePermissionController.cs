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
    public class RolePermissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolePermissionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RolePermission
        // عرض ربط الأدوار بالصلاحيات يتطلب صلاحية إدارة الصلاحيات
        // Using 'manage_permissions' كصلاحية عليا لإدارة الربط
        [HttpGet]
        [MyAPIv3.Attributes.RequirePermission("manage_permissions")]
        public async Task<ActionResult<IEnumerable<RolePermissionDto>>> GetRolePermissions()
        {
            var rolePermissions = await _context.RolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Select(rp => new RolePermissionDto
                {
                    RoleId = rp.RoleId,
                    PermissionId = rp.PermissionId,
                    GrantedAt = rp.GrantedAt,
                    Role = rp.Role != null ? new RoleDto
                    {
                        Id = rp.Role.Id,
                        RoleName = rp.Role.RoleName,
                        Description = rp.Role.Description,
                        IsActive = rp.Role.IsActive,
                        CreatedAt = rp.Role.CreatedAt
                    } : null,
                    Permission = rp.Permission != null ? new PermissionDto
                    {
                        Id = rp.Permission.Id,
                        PermissionName = rp.Permission.PermissionName,
                        Category = rp.Permission.Category,
                        Description = rp.Permission.Description,
                        Module = rp.Permission.Module,
                        IsActive = rp.Permission.IsActive,
                        CreatedAt = rp.Permission.CreatedAt
                    } : null
                })
                .ToListAsync();

            return Ok(rolePermissions);
        }

        // POST: api/RolePermission
        // إنشاء ربط جديد بين دور وصلاحية يتطلب 'manage_permissions'
        [HttpPost]
        [MyAPIv3.Attributes.RequirePermission("manage_permissions")]
        public async Task<ActionResult<RolePermissionDto>> PostRolePermission(CreateRolePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.RolePermissions.AnyAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId))
            {
                return BadRequest("This role already has this permission.");
            }

            var rolePermission = new RolePermission
            {
                RoleId = dto.RoleId,
                PermissionId = dto.PermissionId,
                GrantedAt = DateTime.UtcNow
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();

            var result = await _context.RolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId)
                .Select(rp => new RolePermissionDto
                {
                    RoleId = rp.RoleId,
                    PermissionId = rp.PermissionId,
                    GrantedAt = rp.GrantedAt,
                    Role = rp.Role != null ? new RoleDto
                    {
                        Id = rp.Role.Id,
                        RoleName = rp.Role.RoleName,
                        Description = rp.Role.Description,
                        IsActive = rp.Role.IsActive,
                        CreatedAt = rp.Role.CreatedAt
                    } : null,
                    Permission = rp.Permission != null ? new PermissionDto
                    {
                        Id = rp.Permission.Id,
                        PermissionName = rp.Permission.PermissionName,
                        Category = rp.Permission.Category,
                        Description = rp.Permission.Description,
                        Module = rp.Permission.Module,
                        IsActive = rp.Permission.IsActive,
                        CreatedAt = rp.Permission.CreatedAt
                    } : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetRolePermissions), new { roleId = dto.RoleId, permissionId = dto.PermissionId }, result);
        }

        // DELETE: api/RolePermission?roleId=1&permissionId=2
        // حذف ربط دور-صلاحية يتطلب 'manage_permissions'
        [HttpDelete]
        [MyAPIv3.Attributes.RequirePermission("manage_permissions")]
        public async Task<IActionResult> DeleteRolePermission(long roleId, long permissionId)
        {
            var rolePermission = await _context.RolePermissions.FindAsync(roleId, permissionId);
            if (rolePermission == null)
                return NotFound();

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}


