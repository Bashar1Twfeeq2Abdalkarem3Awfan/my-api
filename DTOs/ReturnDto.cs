using System;
using System.Collections.Generic;
using MyAPIv3.Models;

namespace MyAPIv3.DTOs
{
    public class ReturnDto
    {
        public long Id { get; set; }
        public string? CashierNumber { get; set; }
        public long? OriginalInvoiceId { get; set; }
        //public ReturnTypeEnum ReturnType { get; set; }
        public string ReturnType { get; set; }
        public string? InvoiceStatus { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Notes { get; set; }
        
        // ← إضافة جديدة: ربط المرتجع بالعميل أو المورد
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
        
        public DateTime? CreatedAt { get; set; }
        
        // Navigation
        public InvoiceDto? OriginalInvoice { get; set; }
        
        // ← إضافة جديدة: معلومات العميل والمورد
        public PersonDto? Client { get; set; }
        public PersonDto? Supplier { get; set; }
        
        public List<ReturnProductDto>? ReturnProducts { get; set; }
    }

    public class CreateReturnDto
    {
        public string? CashierNumber { get; set; }
        public long? OriginalInvoiceId { get; set; }
        public string ReturnType { get; set; }
        //public ReturnTypeEnum ReturnType { get; set; } = ReturnTypeEnum.ReturnFromCustomer;
        public string? InvoiceStatus { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Notes { get; set; }
        
        // ← إضافة جديدة: يجب تحديد العميل (للمرتجع من عميل) أو المورد (للمرتجع لمورد)
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
        
        public List<CreateReturnProductDto>? ReturnProducts { get; set; }
    }

    public class UpdateReturnDto
    {
        public string? CashierNumber { get; set; }
        public long? OriginalInvoiceId { get; set; }
        public string ReturnType { get; set; }
        //public ReturnTypeEnum ReturnType { get; set; }
        public string? InvoiceStatus { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Notes { get; set; }
        
        // ← إضافة جديدة: السماح بتحديث العميل/المورد
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
    }
}
