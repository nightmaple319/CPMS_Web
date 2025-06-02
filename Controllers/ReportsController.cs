using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPMS_Web.Services;
using OfficeOpenXml;
using System.Text;
using CPMS_Web.Models.ViewModels;

namespace CPMS_Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Manager")]
    public class ReportsController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IMaterialRequestService _materialRequestService;
        private readonly IStockCountService _stockCountService;

        public ReportsController(
            IInventoryService inventoryService,
            IMaterialRequestService materialRequestService,
            IStockCountService stockCountService)
        {
            _inventoryService = inventoryService;
            _materialRequestService = materialRequestService;
            _stockCountService = stockCountService;
        }

        // GET: 報表首頁
        public IActionResult Index()
        {
            return View();
        }

        // GET: 庫存報表
        public async Task<IActionResult> InventoryReport()
        {
            var searchModel = new SparePartSearchViewModel
            {
                PageSize = int.MaxValue,
                ShowZeroStock = true
            };

            var result = await _inventoryService.SearchSparePartsAsync(searchModel);
            return View(result);
        }

        // GET: 領料報表
        public IActionResult MaterialRequestReport()
        {
            ViewBag.StartDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        // POST: 生成領料報表
        [HttpPost]
        public async Task<IActionResult> GenerateMaterialRequestReport(DateTime startDate, DateTime endDate)
        {
            var report = await _materialRequestService.GetRequestReportAsync(startDate, endDate);
            return View("MaterialRequestReportResult", report);
        }

        // GET: 盤點報表
        public IActionResult StockCountReport()
        {
            ViewBag.StartDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        // POST: 生成盤點報表
        [HttpPost]
        public async Task<IActionResult> GenerateStockCountReport(DateTime startDate, DateTime endDate)
        {
            var report = await _stockCountService.GetStockCountReportAsync(startDate, endDate);
            return View("StockCountReportResult", report);
        }

        // GET: 匯出庫存Excel
        public async Task<IActionResult> ExportInventoryExcel()
        {
            try
            {
                var searchModel = new SparePartSearchViewModel
                {
                    PageSize = int.MaxValue,
                    ShowZeroStock = true
                };

                var result = await _inventoryService.SearchSparePartsAsync(searchModel);

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("庫存報表");

                // 設定標題
                worksheet.Cells[1, 1].Value = "備品編號";
                worksheet.Cells[1, 2].Value = "廠區代碼";
                worksheet.Cells[1, 3].Value = "位置代碼";
                worksheet.Cells[1, 4].Value = "子位置代碼";
                worksheet.Cells[1, 5].Value = "分類";
                worksheet.Cells[1, 6].Value = "描述";
                worksheet.Cells[1, 7].Value = "規格";
                worksheet.Cells[1, 8].Value = "數量";
                worksheet.Cells[1, 9].Value = "最後更新";
                worksheet.Cells[1, 10].Value = "備註";

                // 填入資料
                int row = 2;
                foreach (var item in result.Results)
                {
                    worksheet.Cells[row, 1].Value = item.No;
                    worksheet.Cells[row, 2].Value = item.PlantId;
                    worksheet.Cells[row, 3].Value = item.PositionId;
                    worksheet.Cells[row, 4].Value = item.SubPositionId;
                    worksheet.Cells[row, 5].Value = item.Category;
                    worksheet.Cells[row, 6].Value = item.Description;
                    worksheet.Cells[row, 7].Value = item.Specification;
                    worksheet.Cells[row, 8].Value = item.Quantity;
                    worksheet.Cells[row, 9].Value = item.LastUpdated.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 10].Value = item.Remarks;
                    row++;
                }

                // 自動調整欄寬
                worksheet.Cells.AutoFitColumns();

                // 設定標題列樣式
                using (var range = worksheet.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                var fileName = $"庫存報表_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(package.GetAsByteArray(), contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"匯出失敗：{ex.Message}";
                return RedirectToAction(nameof(InventoryReport));
            }
        }

        // GET: 匯出領料報表Excel
        public async Task<IActionResult> ExportMaterialRequestExcel(DateTime startDate, DateTime endDate)
        {
            try
            {
                var report = await _materialRequestService.GetRequestReportAsync(startDate, endDate);

                using var package = new ExcelPackage();

                // 建立摘要工作表
                var summarySheet = package.Workbook.Worksheets.Add("摘要");
                summarySheet.Cells[1, 1].Value = "領料報表摘要";
                summarySheet.Cells[2, 1].Value = "統計期間：";
                summarySheet.Cells[2, 2].Value = $"{startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}";
                summarySheet.Cells[4, 1].Value = "總領料單數：";
                summarySheet.Cells[4, 2].Value = report.TotalRequests;
                summarySheet.Cells[5, 1].Value = "已核准數：";
                summarySheet.Cells[5, 2].Value = report.ApprovedRequests;
                summarySheet.Cells[6, 1].Value = "已駁回數：";
                summarySheet.Cells[6, 2].Value = report.RejectedRequests;
                summarySheet.Cells[7, 1].Value = "已出庫數：";
                summarySheet.Cells[7, 2].Value = report.IssuedRequests;

                // 建立部門統計工作表
                var deptSheet = package.Workbook.Worksheets.Add("部門統計");
                deptSheet.Cells[1, 1].Value = "部門";
                deptSheet.Cells[1, 2].Value = "申請單數";
                deptSheet.Cells[1, 3].Value = "核准數";
                deptSheet.Cells[1, 4].Value = "駁回數";
                deptSheet.Cells[1, 5].Value = "出庫數";

                int row = 2;
                foreach (var stat in report.DepartmentStatistics)
                {
                    deptSheet.Cells[row, 1].Value = stat.Department;
                    deptSheet.Cells[row, 2].Value = stat.RequestCount;
                    deptSheet.Cells[row, 3].Value = stat.ApprovedCount;
                    deptSheet.Cells[row, 4].Value = stat.RejectedCount;
                    deptSheet.Cells[row, 5].Value = stat.IssuedCount;
                    row++;
                }

                // 建立類別統計工作表
                var categorySheet = package.Workbook.Worksheets.Add("類別統計");
                categorySheet.Cells[1, 1].Value = "類別";
                categorySheet.Cells[1, 2].Value = "申請數量";
                categorySheet.Cells[1, 3].Value = "核准數量";
                categorySheet.Cells[1, 4].Value = "出庫數量";

                row = 2;
                foreach (var stat in report.CategoryStatistics)
                {
                    categorySheet.Cells[row, 1].Value = stat.Category;
                    categorySheet.Cells[row, 2].Value = stat.RequestedQuantity;
                    categorySheet.Cells[row, 3].Value = stat.ApprovedQuantity;
                    categorySheet.Cells[row, 4].Value = stat.IssuedQuantity;
                    row++;
                }

                // 自動調整欄寬
                summarySheet.Cells.AutoFitColumns();
                deptSheet.Cells.AutoFitColumns();
                categorySheet.Cells.AutoFitColumns();

                var fileName = $"領料報表_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(package.GetAsByteArray(), contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"匯出失敗：{ex.Message}";
                return RedirectToAction(nameof(MaterialRequestReport));
            }
        }

        // GET: 異動記錄報表
        public async Task<IActionResult> TransactionReport(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            var transactions = await _inventoryService.GetTransactionHistoryAsync(0); // 取得所有異動記錄
            var filteredTransactions = transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(filteredTransactions);
        }
    }
}