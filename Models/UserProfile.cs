using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CPMS_Web.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [Display(Name = "姓名")]
        [StringLength(50)]
        public string Name { get; set; } = "";

        [Display(Name = "團隊/部門")]
        [StringLength(20)]
        public string Team { get; set; } = "";

        [Display(Name = "超級使用者")]
        public bool IsSuperUser { get; set; }

        [Display(Name = "主管")]
        public bool IsManager { get; set; }

        [Display(Name = "建立日期")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "啟用狀態")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual IdentityUser User { get; set; } = null!;
    }
} 