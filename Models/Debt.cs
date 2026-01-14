using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("debt")]
    public class Debt
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("account_name")]
        [MaxLength(200)]
        public string? AccountName { get; set; }

        [Column("account_type")]
        [MaxLength(100)]
        public string? AccountType { get; set; }

        [Column("debit")]
        [Precision(18, 2)]
        public decimal Debit { get; set; }

        [Column("credit")]
        [Precision(18, 2)]
        public decimal Credit { get; set; }

        [Column("paid")]
        [Precision(18, 2)]
        public decimal Paid { get; set; }

        [Column("remaining")]
        [Precision(18, 2)]
        public decimal Remaining { get; set; }

        [Column("last_payment_date")]
        public DateTime? LastPaymentDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("client_id")]
        public long? ClientId { get; set; }

        [Column("supplier_id")]
        public long? SupplierId { get; set; }

        [Column("created_by")]
        public long? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [ForeignKey(nameof(ClientId))]
        [InverseProperty(nameof(Person.DebtsAsClient))]
        public Person? ClientPerson { get; set; }

        [ForeignKey(nameof(SupplierId))]
        [InverseProperty(nameof(Person.DebtsAsSupplier))]
        public Person? SupplierPerson { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        [InverseProperty(nameof(Person.DebtsCreated))]
        public Person? CreatedByPerson { get; set; }
    }
}