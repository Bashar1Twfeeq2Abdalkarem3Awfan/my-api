namespace MyAPIv3.DTOs
{
    /// <summary>
    /// DTO لعرض الكميات المرتجعة لكل منتج من فاتورة معينة
    /// DTO for displaying returned quantities per product from a specific invoice
    /// </summary>
    public class ReturnedQuantityDto
    {
        public long ProductId { get; set; }
        public long UnitId { get; set; }
        public decimal TotalReturnedQuantity { get; set; }
        public string? ProductName { get; set; }
        public string? UnitName { get; set; }
    }
}
