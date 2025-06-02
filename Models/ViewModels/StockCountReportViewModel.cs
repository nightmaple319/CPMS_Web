using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models.ViewModels
{
    public class StockCountReportViewModel
    {
        [Display(Name = "開始日期")]
        public DateTime StartDate { get; set; }

        [Display(Name = "結束日期")]
        public DateTime EndDate { get; set; }

        [Display(Name = "總盤點次數")]
        public int TotalCounts { get; set; }

        [Display(Name = "已完成盤點")]
        public int CompletedCounts { get; set; }

        [Display(Name = "進行中盤點")]
        public int InProgressCounts { get; set; }

        [Display(Name = "總差異項目")]
        public int TotalDifferenceItems { get; set; }

        [Display(Name = "正差異數量")]
        public int PositiveDifference { get; set; }

        [Display(Name = "負差異數量")]
        public int NegativeDifference { get; set; }

        [Display(Name = "各類別盤點統計")]
        public List<CategoryCountStat> CategoryStatistics { get; set; } = new List<CategoryCountStat>();

        [Display(Name = "差異明細")]
        public List<StockDifferenceDetail> DifferenceDetails { get; set; } = new List<StockDifferenceDetail>();
    }

    public class CategoryCountStat
    {
        [Display(Name = "類別")]
        public string Category { get; set; } = "";

        [Display(Name = "盤點項目數")]
        public int CountedItems { get; set; }

        [Display(Name = "差異項目數")]
        public int DifferenceItems { get; set; }

        [Display(Name = "差異百分比")]
        public decimal DifferencePercentage { get; set; }
    }

    public class StockDifferenceDetail
    {
        [Display(Name = "備品編號")]
        public int SparePartNo { get; set; }

        [Display(Name = "備品描述")]
        public string Description { get; set; } = "";

        [Display(Name = "規格")]
        public string Specification { get; set; } = "";

        [Display(Name = "帳面數量")]
        public int SystemQuantity { get; set; }

        [Display(Name = "盤點數量")]
        public int CountedQuantity { get; set; }

        [Display(Name = "差異數量")]
        public int Difference { get; set; }

        [Display(Name = "盤點日期")]
        public DateTime CountDate { get; set; }
    }
}