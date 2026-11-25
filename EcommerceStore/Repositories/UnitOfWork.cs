using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace EcommerceStore.Repositories
{
    /// <summary>
    /// Unit of Work implementation for transaction management
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IProductRepository Products { get; }
        public IRepository<Category> Categories { get; }
        public ICartRepository Carts { get; }
        public IOrderRepository Orders { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IProductRepository productRepository,
            IRepository<Category> categoryRepository,
            ICartRepository cartRepository,
            IOrderRepository orderRepository)
        {
            _context = context;
            Products = productRepository;
            Categories = categoryRepository;
            Carts = cartRepository;
            Orders = orderRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
