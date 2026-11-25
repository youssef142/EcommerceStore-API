using EcommerceStore.Models;

namespace EcommerceStore.Repositories.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        // ✅ UNIFIED METHOD - Handles both logged-in and guest
        Task<Cart?> GetCartAsync(string? userId, string? sessionId);

        // ✅ UNIFIED METHOD - With items included
        Task<Cart?> GetCartWithItemsAsync(string? userId, string? sessionId);

        // ✅ SPECIFIC METHODS - Only when there's unique logic
        Task<CartItem?> GetCartItemAsync(int cartId, int productId);
        Task RemoveCartItemAsync(CartItem cartItem);
        Task ClearCartAsync(int cartId);
        Task DeleteCartAsync(int cartId);

        // ✅ UTILITY METHODS
        Task<bool> CartExistsAsync(string? userId, string? sessionId);
    }
}
