using EcommerceBackend.DataAccess.Abstract;
using EcommerceBackend.DataAccess.Models;
using System.Threading.Tasks;

namespace EcommerceBackend.BusinessObject.Services
{
    public class CartService
    {
        private readonly ICartRepository _cartRepository;
        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task AddToCart(int userId, int productId, int quantity)
        {
            await _cartRepository.AddToCartAsync(userId, productId, quantity);
        }
        public async Task UpdateCartItem(int userId, int productId, int quantity)
        {
            await _cartRepository.UpdateCartItemAsync(userId, productId, quantity);
        }

        public async Task<Cart> GetCartByUserId(int userId)
        {
            return await _cartRepository.GetCartByUserIdAsync(userId);
        }
    }
} 