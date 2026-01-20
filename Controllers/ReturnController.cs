using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;
using MyAPIv3.DTOs;
using MyAPIv3.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReturnController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Return
        [HttpGet]
        [RequirePermission("view_returns")]
        public async Task<ActionResult<IEnumerable<ReturnDto>>> GetReturns()
        {
            var returns = await _context.Returns
                .Include(r => r.OriginalInvoice)
                .Include(r => r.Client) // ← إضافة جديدة
                .Include(r => r.Supplier) // ← إضافة جديدة
                .Include(r => r.ReturnProducts)
                    .ThenInclude(rp => rp.Product)
                .Include(r => r.ReturnProducts)  // ← إضافة جديدة
                    .ThenInclude(rp => rp.Unit) // ← إضافة جديدة
                .Select(r => new ReturnDto
                {
                    Id = r.Id,
                    CashierNumber = r.CashierNumber,
                    OriginalInvoiceId = r.OriginalInvoiceId,
                    ReturnType = r.ReturnType,
                    InvoiceStatus = r.InvoiceStatus,
                    ReturnDate = r.ReturnDate,
                    Notes = r.Notes,
                    ClientId = r.ClientId, // ← إضافة جديدة
                    SupplierId = r.SupplierId, // ← إضافة جديدة
                    CreatedAt = r.CreatedAt,
                    // ← إضافة معلومات العميل/المورد
                    Client = r.Client != null ? new PersonDto
                    {
                        Id = r.Client.Id,
                        FirstName = r.Client.FirstName,
                        SecondName = r.Client.SecondName,
                        ThirdWithLastname = r.Client.ThirdWithLastname,
                        Email = r.Client.Email,
                        PhoneNumber = r.Client.PhoneNumber,
                        IsActive = r.Client.IsActive,
                        PersonType = r.Client.PersonType ?? "Customer" // إضافة PersonType 12/18/2025
                    } : null,
                    Supplier = r.Supplier != null ? new PersonDto
                    {
                        Id = r.Supplier.Id,
                        FirstName = r.Supplier.FirstName,
                        SecondName = r.Supplier.SecondName,
                        ThirdWithLastname = r.Supplier.ThirdWithLastname,
                        Email = r.Supplier.Email,
                        PhoneNumber = r.Supplier.PhoneNumber,
                        IsActive = r.Supplier.IsActive,
                        PersonType = r.Supplier.PersonType ?? "Supplier" // إضافة PersonType
                    } : null,
                    ReturnProducts = r.ReturnProducts != null ? r.ReturnProducts.Select(rp => new ReturnProductDto
                    {
                        Id = rp.Id,
                        ReturnId = rp.ReturnId,
                        ProductId = rp.ProductId,
                        UnitId = rp.UnitId, // ← إضافة جديدة
                        Quantity = rp.Quantity,
                        Notes = rp.Notes,
                        ProductName = rp.Product != null ? rp.Product.ProductName : null,
                        UnitName = rp.Unit != null ? rp.Unit.UnitName : null // ← إضافة جديدة
                    }).ToList() : null
                })
                .ToListAsync();

            return Ok(returns);
        }

        // GET: api/Return/5
        [HttpGet("{id}")]
        [RequirePermission("view_returns")]
        public async Task<ActionResult<ReturnDto>> GetReturn(long id)
        {
            var returnTbl = await _context.Returns
                .Include(r => r.OriginalInvoice)
                .Include(r => r.Client) // ← إضافة جديدة
                .Include(r => r.Supplier) // ← إضافة جديدة
                .Include(r => r.ReturnProducts)
                    .ThenInclude(rp => rp.Product)
                .Include(r => r.ReturnProducts) // ← إضافة جديدة
                    .ThenInclude(rp => rp.Unit) // ← إضافة جديدة
                .Where(r => r.Id == id)
                .Select(r => new ReturnDto
                {
                    Id = r.Id,
                    CashierNumber = r.CashierNumber,
                    OriginalInvoiceId = r.OriginalInvoiceId,
                    ReturnType = r.ReturnType,
                    InvoiceStatus = r.InvoiceStatus,
                    ReturnDate = r.ReturnDate,
                    Notes = r.Notes,
                    ClientId = r.ClientId,  // ← إضافة جديدة
                    SupplierId = r.SupplierId,  // ← إضافة جديدة
                    CreatedAt = r.CreatedAt,
                    Client = r.Client != null ? new PersonDto
                    {
                        Id = r.Client.Id,
                        FirstName = r.Client.FirstName,
                        SecondName = r.Client.SecondName,
                        ThirdWithLastname = r.Client.ThirdWithLastname,
                        Email = r.Client.Email,
                        PhoneNumber = r.Client.PhoneNumber,
                        IsActive = r.Client.IsActive,
                        PersonType = r.Client.PersonType ?? "Customer" // إضافة PersonType
                    } : null,
                    Supplier = r.Supplier != null ? new PersonDto
                    {
                        Id = r.Supplier.Id,
                        FirstName = r.Supplier.FirstName,
                        SecondName = r.Supplier.SecondName,
                        ThirdWithLastname = r.Supplier.ThirdWithLastname,
                        Email = r.Supplier.Email,
                        PhoneNumber = r.Supplier.PhoneNumber,
                        IsActive = r.Supplier.IsActive,
                        PersonType = r.Supplier.PersonType ?? "Supplier" // إضافة PersonType
                    } : null,
                    ReturnProducts = r.ReturnProducts != null ? r.ReturnProducts.Select(rp => new ReturnProductDto
                    {
                        Id = rp.Id,
                        ReturnId = rp.ReturnId,
                        ProductId = rp.ProductId,
                        UnitId = rp.UnitId, // ← إضافة جديدة
                        Quantity = rp.Quantity,
                        Notes = rp.Notes,
                        ProductName = rp.Product != null ? rp.Product.ProductName : null,
                        UnitName = rp.Unit != null ? rp.Unit.UnitName : null // ← إضافة جديدة
                    }).ToList() : null
                })
                .FirstOrDefaultAsync();

            if (returnTbl == null)
                return NotFound();

            return Ok(returnTbl);
        }

        // GET: api/Return/invoice/{invoiceId}/returned-quantities
        /// <summary>
        /// الحصول على الكميات المرتجعة لكل منتج من فاتورة معينة
        /// Get returned quantities per product from a specific invoice
        /// </summary>
        [HttpGet("invoice/{invoiceId}/returned-quantities")]
        [RequirePermission("view_returns")]
        public async Task<ActionResult<IEnumerable<ReturnedQuantityDto>>> GetReturnedQuantitiesByInvoice(long invoiceId)
        {
            // جلب جميع المرتجعات المرتبطة بهذه الفاتورة
            // Get all returns linked to this invoice
            var returns = await _context.Returns
                .Where(r => r.OriginalInvoiceId == invoiceId)
                .Include(r => r.ReturnProducts)
                    .ThenInclude(rp => rp.Product)
                .Include(r => r.ReturnProducts)
                    .ThenInclude(rp => rp.Unit)
                .ToListAsync();

            // حساب إجمالي الكميات المرتجعة لكل منتج (ProductId + UnitId)
            // Calculate total returned quantities per product (ProductId + UnitId)
            var returnedQuantities = returns
                .SelectMany(r => r.ReturnProducts ?? new List<ReturnProduct>())
                .GroupBy(rp => new { rp.ProductId, rp.UnitId })
                .Select(g => new ReturnedQuantityDto
                {
                    ProductId = g.Key.ProductId,
                    UnitId = g.Key.UnitId,
                    TotalReturnedQuantity = g.Sum(rp => rp.Quantity),
                    ProductName = g.First().Product?.ProductName,
                    UnitName = g.First().Unit?.UnitName
                })
                .ToList();

            return Ok(returnedQuantities);
        }


        // POST: api/Return
        [HttpPost]
        [RequirePermission("create_return")]
        public async Task<ActionResult<ReturnDto>> PostReturn(CreateReturnDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ============================================================
            // إنشاء المرتجع الأساسي (مع الحقول الجديدة)
            // Create basic return (with new fields)
            // ============================================================
            
            // التحقق من أن ReturnType غير null
            if (string.IsNullOrWhiteSpace(dto.ReturnType))
            {
                return BadRequest(new { message = "ReturnType is required" });
            }
            
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            // Declare variable outside try block
            ReturnTbl returnTbl = null!;
            
            try {
                returnTbl = new ReturnTbl
                {
                    CashierNumber = dto.CashierNumber,
                    OriginalInvoiceId = dto.OriginalInvoiceId,
                    ReturnType = dto.ReturnType.Trim(), // التأكد من عدم وجود مسافات
                    InvoiceStatus = dto.InvoiceStatus,
                    ReturnDate = dto.ReturnDate,
                    Notes = dto.Notes,
                    ClientId = dto.ClientId, // ← إضافة جديدة
                    SupplierId = dto.SupplierId, // ← إضافة جديدة
                    CreatedAt = DateTime.UtcNow,
                    ReturnProducts = new List<ReturnProduct>() // تهيئة القائمة
                };

                // إضافة المنتجات المرتجعة (مع UnitId)
                // Add returned products (with UnitId)
                if (dto.ReturnProducts != null && dto.ReturnProducts.Any())
                {
                    foreach (var rpDto in dto.ReturnProducts)
                    {
                        returnTbl.ReturnProducts.Add(new ReturnProduct
                        {
                            ProductId = rpDto.ProductId,
                            UnitId = rpDto.UnitId, // ← إضافة جديدة
                            Quantity = rpDto.Quantity,
                            Notes = rpDto.Notes
                        });
                    }
                }

                _context.Returns.Add(returnTbl);
                
                // ============================================================
                // ربط المخزون تلقائياً
                // Automatically update inventory
                //
                // ملاحظة مهمة 18/12/2025:
                // كما في الفواتير، سيتم التعامل مع المخزون بوحدة أساسية واحدة لكل منتج
                // ويتم تحويل كمية المرتجع من وحدة الفاتورة (UnitId) إلى هذه الوحدة الأساسية
                // باستخدام ConversionFactor من ProductUnit.
                // ============================================================
                
                if (dto.ReturnProducts != null && dto.ReturnProducts.Any())
                {
                    foreach (var rpDto in dto.ReturnProducts)
                    {
                        // جلب وحدة المنتج لمعرفة معامل التحويل للوحدة الأساسية
                        // Get product unit to know conversion factor to base unit
                        // ملاحظة 18/12/2025:
                        // UnitId في CreateReturnProductDto يشير إلى جدول الوحدات (Unit.Id)
                        // لذلك نطابق باستخدام ProductId + UnitId (حقل UnitId في ProductUnit)
                        var productUnit = await _context.ProductUnits
                            .FirstOrDefaultAsync(pu => pu.ProductId == rpDto.ProductId && pu.UnitId == rpDto.UnitId);

                        var conversionFactor = productUnit?.ConversionFactor ?? 1.0m;
                        var baseQuantity = rpDto.Quantity * conversionFactor;

                        // البحث عن المنتج في المخزون
                        // Find product in inventory
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(inv => inv.ProductId == rpDto.ProductId);
                        
                        if (inventory != null)
                        {
                            if (dto.ReturnType == "ReturnFromCustomer")
                            {
                                // مرتجع من عميل → إضافة للمخزون (بوحدة أساسية)
                                // Return from customer → Add to inventory (in base unit)
                                inventory.Quantity += baseQuantity;
                            }
                            else if (dto.ReturnType == "ReturnToSupplier")
                            {
                                // مرتجع لمورد → خصم من المخزون (بوحدة أساسية)
                                // Return to supplier → Deduct from inventory (in base unit)
                                inventory.Quantity -= baseQuantity;
                                
                                // التحقق من عدم سالب
                                // Ensure non-negative quantity
                                if (inventory.Quantity < 0)
                                {
                                    return BadRequest(new
                                    {
                                        message = $"الكمية في المخزون للمنتج {rpDto.ProductId} غير كافية للإرجاع. المتوفر: {inventory.Quantity + baseQuantity}",
                                        availableQuantity = inventory.Quantity + baseQuantity,
                                        requestedQuantity = baseQuantity,
                                        productId = rpDto.ProductId
                                    });
                                }
                            }
                        }
                        else
                        {
                            if (dto.ReturnType == "ReturnFromCustomer")
                            {
                                // إنشاء سجل مخزون جديد عند الإرجاع من العميل (بوحدة أساسية)
                                // Create new inventory record for returns from customer (in base unit)
                                var newInventory = new Inventory
                                {
                                    ProductId = rpDto.ProductId,
                                    Quantity = baseQuantity,
                                    CreatedAt = DateTime.UtcNow
                                };
                                _context.Inventories.Add(newInventory);
                            }
                            else
                            {
                                // لا يمكن إرجاع منتج غير موجود في المخزون للمورد
                                // Cannot return non-existent inventory item to supplier
                                return BadRequest(new
                                {
                                    message = $"المنتج {rpDto.ProductId} غير موجود في المخزون",
                                    productId = rpDto.ProductId
                                });
                            }
                        }
                    }
                }
                
                // ============================================================
                // ربط المديونيات تلقائياً
                // Automatically update debts
                // ============================================================
                
                // حساب إجمالي قيمة المرتجع
                // Calculate total return value
                decimal totalReturnValue = 0;
                if (returnTbl.ReturnProducts != null)
                {
                    foreach (var rp in returnTbl.ReturnProducts)
                    {
                        var product = await _context.Products.FindAsync(rp.ProductId);
                        if (product != null)
                        {
                            // استخدام سعر البيع كقيمة افتراضية
                            // Use selling price as default value
                            totalReturnValue += rp.Quantity * product.SellingPrice;
                        }
                    }
                }
                
                // تحديث المديونية حسب نوع المرتجع
                // Update debt based on return type
                if (totalReturnValue > 0)
                {
                    if (dto.ReturnType == "ReturnFromCustomer" && dto.ClientId.HasValue)
                    {
                        // مرتجع من عميل → تنقيص مديونية العميل
                        // Return from customer → Decrease customer debt
                        var debt = await _context.Debts
                            .FirstOrDefaultAsync(d => d.ClientId == dto.ClientId.Value);
                        
                        if (debt != null)
                        {
                            debt.Debit -= totalReturnValue; // تقليل ما له عندنا
                            debt.Remaining -= totalReturnValue;
                            
                            // التأكد من عدم السالب
                            // Ensure non-negative
                            if (debt.Debit < 0) debt.Debit = 0;
                            if (debt.Remaining < 0) debt.Remaining = 0;
                        }
                    }
                    else if (dto.ReturnType == "ReturnToSupplier" && dto.SupplierId.HasValue)
                    {
                        // مرتجع لمورد → تنقيص مديونيتنا للمورد
                        // Return to supplier → Decrease our debt to supplier
                        var debt = await _context.Debts
                            .FirstOrDefaultAsync(d => d.SupplierId == dto.SupplierId.Value);
                        
                        if (debt != null)
                        {
                            debt.Credit -= totalReturnValue; // تقليل ما علينا له
                            debt.Remaining -= totalReturnValue;
                            
                            // التأكد من عدم السالب
                            // Ensure non-negative
                            if (debt.Credit < 0) debt.Credit = 0;
                            if (debt.Remaining < 0) debt.Remaining = 0;
                        }
                    }
                }
                
                // حفظ جميع التغييرات (المرتجع + المخزون + المديونيات)
                // Save all changes (return + inventory + debts)
                await _context.SaveChangesAsync();
                
                // تأكيد المعاملة
                await transaction.CommitAsync();

            } catch (Exception) {
                await transaction.RollbackAsync();
                throw;
            }

            var result = await _context.Returns
                .Include(r => r.OriginalInvoice)
                .Include(r => r.Client)
                .Include(r => r.Supplier)
                .Include(r => r.ReturnProducts)
                    .ThenInclude(rp => rp.Product)
                .Include(r => r.ReturnProducts)
                    .ThenInclude(rp => rp.Unit)
                .Where(r => r.Id == returnTbl.Id)
                .Select(r => new ReturnDto
                {
                    Id = r.Id,
                    CashierNumber = r.CashierNumber,
                    OriginalInvoiceId = r.OriginalInvoiceId,
                    ReturnType = r.ReturnType,
                    InvoiceStatus = r.InvoiceStatus,
                    ReturnDate = r.ReturnDate,
                    Notes = r.Notes,
                    ClientId = r.ClientId,
                    SupplierId = r.SupplierId,
                    CreatedAt = r.CreatedAt,
                    Client = r.Client != null ? new PersonDto
                    {
                        Id = r.Client.Id,
                        FirstName = r.Client.FirstName,
                        SecondName = r.Client.SecondName,
                        ThirdWithLastname = r.Client.ThirdWithLastname,
                        Email = r.Client.Email,
                        PhoneNumber = r.Client.PhoneNumber,
                        IsActive = r.Client.IsActive,
                        PersonType = r.Client.PersonType ?? "Customer" // إضافة PersonType
                    } : null,
                    Supplier = r.Supplier != null ? new PersonDto
                    {
                        Id = r.Supplier.Id,
                        FirstName = r.Supplier.FirstName,
                        SecondName = r.Supplier.SecondName,
                        ThirdWithLastname = r.Supplier.ThirdWithLastname,
                        Email = r.Supplier.Email,
                        PhoneNumber = r.Supplier.PhoneNumber,
                        IsActive = r.Supplier.IsActive,
                        PersonType = r.Supplier.PersonType ?? "Supplier" // إضافة PersonType
                    } : null,
                    ReturnProducts = r.ReturnProducts != null ? r.ReturnProducts.Select(rp => new ReturnProductDto
                    {
                        Id = rp.Id,
                        ReturnId = rp.ReturnId,
                        ProductId = rp.ProductId,
                        UnitId = rp.UnitId,
                        Quantity = rp.Quantity,
                        Notes = rp.Notes,
                        ProductName = rp.Product != null ? rp.Product.ProductName : null,
                        UnitName = rp.Unit != null ? rp.Unit.UnitName : null
                    }).ToList() : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetReturn), new { id = returnTbl.Id }, result);
        }

        // PUT: api/Return/5
        [HttpPut("{id}")]
        [RequirePermission("edit_return")]
        public async Task<IActionResult> PutReturn(long id, UpdateReturnDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var returnTbl = await _context.Returns.FindAsync(id);
            if (returnTbl == null)
                return NotFound();

            returnTbl.CashierNumber = dto.CashierNumber;
            returnTbl.OriginalInvoiceId = dto.OriginalInvoiceId;
            returnTbl.ReturnType = dto.ReturnType;
            returnTbl.InvoiceStatus = dto.InvoiceStatus;
            returnTbl.ReturnDate = dto.ReturnDate;
            returnTbl.Notes = dto.Notes;
            returnTbl.ClientId = dto.ClientId; // ← إضافة جديدة
            returnTbl.SupplierId = dto.SupplierId; // ← إضافة جديدة

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReturnExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Return/5
        [HttpDelete("{id}")]
        [RequirePermission("delete_return")]
        public async Task<IActionResult> DeleteReturn(long id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var returnTbl = await _context.Returns
                    .Include(r => r.ReturnProducts)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (returnTbl == null)
                    return NotFound();

                // =====================================
                // 1. Reverse Inventory Changes
                // =====================================
                if (returnTbl.ReturnProducts != null)
                {
                    foreach (var rp in returnTbl.ReturnProducts)
                    {
                        var productUnit = await _context.ProductUnits
                            .FirstOrDefaultAsync(pu => pu.ProductId == rp.ProductId && pu.UnitId == rp.UnitId);
                        
                        var factor = productUnit?.ConversionFactor ?? 1.0m;
                        var baseQty = rp.Quantity * factor;

                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductId == rp.ProductId);

                        if (inventory != null)
                        {
                            if (returnTbl.ReturnType == "ReturnFromCustomer")
                            {
                                // Was Added -> Now Deduct
                                inventory.Quantity -= baseQty;
                            }
                            else if (returnTbl.ReturnType == "ReturnToSupplier")
                            {
                                // Was Deducted -> Now Add
                                inventory.Quantity += baseQty;
                            }
                        }
                    }
                }

                // =====================================
                // 2. Reverse Debt Changes
                // =====================================
                decimal totalValue = 0;
                if (returnTbl.ReturnProducts != null)
                {
                    foreach (var rp in returnTbl.ReturnProducts)
                    {
                        var product = await _context.Products.FindAsync(rp.ProductId);
                        if (product != null)
                        {
                            // Using SellingPrice to mirror the logic used in PostReturn
                            totalValue += rp.Quantity * product.SellingPrice;
                        }
                    }
                }

                if (totalValue > 0)
                {
                    if (returnTbl.ReturnType == "ReturnFromCustomer" && returnTbl.ClientId.HasValue)
                    {
                        // Was Credit (Decreased Debt) -> Now Debit (Increase Debt back)
                        var debt = await _context.Debts
                            .FirstOrDefaultAsync(d => d.ClientId == returnTbl.ClientId.Value);
                        
                        if (debt != null)
                        {
                            debt.Debit += totalValue;
                            debt.Remaining += totalValue;
                        }
                    }
                    else if (returnTbl.ReturnType == "ReturnToSupplier" && returnTbl.SupplierId.HasValue)
                    {
                        // Was Debit (Decreased Credit) -> Now Credit (Increase Credit back)
                        var debt = await _context.Debts
                            .FirstOrDefaultAsync(d => d.SupplierId == returnTbl.SupplierId.Value);

                        if (debt != null)
                        {
                            debt.Credit += totalValue;
                            debt.Remaining += totalValue;
                        }
                    }
                }

                _context.Returns.Remove(returnTbl);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = $"فشل في حذف المرتجع: {ex.Message}" });
            }
        }

        private bool ReturnExists(long id) => _context.Returns.Any(e => e.Id == id);
    }
}



