using System;
using System.Collections.Generic;

namespace EcommerceBackend.API.Dtos
{
    public class OrderViewDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal AmountDue { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<OrderDetailViewDto> Items { get; set; }
    }
    public class OrderDetailViewDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
} 