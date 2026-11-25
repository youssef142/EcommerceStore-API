using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Models;
using EcommerceStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers
{
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ApiControllerBase
    {
        private readonly IOrderService _orders;

        public OrdersController(IOrderService orders) => _orders = orders;

        // GET api/orders  (my orders)
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetMine()
        {
            var userId = CurrentUserId;
            if (userId is null) return Unauthorized();

            var orders = await _orders.GetUserOrdersAsync(userId);
            return orders.Select(o => o.ToSummaryDto()).ToList();
        }

        // GET api/orders/5  (also used for the order-confirmation screen)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDetailDto>> GetById(int id)
        {
            var order = await FindAccessibleOrderAsync(id);
            if (order is null) return NotFound();

            return order.ToDetailDto(BaseUrl);
        }

        // POST api/orders  (checkout - the server uses the signed-in user's cart)
        [HttpPost]
        public async Task<ActionResult<OrderDetailDto>> Checkout(CheckoutRequest request)
        {
            var userId = CurrentUserId;
            if (userId is null) return Unauthorized();

            var order = await _orders.CreateOrderAsync(userId, request);
            if (order is null)
                return Problem(title: "Unable to create order. Your cart is empty or some items are no longer available.",
                               statusCode: StatusCodes.Status409Conflict);

            // reload so product images etc. are populated
            var created = await _orders.GetOrderByIdAsync(order.OrderId) ?? order;
            return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created.ToDetailDto(BaseUrl));
        }

        // POST api/orders/5/cancel
        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult<OrderDetailDto>> Cancel(int id)
        {
            var order = await FindAccessibleOrderAsync(id);
            if (order is null) return NotFound();

            var ok = await _orders.CancelOrderAsync(id);
            if (!ok)
                return Problem(title: "Unable to cancel order. It may already be shipped.",
                               statusCode: StatusCodes.Status409Conflict);

            var updated = await _orders.GetOrderByIdAsync(id);
            return updated!.ToDetailDto(BaseUrl);
        }

        /// <summary>
        /// Ownership check. Returns null both when the order doesn't exist and when it belongs to someone
        /// else, so callers answer 404 either way and users can't probe other people's order ids.
        /// Admins can access any order.
        /// </summary>
        private async Task<Order?> FindAccessibleOrderAsync(int id)
        {
            var order = await _orders.GetOrderByIdAsync(id);
            if (order is null) return null;

            return order.UserId == CurrentUserId || User.IsInRole("Admin") ? order : null;
        }
    }
}
