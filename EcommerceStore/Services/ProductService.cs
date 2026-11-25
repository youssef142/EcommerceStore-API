using EcommerceStore.Dtos;
using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;
using EcommerceStore.Services.Interfaces;

namespace EcommerceStore.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Task<IEnumerable<Product>> GetAllProductsAsync() =>
            _unitOfWork.Products.GetAllProductsWithCategoryAsync();

        public Task<IEnumerable<Product>> GetActiveProductsAsync() =>
            _unitOfWork.Products.GetActiveProductsAsync();

        public Task<Product?> GetProductByIdAsync(int productId) =>
            _unitOfWork.Products.GetProductWithCategoryAsync(productId);

        public Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId) =>
            _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);

        public Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm) =>
            _unitOfWork.Products.SearchProductsAsync(searchTerm);

        public async Task<PagedResult<Product>> GetPagedProductsAsync(ProductFilter filter)
        {
            var (products, totalCount) = await _unitOfWork.Products.GetPagedProductsAsync(
                filter.PageNumber,
                filter.PageSize,
                filter.CategoryId,
                filter.SearchTerm,
                filter.MinPrice,
                filter.MaxPrice,
                filter.SortBy,
                filter.IncludeInactive);

            return new PagedResult<Product>
            {
                Items = products.ToList(),
                Page = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<Product> CreateProductAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                ImageUrl = request.ImageUrl,
                IsActive = request.IsActive,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product created: {ProductName} (ID: {ProductId})", product.Name, product.ProductId);

            // reload so the Category navigation is populated for the response
            return (await _unitOfWork.Products.GetProductWithCategoryAsync(product.ProductId))!;
        }

        public async Task<Product?> UpdateProductAsync(int productId, UpdateProductRequest request)
        {
            // Load the existing row and copy only the editable fields.
            // (The old code attached a whole posted entity, which overwrote CreatedAt etc.)
            var product = await _unitOfWork.Products.GetProductWithCategoryAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Attempted to update non-existent product ID: {ProductId}", productId);
                return null;
            }

            product.Name = request.Name.Trim();
            product.Description = request.Description.Trim();
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.ImageUrl = request.ImageUrl;
            product.IsActive = request.IsActive;
            product.CategoryId = request.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Product updated: {ProductName} (ID: {ProductId})", product.Name, productId);

            // category may have changed: reload the navigation
            return await _unitOfWork.Products.GetProductWithCategoryAsync(productId);
        }

        public async Task<bool> SetProductImageAsync(int productId, string imageUrl)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return false;

            product.ImageUrl = imageUrl;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Attempted to delete non-existent product ID: {ProductId}", productId);
                return false;
            }

            // Soft delete - just deactivate the product
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product soft-deleted: {ProductName} (ID: {ProductId})", product.Name, productId);
            return true;
        }

        public Task<bool> ProductExistsAsync(int productId) =>
            _unitOfWork.Products.AnyAsync(p => p.ProductId == productId);

        public async Task<bool> IsProductInStockAsync(int productId, int quantity)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            return product != null && product.IsActive && product.StockQuantity >= quantity;
        }
    }
}
