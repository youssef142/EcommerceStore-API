using EcommerceStore.Models;

namespace EcommerceStore.Services.Interfaces
{
    public interface ICartService
    {
        Task<Cart?> GetUserCartAsync();
        Task<Cart?> GetUserCartWithItemsAsync();
        Task<bool> AddToCartAsync(int productId, int quantity = 1);
        Task<bool> UpdateCartItemAsync(int productId, int quantity);
        Task<bool> RemoveFromCartAsync(int productId);
        Task<bool> ClearCartAsync();
        Task<int> GetCartItemCountAsync();
        /// <summary>Merges the guest cart identified by <paramref name="guestCartId"/> (the client's X-Cart-Id) into the user's cart.</summary>
        Task MergeGuestCartOnLoginAsync(string userId, string? guestCartId);
    }
}
