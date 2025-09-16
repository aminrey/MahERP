# سیستم لاگ فعالیت‌های کاربران - راهنمای استفاده

## معرفی

این سیستم یک راه‌حل کامل و پیشرفته برای ثبت، مدیریت و نظارت بر فعالیت‌های کاربران در سیستم ERP می‌باشد. این سیستم شامل قابلیت‌های زیر است:

- ثبت خودکار تمام فعالیت‌های کاربران
- جستجوی پیشرفته و فیلتر کردن لاگ‌ها
- نمایش آمار و گزارش‌های تفصیلی
- مدیریت آرشیو و نگهداری لاگ‌ها
- نظارت امنیتی و تشخیص فعالیت‌های مشکوک

## ساختار فایل‌ها

### کنترلرها
- `UserActivityLogController.cs` - کنترلر اصلی مدیریت لاگ‌ها

### مدل‌ها و ViewModels
- `LogViewModel.cs` - مدل‌های نمایش لاگ‌ها
- `LogExtendedViewModels.cs` - مدل‌های تکمیلی و آماری

### سرویس‌ها و Repository
- `IUserActivityLogRepository.cs` - رابط مخزن لاگ‌ها
- `UserActivityLogRepository.cs` - پیاده‌سازی مخزن لاگ‌ها
- `ActivityLoggerService.cs` - سرویس کمکی برای ثبت آسان لاگ‌ها

### View ها
- `Index.cshtml` - صفحه اصلی نمایش لاگ‌ها
- `Search.cshtml` - صفحه جستجوی پیشرفته
- `Details.cshtml` - صفحه جزئیات یک لاگ خاص
- `Statistics.cshtml` - صفحه آمار و گزارش‌ها

## نحوه استفاده

### 1. ثبت لاگ ساده

```csharp
// تزریق سرویس در کنترلر
private readonly ActivityLoggerService _activityLogger;

public TaskController(ActivityLoggerService activityLogger)
{
    _activityLogger = activityLogger;
}

// ثبت لاگ ساده
await _activityLogger.LogActivityAsync(
    ActivityTypeEnum.Create,
    "Tasks",
    "CreateTask",
    "ایجاد تسک جدید",
    task.Id.ToString(),
    "Task",
    task.Title
);
```

### 2. ثبت لاگ با تغییرات

```csharp
// ثبت تغییرات
await _activityLogger.LogChangeAsync(
    ActivityTypeEnum.Edit,
    "Tasks",
    "UpdateTask",
    "ویرایش تسک",
    oldTask,
    newTask,
    task.Id.ToString(),
    "Task",
    task.Title
);
```

### 3. ثبت لاگ خطا

```csharp
try
{
    // عملیات کاری
}
catch (Exception ex)
{
    await _activityLogger.LogErrorAsync(
        "Tasks",
        "CreateTask",
        "خطا در ایجاد تسک",
        ex,
        task?.Id.ToString()
    );
    throw;
}
```

### 4. ثبت لاگ ورود/خروج

```csharp
// ورود موفق
await _activityLogger.LogLoginAsync(true, user.UserName);

// ورود ناموفق
await _activityLogger.LogLoginAsync(false, username, "رمز عبور نادرست");

// خروج
await _activityLogger.LogLogoutAsync();
```

## انواع فعالیت‌ها

```csharp
public enum ActivityTypeEnum
{
    View = 0,       // مشاهده
    Create = 1,     // ایجاد
    Edit = 2,       // ویرایش
    Delete = 3,     // حذف
    Approve = 4,    // تایید
    Reject = 5,     // رد
    Login = 6,      // ورود
    Logout = 7,     // خروج
    Download = 8,   // دانلود
    Upload = 9,     // آپلود
    Search = 10,    // جستجو
    Print = 11,     // چاپ
    Email = 12,     // ایمیل
    SMS = 13,       // پیامک
    Error = 99      // خطا
}
```

## وضعیت‌های نتیجه

- `0` - موفق
- `1` - ناموفق
- `2` - خطا
- `3` - دسترسی رد شده

## سطوح اهمیت

- `0` - عادی
- `1` - مهم
- `2` - بحرانی

## مدیریت از طریق رابط کاربری

### 1. مشاهده لاگ‌ها
- مراجعه به `/AdminArea/UserActivityLog`
- مشاهده لیست کامل لاگ‌ها با صفحه‌بندی
- فیلتر سریع بر اساس نوع لاگ

### 2. جستجوی پیشرفته
- مراجعه به `/AdminArea/UserActivityLog/Search`
- جستجو بر اساس کاربر، ماژول، نوع فعالیت، تاریخ و...
- ذخیره و بازیابی جستجوهای محبوب

### 3. مشاهده آمار
- مراجعه به `/AdminArea/UserActivityLog/Statistics`
- مشاهده آمار کلی سیستم
- نمودارهای عملکرد
- گزارش‌های آماری

### 4. مدیریت آرشیو
- مراجعه به `/AdminArea/UserActivityLog/ArchiveManagement`
- آرشیو کردن لاگ‌های قدیمی
- حذف لاگ‌های آرشیو شده
- مدیریت فضای پایگاه داده

## فیلترهای امنیتی

### 1. لاگ‌های حساس
لاگ‌هایی که به صورت خودکار حساس در نظر گرفته می‌شوند:

- تمام عملیات حذف
- تایید/رد عملیات
- تغییرات در ماژول‌های کاربران و مجوزها
- تمام خطاها

### 2. فعالیت‌های مشکوک
- ورودهای ناموفق متعدد
- فعالیت خارج از ساعت کاری
- فعالیت از IP های غیرمعمول

## تنظیمات پیشرفته

### 1. تنظیم در appsettings.json

```json
{
  "ActivityLogging": {
    "IsEnabled": true,
    "RetentionDays": 365,
    "LogNormalActivities": true,
    "LogSensitiveActivities": true,
    "LogErrors": true,
    "MinimumImportanceLevel": 0,
    "ExcludedModules": ["HealthCheck", "Ping"],
    "ExcludedIpAddresses": ["127.0.0.1", "::1"]
  }
}
```

### 2. فعال‌سازی Auto-Logging (اختیاری)

برای ثبت خودکار تمام درخواست‌ها می‌توانید از Middleware استفاده کنید:

```csharp
// در Program.cs
app.UseMiddleware<ActivityLoggingMiddleware>();
```

## نکات مهم

### 1. عملکرد
- تمام عملیات لاگینگ غیرهمزمان (async) هستند
- در صورت خطا در ثبت لاگ، عملکرد اصلی سیستم مختل نمی‌شود
- لاگ‌ها در یک جدول جداگانه ذخیره می‌شوند

### 2. امنیت
- اطلاعات حساس (مثل رمز عبور) هرگز لاگ نمی‌شوند
- لاگ‌ها فقط توسط مدیران سیستم قابل مشاهده هستند
- تمام دسترسی‌ها خودشان لاگ می‌شوند

### 3. نگهداری
- لاگ‌های قدیمی باید به صورت دوره‌ای آرشیو شوند
- پایگاه داده را مرتباً بهینه‌سازی کنید
- از ایندکس‌های مناسب استفاده کنید

## عیب‌یابی رایج

### 1. لاگ‌ها ثبت نمی‌شوند
- بررسی کنید که سرویس‌ها در DI ثبت شده باشند
- اتصال پایگاه داده را بررسی کنید
- مجوزهای کاربر را بررسی کنید

### 2. عملکرد کند
- تعداد لاگ‌های نگهداری شده را کاهش دهید
- از آرشیو استفاده کنید
- ایندکس‌های پایگاه داده را بررسی کنید

### 3. حجم زیاد داده‌ها
- تنظیمات RetentionDays را کاهش دهید
- لاگ‌های غیرضروری را حذف کنید
- از فشرده‌سازی استفاده کنید

## API های موجود

### دریافت آمار
```
GET /AdminArea/UserActivityLog/GetErrorCount?fromDate=2024-01-01&toDate=2024-01-31
GET /AdminArea/UserActivityLog/GetUniqueIps
GET /AdminArea/UserActivityLog/GetLastUserActivity?userId=user123
```

## تماس و پشتیبانی

برای سوالات و مشکلات فنی با تیم توسعه تماس بگیرید.

---

**نسخه:** 1.0.0  
**آخرین بروزرسانی:** خرداد 1403