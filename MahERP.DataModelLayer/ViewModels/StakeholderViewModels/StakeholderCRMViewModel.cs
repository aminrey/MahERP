using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    public class StakeholderCRMViewModel
    {
        public int Id { get; set; }

        public int StakeholderId { get; set; }

        [Display(Name = "منبع آشنایی")]
        public byte LeadSource { get; set; }

        [Display(Name = "مرحله در چرخه فروش")]
        public byte SalesStage { get; set; }

        [Display(Name = "تاریخ آخرین تماس")]
        public DateTime? LastContactDate { get; set; }

        [Display(Name = "ارزش بالقوه (میلیون تومان)")]
        public decimal? PotentialValue { get; set; }

        [Display(Name = "رتبه اعتباری")]
        [MaxLength(1)]
        public string? CreditRating { get; set; }

        [Display(Name = "علاقه‌مندی‌ها و ترجیحات")]
        public string? Preferences { get; set; }

        [Display(Name = "صنعت/حوزه فعالیت")]
        [MaxLength(100)]
        public string? Industry { get; set; }

        [Display(Name = "تعداد کارمندان")]
        public int? EmployeeCount { get; set; }

        [Display(Name = "گردش مالی سالانه (میلیون تومان)")]
        public decimal? AnnualRevenue { get; set; }

        [Display(Name = "کارشناس فروش اختصاصی")]
        public string? SalesRepUserId { get; set; }
        
        [Display(Name = "یادداشت‌های داخلی")]
        public string? InternalNotes { get; set; }
        
        // نمایش منبع آشنایی به صورت متنی
        public string LeadSourceText
        {
            get
            {
                switch (LeadSource)
                {
                    case 0:
                        return "وب سایت";
                    case 1:
                        return "تبلیغات";
                    case 2:
                        return "معرفی";
                    case 3:
                        return "نمایشگاه";
                    case 4:
                        return "تماس مستقیم";
                    case 5:
                        return "سایر";
                    default:
                        return "نامشخص";
                }
            }
        }
        
        // نمایش مرحله چرخه فروش به صورت متنی
        public string SalesStageText
        {
            get
            {
                switch (SalesStage)
                {
                    case 0:
                        return "سرنخ اولیه";
                    case 1:
                        return "مذاکره";
                    case 2:
                        return "پیشنهاد قیمت";
                    case 3:
                        return "در حال قرارداد";
                    case 4:
                        return "مشتری فعال";
                    case 5:
                        return "مشتری غیرفعال";
                    default:
                        return "نامشخص";
                }
            }
        }
    }
}