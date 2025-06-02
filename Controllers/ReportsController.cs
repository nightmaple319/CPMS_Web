using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPMS_Web.Services;
using CPMS_Web.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CPMS_Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> InventoryReport(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var report = await _reportService.GetInventoryReportAsync(startDate.Value, endDate.Value);
            return View(report);
        }

        public async Task<IActionResult> MaterialRequestReport(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var report = await _reportService.GetMaterialRequestReportAsync(startDate.Value, endDate.Value);
            return View(report);
        }

        public async Task<IActionResult> StockCountReport(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var report = await _reportService.GetStockCountReportAsync(startDate.Value, endDate.Value);
            return View(report);
        }

        public async Task<IActionResult> ExportInventoryReport(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var reportBytes = await _reportService.ExportInventoryReportAsync(startDate.Value, endDate.Value);
            return File(reportBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"InventoryReport_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportMaterialRequestReport(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var reportBytes = await _reportService.ExportMaterialRequestReportAsync(startDate.Value, endDate.Value);
            return File(reportBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"MaterialRequestReport_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportStockCountReport(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var reportBytes = await _reportService.ExportStockCountReportAsync(startDate.Value, endDate.Value);
            return File(reportBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"StockCountReport_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.xlsx");
        }
    }
}