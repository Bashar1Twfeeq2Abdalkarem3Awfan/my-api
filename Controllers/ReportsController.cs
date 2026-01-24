using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.DTOs;
using MyAPIv3.Models;
using MyAPIv3.Attributes;

namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================
        // Helper: Get Date Range
        // =====================================
        private (DateTime StartDate, DateTime EndDate, string PeriodName) GetDateRange(ReportFilterDto filter)
        {
            var now = DateTime.UtcNow;
            DateTime start, end;
            string periodName;

            switch (filter.Period)
            {
                case ReportPeriod.Today:
                    start = now.Date;
                    end = now.Date.AddDays(1).AddSeconds(-1);
                    periodName = "اليوم";
                    break;

                case ReportPeriod.Month:
                    start = new DateTime(now.Year, now.Month, 1);
                    end = start.AddMonths(1).AddSeconds(-1);
                    periodName = $"{now:MMMM yyyy}";
                    break;

                case ReportPeriod.Year:
                    start = new DateTime(now.Year, 1, 1);
                    end = start.AddYears(1).AddSeconds(-1);
                    periodName = $"سنة {now.Year}";
                    break;

                case ReportPeriod.Custom:
                    if (filter.StartDate == null || filter.EndDate == null)
                    {
                        throw new ArgumentException("يجب تحديد تاريخ البداية والنهاية للفترة المخصصة");
                    }
                    start = filter.StartDate.Value.Date;
                    end = filter.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                    periodName = $"من {start:yyyy-MM-dd} إلى {filter.EndDate.Value:yyyy-MM-dd}";
                    break;

                default:
                    start = now.Date;
                    end = now.Date.AddDays(1).AddSeconds(-1);
                    periodName = "اليوم";
                    break;
            }

            return (start, end, periodName);
        }

        // =====================================
        // Helper: Calculate COGS   لجل يتعامل مع جميع وحدات المنتج عند حساب صافي الربح / وإصلاح فلترة المصروفات
        // =====================================
        // =====================================
        // Helper: Calculate COGS
        // =====================================
        private async Task<decimal> CalculateCOGS(List<Invoice> salesInvoices)
        {
            decimal totalCost = 0;

            foreach (var sale in salesInvoices)
            {
                foreach (var item in sale.InvoiceProducts)
                {
                    // Calculate sale quantity in base units
                    var saleUnitFactor = item.Product?.ProductUnits?
                        .FirstOrDefault(pu => pu.UnitId == item.UnitId)?.ConversionFactor ?? 1;
                    
                    var saleBaseQty = item.Quantity * saleUnitFactor;

                    // Get all purchase invoice products for this product
                    var purchaseItems = await _context.InvoiceProducts
                        .Where(p => p.Invoice!.InvoiceType == "Purchase" &&
                                    p.ProductId == item.ProductId &&
                                    p.Quantity > 0)
                        .ToListAsync();

                    if (!purchaseItems.Any())
                        continue;

                    // Calculate average cost per base unit from purchases
                    decimal totalPurchBaseQty = 0;
                    decimal totalPurchValue = 0;

                    foreach (var pItem in purchaseItems)
                    {
                        // We need the conversion factor for the purchase unit.
                        // Since we loaded ProductUnits in the sale item (same product), we can try to use it.
                        // However, purchaseItems are fetched separately and might not have Product loaded.
                        // But since it's the SAME productId, we can look up in item.Product.ProductUnits
                        
                        var purchUnitFactor = item.Product?.ProductUnits?
                            .FirstOrDefault(pu => pu.UnitId == pItem.UnitId)?.ConversionFactor ?? 1;
                            
                        totalPurchBaseQty += pItem.Quantity * purchUnitFactor;
                        totalPurchValue += pItem.Subtotal;
                    }

                    if (totalPurchBaseQty == 0) continue;

                    var avgCostPerBaseUnit = totalPurchValue / totalPurchBaseQty;
                    totalCost += saleBaseQty * avgCostPerBaseUnit;
                }
            }

            return totalCost;
        }

        // =====================================
        // 1. Comprehensive Report
        // =====================================
        [HttpPost("comprehensive")]
        [RequirePermission("view_reports")]
        public async Task<ActionResult<ComprehensiveReportDto>> GetComprehensiveReport(
            [FromBody] ReportFilterDto filter)
        {
            try
            {
                var (startDate, endDate, periodName) = GetDateRange(filter);

                // Load all data for the period
                var invoices = await _context.Invoices
                    .Where(i => i.InvoiceDate.HasValue &&
                                i.InvoiceDate.Value >= startDate &&
                                i.InvoiceDate.Value <= endDate)
                    .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                    .ThenInclude(p => p!.ProductUnits)
                    .Include(i => i.Client)
                    .Include(i => i.Supplier)
                    .ToListAsync();

                // Load returns for the period
                var returns = await _context.Returns
                    .Where(r => r.ReturnDate >= startDate && r.ReturnDate <= endDate)
                    .Include(r => r.ReturnProducts)
                    .ThenInclude(rp => rp.Product)
                    .ThenInclude(p => p!.ProductUnits)
                    .ToListAsync();


                var salesReport = await GetSalesReportInternal(invoices, startDate, endDate);
                var purchasesReport = await GetPurchasesReportInternal(invoices);
                var financialReport = await GetFinancialReportInternal(invoices, returns, startDate, endDate);
                var debtsReport = await GetDebtsReportInternal();
                var inventoryReport = await GetInventoryReportInternal();

                return Ok(new ComprehensiveReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    PeriodName = periodName,
                    Sales = salesReport,
                    Purchases = purchasesReport,
                    Financial = financialReport,
                    Debts = debtsReport,
                    Inventory = inventoryReport
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"خطأ في إنشاء التقرير: {ex.Message}" });
            }
        }

        // =====================================
        // 2. Sales Report
        // =====================================
        [HttpPost("sales")]
        [RequirePermission("view_sales_reports")]
        public async Task<ActionResult<SalesReportDto>> GetSalesReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                var (startDate, endDate, _) = GetDateRange(filter);

                var invoices = await _context.Invoices
                    .Where(i => i.InvoiceType == "Sale" &&
                                i.InvoiceDate.HasValue &&
                                i.InvoiceDate.Value >= startDate &&
                                i.InvoiceDate.Value <= endDate)
                    .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                    .ToListAsync();

                var report = await GetSalesReportInternal(invoices, startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"خطأ: {ex.Message}" });
            }
        }

        private async Task<SalesReportDto> GetSalesReportInternal(
    List<Invoice> allInvoices, DateTime startDate, DateTime endDate)
{
    var salesInvoices = allInvoices
        .Where(i => i.InvoiceType == "Sale")
        .ToList();

    var totalSales = salesInvoices.Sum(i => i.TotalAmount);
    var cashSales = salesInvoices
        .Where(i => i.PaymentMethod?.Contains("نقدي") == true ||
                    i.PaymentMethod?.ToLower().Contains("cash") == true)
        .Sum(i => i.TotalAmount);
    var creditSales = totalSales - cashSales;

    // =====================================
    // حساب المرتجعات من العملاء
    // =====================================
    var customerReturns = await _context.Returns
        .Where(r => r.ReturnType == "ReturnFromCustomer" &&
                    r.ReturnDate.HasValue &&
                    r.ReturnDate.Value >= startDate &&
                    r.ReturnDate.Value <= endDate)
        .Include(r => r.ReturnProducts)
        .ToListAsync();

    // حساب إجمالي قيمة المرتجعات
    decimal totalReturns = 0;
    foreach (var returnItem in customerReturns)
    {
        if (returnItem.ReturnProducts != null)
        {
            foreach (var returnProduct in returnItem.ReturnProducts)
            {
                // استخدام UnitPrice إذا كان متوفراً، وإلا نحاول الحصول عليه من الفاتورة الأصلية
                decimal unitPrice = returnProduct.UnitPrice ?? 0;
                totalReturns += returnProduct.Quantity * unitPrice;
            }
        }
    }

    // حساب صافي المبيعات
    var netSales = totalSales - totalReturns;

    // =====================================
    // حساب أفضل المنتجات مبيعاً (مع خصم المرتجعات)
    // =====================================
    
    // جمع الكميات المباعة
    var productSales = salesInvoices
        .SelectMany(i => i.InvoiceProducts)
        .GroupBy(ip => new { ip.ProductId, ip.Product!.ProductName })
        .Select(g => new
        {
            ProductId = g.Key.ProductId,
            ProductName = g.Key.ProductName,
            QuantitySold = g.Sum(x => x.Quantity),
            Revenue = g.Sum(x => x.Subtotal)
        })
        .ToDictionary(x => x.ProductId);

    // خصم الكميات المرتجعة
    var returnedQuantities = customerReturns
        .SelectMany(r => r.ReturnProducts ?? new List<ReturnProduct>())
        .GroupBy(rp => rp.ProductId)
        .Select(g => new
        {
            ProductId = g.Key,
            ReturnedQuantity = g.Sum(x => x.Quantity),
            ReturnedValue = g.Sum(x => x.Quantity * (x.UnitPrice ?? 0))
        })
        .ToDictionary(x => x.ProductId);

    // دمج البيانات وحساب صافي المبيعات لكل منتج
    var topProducts = productSales.Values
        .Select(ps => new TopProductDto
        {
            ProductName = ps.ProductName,
            QuantitySold = ps.QuantitySold - (returnedQuantities.ContainsKey(ps.ProductId) 
                ? returnedQuantities[ps.ProductId].ReturnedQuantity 
                : 0),
            Revenue = ps.Revenue - (returnedQuantities.ContainsKey(ps.ProductId) 
                ? returnedQuantities[ps.ProductId].ReturnedValue 
                : 0)
        })
        .Where(p => p.QuantitySold > 0) // استبعاد المنتجات التي تم إرجاعها بالكامل
        .OrderByDescending(x => x.Revenue)
        .Take(10)
        .ToList();

    var avgInvoiceValue = salesInvoices.Count > 0
        ? totalSales / salesInvoices.Count
        : 0;

    return new SalesReportDto
    {
        TotalSales = totalSales,
        TotalReturns = totalReturns,
        NetSales = netSales,
        CashSales = cashSales,
        CreditSales = creditSales,
        InvoicesCount = salesInvoices.Count,
        ReturnsCount = customerReturns.Count,
        AverageInvoiceValue = avgInvoiceValue,
        TopProducts = topProducts
    };
}
        // =====================================
        // 3. Purchases Report
        // =====================================
        [HttpPost("purchases")]
        [RequirePermission("view_purchases")]
        public async Task<ActionResult<PurchasesReportDto>> GetPurchasesReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                var (startDate, endDate, _) = GetDateRange(filter);

                var invoices = await _context.Invoices
                    .Where(i => i.InvoiceType == "Purchase" &&
                                i.InvoiceDate.HasValue &&
                                i.InvoiceDate.Value >= startDate &&
                                i.InvoiceDate.Value <= endDate)
                    .Include(i => i.Supplier)
                    .ToListAsync();

                var report = await GetPurchasesReportInternal(invoices);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"خطأ: {ex.Message}" });
            }
        }

        private Task<PurchasesReportDto> GetPurchasesReportInternal(List<Invoice> allInvoices)
        {
            var purchaseInvoices = allInvoices
                .Where(i => i.InvoiceType == "Purchase")
                .ToList();

            var totalPurchases = purchaseInvoices.Sum(i => i.TotalAmount);
            
            // Top suppliers
            var topSuppliers = purchaseInvoices
                .Where(i => i.Supplier != null)
                .GroupBy(i => new { i.SupplierId, i.Supplier!.FirstName })
                .Select(g => new TopSupplierDto
                {
                    SupplierName = g.Key.FirstName,
                    TotalPurchased = g.Sum(x => x.TotalAmount),
                    InvoicesCount = g.Count()
                })
                .OrderByDescending(x => x.TotalPurchased)
                .Take(10)
                .ToList();

            var avgInvoiceValue = purchaseInvoices.Count > 0
                ? totalPurchases / purchaseInvoices.Count
                : 0;

            return Task.FromResult(new PurchasesReportDto
            {
                TotalPurchases = totalPurchases,
                InvoicesCount = purchaseInvoices.Count,
                AverageInvoiceValue = avgInvoiceValue,
                TopSuppliers = topSuppliers
            });
        }

        // =====================================
        // 4. Financial Report
        // =====================================
        [HttpPost("financial")]
        [RequirePermission("view_financial_reports")]
        public async Task<ActionResult<FinancialReportDto>> GetFinancialReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                var (startDate, endDate, _) = GetDateRange(filter);

                var invoices = await _context.Invoices
                    .Where(i => i.InvoiceDate.HasValue &&
                                i.InvoiceDate.Value >= startDate &&
                                i.InvoiceDate.Value <= endDate)
                    .Include(i => i.InvoiceProducts)
                    .ThenInclude(ip => ip.Product)
                    .ThenInclude(p => p!.ProductUnits)
                    .ToListAsync();

                var returns = await _context.Returns
                    .Where(r => r.ReturnDate >= startDate && r.ReturnDate <= endDate)
                    .Include(r => r.ReturnProducts)
                    .ThenInclude(rp => rp.Product)
                    .ThenInclude(p => p!.ProductUnits)
                    .ToListAsync();

                var report = await GetFinancialReportInternal(invoices, returns, startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"خطأ: {ex.Message}" });
            }
        }

        private async Task<FinancialReportDto> GetFinancialReportInternal(List<Invoice> allInvoices, List<ReturnTbl> allReturns, DateTime startDate, DateTime endDate)
        {
            // 1. Calculate Gross Sales
            var salesInvoices = allInvoices
                .Where(i => i.InvoiceType == "Sale")
                .ToList();

            var grossRevenue = salesInvoices.Sum(i => i.TotalAmount);
            var grossCost = await CalculateCOGS(salesInvoices);

            // 2. Calculate Returns (Sales Returns - ReturnFromCustomer)
            var customerReturns = allReturns
                .Where(r => r.ReturnType == "ReturnFromCustomer")
                .ToList();

            decimal returnsValue = 0;
            decimal returnsCost = 0;

            foreach (var ret in customerReturns)
            {
                if (ret.ReturnProducts == null) continue;

                foreach (var item in ret.ReturnProducts)
                {
                    // Calculate return value (Revenue deduction)
                    // Assuming price at return time or original selling price. 
                    // Since we don't have original price easily linked here without extra complex logic,
                    // we'll use the Product's current SellingPrice as a proxy or if we stored it.
                    // However, Returns don't store "Price", they store Quantity.
                    // So we must fetch Product Selling Price.
                    var productPrice = item.Product?.SellingPrice ?? 0;
                    returnsValue += item.Quantity * productPrice;

                    // Calculate return Cost (COGS deduction / Reversal)
                    // We need the same logic as CalculateCOGS: finding Average Cost per Base Unit
                    var unitFactor = item.Product?.ProductUnits?
                        .FirstOrDefault(pu => pu.UnitId == item.UnitId)?.ConversionFactor ?? 1;
                    
                    var returnBaseQty = item.Quantity * unitFactor;

                    // Calculate Average Cost for this product (re-using logic or we could extract it)
                    // Ideally we should cache this, but for now we re-calculate
                    var purchaseItems = await _context.InvoiceProducts
                        .Where(p => p.Invoice!.InvoiceType == "Purchase" &&
                                    p.ProductId == item.ProductId &&
                                    p.Quantity > 0)
                        .ToListAsync(); // This is expensive inside loop, but needed for accuracy

                    if (purchaseItems.Any())
                    {
                        decimal totalPurchBaseQty = 0;
                        decimal totalPurchValue = 0;
                        foreach (var pItem in purchaseItems)
                        {
                            var purchUnitFactor = item.Product?.ProductUnits?
                                .FirstOrDefault(pu => pu.UnitId == pItem.UnitId)?.ConversionFactor ?? 1;
                            totalPurchBaseQty += pItem.Quantity * purchUnitFactor;
                            totalPurchValue += pItem.Subtotal;
                        }
                        if (totalPurchBaseQty > 0)
                        {
                            var avgCost = totalPurchValue / totalPurchBaseQty;
                            returnsCost += returnBaseQty * avgCost;
                        }
                    }
                }
            }

            // 3. Net Calculation
            var netRevenue = grossRevenue - returnsValue;
            var netCost = grossCost - returnsCost;
            var grossProfit = netRevenue - netCost;

            // Get expenses for the same period
            var expenses = await _context.Expenses
                .Where(e => e.ExpenseDate.HasValue && 
                            e.ExpenseDate.Value >= startDate && 
                            e.ExpenseDate.Value <= endDate)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;
            
            var netProfit = grossProfit - expenses;
            var profitMargin = netRevenue > 0 ? (netProfit / netRevenue) * 100 : 0;

            return new FinancialReportDto
            {
                TotalRevenue = netRevenue, // Now Net Revenue
                TotalCost = netCost,       // Now Net Cost
                GrossProfit = grossProfit,
                TotalExpenses = expenses,
                NetProfit = netProfit,
                ProfitMargin = profitMargin
            };
        }

        // =====================================
        // 5. Debts Report
        // =====================================
        [HttpPost("debts")]
        [RequirePermission("view_debts")]
        public async Task<ActionResult<DebtsReportDto>> GetDebtsReport()
        {
            try
            {
                var report = await GetDebtsReportInternal();
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"خطأ: {ex.Message}" });
            }
        }

        private async Task<DebtsReportDto> GetDebtsReportInternal()
        {
            var debts = await _context.Debts
                .Include(d => d.ClientPerson)
                .Include(d => d.SupplierPerson)
                .Where(d => d.Remaining > 0)
                .ToListAsync();

            var customerDebts = debts.Where(d => d.ClientId != null).ToList();
            var supplierDebts = debts.Where(d => d.SupplierId != null).ToList();

            var totalCustomerDebts = customerDebts.Sum(d => d.Remaining);
            var totalSupplierDebts = supplierDebts.Sum(d => d.Remaining);

            // Top debtors
            var topDebtors = new List<TopDebtorDto>();

            topDebtors.AddRange(customerDebts
                .GroupBy(d => new { d.ClientId, d.ClientPerson!.FirstName })
                .Select(g => new TopDebtorDto
                {
                    Name = g.Key.FirstName,
                    DebtAmount = g.Sum(x => x.Remaining),
                    Type = "Customer"
                })
                .OrderByDescending(x => x.DebtAmount)
                .Take(5));

            topDebtors.AddRange(supplierDebts
                .GroupBy(d => new { d.SupplierId, d.SupplierPerson!.FirstName })
                .Select(g => new TopDebtorDto
                {
                    Name = g.Key.FirstName,
                    DebtAmount = g.Sum(x => x.Remaining),
                    Type = "Supplier"
                })
                .OrderByDescending(x => x.DebtAmount)
                .Take(5));

            return new DebtsReportDto
            {
                TotalCustomerDebts = totalCustomerDebts,
                CustomersWithDebtCount = customerDebts
                    .Select(d => d.ClientId)
                    .Distinct()
                    .Count(),
                TotalSupplierDebts = totalSupplierDebts,
                SuppliersWithDebtCount = supplierDebts
                    .Select(d => d.SupplierId)
                    .Distinct()
                    .Count(),
                TopDebtors = topDebtors.OrderByDescending(x => x.DebtAmount).Take(10).ToList()
            };
        }

        // =====================================
        // 6. Inventory Report
        // =====================================
        [HttpPost("inventory")]
        [RequirePermission("view_inventory_reports")]
        public async Task<ActionResult<InventoryReportDto>> GetInventoryReport()
        {
            try
            {
                var report = await GetInventoryReportInternal();
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"خطأ: {ex.Message}" });
            }
        }

        private async Task<InventoryReportDto> GetInventoryReportInternal()
        {
            var inventories = await _context.Inventories
                .Include(i => i.Product)
                .ToListAsync();

            var products = await _context.Products.ToListAsync();

            // Calculate total inventory value
            var inventoryValue = inventories
                .GroupBy(i => i.ProductId)
                .Sum(g =>
                {
                    var product = products.FirstOrDefault(p => p.Id == g.Key);
                    if (product == null) return 0;
                    var totalQty = g.Sum(x => x.Quantity);
                    return totalQty * product.SellingPrice;
                });

            // Low stock products
            var lowStockProducts = inventories
                .GroupBy(i => new { i.ProductId, i.Product!.ProductName })
                .Select(g => new LowStockDto
                {
                    ProductName = g.Key.ProductName,
                    Quantity = g.Sum(x => x.Quantity),
                    MinimumStock = 10
                })
                .Where(x => x.Quantity < x.MinimumStock)
                .OrderBy(x => x.Quantity)
                .ToList();

            // Expiring soon products
            var today = DateTime.UtcNow.Date;
            var expiringProducts = inventories
                .Where(i => i.ExpiryDate.HasValue)
                .Select(i => new ExpiringProductDto
                {
                    ProductName = i.Product!.ProductName,
                    Quantity = i.Quantity,
                    ExpiryDate = i.ExpiryDate,
                    DaysRemaining = (i.ExpiryDate!.Value.Date - today).Days
                })
                .Where(x => x.DaysRemaining >= 0 && x.DaysRemaining <= 30)
                .OrderBy(x => x.DaysRemaining)
                .ToList();

            return new InventoryReportDto
            {
                TotalInventoryValue = inventoryValue,
                TotalProducts = products.Count,
                LowStockCount = lowStockProducts.Count,
                ExpiringSoonCount = expiringProducts.Count,
                LowStockProducts = lowStockProducts,
                ExpiringProducts = expiringProducts
            };
        }
    }
}
