using System;
using System.Collections.Generic;

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