using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("return_product")]
    public class ReturnProduct
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("return_id")]
        public long ReturnId { get; set; }

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

        [Column("notes")]
        public string? Notes { get; set; }

        [ForeignKey(nameof(ReturnId))]
        [InverseProperty(nameof(ReturnTbl.ReturnProducts))]
        public ReturnTbl? Return { get; set; }

        [ForeignKey(nameof(ProductId))][InverseProperty(nameof(Product.ReturnProducts))]
        public Product? Product { get; set; }

        [ForeignKey(nameof(UnitId))]
        public Unit? Unit { get; set; }
    }
}