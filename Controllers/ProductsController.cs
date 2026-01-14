using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.DTOs;
using MyAPIv3.Models;
using MyAPIv3.Data;
using MyAPIv3.Attributes;
using Microsoft.AspNetCore.Authorization;


namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // ====================
        // GET ALL PRODUCTS
        // ====================
        [HttpGet]
        [RequirePermission("view_products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductUnits!).ThenInclude(pu => pu.Unit)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    QrCode = p.QrCode,
                    SellingPrice = p.SellingPrice,
                    Notes = p.Notes,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryTitle = p.Category != null ? p.Category.Title : null,
                    UnitId = p.ProductUnits!.Where(pu => pu.IsDefault).Select(pu => (long?)pu.UnitId).FirstOrDefault(),
                    Unit = p.ProductUnits!.Where(pu => pu.IsDefault).Select(pu => new UnitDto
                    {
                        Id = pu.Unit!.Id,
                        UnitName = pu.Unit.UnitName,
                        IsActive = pu.Unit.IsActive
                    }).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(products);
        }

        // ====================
        // GET PRODUCT BY ID
        // ====================
        [HttpGet("{id}")]
        [RequirePermission("view_products")]
        public async Task<ActionResult<ProductDto>> GetProduct(long id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductUnits!).ThenInclude(pu => pu.Unit)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    QrCode = p.QrCode,
                    SellingPrice = p.SellingPrice,
                    Notes = p.Notes,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryTitle = p.Category != null ? p.Category.Title : null,
                    UnitId = p.ProductUnits!.Where(pu => pu.IsDefault).Select(pu => (long?)pu.UnitId).FirstOrDefault(),
                    Unit = p.ProductUnits!.Where(pu => pu.IsDefault).Select(pu => new UnitDto
                    {
                        Id = pu.Unit!.Id,
                        UnitName = pu.Unit.UnitName,
                        IsActive = pu.Unit.IsActive
                    }).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // ====================
        // CREATE PRODUCT
        // ====================
        [HttpPost]
        [RequirePermission("create_product")]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                ProductName = dto.ProductName,
                // تحويل string فارغ إلى null
                // Convert empty string to null
                QrCode = string.IsNullOrWhiteSpace(dto.QrCode) ? null : dto.QrCode,
                SellingPrice = dto.SellingPrice,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes,
                IsActive = dto.IsActive,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Create Default Unit if provided
            if (dto.UnitId.HasValue)
            {
                var productUnit = new ProductUnit
                {
                    ProductId = product.Id,
                    UnitId = dto.UnitId.Value,
                    SalePrice = product.SellingPrice,
                    ConversionFactor = 1, // Default 1:1
                    IsDefault = true
                };
                _context.ProductUnits.Add(productUnit);
                await _context.SaveChangesAsync();
            }

            var result = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductUnits!).ThenInclude(pu => pu.Unit)
                .Where(p => p.Id == product.Id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    QrCode = p.QrCode,
                    SellingPrice = p.SellingPrice,
                    Notes = p.Notes,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryTitle = p.Category != null ? p.Category.Title : null,
                    UnitId = p.ProductUnits!.Where(pu => pu.IsDefault).Select(pu => (long?)pu.UnitId).FirstOrDefault(),
                    Unit = p.ProductUnits!.Where(pu => pu.IsDefault).Select(pu => new UnitDto
                    {
                        Id = pu.Unit!.Id,
                        UnitName = pu.Unit.UnitName,
                        IsActive = pu.Unit.IsActive
                    }).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, result);
        }

        // ====================
        // UPDATE PRODUCT    ����� ������ ������
        // ====================
        [HttpPut("{id}")]
        [RequirePermission("edit_product")]
        public async Task<IActionResult> UpdateProduct(long id, UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.ProductName = dto.ProductName;
            // تحويل string فارغ إلى null
            // Convert empty string to null
            product.QrCode = string.IsNullOrWhiteSpace(dto.QrCode) ? null : dto.QrCode;
            product.SellingPrice = dto.SellingPrice;
            product.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;
            product.IsActive = dto.IsActive;
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            // تحديث UnitId إذا تم توفيره
            // Update UnitId if provided
            if (dto.UnitId.HasValue)
            {
                // البحث عن ProductUnit الافتراضي الحالي
                // Find current default ProductUnit
                var currentDefaultUnit = await _context.ProductUnits
                    .FirstOrDefaultAsync(pu => pu.ProductId == product.Id && pu.IsDefault);

                if (currentDefaultUnit != null)
                {
                    // إذا تغيرت الوحدة، تحديث أو إنشاء ProductUnit جديد
                    // If unit changed, update or create new ProductUnit
                    if (currentDefaultUnit.UnitId != dto.UnitId.Value)
                    {
                        // إلغاء الافتراضية من الوحدة القديمة
                        // Remove default from old unit
                        currentDefaultUnit.IsDefault = false;

                        // البحث عن ProductUnit موجود للوحدة الجديدة
                        // Find existing ProductUnit for new unit
                        var existingProductUnit = await _context.ProductUnits
                            .FirstOrDefaultAsync(pu => pu.ProductId == product.Id && pu.UnitId == dto.UnitId.Value);

                        if (existingProductUnit != null)
                        {
                            // تحديث الوحدة الموجودة لتكون افتراضية
                            // Update existing unit to be default
                            existingProductUnit.IsDefault = true;
                            existingProductUnit.SalePrice = product.SellingPrice;
                        }
                        else
                        {
                            // إنشاء ProductUnit جديد
                            // Create new ProductUnit
                            var newProductUnit = new ProductUnit
                            {
                                ProductId = product.Id,
                                UnitId = dto.UnitId.Value,
                                SalePrice = product.SellingPrice,
                                ConversionFactor = 1,
                                IsDefault = true
                            };
                            _context.ProductUnits.Add(newProductUnit);
                        }
                    }
                    else
                    {
                        // نفس الوحدة، فقط تحديث السعر إذا تغير
                        // Same unit, just update price if changed
                        if (currentDefaultUnit.SalePrice != product.SellingPrice)
                        {
                            currentDefaultUnit.SalePrice = product.SellingPrice;
                        }
                    }
                }
                else
                {
                    // لا توجد وحدة افتراضية، إنشاء واحدة جديدة
                    // No default unit, create new one
                    var newProductUnit = new ProductUnit
                    {
                        ProductId = product.Id,
                        UnitId = dto.UnitId.Value,
                        SalePrice = product.SellingPrice,
                        ConversionFactor = 1,
                        IsDefault = true
                    };
                    _context.ProductUnits.Add(newProductUnit);
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ====================
        // DELETE PRODUCT
        // ====================
        [HttpDelete("{id}")]
        [RequirePermission("delete_product")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            // التحقق من وجود المنتج في فواتير قبل الحذف
            // Check if product is used in invoices before deletion
            var isUsedInInvoices = await _context.InvoiceProducts
                .AnyAsync(ip => ip.ProductId == id);

            if (isUsedInInvoices)
            {
                return BadRequest(new 
                { 
                    message = $"لا يمكن حذف المنتج '{product.ProductName}' لأنه مستخدم في فواتير",
                    productId = id,
                    productName = product.ProductName
                });
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return NoContent(); // 204 No Content - المعيار الصحيح لعمليات الحذف
            }
            catch (DbUpdateException ex)
            {
                // معالجة أخطاء قاعدة البيانات (مثل Foreign Key constraints)
                // Handle database errors (like Foreign Key constraints)
                return BadRequest(new 
                { 
                    message = $"لا يمكن حذف المنتج '{product.ProductName}': {ex.InnerException?.Message ?? ex.Message}",
                    productId = id,
                    productName = product.ProductName
                });
            }
        }
    }
}
