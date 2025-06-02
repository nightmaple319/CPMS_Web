using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CPMS_Web.Models;
using CPMS_Web.Services;

namespace CPMS_Web.Controllers
{
    [Authorize]
    public class StockCountController : Controller
    {
        private readonly IStockCountService _stockCountService;
        private readonly IInventoryService _inventoryService;
        private readonly UserManager<IdentityUser> _userManager;

        public StockCountController(
            IStockCountService stockCountService,
            IInventoryService inventoryService,
            UserManager<IdentityUser> userManager)
        {
            _stockCountService = stockCountService;
            _inventoryService = inventoryService;
            _userManager = userManager;
        }

        // GET: 盤點列表
        public async Task<IActionResult> Index()
        {
            var stockCounts = await _stockCountService.GetStockCountsAsync();
            return View(stockCounts);
        }

        // GET: 盤點詳細資訊
        public async Task<IActionResult> Details(int id)
        {
            var stockCount = await _stockCountService.GetStockCountAsync(id);
            if (stockCount == null)
            {
                return NotFound();
            }

            return View(stockCount);
        }

        // GET: 建立盤點
        [Authorize(Roles = "SuperAdmin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: 建立盤點
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> Create(StockCount stockCount, List<int> sparePartNos)
        {
            if (ModelState.IsValid && sparePartNos.Any())
            {
                var currentUser = await _userManager.GetUserAsync(User);
                stockCount.CounterId = currentUser!.Id;

                var result = await _stockCountService.CreateStockCountAsync(stockCount, sparePartNos);
                
                if (result)
                {
                    TempData["Success"] = "盤點單建立成功！";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Error"] = "盤點單建立失敗！";
            }

            if (!sparePartNos.Any())
            {
                TempData["Error"] = "請選擇要盤點的備品！";
            }

            return View(stockCount);
        }

        // GET: 執行盤點
        public async Task<IActionResult> Count(int id)
        {
            var stockCount = await _stockCountService.GetStockCountAsync(id);
            if (stockCount == null || stockCount.Status != "IN_PROGRESS")
            {
                return NotFound();
            }

            return View(stockCount);
        }

        // POST: 更新盤點數量
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCountQuantity(int countDetailId, int countedQuantity)
        {
            var result = await _stockCountService.UpdateCountQuantityAsync(countDetailId, countedQuantity);
            
            return Json(new { success = result });
        }

        // POST: 完成盤點
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> CompleteCount(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _stockCountService.CompleteStockCountAsync(id, userId!);
            
            if (result)
            {
                TempData["Success"] = "盤點完成！庫存已自動調整。";
            }
            else
            {
                TempData["Error"] = "盤點完成失敗！";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }
    }
} 