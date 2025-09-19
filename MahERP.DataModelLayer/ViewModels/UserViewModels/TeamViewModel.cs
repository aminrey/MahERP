using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public List<TeamMemberViewModel> TeamMembers { get; set; } = new List<TeamMemberViewModel>();
        public int Level { get; set; } // سطح در چارت سازمانی
        public bool HasManager => !string.IsNullOrEmpty(ManagerUserId);
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
}