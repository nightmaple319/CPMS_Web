using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models
{
    public class MaterialRequestDetail
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "領料單")]
        public int RequestId { get; set; }

        [Display(Name = "備品編號")]
        public int SparePartNo { get; set; }

        [Display(Name = "申請數量")]
        public int RequestedQuantity { get; set; }

        [Display(Name = "核准數量")]
        public int ApprovedQuantity { get; set; }

        [Display(Name = "已發數量")]
        public int IssuedQuantity { get; set; }

        [Display(Name = "備註")]
        [StringLength(200)]
        public string Remarks { get; set; } = "";

        // Navigation properties
        public virtual MaterialRequest MaterialRequest { get; set; } = null!;
        public virtual SparePart SparePart { get; set; } = null!;
    }
} 