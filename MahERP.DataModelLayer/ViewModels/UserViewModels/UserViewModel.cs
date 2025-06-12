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
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        [StringLength(maximumLength: 40, MinimumLength = 4, ErrorMessage = "{0} باید حداقل 4 و حداکثر 40 کاراکتر باشد.")]
        [RegularExpression(@"^[^\\/:*;\)\(]+$", ErrorMessage = "از کاراکترهای غیرمجاز استفاده نکنید.")]
        public string Email { get; set; }

        [Display(Name = "شماره همراه")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        [StringLength(maximumLength: 11, MinimumLength = 11, ErrorMessage = "{0} باید 11  کاراکتر باشد.")]
        [RegularExpression(@"^[^\\/:*;\.\)\(]+$", ErrorMessage = "از کاراکترهای غیرمجاز استفاده نکنید.")]
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
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

    public class UserProfile
    {
        public string? Id { get; set; }// Id
        public string? FirstName { get; set; }// نام   
        public string? LastName { get; set; }// نام خانوادگی
        public string? Email { get; set; }    
        public string? PhoneNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public string? UserName { get; set; }
        public string? City { get; set; }
        public DateTime RegisterDate { get; set; }//تاریخ عضویت
        public bool? IsAdmin { get; set; }
    }

    public class EditProfileViewModel
    {
        [Display(Name = "نام")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        public string? FirstName { get; set; }// نام

        [Display(Name = " نام خانوادگی")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        public string? LastName { get; set; }// نام
        public string? Email { get; set; }
        public int? date { get; set; }
        public int? month { get; set; }
        public int? year { get; set; }
        public byte gender { get; set; }    

    }
    
    public class AddressViewModel
    {
        public int id { get; set; }

        [Display(Name = "نام")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        public string rFirstName { get; set; }

        [Display(Name = " نام خانوادگی")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        public string rLastName { get; set; }

        [Display(Name = "شماره همراه")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        public string rPhoneNumber { get; set; }

        public int province { get; set; }
        public int city { get; set; }

        public string? postalCode { get; set; }

        public bool defaultAd { get; set; }

        public string? vahed { get; set; }

        [Display(Name = "شماره پلاک")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        public string pelak { get; set; }

        [Display(Name = "آدرس")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} وارد نشده است.")]
        public string address { get; set; }

    }

}
