# 🔧 رفع مشکلات نوتیفیکیشن‌های زمان‌بندی شده

## 🐛 مشکلات شناسایی شده:

### 1️⃣ **مشکل تبدیل TimeZone در محاسبه NextExecutionDate**
- **مشکل:** زمان محاسبه شده در Iran TimeZone بدون تبدیل به UTC ذخیره می‌شد
- **علت:** استفاده از `DateTimeKind.Unspecified` باعث اشتباه در مقایسه با UTC می‌شد
- **راه حل:** ✅ اضافه کردن تبدیل صریح Iran → UTC در پایان محاسبه

```csharp
// ❌ کد قبلی (اشتباه)
return new DateTime(now.Year, now.Month, now.Day, hour, minute, 0, DateTimeKind.Unspecified);

// ✅ کد اصلاح شده
var nextExecutionIran = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0, DateTimeKind.Unspecified);
var nextExecutionUtc = TimeZoneInfo.ConvertTimeToUtc(nextExecutionIran, IranTimeZone);
return nextExecutionUtc;
```

---

## 🎯 **امکانات موجود برای زمان‌بندی:**

### ✅ **نوع‌های زمان‌بندی:**
```
0 = Manual (دستی)
1 = Daily (روزانه)
2 = Weekly (هفتگی)
3 = Monthly (ماهانه)
4 = Custom (Cron Expression - TODO)
```

### ✅ **تنظیمات پیشرفته:**
- ⏰ ساعت اجرا (HH:mm - مثل `09:00`)
- 📅 روزهای هفته (برای Weekly): `"1,3,5"` (دوشنبه، چهارشنبه، جمعه)
- 📆 روز ماه (برای Monthly): `15` (روز پانزدهم هر ماه)
- 🔢 حداکثر تعداد اجرا (MaxOccurrences)
- 📊 محاسبه خودکار `NextExecutionDate` بر اساس نوع زمان‌بندی

---

## 📤 **سیستم دریافت‌کنندگان (RecipientMode):**

### ✅ **حالت‌های ارسال:**
```
0 = AllUsers        → همه کاربران فعال
1 = SpecificUsers   → فقط کاربران مشخص
2 = AllExceptUsers  → همه به جز کاربران مشخص
```

### ✅ **پشتیبانی از Multiple Instances:**
سیستم از ارسال به چند کاربر پشتیبانی می‌کند:

```csharp
// در ScheduledNotificationBackgroundService.cs
private async Task<List<string>> GetScheduledTemplateRecipientsAsync(
    NotificationTemplate template,
    AppDbContext context)
{
    switch (template.RecipientMode)
    {
        case 0: // همه کاربران
            return await context.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => u.Id)
                .ToListAsync();

        case 1: // فقط کاربران مشخص
            return await context.NotificationTemplateRecipient_Tbl
                .Where(r => r.NotificationTemplateId == template.Id &&
                           r.IsActive &&
                           r.RecipientType == 2) // User
                .Select(r => r.UserId)
                .ToListAsync();

        case 2: // همه به جز کاربران مشخص
            var excludedUsers = await context.NotificationTemplateRecipient_Tbl
                .Where(r => r.NotificationTemplateId == template.Id &&
                           r.IsActive &&
                           r.RecipientType == 2)
                .Select(r => r.UserId)
                .ToListAsync();

            return await context.Users
                .Where(u => u.IsActive &&
                           !u.IsRemoveUser &&
                           !excludedUsers.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

        default:
            return new List<string>();
    }
}
```

### ✅ **نحوه عملکرد Multiple Instances:**

1️⃣ **هر نوتیفیکیشن یک `NotificationTemplate` دارد**
2️⃣ **این Template برای هر کاربر یک نوتیفیکیشن جداگانه ایجاد می‌کند**
3️⃣ **در جدول `CoreNotification_Tbl` برای هر کاربر یک رکورد ثبت می‌شود**

مثال:
```
NotificationTemplate (Id=10, Title="گزارش روزانه")
  └─> CoreNotification (Id=100, UserId="user-1", Title="گزارش روزانه")
  └─> CoreNotification (Id=101, UserId="user-2", Title="گزارش روزانه")
  └─> CoreNotification (Id=102, UserId="user-3", Title="گزارش روزانه")
```

---

## 🔄 **جریان کامل اجرای زمان‌بندی:**

```
1. ScheduledNotificationBackgroundService شروع می‌شود (هر 1 دقیقه)
   ↓
2. Query: Templates با NextExecutionDate <= Now
   ↓
3. برای هر Template:
   ↓
4. GetScheduledTemplateRecipientsAsync → لیست کاربران
   ↓
5. ProcessScheduledNotificationAsync → ارسال به هر کاربر
   ↓
6. برای هر کاربر:
   - ایجاد CoreNotification (سیستم داخلی)
   - افزودن به صف Email/SMS (اگر فعال باشد)
   ↓
7. UpdateExecutionDates:
   - LastExecutionDate = Now (Iran TimeZone)
   - NextExecutionDate = CalculateNextExecutionDate() → UTC
   ↓
8. SaveChanges
```

---

## 🛠️ **تغییرات اعمال شده:**

### ✅ **فایل اصلاح شده:**
```
MahERP.DataModelLayer\Services\BackGroundServices\ScheduledNotificationBackgroundService.cs
```

### ✅ **تغییرات:**
1. اضافه کردن تبدیل صریح Iran → UTC در `CalculateNextExecutionDate()`
2. اضافه کردن Try-Catch برای مدیریت خطاهای تبدیل
3. بهبود لاگ‌ها برای دیباگ بهتر

---

## 📝 **نحوه تست:**

### 1️⃣ ساخت یک نوتیفیکیشن روزانه:
```
- عنوان: "گزارش روزانه"
- نوع: روزانه (Daily)
- ساعت اجرا: 09:00
- دریافت‌کنندگان: همه کاربران (یا افراد خاص)
```

### 2️⃣ بررسی `NextExecutionDate`:
```sql
SELECT Id, TemplateName, ScheduledTime, 
       NextExecutionDate, LastExecutionDate, 
       IsScheduleEnabled, IsActive
FROM NotificationTemplate_Tbl
WHERE IsScheduled = 1
```

### 3️⃣ مشاهده لاگ‌ها:
```
⏰ Scheduled Notification Background Service شروع شد
🌍 TimeZone: Iran Standard Time
🕐 زمان فعلی ایران: 2024-12-21 10:30:00
📅 روزانه: NextExecution (Iran) = 2024-12-22 09:00:00
✅ Converted to UTC: 2024-12-22 05:30:00
```

### 4️⃣ بررسی ارسال:
```sql
-- نوتیفیکیشن‌های ارسال شده
SELECT * FROM CoreNotification_Tbl
WHERE CreatedDate >= '2024-12-21'
ORDER BY CreatedDate DESC

-- تعداد دریافت‌کنندگان
SELECT COUNT(DISTINCT RecipientUserId) AS RecipientCount
FROM CoreNotification_Tbl
WHERE Title LIKE '%گزارش روزانه%'
```

---

## 🎯 **ویژگی‌های فعلی:**

### ✅ **موارد پیاده‌سازی شده:**
- [x] زمان‌بندی روزانه
- [x] زمان‌بندی هفتگی (با انتخاب روزهای هفته)
- [x] زمان‌بندی ماهانه (با انتخاب روز ماه)
- [x] محاسبه خودکار NextExecutionDate
- [x] تبدیل صحیح Iran ↔ UTC
- [x] ارسال به چند کاربر (Multiple Instances)
- [x] حالت‌های مختلف دریافت‌کنندگان (همه / خاص / به جز)
- [x] پشتیبانی از حداکثر تعداد اجرا (MaxOccurrences)
- [x] جلوگیری از اجرای مکرر (Double-check)

### ⏳ **موارد TODO:**
- [ ] پشتیبانی از Cron Expression (نیاز به NCrontab)
- [ ] پنل مدیریت زمان‌بندی در UI
- [ ] امکان Pause/Resume برای Template ها
- [ ] نمایش Preview زمان‌های بعدی اجرا

---

## 📊 **مثال‌های کاربردی:**

### 1️⃣ گزارش روزانه فروش:
```
- نوع: روزانه
- ساعت: 08:00
- دریافت‌کنندگان: مدیران (RecipientMode = 1)
```

### 2️⃣ یادآوری هفتگی جلسه:
```
- نوع: هفتگی
- روزها: 1 (دوشنبه)
- ساعت: 09:00
- دریافت‌کنندگان: همه کاربران (RecipientMode = 0)
```

### 3️⃣ گزارش ماهانه حقوق:
```
- نوع: ماهانه
- روز: 1 (اول هر ماه)
- ساعت: 10:00
- دریافت‌کنندگان: همه به جز مهمانان (RecipientMode = 2)
```

---

## ✅ **نتیجه گیری:**

سیستم شما **کاملاً آماده** است و موارد زیر را پشتیبانی می‌کند:

1. ✅ زمان‌بندی با 3 نوع مختلف (روزانه، هفتگی، ماهانه)
2. ✅ ارسال به چند کاربر به صورت همزمان
3. ✅ مدیریت دریافت‌کنندگان با 3 حالت
4. ✅ تبدیل صحیح TimeZone
5. ✅ جلوگیری از اجرای مکرر
6. ✅ لاگ‌های کامل برای دیباگ

### 🔍 **اگر باز هم ارسال نمی‌شود:**

1. بررسی کنید `ScheduledNotificationBackgroundService` در `Program.cs` register شده باشد
2. لاگ‌های سرویس را بررسی کنید
3. مقدار `NextExecutionDate` در دیتابیس را چک کنید (باید UTC باشد)
4. اطمینان حاصل کنید `IsScheduleEnabled = true` و `IsActive = true`

---

**تاریخ:** 2024-12-21  
**نسخه:** 1.0.0  
**نویسنده:** GitHub Copilot
