using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.UserViewModels
{
    /// <summary>
    /// مدل ارسال پیام به کاربر (تلگرام، ایمیل، SMS)
    /// </summary>
    public class SendMessageToUserViewModel
    {
        /// <summary>
        /// شناسه کاربر دریافت کننده
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// نام کامل کاربر دریافت کننده
        /// </summary>
        public string UserFullName { get; set; }
        
        /// <summary>
        /// نام کاربری دریافت کننده
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// ایمیل کاربر (در صورت وجود)
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// شماره تلفن کاربر
        /// </summary>
        public string? PhoneNumber { get; set; }
        
        /// <summary>
        /// چت آی دی تلگرام کاربر (در صورت وجود)
        /// </summary>
        public long? TelegramChatId { get; set; }
        
        /// <summary>
        /// ⭐⭐⭐ شناسه قالب انتخاب شده (اختیاری)
        /// </summary>
        public int? TemplateId { get; set; }
        
        /// <summary>
        /// عنوان پیام (برای ایمیل)
        /// </summary>
        [Display(Name = "عنوان پیام")]
        [MaxLength(200, ErrorMessage = "عنوان پیام حداکثر 200 کاراکتر")]
        public string? Subject { get; set; }
        
        /// <summary>
        /// متن پیام
        /// </summary>
        [Display(Name = "متن پیام")]
        [MaxLength(2000, ErrorMessage = "متن پیام حداکثر 2000 کاراکتر")]
        public string? Message { get; set; }
        
        /// <summary>
        /// ارسال از طریق تلگرام
        /// </summary>
        [Display(Name = "ارسال از طریق تلگرام")]
        public bool SendViaTelegram { get; set; }
        
        /// <summary>
        /// ارسال از طریق ایمیل
        /// </summary>
        [Display(Name = "ارسال از طریق ایمیل")]
        public bool SendViaEmail { get; set; }
        
        /// <summary>
        /// ارسال از طریق SMS (آماده برای آینده)
        /// </summary>
        [Display(Name = "ارسال از طریق پیامک")]
        public bool SendViaSms { get; set; }
    }
}
