using System;

namespace MyAPIv3.Models
{
    /// <summary>
    /// إعدادات الشركة/المنشأة
    /// Company settings model
    /// </summary>
    public class CompanySettings
    {
        public int Id { get; set; }
        
        /// <summary>
        /// اسم الشركة
        /// </summary>
        public string CompanyName { get; set; } = "متجر جديد";
        
        /// <summary>
        /// مسار شعار الشركة
        /// </summary>
        public string? LogoPath { get; set; }
        
        /// <summary>
        /// العنوان التفصيلي
        /// </summary>
        public string? Address { get; set; }
        
        /// <summary>
        /// الرقم الضريبي
        /// </summary>
        public string? TaxId { get; set; }
        
        /// <summary>
        /// رقم الهاتف
        /// </summary>
        public string? PhoneNumber { get; set; }
        
        /// <summary>
        /// البريد الإلكتروني
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// السجل التجاري
        /// </summary>
        public string? CommercialRegistration { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
