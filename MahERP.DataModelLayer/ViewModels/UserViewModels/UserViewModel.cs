using MahERP.DataModelLayer.Entities.AcControl;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    public class AddUserViewModel
    {
        [Display(Name = "نام")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        [StringLength(maximumLength: 100, MinimumLength = 2, ErrorMessage = "{0} باید حداقل 2 و حداکثر 100 کاراکتر باشد.")]
        [RegularExpression(@"^[^\\/:*;\.\)\(]+$", ErrorMessage = "از کاراکترهای غیرمجاز استفاده نکنید.")]
        public string FirstName { get; set; }// نام

        [Display(Name = " نام خانوادگی")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        [StringLength(maximumLength: 100, MinimumLength = 2, ErrorMessage = "{0} باید حداقل 2 و حداکثر 100 کاراکتر باشد.")]
        [RegularExpression(@"^[^\\/:*;\.\)\(]+$", ErrorMessage = "از کاراکترهای غیرمجاز استفاده نکنید.")]
        public string LastName { get; set; }// نام

        [Display(Name = "نام کاربری")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        [StringLength(maximumLength: 40, MinimumLength = 4, ErrorMessage = "{0} باید حداقل 4 و حداکثر 40 کاراکتر باشد.")]
        [RegularExpression(@"^[^\\/:*;\.\)\(]+$", ErrorMessage = "از کاراکترهای غیرمجاز استفاده نکنید.")]
        public string UserName { get; set; }

        [Display(Name = "ایمیل")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        //[StringLength(maximumLength: 40, MinimumLength = 4, ErrorMessage = "{0} باید حداقل 4 و حداکثر 40 کاراکتر باشد.")]
        //[RegularExpression(@"^[^\\/:*;\)\(]+$", ErrorMessage = "از کاراکترهای غیرمجاز استفاده نکنید.")]
        public string? Email { get; set; }

        [Display(Name = "شماره همراه")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        [StringLength(maximumLength: 11, MinimumLength = 11, ErrorMessage = "{0} باید 11  کاراکتر باشد.")]
        [RegularExpression(@"^[^\\/:*;\.\)\(]+$", ErrorMessage = "از کاراکترهای غیرمجاز استفاده نکنید.")]
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }

        public DateTime RegisterDate { get; set; }

        [Display(Name = "کد ملی")]
        public string? MelliCode { get; set; }

        [Display(Name = "آدرس")]
        public string? Address { get; set; }
        public string? CompanyName { get; set; }

        public byte? Gender { get; set; } //جنسیت

        [Display(Name = "شهر")]
        public string? City { get; set; }


        [Display(Name = "استان")]
        public string? Province { get; set; }
        public string? PositionName { get; set; }

        [Display(Name = "چت آی دی تلگرام")]
        [RegularExpression(@"^\d+$", ErrorMessage = "چت آی دی باید عدد باشد")]
        public long? TelegramChatId { get; set; }

    }

    public class EditUserViewModel : AddUserViewModel
    {
        public string Id { get; set; }

    }


    public class LoginViewModel
    {
        [Display(Name = "نام کاربری")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "نام کاربری وارد نشده است")]
        [RegularExpression(@"^[^\\/:*;\.\)\(]+$", ErrorMessage = "از کاراکترهای غیرمجاز استفاده نکنید.")]
        public string UserName { get; set; }


        [Display(Name = "رمز عبور")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "رمز عبور وارد نشده است")]
        public string Password { get; set; }
    }

    public class LoginPageViewModel
    {
        public string? userName { get; set; }
        public string? url { get; set; }
        public string? message { get; set; }
        public bool staylogin { get; set; }
        public string? token { get; set; }
        public AppUsers? user { get; set; }

    }

    public class ChangePasswordByAdminViewModel
    {
        [Display(Name = "رمز عبور جدید")]
        public string NewPassword { get; set; }

        [Display(Name = "تکرار رمز عبور جدید")]
        public string ConfirmNewPassword { get; set; }

        public string userId { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        public string Email { get; set; }
        public string Url { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن متفاوت هستند.")]
        public string ConfirmPassword { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }


    public class UserViewModelFull : EditUserViewModel
    {
        public string? FullNamesString { get; set; }
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// مسیر تصویر پروفایل کاربر
        /// </summary>
        public string? ProfileImagePath { get; set; }
    }
    /// <summary>
    /// مدل تغییر رمز عبور کاربر
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور فعلی")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "رمز عبور حداقل باید 6 کاراکتر باشد")]
        [Display(Name = "رمز عبور جدید")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "تکرار رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "رمز عبور جدید و تکرار آن باید یکسان باشند")]
        [Display(Name = "تکرار رمز عبور جدید")]
        public string ConfirmPassword { get; set; }
    }
}
