using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MahERP.DataModelLayer.ViewModels.TaskViewModels
{
    public class TaskViewModel
    {
        public int Id { get; set; }

        //[Required(ErrorMessage = "کد تسک الزامی است")]
        //[Display(Name = "کد تسک")]
        //[MaxLength(7, ErrorMessage = "کد تسک حداکثر 7 کاراکتر باشد")]
        public string? TaskCode { get; set; }

        [Required(ErrorMessage = "عنوان تسک الزامی است")]
        [Display(Name = "عنوان تسک")]
        [MaxLength(200, ErrorMessage = "عنوان تسک حداکثر 200 کاراکتر باشد")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "نوع تسک")]
        public byte TaskType { get; set; }

        [Display(Name = "دسته‌بندی")]
        public int? TaskCategoryId { get; set; }

        [Display(Name = "طرف حساب")]
        public int? StakeholderId { get; set; }

        [Display(Name = "قرارداد")]
        public int? ContractId { get; set; }

        [Display(Name = "شعبه")]
        public int? BranchId { get; set; }

        [Display(Name = "تسک بالادست")]
        public int? ParentTaskId { get; set; }

        [Display(Name = "تاریخ مهلت انجام")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "تاریخ مهلت انجام")]
        public string? DueDatePersian { get; set; }

        [Display(Name = "تاریخ تکمیل")]
        public DateTime? CompletionDate { get; set; }

        [Display(Name = "تاریخ تایید سرپرست")]
        public DateTime? SupervisorApprovedDate { get; set; }

        [Display(Name = "تاریخ تایید مدیر")]
        public DateTime? ManagerApprovedDate { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "ایجاد کننده")]
        public string? CreatorUserId { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties for display
        [Display(Name = "عنوان دسته‌بندی")]
        public string? CategoryTitle { get; set; }

        [Display(Name = "نام ایجاد کننده")]
        public string? CreatorName { get; set; }

        [Display(Name = "نام طرف حساب")]
        public string? StakeholderName { get; set; }

        [Display(Name = "عنوان قرارداد")]
        public string? ContractTitle { get; set; }

        // Attachments
        [Display(Name = "فایل‌های پیوست")]
        public List<IFormFile>? Attachments { get; set; }

        // Operations
        public List<TaskOperationViewModel>? Operations { get; set; }

        // AssignmentsTask
        public List<TaskAssignmentViewModel>? AssignmentsTaskUsers { get; set; }

        // AssignmentsCopyCarbon
        public List<TaskAssignmentViewModel>? AssignmentsCopyCarbonUsers { get; set; }


        // Status display
        [Display(Name = "وضعیت")]
        public string StatusText
        {
            get
            {
                if (IsDeleted) return "حذف شده";
                if (!IsActive) return "غیرفعال";
                if (CompletionDate.HasValue && ManagerApprovedDate.HasValue) return "تکمیل و تایید شده";
                if (CompletionDate.HasValue && SupervisorApprovedDate.HasValue) return "تکمیل و تایید سرپرست";
                if (CompletionDate.HasValue) return "تکمیل شده";
                if (DueDate.HasValue && DateTime.Now > DueDate.Value) return "تاخیر";
                return "در حال انجام";
            }
        }

        // Progress calculation
        [Display(Name = "درصد پیشرفت")]
        public int ProgressPercentage
        {
            get
            {
                if (Operations == null || !Operations.Any()) return 0;

                int totalOperations = Operations.Count;
                int completedOperations = Operations.Count(o => o.IsCompleted);

                if (totalOperations == 0) return 0;
                return (completedOperations * 100) / totalOperations;
            }
        }
    }

    public class TaskOperationViewModel
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        [Required(ErrorMessage = "عنوان عملیات الزامی است")]
        [Display(Name = "عنوان عملیات")]
        [MaxLength(200, ErrorMessage = "عنوان عملیات حداکثر 200 کاراکتر باشد")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "ترتیب انجام")]
        public int OperationOrder { get; set; }

        [Display(Name = "تکمیل شده")]
        public bool IsCompleted { get; set; }

        [Display(Name = "تاریخ تکمیل")]
        public DateTime? CompletionDate { get; set; }

        [Display(Name = "تکمیل کننده")]
        public string? CompletedByUserId { get; set; }

        [Display(Name = "نام تکمیل کننده")]
        public string? CompletedByUserName { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreateDate { get; set; }
    }

    public class TaskAssignmentViewModel
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        [Required(ErrorMessage = "کاربر اختصاص داده شده الزامی است")]
        [Display(Name = "کاربر")]
        public string AssignedUserId { get; set; }

        [Display(Name = "نام کاربر")]
        public string? AssignedUserName { get; set; }

        [Display(Name = "تخصیص دهنده")]
        public string? AssignerUserId { get; set; }

        [Display(Name = "نام تخصیص دهنده")]
        public string? AssignerUserName { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "تاریخ تکمیل")]
        public DateTime? CompletionDate { get; set; }

        [Display(Name = "تاریخ تخصیص")]
        public DateTime AssignDate { get; set; }
    }

    public class TaskCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان دسته‌بندی الزامی است")]
        [Display(Name = "عنوان دسته‌بندی")]
        [MaxLength(100, ErrorMessage = "عنوان دسته‌بندی حداکثر 100 کاراکتر باشد")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "دسته‌بندی والد")]
        public int? ParentCategoryId { get; set; }

        [Display(Name = "عنوان دسته‌بندی والد")]
        public string? ParentCategoryTitle { get; set; }

        [Display(Name = "رنگ (کلاس CSS)")]
        public string? ColorClass { get; set; }

        [Display(Name = "آیکون")]
        public string? IconClass { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; } = true;
    }

    public class TaskSearchViewModel
    {
        [Display(Name = "عبارت جستجو")]
        public string? SearchTerm { get; set; }

        [Display(Name = "دسته‌بندی")]
        public int? CategoryId { get; set; }

        [Display(Name = "اختصاص داده شده به")]
        public string? AssignedUserId { get; set; }

        [Display(Name = "وضعیت تکمیل")]
        public bool? IsCompleted { get; set; }

        [Display(Name = "تاریخ شروع")]
        public string? FromDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public string? ToDate { get; set; }

        [Display(Name = "طرف حساب")]
        public int? StakeholderId { get; set; }

        [Display(Name = "نمایش حذف شده‌ها")]
        public bool IncludeDeleted { get; set; } = false;
    }
}