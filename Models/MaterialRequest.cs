using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models
{
    public class MaterialRequest
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "領料單號")]
        [StringLength(20)]
        public string RequestNo { get; set; } = "";

        [Display(Name = "申請日期")]
        public DateTime RequestDate { get; set; }

        [Display(Name = "申請人")]
        public string RequesterId { get; set; } = "";

        [Display(Name = "申請部門")]
        [StringLength(50)]
        public string Department { get; set; } = "";

        [Display(Name = "狀態")]
        [StringLength(20)]
        public string Status { get; set; } = "PENDING";

        [Display(Name = "審核者")]
        public string? ApproverId { get; set; }

        [Display(Name = "審核日期")]
        public DateTime? ApprovalDate { get; set; }

        [Display(Name = "出庫者")]
        public string? IssuerId { get; set; }

        [Display(Name = "出庫日期")]
        public DateTime? IssueDate { get; set; }

        [Display(Name = "駁回原因")]
        [StringLength(200)]
        public string? RejectReason { get; set; }

        [Display(Name = "備註")]
        [StringLength(500)]
        public string Remarks { get; set; } = "";

        [Display(Name = "建立日期")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual IdentityUser Requester { get; set; } = null!;
        public virtual IdentityUser? Approver { get; set; }
        public virtual IdentityUser? Issuer { get; set; }
        public virtual ICollection<MaterialRequestDetail> MaterialRequestDetails { get; set; } = new List<MaterialRequestDetail>();

        // 為了向後相容，保留 Details 屬性
        public virtual ICollection<MaterialRequestDetail> Details => MaterialRequestDetails;
    }
}