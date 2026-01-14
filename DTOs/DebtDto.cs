using System;

namespace MyAPIv3.DTOs
{
    public class DebtDto
    {
        public long Id { get; set; }
        public string? AccountName { get; set; }
        public string? AccountType { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Paid { get; set; }
        public decimal Remaining { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string? Notes { get; set; }
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        // Navigation
        public PersonDto? ClientPerson { get; set; }
        public PersonDto? SupplierPerson { get; set; }
        public PersonDto? CreatedByPerson { get; set; }
    }

    public class CreateDebtDto
    {
        public string? AccountName { get; set; }
        public string? AccountType { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Paid { get; set; }
        public decimal Remaining { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string? Notes { get; set; }
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
        public long? CreatedBy { get; set; }
    }

    public class UpdateDebtDto
    {
        public string? AccountName { get; set; }
        public string? AccountType { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Paid { get; set; }
        public decimal Remaining { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string? Notes { get; set; }
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
        public long? CreatedBy { get; set; }
    }
}

