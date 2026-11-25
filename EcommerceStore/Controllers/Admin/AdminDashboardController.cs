using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers.Admin
{
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ApiControllerBase
    {
        private readonly IDashboardService _dashboard;

        public AdminDashboardController(IDashboardService dashboard) => _dashboard = dashboard;

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> Get() => await _dashboard.GetDashboardAsync();
    }
}
