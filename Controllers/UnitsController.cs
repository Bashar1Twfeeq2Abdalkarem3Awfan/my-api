using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;
using MyAPIv3.DTOs;
using MyAPIv3.Attributes;

namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UnitsController(AppDbContext db) => _db = db;

        // GET: api/Units
        [HttpGet]
        [RequirePermission("view_products")]
        public async Task<ActionResult<IEnumerable<UnitDto>>> GetAll()
        {
            var units = await _db.Units
                .Select(u => new UnitDto
                {
                    Id = u.Id,
                    UnitName = u.UnitName,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(units);
        }

        // GET: api/Units/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UnitDto>> GetById(long id)
        {
            var unit = await _db.Units
                .Where(u => u.Id == id)
                .Select(u => new UnitDto
                {
                    Id = u.Id,
                    UnitName = u.UnitName,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (unit == null)
                return NotFound();

            return Ok(unit);
        }

        // POST: api/Units
        [HttpPost]
        [RequirePermission("manage_products")]
        public async Task<ActionResult<UnitDto>> Create(CreateUnitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var unit = new Unit
            {
                UnitName = dto.UnitName,
                IsActive = dto.IsActive
            };

            _db.Units.Add(unit);
            await _db.SaveChangesAsync();

            var result = new UnitDto
            {
                Id = unit.Id,
                UnitName = unit.UnitName,
                IsActive = unit.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = unit.Id }, result);
        }

        // PUT: api/Units/5
        [HttpPut("{id}")]
        [RequirePermission("manage_products")]
        public async Task<IActionResult> Update(long id, UpdateUnitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var unit = await _db.Units.FindAsync(id);
            if (unit == null)
                return NotFound();

            unit.UnitName = dto.UnitName;
            unit.IsActive = dto.IsActive;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Units.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            // return NoContent();
            return Ok(dto);
        }

        // DELETE: api/Units/5
        [HttpDelete("{id}")]
        [RequirePermission("manage_products")]
        public async Task<IActionResult> Delete(long id, [FromQuery] bool force = false)
        {
            var unit = await _db.Units.FindAsync(id);
            if (unit == null)
                return NotFound();

            // Check for dependencies
            var linkedProductsCount = await _db.ProductUnits.CountAsync(pu => pu.UnitId == id);

            if (linkedProductsCount > 0 && !force)
            {
                // Return 409 Conflict with details
                return Conflict(new 
                { 
                    message = $"لا يمكن حذف الوحدة لأنها مرتبطة بـ {linkedProductsCount} منتج/منتجات.",
                    count = linkedProductsCount,
                    requiresForce = true 
                });
            }

            // If force is true or no dependencies, delete everything
            if (linkedProductsCount > 0) // force is implicitly true here
            {
                var dependencies = _db.ProductUnits.Where(pu => pu.UnitId == id);
                _db.ProductUnits.RemoveRange(dependencies);
            }

            _db.Units.Remove(unit);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}



