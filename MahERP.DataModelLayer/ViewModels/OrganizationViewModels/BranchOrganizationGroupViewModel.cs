using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای گروه سازمان‌های شعبه
    /// </summary>
    public class BranchOrganizationGroupViewModel
    {
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Display(Name = "نام شعبه")]
        public string? BranchName { get; set; }

        [Required(ErrorMessage = "کد گروه الزامی است")]
        [Display(Name = "کد گروه")]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "عنوان گروه الزامی است")]
        [Display(Name = "عنوان گروه")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "رنگ")]
        [MaxLength(7)]
        public string? ColorHex { get; set; }

        [Display(Name = "آیکون")]
        [MaxLength(50)]
        public string? IconClass { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تعداد اعضا")]
        public int MembersCount { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public string? CreatedDatePersian { get; set; }
    }
}