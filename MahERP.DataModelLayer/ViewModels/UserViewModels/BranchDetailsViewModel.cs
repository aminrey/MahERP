using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MahERP.DataModelLayer.Entities.AcControl;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل یک شعبه شامل کاربران، طرف حساب‌ها و شعبه‌های زیرمجموعه
    /// </summary>
    public class BranchDetailsViewModel
    {
        // اطلاعات پایه شعبه
        public int Id { get; set; }

        [Display(Name = "نام شعبه")]
        public string Name { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "تلفن")]
        public string Phone { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "نام مدیر")]
        public string ManagerName { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "شعبه اصلی")]
        public bool IsMainBranch { get; set; }

        [Display(Name = "شعبه مادر")]
        public int? ParentId { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "تاریخ آخرین بروزرسانی")]
        public DateTime? LastUpdateDate { get; set; }

        // اطلاعات شعبه مادر
        public BranchViewModel ParentBranch { get; set; }

        // کاربران شعبه
        [Display(Name = "کاربران شعبه")]
        public List<BranchUser> BranchUsers { get; set; } = new List<BranchUser>();

        // طرف حساب‌های شعبه (با اطلاعات اتصال)
        [Display(Name = "طرف حساب‌های شعبه")]
        public List<StakeholderBranch> BranchStakeholders { get; set; } = new List<StakeholderBranch>();

        // طرف حساب‌های شعبه (فقط اطلاعات Stakeholder)
        [Display(Name = "طرف حساب‌های شعبه")]
        public List<Stakeholder> Stakeholders { get; set; } = new List<Stakeholder>();

        // شعبه‌های زیرمجموعه
        [Display(Name = "شعبه‌های زیرمجموعه")]
        public List<Branch> ChildBranches { get; set; } = new List<Branch>();

        // آمار و اطلاعات خلاصه
        [Display(Name = "تعداد کاربران فعال")]
        public int ActiveUsersCount { get; set; }

        [Display(Name = "تعداد طرف حساب‌های فعال")]
        public int ActiveStakeholdersCount { get; set; }

        [Display(Name = "تعداد شعبه‌های زیرمجموعه")]
        public int ChildBranchesCount { get; set; }

        [Display(Name = "تعداد شعبه‌های زیرمجموعه فعال")]
        public int ActiveChildBranchesCount { get; set; }

        // خصوصیات محاسبه شده
        public string ParentBranchName => ParentBranch?.Name;
        public string StatusText => IsActive ? "فعال" : "غیرفعال";
        public string BranchTypeText => IsMainBranch ? "شعبه اصلی" : "شعبه فرعی";
    }
}