using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("unit")]
    public class Unit
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("unit_name")]
        [MaxLength(100)]
        public string UnitName { get; set; } = null!;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [InverseProperty(nameof(ProductUnit.Unit))]
        public ICollection<ProductUnit>? ProductUnits { get; set; }

        [InverseProperty(nameof(InvoiceProduct.Unit))]
        public ICollection<InvoiceProduct>? InvoiceProducts { get; set; }
    }
}