namespace EcommerceBackend.BusinessObject.dtos
{
    public class OrderRequestDto
    {
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderDetailDto> Items { get; set; }
        public string? ShippingAddress { get; set; }
        public string? OrderNote { get; set; }
    }

} 