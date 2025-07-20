using EcommerceBackend.BusinessObject.Services;
using EcommerceBackend.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EcommerceBackend.API.Dtos;

namespace EcommerceBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _service;
        public RatingController(IRatingService service)
        {
            _service = service;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var ratings = await _service.GetByProductIdAsync(productId);
            return Ok(ratings);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] RatingRequestDto dto)
        {
            var rating = new Rating
            {
                ProductId = dto.ProductId,
                UserId = dto.UserId,
                Score = dto.Score
            };
            var success = await _service.AddAsync(rating);
            if (!success)
                return BadRequest("Bạn chỉ có thể rating sản phẩm đã mua!");
            return Ok();
        }
    }
} 