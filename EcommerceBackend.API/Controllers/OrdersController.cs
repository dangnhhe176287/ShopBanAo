using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.API.Dtos;
using EcommerceBackend.DataAccess.Models;
using EcommerceBackend.BusinessObject.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly EcommerceDBContext _context;
        private readonly CartService _cartService;
        
        public OrdersController(EcommerceDBContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto orderDto)
        {
            if (orderDto == null || orderDto.Items == null || !orderDto.Items.Any())
                return BadRequest("Dữ liệu đơn hàng không hợp lệ");

            var order = new Order
            {
                CustomerId = orderDto.CustomerId,
                AmountDue = orderDto.TotalAmount + orderDto.ShippingFee, // Cộng thêm phí ship
                ShippingAddress = orderDto.ShippingAddress,
                OrderNote = orderDto.OrderNote,
                OrderStatusId = 1 // Pending
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in orderDto.Items)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    VariantId = item.VariantId.ToString(),
                    VariantAttributes = item.VariantAttributes
                };
                _context.OrderDetails.Add(detail);
            }
            await _context.SaveChangesAsync();

            // Xóa cart sau khi đặt hàng thành công
            try
            {
                await _cartService.ClearCart(orderDto.CustomerId);
                Console.WriteLine($"Cart cleared successfully for customer {orderDto.CustomerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cart for customer {orderDto.CustomerId}: {ex.Message}");
                // Vẫn tiếp tục trả về thành công vì order đã được tạo
            }

            return Ok(new EcommerceBackend.API.Dtos.OrderResponseDto
            {
                OrderId = order.OrderId,
                Status = "Success",
                Message = "Đặt hàng thành công!"
            });
        }

        [HttpGet]
        public IActionResult GetOrders([FromQuery] int customerId)
        {
            var orders = _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderViewDto
                {
                    OrderId = o.OrderId,
                    AmountDue = o.AmountDue ?? 0,
                    ShippingAddress = o.ShippingAddress,
                    OrderNote = o.OrderNote,
                    OrderStatusId = o.OrderStatusId ?? 1,
                    OrderStatusTitle = o.OrderStatus.OrderStatusTittle,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Items = o.OrderDetails.Select(d => new OrderDetailViewDto
                    {
                        ProductId = d.ProductId ?? 0,
                        ProductName = d.ProductName,
                        Price = d.Price ?? 0,
                        Quantity = d.Quantity ?? 0,
                        VariantAttributes = d.VariantAttributes
                    }).ToList()
                }).ToList();
            return Ok(orders);
        }

//[HttpGet]
//public IActionResult GetOrders([FromQuery] int customerId)
//{
//    var orders = _context.Orders
//        .Where(o => o.CustomerId == customerId)
//        .OrderByDescending(o => o.CreatedAt)
//        .Select(o => new OrderViewDto
//        {
//            OrderId = o.OrderId,
//            AmountDue = o.AmountDue ?? 0,
//            Items = o.OrderDetails.Select(d => new OrderDetailViewDto
//            {
//                ProductId = d.ProductId ?? 0,
//                ProductName = d.ProductName,
//                Price = d.Price ?? 0,
//                Quantity = d.Quantity ?? 0
//            }).ToList()
//        }).ToList();
//    return Ok(orders);
//}


        [HttpGet("{orderId}")]
        public IActionResult GetOrderDetail(int orderId)
        {
            var order = _context.Orders
                .Where(o => o.OrderId == orderId)
                .Select(o => new OrderViewDto
                {
                    OrderId = o.OrderId,
                    AmountDue = o.AmountDue ?? 0,
                    ShippingAddress = o.ShippingAddress,
                    OrderNote = o.OrderNote,
                    OrderStatusId = o.OrderStatusId ?? 1,
                    OrderStatusTitle = o.OrderStatus.OrderStatusTittle,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Items = o.OrderDetails.Select(d => new OrderDetailViewDto
                    {
                        ProductId = d.ProductId ?? 0,
                        ProductName = d.ProductName,
                        Price = d.Price ?? 0,
                        Quantity = d.Quantity ?? 0,
                        VariantAttributes = d.VariantAttributes
                    }).ToList()
                }).FirstOrDefault();
            if (order == null) return NotFound();
            return Ok(order);
        }
    }
} 