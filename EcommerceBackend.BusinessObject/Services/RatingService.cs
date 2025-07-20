using EcommerceBackend.DataAccess.Models;
using EcommerceBackend.DataAccess.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceBackend.BusinessObject.Services
{
    public interface IRatingService
    {
        Task<List<Rating>> GetByProductIdAsync(int productId);
        Task<Rating?> GetByIdAsync(int ratingId);
        Task<bool> AddAsync(Rating rating);
    }

    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _repo;
        public RatingService(IRatingRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<Rating>> GetByProductIdAsync(int productId)
        {
            return await _repo.GetByProductIdAsync(productId);
        }
        public async Task<Rating?> GetByIdAsync(int ratingId)
        {
            return await _repo.GetByIdAsync(ratingId);
        }
        public async Task<bool> AddAsync(Rating rating)
        {
            if (!await _repo.CanUserRateAsync(rating.UserId, rating.ProductId))
                return false;
            await _repo.AddAsync(rating);
            return true;
        }
    }
} 