using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    /// <summary>
    /// ViewModel برای اعطای مجوز خاص مشاهده تسک‌ها
    /// </summary>
    public class GrantSpecialPermissionViewModel
    {
        [Required(ErrorMessage = "انتخاب تیم الزامی است")]
        public int TeamId { get; set; }
        
        [Required(ErrorMessage = "انتخاب کاربر الزامی است")]
        public string GranteeUserId { get; set; }
        
        /// <summary>
        /// نوع دسترسی
        /// 0- مجوز خاص
        /// 1- مدیر تیم
        /// 2- عضو تیم
        /// </summary>
        [Required(ErrorMessage = "نوع دسترسی الزامی است")]
        public byte AccessType { get; set; } = 0; // پیش‌فرض: مجوز خاص
        
        /// <summary>
        /// نوع مجوز خاص (برای AccessType = 0)
        /// 0 = مشاهده تسک‌های یک کاربر خاص
        /// 1 = مشاهده تسک‌های یک تیم خاص  
        /// 2 = مشاهده تسک‌های تیم و زیرتیم‌های آن
        /// </summary>
        public byte? PermissionType { get; set; }
        
        /// <summary>
        /// کاربر هدف (برای مجوز خاص نوع 0)
        /// </summary>
        public string? TargetUserId { get; set; }
        
        /// <summary>
        /// تیم هدف (برای مجوز خاص نوع 1 و 2)
        /// </summary>
        public int? TargetTeamId { get; set; }
        
        /// <summary>
        /// تاریخ شروع اعتبار
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// تاریخ پایان اعتبار
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر مجاز است")]
        public string? Description { get; set; }

        /// <summary>
        /// نام کاربر دریافت کننده (فقط برای نمایش)
        /// </summary>
        public string? GranteeUserName { get; set; }

        /// <summary>
        /// نام تیم (فقط برای نمایش)
        /// </summary>
        public string? TeamTitle { get; set; }

        /// <summary>
        /// نام کاربر هدف (فقط برای نمایش)
        /// </summary>
        public string? TargetUserName { get; set; }

        /// <summary>
        /// نام تیم هدف (فقط برای نمایش)
        /// </summary>
        public string? TargetTeamTitle { get; set; }

        /// <summary>
        /// متن نوع مجوز برای نمایش
        /// </summary>
        public string PermissionTypeText
        {
            get
            {
                return PermissionType switch
                {
                    0 => "مشاهده تسک‌های کاربر خاص",
                    1 => "مشاهده تسک‌های تیم خاص",
                    2 => "مشاهده تسک‌های تیم و زیرتیم‌ها",
                    _ => "نامشخص"
                };
            }
        }

        /// <summary>
        /// متن نوع دسترسی برای نمایش
        /// </summary>
        public string AccessTypeText
        {
            get
            {
                return AccessType switch
                {
                    0 => "مجوز خاص",
                    1 => "مدیر تیم",
                    2 => "عضو تیم",
                    3 => "دسترسی عمومی",
                    4 => "سازنده",
                    5 => "منتصب",
                    _ => "نامشخص"
                };
            }
        }

        /// <summary>
        /// بررسی معتبر بودن تاریخ‌ها
        /// </summary>
        public bool IsValidDateRange
        {
            get
            {
                if (!StartDate.HasValue || !EndDate.HasValue)
                    return true;

                return StartDate.Value <= EndDate.Value;
            }
        }

        /// <summary>
        /// بررسی اینکه آیا مجوز منقضی شده است
        /// </summary>
        public bool IsExpired
        {
            get
            {
                return EndDate.HasValue && EndDate.Value < DateTime.Now;
            }
        }

        /// <summary>
        /// بررسی اینکه آیا مجوز فعال است
        /// </summary>
        public bool IsCurrentlyActive
        {
            get
            {
                var now = DateTime.Now;
                return (!StartDate.HasValue || StartDate.Value <= now) &&
                       (!EndDate.HasValue || EndDate.Value >= now);
            }
        }
    }

    /// <summary>
    /// ViewModel برای مدیریت مجوزهای مشاهده تسک‌های یک تیم
    /// </summary>
    public class ManageTeamTaskViewersViewModel
    {
        public int TeamId { get; set; }
        public string TeamTitle { get; set; }
        public int BranchId { get; set; }
        
        /// <summary>
        /// اعضای فعلی تیم
        /// </summary>
        public List<TeamMemberViewModel> TeamMembers { get; set; } = new();
        
        /// <summary>
        /// مجوزهای موجود
        /// </summary>
        public List<TaskViewerViewModel> ExistingViewers { get; set; } = new();
        
        /// <summary>
        /// کاربران قابل انتخاب برای اعطای مجوز
        /// </summary>
        public List<UserSelectListItem> AvailableUsers { get; set; } = new();
        
        /// <summary>
        /// آمار مجوزها
        /// </summary>
        public TaskViewerStatsViewModel Stats { get; set; } = new();
    }

   
}