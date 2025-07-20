using EcommerceBackend.API.Dtos;
using EcommerceBackend.BusinessObject.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using EcommerceBackend.DataAccess.Repository;

namespace EcommerceBackend.API.Controllers
{
    [ApiController]
    [Route("cart")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        private readonly IProductRepository _productRepository;
        public CartController(CartService cartService, IProductRepository productRepository)
        {
            _cartService = cartService;
            _productRepository = productRepository;
        }

        [HttpPost("{userId}/add")]
        public async Task<IActionResult> AddToCart(int userId, [FromBody] CartItemDto item)
        {
            await _cartService.AddToCart(userId, item.ProductId, item.VariantId, item.Quantity);
            return Ok();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var cart = await _cartService.GetCartByUserId(userId);
            if (cart == null)
                return Ok(new Dtos.CartResponseDto { CartId = 0, CustomerId = userId, Items = new List<Dtos.CartItemDto>() });

            var items = new List<Dtos.CartItemDto>();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            if (Request.Host.Host == "localhost" || Request.Host.Host == "127.0.0.1")
            {
                baseUrl = "http://10.0.2.2:5287";
            }
            foreach (var cd in cart.CartDetails)
            {
                var product = await _productRepository.GetProductByIdAsync(cd.ProductId ?? 0);
                var imageUrl = product?.ProductImages?.FirstOrDefault()?.ImageUrl;
                if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.StartsWith("http"))
                {
                    imageUrl = baseUrl + imageUrl;
                }
                items.Add(new Dtos.CartItemDto
                {
                    ProductId = cd.ProductId ?? 0,
                    ProductName = product?.Name ?? $"ID: {cd.ProductId}",
                    Quantity = (int)cd.Quantity,
                    Price = cd.Price,
                    ImageUrl = imageUrl
                });
            }

            var dto = new Dtos.CartResponseDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                Items = items,
                AmountDue = cart.CartDetails.Sum(cd => (cd.Price ?? 0) * (cd.Quantity ?? 0))
            };
            return Ok(dto);
        }

        [HttpPut("{userId}/update")]
        public async Task<IActionResult> UpdateCartItem(int userId, [FromBody] CartItemDto item)
        {
            await _cartService.UpdateCartItem(userId, item.ProductId, item.VariantId, item.Quantity);
            return Ok();
        }

        [HttpDelete("{userId}/remove/{productId}/{variantId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int productId, int variantId)
        {
            await _cartService.AddToCart(userId, productId, variantId, 0); // Đặt quantity = 0 để xóa
            return Ok();
        }
    }
} 