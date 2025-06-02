using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CPMS_Web.Models
{
    public class SparePart
    {
        [Key]
        public int No { get; set; }

        [Display(Name = "廠區代碼")]
        [StringLength(20)]
        public string PlantId { get; set; } = "";

        [Display(Name = "位置代碼")]
        [StringLength(50)]
        public string PositionId { get; set; } = "";

        [Display(Name = "子位置代碼")]
        [StringLength(20)]
        public string SubPositionId { get; set; } = "";

        [Display(Name = "分類")]
        [StringLength(50)]
        public string Category { get; set; } = "";

        [Display(Name = "描述")]
        [StringLength(200)]
        public string Description { get; set; } = "";

        [Display(Name = "規格")]
        [StringLength(100)]
        public string Specification { get; set; } = "";

        [Display(Name = "數量")]
        public int Quantity { get; set; }

        [Display(Name = "最後更新")]
        [DataType(DataType.Date)]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "備註")]
        [StringLength(200)]
        public string Remarks { get; set; } = "";

        // Navigation properties
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<MaterialRequestDetail> MaterialRequestDetails { get; set; } = new List<MaterialRequestDetail>();
        public virtual ICollection<StockCountDetail> StockCountDetails { get; set; } = new List<StockCountDetail>();
    }
}