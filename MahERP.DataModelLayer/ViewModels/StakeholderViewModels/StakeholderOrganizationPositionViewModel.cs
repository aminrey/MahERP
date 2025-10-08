using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    public class StakeholderOrganizationPositionViewModel
    {
        public int Id { get; set; }

        public int OrganizationId { get; set; }

        [Display(Name = "عنوان سمت")]
        [Required(ErrorMessage = "عنوان سمت الزامی است")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "سطح قدرت")]
        public int PowerLevel { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}