using MahERP.CommonLayer.PublicClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای مدیریت مجوزهای مشاهده تسک‌های تیم
    /// </summary>
    public class ManageTeamTaskViewersViewModel
    {
        public int TeamId { get; set; }
        public string TeamTitle { get; set; }
        public int BranchId { get; set; }
        
        public List<TeamMemberViewModel> TeamMembers { get; set; } = new();
        public List<TaskViewerViewModel> ExistingViewers { get; set; } = new();
        public List<UserSelectListItem> AvailableUsers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای نمایش مجوز مشاهده تسک
    /// </summary>
    public class TaskViewerViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public byte AccessType { get; set; }
        public string AccessTypeText { get; set; }
        public string? Description { get; set; }
        public DateTime AddedDate { get; set; }
        public bool IsActive { get; set; }
        public string AddedDatePersian => ConvertDateTime.ConvertMiladiToShamsi(AddedDate, "yyyy/MM/dd");
    }

    /// <summary>
    /// ViewModel برای اعطای مجوز خاص
    /// </summary>
    public class GrantSpecialPermissionViewModel
    {
        public int TeamId { get; set; }

        [Required(ErrorMessage = "انتخاب کاربر الزامی است")]
        [Display(Name = "کاربر دریافت‌کننده مجوز")]
        public string GranteeUserId { get; set; }

        [Required(ErrorMessage = "نوع دسترسی الزامی است")]
        [Display(Name = "نوع دسترسی")]
        public byte AccessType { get; set; }

        [Display(Name = "نوع مجوز خاص")]
        public byte? PermissionType { get; set; }

        [Display(Name = "تاریخ شروع اعتبار")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ پایان اعتبار")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(500, ErrorMessage = "توضیحات حداکثر 500 کاراکتر باشد")]
        public string? Description { get; set; }
    }
}