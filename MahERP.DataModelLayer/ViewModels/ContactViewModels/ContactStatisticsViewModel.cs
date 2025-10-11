using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.ContactViewModels
{
    /// <summary>
    /// ViewModel برای آمار افراد
    /// </summary>
    public class ContactStatisticsViewModel
    {
        [Display(Name = "تعداد کل افراد")]
        public int TotalContacts { get; set; }

        [Display(Name = "تعداد مردان")]
        public int TotalMale { get; set; }

        [Display(Name = "تعداد زنان")]
        public int TotalFemale { get; set; }

        [Display(Name = "افراد دارای ایمیل")]
        public int TotalWithEmail { get; set; }

        [Display(Name = "افراد دارای شماره تماس")]
        public int TotalWithPhone { get; set; }

        [Display(Name = "اعضای بخش‌های سازمانی")]
        public int TotalInDepartments { get; set; }

        [Display(Name = "افراد مرتبط با سازمان‌ها")]
        public int TotalInOrganizations { get; set; }

        // Percentages
        public double MalePercentage => TotalContacts > 0 ? (double)TotalMale / TotalContacts * 100 : 0;
        public double FemalePercentage => TotalContacts > 0 ? (double)TotalFemale / TotalContacts * 100 : 0;
        public double WithEmailPercentage => TotalContacts > 0 ? (double)TotalWithEmail / TotalContacts * 100 : 0;
        public double WithPhonePercentage => TotalContacts > 0 ? (double)TotalWithPhone / TotalContacts * 100 : 0;
    }
}