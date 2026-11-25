using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;

namespace EcommerceStore.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface for managing database transactions
    /// and coordinating multiple repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IRepository<Category> Categories { get; }
        ICartRepository Carts { get; }
        IOrderRepository Orders { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
