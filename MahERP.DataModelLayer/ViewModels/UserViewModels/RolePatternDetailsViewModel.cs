using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    public class RolePatternDetailsViewModel
    {
        public int Id { get; set; }

        public int RolePatternId { get; set; }

        [Display(Name = "کنترلر")]
        public string ControllerName { get; set; }

        [Display(Name = "عملیات")]
        public string ActionName { get; set; }

        [Display(Name = "مشاهده")]
        public bool CanRead { get; set; }

        [Display(Name = "ایجاد")]
        public bool CanCreate { get; set; }

        [Display(Name = "ویرایش")]
        public bool CanEdit { get; set; }

        [Display(Name = "حذف")]
        public bool CanDelete { get; set; }

        [Display(Name = "تایید")]
        public bool CanApprove { get; set; }

        [Display(Name = "سطح دسترسی داده")]
        public byte DataAccessLevel { get; set; }

        [Display(Name = "سطح دسترسی داده")]
        public string DataAccessLevelText { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        // نمایش نام کنترلر و اکشن به فارسی
        public string ControllerDisplayName { get; set; }
        public string ActionDisplayName { get; set; }
    }
}