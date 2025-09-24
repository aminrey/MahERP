using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای سمت‌های تیم
    /// </summary>
    public class TeamPositionViewModel
    {
        public int Id { get; set; }

        public int TeamId { get; set; }
        public string? TeamTitle { get; set; }

        [Required(ErrorMessage = "عنوان سمت الزامی است")]
        [Display(Name = "عنوان سمت")]
        [MaxLength(100, ErrorMessage = "عنوان سمت حداکثر 100 کاراکتر باشد")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(500, ErrorMessage = "توضیحات حداکثر 500 کاراکتر باشد")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "سطح قدرت الزامی است")]
        [Display(Name = "سطح قدرت")]
        [Range(0, 100, ErrorMessage = "سطح قدرت باید بین 0 تا 100 باشد")]
        public int PowerLevel { get; set; }

        [Display(Name = "مشاهده تسک‌های زیردستان")]
        public bool CanViewSubordinateTasks { get; set; } = true;

        [Display(Name = "مشاهده تسک‌های همسطح")]
        public bool CanViewPeerTasks { get; set; } = false;

        [Display(Name = "حداکثر تعداد اعضا")]
        public int? MaxMembers { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "سمت پیش‌فرض")]
        public bool IsDefault { get; set; } = false;

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        public DateTime CreateDate { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdaterName { get; set; }

        // اطلاعات اعضا
        public List<TeamMemberViewModel> Members { get; set; } = new();
        public int CurrentMembersCount => Members.Count(m => m.IsActive);
        public bool CanAddMember => !MaxMembers.HasValue || CurrentMembersCount < MaxMembers.Value;

        /// <summary>
        /// متن سطح قدرت برای نمایش
        /// </summary>
        public string PowerLevelText => PowerLevel switch
        {
            0 => "مدیر تیم",
            1 => "معاون/سرپرست",
            2 => "کارشناس ارشد",
            3 => "کارشناس",
            _ => $"سطح {PowerLevel}"
        };
    }

    /// <summary>
    /// ViewModel برای ایجاد/ویرایش سمت
    /// </summary>
    public class CreateTeamPositionViewModel
    {
        public int TeamId { get; set; }
        public string? TeamTitle { get; set; }

        [Required(ErrorMessage = "عنوان سمت الزامی است")]
        [Display(Name = "عنوان سمت")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "سطح قدرت الزامی است")]
        [Display(Name = "سطح قدرت")]
        [Range(0, 100, ErrorMessage = "سطح قدرت باید بین 0 تا 100 باشد")]
        public int PowerLevel { get; set; }

        [Display(Name = "مشاهده تسک‌های زیردستان")]
        public bool CanViewSubordinateTasks { get; set; } = true;

        [Display(Name = "مشاهده تسک‌های همسطح")]
        public bool CanViewPeerTasks { get; set; } = false;

        [Display(Name = "حداکثر تعداد اعضا")]
        public int? MaxMembers { get; set; }

        [Display(Name = "سمت پیش‌فرض")]
        public bool IsDefault { get; set; } = false;

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ViewModel برای نمایش سلسله مراتب سمت‌ها
    /// </summary>
    public class TeamPositionHierarchyViewModel
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int PowerLevel { get; set; }
        public string? PowerLevelText { get; set; }
        public int CurrentMembers { get; set; }
        public int? MaxMembers { get; set; }
        public bool CanAddMember { get; set; }
        public List<TeamMemberViewModel> Members { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای تخصیص سمت به عضو
    /// </summary>
    public class AssignPositionViewModel
    {
        public int MemberId { get; set; }
        public int TeamId { get; set; }
        public string UserFullName { get; set; }
        public string TeamTitle { get; set; }

        [Required(ErrorMessage = "انتخاب سمت الزامی است")]
        public int PositionId { get; set; }

        // فیلدهای جدید برای سمت دستی
        public string CustomTitle { get; set; }
        public int? CustomPowerLevel { get; set; }
        public string CustomDescription { get; set; }
        public bool CustomCanViewSubordinateTasks { get; set; } = true;
        public bool CustomCanViewPeerTasks { get; set; } = false;
    }

    /// <summary>
    /// ViewModel برای اضافه کردن عضو به سمت خاص
    /// </summary>
    public class AddMemberToPositionViewModel
    {
        public int TeamId { get; set; }
        public int PositionId { get; set; }
        public string? TeamTitle { get; set; }
        public string? PositionTitle { get; set; }

        [Required(ErrorMessage = "انتخاب عضو الزامی است")]
        public int MemberId { get; set; }
    }
    /// <summary>
     /// کلاس برای سمت‌های پیشنهادی
     /// </summary>
    public class SuggestedPosition
    {
        public string Title { get; set; } = string.Empty;
        public int PowerLevel { get; set; }
        public string? Description { get; set; }
        public bool CanViewSubordinateTasks => PowerLevel <= 2; // مدیر، معاون، کارشناس ارشد
        public bool CanViewPeerTasks => PowerLevel <= 1; // فقط مدیر و معاون
    }
}