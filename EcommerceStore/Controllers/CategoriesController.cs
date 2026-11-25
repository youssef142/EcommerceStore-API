using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers
{
    [Route("api/categories")]
    public class CategoriesController : ApiControllerBase
    {
        private readonly ICategoryService _categories;

        public CategoriesController(ICategoryService categories) => _categories = categories;

        // GET api/categories  (active only - used by the product filter)
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll()
        {
            var categories = await _categories.GetAllAsync(includeInactive: false);
            return categories.Select(c => c.ToDto()).ToList();
        }
    }
}
