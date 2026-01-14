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
    public class InvoiceProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoiceProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceProduct
        [HttpGet]
        [RequirePermission("view_sales")]
        public async Task<ActionResult<IEnumerable<InvoiceProductDto>>> GetInvoiceProducts()
        {
            var invoiceProducts = await _context.InvoiceProducts
                .Include(ip => ip.Invoice)
                .Include(ip => ip.Product)
                .Include(ip => ip.Unit)
                .Select(ip => new InvoiceProductDto
                {
                    Id = ip.Id,
                    InvoiceId = ip.InvoiceId,
                    ProductId = ip.ProductId,
                    UnitId = ip.UnitId,
                    Quantity = ip.Quantity,
                    Subtotal = ip.Subtotal,
                    CreatedAt = ip.CreatedAt,
                    ProductName = ip.Product != null ? ip.Product.ProductName : null,
                    UnitName = ip.Unit != null ? ip.Unit.UnitName : null
                })
                .ToListAsync();

            return Ok(invoiceProducts);
        }

        // GET: api/InvoiceProduct/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceProductDto>> GetInvoiceProduct(long id)
        {
            var invoiceProduct = await _context.InvoiceProducts
                .Include(ip => ip.Invoice)
                .Include(ip => ip.Product)
                .Include(ip => ip.Unit)
                .Where(ip => ip.Id == id)
                .Select(ip => new InvoiceProductDto
                {
                    Id = ip.Id,
                    InvoiceId = ip.InvoiceId,
                    ProductId = ip.ProductId,
                    UnitId = ip.UnitId,
                    Quantity = ip.Quantity,
                    Subtotal = ip.Subtotal,
                    CreatedAt = ip.CreatedAt,
                    ProductName = ip.Product != null ? ip.Product.ProductName : null,
                    UnitName = ip.Unit != null ? ip.Unit.UnitName : null
                })
                .FirstOrDefaultAsync();

            if (invoiceProduct == null)
                return NotFound();

            return Ok(invoiceProduct);
        }

        // POST: api/InvoiceProduct
        [HttpPost]
        [RequirePermission("create_invoice")]
        public async Task<ActionResult<InvoiceProductDto>> PostInvoiceProduct(CreateInvoiceProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var invoiceProduct = new InvoiceProduct
            {
                InvoiceId = dto.InvoiceId,
                ProductId = dto.ProductId,
                UnitId = dto.UnitId,
                Quantity = dto.Quantity,
                Subtotal = dto.Subtotal,
                CreatedAt = DateTime.UtcNow
            };

            _context.InvoiceProducts.Add(invoiceProduct);
            await _context.SaveChangesAsync();

            var result = await _context.InvoiceProducts
                .Include(ip => ip.Product)
                .Include(ip => ip.Unit)
                .Where(ip => ip.Id == invoiceProduct.Id)
                .Select(ip => new InvoiceProductDto
                {
                    Id = ip.Id,
                    InvoiceId = ip.InvoiceId,
                    ProductId = ip.ProductId,
                    UnitId = ip.UnitId,
                    Quantity = ip.Quantity,
                    Subtotal = ip.Subtotal,
                    CreatedAt = ip.CreatedAt,
                    ProductName = ip.Product != null ? ip.Product.ProductName : null,
                    UnitName = ip.Unit != null ? ip.Unit.UnitName : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetInvoiceProduct), new { id = invoiceProduct.Id }, result);
        }

        // PUT: api/InvoiceProduct/5
        [HttpPut("{id}")]
        [RequirePermission("edit_invoice")]
        public async Task<IActionResult> PutInvoiceProduct(long id, UpdateInvoiceProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var invoiceProduct = await _context.InvoiceProducts.FindAsync(id);
            if (invoiceProduct == null)
                return NotFound();

            invoiceProduct.ProductId = dto.ProductId;
            invoiceProduct.UnitId = dto.UnitId;
            invoiceProduct.Quantity = dto.Quantity;
            invoiceProduct.Subtotal = dto.Subtotal;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceProductExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/InvoiceProduct/5
        [HttpDelete("{id}")]
        [RequirePermission("delete_invoice")]
        public async Task<IActionResult> DeleteInvoiceProduct(long id)
        {
            var invoiceProduct = await _context.InvoiceProducts.FindAsync(id);
            if (invoiceProduct == null)
                return NotFound();

            _context.InvoiceProducts.Remove(invoiceProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceProductExists(long id) => _context.InvoiceProducts.Any(e => e.Id == id);
    }
}


