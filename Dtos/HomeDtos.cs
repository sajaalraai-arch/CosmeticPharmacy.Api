namespace CosmeticPharmacy.Api.Dtos
{
    public class HomeDto
    {
        public List<CategoryListItemDto> Categories { get; set; } = new();
        public List<ProductListItemDto> TopProducts { get; set; } = new();
    }

    public class DashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal InventoryValue { get; set; }
        public List<ProductListItemDto> LatestProducts { get; set; } = new();
    }
}
