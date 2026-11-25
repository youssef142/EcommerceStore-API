using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers
{
    /// <summary>
    /// Works for guests and signed-in users.
    ///  - signed in: send "Authorization: Bearer ..."  -> the user's cart
    ///  - guest:     send "X-Cart-Id: {guid}"           -> the guest cart (React generates the GUID once and stores it)
    /// </summary>
    [Route("api/cart")]
    public class CartController : ApiControllerBase
    {
        private readonly ICartService _cart;

        public CartController(ICartService cart) => _cart = cart;

        [HttpGet]
        public async Task<ActionResult<CartDto>> Get() => await LoadCartAsync();

        // Header badge
        [HttpGet("count")]
        public async Task<ActionResult<CartCountDto>> Count() =>
            new CartCountDto(await _cart.GetCartItemCountAsync());

        [HttpPost("items")]
        public async Task<ActionResult<CartDto>> AddItem(AddCartItemRequest request)
        {
            var ok = await _cart.AddToCartAsync(request.ProductId, request.Quantity);
            if (!ok)
                return Problem(title: "Product not found, inactive, or not enough stock.",
                               statusCode: StatusCodes.Status409Conflict);

            return await LoadCartAsync();
        }

        [HttpPut("items/{productId:int}")]
        public async Task<ActionResult<CartDto>> UpdateItem(int productId, UpdateCartItemRequest request)
        {
            var ok = await _cart.UpdateCartItemAsync(productId, request.Quantity);
            if (!ok)
                return Problem(title: "Unable to update cart (item missing or not enough stock).",
                               statusCode: StatusCodes.Status409Conflict);

            return await LoadCartAsync();
        }

        [HttpDelete("items/{productId:int}")]
        public async Task<ActionResult<CartDto>> RemoveItem(int productId)
        {
            var ok = await _cart.RemoveFromCartAsync(productId);
            if (!ok) return NotFound();

            return await LoadCartAsync();
        }

        [HttpDelete]
        public async Task<IActionResult> Clear()
        {
            await _cart.ClearCartAsync();
            return NoContent();
        }

        private async Task<CartDto> LoadCartAsync()
        {
            var cart = await _cart.GetUserCartWithItemsAsync();
            return cart.ToDto(BaseUrl);
        }
    }
}
