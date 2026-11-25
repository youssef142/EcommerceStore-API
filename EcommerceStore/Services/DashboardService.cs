using EcommerceStore.Dtos;
using EcommerceStore.Models;
using EcommerceStore.Repositories.Interfaces;
using EcommerceStore.Services.Interfaces;

namespace EcommerceStore.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var totalProducts = await _unitOfWork.Products.CountAsync();
            var activeProducts = await _unitOfWork.Products.CountAsync(p => p.IsActive);
            var totalCategories = await _unitOfWork.Categories.CountAsync(c => c.IsActive);
            var totalOrders = await _unitOfWork.Orders.CountAsync();
            var pendingOrders = await _unitOfWork.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var recent = await _unitOfWork.Orders.GetRecentOrdersAsync(10);

            return new DashboardDto(
                totalProducts, activeProducts, totalCategories, totalOrders, pendingOrders,
                recent.Select(o => o.ToSummaryDto()).ToList());
        }
    }
}
