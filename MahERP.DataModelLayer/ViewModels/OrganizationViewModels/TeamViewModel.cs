using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای نمایش تیم
    /// </summary>
    public class TeamViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان تیم الزامی است")]
        [Display(Name = "عنوان تیم")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "تیم والد")]
        public int? ParentTeamId { get; set; }
        public string? ParentTeamTitle { get; set; }

        [Display(Name = "مدیر تیم")]
        public string? ManagerUserId { get; set; }
        public string? ManagerFullName { get; set; }

        [Display(Name = "شعبه")]
        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        [Display(Name = "سطح دسترسی")]
        public byte AccessLevel { get; set; }
        public string? AccessLevelText { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdaterName { get; set; }

        // برای نمایش چارت سازمانی
        public List<TeamViewModel> ChildTeams { get; set; } = new List<TeamViewModel>();

        /// <summary>
        /// سمت‌های تیم (در صورت درخواست)
        /// </summary>
        public List<TeamPositionViewModel> TeamPositions { get; set; } = new();

        /// <summary>
        /// اعضای تیم (در صورت درخواست)
        /// </summary>
        public List<TeamMemberViewModel> TeamMembers { get; set; } = new();
        
        public int Level { get; set; } // سطح در چارت سازمانی
        public bool HasManager => !string.IsNullOrEmpty(ManagerUserId);

        // خصوصیات محاسبه شده جدید
        /// <summary>
        /// تعداد سمت‌های فعال در تیم
        /// </summary>
        public int ActivePositionsCount => TeamPositions.Count(p => p.IsActive);

        /// <summary>
        /// آیا تیم سمت‌هایی تعریف شده دارد
        /// </summary>
        public bool HasPositions => TeamPositions.Any(p => p.IsActive);

        /// <summary>
        /// اعضای بدون سمت رسمی
        /// </summary>
        public List<TeamMemberViewModel> MembersWithoutPosition =>
            TeamMembers.Where(m => m.IsActive && !m.HasFormalPosition).ToList();

        /// <summary>
        /// اعضای دارای سمت رسمی مرتب شده بر اساس سطح قدرت
        /// </summary>
        public List<TeamMemberViewModel> MembersWithPosition =>
            TeamMembers.Where(m => m.IsActive && m.HasFormalPosition)
                       .OrderBy(m => m.PowerLevel)
                       .ToList();
    }

    /// <summary>
    /// ViewModel برای نمایش عضو تیم
    /// </summary>
    public class TeamMemberViewModel
    {
        public int Id { get; set; }

        [Display(Name = "تیم")]
        public int TeamId { get; set; }
        public string? TeamTitle { get; set; }

        [Required(ErrorMessage = "انتخاب کاربر الزامی است")]
        [Display(Name = "کاربر")]
        public string UserId { get; set; }
        public string? UserFullName { get; set; }

        /// <summary>
        /// شناسه سمت رسمی در تیم (رابطه با TeamPosition)
        /// </summary>
        [Display(Name = "سمت رسمی")]
        public int? PositionId { get; set; }
        public string? PositionTitle { get; set; }
        public int? PowerLevel { get; set; }

        /// <summary>
        /// عنوان سمت (فیلد legacy برای سازگاری)
        /// </summary>
        [Display(Name = "سمت")]
        [MaxLength(100, ErrorMessage = "حداکثر 100 کاراکتر")]
        public string? Position { get; set; }

        [Display(Name = "شرح نقش")]
        [MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
        public string? RoleDescription { get; set; }

        [Display(Name = "نوع عضویت")]
        public byte MembershipType { get; set; }
        public string? MembershipTypeText { get; set; }

        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "اضافه شده توسط")]
        public string? AddedByUserId { get; set; }
        public string? AddedByUserName { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }

        // خصوصیات محاسبه شده
        /// <summary>
        /// نام سمت برای نمایش (اولویت با سمت رسمی)
        /// </summary>
        public string DisplayPosition => !string.IsNullOrEmpty(PositionTitle) ? PositionTitle : Position ?? "عضو";

        /// <summary>
        /// سطح قدرت عضو (از سمت گرفته می‌شود یا 999 برای بدون سمت)
        /// </summary>
        public int EffectivePowerLevel => PowerLevel ?? 999;

        /// <summary>
        /// آیا این عضو سمت رسمی دارد
        /// </summary>
        public bool HasFormalPosition => PositionId.HasValue;

        /// <summary>
        /// متن سطح قدرت برای نمایش
        /// </summary>
        public string PowerLevelText => PowerLevel switch
        {
            0 => "مدیر تیم",
            1 => "معاون/سرپرست",
            2 => "کارشناس ارشد",
            3 => "کارشناس",
            _ when PowerLevel.HasValue => $"سطح {PowerLevel}",
            _ => "بدون سمت"
        };

        /// <summary>
        /// کلاس CSS برای نمایش سطح قدرت
        /// </summary>
        public string PowerLevelCssClass => PowerLevel switch
        {
            0 => "badge bg-danger",      // مدیر - قرمز
            1 => "badge bg-warning",     // معاون - نارنجی
            2 => "badge bg-info",        // کارشناس ارشد - آبی
            3 => "badge bg-primary",     // کارشناس - آبی تیره
            _ when PowerLevel.HasValue => "badge bg-secondary", // سایر سطوح - خاکستری
            _ => "badge bg-light text-dark" // بدون سمت - روشن
        };

        /// <summary>
        /// نوع عضویت به صورت متن
        /// </summary>
        public string MembershipTypeDisplay => MembershipType switch
        {
            0 => "عضو عادی",
            1 => "عضو ویژه",
            2 => "مدیر تیم",
            _ => "نامشخص"
        };
    }

    /// <summary>
    /// ViewModel برای نمایش چارت سازمانی
    /// </summary>
    public class OrganizationalChartViewModel
    {
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public List<TeamViewModel> RootTeams { get; set; } = new List<TeamViewModel>();
        public List<TeamViewModel> AllTeams { get; set; } = new List<TeamViewModel>();
        public List<UserSelectListItem> AvailableUsers { get; set; } = new List<UserSelectListItem>();
    }

    public class UserSelectListItem
    {
        public string UserId { get; set; }
        public string? FullName { get; set; }
        public string? Position { get; set; }
    }

    /// <summary>
    /// ViewModel برای انتخاب مدیر تیم
    /// </summary>
    public class AssignManagerViewModel
    {
        public int TeamId { get; set; }
        public string? TeamTitle { get; set; }

        [Required(ErrorMessage = "انتخاب مدیر الزامی است")]
        [Display(Name = "مدیر تیم")]
        public string ManagerUserId { get; set; }

        public string? CurrentManagerName { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش درخت سمت‌ها و اعضا
    /// </summary>
    public class TeamHierarchyViewModel
    {
        public int TeamId { get; set; }
        public string TeamTitle { get; set; }
        public List<TeamPositionHierarchyViewModel> Positions { get; set; } = new();
        public List<TeamMemberViewModel> MembersWithoutPosition { get; set; } = new();
    }

}