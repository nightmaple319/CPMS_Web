using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CPMS_Web.Models;
using CPMS_Web.Models.ViewModels;
using CPMS_Web.Services;

namespace CPMS_Web.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly UserManager<IdentityUser> _userManager;

        public InventoryController(IInventoryService inventoryService, UserManager<IdentityUser> userManager)
        {
            _inventoryService = inventoryService;
            _userManager = userManager;
        }

        // GET: 庫存查詢
        public async Task<IActionResult> Index(SparePartSearchViewModel model)
        {
            var searchResult = await _inventoryService.SearchSparePartsAsync(model);
            return View(searchResult);
        }

        // GET: 庫存詳細資訊
        public async Task<IActionResult> Details(int id)
        {
            var sparePart = await _inventoryService.GetSparePartAsync(id);
            if (sparePart == null)
            {
                return NotFound();
            }

            ViewBag.TransactionHistory = await _inventoryService.GetTransactionHistoryAsync(id);
            return View(sparePart);
        }

        // GET: 新增庫存
        [Authorize(Roles = "SuperAdmin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: 新增庫存
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> Create(SparePart sparePart)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var result = await _inventoryService.CreateSparePartAsync(sparePart, userId!);
                
                if (result)
                {
                    TempData["Success"] = "備品新增成功！";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Error"] = "備品新增失敗！";
            }
            return View(sparePart);
        }

        // GET: 編輯庫存
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var sparePart = await _inventoryService.GetSparePartAsync(id);
            if (sparePart == null)
            {
                return NotFound();
            }
            return View(sparePart);
        }

        // POST: 編輯庫存
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> Edit(int id, SparePart sparePart)
        {
            if (id != sparePart.No)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var result = await _inventoryService.UpdateSparePartAsync(sparePart, userId!);
                
                if (result)
                {
                    TempData["Success"] = "備品更新成功！";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Error"] = "備品更新失敗！";
            }
            return View(sparePart);
        }

        // GET: 刪除庫存
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var sparePart = await _inventoryService.GetSparePartAsync(id);
            if (sparePart == null)
            {
                return NotFound();
            }
            return View(sparePart);
        }

        // POST: 刪除庫存
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _inventoryService.DeleteSparePartAsync(id, userId!);
            
            if (result)
            {
                TempData["Success"] = "備品刪除成功！";
            }
            else
            {
                TempData["Error"] = "備品刪除失敗！";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: 調整庫存數量
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> AdjustQuantity(int id)
        {
            var sparePart = await _inventoryService.GetSparePartAsync(id);
            if (sparePart == null)
            {
                return NotFound();
            }

            ViewBag.SparePart = sparePart;
            return View();
        }

        // POST: 調整庫存數量
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> AdjustQuantity(int id, int newQuantity, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "請輸入調整原因！";
                return RedirectToAction(nameof(AdjustQuantity), new { id });
            }

            var userId = _userManager.GetUserId(User);
            var result = await _inventoryService.AdjustQuantityAsync(id, newQuantity, reason, userId!);
            
            if (result)
            {
                TempData["Success"] = "庫存數量調整成功！";
            }
            else
            {
                TempData["Error"] = "庫存數量調整失敗！";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }

        // AJAX: 取得備品資訊
        [HttpGet]
        public async Task<IActionResult> GetSparePartInfo(int id)
        {
            var sparePart = await _inventoryService.GetSparePartAsync(id);
            if (sparePart == null)
            {
                return Json(new { success = false, message = "備品不存在" });
            }

            return Json(new { 
                success = true, 
                data = new {
                    no = sparePart.No,
                    description = sparePart.Description,
                    specification = sparePart.Specification,
                    quantity = sparePart.Quantity,
                    category = sparePart.Category
                }
            });
        }
    }
} 