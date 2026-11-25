using EcommerceStore.Dtos;

namespace EcommerceStore.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
    }
}
