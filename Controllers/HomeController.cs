using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPMS_Web.Models;
using CPMS_Web.Services;
using System.Diagnostics;

namespace CPMS_Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public HomeController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = await _inventoryService.GetDashboardDataAsync();
            return View(dashboardData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 