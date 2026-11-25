using EcommerceStore.Models;
using EcommerceStore.Dtos;

namespace EcommerceStore.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(string userId, CheckoutRequest checkoutModel);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task<bool> CancelOrderAsync(int orderId);
    }
}
