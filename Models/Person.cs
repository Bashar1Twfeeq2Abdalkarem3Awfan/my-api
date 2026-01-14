using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("person")]
    public class Person
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("second_name")]
        public string? SecondName { get; set; }

        [MaxLength(200)]
        [Column("third_with_lastname")]
        public string? ThirdWithLastname { get; set; }

        [MaxLength(200)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(15)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("person_type_enum")]
        public string PersonType { get; set; } 

        // Navigation Properties
        [InverseProperty(nameof(User.Person))]
        public User? User { get; set; }

        [InverseProperty(nameof(Invoice.Client))]
        public ICollection<Invoice>? ClientInvoices { get; set; }

        [InverseProperty(nameof(Invoice.Supplier))]
        public ICollection<Invoice>? SupplierInvoices { get; set; }

        [InverseProperty(nameof(Invoice.CreatedByPerson))]
        public ICollection<Invoice>? CreatedInvoices { get; set; }

        [InverseProperty(nameof(Expense.CreatedByPerson))]
        public ICollection<Expense>? ExpensesCreated { get; set; }

        [InverseProperty(nameof(Debt.ClientPerson))]
        public ICollection<Debt>? DebtsAsClient { get; set; }

        [InverseProperty(nameof(Debt.SupplierPerson))]
        public ICollection<Debt>? DebtsAsSupplier { get; set; }

        [InverseProperty(nameof(Debt.CreatedByPerson))]
        public ICollection<Debt>? DebtsCreated { get; set; }

        [InverseProperty(nameof(ReturnTbl.Client))]
        public ICollection<ReturnTbl>? ReturnsAsClient { get; set; }

        [InverseProperty(nameof(ReturnTbl.Supplier))]
        public ICollection<ReturnTbl>? ReturnsAsSupplier { get; set; }
    }
}