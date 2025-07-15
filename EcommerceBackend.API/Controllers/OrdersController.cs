//using Microsoft.AspNetCore.Mvc;
//using EcommerceBackend.API.Dtos;
//using EcommerceBackend.DataAccess.Models;
//using System;
//using System.Linq;

//namespace EcommerceBackend.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class OrdersController : ControllerBase
//    {
//        private readonly EcommerceDBContext _context;
//        public OrdersController(EcommerceDBContext context)
//        {
//            _context = context;
//        }

//        [HttpPost]
//        public IActionResult CreateOrder([FromBody] OrderRequestDto orderDto)
//        {
//            if (orderDto == null || orderDto.Items == null || !orderDto.Items.Any())
//                return BadRequest("Dữ liệu đơn hàng không hợp lệ");

//            var order = new Order
//            {
//                CustomerId = orderDto.CustomerId,
//                AmountDue = orderDto.TotalAmount
//            };
//            _context.Orders.Add(order);
//            _context.SaveChanges();

//            foreach (var item in orderDto.Items)
//            {
//                var detail = new OrderDetail
//                {
//                    OrderId = order.OrderId,
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    Price = item.Price,
//                    Quantity = item.Quantity
//                };
//                _context.OrderDetails.Add(detail);
//            }
//            _context.SaveChanges();

//            return Ok(new OrderResponseDto
//            {
//                OrderId = order.OrderId,
//                Status = "Success",
//                Message = "Đặt hàng thành công!"
//            });
//        }

//        [HttpGet]
//        public IActionResult GetOrders([FromQuery] int customerId)
//        {
//            var orders = _context.Orders
//                .Where(o => o.CustomerId == customerId)
//                .OrderByDescending(o => o.CreatedAt)
//                .Select(o => new OrderViewDto
//                {
//                    OrderId = o.OrderId,
//                    AmountDue = o.AmountDue ?? 0,
//                    Items = o.OrderDetails.Select(d => new OrderDetailViewDto
//                    {
//                        ProductId = d.ProductId ?? 0,
//                        ProductName = d.ProductName,
//                        Price = d.Price ?? 0,
//                        Quantity = d.Quantity ?? 0
//                    }).ToList()
//                }).ToList();
//            return Ok(orders);
//        }

//        [HttpGet("{orderId}")]
//        public IActionResult GetOrderDetail(int orderId)
//        {
//            var order = _context.Orders
//                .Where(o => o.OrderId == orderId)
//                .Select(o => new OrderViewDto
//                {
//                    OrderId = o.OrderId,
//                    AmountDue = o.AmountDue ?? 0,
//                    Items = o.OrderDetails.Select(d => new OrderDetailViewDto
//                    {
//                        ProductId = d.ProductId ?? 0,
//                        ProductName = d.ProductName,
//                        Price = d.Price ?? 0,
//                        Quantity = d.Quantity ?? 0
//                    }).ToList()
//                }).FirstOrDefault();
//            if (order == null) return NotFound();
//            return Ok(order);
//        }
//    }
//} 