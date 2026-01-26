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
    public class ProductUnitController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductUnitController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ProductUnit
        // يدعم فلترة حسب productId (اختياري)
        // Supports filtering by productId (optional)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductUnitDto>>> GetProductUnits([FromQuery] long? productId)
        {
            var query = _context.ProductUnits
                .Include(pu => pu.Product)
                .Include(pu => pu.Unit)
                .AsQueryable();

            // فلترة حسب productId إذا تم تمريره
            // Filter by productId if provided
            if (productId.HasValue && productId.Value > 0)
            {
                query = query.Where(pu => pu.ProductId == productId.Value);
            }

            var productUnits = await query
                .Select(pu => new ProductUnitDto
                {
                    Id = pu.Id,
                    ProductId = pu.ProductId,
                    UnitId = pu.UnitId,
                    SalePrice = pu.SalePrice,
                    ConversionFactor = pu.ConversionFactor,
                    IsDefault = pu.IsDefault,
                    ProductName = pu.Product != null ? pu.Product.ProductName : null,
                    UnitName = pu.Unit != null ? pu.Unit.UnitName : null
                })
                .ToListAsync();

            return Ok(productUnits);
        }

        // GET: api/ProductUnit/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductUnitDto>> GetProductUnit(long id)
        {
            var productUnit = await _context.ProductUnits
                .Include(pu => pu.Product)
                .Include(pu => pu.Unit)
                .Where(pu => pu.Id == id)
                .Select(pu => new ProductUnitDto
                {
                    Id = pu.Id,
                    ProductId = pu.ProductId,
                    UnitId = pu.UnitId,
                    SalePrice = pu.SalePrice,
                    ConversionFactor = pu.ConversionFactor,
                    IsDefault = pu.IsDefault,
                    ProductName = pu.Product != null ? pu.Product.ProductName : null,
                    UnitName = pu.Unit != null ? pu.Unit.UnitName : null
                })
                .FirstOrDefaultAsync();

            if (productUnit == null)
                return NotFound();

            return Ok(productUnit);
        }

        // POST: api/ProductUnit
        [HttpPost]
        [RequirePermission("manage_products")]
        public async Task<ActionResult<ProductUnitDto>> PostProductUnit(CreateProductUnitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var productUnit = new ProductUnit
            {
                ProductId = dto.ProductId,
                UnitId = dto.UnitId,
                SalePrice = dto.SalePrice,
                ConversionFactor = dto.ConversionFactor,
                IsDefault = dto.IsDefault
            };

            _context.ProductUnits.Add(productUnit);
            await _context.SaveChangesAsync();

            var result = await _context.ProductUnits
                .Include(pu => pu.Product)
                .Include(pu => pu.Unit)
                .Where(pu => pu.Id == productUnit.Id)
                .Select(pu => new ProductUnitDto
                {
                    Id = pu.Id,
                    ProductId = pu.ProductId,
                    UnitId = pu.UnitId,
                    SalePrice = pu.SalePrice,
                    ConversionFactor = pu.ConversionFactor,
                    IsDefault = pu.IsDefault,
                    ProductName = pu.Product != null ? pu.Product.ProductName : null,
                    UnitName = pu.Unit != null ? pu.Unit.UnitName : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetProductUnit), new { id = productUnit.Id }, result);
        }

        // PUT: api/ProductUnit/5
        [HttpPut("{id}")]
        [RequirePermission("manage_products")]
        public async Task<IActionResult> PutProductUnit(long id, UpdateProductUnitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var productUnit = await _context.ProductUnits.FindAsync(id);
            if (productUnit == null)
                return NotFound();

            // التحقق من سلامة البيانات: منع تعديل معامل التحويل إذا تم استخدامه سابقاً
            // Data Integrity Check: Prevent modifying conversion factor if used previously
            if (productUnit.ConversionFactor != dto.ConversionFactor)
            {
                // Check if this ProductUnit (ProductId + UnitId) has been used in any InvoiceProduct
                var isUsedInInvoices = await _context.InvoiceProducts
                    .AnyAsync(ip => ip.ProductId == productUnit.ProductId && ip.UnitId == productUnit.UnitId);

                // Check if used in ReturnProducts
                var isUsedInReturns = await _context.ReturnProducts
                     .AnyAsync(rp => rp.ProductId == productUnit.ProductId && rp.UnitId == productUnit.UnitId);

                if (isUsedInInvoices || isUsedInReturns)
                {
                    return BadRequest(new 
                    { 
                        message = "لا يمكن تعديل معامل التحويل لهذه الوحدة لأنها استخدمت في فواتير أو مرتجعات سابقة. قم بإنشاء وحدة جديدة بدلاً من ذلك.",
                        error = "Cannot modify ConversionFactor for a unit used in existing transactions." 
                    });
                }
            }

            productUnit.SalePrice = dto.SalePrice;
            productUnit.ConversionFactor = dto.ConversionFactor;
            productUnit.IsDefault = dto.IsDefault;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductUnitExists(id))
                    return NotFound();
                else
                    throw;
            }

            // return NoContent();
            return Ok(dto);
        }

        // DELETE: api/ProductUnit/5
        [HttpDelete("{id}")]
        [RequirePermission("manage_products")]
        public async Task<IActionResult> DeleteProductUnit(long id)
        {
            var productUnit = await _context.ProductUnits.FindAsync(id);
            if (productUnit == null)
                return NotFound();

            _context.ProductUnits.Remove(productUnit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductUnitExists(long id) => _context.ProductUnits.Any(e => e.Id == id);
    }
}


