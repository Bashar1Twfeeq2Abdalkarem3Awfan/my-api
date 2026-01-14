using System;
using System.Collections.Generic;
using MyAPIv3.Models;

namespace MyAPIv3.DTOs
{
    public class InvoiceDto
    {
        public long Id { get; set; }
        public string InvoiceType { get; set; }
        //public InvoiceTypeEnum InvoiceType { get; set; }
        public string? PaymentReference { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountRemaining { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
        public long? CreatedBy { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Navigation
        public PersonDto? Client { get; set; }
        public PersonDto? Supplier { get; set; }
        public PersonDto? CreatedByPerson { get; set; }
        public List<InvoiceProductDto>? InvoiceProducts { get; set; }
    }

    public class CreateInvoiceDto
    {
        //public InvoiceTypeEnum InvoiceType { get; set; }
        public string InvoiceType { get; set; }
        public string? PaymentReference { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; } = 0m;
        public decimal AmountPaid { get; set; } = 0m;
        public decimal AmountRemaining { get; set; } = 0m;
        public DateTime? InvoiceDate { get; set; }
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
        public long? CreatedBy { get; set; }
        public string? Notes { get; set; }
        public List<CreateInvoiceProductDto>? InvoiceProducts { get; set; }
    }

    public class UpdateInvoiceDto
    {
        //public InvoiceTypeEnum InvoiceType { get; set; }
        public string InvoiceType { get; set; }
        public string? PaymentReference { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountRemaining { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
        public long? CreatedBy { get; set; }
        public string? Notes { get; set; }
    }
}

