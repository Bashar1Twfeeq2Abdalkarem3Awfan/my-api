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
    public class PermissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Permission
        // ملاحظة 18/12/2025:
        // هذه العمليات على الصلاحيات نفسها يتم حمايتها بصلاحيات خاصة:
        // view_permissions, create_permission, edit_permission, delete_permission
        // والمتطابقة مع PermissionName في جدول Permissions وثوابت Flutter.
        [HttpGet]
        [MyAPIv3.Attributes.RequirePermission("view_permissions")]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissions()
        {
            var permissions = await _context.Permissions
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    PermissionName = p.PermissionName,
                    Category = p.Category,
                    Description = p.Description,
                    Module = p.Module,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(permissions);
        }

        // GET: api/Permission/5
        [HttpGet("{id}")]
        [MyAPIv3.Attributes.RequirePermission("view_permissions")]
        public async Task<ActionResult<PermissionDto>> GetPermission(long id)
        {
            var permission = await _context.Permissions
                .Where(p => p.Id == id)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    PermissionName = p.PermissionName,
                    Category = p.Category,
                    Description = p.Description,
                    Module = p.Module,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (permission == null)
                return NotFound();

            return Ok(permission);
        }

        // POST: api/Permission
        [HttpPost]
        [MyAPIv3.Attributes.RequirePermission("create_permission")]
        public async Task<ActionResult<PermissionDto>> PostPermission(CreatePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var permission = new Permission
            {
                PermissionName = dto.PermissionName,
                Category = dto.Category,
                Description = dto.Description,
                Module = dto.Module,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            var result = new PermissionDto
            {
                Id = permission.Id,
                PermissionName = permission.PermissionName,
                Category = permission.Category,
                Description = permission.Description,
                Module = permission.Module,
                IsActive = permission.IsActive,
                CreatedAt = permission.CreatedAt
            };

            return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, result);
        }

        // PUT: api/Permission/5
        [HttpPut("{id}")]
        [MyAPIv3.Attributes.RequirePermission("edit_permission")]
        public async Task<IActionResult> PutPermission(long id, UpdatePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
                return NotFound();

            permission.PermissionName = dto.PermissionName;
            permission.Category = dto.Category;
            permission.Description = dto.Description;
            permission.Module = dto.Module;
            permission.IsActive = dto.IsActive;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Permissions.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Permission/5
        [HttpDelete("{id}")]
        [MyAPIv3.Attributes.RequirePermission("delete_permission")]
        public async Task<IActionResult> DeletePermission(long id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
                return NotFound();

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}


