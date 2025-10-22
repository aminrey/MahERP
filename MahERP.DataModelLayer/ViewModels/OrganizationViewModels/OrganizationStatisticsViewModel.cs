using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای نمایش آمار یک سازمان
    /// </summary>
    public class OrganizationStatisticsViewModel
    {
        [Display(Name = "تعداد کل بخش‌ها")]
        public int TotalDepartments { get; set; }

        [Display(Name = "تعداد بخش‌های فعال")]
        public int ActiveDepartments { get; set; }

        [Display(Name = "تعداد کل اعضا")]
        public int TotalMembers { get; set; }

        [Display(Name = "تعداد اعضای فعال")]
        public int ActiveMembers { get; set; }

        [Display(Name = "تعداد سمت‌ها")]
        public int TotalPositions { get; set; }

        // ⭐⭐⭐ اضافه شده - property های گم‌شده
        [Display(Name = "تعداد افراد مرتبط")]
        public int TotalRelatedContacts { get; set; }

        /// <summary>
        /// ⭐ اضافه شده
        /// </summary>
        [Display(Name = "تعداد کل Contacts")]
        public int TotalContacts { get; set; }

        /// <summary>
        /// ⭐ اضافه شده
        /// </summary>
        [Display(Name = "تعداد کارمندان تمام‌وقت")]
        public int TotalEmployees { get; set; }

        /// <summary>
        /// ⭐ اضافه شده
        /// </summary>
        [Display(Name = "تعداد مشتریان")]
        public int TotalCustomers { get; set; }

        [Display(Name = "عمیق‌ترین سطح سلسله مراتب")]
        public int MaxDepartmentDepth { get; set; }
    }

        /// <summary>
        /// آمار کلی گروه‌های سازمان‌ها
        /// </summary>
        public class OrganizationGroupStatisticsViewModel
        {
            /// <summary>
            /// کل تعداد گروه‌ها
            /// </summary>
            public int TotalGroups { get; set; }

            /// <summary>
            /// تعداد گروه‌های فعال
            /// </summary>
            public int ActiveGroups { get; set; }

            /// <summary>
            /// کل تعداد سازمان‌ها در سیستم
            /// </summary>
            public int TotalOrganizations { get; set; }

            /// <summary>
            /// تعداد سازمان‌هایی که حداقل در یک گروه هستند
            /// </summary>
            public int GroupedOrganizations { get; set; }

            /// <summary>
            /// درصد سازمان‌های گروه‌بندی شده
            /// </summary>
            public decimal GroupedPercentage => TotalOrganizations > 0
                ? Math.Round((decimal)GroupedOrganizations / TotalOrganizations * 100, 2)
                : 0;
        }
    
}