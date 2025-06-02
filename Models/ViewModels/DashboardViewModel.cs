using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        [Display(Name = "總備品數量")]
        public int TotalSparePartsCount { get; set; }

        [Display(Name = "總庫存數量")]
        public int TotalStockQuantity { get; set; }

        [Display(Name = "零庫存備品數")]
        public int ZeroStockCount { get; set; }

        [Display(Name = "低庫存備品數")]
        public int LowStockCount { get; set; }

        [Display(Name = "待審核領料單數")]
        public int PendingRequestsCount { get; set; }

        [Display(Name = "今日異動次數")]
        public int TodayTransactionsCount { get; set; }

        [Display(Name = "本月領料數量")]
        public int MonthlyMaterialOut { get; set; }

        [Display(Name = "最近異動記錄")]
        public List<InventoryTransaction> RecentTransactions { get; set; } = new List<InventoryTransaction>();

        [Display(Name = "庫存分類統計")]
        public Dictionary<string, int> CategoryStatistics { get; set; } = new Dictionary<string, int>();
    }
} 