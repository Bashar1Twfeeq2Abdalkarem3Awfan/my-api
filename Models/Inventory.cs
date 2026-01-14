using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("inventory")]
    public class Inventory
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("purchase_invoice_number")]
        [MaxLength(100)]
        public string? PurchaseInvoiceNumber { get; set; }

        [Column("production_date")]
        public DateTime? ProductionDate { get; set; }

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [Column("quantity")]
        [Precision(14, 2)]
        public decimal Quantity { get; set; } = 0m;

        [Required]
        [Column("product_id")]
        public long ProductId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [ForeignKey(nameof(ProductId))]
        [InverseProperty(nameof(Product.Inventories))]
        public Product? Product { get; set; }
    }
}