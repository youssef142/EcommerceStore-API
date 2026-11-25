using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Repositories
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        // ✅ UNIFIED METHOD - Simple version
        public async Task<Cart?> GetCartAsync(string? userId, string? sessionId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                // Logged-in user
                return await _dbSet
                    .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsGuest);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                // Guest user
                return await _dbSet
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.IsGuest);
            }

            return null;
        }

        // ✅ UNIFIED METHOD - With includes (most commonly used)
        public async Task<Cart?> GetCartWithItemsAsync(string? userId, string? sessionId)
        {
            var query = _dbSet
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                // Logged-in user
                return await query.FirstOrDefaultAsync(c => c.UserId == userId && !c.IsGuest);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                // Guest user
                return await query.FirstOrDefaultAsync(c => c.SessionId == sessionId && c.IsGuest);
            }

            return null;
        }

        // ✅ SPECIFIC METHOD - Unique logic
        public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
        }

        // ✅ SPECIFIC METHOD - Unique operation
        public async Task RemoveCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await Task.CompletedTask;
        }

        // ✅ SPECIFIC METHOD - Unique operation
        public async Task ClearCartAsync(int cartId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
        }

        // ✅ RENAMED for consistency
        public async Task DeleteCartAsync(int cartId)
        {
            // First remove all cart items to avoid cascade issues
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _context.CartItems.RemoveRange(cartItems);
            }

            // Then remove the cart itself
            var cart = await _dbSet.FindAsync(cartId);
            if (cart != null)
            {
                _context.Remove(cart);
            }
        }

        // ✅ UTILITY METHOD
        public async Task<bool> CartExistsAsync(string? userId, string? sessionId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                return await _dbSet.AnyAsync(c => c.UserId == userId && !c.IsGuest);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                return await _dbSet.AnyAsync(c => c.SessionId == sessionId && c.IsGuest);
            }

            return false;
        }
    }
}