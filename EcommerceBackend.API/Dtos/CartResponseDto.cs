using System.Collections.Generic;

namespace EcommerceBackend.API.Dtos
{
    public class CartResponseDto
    {
        public int CartId { get; set; }
        public int? CustomerId { get; set; }
        public List<CartItemDto> Items { get; set; }
        public decimal AmountDue { get; set; }
    }
} 