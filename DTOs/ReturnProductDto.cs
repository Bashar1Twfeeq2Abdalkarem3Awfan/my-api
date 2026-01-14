namespace MyAPIv3.DTOs
{
    public class ReturnProductDto
    {
        public long Id { get; set; }
        public long ReturnId { get; set; }
        public long ProductId { get; set; }
        
        // ← إضافة جديدة: الوحدة المستخدمة في المرتجع
        public long UnitId { get; set; }
        
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
        
        // Navigation للعرض
        public string? ProductName { get; set; }
        
        // ← إضافة جديدة: اسم الوحدة للعرض
        public string? UnitName { get; set; }
    }

    public class CreateReturnProductDto
    {
        public long ProductId { get; set; }
        
        // ← إضافة جديدة: يجب تحديد الوحدة (مطلوب)
        public long UnitId { get; set; }
        
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateReturnProductDto
    {
        public long ProductId { get; set; }
        
        // ← إضافة جديدة: السماح بتحديث الوحدة
        public long UnitId { get; set; }
        
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
