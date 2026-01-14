using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("invoice_product")]
    public class InvoiceProduct
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("invoice_id")]
        public long InvoiceId { get; set; }

        [Required]
        [Column("product_id")]
        public long ProductId { get; set; }

        [Required]
        [Column("unit_id")]
        public long UnitId { get; set; }

        [Required]
        [Column("quantity")]
        [Precision(14, 2)]
        public decimal Quantity { get; set; }

        [Required]
        [Column("subtotal")]
        [Precision(18, 2)]
        public decimal Subtotal { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        [InverseProperty(nameof(Invoice.InvoiceProducts))]
        public Invoice? Invoice { get; set; }

        [ForeignKey(nameof(ProductId))]
        [InverseProperty(nameof(Product.InvoiceProducts))]
        public Product? Product { get; set; }

        [ForeignKey(nameof(UnitId))]
        [InverseProperty(nameof(Unit.InvoiceProducts))]
        public Unit? Unit { get; set; }
    }
}