using System.ComponentModel.DataAnnotations;
using EcommerceStore.Models;

namespace EcommerceStore.Dtos
{
    // ---------- Auth ----------
    public class RegisterRequest
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        /// <summary>Guest cart id (the X-Cart-Id the client was using) so it can be merged.</summary>
        public string? GuestCartId { get; set; }
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? GuestCartId { get; set; }
    }

    public class GoogleLoginRequest
    {
        /// <summary>The Google ID token (credential) obtained by the React app.</summary>
        [Required]
        public string IdToken { get; set; } = string.Empty;

        public string? GuestCartId { get; set; }
    }

    // ---------- Products ----------
    public class ProductFilter
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 12;

        /// <summary>name_asc | name_desc | price_asc | price_desc | newest | oldest</summary>
        public string? SortBy { get; set; } = "name_asc";

        /// <summary>Only honoured by the admin endpoints.</summary>
        public bool IncludeInactive { get; set; }
    }

    public class CreateProductRequest
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        /// <summary>Relative path (e.g. /images/products/x.jpg) or absolute http(s) URL.</summary>
        [StringLength(500)]
        [RegularExpression(@"^(/|https?://).*", ErrorMessage = "ImageUrl must start with '/' or 'http(s)://'.")]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
    }

    public class UpdateProductRequest : CreateProductRequest
    {
    }

    // ---------- Categories ----------
    public class CreateCategoryRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateCategoryRequest : CreateCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }

    // ---------- Cart ----------
    public class AddCartItemRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequest
    {
        /// <summary>0 (or less) removes the item.</summary>
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }

    // ---------- Orders ----------
    /// <summary>
    /// Checkout payload. Deliberately has NO cart: the server always loads the
    /// authenticated user's cart itself, so the client can't tamper with prices/items.
    /// </summary>
    public class CheckoutRequest
    {
        [Required, StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Country { get; set; } = "Egypt";

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Cash on Delivery";
    }

    public class UpdateOrderStatusRequest
    {
        [Required]
        public OrderStatus? Status { get; set; }
    }

    // ---------- Contact ----------
    public class ContactRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}
