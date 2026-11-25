using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers.Admin
{
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : ApiControllerBase
    {
        private readonly IProductService _products;
        private readonly ICategoryService _categories;
        private readonly IImageStorageService _images;

        public AdminProductsController(
            IProductService products,
            ICategoryService categories,
            IImageStorageService images)
        {
            _products = products;
            _categories = categories;
            _images = images;
        }

        // GET api/admin/products?includeInactive=true&...
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductFilter filter)
        {
            var page = await _products.GetPagedProductsAsync(filter);
            var baseUrl = BaseUrl;
            return page.Map(p => p.ToDto(baseUrl));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var product = await _products.GetProductByIdAsync(id); // includes inactive
            if (product is null) return NotFound();

            return product.ToDto(BaseUrl);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request)
        {
            if (!await _categories.IsAssignableAsync(request.CategoryId)) return InvalidCategory();

            var product = await _products.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product.ToDto(BaseUrl));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductDto>> Update(int id, UpdateProductRequest request)
        {
            if (!await _categories.IsAssignableAsync(request.CategoryId)) return InvalidCategory();

            var product = await _products.UpdateProductAsync(id, request);
            if (product is null) return NotFound();

            return product.ToDto(BaseUrl);
        }

        // Soft delete (sets IsActive = false), same as before
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _products.DeleteProductAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // POST api/admin/products/5/image   (multipart/form-data, field name: "file")
        [HttpPost("{id:int}/image")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<ActionResult<ProductDto>> UploadImage(int id, IFormFile file, CancellationToken cancellationToken)
        {
            if (!await _products.ProductExistsAsync(id)) return NotFound();

            var imageUrl = await _images.SaveProductImageAsync(file, cancellationToken);
            await _products.SetProductImageAsync(id, imageUrl);

            var product = await _products.GetProductByIdAsync(id);
            return product!.ToDto(BaseUrl);
        }

        private ActionResult InvalidCategory()
        {
            ModelState.AddModelError(nameof(CreateProductRequest.CategoryId), "Category does not exist or is inactive.");
            return ValidationProblem(ModelState);
        }
    }
}
