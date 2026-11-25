using EcommerceStore.Models;

namespace EcommerceStore.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
        Task<string> GenerateOrderNumberAsync();
    }
}
