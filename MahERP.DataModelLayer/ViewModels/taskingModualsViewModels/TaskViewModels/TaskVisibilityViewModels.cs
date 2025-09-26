using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای نمایش چارت قدرت مشاهده تسک‌ها
    /// </summary>
    public class TaskVisibilityChartViewModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        
        /// <summary>
        /// ساختار سلسله مراتبی تیم‌ها
        /// </summary>
        public List<TeamTaskVisibilityNode> TeamHierarchy { get; set; } = new();
        
        /// <summary>
        /// مجوزهای خاص (تبصره‌ها)
        /// </summary>
        public List<SpecialTaskPermissionNode> SpecialPermissions { get; set; } = new();
        
        /// <summary>
        /// آمار کلی
        /// </summary>
        public TaskVisibilityStatsViewModel Stats { get; set; } = new();
    }

    /// <summary>
    /// ViewModel برای نمایش مجوزهای موجود
    /// </summary>
    public class TaskViewPermissionViewModel
    {
        public int Id { get; set; }
        public string GranteeUserId { get; set; }
        public string UserFullName { get; set; }
        public byte PermissionType { get; set; }
        public string PermissionTypeText { get; set; }
        public string? TargetUserId { get; set; }
        public string? TargetUserFullName { get; set; }
        public int? TargetTeamId { get; set; }
        public string? TargetTeamTitle { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedByUserName { get; set; }

        /// <summary>
        /// متن کامل نوع دسترسی برای نمایش
        /// </summary>
        public string AccessTypeText
        {
            get
            {
                return PermissionType switch
                {
                    0 => $"مشاهده تسک‌های {TargetUserFullName}",
                    1 => $"مشاهده تسک‌های تیم {TargetTeamTitle}",
                    2 => $"مشاهده تسک‌های تیم {TargetTeamTitle} و زیرتیم‌ها",
                    _ => "نامشخص"
                };
            }
        }
    }

    /// <summary>
    /// ViewModel برای مدیریت مجوزهای مشاهده تسک‌های تیم
    /// این کلاس واحد برای مدیریت تبصره‌های تسک
    /// </summary>
    public class ManageTeamTaskViewersViewModel
    {
        public int TeamId { get; set; }
        public string TeamTitle { get; set; }
        public int BranchId { get; set; }

        /// <summary>
        /// اعضای فعال تیم
        /// </summary>
        public List<TeamMemberViewModel> TeamMembers { get; set; } = new();

        /// <summary>
        /// کاربران موجود در شعبه برای انتخاب به عنوان مبدا
        /// </summary>
        public List<UserSelectListItem> AvailableUsers { get; set; } = new();

        /// <summary>
        /// تیم‌های موجود در شعبه برای انتخاب به عنوان مقصد
        /// </summary>
        public List<TeamSelectListItem> AvailableTeams { get; set; } = new();

        /// <summary>
        /// مجوزهای موجود برای این تیم (استفاده از TaskViewPermissionViewModel)
        /// </summary>
        public List<TaskViewPermissionViewModel> ExistingViewers { get; set; } = new();
    }

    public class TeamSelectListItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    /// <summary>
    /// ViewModel برای ایجاد تبصره جدید
    /// </summary>
    public class GrantSpecialPermissionViewModel
    {
        public int TeamId { get; set; }
        
        [Required(ErrorMessage = "انتخاب کاربر مبدا الزامی است")]
        [Display(Name = "کاربر مبدا")]
        public string GranteeUserId { get; set; }

        [Required(ErrorMessage = "انتخاب نوع مجوز الزامی است")]
        [Display(Name = "نوع مجوز")]
        public byte PermissionType { get; set; }

        [Display(Name = "کاربر مقصد")]
        public string TargetUserId { get; set; }

        [Display(Name = "تیم مقصد")]
        public int? TargetTeamId { get; set; }

        [Display(Name = "نوع دسترسی")]
        public byte AccessType { get; set; } = 0; // مجوز خاص

        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(500, ErrorMessage = "توضیحات حداکثر 500 کاراکتر باشد")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// گره تیم در چارت قدرت مشاهده
    /// </summary>
    public class TeamTaskVisibilityNode
    {
        public int TeamId { get; set; }
        public string TeamTitle { get; set; }
        public int Level { get; set; }
        public string ManagerName { get; set; }
        public string ManagerUserId { get; set; }
        
        /// <summary>
        /// سمت‌ها و قدرت مشاهده آن‌ها
        /// </summary>
        public List<PositionTaskVisibilityInfo> Positions { get; set; } = new();
        
        /// <summary>
        /// اعضای بدون سمت
        /// </summary>
        public List<MemberTaskVisibilityInfo> MembersWithoutPosition { get; set; } = new();
        
        /// <summary>
        /// تیم‌های فرزند
        /// </summary>
        public List<TeamTaskVisibilityNode> SubTeams { get; set; } = new();
        
        /// <summary>
        /// مجوزهای خاص مرتبط با این تیم
        /// </summary>
        public List<SpecialTaskPermissionNode> RelatedSpecialPermissions { get; set; } = new();
    }

    /// <summary>
    /// اطلاعات قدرت مشاهده سمت
    /// </summary>
    public class PositionTaskVisibilityInfo
    {
        public int PositionId { get; set; }
        public string PositionTitle { get; set; }
        public int PowerLevel { get; set; }
        public bool CanViewSubordinateTasks { get; set; }
        public bool CanViewPeerTasks { get; set; }
        
        /// <summary>
        /// اعضای این سمت
        /// </summary>
        public List<MemberTaskVisibilityInfo> Members { get; set; } = new();
        
        /// <summary>
        /// سمت‌هایی که این سمت می‌تواند تسک‌هایشان را ببیند
        /// </summary>
        public List<int> VisiblePositionIds { get; set; } = new();
    }

    /// <summary>
    /// اطلاعات قدرت مشاهده عضو
    /// </summary>
    public class MemberTaskVisibilityInfo
    {
        public int MemberId { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string? PositionTitle { get; set; }
        public int? PowerLevel { get; set; }
        
        /// <summary>
        /// تعداد تسک‌هایی که می‌تواند ببیند
        /// </summary>
        public int VisibleTasksCount { get; set; }
        
        /// <summary>
        /// منابع دسترسی (مدیر، عضو، مجوز خاص)
        /// </summary>
        public List<string> AccessSources { get; set; } = new();
    }

    /// <summary>
    /// گره مجوز خاص (تبصره)
    /// </summary>
    public class SpecialTaskPermissionNode
    {
        public int ViewerId { get; set; }
        public string GranteeUserId { get; set; }
        public string GranteeUserName { get; set; }
        public string GranteeTeamTitle { get; set; }
        
        public byte PermissionType { get; set; }
        public string PermissionTypeText { get; set; }
        
        public string? TargetUserId { get; set; }
        public string? TargetUserName { get; set; }
        public int? TargetTeamId { get; set; }
        public string? TargetTeamTitle { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        
        public string Description { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedByUserName { get; set; }
    }

    /// <summary>
    /// آمار چارت قدرت مشاهده
    /// </summary>
    public class TaskVisibilityStatsViewModel
    {
        public int TotalTeams { get; set; }
        public int TotalMembers { get; set; }
        public int TotalPositions { get; set; }
        public int TotalSpecialPermissions { get; set; }
        public int ActiveSpecialPermissions { get; set; }
        public int ExpiredSpecialPermissions { get; set; }
        
        /// <summary>
        /// توزیع قدرت مشاهده بر اساس سطح
        /// </summary>
        public Dictionary<int, int> PowerLevelDistribution { get; set; } = new();
    }
}