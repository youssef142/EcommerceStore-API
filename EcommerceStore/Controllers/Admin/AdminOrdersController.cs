using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Models;
using EcommerceStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers.Admin
{
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ApiControllerBase
    {
        private readonly IOrderService _orders;

        public AdminOrdersController(IOrderService orders) => _orders = orders;

        // GET api/admin/orders?status=Pending
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetOrders([FromQuery] OrderStatus? status)
        {
            var orders = status.HasValue
                ? await _orders.GetOrdersByStatusAsync(status.Value)
                : await _orders.GetAllOrdersAsync();

            return orders.Select(o => o.ToSummaryDto()).ToList();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDetailDto>> GetById(int id)
        {
            var order = await _orders.GetOrderByIdAsync(id);
            if (order is null) return NotFound();

            return order.ToDetailDto(BaseUrl);
        }

        // PUT api/admin/orders/5/status   body: { "status": "Shipped" }
        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<OrderDetailDto>> UpdateStatus(int id, UpdateOrderStatusRequest request)
        {
            if (await _orders.GetOrderByIdAsync(id) is null) return NotFound();

            // Cancelling must go through CancelOrderAsync so product stock is restored.
            var status = request.Status!.Value;
            var ok = status == OrderStatus.Cancelled
                ? await _orders.CancelOrderAsync(id)
                : await _orders.UpdateOrderStatusAsync(id, status);

            if (!ok)
                return Problem(title: "Unable to change the order status (is the order already cancelled or shipped?).",
                               statusCode: StatusCodes.Status409Conflict);

            return (await _orders.GetOrderByIdAsync(id))!.ToDetailDto(BaseUrl);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult<OrderDetailDto>> Cancel(int id)
        {
            if (await _orders.GetOrderByIdAsync(id) is null) return NotFound();

            if (!await _orders.CancelOrderAsync(id))
                return Problem(title: "Unable to cancel order. Check the order status.",
                               statusCode: StatusCodes.Status409Conflict);

            return (await _orders.GetOrderByIdAsync(id))!.ToDetailDto(BaseUrl);
        }
    }
}
