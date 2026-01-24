using System;
using System.Collections.Generic;

namespace MyAPIv3.DTOs
{
    // =====================================
    // Enums & Filters
    // =====================================
    public enum ReportPeriod
    {
        Today,      // اليوم
        Month,      // الشهر الحالي
        Year,       // السنة الحالية
        Custom      // مخصص
    }

    public class ReportFilterDto
    {
        public ReportPeriod Period { get; set; } = ReportPeriod.Today;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // =====================================
    // Sales Report DTOs
    // =====================================
    public class SalesReportDto
    {
        public decimal TotalSales { get; set; }
        public decimal TotalReturns { get; set; }        // إجمالي المرتجعات
        public decimal NetSales { get; set; }            // صافي المبيعات (المبيعات - المرتجعات)
        public decimal CashSales { get; set; }
        public decimal CreditSales { get; set; }
        public int InvoicesCount { get; set; }
        public int ReturnsCount { get; set; }            // عدد عمليات الإرجاع
        public decimal AverageInvoiceValue { get; set; }
        public List<TopProductDto> TopProducts { get; set; } = new();
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = "";
        public decimal QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    // =====================================
    // Purchases Report DTOs
    // =====================================
    public class PurchasesReportDto
    {
        public decimal TotalPurchases { get; set; }
        public int InvoicesCount { get; set; }
        public decimal AverageInvoiceValue { get; set; }
        public List<TopSupplierDto> TopSuppliers { get; set; } = new();
    }

    public class TopSupplierDto
    {
        public string SupplierName { get; set; } = "";
        public decimal TotalPurchased { get; set; }
        public int InvoicesCount { get; set; }
    }

    // =====================================
    // Financial Report DTOs
    // =====================================
    public class FinancialReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }        // COGS
        public decimal GrossProfit { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }     // نسبة الربح %
    }

    // =====================================
    // Debts Report DTOs
    // =====================================
    public class DebtsReportDto
    {
        public decimal TotalCustomerDebts { get; set; }
        public int CustomersWithDebtCount { get; set; }
        public decimal TotalSupplierDebts { get; set; }
        public int SuppliersWithDebtCount { get; set; }
        public List<TopDebtorDto> TopDebtors { get; set; } = new();
    }

    public class TopDebtorDto
    {
        public string Name { get; set; } = "";
        public decimal DebtAmount { get; set; }
        public string Type { get; set; } = ""; // "Customer" or "Supplier"
    }

    // =====================================
    // Inventory Report DTOs
    // =====================================
    public class InventoryReportDto
    {
        public decimal TotalInventoryValue { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public List<LowStockDto> LowStockProducts { get; set; } = new();
        public List<ExpiringProductDto> ExpiringProducts { get; set; } = new();
        public List<InventoryItemDto> AllProducts { get; set; } = new();
    }

    public class InventoryItemDto
    {
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal Price { get; set; } // قد نحتاج لعرض السعر أيضاً
    }

    public class LowStockDto
    {
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal MinimumStock { get; set; } = 10;
    }

    public class ExpiringProductDto
    {
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    // =====================================
    // Comprehensive Report DTO
    // =====================================
    public class ComprehensiveReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PeriodName { get; set; } = "";
        public SalesReportDto Sales { get; set; } = new();
        public PurchasesReportDto Purchases { get; set; } = new();
        public FinancialReportDto Financial { get; set; } = new();
        public DebtsReportDto Debts { get; set; } = new();
        public InventoryReportDto Inventory { get; set; } = new();
    }
}
