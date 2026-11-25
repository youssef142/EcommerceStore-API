using EcommerceStore.Dtos;
using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;
using EcommerceStore.Services.Interfaces;

namespace EcommerceStore.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Category>> GetAllAsync(bool includeInactive)
        {
            var categories = includeInactive
                ? await _unitOfWork.Categories.GetAllAsync()
                : await _unitOfWork.Categories.FindAsync(c => c.IsActive);

            return categories.OrderBy(c => c.Name).ToList();
        }

        public Task<Category?> GetByIdAsync(int id) => _unitOfWork.Categories.GetByIdAsync(id);

        public Task<bool> IsAssignableAsync(int id) =>
            _unitOfWork.Categories.AnyAsync(c => c.CategoryId == id && c.IsActive);

        public async Task<Category> CreateAsync(CreateCategoryRequest request)
        {
            var category = new Category
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Category created: {Name} (ID: {Id})", category.Name, category.CategoryId);
            return category;
        }

        public async Task<Category?> UpdateAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return null;

            category.Name = request.Name.Trim();
            category.Description = request.Description?.Trim();
            category.IsActive = request.IsActive;

            await _unitOfWork.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return false;

            // Soft delete - just deactivate
            category.IsActive = false;
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
