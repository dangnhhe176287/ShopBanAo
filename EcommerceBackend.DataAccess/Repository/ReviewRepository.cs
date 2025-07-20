using EcommerceBackend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceBackend.DataAccess.Repository
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetByProductIdAsync(int productId);
        Task<Review?> GetByIdAsync(int reviewId);
        Task AddAsync(Review review);
        Task<bool> CanUserReviewAsync(int userId, int productId);
    }

    public class ReviewRepository : IReviewRepository
    {
        private readonly EcommerceDBContext _context;
        public ReviewRepository(EcommerceDBContext context)
        {
            _context = context;
        }
        public async Task<List<Review>> GetByProductIdAsync(int productId)
        {
            return await _context.Reviews.Where(r => r.ProductId == productId && r.IsVisible).ToListAsync();
        }
        public async Task<Review?> GetByIdAsync(int reviewId)
        {
            return await _context.Reviews.FindAsync(reviewId);
        }
        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> CanUserReviewAsync(int userId, int productId)
        {
            // User phải có ít nhất 1 OrderDetail với ProductId này
            return await _context.OrderDetails.AnyAsync(od => od.ProductId == productId && od.Order != null && od.Order.CustomerId == userId);
        }
    }
} 