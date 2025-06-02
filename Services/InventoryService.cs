using Microsoft.EntityFrameworkCore;
using CPMS_Web.Data;
using CPMS_Web.Models;
using CPMS_Web.Models.ViewModels;

namespace CPMS_Web.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SparePartSearchViewModel> SearchSparePartsAsync(SparePartSearchViewModel searchModel)
        {
            var query = _context.SpareParts.AsQueryable();

            // 套用搜尋條件
            if (!string.IsNullOrWhiteSpace(searchModel.PlantId))
                query = query.Where(s => s.PlantId.Contains(searchModel.PlantId));

            if (!string.IsNullOrWhiteSpace(searchModel.PositionId))
                query = query.Where(s => s.PositionId.Contains(searchModel.PositionId));

            if (!string.IsNullOrWhiteSpace(searchModel.Category))
                query = query.Where(s => s.Category.Contains(searchModel.Category));

            if (!string.IsNullOrWhiteSpace(searchModel.Description))
                query = query.Where(s => s.Description.Contains(searchModel.Description));

            if (!string.IsNullOrWhiteSpace(searchModel.Specification))
                query = query.Where(s => s.Specification.Contains(searchModel.Specification));

            if (!searchModel.ShowZeroStock)
                query = query.Where(s => s.Quantity > 0);

            // 計算總數
            searchModel.TotalCount = await query.CountAsync();

            // 分頁
            var results = await query
                .OrderBy(s => s.PlantId)
                .ThenBy(s => s.PositionId)
                .ThenBy(s => s.SubPositionId)
                .Skip((searchModel.PageIndex - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .ToListAsync();

            searchModel.Results = results;
            return searchModel;
        }

        public async Task<SparePart?> GetSparePartAsync(int no)
        {
            return await _context.SpareParts.FindAsync(no);
        }

        public async Task<bool> CreateSparePartAsync(SparePart sparePart, string userId)
        {
            try
            {
                sparePart.LastUpdated = DateTime.Now;
                _context.SpareParts.Add(sparePart);
                await _context.SaveChangesAsync();

                // 記錄庫存異動
                await CreateInventoryTransactionAsync(sparePart.No, "IN", sparePart.Quantity, 0, sparePart.Quantity, "新增備品", userId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSparePartAsync(SparePart sparePart, string userId)
        {
            try
            {
                var existingSparePart = await _context.SpareParts.FindAsync(sparePart.No);
                if (existingSparePart == null) return false;

                var oldQuantity = existingSparePart.Quantity;
                
                existingSparePart.PlantId = sparePart.PlantId;
                existingSparePart.PositionId = sparePart.PositionId;
                existingSparePart.SubPositionId = sparePart.SubPositionId;
                existingSparePart.Category = sparePart.Category;
                existingSparePart.Description = sparePart.Description;
                existingSparePart.Specification = sparePart.Specification;
                existingSparePart.Quantity = sparePart.Quantity;
                existingSparePart.Remarks = sparePart.Remarks;
                existingSparePart.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                // 如果數量有變化，記錄異動
                if (oldQuantity != sparePart.Quantity)
                {
                    await CreateInventoryTransactionAsync(sparePart.No, "ADJUST", 
                        sparePart.Quantity - oldQuantity, oldQuantity, sparePart.Quantity, "修改備品資料", userId);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSparePartAsync(int no, string userId)
        {
            try
            {
                var sparePart = await _context.SpareParts.FindAsync(no);
                if (sparePart == null) return false;

                // 記錄刪除前的異動
                await CreateInventoryTransactionAsync(no, "OUT", -sparePart.Quantity, sparePart.Quantity, 0, "刪除備品", userId);

                _context.SpareParts.Remove(sparePart);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AdjustQuantityAsync(int sparePartNo, int newQuantity, string reason, string userId)
        {
            try
            {
                var sparePart = await _context.SpareParts.FindAsync(sparePartNo);
                if (sparePart == null) return false;

                var oldQuantity = sparePart.Quantity;
                var adjustQuantity = newQuantity - oldQuantity;

                sparePart.Quantity = newQuantity;
                sparePart.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                // 記錄異動
                await CreateInventoryTransactionAsync(sparePartNo, "ADJUST", adjustQuantity, oldQuantity, newQuantity, reason, userId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<InventoryTransaction>> GetTransactionHistoryAsync(int sparePartNo)
        {
            return await _context.InventoryTransactions
                .Include(t => t.User)
                .Include(t => t.SparePart)
                .Where(t => t.SparePartNo == sparePartNo)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<InventoryTransaction>> GetRecentTransactionsAsync(int count = 10)
        {
            return await _context.InventoryTransactions
                .Include(t => t.User)
                .Include(t => t.SparePart)
                .OrderByDescending(t => t.TransactionDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var dashboard = new DashboardViewModel();

            // 基本統計
            dashboard.TotalSparePartsCount = await _context.SpareParts.CountAsync();
            dashboard.TotalStockQuantity = await _context.SpareParts.SumAsync(s => s.Quantity);
            dashboard.ZeroStockCount = await _context.SpareParts.CountAsync(s => s.Quantity == 0);
            dashboard.LowStockCount = await _context.SpareParts.CountAsync(s => s.Quantity > 0 && s.Quantity <= 5); // 假設5以下為低庫存

            // 待審核領料單
            dashboard.PendingRequestsCount = await _context.MaterialRequests.CountAsync(r => r.Status == "PENDING");

            // 今日異動次數
            var today = DateTime.Today;
            dashboard.TodayTransactionsCount = await _context.InventoryTransactions
                .CountAsync(t => t.TransactionDate.Date == today);

            // 本月領料數量
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dashboard.MonthlyMaterialOut = await _context.InventoryTransactions
                .Where(t => t.TransactionDate >= startOfMonth && t.TransactionType == "OUT")
                .SumAsync(t => Math.Abs(t.Quantity));

            // 最近異動記錄
            dashboard.RecentTransactions = await GetRecentTransactionsAsync(5);

            // 分類統計
            dashboard.CategoryStatistics = await _context.SpareParts
                .GroupBy(s => s.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            return dashboard;
        }

        private async Task CreateInventoryTransactionAsync(int sparePartNo, string transactionType, int quantity, 
            int previousQuantity, int newQuantity, string reason, string userId, string referenceNo = "")
        {
            var transaction = new InventoryTransaction
            {
                SparePartNo = sparePartNo,
                TransactionType = transactionType,
                Quantity = quantity,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
                TransactionDate = DateTime.Now,
                UserId = userId,
                Reason = reason,
                ReferenceNo = referenceNo
            };

            _context.InventoryTransactions.Add(transaction);
        }
    }
} 