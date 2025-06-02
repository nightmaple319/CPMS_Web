using CPMS_Web.Models;
using CPMS_Web.Models.ViewModels;

namespace CPMS_Web.Services
{
    public interface IInventoryService
    {
        Task<SparePartSearchViewModel> SearchSparePartsAsync(SparePartSearchViewModel searchModel);
        Task<SparePart?> GetSparePartAsync(int no);
        Task<bool> CreateSparePartAsync(SparePart sparePart, string userId);
        Task<bool> UpdateSparePartAsync(SparePart sparePart, string userId);
        Task<bool> DeleteSparePartAsync(int no, string userId);
        Task<bool> AdjustQuantityAsync(int sparePartNo, int newQuantity, string reason, string userId);
        Task<List<InventoryTransaction>> GetTransactionHistoryAsync(int sparePartNo);
        Task<List<InventoryTransaction>> GetRecentTransactionsAsync(int count = 10);
        Task<List<InventoryTransaction>> GetAllTransactionHistoryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
} 