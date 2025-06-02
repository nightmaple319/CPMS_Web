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

        // GET: ������
        public IActionResult Index()
        {
            return View();
        }

        // GET: �w�s����
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

        // GET: ��Ƴ���
        public IActionResult MaterialRequestReport()
        {
            ViewBag.StartDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        // POST: �ͦ���Ƴ���
        [HttpPost]
        public async Task<IActionResult> GenerateMaterialRequestReport(DateTime startDate, DateTime endDate)
        {
            var report = await _materialRequestService.GetRequestReportAsync(startDate, endDate);
            return View("MaterialRequestReportResult", report);
        }

        // GET: �L�I����
        public IActionResult StockCountReport()
        {
            ViewBag.StartDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        // POST: �ͦ��L�I����
        [HttpPost]
        public async Task<IActionResult> GenerateStockCountReport(DateTime startDate, DateTime endDate)
        {
            var report = await _stockCountService.GetStockCountReportAsync(startDate, endDate);
            return View("StockCountReportResult", report);
        }

        // GET: �ץX�w�sExcel
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
                var worksheet = package.Workbook.Worksheets.Add("�w�s����");

                // �]�w���D
                worksheet.Cells[1, 1].Value = "�ƫ~�s��";
                worksheet.Cells[1, 2].Value = "�t�ϥN�X";
                worksheet.Cells[1, 3].Value = "��m�N�X";
                worksheet.Cells[1, 4].Value = "�l��m�N�X";
                worksheet.Cells[1, 5].Value = "����";
                worksheet.Cells[1, 6].Value = "�y�z";
                worksheet.Cells[1, 7].Value = "�W��";
                worksheet.Cells[1, 8].Value = "�ƶq";
                worksheet.Cells[1, 9].Value = "�̫��s";
                worksheet.Cells[1, 10].Value = "�Ƶ�";

                // ��J���
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

                // �۰ʽվ���e
                worksheet.Cells.AutoFitColumns();

                // �]�w���D�C�˦�
                using (var range = worksheet.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                var fileName = $"�w�s����_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(package.GetAsByteArray(), contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"�ץX���ѡG{ex.Message}";
                return RedirectToAction(nameof(InventoryReport));
            }
        }

        // GET: �ץX��Ƴ���Excel
        public async Task<IActionResult> ExportMaterialRequestExcel(DateTime startDate, DateTime endDate)
        {
            try
            {
                var report = await _materialRequestService.GetRequestReportAsync(startDate, endDate);

                using var package = new ExcelPackage();

                // �إߺK�n�u�@��
                var summarySheet = package.Workbook.Worksheets.Add("�K�n");
                summarySheet.Cells[1, 1].Value = "��Ƴ���K�n";
                summarySheet.Cells[2, 1].Value = "�έp�����G";
                summarySheet.Cells[2, 2].Value = $"{startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}";
                summarySheet.Cells[4, 1].Value = "�`��Ƴ�ơG";
                summarySheet.Cells[4, 2].Value = report.TotalRequests;
                summarySheet.Cells[5, 1].Value = "�w�֭�ơG";
                summarySheet.Cells[5, 2].Value = report.ApprovedRequests;
                summarySheet.Cells[6, 1].Value = "�w��^�ơG";
                summarySheet.Cells[6, 2].Value = report.RejectedRequests;
                summarySheet.Cells[7, 1].Value = "�w�X�w�ơG";
                summarySheet.Cells[7, 2].Value = report.IssuedRequests;

                // �إ߳����έp�u�@��
                var deptSheet = package.Workbook.Worksheets.Add("�����έp");
                deptSheet.Cells[1, 1].Value = "����";
                deptSheet.Cells[1, 2].Value = "�ӽг��";
                deptSheet.Cells[1, 3].Value = "�֭��";
                deptSheet.Cells[1, 4].Value = "��^��";
                deptSheet.Cells[1, 5].Value = "�X�w��";

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

                // �إ����O�έp�u�@��
                var categorySheet = package.Workbook.Worksheets.Add("���O�έp");
                categorySheet.Cells[1, 1].Value = "���O";
                categorySheet.Cells[1, 2].Value = "�ӽмƶq";
                categorySheet.Cells[1, 3].Value = "�֭�ƶq";
                categorySheet.Cells[1, 4].Value = "�X�w�ƶq";

                row = 2;
                foreach (var stat in report.CategoryStatistics)
                {
                    categorySheet.Cells[row, 1].Value = stat.Category;
                    categorySheet.Cells[row, 2].Value = stat.RequestedQuantity;
                    categorySheet.Cells[row, 3].Value = stat.ApprovedQuantity;
                    categorySheet.Cells[row, 4].Value = stat.IssuedQuantity;
                    row++;
                }

                // �۰ʽվ���e
                summarySheet.Cells.AutoFitColumns();
                deptSheet.Cells.AutoFitColumns();
                categorySheet.Cells.AutoFitColumns();

                var fileName = $"��Ƴ���_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(package.GetAsByteArray(), contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"�ץX���ѡG{ex.Message}";
                return RedirectToAction(nameof(MaterialRequestReport));
            }
        }

        // GET: ���ʰO������
        public async Task<IActionResult> TransactionReport(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            var transactions = await _inventoryService.GetTransactionHistoryAsync(0); // ���o�Ҧ����ʰO��
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