using EcommerceBackend.DataAccess.Models;
using EcommerceBackend.DataAccess.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceBackend.BusinessObject.Services
{
    public interface IReviewService
    {
        Task<List<Review>> GetByProductIdAsync(int productId);
        Task<Review?> GetByIdAsync(int reviewId);
        Task<bool> AddAsync(Review review);
    }

    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repo;
        public ReviewService(IReviewRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<Review>> GetByProductIdAsync(int productId)
        {
            return await _repo.GetByProductIdAsync(productId);
        }
        public async Task<Review?> GetByIdAsync(int reviewId)
        {
            return await _repo.GetByIdAsync(reviewId);
        }
        public async Task<bool> AddAsync(Review review)
        {
            if (!await _repo.CanUserReviewAsync(review.UserId, review.ProductId))
                return false;
            await _repo.AddAsync(review);
            return true;
        }
    }
} 