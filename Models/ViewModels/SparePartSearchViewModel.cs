using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models.ViewModels
{
    public class SparePartSearchViewModel
    {
        [Display(Name = "廠區代碼")]
        public string? PlantId { get; set; }

        [Display(Name = "位置代碼")]
        public string? PositionId { get; set; }

        [Display(Name = "分類")]
        public string? Category { get; set; }

        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Display(Name = "規格")]
        public string? Specification { get; set; }

        [Display(Name = "是否顯示零庫存")]
        public bool ShowZeroStock { get; set; } = true;

        public IEnumerable<SparePart> Results { get; set; } = new List<SparePart>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
} 