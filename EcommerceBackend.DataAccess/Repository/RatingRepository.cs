using EcommerceBackend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceBackend.DataAccess.Repository
{
    public interface IRatingRepository
    {
        Task<List<Rating>> GetByProductIdAsync(int productId);
        Task<Rating?> GetByIdAsync(int ratingId);
        Task AddAsync(Rating rating);
        Task<bool> CanUserRateAsync(int userId, int productId);
    }

    public class RatingRepository : IRatingRepository
    {
        private readonly EcommerceDBContext _context;
        public RatingRepository(EcommerceDBContext context)
        {
            _context = context;
        }
        public async Task<List<Rating>> GetByProductIdAsync(int productId)
        {
            return await _context.Ratings.Where(r => r.ProductId == productId && r.IsVisible).ToListAsync();
        }
        public async Task<Rating?> GetByIdAsync(int ratingId)
        {
            return await _context.Ratings.FindAsync(ratingId);
        }
        public async Task AddAsync(Rating rating)
        {
            await _context.Ratings.AddAsync(rating);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> CanUserRateAsync(int userId, int productId)
        {
            // User phải có ít nhất 1 OrderDetail với ProductId này
            return await _context.OrderDetails.AnyAsync(od => od.ProductId == productId && od.Order != null && od.Order.CustomerId == userId);
        }
    }
} 