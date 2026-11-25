using EcommerceStore.Dtos;
using EcommerceStore.Models;

namespace EcommerceStore.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<Category>> GetAllAsync(bool includeInactive);
        Task<Category?> GetByIdAsync(int id);
        /// <summary>True when the category exists and is active (i.e. products can be assigned to it).</summary>
        Task<bool> IsAssignableAsync(int id);
        Task<Category> CreateAsync(CreateCategoryRequest request);
        Task<Category?> UpdateAsync(int id, UpdateCategoryRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
