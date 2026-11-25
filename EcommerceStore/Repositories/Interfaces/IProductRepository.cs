using EcommerceStore.Models;

namespace EcommerceStore.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetAllProductsWithCategoryAsync();
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<Product?> GetProductWithCategoryAsync(int productId);
        Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedProductsAsync(
            int pageNumber, 
            int pageSize, 
            int? categoryId = null, 
            string? searchTerm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = null,
            bool includeInactive = false);
    }
}
