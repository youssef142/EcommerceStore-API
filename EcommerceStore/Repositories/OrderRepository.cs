using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var date = DateTime.UtcNow;
            var prefix = $"ORD{date:yyyyMMdd}";
            
            var lastOrder = await _dbSet
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            if (lastOrder == null)
            {
                return $"{prefix}-0001";
            }

            var lastNumber = int.Parse(lastOrder.OrderNumber.Substring(lastOrder.OrderNumber.LastIndexOf('-') + 1));
            var newNumber = lastNumber + 1;

            return $"{prefix}-{newNumber:D4}";
        }
    }
}
