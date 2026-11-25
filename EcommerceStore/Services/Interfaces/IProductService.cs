using EcommerceStore.Dtos;
using EcommerceStore.Models;

namespace EcommerceStore.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<Product?> GetProductByIdAsync(int productId);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<PagedResult<Product>> GetPagedProductsAsync(ProductFilter filter);
        Task<Product> CreateProductAsync(CreateProductRequest request);
        Task<Product?> UpdateProductAsync(int productId, UpdateProductRequest request);
        Task<bool> SetProductImageAsync(int productId, string imageUrl);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> ProductExistsAsync(int productId);
        Task<bool> IsProductInStockAsync(int productId, int quantity);
    }
}
