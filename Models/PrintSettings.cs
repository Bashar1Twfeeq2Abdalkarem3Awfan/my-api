using System;

namespace MyAPIv3.Models
{
    /// <summary>
    /// إعدادات الطباعة
    /// Print settings model
    /// </summary>
    public class PrintSettings
    {
        public int Id { get; set; }
        
        /// <summary>
        /// نوع الطابعة: "thermal_80" أو "a4"
        /// Printer type: thermal_80 or a4
        /// </summary>
        public string PrinterType { get; set; } = "thermal_80";
        
        /// <summary>
        /// إظهار شعار الشركة في الفواتير
        /// Show company logo in invoices
        /// </summary>
        public bool ShowLogo { get; set; } = true;
        
        /// <summary>
        /// إظهار معلومات الشركة في الفواتير
        /// Show company info in invoices
        /// </summary>
        public bool ShowCompanyInfo { get; set; } = true;
        
        /// <summary>
        /// نص إضافي في أسفل الفاتورة
        /// Footer text for invoices
        /// </summary>
        public string? FooterText { get; set; }
        
        /// <summary>
        /// حجم الخط
        /// Font size
        /// </summary>
        public int FontSize { get; set; } = 12;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
