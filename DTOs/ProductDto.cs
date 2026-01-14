namespace MyAPIv3.DTOs
{
    public class ProductDto
    {
        public long Id { get; set; }
        public string ProductName { get; set; } = null!;
        public string? QrCode { get; set; }
        public decimal SellingPrice { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryTitle { get; set; }
        
        // Added for Flutter Compatibility (Default Unit)
        public long? UnitId { get; set; }
        public UnitDto? Unit { get; set; }
    }

    public class CreateProductDto
    {
        public string ProductName { get; set; } = null!;
        public string? QrCode { get; set; }
        public decimal SellingPrice { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public long? CategoryId { get; set; }
        
        // Added for Flutter Compatibility
        public long? UnitId { get; set; }
    }

    public class UpdateProductDto
    {
        public string ProductName { get; set; } = null!;
        public string? QrCode { get; set; }
        public decimal SellingPrice { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public long? CategoryId { get; set; }

        // Added for Flutter Compatibility
        public long? UnitId { get; set; }
    }
}
