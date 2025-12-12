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
        // GET: /Dashboard/Index
        public async Task<IActionResult> Index(string filter = "today", DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime? start = null;
            DateTime? end = null;
            var now = DateTime.Now;

            if (startDate.HasValue || endDate.HasValue)
            {
                start = startDate;
                end = endDate;
                filter = "custom";
            }
            else
            {
                switch (filter?.ToLower())
                {
                    case "today":
                        start = now.Date;
                        end = now.Date;
                        break;
                    case "week":
                        // Start of week (Monday)
                        var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                        start = now.Date.AddDays(-1 * diff);
                        end = start.Value.AddDays(6); // End of Sunday
                        break;
                    case "month":
                        start = new DateTime(now.Year, now.Month, 1);
                        end = start.Value.AddMonths(1).AddDays(-1);
                        break;
                    case "all":
                        break;
                    default: // Default to today if unknown
                         start = now.Date;
                        end = now.Date;
                        filter = "today";
                        break;
                }
            }

            var dashboardData = await _dashboardService.GetDashboardDataAsync(start, end);

            ViewBag.CurrentFilter = filter;
            if (startDate.HasValue) ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            if (endDate.HasValue) ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            return View(dashboardData);
        }
    }
}
