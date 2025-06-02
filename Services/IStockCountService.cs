using CPMS_Web.Models;

namespace CPMS_Web.Services
{
    public interface IStockCountService
    {
        Task<List<StockCount>> GetStockCountsAsync();
        Task<StockCount?> GetStockCountAsync(int id);
        Task<bool> CreateStockCountAsync(StockCount stockCount, List<int> sparePartNos);
        Task<bool> UpdateCountQuantityAsync(int countDetailId, int countedQuantity);
        Task<bool> CompleteStockCountAsync(int stockCountId, string userId);
        Task<List<StockCount>> GetInProgressCountsAsync();
        Task<List<StockCount>> GetCompletedCountsAsync();
        Task<StockCountReportViewModel> GetStockCountReportAsync(DateTime startDate, DateTime endDate);
    }
} 