using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;
using EcommerceStore.Services.Interfaces;
using EcommerceStore.Dtos;

namespace EcommerceStore.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            ICartService cartService,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<Order?> CreateOrderAsync(string userId, CheckoutRequest checkoutModel)
        {
            try
            {
                // Start transaction
                await _unitOfWork.BeginTransactionAsync();

                // Get user's cart with items
                var cart = await _unitOfWork.Carts.GetCartWithItemsAsync(userId, null);
                if (cart == null || !cart.CartItems.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync(); // don't leave the transaction open
                    _logger.LogWarning("Cannot create order: Empty cart for user {UserId}", userId);
                    return null;
                }

                // Validate stock availability for all items
                foreach (var cartItem in cart.CartItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                    if (product == null || !product.IsActive || product.StockQuantity < cartItem.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogWarning("Order creation failed: Insufficient stock for product {ProductId}", 
                            cartItem.ProductId);
                        return null;
                    }
                }

                // Generate order number
                var orderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync();

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = orderNumber,
                    TotalAmount = cart.TotalAmount,
                    Status = OrderStatus.Pending,
                    ShippingAddress = checkoutModel.ShippingAddress,
                    ShippingCity = checkoutModel.City,
                    ShippingPostalCode = checkoutModel.PostalCode,
                    ShippingCountry = checkoutModel.Country,
                    Email = checkoutModel.Email,
                    PhoneNumber = checkoutModel.PhoneNumber,
                    Notes = checkoutModel.Notes,
                    OrderDate = DateTime.UtcNow
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Create order items and update product stock
                foreach (var cartItem in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        TotalPrice = cartItem.TotalPrice,
                        ProductName = cartItem.Product.Name
                    };

                    order.OrderItems.Add(orderItem);

                    // Reduce product stock
                    var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= cartItem.Quantity;
                        _unitOfWork.Products.Update(product);
                    }
                }

                // Clear the cart
                await _unitOfWork.Carts.ClearCartAsync(cart.CartId);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Order created successfully: {OrderNumber} for user {UserId}", 
                    orderNumber, userId);

                return order;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            try
            {
                return await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            try
            {
                return await _unitOfWork.Orders.GetOrderByOrderNumberAsync(orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order by number {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            try
            {
                return await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            try
            {
                return await _unitOfWork.Orders.GetRecentOrdersAsync(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            try
            {
                return await _unitOfWork.Orders.GetOrdersByStatusAsync(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders with status {Status}", status);
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    return false;
                }

                // A cancelled order has already had its stock restored; re-opening it would corrupt stock.
                if (order.Status == OrderStatus.Cancelled)
                {
                    _logger.LogWarning("Order {OrderId} is cancelled and cannot change status", orderId);
                    return false;
                }

                order.Status = newStatus;

                // Update timestamps based on status
                if (newStatus == OrderStatus.Shipped)
                {
                    order.ShippedDate = DateTime.UtcNow;
                }
                else if (newStatus == OrderStatus.Delivered)
                {
                    order.DeliveredDate = DateTime.UtcNow;
                }

                _unitOfWork.Orders.Update(order);
                var result = await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, newStatus);

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status", orderId);
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Only allow cancellation if order is pending or processing
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("Cannot cancel order {OrderId} with status {Status}", orderId, order.Status);
                    return false;
                }

                // Restore product stock
                foreach (var item in order.OrderItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _unitOfWork.Products.Update(product);
                    }
                }

                order.Status = OrderStatus.Cancelled;
                _unitOfWork.Orders.Update(order);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                throw;
            }
        }
    }
}
