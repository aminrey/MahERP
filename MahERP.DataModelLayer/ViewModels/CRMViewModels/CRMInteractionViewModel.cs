using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MahERP.DataModelLayer.ViewModels.CRMViewModels
{
    public class CRMInteractionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "کد تعامل الزامی است")]
        [Display(Name = "کد تعامل")]
        public string CRMCode { get; set; }

        [Required(ErrorMessage = "عنوان تعامل الزامی است")]
        [Display(Name = "عنوان تعامل")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "نوع تعامل")]
        public byte CRMType { get; set; }

        [Display(Name = "جهت تماس")]
        public byte Direction { get; set; }

        [Display(Name = "نتیجه تعامل")]
        public byte Result { get; set; }

        [Display(Name = "مدت زمان (دقیقه)")]
        public int? Duration { get; set; }

        [Display(Name = "شماره تماس")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "آدرس ایمیل")]
        [EmailAddress(ErrorMessage = "آدرس ایمیل معتبر نیست")]
        public string? EmailAddress { get; set; }

        [Display(Name = "محل جلسه")]
        public string? MeetingLocation { get; set; }

        [Display(Name = "زمان شروع")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "زمان پایان")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "طرف حساب")]
        public int? StakeholderId { get; set; }

        [Display(Name = "شخص مرتبط")]
        public int? StakeholderContactId { get; set; }

        [Required(ErrorMessage = "شعبه الزامی است")]
        [Display(Name = "شعبه")]
        public int BranchId { get; set; }

        [Display(Name = "قرارداد")]
        public int? ContractId { get; set; }

        [Display(Name = "تاریخ پیگیری بعدی")]
        public DateTime? NextFollowUpDate { get; set; }

        [Display(Name = "یادداشت پیگیری")]
        public string? NextFollowUpNote { get; set; }

        public bool IsActive { get; set; } = true;

        // Read-only properties
        public DateTime CreateDate { get; set; }
        public string? CreatorUserId { get; set; }
        public string? CreatorName { get; set; }
        public string? StakeholderName { get; set; }
        public string? StakeholderContactName { get; set; }
        public string? BranchName { get; set; }
        public string? ContractTitle { get; set; }
        public DateTime? LastUpdateDate { get; set; }

        // Collections
        public List<CRMAttachmentViewModel>? Attachments { get; set; }
        public List<CRMCommentViewModel>? Comments { get; set; }
        public List<CRMParticipantViewModel>? Participants { get; set; }

        // File uploads
        public List<IFormFile>? UploadFiles { get; set; }

        // Helper properties
        public string CRMTypeText => CRMType switch
        {
            0 => "تماس تلفنی",
            1 => "جلسه حضوری", 
            2 => "ایمیل",
            3 => "پیامک",
            4 => "سایر",
            _ => "نامشخص"
        };

        public string DirectionText => Direction switch
        {
            0 => "ورودی",
            1 => "خروجی",
            _ => "نامشخص"
        };

        public string ResultText => Result switch
        {
            0 => "بی نتیجه",
            1 => "موفق",
            2 => "نیاز به پیگیری",
            _ => "نامشخص"
        };

        public string StatusText => IsActive ? "فعال" : "غیرفعال";
    }
}