namespace MyAPIv3.DTOs
{
    public class ProductUnitDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long UnitId { get; set; }
        public decimal SalePrice { get; set; }
        public decimal? ConversionFactor { get; set; }
        public bool IsDefault { get; set; }
        // Navigation
        public string? ProductName { get; set; }
        public string? UnitName { get; set; }
    }

    public class CreateProductUnitDto
    {
        public long ProductId { get; set; }
        public long UnitId { get; set; }
        public decimal SalePrice { get; set; }
        public decimal? ConversionFactor { get; set; } = 1.0m;
        public bool IsDefault { get; set; } = false;
    }

    public class UpdateProductUnitDto
    {
        public decimal SalePrice { get; set; }
        public decimal? ConversionFactor { get; set; }
        public bool IsDefault { get; set; }
    }
}

