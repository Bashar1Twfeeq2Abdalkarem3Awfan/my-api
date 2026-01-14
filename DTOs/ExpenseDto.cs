using System;

namespace MyAPIv3.DTOs
{
    public class ExpenseDto
    {
        public long Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public string? Notes { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        // Navigation
        public PersonDto? CreatedByPerson { get; set; }
    }

    public class CreateExpenseDto
    {
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "YER";
        public DateTime? ExpenseDate { get; set; }
        public string? Notes { get; set; }
        public long? CreatedBy { get; set; }
    }

    public class UpdateExpenseDto
    {
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public string? Notes { get; set; }
        public long? CreatedBy { get; set; }
    }
}

