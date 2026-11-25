using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers.Admin
{
    [Route("api/admin/categories")]
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : ApiControllerBase
    {
        private readonly ICategoryService _categories;

        public AdminCategoriesController(ICategoryService categories) => _categories = categories;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll()
        {
            var categories = await _categories.GetAllAsync(includeInactive: true);
            return categories.Select(c => c.ToDto()).ToList();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var category = await _categories.GetByIdAsync(id);
            if (category is null) return NotFound();

            return category.ToDto();
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request)
        {
            var category = await _categories.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoryDto>> Update(int id, UpdateCategoryRequest request)
        {
            var category = await _categories.UpdateAsync(id, request);
            if (category is null) return NotFound();

            return category.ToDto();
        }

        // Soft delete (sets IsActive = false), same as before
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _categories.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
