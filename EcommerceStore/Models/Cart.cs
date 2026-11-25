using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceStore.Models
{
    public class Cart
    {
        public int CartId { get; set; }
        
        public string? UserId { get; set; }

        // ✅ ADD THIS - Session ID for guest users
        public string? SessionId { get; set; }

        // ✅ ADD THIS - Flag to identify guest carts
        public bool IsGuest { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Computed property
        [NotMapped]
        public decimal TotalAmount => CartItems.Sum(ci => ci.TotalPrice);

        [NotMapped]
        public int TotalItems => CartItems.Sum(ci => ci.Quantity);
    }

    public class CartItem
    {
        public int CartItemId { get; set; }

        [Required]
        public int CartId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        // Computed property
        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
