using System.ComponentModel.DataAnnotations;

namespace MyAPIv3.DTOs
{
    /// <summary>
    /// DTO for making a payment on an invoice
    /// كائنات نقل البيانات لتسديد دفعة على فاتورة
    /// </summary>
    public class MakePaymentDto
    {
        /// <summary>
        /// المبلغ المدفوع
        /// Payment amount
        /// </summary>
        [Required(ErrorMessage = "المبلغ المدفوع مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        /// <summary>
        /// طريقة الدفع (نقدي، بنكي، إلخ)
        /// Payment method (cash, bank transfer, etc.)
        /// </summary>
        [MaxLength(100)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// رقم مرجعي للدفعة (اختياري)
        /// Payment reference number (optional)
        /// </summary>
        [MaxLength(200)]
        public string? PaymentReference { get; set; }

        /// <summary>
        /// ملاحظات حول الدفعة
        /// Notes about the payment
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// تاريخ الدفع (افتراضياً الآن)
        /// Payment date (defaults to now)
        /// </summary>
        public DateTime? PaymentDate { get; set; }
    }

    /// <summary>
    /// Response DTO after making a payment
    /// استجابة بعد تسديد الدفعة
    /// </summary>
    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public decimal PreviousAmountPaid { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal NewAmountPaid { get; set; }
        public decimal NewAmountRemaining { get; set; }
        public bool IsFullyPaid { get; set; }
    }
}
