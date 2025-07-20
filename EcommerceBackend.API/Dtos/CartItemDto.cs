namespace EcommerceBackend.API.Dtos
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public int VariantId { get; set; }
    }
} 