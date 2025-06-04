using CPMS_Web.Data;
using CPMS_Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;

namespace CPMS_Web.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InventoryReport>> GetInventoryReportAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.InventoryTransactions
                .Include(t => t.SparePart)
                .Include(t => t.User)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .Select(t => new InventoryReport
                {
                    TransactionDate = t.TransactionDate,
                    SparePartId = t.SparePartNo.ToString(),
                    SparePartName = t.SparePart.Description,
                    TransactionType = t.TransactionType,
                    Quantity = t.Quantity,
                    UnitPrice = 0, // 暫時設為0，因為模型中沒有單價欄位
                    TotalAmount = 0, // 暫時設為0
                    Remarks = t.Remarks
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MaterialRequestReport>> GetMaterialRequestReportAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.MaterialRequests
                .Include(mr => mr.Requester)
                .Include(mr => mr.MaterialRequestDetails)
                .Where(mr => mr.RequestDate >= startDate && mr.RequestDate <= endDate)
                .Select(mr => new MaterialRequestReport
                {
                    RequestDate = mr.RequestDate,
                    RequestNumber = mr.RequestNo,
                    RequesterName = mr.Requester.UserName ?? "",
                    Department = mr.Department,
                    Status = mr.Status,
                    TotalItems = mr.MaterialRequestDetails.Count,
                    TotalAmount = 0 // 暫時設為0，因為沒有價格資訊
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<StockCountReport>> GetStockCountReportAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.StockCounts
                .Include(sc => sc.Counter)
                .Include(sc => sc.StockCountDetails)
                .Where(sc => sc.CountDate >= startDate && sc.CountDate <= endDate)
                .Select(sc => new StockCountReport
                {
                    CountDate = sc.CountDate,
                    CountNumber = sc.CountNo,
                    CounterName = sc.Counter.UserName ?? "",
                    Status = sc.Status,
                    TotalItems = sc.StockCountDetails.Count,
                    SystemQuantity = sc.StockCountDetails.Sum(item => item.SystemQuantity),
                    ActualQuantity = sc.StockCountDetails.Sum(item => item.CountedQuantity),
                    Variance = sc.StockCountDetails.Sum(item => item.CountedQuantity - item.SystemQuantity)
                })
                .ToListAsync();
        }

        public async Task<byte[]> ExportInventoryReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = await GetInventoryReportAsync(startDate, endDate);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventory Report");

                // Add headers
                worksheet.Cell(1, 1).Value = "Transaction Date";
                worksheet.Cell(1, 2).Value = "Spare Part ID";
                worksheet.Cell(1, 3).Value = "Spare Part Name";
                worksheet.Cell(1, 4).Value = "Transaction Type";
                worksheet.Cell(1, 5).Value = "Quantity";
                worksheet.Cell(1, 6).Value = "Remarks";

                // Add data
                int row = 2;
                foreach (var item in report)
                {
                    worksheet.Cell(row, 1).Value = item.TransactionDate;
                    worksheet.Cell(row, 2).Value = item.SparePartId;
                    worksheet.Cell(row, 3).Value = item.SparePartName;
                    worksheet.Cell(row, 4).Value = item.TransactionType;
                    worksheet.Cell(row, 5).Value = item.Quantity;
                    worksheet.Cell(row, 6).Value = item.Remarks;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> ExportMaterialRequestReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = await GetMaterialRequestReportAsync(startDate, endDate);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Material Request Report");

                // Add headers
                worksheet.Cell(1, 1).Value = "Request Date";
                worksheet.Cell(1, 2).Value = "Request Number";
                worksheet.Cell(1, 3).Value = "Requester Name";
                worksheet.Cell(1, 4).Value = "Department";
                worksheet.Cell(1, 5).Value = "Status";
                worksheet.Cell(1, 6).Value = "Total Items";

                // Add data
                int row = 2;
                foreach (var item in report)
                {
                    worksheet.Cell(row, 1).Value = item.RequestDate;
                    worksheet.Cell(row, 2).Value = item.RequestNumber;
                    worksheet.Cell(row, 3).Value = item.RequesterName;
                    worksheet.Cell(row, 4).Value = item.Department;
                    worksheet.Cell(row, 5).Value = item.Status;
                    worksheet.Cell(row, 6).Value = item.TotalItems;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> ExportStockCountReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = await GetStockCountReportAsync(startDate, endDate);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Stock Count Report");

                // Add headers
                worksheet.Cell(1, 1).Value = "Count Date";
                worksheet.Cell(1, 2).Value = "Count Number";
                worksheet.Cell(1, 3).Value = "Counter Name";
                worksheet.Cell(1, 4).Value = "Status";
                worksheet.Cell(1, 5).Value = "Total Items";
                worksheet.Cell(1, 6).Value = "System Quantity";
                worksheet.Cell(1, 7).Value = "Actual Quantity";
                worksheet.Cell(1, 8).Value = "Variance";

                // Add data
                int row = 2;
                foreach (var item in report)
                {
                    worksheet.Cell(row, 1).Value = item.CountDate;
                    worksheet.Cell(row, 2).Value = item.CountNumber;
                    worksheet.Cell(row, 3).Value = item.CounterName;
                    worksheet.Cell(row, 4).Value = item.Status;
                    worksheet.Cell(row, 5).Value = item.TotalItems;
                    worksheet.Cell(row, 6).Value = item.SystemQuantity;
                    worksheet.Cell(row, 7).Value = item.ActualQuantity;
                    worksheet.Cell(row, 8).Value = item.Variance;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
