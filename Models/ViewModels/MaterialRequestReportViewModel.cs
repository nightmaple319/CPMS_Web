using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models.ViewModels
{
    public class MaterialRequestReportViewModel
    {
        [Display(Name = "開始日期")]
        public DateTime StartDate { get; set; }

        [Display(Name = "結束日期")]
        public DateTime EndDate { get; set; }

        [Display(Name = "總領料單數")]
        public int TotalRequests { get; set; }

        [Display(Name = "已核准數")]
        public int ApprovedRequests { get; set; }

        [Display(Name = "已駁回數")]
        public int RejectedRequests { get; set; }

        [Display(Name = "已出庫數")]
        public int IssuedRequests { get; set; }

        [Display(Name = "各部門統計")]
        public List<DepartmentRequestStat> DepartmentStatistics { get; set; } = new List<DepartmentRequestStat>();

        [Display(Name = "各類別統計")]
        public List<CategoryRequestStat> CategoryStatistics { get; set; } = new List<CategoryRequestStat>();
    }

    public class DepartmentRequestStat
    {
        [Display(Name = "部門")]
        public string Department { get; set; } = "";

        [Display(Name = "申請單數")]
        public int RequestCount { get; set; }

        [Display(Name = "核准數")]
        public int ApprovedCount { get; set; }

        [Display(Name = "駁回數")]
        public int RejectedCount { get; set; }

        [Display(Name = "出庫數")]
        public int IssuedCount { get; set; }
    }

    public class CategoryRequestStat
    {
        [Display(Name = "類別")]
        public string Category { get; set; } = "";

        [Display(Name = "申請數量")]
        public int RequestedQuantity { get; set; }

        [Display(Name = "核准數量")]
        public int ApprovedQuantity { get; set; }

        [Display(Name = "出庫數量")]
        public int IssuedQuantity { get; set; }
    }
}