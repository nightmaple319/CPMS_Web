using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models
{
    public class StockCount
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "盤點單號")]
        [StringLength(20)]
        public string CountNo { get; set; } = "";

        [Display(Name = "盤點日期")]
        public DateTime CountDate { get; set; }

        [Display(Name = "盤點人員")]
        public string CounterId { get; set; } = "";

        [Display(Name = "狀態")]
        [StringLength(20)]
        public string Status { get; set; } = "IN_PROGRESS";

        [Display(Name = "備註")]
        [StringLength(500)]
        public string Remarks { get; set; } = "";

        [Display(Name = "建立日期")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual IdentityUser Counter { get; set; } = null!;
        public virtual ICollection<StockCountDetail> StockCountDetails { get; set; } = new List<StockCountDetail>();
    }
} 