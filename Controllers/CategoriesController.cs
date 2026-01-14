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
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/categories
        [HttpGet]
        [RequirePermission("view_products")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            var categories = await _db.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        [RequirePermission("view_products")]
        public async Task<ActionResult<CategoryDto>> GetById(long id)
        {
            var category = await _db.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        [RequirePermission("create_product")]
        public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                Title = dto.Title,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                Title = category.Title,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, result);
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        [RequirePermission("edit_product")]
        public async Task<IActionResult> Update(long id, UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            category.Title = dto.Title;
            category.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                Title = category.Title,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };

            return Ok(result);
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        [RequirePermission("delete_product")]
        public async Task<IActionResult> Delete(long id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}



