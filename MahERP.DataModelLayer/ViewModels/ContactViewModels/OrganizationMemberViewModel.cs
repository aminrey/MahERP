using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.ContactViewModels
{
    /// <summary>
    /// ViewModel برای نمایش عضو سازمان با سمت و بخش
    /// </summary>
    public class OrganizationMemberViewModel
    {
        /// <summary>
        /// شناسه عضو
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// شناسه فرد (Contact)
        /// </summary>
        public int ContactId { get; set; }

        /// <summary>
        /// نام کامل فرد
        /// </summary>
        [Display(Name = "نام و نام خانوادگی")]
        public string ContactFullName { get; set; }

        /// <summary>
        /// کد ملی فرد
        /// </summary>
        [Display(Name = "کد ملی")]
        public string? ContactNationalCode { get; set; }

        /// <summary>
        /// شماره تماس فرد
        /// </summary>
        [Display(Name = "تلفن")]
        public string? ContactPhone { get; set; }

        /// <summary>
        /// شناسه سازمان
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// نام سازمان
        /// </summary>
        [Display(Name = "سازمان")]
        public string? OrganizationName { get; set; }

        /// <summary>
        /// شناسه بخش
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// نام بخش
        /// </summary>
        [Display(Name = "بخش")]
        public string? DepartmentName { get; set; }

        /// <summary>
        /// شناسه سمت
        /// </summary>
        public int? PositionId { get; set; }

        /// <summary>
        /// عنوان سمت
        /// </summary>
        [Display(Name = "سمت")]
        public string? PositionTitle { get; set; }

        /// <summary>
        /// نوع رابطه با سازمان
        /// </summary>
        [Display(Name = "نوع رابطه")]
        public byte RelationType { get; set; }

        /// <summary>
        /// آیا این عضو رابط اصلی است؟
        /// </summary>
        [Display(Name = "رابط اصلی")]
        public bool IsPrimary { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        // Computed Properties

        [Display(Name = "نوع رابطه")]
        public string RelationTypeText => RelationType switch
        {
            0 => "کارمند",
            1 => "مدیر",
            2 => "نماینده",
            3 => "تصمیم‌گیرنده",
            4 => "رابط",
            _ => "نامشخص"
        };
    }
}
