using System.Collections.Generic;

namespace EcommerceBackend.API.Dtos
{
    public class OrderRequestDto
    {
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderDetailDto> Items { get; set; }
    }
    public class OrderDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
} 