using System;

namespace MyAPIv3.DTOs
{
    public class InventoryDto
    {
        public long Id { get; set; }
        public string? PurchaseInvoiceNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public long ProductId { get; set; }
        public DateTime? CreatedAt { get; set; }
        // Navigation
        public string? ProductName { get; set; }
    }

    public class CreateInventoryDto
    {
        public string? PurchaseInvoiceNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public long ProductId { get; set; }
    }

    public class UpdateInventoryDto
    {
        public string? PurchaseInvoiceNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public long ProductId { get; set; }
    }
}

