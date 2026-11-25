using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;
using EcommerceStore.Services.Interfaces;

namespace EcommerceStore.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartService> _logger;

        /// <summary>Header the React app sends to identify a guest cart (replaces the old server session).</summary>
        public const string CartIdHeaderName = "X-Cart-Id";

        public CartService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

        }
        public Task<Cart?> GetUserCartAsync()
        {
            try
            {
                var (userId, sessionId) = GetSessionOrUserId();
                return _unitOfWork.Carts.GetCartAsync(userId, sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user cart");
                throw;
            }
        }

        public Task<Cart?> GetUserCartWithItemsAsync()
        {
            try
            {
                var (userId, sessionId) = GetSessionOrUserId();
                return _unitOfWork.Carts.GetCartWithItemsAsync(userId, sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user cart with items");
                throw;
            }
        }
        
        public async Task<bool> AddToCartAsync(int productId, int quantity = 1)
        {
            var (userId, sessionId) = GetSessionOrUserId();
            if (userId == null && sessionId == null)
            {
                throw new BadHttpRequestException("A valid X-Cart-Id header (GUID) is required for guest carts.");
            }
            try
            {
                // Validate product exists and is in stock
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null || !product.IsActive)
                {
                    _logger.LogWarning("Attempted to add inactive/non-existent product {ProductId} to cart", productId);
                    return false;
                }

                if (product.StockQuantity < quantity)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}. Requested: {Quantity}, Available: {Stock}",
                        productId, quantity, product.StockQuantity);
                    return false;
                }

                // Get or create cart

                Cart? cart;
               
                cart = await _unitOfWork.Carts.GetCartAsync(userId, sessionId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        SessionId = sessionId,
                        IsGuest = userId == null,   
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Carts.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Check if item already exists in cart
                var existingItem = await _unitOfWork.Carts.GetCartItemAsync(cart.CartId, productId);

                if (existingItem != null)
                {
                    // Prevent exceeding stock
                    if (product.StockQuantity < existingItem.Quantity + quantity)
                    {
                        _logger.LogWarning(
                            "Cannot add more items. Stock limit reached for product {ProductId}",
                            productId);
                        return false;
                    }

                    existingItem.Quantity += quantity;
                    cart.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = product.Price,
                        AddedAt = DateTime.UtcNow
                    };

                    cart.CartItems.Add(cartItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;

                var result = await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    IsUserAuthenticated()
                        ? "Added {Quantity} of product {ProductId} to user cart"
                        : "Added {Quantity} of product {ProductId} to guest cart",
                    quantity, productId);

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart", productId);
                throw;
            }
        }

        public async Task<bool> UpdateCartItemAsync(int productId, int quantity)
        {
            var (userId, sessionId) = GetSessionOrUserId();
            try
            {
                if (quantity < 1)
                {
                    return await RemoveFromCartAsync(productId);
                }

                var cart = await _unitOfWork.Carts.GetCartAsync(userId, sessionId);
                if (cart == null)
                {
                    _logger.LogWarning("No cart found for user {UserId}", userId);
                    return false;
                }

                var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cart.CartId, productId);
                if (cartItem == null)
                {
                    _logger.LogWarning("Cart item not found for product {ProductId}", productId);
                    return false;
                }

                // Validate stock
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("Product isn't found");
                    return false;
                }
                else if (product.StockQuantity < quantity)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}", productId);
                    return false;
                }

                cartItem.Quantity = quantity;
                cart.UpdatedAt = DateTime.UtcNow;

                var result = await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated cart item quantity to {Quantity} for product {ProductId}, user {UserId}",
                    quantity, productId, userId);

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item for product {ProductId}, user {UserId}", productId, userId);
                throw;
            }
        }

        public async Task<bool> RemoveFromCartAsync(int productId)
        {
            var (userId, sessionId) = GetSessionOrUserId();
            try
            {               
                var cart = await _unitOfWork.Carts.GetCartAsync(userId, sessionId);
                if (cart == null)
                {
                    return false;
                }

                var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cart.CartId, productId);
                if (cartItem == null)
                {
                    return false;
                }

                await _unitOfWork.Carts.RemoveCartItemAsync(cartItem);
                cart.UpdatedAt = DateTime.UtcNow;

                var result = await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Removed product {ProductId} from cart. UserId: {UserId}, SessionId: {SessionId}",productId, userId, sessionId);

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from cart for UserId: {UserId}, SessionId: {SessionId}", productId, userId, sessionId);
                throw;
            }
        }

        public async Task<bool> ClearCartAsync()
        {
            var (userId, sessionId) = GetSessionOrUserId();
            try
            {
                var cart = await _unitOfWork.Carts.GetCartAsync(userId, sessionId);
                if (cart == null)
                {
                    return true; // Cart doesn't exist, so it's already "clear"
                }

                await _unitOfWork.Carts.ClearCartAsync(cart.CartId);
                cart.UpdatedAt = DateTime.UtcNow;

                var result = await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Cleared cart for user {UserId}", userId);

                return result >= 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetCartItemCountAsync()
        {
            var (userId, sessionId) = GetSessionOrUserId();
            try
            {
                var cart = await _unitOfWork.Carts.GetCartWithItemsAsync(userId, sessionId);
                return cart?.TotalItems ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>Guest cart id from the X-Cart-Id header, or null if missing/invalid.</summary>
        private string? GetGuestCartId()
        {
            var raw = _httpContextAccessor.HttpContext?.Request.Headers[CartIdHeaderName].FirstOrDefault();
            return Guid.TryParse(raw, out var id) ? id.ToString("D") : null;
        }

        // ✅ MERGE GUEST CART ON LOGIN (CRITICAL!)
        public async Task MergeGuestCartOnLoginAsync(string userId, string? guestCartId)
        {
            var sessionId = Guid.TryParse(guestCartId, out var parsedGuestId) ? parsedGuestId.ToString("D") : null;
            if (sessionId == null)
            {
                return; // client had no guest cart - nothing to merge
            }

            _logger.LogInformation("=== MERGE CART START === SessionId: {SessionId}, UserId: {UserId}", sessionId, userId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _logger.LogInformation("Transaction started");

                // Get guest cart
                var guestCart = await _unitOfWork.Carts.GetCartWithItemsAsync(null,sessionId);

                if (guestCart == null)
                {
                    _logger.LogWarning("No guest cart found for SessionId: {SessionId}", sessionId);
                    await _unitOfWork.CommitTransactionAsync();
                    return;
                }

                _logger.LogInformation("Guest cart found: CartId={CartId}, Items={ItemCount}",
                    guestCart.CartId, guestCart.CartItems.Count);

                if (!guestCart.CartItems.Any())
                {
                    _logger.LogInformation("Guest cart is empty, nothing to merge");
                    await _unitOfWork.CommitTransactionAsync();
                    return;
                }

                // Log guest cart items
                foreach (var item in guestCart.CartItems)
                {
                    _logger.LogInformation("Guest item: ProductId={ProductId}, Quantity={Quantity}, CartItemId={CartItemId}",
                        item.ProductId, item.Quantity, item.CartItemId);
                }

                // Get or create user cart
                var userCart = await _unitOfWork.Carts.GetCartWithItemsAsync(userId, null);

                if (userCart == null)
                {
                    _logger.LogInformation("No existing user cart. Converting guest cart {CartId} to user cart for UserId {UserId}", guestCart.CartId, userId);
                    
                    guestCart.UserId = userId;
                    guestCart.SessionId = null;
                    guestCart.IsGuest = false;
                    guestCart.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _logger.LogInformation("User cart exists: CartId={CartId}, Items={ItemCount}",
                        userCart.CartId, userCart.CartItems.Count);

                    // Merge guest items into user cart
                    foreach (var guestItem in guestCart.CartItems.ToList()) // Use ToList() to avoid collection modification issues
                    {
                        var existingItem = userCart.CartItems
                            .FirstOrDefault(ci => ci.ProductId == guestItem.ProductId);

                        if (existingItem != null)
                        {
                            _logger.LogInformation("Product {ProductId} exists in user cart, adding quantities: {ExistingQty} + {GuestQty}",
                                guestItem.ProductId, existingItem.Quantity, guestItem.Quantity);

                            // Add quantities
                            existingItem.Quantity += guestItem.Quantity;

                        }
                        else
                        {
                            _logger.LogInformation("Product {ProductId} new to user cart, adding item", guestItem.ProductId);

                            // Create new cart item (don't reuse guest item)
                            var newCartItem = new CartItem
                            {
                                CartId = userCart.CartId,
                                ProductId = guestItem.ProductId,
                                Quantity = guestItem.Quantity,
                                UnitPrice = guestItem.UnitPrice,
                                AddedAt = DateTime.UtcNow
                            };

                            userCart.CartItems.Add(newCartItem);
                        }
                    }

                    userCart.UpdatedAt = DateTime.UtcNow;

                    // Delete guest cart and its items
                    _logger.LogInformation("Deleting guest cart: {CartId}", guestCart.CartId);
                    await _unitOfWork.Carts.DeleteCartAsync(guestCart.CartId);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("=== MERGE CART SUCCESS === Cart merged for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== MERGE CART FAILED === Error merging guest cart for SessionId: {SessionId}, UserId: {UserId}",
                    sessionId, userId);

                try
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogInformation("Transaction rolled back");
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error during transaction rollback");
                }

                throw;
            }
        }     
        private (string? userId, string? sessionId) GetSessionOrUserId()
        {
            if (IsUserAuthenticated())
            {
                return (GetCurrentUserId(), null);
            }

            return (null, GetGuestCartId());
        }

        // ? HELPER: Check if user is authenticated
        private bool IsUserAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        // ? HELPER: Get current user ID
        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
