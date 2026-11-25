using EcommerceStore.Models;

namespace EcommerceStore.Dtos
{
    /// <summary>Entity -> DTO mapping. Entities never leave the API.</summary>
    public static class Mappings
    {
        /// <summary>
        /// Stored image paths are relative ("/images/products/x.jpg" or "/uploads/products/y.jpg").
        /// The API returns absolute URLs so the React app can use them directly in &lt;img src&gt;.
        /// </summary>
        public static string? ToPublicUrl(this string? url, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return url;
            return baseUrl.TrimEnd('/') + (url.StartsWith('/') ? url : "/" + url);
        }

        public static CategoryDto ToDto(this Category c) =>
            new(c.CategoryId, c.Name, c.Description, c.IsActive);

        public static ProductDto ToDto(this Product p, string baseUrl) =>
            new(p.ProductId, p.Name, p.Description, p.Price, p.StockQuantity, p.IsInStock,
                p.ImageUrl.ToPublicUrl(baseUrl), p.IsActive, p.CategoryId,
                p.Category?.Name ?? string.Empty, p.CreatedAt, p.UpdatedAt);

        public static CartDto ToDto(this Cart? cart, string baseUrl)
        {
            if (cart is null) return new CartDto(Array.Empty<CartItemDto>(), 0, 0m);

            var items = cart.CartItems
                .OrderBy(ci => ci.AddedAt)
                .Select(ci => new CartItemDto(
                    ci.ProductId,
                    ci.Product?.Name ?? string.Empty,
                    ci.Product?.ImageUrl.ToPublicUrl(baseUrl),
                    ci.UnitPrice,
                    ci.Quantity,
                    ci.TotalPrice,
                    ci.Product?.StockQuantity ?? 0))
                .ToList();

            return new CartDto(items, cart.TotalItems, cart.TotalAmount);
        }

        public static OrderSummaryDto ToSummaryDto(this Order o) =>
            new(o.OrderId, o.OrderNumber, o.Status, o.TotalAmount, o.OrderDate,
                o.OrderItems.Sum(i => i.Quantity), o.Email);

        public static OrderDetailDto ToDetailDto(this Order o, string baseUrl) =>
            new(o.OrderId, o.OrderNumber, o.Status, o.TotalAmount,
                o.ShippingAddress, o.ShippingCity, o.ShippingPostalCode, o.ShippingCountry,
                o.Email, o.PhoneNumber, o.Notes, o.OrderDate, o.ShippedDate, o.DeliveredDate,
                o.OrderItems.Select(i => new OrderItemDto(
                    i.ProductId,
                    i.ProductName,
                    i.Product?.ImageUrl.ToPublicUrl(baseUrl),
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice)).ToList());
    }
}
