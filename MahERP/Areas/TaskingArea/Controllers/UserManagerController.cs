using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using MahERP.Areas.TaskingArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using Telegram.Bot;

namespace MahERP.Areas.TaskingArea.Controllers.UserControllers
{
    [Area("TaskingArea")]
    //[PermissionRequired("CORE.USER")]
    public class UserManagerController : BaseController
    {
        private readonly IUnitOfWork _Context;
        private readonly UserManager<AppUsers> _UserManager;
        private readonly IMapper _Mapper;
        private readonly IUserManagerRepository _UserRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserManagerController(
            IUnitOfWork context, 
            UserManager<AppUsers> userManager, 
            IMapper Mapper,
            IUserManagerRepository userrepository, // تصحیح نوع پارامتر
            PersianDateHelper persianDateHelper, 
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger, IWebHostEnvironment webHostEnvironment, IBaseRepository BaseRepository, ModuleTrackingBackgroundService moduleTracking, IModuleAccessService moduleAccessService)


 : base(context, userManager, persianDateHelper, memoryCache, activityLogger, userrepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _Context = context;
            _UserManager = userManager;
            _Mapper = Mapper;
            _UserRepository = userrepository; // ✅ اصلاح شده
            _webHostEnvironment = webHostEnvironment;

        }

        /// <summary>
        /// صفحه پروفایل کاربر جاری - مشاهده و ویرایش اطلاعات شخصی
        /// </summary>
        /// <returns>صفحه پروفایل کاربر</returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var currentUser = await _UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.View,
                        "UserManager",
                        "Profile",
                        "تلاش برای مشاهده پروفایل کاربر غیرموجود"
                    );
                    return RedirectToAction("ErrorView", "Home");
                }

                // دریافت اطلاعات کامل کاربر
                var userProfile = _UserRepository.GetUserInfoData(currentUser.Id);
                if (userProfile == null)
                {
                    userProfile = new UserViewModelFull
                    {
                        Id = currentUser.Id,
                        FirstName = currentUser.FirstName,
                        LastName = currentUser.LastName,
                        Email = currentUser.Email,
                        PhoneNumber = currentUser.PhoneNumber,
                        UserName = currentUser.UserName,
                        CompanyName = currentUser.CompanyName,
                        PositionName = currentUser.PositionName,
                        City = currentUser.City,
                        Province = currentUser.Province,
                        Address = currentUser.Address,
                        MelliCode = currentUser.MelliCode,
                        Gender = currentUser.Gender,
                        IsActive = currentUser.IsActive,
                        ProfileImagePath = currentUser.ProfileImagePath,
                        RegisterDate = currentUser.RegisterDate
                    };
                }

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "Profile",
                    $"مشاهده پروفایل شخصی: {currentUser.FirstName} {currentUser.LastName}",
                    recordId: currentUser.Id,
                    entityType: "AppUsers",
                    recordTitle: $"{currentUser.FirstName} {currentUser.LastName}"
                );

                return View(userProfile);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "Profile",
                    "خطا در نمایش پروفایل کاربر",
                    ex
                );

                return RedirectToAction("ErrorView", "Home");
            }
        }
        /// <summary>
        /// ویرایش اطلاعات پروفایل کاربر جاری
        /// </summary>
        /// <param name="model">مدل اطلاعات کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserViewModelFull model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _UserManager.GetUserAsync(User);
                    if (currentUser == null || currentUser.Id != model.Id)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "UpdateProfile",
                            "تلاش برای ویرایش پروفایل کاربر غیرمجاز"
                        );

                        var errorMessages = ResponseMessage.CreateErrorResponse("دسترسی غیرمجاز");
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                        return RedirectToAction("Profile");
                    }

                    // ذخیره مقادیر قبلی برای لاگ
                    var oldValues = new
                    {
                        currentUser.FirstName,
                        currentUser.LastName,
                        currentUser.Email,
                        currentUser.PhoneNumber,
                        currentUser.CompanyName,
                        currentUser.PositionName,
                        currentUser.City,
                        currentUser.Province,
                        currentUser.Address,
                        currentUser.MelliCode,
                        currentUser.Gender,
                        currentUser.ProfileImagePath,
                        currentUser.TelegramChatId
                    };

                    // به‌روزرسانی اطلاعات
                    currentUser.FirstName = model.FirstName;
                    currentUser.LastName = model.LastName;
                    currentUser.Email = model.Email;
                    currentUser.PhoneNumber = model.PhoneNumber;
                    currentUser.CompanyName = model.CompanyName;
                    currentUser.PositionName = model.PositionName;
                    currentUser.City = model.City;
                    currentUser.Province = model.Province;
                    currentUser.Address = model.Address;
                    currentUser.MelliCode = model.MelliCode;
                    currentUser.ProfileImagePath = model.ProfileImagePath;
                    currentUser.Gender = model.Gender ?? 1;

                    // فقط در صورتی که TelegramChatId قبلاً ثبت نشده باشد، امکان تغییر وجود دارد
                    if (!currentUser.TelegramChatId.HasValue )
                    {
                        currentUser.TelegramChatId = model.TelegramChatId;
                    }

                    var result = await _UserManager.UpdateAsync(currentUser);
                    if (result.Succeeded)
                    {
                        // مقادیر جدید برای لاگ
                        var newValues = new
                        {
                            currentUser.FirstName,
                            currentUser.LastName,
                            currentUser.Email,
                            currentUser.PhoneNumber,
                            currentUser.CompanyName,
                            currentUser.PositionName,
                            currentUser.City,
                            currentUser.Province,
                            currentUser.Address,
                            currentUser.MelliCode,
                            currentUser.Gender,
                            currentUser.ProfileImagePath,
                            currentUser.TelegramChatId
                        };

                        // ثبت لاگ تغییرات
                        await _activityLogger.LogChangeAsync<object>(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "UpdateProfile",
                            $"به‌روزرسانی پروفایل شخصی: {currentUser.FirstName} {currentUser.LastName}",
                            oldValues,
                            newValues,
                            recordId: currentUser.Id,
                            entityType: "AppUsers",
                            recordTitle: $"{currentUser.FirstName} {currentUser.LastName}"
                        );

                        var successMessages = ResponseMessage.CreateSuccessResponse("اطلاعات با موفقیت به‌روزرسانی شد");
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(successMessages);
                        return RedirectToAction("Profile");
                    }
                    else
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "UpdateProfile",
                            $"شکست در ویرایش پروفایل: {string.Join(", ", result.Errors.Select(e => e.Description))}",
                            recordId: currentUser.Id
                        );

                        var errorMessages = ResponseMessage.CreateErrorResponse(result.Errors.Select(e => e.Description).ToArray());
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                        return View("Profile", model);
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "UserManager",
                        "UpdateProfile",
                        "خطا در به‌روزرسانی پروفایل",
                        ex,
                        recordId: model.Id
                    );

                    var errorMessages = ResponseMessage.CreateErrorResponse("خطا در به‌روزرسانی اطلاعات");
                    TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                    return View("Profile", model);
                }
            }

            var modelErrors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                .ToArray();

            if (modelErrors.Any())
            {
                ModelState.Clear();
                var errorMessages = ResponseMessage.CreateErrorResponse(modelErrors);
                TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
            }

            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.Edit,
                "UserManager",
                "UpdateProfile",
                $"تلاش ناموفق برای به‌روزرسانی پروفایل - خطاهای validation: {string.Join(", ", modelErrors)}",
                recordId: model.Id
            );

            return View("Profile", model);
        }
        /// <summary>
        /// تغییر رمز عبور کاربر جاری
        /// </summary>
        /// <param name="model">مدل تغییر رمز عبور</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _UserManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "ChangePassword",
                            "تلاش برای تغییر رمز عبور کاربر غیرموجود"
                        );

                        var errorMessages = ResponseMessage.CreateErrorResponse("کاربر یافت نشد");
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                        return RedirectToAction("Profile");
                    }

                    var result = await _UserManager.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        // ثبت لاگ تغییر رمز عبور موفق
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "ChangePassword",
                            $"تغییر رمز عبور: {currentUser.FirstName} {currentUser.LastName}",
                            recordId: currentUser.Id,
                            entityType: "AppUsers",
                            recordTitle: $"{currentUser.FirstName} {currentUser.LastName}"
                        );

                        var successMessages = ResponseMessage.CreateSuccessResponse("رمز عبور با موفقیت تغییر یافت");
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(successMessages);
                        return RedirectToAction("Profile");
                    }
                    else
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Edit,
                            "UserManager",
                            "ChangePassword",
                            $"شکست در تغییر رمز عبور: {string.Join(", ", result.Errors.Select(e => e.Description))}",
                            recordId: currentUser.Id
                        );

                        var errorMessages = ResponseMessage.CreateErrorResponse(result.Errors.Select(e => e.Description).ToArray());
                        TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                        return RedirectToAction("Profile");
                    }
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync(
                        "UserManager",
                        "ChangePassword",
                        "خطا در تغییر رمز عبور",
                        ex
                    );

                    var errorMessages = ResponseMessage.CreateErrorResponse("خطا در تغییر رمز عبور");
                    TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
                    return RedirectToAction("Profile");
                }
            }

            // در صورت نامعتبر بودن ModelState
            var modelErrors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                .ToArray();

            if (modelErrors.Any())
            {
                // پاک کردن ModelState تا خطاهای مکرر نمایش داده نشوند
                ModelState.Clear();

                var errorMessages = ResponseMessage.CreateErrorResponse(modelErrors);
                TempData["ResponseMessages"] = System.Text.Json.JsonSerializer.Serialize(errorMessages);
            }

            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.Edit,
                "UserManager",
                "ChangePassword",
                $"تلاش ناموفق برای تغییر رمز عبور - خطاهای validation: {string.Join(", ", modelErrors)}"
            );

            return RedirectToAction("Profile");
        }

        /// <summary>
        /// آپلود تصویر پروفایل کاربر
        /// </summary>
        /// <param name="profileImage">فایل تصویر</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            try
            {
                var currentUser = await _UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { status = "error", message = "کاربر یافت نشد" });
                }

                if (profileImage == null || profileImage.Length == 0)
                {
                    return Json(new { status = "error", message = "فایلی انتخاب نشده است" });
                }

                // بررسی نوع فایل
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(profileImage.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new { status = "error", message = "فرمت تصویر مجاز نیست" });
                }

                // بررسی حجم فایل (5MB)
                if (profileImage.Length > 5 * 1024 * 1024)
                {
                    return Json(new { status = "error", message = "حجم تصویر نباید بیشتر از 5 مگابایت باشد" });
                }

                // حذف تصویر قبلی در صورت وجود
                if (!string.IsNullOrEmpty(currentUser.ProfileImagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, currentUser.ProfileImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // ایجاد مسیر ذخیره‌سازی
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // ایجاد نام فایل یکتا
                var uniqueFileName = $"{currentUser.Id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                // ذخیره فایل
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                // به‌روزرسانی مسیر در دیتابیس
                var relativePath = $"/uploads/profiles/{uniqueFileName}";
                currentUser.ProfileImagePath = relativePath;

                var result = await _UserManager.UpdateAsync(currentUser);

                if (result.Succeeded)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "UserManager",
                        "UploadProfileImage",
                        $"آپلود تصویر پروفایل: {currentUser.FirstName} {currentUser.LastName}",
                        recordId: currentUser.Id
                    );

                    return Json(new { status = "success", message = "تصویر با موفقیت آپلود شد", imagePath = relativePath });
                }
                else
                {
                    // حذف فایل در صورت خطا
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    return Json(new { status = "error", message = "خطا در ذخیره تصویر در دیتابیس" });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "UploadProfileImage",
                    "خطا در آپلود تصویر پروفایل",
                    ex
                );

                return Json(new { status = "error", message = $"خطا در آپلود تصویر: {ex.Message}" });
            }
        }

       
        /// <summary>
        /// ثبت دستی Chat ID تلگرام برای کاربر جاری
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterTelegramChatId([FromBody] TelegramChatIdRequest request)
        {
            try
            {
                // اعتبارسنجی ورودی
                if (request == null || request.ChatId == 0)
                {
                    return Json(new { success = false, message = "Chat ID وارد نشده است" });
                }

                // دریافت کاربر جاری
                var currentUser = await _UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "کاربر یافت نشد" });
                }

                // بررسی اینکه Chat ID قبلاً ثبت نشده باشد
                if (currentUser.TelegramChatId.HasValue)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = "Chat ID قبلاً ثبت شده است",
                        alreadyRegistered = true,
                        userName = $"{currentUser.FirstName} {currentUser.LastName}"
                    });
                }

                // بررسی اینکه Chat ID توسط کاربر دیگری استفاده نشده باشد
                var existingUser =  _UserManager.Users
                    .FirstOrDefault(u => u.TelegramChatId == request.ChatId && u.Id != currentUser.Id);

                if (existingUser != null)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = "این Chat ID قبلاً توسط کاربر دیگری ثبت شده است"
                    });
                }

                // ذخیره مقدار قبلی برای لاگ
                var oldChatId = currentUser.TelegramChatId;

                // ⭐ ثبت Chat ID
                currentUser.TelegramChatId = request.ChatId;

                // ⭐ به‌روزرسانی در دیتابیس
                var result = await _UserManager.UpdateAsync(currentUser);

                if (result.Succeeded)
                {
                    // ⭐ COMMIT تغییرات
                    await _Context.SaveAsync();

                    // ثبت لاگ تغییرات
                    await _activityLogger.LogChangeAsync<object>(
                        ActivityTypeEnum.Edit,
                        "UserManager",
                        "RegisterTelegramChatId",
                        $"ثبت دستی Chat ID تلگرام: {currentUser.FirstName} {currentUser.LastName}",
                        new { TelegramChatId = oldChatId },
                        new { TelegramChatId = request.ChatId },
                        recordId: currentUser.Id,
                        entityType: "AppUsers",
                        recordTitle: $"{currentUser.FirstName} {currentUser.LastName}"
                    );

                    return Json(new 
                    { 
                        success = true, 
                        message = "Chat ID با موفقیت ثبت شد",
                        userName = $"{currentUser.FirstName} {currentUser.LastName}",
                        chatId = request.ChatId
                    });
                }
                else
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = "خطا در ثبت Chat ID: " + string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "RegisterTelegramChatId",
                    "خطا در ثبت Chat ID تلگرام",
                    ex
                );

                return Json(new { success = false, message = $"خطای سیستمی: {ex.Message}" });
            }
        }
        /// <summary>
        /// دریافت لینک ربات تلگرام از تنظیمات
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTelegramBotLink()
        {
            try
            {
                // ⭐ دریافت Bot Token از Settings
                var settings = _Context.SettingsUW.Get().FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.TelegramBotToken))
                {
                    return Json(new
                    {
                        success = false,
                        message = "ربات تلگرام در تنظیمات سیستم ثبت نشده است"
                    });
                }

                // استخراج Bot Username از API تلگرام
                var botUsername = await GetBotUsernameFromToken(settings.TelegramBotToken);

                if (string.IsNullOrEmpty(botUsername))
                {
                    return Json(new
                    {
                        success = false,
                        message = "خطا در دریافت اطلاعات ربات از تلگرام"
                    });
                }

                // ایجاد لینک ربات
                var botLink = $"https://t.me/{botUsername}";

                // ثبت لاگ
                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "UserManager",
                    "GetTelegramBotLink",
                    $"دریافت لینک ربات تلگرام: @{botUsername}"
                );

                return Json(new
                {
                    success = true,
                    botLink = botLink,
                    botUsername = botUsername
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "GetTelegramBotLink",
                    "خطا در دریافت لینک ربات",
                    ex
                );

                return Json(new { success = false, message = "خطای سیستمی در دریافت لینک ربات" });
            }
        }

        /// <summary>
        /// دریافت Username ربات از Telegram API با استفاده از توکن
        /// </summary>
        private async Task<string> GetBotUsernameFromToken(string botToken)
        {
            try
            {
                var botClient = new TelegramBotClient(botToken);
                var botInfo = await botClient.GetMe();
                return botInfo.Username;
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "UserManager",
                    "GetBotUsernameFromToken",
                    "خطا در دریافت اطلاعات ربات از API تلگرام",
                    ex
                );
                return null;
            }
        }
        // ViewModel برای درخواست ثبت Chat ID
        public class TelegramChatIdRequest
        {
            public long ChatId { get; set; }
            public string Token { get; set; } // UserId کاربر
        }
    }
}
