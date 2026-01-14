using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("product_unit")]
    public class ProductUnit
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("product_id")]
        public long ProductId { get; set; }

        [Required]
        [Column("unit_id")]
        public long UnitId { get; set; }

        [Required]
        [Column("sale_price")]
        [Precision(14, 2)]
        public decimal SalePrice { get; set; } = 0m;

        [Column("conversion_factor")]
        [Precision(14, 4)]
        public decimal? ConversionFactor { get; set; } = 1.0m;

        [Column("is_default")]
        public bool IsDefault { get; set; } = false;

        [ForeignKey(nameof(ProductId))]
        [InverseProperty(nameof(Product.ProductUnits))]
        public Product? Product { get; set; }

        [ForeignKey(nameof(UnitId))]
        [InverseProperty(nameof(Unit.ProductUnits))]
        public Unit? Unit { get; set; }
    }
}