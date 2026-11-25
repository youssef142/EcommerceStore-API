using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EcommerceStore.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetAllProductsWithCategoryAsync()
        {
            return await _dbSet
                .Include(p => p.Category)  // ✅ Load Category navigation property
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveProductsAsync();

            searchTerm = searchTerm.ToLower();
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsActive && 
                       (p.Name.ToLower().Contains(searchTerm) || 
                        p.Description.ToLower().Contains(searchTerm)))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithCategoryAsync(int productId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            int? categoryId = null,
            string? searchTerm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = null,
            bool includeInactive = false)
        {
            IQueryable<Product> query = _dbSet.Include(p => p.Category);

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            // Apply filters
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search) || 
                                        p.Description.ToLower().Contains(search));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            query = sortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "oldest" => query.OrderBy(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name) // Default: name ascending
            };
            var totalCount = await query.CountAsync();

            var products = await query               
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }
    }
}
