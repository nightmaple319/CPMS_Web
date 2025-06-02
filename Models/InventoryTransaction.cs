using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "備品編號")]
        public int SparePartNo { get; set; }

        [Display(Name = "異動類型")]
        [StringLength(20)]
        public string TransactionType { get; set; } = "";

        [Display(Name = "異動數量")]
        public int Quantity { get; set; }

        [Display(Name = "異動前數量")]
        public int PreviousQuantity { get; set; }

        [Display(Name = "異動後數量")]
        public int NewQuantity { get; set; }

        [Display(Name = "異動日期")]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "異動人員")]
        public string UserId { get; set; } = "";

        [Display(Name = "異動原因")]
        [StringLength(200)]
        public string Reason { get; set; } = "";

        [Display(Name = "參考單號")]
        [StringLength(50)]
        public string ReferenceNo { get; set; } = "";

        [Display(Name = "備註")]
        [StringLength(500)]
        public string Remarks { get; set; } = "";

        // Navigation properties
        public virtual SparePart SparePart { get; set; } = null!;
        public virtual IdentityUser User { get; set; } = null!;
    }
} 