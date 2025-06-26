using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CRMViewModels
{
    public class CRMSearchViewModel
    {
        [Display(Name = "جستجو در عنوان و توضیحات")]
        public string? SearchTerm { get; set; }

        [Display(Name = "نوع تعامل")]
        public byte? CRMType { get; set; }

        [Display(Name = "جهت تماس")]
        public byte? Direction { get; set; }

        [Display(Name = "نتیجه تعامل")]
        public byte? Result { get; set; }

        [Display(Name = "طرف حساب")]
        public int? StakeholderId { get; set; }

        [Display(Name = "شعبه")]
        public int? BranchId { get; set; }

        [Display(Name = "از تاریخ")]
        public string? FromDate { get; set; }

        [Display(Name = "تا تاریخ")]
        public string? ToDate { get; set; }

        [Display(Name = "فقط فعال")]
        public bool OnlyActive { get; set; } = true;

        [Display(Name = "فقط در انتظار پیگیری")]
        public bool OnlyPendingFollowUp { get; set; }
    }
}