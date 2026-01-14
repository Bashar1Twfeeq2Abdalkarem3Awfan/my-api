using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("expense")]
    public class Expense
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; } = null!;

        [Required]
        [Column("amount")]
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        [Column("currency")]
        [MaxLength(3)]
        public string? Currency { get; set; } = "YER";

        [Column("expense_date")]
        public DateTime? ExpenseDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_by")]
        public long? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        [InverseProperty(nameof(Person.ExpensesCreated))]
        public Person? CreatedByPerson { get; set; }
    }
}