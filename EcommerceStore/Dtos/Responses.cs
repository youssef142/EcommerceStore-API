using EcommerceStore.Models;

namespace EcommerceStore.Dtos
{
    public record UserDto(string Id, string Email, string? FirstName, string? LastName, IReadOnlyList<string> Roles);
    public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

    public record CategoryDto(int Id, string Name, string? Description, bool IsActive);

    public record ProductDto(
        int Id,
        string Name,
        string Description,
        decimal Price,
        int StockQuantity,
        bool InStock,
        string? ImageUrl,
        bool IsActive,
        int CategoryId,
        string CategoryName,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    public record CartItemDto(
        int ProductId,
        string ProductName,
        string? ImageUrl,
        decimal UnitPrice,
        int Quantity,
        decimal TotalPrice,
        int AvailableStock);

    public record CartDto(IReadOnlyList<CartItemDto> Items, int TotalItems, decimal TotalAmount);
    public record CartCountDto(int Count);

    public record OrderItemDto(
        int ProductId,
        string ProductName,
        string? ImageUrl,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice);

    public record OrderSummaryDto(
        int Id,
        string OrderNumber,
        OrderStatus Status,
        decimal TotalAmount,
        DateTime OrderDate,
        int ItemCount,
        string Email);

    public record OrderDetailDto(
        int Id,
        string OrderNumber,
        OrderStatus Status,
        decimal TotalAmount,
        string ShippingAddress,
        string ShippingCity,
        string ShippingPostalCode,
        string ShippingCountry,
        string Email,
        string PhoneNumber,
        string? Notes,
        DateTime OrderDate,
        DateTime? ShippedDate,
        DateTime? DeliveredDate,
        IReadOnlyList<OrderItemDto> Items);

    public record DashboardDto(
        int TotalProducts,
        int ActiveProducts,
        int TotalCategories,
        int TotalOrders,
        int PendingOrders,
        IReadOnlyList<OrderSummaryDto> RecentOrders);
}
