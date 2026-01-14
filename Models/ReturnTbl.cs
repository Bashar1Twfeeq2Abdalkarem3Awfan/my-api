using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("return_tbl")]
    public class ReturnTbl
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("cashier_number")]
        [MaxLength(100)]
        public string? CashierNumber { get; set; }

        [Column("original_invoice_id")]
        public long? OriginalInvoiceId { get; set; }

        [Column("return_type")]
        public string ReturnType { get; set; }
        //public ReturnTypeEnum ReturnType { get; set; } = ReturnTypeEnum.ReturnFromCustomer;

        [Column("invoice_status")]
        [MaxLength(100)]
        public string? InvoiceStatus { get; set; }

        [Column("return_date")]
        public DateTime? ReturnDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("client_id")]
        public long? ClientId { get; set; }

        [Column("supplier_id")]
        public long? SupplierId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(OriginalInvoiceId))]
        public Invoice? OriginalInvoice { get; set; }

        [ForeignKey(nameof(ClientId))]
        [InverseProperty(nameof(Person.ReturnsAsClient))]
        public Person? Client { get; set; }

        [ForeignKey(nameof(SupplierId))]
        [InverseProperty(nameof(Person.ReturnsAsSupplier))]
        public Person? Supplier { get; set; }

        [InverseProperty(nameof(ReturnProduct.Return))]
        public ICollection<ReturnProduct>? ReturnProducts { get; set; }
    }
}