using Assignment_ASP.NET.Models;

namespace Assignment_ASP.NET.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
