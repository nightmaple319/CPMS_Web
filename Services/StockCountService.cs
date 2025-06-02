using Microsoft.EntityFrameworkCore;
using CPMS_Web.Data;
using CPMS_Web.Models;
using CPMS_Web.Models.ViewModels;

namespace CPMS_Web.Services
{
    public class StockCountService : IStockCountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;

        public StockCountService(ApplicationDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<List<StockCount>> GetStockCountsAsync()
        {
            return await _context.StockCounts
                .Include(sc => sc.Counter)
                .Include(sc => sc.StockCountDetails)
                    .ThenInclude(scd => scd.SparePart)
                .OrderByDescending(sc => sc.CountDate)
                .ToListAsync();
        }

        public async Task<StockCount?> GetStockCountAsync(int id)
        {
            return await _context.StockCounts
                .Include(sc => sc.Counter)
                .Include(sc => sc.StockCountDetails)
                    .ThenInclude(scd => scd.SparePart)
                .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        public async Task<bool> CreateStockCountAsync(StockCount stockCount, List<int> sparePartNos)
        {
            try
            {
                stockCount.CountDate = DateTime.Now;
                stockCount.Status = "IN_PROGRESS";
                stockCount.CountNo = await GenerateCountNoAsync();
                stockCount.CreatedDate = DateTime.Now;

                _context.StockCounts.Add(stockCount);
                await _context.SaveChangesAsync();

                // 創建盤點明細
                foreach (var sparePartNo in sparePartNos)
                {
                    var sparePart = await _inventoryService.GetSparePartAsync(sparePartNo);
                    if (sparePart != null)
                    {
                        var countDetail = new StockCountDetail
                        {
                            CountId = stockCount.Id,
                            SparePartNo = sparePartNo,
                            SystemQuantity = sparePart.Quantity,
                            CountedQuantity = 0 // 初始為0，等待實際盤點
                        };
                        _context.StockCountDetails.Add(countDetail);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCountQuantityAsync(int countDetailId, int countedQuantity)
        {
            try
            {
                var countDetail = await _context.StockCountDetails.FindAsync(countDetailId);
                if (countDetail == null) return false;

                countDetail.CountedQuantity = countedQuantity;
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CompleteStockCountAsync(int stockCountId, string userId)
        {
            try
            {
                var stockCount = await _context.StockCounts
                    .Include(sc => sc.StockCountDetails)
                    .FirstOrDefaultAsync(sc => sc.Id == stockCountId);

                if (stockCount == null || stockCount.Status != "IN_PROGRESS") return false;

                // 根據盤點結果調整庫存
                foreach (var detail in stockCount.StockCountDetails)
                {
                    if (detail.Difference != 0)
                    {
                        await _inventoryService.AdjustQuantityAsync(
                            detail.SparePartNo,
                            detail.CountedQuantity,
                            $"盤點調整: {stockCount.CountNo}",
                            userId
                        );
                    }
                }

                stockCount.Status = "COMPLETED";
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<StockCount>> GetInProgressCountsAsync()
        {
            return await _context.StockCounts
                .Include(sc => sc.Counter)
                .Include(sc => sc.StockCountDetails)
                    .ThenInclude(scd => scd.SparePart)
                .Where(sc => sc.Status == "IN_PROGRESS")
                .OrderByDescending(sc => sc.CountDate)
                .ToListAsync();
        }

        public async Task<List<StockCount>> GetCompletedCountsAsync()
        {
            return await _context.StockCounts
                .Include(sc => sc.Counter)
                .Include(sc => sc.StockCountDetails)
                    .ThenInclude(scd => scd.SparePart)
                .Where(sc => sc.Status == "COMPLETED")
                .OrderByDescending(sc => sc.CountDate)
                .ToListAsync();
        }

        public async Task<StockCountReportViewModel> GetStockCountReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = new StockCountReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // 取得期間內的盤點記錄
            var stockCounts = await _context.StockCounts
                .Include(sc => sc.StockCountDetails)
                    .ThenInclude(scd => scd.SparePart)
                .Where(sc => sc.CountDate >= startDate && sc.CountDate <= endDate)
                .ToListAsync();

            report.TotalCounts = stockCounts.Count;
            report.CompletedCounts = stockCounts.Count(sc => sc.Status == "COMPLETED");
            report.InProgressCounts = stockCounts.Count(sc => sc.Status == "IN_PROGRESS");

            // 計算差異統計
            var allDetails = stockCounts.SelectMany(sc => sc.StockCountDetails).ToList();
            var differenceDetails = allDetails.Where(d => d.Difference != 0).ToList();

            report.TotalDifferenceItems = differenceDetails.Count;
            report.PositiveDifference = differenceDetails.Where(d => d.Difference > 0).Sum(d => d.Difference);
            report.NegativeDifference = Math.Abs(differenceDetails.Where(d => d.Difference < 0).Sum(d => d.Difference));

            // 各類別統計
            report.CategoryStatistics = allDetails
                .GroupBy(d => d.SparePart.Category)
                .Select(g => new CategoryCountStat
                {
                    Category = g.Key,
                    CountedItems = g.Count(),
                    DifferenceItems = g.Count(d => d.Difference != 0),
                    DifferencePercentage = g.Count() > 0 ? (decimal)g.Count(d => d.Difference != 0) / g.Count() * 100 : 0
                })
                .ToList();

            // 差異明細
            report.DifferenceDetails = differenceDetails
                .Select(d => new StockDifferenceDetail
                {
                    SparePartNo = d.SparePartNo,
                    Description = d.SparePart.Description,
                    Specification = d.SparePart.Specification,
                    SystemQuantity = d.SystemQuantity,
                    CountedQuantity = d.CountedQuantity,
                    Difference = d.Difference,
                    CountDate = d.StockCount.CountDate
                })
                .OrderByDescending(d => Math.Abs(d.Difference))
                .ToList();

            return report;
        }

        private async Task<string> GenerateCountNoAsync()
        {
            var today = DateTime.Now;
            var prefix = $"SC{today:yyyyMMdd}";

            var lastCount = await _context.StockCounts
                .Where(sc => sc.CountNo.StartsWith(prefix))
                .OrderByDescending(sc => sc.CountNo)
                .FirstOrDefaultAsync();

            if (lastCount == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = int.Parse(lastCount.CountNo.Substring(prefix.Length));
            return $"{prefix}{(lastNumber + 1):D3}";
        }
    }
}