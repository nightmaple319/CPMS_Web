using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models
{
    public class StockCountDetail
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "盤點單")]
        public int CountId { get; set; }

        [Display(Name = "備品編號")]
        public int SparePartNo { get; set; }

        [Display(Name = "帳面數量")]
        public int SystemQuantity { get; set; }

        [Display(Name = "盤點數量")]
        public int CountedQuantity { get; set; }

        [Display(Name = "差異數量")]
        public int Difference => CountedQuantity - SystemQuantity;

        [Display(Name = "備註")]
        [StringLength(200)]
        public string Remarks { get; set; } = "";

        // Navigation properties
        public virtual StockCount StockCount { get; set; } = null!;
        public virtual SparePart SparePart { get; set; } = null!;
    }
} 