using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers
{
    [Route("api/products")]
    public class ProductsController : ApiControllerBase
    {
        private readonly IProductService _products;

        public ProductsController(IProductService products) => _products = products;

        // GET api/products?searchTerm=&categoryId=&minPrice=&maxPrice=&sortBy=&pageNumber=&pageSize=
        // (the home page "featured" list is just GET api/products?pageSize=8)
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductFilter filter)
        {
            filter.IncludeInactive = false; // the public endpoint never exposes inactive products

            var page = await _products.GetPagedProductsAsync(filter);
            var baseUrl = BaseUrl;
            return page.Map(p => p.ToDto(baseUrl));
        }

        // GET api/products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var product = await _products.GetProductByIdAsync(id);
            if (product is null || !product.IsActive) return NotFound();

            return product.ToDto(BaseUrl);
        }
    }
}
