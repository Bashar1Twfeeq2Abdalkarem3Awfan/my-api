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
    public class RoleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Role
        // ملاحظة 18/12/2025:
        // هذه العملية تتطلب صلاحية 'view_roles' والتي تم تعريفها في جدول Permissions
        // وفي Flutter داخل permission_constants.dart لضمان تطابق كامل.
        [HttpGet]
        [MyAPIv3.Attributes.RequirePermission("view_roles")]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/Role/5
        // تتطلب نفس صلاحية العرض العامة للأدوار
        [HttpGet("{id}")]
        [MyAPIv3.Attributes.RequirePermission("view_roles")]
        public async Task<ActionResult<RoleDto>> GetRole(long id)
        {
            var role = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (role == null)
                return NotFound();

            return Ok(role);
        }

        // POST: api/Role
        // تتطلب صلاحية إنشاء دور جديد 'create_role'
        [HttpPost]
        [MyAPIv3.Attributes.RequirePermission("create_role")]
        public async Task<ActionResult<RoleDto>> PostRole(CreateRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = new Role
            {
                RoleName = dto.RoleName,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var result = new RoleDto
            {
                Id = role.Id,
                RoleName = role.RoleName,
                Description = role.Description,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt
            };

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, result);
        }

        // PUT: api/Role/5
        // تتطلب صلاحية تعديل دور 'edit_role'
        [HttpPut("{id}")]
        [MyAPIv3.Attributes.RequirePermission("edit_role")]
        public async Task<IActionResult> PutRole(long id, UpdateRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound();

            role.RoleName = dto.RoleName;
            role.Description = dto.Description;
            role.IsActive = dto.IsActive;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Roles.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Role/5
        // تتطلب صلاحية حذف دور 'delete_role'
        [HttpDelete("{id}")]
        [MyAPIv3.Attributes.RequirePermission("delete_role")]
        public async Task<IActionResult> DeleteRole(long id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound();

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}


