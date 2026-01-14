using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;
using MyAPIv3.DTOs;
using MyAPIv3.Attributes;
using System.Linq;

namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoiceController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// التحقق من امتلاك المستخدم لصلاحية معينة من JWT Token
        /// Helper method to check if current user has a specific permission from JWT.
        /// </summary>
        private bool UserHasPermissionAsync(string requiredPermission)
        {
            // قراءة الصلاحيات من JWT Claims بدلاً من قاعدة البيانات
            // Read permissions from JWT Claims instead of database
            var permissions = HttpContext.User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return permissions.Contains(requiredPermission);
        }

        // GET: api/Invoice
        [HttpGet]
        [RequirePermission("view_sales")] // لعرض جميع الفواتير
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoices(
            [FromQuery] string? invoiceType,
            [FromQuery] long? clientId,
            [FromQuery] long? supplierId)
        {
            var query = _context.Invoices.AsQueryable();

            if (!string.IsNullOrEmpty(invoiceType))
            {
                query = query.Where(i => i.InvoiceType == invoiceType);
            }

            if (clientId.HasValue)
            {
                query = query.Where(i => i.ClientId == clientId.Value);
            }

            if (supplierId.HasValue)
            {
                query = query.Where(i => i.SupplierId == supplierId.Value);
            }

            var invoices = await query
                .Include(i => i.Client)
                .Include(i => i.Supplier)
                .Include(i => i.CreatedByPerson)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Unit)
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    InvoiceType = i.InvoiceType,
                    PaymentReference = i.PaymentReference,
                    PaymentMethod = i.PaymentMethod,
                    TotalAmount = i.TotalAmount,
                    AmountPaid = i.AmountPaid,
                    AmountRemaining = i.AmountRemaining,
                    InvoiceDate = i.InvoiceDate,
                    ClientId = i.ClientId,
                    SupplierId = i.SupplierId,
                    CreatedBy = i.CreatedBy,
                    Notes = i.Notes,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt,
                    Client = i.Client != null ? new PersonDto
                    {
                        Id = i.Client.Id,
                        FirstName = i.Client.FirstName,
                        SecondName = i.Client.SecondName,
                        ThirdWithLastname = i.Client.ThirdWithLastname,
                        Email = i.Client.Email,
                        PhoneNumber = i.Client.PhoneNumber,
                        Address = i.Client.Address,
                        CreatedAt = i.Client.CreatedAt,
                        UpdatedAt = i.Client.UpdatedAt,
                        IsActive = i.Client.IsActive,
                        PersonType = i.Client.PersonType
                    } : null,
                    Supplier = i.Supplier != null ? new PersonDto
                    {
                        Id = i.Supplier.Id,
                        FirstName = i.Supplier.FirstName,
                        SecondName = i.Supplier.SecondName,
                        ThirdWithLastname = i.Supplier.ThirdWithLastname,
                        Email = i.Supplier.Email,
                        PhoneNumber = i.Supplier.PhoneNumber,
                        Address = i.Supplier.Address,
                        CreatedAt = i.Supplier.CreatedAt,
                        UpdatedAt = i.Supplier.UpdatedAt,
                        IsActive = i.Supplier.IsActive,
                        PersonType = i.Supplier.PersonType
                    } : null,
                    CreatedByPerson = i.CreatedByPerson != null ? new PersonDto
                    {
                        Id = i.CreatedByPerson.Id,
                        FirstName = i.CreatedByPerson.FirstName,
                        SecondName = i.CreatedByPerson.SecondName,
                        ThirdWithLastname = i.CreatedByPerson.ThirdWithLastname,
                        Email = i.CreatedByPerson.Email,
                        PhoneNumber = i.CreatedByPerson.PhoneNumber,
                        Address = i.CreatedByPerson.Address,
                        CreatedAt = i.CreatedByPerson.CreatedAt,
                        UpdatedAt = i.CreatedByPerson.UpdatedAt,
                        IsActive = i.CreatedByPerson.IsActive,
                        PersonType = i.CreatedByPerson.PersonType
                    } : null,
                    InvoiceProducts = i.InvoiceProducts.Select(ip => new InvoiceProductDto
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
                    }).ToList()
                })
                .ToListAsync();

            return Ok(invoices);
        }

        // GET: api/Invoice/ByPerson/5
        [HttpGet("ByPerson/{personId}")]
        [RequirePermission("view_sales")] // لعرض فواتير شخص معين
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoicesByPerson(long personId)
        {
            var invoices = await _context.Invoices
                .Where(i => i.ClientId == personId || i.SupplierId == personId)
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    InvoiceType = i.InvoiceType,
                    PaymentReference = i.PaymentReference,
                    PaymentMethod = i.PaymentMethod,
                    TotalAmount = i.TotalAmount,
                    AmountPaid = i.AmountPaid,
                    AmountRemaining = i.AmountRemaining,
                    InvoiceDate = i.InvoiceDate,
                    ClientId = i.ClientId,
                    SupplierId = i.SupplierId,
                    CreatedBy = i.CreatedBy,
                    Notes = i.Notes,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt,
                    Client = i.Client != null ? new PersonDto
                    {
                        Id = i.Client.Id,
                        FirstName = i.Client.FirstName,
                        SecondName = i.Client.SecondName,
                        ThirdWithLastname = i.Client.ThirdWithLastname,
                        Email = i.Client.Email,
                        PhoneNumber = i.Client.PhoneNumber,
                        Address = i.Client.Address,
                        CreatedAt = i.Client.CreatedAt,
                        UpdatedAt = i.Client.UpdatedAt,
                        IsActive = i.Client.IsActive,
                        PersonType = i.Client.PersonType
                    } : null,
                    Supplier = i.Supplier != null ? new PersonDto
                    {
                        Id = i.Supplier.Id,
                        FirstName = i.Supplier.FirstName,
                        SecondName = i.Supplier.SecondName,
                        ThirdWithLastname = i.Supplier.ThirdWithLastname,
                        Email = i.Supplier.Email,
                        PhoneNumber = i.Supplier.PhoneNumber,
                        Address = i.Supplier.Address,
                        CreatedAt = i.Supplier.CreatedAt,
                        UpdatedAt = i.Supplier.UpdatedAt,
                        IsActive = i.Supplier.IsActive,
                        PersonType = i.Supplier.PersonType
                    } : null,
                    CreatedByPerson = i.CreatedByPerson != null ? new PersonDto
                    {
                        Id = i.CreatedByPerson.Id,
                        FirstName = i.CreatedByPerson.FirstName,
                        SecondName = i.CreatedByPerson.SecondName,
                        ThirdWithLastname = i.CreatedByPerson.ThirdWithLastname,
                        Email = i.CreatedByPerson.Email,
                        PhoneNumber = i.CreatedByPerson.PhoneNumber,
                        Address = i.CreatedByPerson.Address,
                        CreatedAt = i.CreatedByPerson.CreatedAt,
                        UpdatedAt = i.CreatedByPerson.UpdatedAt,
                        IsActive = i.CreatedByPerson.IsActive,
                        PersonType = i.CreatedByPerson.PersonType
                    } : null,
                    InvoiceProducts = i.InvoiceProducts.Select(ip => new InvoiceProductDto
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
                    }).ToList()
                })
                .ToListAsync();

            return Ok(invoices);
        }

        // GET: api/Invoice/5
        [HttpGet("{id}")]
        [RequirePermission("view_sales")] // لعرض فاتورة محددة
        public async Task<ActionResult<InvoiceDto>> GetInvoice(long id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Supplier)
                .Include(i => i.CreatedByPerson)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Unit)
                .Where(i => i.Id == id)
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    InvoiceType = i.InvoiceType,
                    PaymentReference = i.PaymentReference,
                    PaymentMethod = i.PaymentMethod,
                    TotalAmount = i.TotalAmount,
                    AmountPaid = i.AmountPaid,
                    AmountRemaining = i.AmountRemaining,
                    InvoiceDate = i.InvoiceDate,
                    ClientId = i.ClientId,
                    SupplierId = i.SupplierId,
                    CreatedBy = i.CreatedBy,
                    Notes = i.Notes,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt,
                    Client = i.Client != null ? new PersonDto
                    {
                        Id = i.Client.Id,
                        FirstName = i.Client.FirstName,
                        SecondName = i.Client.SecondName,
                        ThirdWithLastname = i.Client.ThirdWithLastname,
                        Email = i.Client.Email,
                        PhoneNumber = i.Client.PhoneNumber,
                        Address = i.Client.Address,
                        CreatedAt = i.Client.CreatedAt,
                        UpdatedAt = i.Client.UpdatedAt,
                        IsActive = i.Client.IsActive,
                        PersonType = i.Client.PersonType
                    } : null,
                    Supplier = i.Supplier != null ? new PersonDto
                    {
                        Id = i.Supplier.Id,
                        FirstName = i.Supplier.FirstName,
                        SecondName = i.Supplier.SecondName,
                        ThirdWithLastname = i.Supplier.ThirdWithLastname,
                        Email = i.Supplier.Email,
                        PhoneNumber = i.Supplier.PhoneNumber,
                        Address = i.Supplier.Address,
                        CreatedAt = i.Supplier.CreatedAt,
                        UpdatedAt = i.Supplier.UpdatedAt,
                        IsActive = i.Supplier.IsActive,
                        PersonType = i.Supplier.PersonType
                    } : null,
                    CreatedByPerson = i.CreatedByPerson != null ? new PersonDto
                    {
                        Id = i.CreatedByPerson.Id,
                        FirstName = i.CreatedByPerson.FirstName,
                        SecondName = i.CreatedByPerson.SecondName,
                        ThirdWithLastname = i.CreatedByPerson.ThirdWithLastname,
                        Email = i.CreatedByPerson.Email,
                        PhoneNumber = i.CreatedByPerson.PhoneNumber,
                        Address = i.CreatedByPerson.Address,
                        CreatedAt = i.CreatedByPerson.CreatedAt,
                        UpdatedAt = i.CreatedByPerson.UpdatedAt,
                        IsActive = i.CreatedByPerson.IsActive,
                        PersonType = i.CreatedByPerson.PersonType
                    } : null,
                    InvoiceProducts = i.InvoiceProducts.Select(ip => new InvoiceProductDto
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
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }

        // POST: api/Invoice
        [HttpPost]
        // ملاحظة: نوع الفاتورة يحدد الصلاحية المطلوبة
        // Note: InvoiceType determines required permission
        public async Task<ActionResult<InvoiceDto>> PostInvoice([FromBody] CreateInvoiceDto dto)

        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // التحقق من الصلاحيات حسب نوع الفاتورة
            // Check permissions based on invoice type
            if (dto.InvoiceType == "Sale")
            {
                // إنشاء فاتورة مبيعات يتطلب create_invoice
                if (!UserHasPermissionAsync("create_invoice"))
                    return Forbid();
            }
            else if (dto.InvoiceType == "Purchase")
            {
                // إنشاء فاتورة مشتريات يتطلب create_purchase
                if (!UserHasPermissionAsync("create_purchase"))
                    return Forbid();
            }

            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            // Declare variable outside try block
            Invoice invoice = null!;
            
            try
            {
                invoice = new Invoice
                {
                    InvoiceType = dto.InvoiceType,
                    PaymentReference = dto.PaymentReference,
                    PaymentMethod = dto.PaymentMethod,
                    TotalAmount = dto.TotalAmount,
                    AmountPaid = dto.AmountPaid,
                    AmountRemaining = dto.AmountRemaining,
                    InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
                    ClientId = dto.ClientId,
                    SupplierId = dto.SupplierId,
                    CreatedBy = dto.CreatedBy,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                if (dto.InvoiceProducts != null && dto.InvoiceProducts.Any())
                {
                    foreach (var ipDto in dto.InvoiceProducts)
                    {
                        invoice.InvoiceProducts.Add(new InvoiceProduct
                        {
                            ProductId = ipDto.ProductId,
                            UnitId = ipDto.UnitId,
                            Quantity = ipDto.Quantity,
                            Subtotal = ipDto.Subtotal,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                _context.Invoices.Add(invoice);
                
                // حفظ الفاتورة أولاً للحصول على Id (للاستخدام في PurchaseInvoiceNumber)
                // Save invoice first to get Id (for use in PurchaseInvoiceNumber)
                await _context.SaveChangesAsync();
                
                // ============================================================
                // تحديث المخزون تلقائياً بناءً على نوع الفاتورة
                // Automatically update inventory based on invoice type
                //
                // ملاحظة مهمة 18/12/2025:
                // من الآن فصاعداً سيتم تخزين وتحريك المخزون بوحدة أساسية واحدة لكل منتج
                // (الوحدة الأساسية)، ويتم تحويل الكميات من الوحدة المختارة في الفاتورة
                // إلى هذه الوحدة الأساسية باستخدام ConversionFactor من ProductUnit.
                // Example: 1 كرتون (ConversionFactor = 5) => 5 حبات في المخزون.
                // ============================================================
                
                if (dto.InvoiceProducts != null && dto.InvoiceProducts.Any())
                {
                    foreach (var ipDto in dto.InvoiceProducts)
                    {
                        // جلب وحدة المنتج لمعرفة معامل التحويل للوحدة الأساسية
                        // Get ProductUnit to know conversion factor to base unit
                        // ملاحظة 18/12/2025:
                        // UnitId في CreateInvoiceProductDto يشير إلى جدول الوحدات (Unit.Id)
                        // وليس إلى ProductUnit.Id، لذلك يجب المطابقة بـ ProductId + UnitId (حقل UnitId في ProductUnit)
                        var productUnit = await _context.ProductUnits
                            .FirstOrDefaultAsync(pu => pu.ProductId == ipDto.ProductId && pu.UnitId == ipDto.UnitId);

                        // إذا لم يتم العثور على ProductUnit نعتبر معامل التحويل = 1 (لضمان الاستمرارية)
                        // If ProductUnit not found, assume conversionFactor = 1 as safe default
                        var conversionFactor = productUnit?.ConversionFactor ?? 1.0m;

                        // تحويل الكمية من وحدة الفاتورة إلى الوحدة الأساسية للمخزون
                        // Convert invoice quantity to base inventory unit
                        var baseQuantity = ipDto.Quantity * conversionFactor;

                        // البحث عن جميع سجلات المخزون للمنتج
                        // Find all inventory records for the product
                        var inventories = await _context.Inventories
                            .Where(inv => inv.ProductId == ipDto.ProductId)
                            .ToListAsync();
                        
                        // حساب إجمالي الكمية المتاحة من جميع السجلات (بوحدة أساسية)
                        // Calculate total available quantity from all records (in base unit)
                        var totalQuantity = inventories.Sum(inv => inv.Quantity);
                        
                        switch (dto.InvoiceType)
                        {
                            case "Sale":
                                // فاتورة بيع: خصم الكمية (بوحدة أساسية) من المخزون
                                // Sale invoice: Decrease inventory quantity (in base unit)
                                
                                // التحقق من الكمية المتاحة
                                // Check available quantity
                                if (totalQuantity < baseQuantity)
                                {
                                    return BadRequest(new 
                                    { 
                                        message = $"الكمية المتاحة في المخزون للمنتج {ipDto.ProductId} غير كافية. المتوفر: {totalQuantity}، المطلوب (بوحدة أساسية): {baseQuantity}",
                                        availableQuantity = totalQuantity,
                                        requestedQuantity = baseQuantity,
                                        productId = ipDto.ProductId
                                    });
                                }
                                
                                // خصم الكمية من السجلات (FIFO - First In First Out)
                                // Decrease quantity from records (FIFO - First In First Out)
                                var remainingToDeduct = baseQuantity;
                                foreach (var inv in inventories.OrderBy(inv => inv.CreatedAt))
                                {
                                    if (remainingToDeduct <= 0)
                                        break;
                                    
                                    if (inv.Quantity >= remainingToDeduct)
                                    {
                                        inv.Quantity -= remainingToDeduct;
                                        remainingToDeduct = 0;
                                    }
                                    else
                                    {
                                        remainingToDeduct -= inv.Quantity;
                                        inv.Quantity = 0;
                                    }
                                }
                                break;
                            
                            case "Purchase":
                                // فاتورة شراء: إضافة الكمية (بوحدة أساسية) إلى المخزون
                                // Purchase invoice: Increase inventory quantity (in base unit)
                                
                                // استخدام رقم الفاتورة كـ PurchaseInvoiceNumber
                                // Use invoice ID as PurchaseInvoiceNumber
                                var purchaseInvoiceNumber = invoice.Id.ToString();
                                
                                // إنشاء سجل جديد في المخزون لكل دفعة (لحفظ تاريخ الإنتاج والانتهاء)
                                // Create new inventory record for each batch (to save production and expiry dates)
                                var newInventory = new Inventory
                                {
                                    ProductId = ipDto.ProductId,
                                    Quantity = baseQuantity,
                                    PurchaseInvoiceNumber = ipDto.PurchaseInvoiceNumber ?? purchaseInvoiceNumber,
                                    ProductionDate = ipDto.ProductionDate,
                                    ExpiryDate = ipDto.ExpiryDate,
                                    CreatedAt = DateTime.UtcNow
                                };
                                _context.Inventories.Add(newInventory);
                                break;
                            
                            case "Return":
                                // فاتورة مرتجع: إرجاع الكمية (بوحدة أساسية) إلى المخزون
                                // Return invoice: Restore quantity to inventory (in base unit)
                                
                                if (inventories.Any())
                                {
                                    // إضافة الكمية إلى أول سجل موجود
                                    // Add quantity to first existing record
                                    inventories[0].Quantity += baseQuantity;
                                }
                                else
                                {
                                    // إنشاء سجل جديد في المخزون
                                    // Create new inventory record
                                    var returnInventory = new Inventory
                                    {
                                        ProductId = ipDto.ProductId,
                                        Quantity = baseQuantity,
                                        CreatedAt = DateTime.UtcNow
                                    };
                                    _context.Inventories.Add(returnInventory);
                                }
                                break;
                        }
                    }
                }
                
                // ============================================================
                // تحديث المديونيات تلقائياً
                // Automatically update debts
                // ============================================================
                
                // فقط إذا كان هناك مبلغ متبقي (الفاتورة ليست مدفوعة بالكامل)
                // Only if there's a remaining amount (invoice is not fully paid)
                if (invoice.AmountRemaining > 0)
                {
                    Debt? existingDebt = null;
                    
                    // البحث عن سجل دين موجود للشخص (عميل أو مورد)
                    // Search for existing debt record for the person (client or supplier)
                    if (dto.InvoiceType == "Sale" && dto.ClientId.HasValue)
                    {
                        existingDebt = await _context.Debts
                            .FirstOrDefaultAsync(d => d.ClientId == dto.ClientId.Value);
                    }
                    else if (dto.InvoiceType == "Purchase" && dto.SupplierId.HasValue)
                    {
                        existingDebt = await _context.Debts
                            .FirstOrDefaultAsync(d => d.SupplierId == dto.SupplierId.Value);
                    }
                    
                    if (existingDebt != null)
                    {
                        // تحديث سجل دين موجود
                        // Update existing debt record
                        
                        if (dto.InvoiceType == "Sale")
                        {
                            // فاتورة بيع: العميل مدين لنا
                            // Sale invoice: Customer owes us
                            existingDebt.Debit += invoice.TotalAmount;
                            existingDebt.Paid += invoice.AmountPaid;
                            existingDebt.Remaining += invoice.AmountRemaining;
                        }
                        else if (dto.InvoiceType == "Purchase")
                        {
                            // فاتورة شراء: نحن مدينون للمورد
                            // Purchase invoice: We owe the supplier
                            existingDebt.Credit += invoice.TotalAmount;
                            existingDebt.Paid += invoice.AmountPaid;
                            existingDebt.Remaining += invoice.AmountRemaining;
                        }
                        
                        // تحديث تاريخ آخر دفعة إذا تم الدفع
                        // Update last payment date if payment was made
                        if (invoice.AmountPaid > 0)
                        {
                            existingDebt.LastPaymentDate = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // إنشاء سجل دين جديد
                        // Create new debt record
                        
                        var newDebt = new Debt
                        {
                            AccountName = dto.InvoiceType == "Sale" 
                                ? "مديونية عميل" 
                                : "مديونية مورد",
                            AccountType = dto.InvoiceType.ToString(),
                            Debit = dto.InvoiceType == "Sale" 
                                ? invoice.TotalAmount 
                                : 0,
                            Credit = dto.InvoiceType == "Purchase" 
                                ? invoice.TotalAmount 
                                : 0,
                            Paid = invoice.AmountPaid,
                            Remaining = invoice.AmountRemaining,
                            ClientId = dto.ClientId,
                            SupplierId = dto.SupplierId,
                            CreatedBy = dto.CreatedBy,
                            CreatedAt = DateTime.UtcNow,
                            LastPaymentDate = invoice.AmountPaid > 0 
                                ? DateTime.UtcNow 
                                : null,
                            Notes = $"دين من فاتورة #{invoice.Id}"
                        };
                        
                        _context.Debts.Add(newDebt);
                    }
                }
                
                // حفظ جميع التغييرات (الفاتورة + المخزون + المديونيات)
                // Save all changes (invoice + inventory + debts)
                await _context.SaveChangesAsync();
                
                // تأكيد المعاملة
                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                // التراجع عند الخطأ
                await transaction.RollbackAsync();
                throw; // إعادة رمي الخطأ ليتم معالجته بواسطة Middleware أو يظهر كـ 500 Error
            }

            var result = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Supplier)
                .Include(i => i.CreatedByPerson)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Unit)
                .Where(i => i.Id == invoice.Id)
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    InvoiceType = i.InvoiceType,
                    PaymentReference = i.PaymentReference,
                    PaymentMethod = i.PaymentMethod,
                    TotalAmount = i.TotalAmount,
                    AmountPaid = i.AmountPaid,
                    AmountRemaining = i.AmountRemaining,
                    InvoiceDate = i.InvoiceDate,
                    ClientId = i.ClientId,
                    SupplierId = i.SupplierId,
                    CreatedBy = i.CreatedBy,
                    Notes = i.Notes,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt,
                    Client = i.Client != null ? new PersonDto
                    {
                        Id = i.Client.Id,
                        FirstName = i.Client.FirstName,
                        SecondName = i.Client.SecondName,
                        ThirdWithLastname = i.Client.ThirdWithLastname,
                        Email = i.Client.Email,
                        PhoneNumber = i.Client.PhoneNumber,
                        Address = i.Client.Address,
                        CreatedAt = i.Client.CreatedAt,
                        UpdatedAt = i.Client.UpdatedAt,
                        IsActive = i.Client.IsActive,
                        PersonType = i.Client.PersonType
                    } : null,
                    Supplier = i.Supplier != null ? new PersonDto
                    {
                        Id = i.Supplier.Id,
                        FirstName = i.Supplier.FirstName,
                        SecondName = i.Supplier.SecondName,
                        ThirdWithLastname = i.Supplier.ThirdWithLastname,
                        Email = i.Supplier.Email,
                        PhoneNumber = i.Supplier.PhoneNumber,
                        Address = i.Supplier.Address,
                        CreatedAt = i.Supplier.CreatedAt,
                        UpdatedAt = i.Supplier.UpdatedAt,
                        IsActive = i.Supplier.IsActive,
                        PersonType = i.Supplier.PersonType
                    } : null,
                    CreatedByPerson = i.CreatedByPerson != null ? new PersonDto
                    {
                        Id = i.CreatedByPerson.Id,
                        FirstName = i.CreatedByPerson.FirstName,
                        SecondName = i.CreatedByPerson.SecondName,
                        ThirdWithLastname = i.CreatedByPerson.ThirdWithLastname,
                        Email = i.CreatedByPerson.Email,
                        PhoneNumber = i.CreatedByPerson.PhoneNumber,
                        Address = i.CreatedByPerson.Address,
                        CreatedAt = i.CreatedByPerson.CreatedAt,
                        UpdatedAt = i.CreatedByPerson.UpdatedAt,
                        IsActive = i.CreatedByPerson.IsActive,
                        PersonType = i.CreatedByPerson.PersonType
                    } : null,
                    InvoiceProducts = i.InvoiceProducts.Select(ip => new InvoiceProductDto
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
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, result);
        }

        // PUT: api/Invoice/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoice(long id, UpdateInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // التحقق من الصلاحيات حسب نوع الفاتورة الجديدة
            // Check permissions for new invoice type
            if (dto.InvoiceType == "Sale")
            {
                if (!UserHasPermissionAsync("edit_invoice"))
                    return Forbid();
            }
            else if (dto.InvoiceType == "Purchase")
            {
                if (!UserHasPermissionAsync("edit_purchase"))
                    return Forbid();
            }

            var existingInvoice = await _context.Invoices
                .Include(i => i.InvoiceProducts)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (existingInvoice == null)
                return NotFound();

            // حفظ نوع الفاتورة القديم والمنتجات القديمة لتحديث المخزون
            // Save old invoice type and products for inventory update
            var oldInvoiceType = existingInvoice.InvoiceType;
            var oldInvoiceProducts = existingInvoice.InvoiceProducts.ToList();

            // إرجاع الكميات القديمة إلى المخزون (بوحدة أساسية باستخدام معامل التحويل)
            // Restore old quantities to inventory (in base unit using conversion factor)
            foreach (var oldIp in oldInvoiceProducts)
            {
                // جلب وحدة المنتج القديمة لمعرفة معامل التحويل للوحدة الأساسية
                // Get old product unit to know conversion factor to base unit
                // ملاحظة 18/12/2025:
                // هنا أيضاً نستخدم ProductId + UnitId (حقل UnitId في ProductUnit)
                // لأن UnitId في InvoiceProduct يشير إلى جدول الوحدات وليس ProductUnit.Id
                var oldProductUnit = await _context.ProductUnits
                    .FirstOrDefaultAsync(pu => pu.ProductId == oldIp.ProductId && pu.UnitId == oldIp.UnitId);

                var oldConversionFactor = oldProductUnit?.ConversionFactor ?? 1.0m;
                var oldBaseQuantity = oldIp.Quantity * oldConversionFactor;

                var inventories = await _context.Inventories
                    .Where(inv => inv.ProductId == oldIp.ProductId)
                    .ToListAsync();
                
                // إرجاع الكمية بناءً على نوع الفاتورة القديمة (بوحدة أساسية)
                // Restore quantity based on old invoice type (in base unit)
                if (oldInvoiceType == "Sale")
                {
                    // فاتورة بيع: إرجاع الكمية إلى المخزون
                    // Sale invoice: Restore quantity to inventory
                    if (inventories.Any())
                    {
                        inventories[0].Quantity += oldBaseQuantity;
                    }
                    else
                    {
                        var newInventory = new Inventory
                        {
                            ProductId = oldIp.ProductId,
                            Quantity = oldBaseQuantity,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Inventories.Add(newInventory);
                    }
                }
                else if (oldInvoiceType == "Purchase")
                {
                    // فاتورة شراء: خصم الكمية من المخزون
                    // Purchase invoice: Decrease quantity from inventory
                    if (inventories.Any())
                    {
                        var remainingToDeduct = oldBaseQuantity;
                        
                        foreach (var inv in inventories.OrderBy(inv => inv.CreatedAt))
                        {
                            if (remainingToDeduct <= 0)
                                break;
                            
                            if (inv.Quantity >= remainingToDeduct)
                            {
                                inv.Quantity -= remainingToDeduct;
                                remainingToDeduct = 0;
                            }
                            else
                            {
                                remainingToDeduct -= inv.Quantity;
                                inv.Quantity = 0;
                            }
                        }
                    }
                }
                else if (oldInvoiceType == "Return")
                {
                    // فاتورة مرتجع: خصم الكمية من المخزون
                    // Return invoice: Decrease quantity from inventory
                    if (inventories.Any())
                    {
                        var remainingToDeduct = oldBaseQuantity;
                        
                        foreach (var inv in inventories.OrderBy(inv => inv.CreatedAt))
                        {
                            if (remainingToDeduct <= 0)
                                break;
                            
                            if (inv.Quantity >= remainingToDeduct)
                            {
                                inv.Quantity -= remainingToDeduct;
                                remainingToDeduct = 0;
                            }
                            else
                            {
                                remainingToDeduct -= inv.Quantity;
                                inv.Quantity = 0;
                            }
                        }
                    }
                }
            }

            // حفظ القيم القديمة لتحديث Debt
            // Save old values for Debt update
            // ملاحظة: oldInvoiceType محفوظ بالفعل في السطر 646
            // Note: oldInvoiceType is already saved at line 646
            var oldTotalAmount = existingInvoice.TotalAmount;
            var oldAmountPaid = existingInvoice.AmountPaid;
            var oldAmountRemaining = existingInvoice.AmountRemaining;
            var oldClientId = existingInvoice.ClientId;
            var oldSupplierId = existingInvoice.SupplierId;

            // تحديث بيانات الفاتورة
            // Update invoice data
            existingInvoice.InvoiceType = dto.InvoiceType;
            existingInvoice.PaymentReference = dto.PaymentReference;
            existingInvoice.PaymentMethod = dto.PaymentMethod;
            existingInvoice.TotalAmount = dto.TotalAmount;
            existingInvoice.AmountPaid = dto.AmountPaid;
            existingInvoice.AmountRemaining = dto.AmountRemaining;
            existingInvoice.InvoiceDate = dto.InvoiceDate;
            existingInvoice.ClientId = dto.ClientId;
            existingInvoice.SupplierId = dto.SupplierId;
            existingInvoice.CreatedBy = dto.CreatedBy;
            existingInvoice.Notes = dto.Notes;
            existingInvoice.UpdatedAt = DateTime.UtcNow;

            // حذف المنتجات القديمة
            // Delete old products
            _context.InvoiceProducts.RemoveRange(existingInvoice.InvoiceProducts);

            // إضافة المنتجات الجديدة (إذا كانت موجودة في DTO)
            // Add new products (if provided in DTO)
            // ملاحظة: UpdateInvoiceDto لا يحتوي على InvoiceProducts
            // Note: UpdateInvoiceDto doesn't contain InvoiceProducts
            // لذلك لن نضيف منتجات جديدة هنا
            // So we won't add new products here

            // ============================================================
            // تحديث المديونيات تلقائياً عند تحديث الفاتورة
            // Automatically update debts when updating invoice
            // ============================================================
            
            // البحث عن سجل Debt للشخص (عميل أو مورد)
            // Search for debt record for the person (client or supplier)
            Debt? debtToUpdate = null;
            
            if (oldInvoiceType == "Sale" && oldClientId.HasValue)
            {
                debtToUpdate = await _context.Debts
                    .FirstOrDefaultAsync(d => d.ClientId == oldClientId.Value);
            }
            else if (oldInvoiceType == "Purchase" && oldSupplierId.HasValue)
            {
                debtToUpdate = await _context.Debts
                    .FirstOrDefaultAsync(d => d.SupplierId == oldSupplierId.Value);
            }

            if (debtToUpdate != null)
            {
                // إرجاع القيم القديمة
                // Reverse old values
                if (oldInvoiceType == "Sale")
                {
                    debtToUpdate.Debit -= oldTotalAmount;
                    debtToUpdate.Paid -= oldAmountPaid;
                    debtToUpdate.Remaining -= oldAmountRemaining;
                }
                else if (oldInvoiceType == "Purchase")
                {
                    debtToUpdate.Credit -= oldTotalAmount;
                    debtToUpdate.Paid -= oldAmountPaid;
                    debtToUpdate.Remaining -= oldAmountRemaining;
                }

                // إضافة القيم الجديدة
                // Add new values
                if (dto.InvoiceType == "Sale" && dto.ClientId.HasValue)
                {
                    debtToUpdate.Debit += dto.TotalAmount;
                    debtToUpdate.Paid += dto.AmountPaid;
                    debtToUpdate.Remaining += dto.AmountRemaining;
                }
                else if (dto.InvoiceType == "Purchase" && dto.SupplierId.HasValue)
                {
                    debtToUpdate.Credit += dto.TotalAmount;
                    debtToUpdate.Paid += dto.AmountPaid;
                    debtToUpdate.Remaining += dto.AmountRemaining;
                }

                // تحديث تاريخ آخر دفعة إذا تم الدفع
                // Update last payment date if payment was made
                if (dto.AmountPaid > oldAmountPaid)
                {
                    debtToUpdate.LastPaymentDate = DateTime.UtcNow;
                }

                // حذف سجل Debt إذا أصبح المتبقي صفر أو أقل
                // Delete debt record if remaining becomes zero or less
                if (debtToUpdate.Remaining <= 0)
                {
                    _context.Debts.Remove(debtToUpdate);
                }
            }
            else if (dto.AmountRemaining > 0)
            {
                // إنشاء سجل Debt جديد إذا لم يكن موجوداً
                // Create new debt record if it doesn't exist
                var newDebt = new Debt
                {
                    AccountName = dto.InvoiceType == "Sale" 
                        ? "مديونية عميل" 
                        : "مديونية مورد",
                    AccountType = dto.InvoiceType,
                    Debit = dto.InvoiceType == "Sale" ? dto.TotalAmount : 0,
                    Credit = dto.InvoiceType == "Purchase" ? dto.TotalAmount : 0,
                    Paid = dto.AmountPaid,
                    Remaining = dto.AmountRemaining,
                    ClientId = dto.ClientId,
                    SupplierId = dto.SupplierId,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    LastPaymentDate = dto.AmountPaid > 0 ? DateTime.UtcNow : null,
                    Notes = $"دين من فاتورة #{existingInvoice.Id}"
                };
                
                _context.Debts.Add(newDebt);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Invoice/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(long id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceProducts)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (invoice == null)
                return NotFound();

            // التحقق من الصلاحيات حسب نوع الفاتورة
            // Check permissions based on invoice type
            if (invoice.InvoiceType == "Sale")
            {
                if (!UserHasPermissionAsync("delete_invoice"))
                    return Forbid();
            }
            else if (invoice.InvoiceType == "Purchase")
            {
                if (!UserHasPermissionAsync("delete_purchase"))
                    return Forbid();
            }

            // إرجاع الكميات إلى المخزون قبل حذف الفاتورة (بوحدة أساسية باستخدام معامل التحويل)
            // Restore quantities to inventory before deleting invoice (in base unit using conversion factor)
            if (invoice.InvoiceProducts != null && invoice.InvoiceProducts.Any())
            {
                foreach (var ip in invoice.InvoiceProducts)
                {
                    // جلب وحدة المنتج لمعرفة معامل التحويل للوحدة الأساسية
                    // Get product unit to know conversion factor to base unit
                    // ملاحظة 18/12/2025:
                    // تصحيح المطابقة لاستخدام UnitId الفعلي من جدول الوحدات
                    var productUnit = await _context.ProductUnits
                        .FirstOrDefaultAsync(pu => pu.ProductId == ip.ProductId && pu.UnitId == ip.UnitId);

                    var conversionFactor = productUnit?.ConversionFactor ?? 1.0m;
                    var baseQuantity = ip.Quantity * conversionFactor;

                    var inventories = await _context.Inventories
                        .Where(inv => inv.ProductId == ip.ProductId)
                        .ToListAsync();
                    
                    // إرجاع الكمية بناءً على نوع الفاتورة (بوحدة أساسية)
                    // Restore quantity based on invoice type (in base unit)
                    if (invoice.InvoiceType == "Sale")
                    {
                        // فاتورة بيع: إرجاع الكمية إلى المخزون
                        // Sale invoice: Restore quantity to inventory
                        if (inventories.Any())
                        {
                            inventories[0].Quantity += baseQuantity;
                        }
                        else
                        {
                            var newInventory = new Inventory
                            {
                                ProductId = ip.ProductId,
                                Quantity = baseQuantity,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Inventories.Add(newInventory);
                        }
                    }
                    else if (invoice.InvoiceType == "Purchase")
                    {
                        // فاتورة شراء: خصم الكمية من المخزون
                        // Purchase invoice: Decrease quantity from inventory
                        if (inventories.Any())
                        {
                            var remainingToDeduct = baseQuantity;
                            
                            foreach (var inv in inventories.OrderBy(inv => inv.CreatedAt))
                            {
                                if (remainingToDeduct <= 0)
                                    break;
                                
                                if (inv.Quantity >= remainingToDeduct)
                                {
                                    inv.Quantity -= remainingToDeduct;
                                    remainingToDeduct = 0;
                                }
                                else
                                {
                                    remainingToDeduct -= inv.Quantity;
                                    inv.Quantity = 0;
                                }
                            }
                        }
                    }
                    else if (invoice.InvoiceType == "Return")
                    {
                        // فاتورة مرتجع: خصم الكمية من المخزون
                        // Return invoice: Decrease quantity from inventory
                        if (inventories.Any())
                        {
                            var remainingToDeduct = baseQuantity;
                            
                            foreach (var inv in inventories.OrderBy(inv => inv.CreatedAt))
                            {
                                if (remainingToDeduct <= 0)
                                    break;
                                
                                if (inv.Quantity >= remainingToDeduct)
                                {
                                    inv.Quantity -= remainingToDeduct;
                                    remainingToDeduct = 0;
                                }
                                else
                                {
                                    remainingToDeduct -= inv.Quantity;
                                    inv.Quantity = 0;
                                }
                            }
                        }
                    }
                }
            }

            // ============================================================
            // تحديث المديونيات عند حذف الفاتورة
            // Update debts when deleting invoice
            // ============================================================
            
            if (invoice.AmountRemaining > 0)
            {
                Debt? debtToUpdate = null;
                
                if (invoice.InvoiceType == "Sale" && invoice.ClientId.HasValue)
                {
                    debtToUpdate = await _context.Debts
                        .FirstOrDefaultAsync(d => d.ClientId == invoice.ClientId.Value);
                }
                else if (invoice.InvoiceType == "Purchase" && invoice.SupplierId.HasValue)
                {
                    debtToUpdate = await _context.Debts
                        .FirstOrDefaultAsync(d => d.SupplierId == invoice.SupplierId.Value);
                }

                if (debtToUpdate != null)
                {
                    // إرجاع القيم من Debt
                    // Reverse values from Debt
                    if (invoice.InvoiceType == "Sale")
                    {
                        debtToUpdate.Debit -= invoice.TotalAmount;
                        debtToUpdate.Paid -= invoice.AmountPaid;
                        debtToUpdate.Remaining -= invoice.AmountRemaining;
                    }
                    else if (invoice.InvoiceType == "Purchase")
                    {
                        debtToUpdate.Credit -= invoice.TotalAmount;
                        debtToUpdate.Paid -= invoice.AmountPaid;
                        debtToUpdate.Remaining -= invoice.AmountRemaining;
                    }

                    // حذف سجل Debt إذا أصبح المتبقي صفر أو أقل
                    // Delete debt record if remaining becomes zero or less
                    if (debtToUpdate.Remaining <= 0)
                    {
                        _context.Debts.Remove(debtToUpdate);
                    }
                }
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // POST: api/Invoice/{id}/Payment
        /// <summary>
        /// تسديد دفعة على فاتورة محددة
        /// Make a payment on a specific invoice
        /// </summary>
        [HttpPost("{id}/Payment")]
        public async Task<ActionResult<PaymentResponseDto>> MakePayment(long id, MakePaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // جلب الفاتورة
                var invoice = await _context.Invoices
                    .Include(i => i.Client)
                    .Include(i => i.Supplier)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                    return NotFound(new { message = "الفاتورة غير موجودة" });

                // التحقق من الصلاحيات حسب نوع الفاتورة
                if (invoice.InvoiceType == "Sale")
                {
                    if (!UserHasPermissionAsync("edit_invoice"))
                        return Forbid();
                }
                else if (invoice.InvoiceType == "Purchase")
                {
                    if (!UserHasPermissionAsync("edit_purchase"))
                        return Forbid();
                }

                // التحقق من أن المبلغ المدفوع لا يتجاوز المتبقي
                if (dto.Amount > invoice.AmountRemaining)
                {
                    return BadRequest(new
                    {
                        message = $"المبلغ المدفوع ({dto.Amount}) أكبر من المبلغ المتبقي ({invoice.AmountRemaining})"
                    });
                }

                // حفظ القيم القديمة للاستجابة
                var previousAmountPaid = invoice.AmountPaid;

                // تحديث الفاتورة
                invoice.AmountPaid += dto.Amount;
                invoice.AmountRemaining -= dto.Amount;

                // تحديث طريقة الدفع إذا تم توفيرها
                if (!string.IsNullOrEmpty(dto.PaymentMethod))
                {
                    invoice.PaymentMethod = dto.PaymentMethod;
                }

                // تحديث المرجع إذا تم توفيره
                if (!string.IsNullOrEmpty(dto.PaymentReference))
                {
                    invoice.PaymentReference = dto.PaymentReference;
                }

                invoice.UpdatedAt = DateTime.UtcNow;

                // تحديث جدول الديون
                long? personId = invoice.InvoiceType == "Sale" ? invoice.ClientId : invoice.SupplierId;

                if (personId.HasValue)
                {
                    var debt = await _context.Debts
                        .FirstOrDefaultAsync(d =>
                            (invoice.InvoiceType == "Sale" && d.ClientId == personId) ||
                            (invoice.InvoiceType == "Purchase" && d.SupplierId == personId));

                    if (debt != null)
                    {
                        // تحديث المدفوع والمتبقي في جدول الديون
                        debt.Paid += dto.Amount;
                        debt.Remaining -= dto.Amount;
                        debt.LastPaymentDate = dto.PaymentDate ?? DateTime.UtcNow;

                        // حذف سجل الدين إذا أصبح المتبقي صفر أو أقل
                        if (debt.Remaining <= 0)
                        {
                            _context.Debts.Remove(debt);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // إعداد الاستجابة
                var response = new PaymentResponseDto
                {
                    Success = true,
                    Message = invoice.AmountRemaining == 0
                        ? "تم تسديد الفاتورة بالكامل بنجاح"
                        : $"تم تسديد دفعة بنجاح. المتبقي: {invoice.AmountRemaining:F2}",
                    PreviousAmountPaid = previousAmountPaid,
                    PaymentAmount = dto.Amount,
                    NewAmountPaid = invoice.AmountPaid,
                    NewAmountRemaining = invoice.AmountRemaining,
                    IsFullyPaid = invoice.AmountRemaining == 0
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    message = "حدث خطأ أثناء تسديد الدفعة",
                    error = ex.Message
                });
            }
        }

        private bool InvoiceExists(long id) => _context.Invoices.Any(e => e.Id == id);
    }
}
