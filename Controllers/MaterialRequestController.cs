using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CPMS_Web.Models;
using CPMS_Web.Services;
using CPMS_Web.Models.ViewModels;

namespace CPMS_Web.Controllers
{
    [Authorize]
    public class MaterialRequestController : Controller
    {
        private readonly IMaterialRequestService _materialRequestService;
        private readonly IInventoryService _inventoryService;
        private readonly UserManager<IdentityUser> _userManager;

        public MaterialRequestController(
            IMaterialRequestService materialRequestService,
            IInventoryService inventoryService,
            UserManager<IdentityUser> userManager)
        {
            _materialRequestService = materialRequestService;
            _inventoryService = inventoryService;
            _userManager = userManager;
        }

        // GET: 領料單列表
        public async Task<IActionResult> Index(string status = "")
        {
            var userId = User.IsInRole("SuperAdmin") || User.IsInRole("Manager") ? null : _userManager.GetUserId(User);
            var requests = await _materialRequestService.GetRequestsAsync(status, userId);
            
            ViewBag.Status = status;
            return View(requests);
        }

        // GET: 領料單詳細資訊
        public async Task<IActionResult> Details(int id)
        {
            var request = await _materialRequestService.GetRequestAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            // 檢查權限：只能查看自己的申請單或管理員可以查看所有
            var currentUserId = _userManager.GetUserId(User);
            if (request.RequesterId != currentUserId && !User.IsInRole("SuperAdmin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            return View(request);
        }

        // GET: 建立領料單
        public IActionResult Create()
        {
            return View();
        }

        // POST: 建立領料單
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialRequest request, List<MaterialRequestDetail> details)
        {
            if (ModelState.IsValid && details.Any())
            {
                var currentUser = await _userManager.GetUserAsync(User);
                request.RequesterId = currentUser!.Id;
                request.Department = ""; // 可以從 UserProfile 取得

                var result = await _materialRequestService.CreateRequestAsync(request, details);
                
                if (result)
                {
                    TempData["Success"] = "領料單建立成功！";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Error"] = "領料單建立失敗！";
            }

            if (!details.Any())
            {
                TempData["Error"] = "請至少新增一項領料品項！";
            }

            return View(request);
        }

        // GET: 審核領料單
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _materialRequestService.GetRequestAsync(id);
            if (request == null || request.Status != "PENDING")
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: 核准領料單
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> ApproveRequest(int id, List<int> approvedQuantities)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _materialRequestService.ApproveRequestAsync(id, userId!, approvedQuantities);
            
            if (result)
            {
                TempData["Success"] = "領料單核准成功！";
            }
            else
            {
                TempData["Error"] = "領料單核准失敗！";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: 駁回領料單
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> RejectRequest(int id, string reason)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _materialRequestService.RejectRequestAsync(id, userId!, reason);
            
            if (result)
            {
                TempData["Success"] = "領料單已駁回！";
            }
            else
            {
                TempData["Error"] = "操作失敗！";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: 執行出庫
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Manager")]
        public async Task<IActionResult> IssueRequest(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _materialRequestService.IssueRequestAsync(id, userId!);
            
            if (result)
            {
                TempData["Success"] = "領料出庫完成！";
            }
            else
            {
                TempData["Error"] = "出庫失敗！可能是庫存不足。";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }

        // AJAX: 取得備品搜尋結果
        [HttpGet]
        public async Task<IActionResult> SearchSpareParts(string term)
        {
            var searchModel = new SparePartSearchViewModel
            {
                Description = term,
                PageSize = 10,
                ShowZeroStock = false
            };

            var result = await _inventoryService.SearchSparePartsAsync(searchModel);
            
            var data = result.Results.Select(s => new {
                id = s.No,
                text = $"{s.Description} ({s.Specification}) - 庫存: {s.Quantity}",
                quantity = s.Quantity
            });

            return Json(data);
        }
    }
} 