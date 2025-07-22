using System.Collections.Generic;

namespace EcommerceBackend.API.Dtos
{
    public class OrderRequestDto
    {
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; } = 0;
        public List<OrderDetailDto> Items { get; set; }
        public string? ShippingAddress { get; set; }
        public string? OrderNote { get; set; }
        public int OrderStatusId { get; set; } = 1;
    }
    public class OrderDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int VariantId { get; set; }
        public string? VariantAttributes { get; set; }
    }
} 