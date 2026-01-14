using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("invoice")]
    public class Invoice
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("invoice_type")]
        public string InvoiceType { get; set; }

        [Column("payment_reference")]
        [MaxLength(200)]
        public string? PaymentReference { get; set; }

        [Column("payment_method")]
        [MaxLength(100)]
        public string? PaymentMethod { get; set; }

        [Column("total_amount")]
        [Precision(18, 2)]
        public decimal TotalAmount { get; set; } = 0m;

        [Column("amount_paid")]
        [Precision(18, 2)]
        public decimal AmountPaid { get; set; } = 0m;

        [Column("amount_remaining")]
        [Precision(18, 2)]
        public decimal AmountRemaining { get; set; } = 0m;

        [Column("invoice_date")]
        public DateTime? InvoiceDate { get; set; }

        [Column("client_id")]
        public long? ClientId { get; set; }

        [Column("supplier_id")]
        public long? SupplierId { get; set; }

        [Column("created_by")]
        public long? CreatedBy { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(ClientId))]
        [InverseProperty(nameof(Person.ClientInvoices))]
        public Person? Client { get; set; }

        [ForeignKey(nameof(SupplierId))]
        [InverseProperty(nameof(Person.SupplierInvoices))]
        public Person? Supplier { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        [InverseProperty(nameof(Person.CreatedInvoices))]
        public Person? CreatedByPerson { get; set; }

        [InverseProperty(nameof(InvoiceProduct.Invoice))]
        public ICollection<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();
    }
}