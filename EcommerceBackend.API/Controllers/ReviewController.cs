using EcommerceBackend.BusinessObject.Services;
using EcommerceBackend.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EcommerceBackend.API.Dtos;

namespace EcommerceBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _service;
        public ReviewController(IReviewService service)
        {
            _service = service;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var reviews = await _service.GetByProductIdAsync(productId);
            return Ok(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ReviewRequestDto dto)
        {
            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = dto.UserId,
                Content = dto.Content
            };
            var success = await _service.AddAsync(review);
            if (!success)
                return BadRequest("Bạn chỉ có thể đánh giá sản phẩm đã mua!");
            return Ok();
        }
    }
} 