using System;

namespace MyAPIv3.DTOs
{
    public class InvoiceProductDto
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public long ProductId { get; set; }
        public long UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime? CreatedAt { get; set; }
        // Navigation
        public string? ProductName { get; set; }
        public string? UnitName { get; set; }
    }

    public class CreateInvoiceProductDto
    {
        public long InvoiceId { get; set; }
        public long ProductId { get; set; }
        public long UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Subtotal { get; set; }
        // حقول المخزون (للفواتير الشراء فقط)
        // Inventory fields (for purchase invoices only)
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? PurchaseInvoiceNumber { get; set; }
    }

    public class UpdateInvoiceProductDto
    {
        public long ProductId { get; set; }
        public long UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}

