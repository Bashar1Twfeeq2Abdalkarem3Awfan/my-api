using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;
using MyAPIv3.DTOs;
using MyAPIv3.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventory
        // يدعم فلترة حسب productId (اختياري)
        // Supports filtering by productId (optional)
        [HttpGet]
        [RequirePermission("view_inventory")]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventories([FromQuery] long? productId)
        {
            var query = _context.Inventories
                .Include(i => i.Product)
                .AsQueryable();

            // فلترة حسب productId إذا تم تمريره
            // Filter by productId if provided
            if (productId.HasValue && productId.Value > 0)
            {
                query = query.Where(i => i.ProductId == productId.Value);
            }

            var inventories = await query
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    PurchaseInvoiceNumber = i.PurchaseInvoiceNumber,
                    ProductionDate = i.ProductionDate,
                    ExpiryDate = i.ExpiryDate,
                    Quantity = i.Quantity,
                    ProductId = i.ProductId,
                    CreatedAt = i.CreatedAt,
                    ProductName = i.Product != null ? i.Product.ProductName : null
                })
                .ToListAsync();

            return Ok(inventories);
        }

        // GET: api/Inventory/5
        [HttpGet("{id}")]
        [RequirePermission("view_inventory")]
        public async Task<ActionResult<InventoryDto>> GetInventory(long id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.Id == id)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    PurchaseInvoiceNumber = i.PurchaseInvoiceNumber,
                    ProductionDate = i.ProductionDate,
                    ExpiryDate = i.ExpiryDate,
                    Quantity = i.Quantity,
                    ProductId = i.ProductId,
                    CreatedAt = i.CreatedAt,
                    ProductName = i.Product != null ? i.Product.ProductName : null
                })
                .FirstOrDefaultAsync();

            if (inventory == null)
                return NotFound();

            return Ok(inventory);
        }

        // POST: api/Inventory
        [HttpPost]
        [RequirePermission("adjust_inventory")]
        public async Task<ActionResult<InventoryDto>> PostInventory(CreateInventoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var inventory = new Inventory
            {
                PurchaseInvoiceNumber = dto.PurchaseInvoiceNumber,
                ProductionDate = dto.ProductionDate,
                ExpiryDate = dto.ExpiryDate,
                Quantity = dto.Quantity,
                ProductId = dto.ProductId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            var result = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.Id == inventory.Id)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    PurchaseInvoiceNumber = i.PurchaseInvoiceNumber,
                    ProductionDate = i.ProductionDate,
                    ExpiryDate = i.ExpiryDate,
                    Quantity = i.Quantity,
                    ProductId = i.ProductId,
                    CreatedAt = i.CreatedAt,
                    ProductName = i.Product != null ? i.Product.ProductName : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetInventory), new { id = inventory.Id }, result);
        }

        // PUT: api/Inventory/5
        [HttpPut("{id}")]
        [RequirePermission("adjust_inventory")]
        public async Task<IActionResult> PutInventory(long id, UpdateInventoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
                return NotFound();

            inventory.PurchaseInvoiceNumber = dto.PurchaseInvoiceNumber;
            inventory.ProductionDate = dto.ProductionDate;
            inventory.ExpiryDate = dto.ExpiryDate;
            inventory.Quantity = dto.Quantity;
            inventory.ProductId = dto.ProductId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Inventory/5
        [HttpDelete("{id}")]
        [RequirePermission("adjust_inventory")]
        public async Task<IActionResult> DeleteInventory(long id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
                return NotFound();

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InventoryExists(long id) => _context.Inventories.Any(e => e.Id == id);
    }
}


