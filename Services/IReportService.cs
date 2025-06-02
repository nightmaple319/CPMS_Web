using CPMS_Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CPMS_Web.Services
{
    public interface IReportService
    {
        Task<IEnumerable<InventoryReport>> GetInventoryReportAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<MaterialRequestReport>> GetMaterialRequestReportAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<StockCountReport>> GetStockCountReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportInventoryReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportMaterialRequestReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportStockCountReportAsync(DateTime startDate, DateTime endDate);
    }
} 