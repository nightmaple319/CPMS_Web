using Microsoft.EntityFrameworkCore;
using CPMS_Web.Data;
using CPMS_Web.Models;
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
            var transactions = await _context.InventoryTransactions
                .Include(t => t.SparePart)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return transactions.Select(t => new InventoryReport
            {
                TransactionDate = t.TransactionDate,
                SparePartId = t.SparePartNo.ToString(),
                SparePartName = t.SparePart?.Description ?? "",
                TransactionType = t.TransactionType,
                Quantity = t.Quantity,
                UnitPrice = 0, // �p�G��������i�H�[�J
                TotalAmount = 0, // �p�G�����B���i�H�[�J
                Remarks = t.Reason
            });
        }

        public async Task<IEnumerable<MaterialRequestReport>> GetMaterialRequestReportAsync(DateTime startDate, DateTime endDate)
        {
            var requests = await _context.MaterialRequests
                .Include(r => r.Requester)
                .Include(r => r.MaterialRequestDetails)
                .Where(r => r.RequestDate >= startDate && r.RequestDate <= endDate)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return requests.Select(r => new MaterialRequestReport
            {
                RequestDate = r.RequestDate,
                RequestNumber = r.RequestNo,
                RequesterName = r.Requester?.UserName ?? "",
                Department = r.Department,
                Status = r.Status,
                TotalItems = r.MaterialRequestDetails.Count,
                TotalAmount = 0 // �p�G�����B���i�H�[�J
            });
        }

        public async Task<IEnumerable<StockCountReport>> GetStockCountReportAsync(DateTime startDate, DateTime endDate)
        {
            var stockCounts = await _context.StockCounts
                .Include(sc => sc.Counter)
                .Include(sc => sc.StockCountDetails)
                .Where(sc => sc.CountDate >= startDate && sc.CountDate <= endDate)
                .OrderByDescending(sc => sc.CountDate)
                .ToListAsync();

            return stockCounts.Select(sc => new StockCountReport
            {
                CountDate = sc.CountDate,
                CountNumber = sc.CountNo,
                CounterName = sc.Counter?.UserName ?? "",
                Status = sc.Status,
                TotalItems = sc.StockCountDetails.Count,
                SystemQuantity = sc.StockCountDetails.Sum(d => d.SystemQuantity),
                ActualQuantity = sc.StockCountDetails.Sum(d => d.CountedQuantity),
                Variance = sc.StockCountDetails.Sum(d => d.Difference)
            });
        }

        public async Task<byte[]> ExportInventoryReportAsync(DateTime startDate, DateTime endDate)
        {
            var data = await GetInventoryReportAsync(startDate, endDate);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("�w�s����");

            // �]�w���D
            worksheet.Cell(1, 1).Value = "������";
            worksheet.Cell(1, 2).Value = "�ƥ�s��";
            worksheet.Cell(1, 3).Value = "�ƥ�W��";
            worksheet.Cell(1, 4).Value = "�������";
            worksheet.Cell(1, 5).Value = "�ƶq";
            worksheet.Cell(1, 6).Value = "�Ƶ�";

            // �]�w���D�˦�
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // ��J���
            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.TransactionDate.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 2).Value = item.SparePartId;
                worksheet.Cell(row, 3).Value = item.SparePartName;
                worksheet.Cell(row, 4).Value = GetTransactionTypeText(item.TransactionType);
                worksheet.Cell(row, 5).Value = item.Quantity;
                worksheet.Cell(row, 6).Value = item.Remarks;
                row++;
            }

            // �۰ʽվ���e
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportMaterialRequestReportAsync(DateTime startDate, DateTime endDate)
        {
            var data = await GetMaterialRequestReportAsync(startDate, endDate);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("��Ƴ���");

            // �]�w���D
            worksheet.Cell(1, 1).Value = "�ӽФ��";
            worksheet.Cell(1, 2).Value = "�ӽг渹";
            worksheet.Cell(1, 3).Value = "�ӽФH";
            worksheet.Cell(1, 4).Value = "����";
            worksheet.Cell(1, 5).Value = "���A";
            worksheet.Cell(1, 6).Value = "���ؼƶq";

            // �]�w���D�˦�
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // ��J���
            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.RequestDate.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 2).Value = item.RequestNumber;
                worksheet.Cell(row, 3).Value = item.RequesterName;
                worksheet.Cell(row, 4).Value = item.Department;
                worksheet.Cell(row, 5).Value = GetStatusText(item.Status);
                worksheet.Cell(row, 6).Value = item.TotalItems;
                row++;
            }

            // �۰ʽվ���e
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportStockCountReportAsync(DateTime startDate, DateTime endDate)
        {
            var data = await GetStockCountReportAsync(startDate, endDate);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("�L�I����");

            // �]�w���D
            worksheet.Cell(1, 1).Value = "�L�I���";
            worksheet.Cell(1, 2).Value = "�L�I�渹";
            worksheet.Cell(1, 3).Value = "�L�I�H��";
            worksheet.Cell(1, 4).Value = "���A";
            worksheet.Cell(1, 5).Value = "���ؼƶq";
            worksheet.Cell(1, 6).Value = "�t�μƶq";
            worksheet.Cell(1, 7).Value = "��ڼƶq";
            worksheet.Cell(1, 8).Value = "�t��";

            // �]�w���D�˦�
            var headerRange = worksheet.Range(1, 1, 1, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // ��J���
            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.CountDate.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 2).Value = item.CountNumber;
                worksheet.Cell(row, 3).Value = item.CounterName;
                worksheet.Cell(row, 4).Value = GetCountStatusText(item.Status);
                worksheet.Cell(row, 5).Value = item.TotalItems;
                worksheet.Cell(row, 6).Value = item.SystemQuantity;
                worksheet.Cell(row, 7).Value = item.ActualQuantity;
                worksheet.Cell(row, 8).Value = item.Variance;
                row++;
            }

            // �۰ʽվ���e
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private string GetTransactionTypeText(string transactionType)
        {
            return transactionType switch
            {
                "IN" => "�J�w",
                "OUT" => "�X�w",
                "ADJUST" => "�վ�",
                _ => transactionType
            };
        }

        private string GetStatusText(string status)
        {
            return status switch
            {
                "PENDING" => "�ݼf��",
                "APPROVED" => "�w�֭�",
                "REJECTED" => "�w��^",
                "ISSUED" => "�w�X�w",
                _ => status
            };
        }

        private string GetCountStatusText(string status)
        {
            return status switch
            {
                "IN_PROGRESS" => "�i�椤",
                "COMPLETED" => "�w����",
                _ => status
            };
        }
    }
}