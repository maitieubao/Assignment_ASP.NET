using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assignment_ASP.NET.Constants;
using Assignment_ASP.NET.Services;
using Assignment_ASP.NET.Models;

namespace Assignment_ASP.NET.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: /Dashboard/Index
        public async Task<IActionResult> Index()
        {
            var dashboardData = await _dashboardService.GetDashboardDataAsync();
            return View(dashboardData);
        }
    }
}
