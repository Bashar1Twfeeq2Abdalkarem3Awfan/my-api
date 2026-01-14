using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("product")]
    public class Product
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("product_name")]
        [MaxLength(250)]
        public string ProductName { get; set; } = null!;

        [Column("qr_code")]
        [MaxLength(255)]
        public string? QrCode { get; set; }

        [Column("selling_price")]
        public decimal SellingPrice { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("category_id")]
        public long? CategoryId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(CategoryId))]
        [InverseProperty(nameof(Category.Products))]
        public Category? Category { get; set; }

        [InverseProperty(nameof(ProductUnit.Product))]
        public ICollection<ProductUnit>? ProductUnits { get; set; }

        [InverseProperty(nameof(Inventory.Product))]
        public ICollection<Inventory>? Inventories { get; set; }

        [InverseProperty(nameof(InvoiceProduct.Product))]
        public ICollection<InvoiceProduct>? InvoiceProducts { get; set; }

        [InverseProperty(nameof(ReturnProduct.Product))]
        public ICollection<ReturnProduct>? ReturnProducts { get; set; }
    }
}