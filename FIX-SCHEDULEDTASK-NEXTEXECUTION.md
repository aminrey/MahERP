# ✅ الگوریتم محاسبه NextExecutionDate - کپی شده از NotificationTemplateRepository

## 🎯 هدف:
اصلاح الگوریتم محاسبه `NextExecutionDate` در `TaskRepository.ScheduledTasks.cs` با کپی کردن الگوریتم تست شده از `NotificationTemplateRepository.cs`

---

## ✅ تغییرات اعمال شده:

### 1️⃣ **فایل: `ConvertDateTime.cs`**
- ✅ اضافه شدن متد `ConvertShamsiToMiladiNullable(string? date)` برای تبدیل ایمن null
- ✅ بهبود متد `ConvertMiladiToShamsi` برای پشتیبانی از nullable DateTime

### 2️⃣ **فایل: `TaskRepository.ScheduledTasks.cs`**

#### متد `CalculateNextExecutionDate`:
```csharp
✅ استفاده از IranTimeZone برای محاسبه دقیق
✅ اعتبارسنجی ساعت (0-23) و دقیقه (0-59)
✅ پشتیبانی از OneTime با OneTimeExecutionDatePersian
✅ FIX روزانه: اگر امروز گذشت، حتماً فردا
✅ FIX هفتگی: بررسی امروز + 7 روز آینده
✅ FIX ماهانه: پشتیبانی از چند روز (ScheduledDaysOfMonth)
✅ تبدیل صحیح Iran Time → UTC
```

#### متد `FindNextWeeklyExecution`:
```csharp
✅ چک کردن امروز (اگر ساعت نگذشته)
✅ جستجوی 7 روز آینده
✅ بازگشت نزدیک‌ترین روز انتخاب شده
```

#### متد `FindNextMonthlyExecution`:
```csharp
✅ بررسی ماه جاری
✅ اگر گذشت، ماه بعد
✅ مدیریت ماه‌های 28/29/30/31 روزه
```

#### متد `FindNextMonthlyMultipleDaysExecution`:
```csharp
✅ بررسی ماه جاری (روزهای باقی مانده)
✅ اگر نبود، ماه بعد
✅ اگر در ماه بعد هم نبود، 2 ماه بعد
✅ مدیریت ماه‌های کوتاه (مثلاً 31 فوریه → اولین روز موجود)
```

#### متد `UpdateScheduledTaskAsync`:
```csharp
✅ استفاده از ConvertShamsiToMiladiNullable
✅ محاسبه مجدد NextExecutionDate با الگوریتم جدید
```

#### متد `UpdateExecutionStatusAsync`:
```csharp
✅ بررسی OneTime (غیرفعال شدن خودکار)
✅ بررسی MaxOccurrences (غیرفعال شدن خودکار)
✅ بررسی EndDate (غیرفعال شدن خودکار)
✅ محاسبه NextExecutionDate با الگوریتم جدید
```

---

## 📊 مقایسه قبل و بعد:

### ❌ قبل:
```
ساخت مستقیم DateTime با DateTimeKind ❌
محاسبه نادرست NextExecutionDate ❌
TimeZone مشکل‌دار ❌
OneTime بدون پشتیبانی درست ❌
```

### ✅ بعد:
```
استفاده از .Add(new TimeSpan()) ✅
محاسبه دقیق با الگوریتم Notification ✅
TimeZone Iran → UTC صحیح ✅
OneTime با OneTimeExecutionDatePersian ✅
```

---

## 🧪 مراحل Testing:

### تست 1: روزانه ساعت 12:55
```
الان: 1403/09/30 13:00
ساعت اجرا: 12:55
نتیجه مورد انتظار: 1403/10/01 12:55 (فردا)

✅ باید یک روز اضافه شود (چون امروز گذشته)
```

### تست 2: هفتگی شنبه‌ها ساعت 14:00
```
الان: 1403/09/30 (یکشنبه) 15:00
ساعت اجرا: 14:00
روز انتخابی: شنبه (6)
نتیجه مورد انتظار: 1403/10/06 (شنبه آینده) 14:00

✅ باید شنبه هفته بعد را برگرداند
```

### تست 3: ماهانه روز 10 ساعت 09:00
```
الان: 1403/09/30 10:00
ساعت اجرا: 09:00
روز: 10
نتیجه مورد انتظار: 1403/10/10 09:00 (ماه بعد)

✅ باید روز 10 ماه بعد را برگرداند
```

### تست 4: یکبار 1403/10/05 ساعت 16:00
```
الان: 1403/09/30 13:00
تاریخ OneTime: 1403/10/05
ساعت: 16:00
نتیجه مورد انتظار: 1403/10/05 16:00

✅ باید دقیقاً همان تاریخ و ساعت را برگرداند
```

---

## 🔍 بررسی در دیتابیس:

```sql
SELECT 
    Id,
    ScheduleTitle,
    ScheduleType,
    ScheduledTime,
    NextExecutionDate,
    -- ⭐ تبدیل UTC به Iran Time
    DATEADD(HOUR, 3, DATEADD(MINUTE, 30, NextExecutionDate)) AS NextExecutionIranTime,
    IsActive,
    IsScheduleEnabled,
    CreatedDate
FROM ScheduledTaskCreation_Tbl
WHERE IsActive = 1
ORDER BY NextExecutionDate;
```

**انتظار:**
- `NextExecutionDate` در UTC ذخیره شده باشد
- Iran Time = UTC + 3:30 (زمستان) یا UTC + 4:30 (تابستان)
- ساعت دقیق باشد (مثلاً 12:55 UTC → 16:25 ایران)

---

## ⚙️ نکات فنی:

### 1. TimeZone Management:
```csharp
// ✅ Iran TimeZone
private static readonly TimeZoneInfo IranTimeZone = 
    TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");

// ✅ تبدیل UTC → Iran
var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);

// ✅ تبدیل Iran → UTC
return TimeZoneInfo.ConvertTimeToUtc(nextExecutionIran, IranTimeZone);
```

### 2. DateTime Construction:
```csharp
// ❌ اشتباه (خطای Compiler)
new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);

// ✅ درست
new DateTime(year, month, day).Add(new TimeSpan(hour, minute, 0));
```

### 3. Nullable Date Handling:
```csharp
// ✅ استفاده از متد nullable-safe
StartDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(
    model.TaskSchedule?.StartDatePersian);
```

---

## 📝 چک‌لیست نهایی:

- [x] کپی کردن الگوریتم `CalculateNextExecutionDate`
- [x] کپی کردن متد `FindNextWeeklyExecution`
- [x] کپی کردن متد `FindNextMonthlyExecution`
- [x] کپی کردن متد `FindNextMonthlyMultipleDaysExecution`
- [x] اصلاح `UpdateScheduledTaskAsync`
- [x] اصلاح `UpdateExecutionStatusAsync`
- [x] اضافه کردن `ConvertShamsiToMiladiNullable`
- [x] اصلاح سینتکس DateTime
- [x] Build موفق

---

## 🚀 مرحله بعدی:

### 1. **Stop Debugging** و **Run مجدد**

### 2. **تست ساخت Schedule جدید:**
```
- نوع: روزانه
- ساعت: 12:55
- چک کردن NextExecutionDate در دیتابیس
```

### 3. **مقایسه با زمان فعلی:**
```csharp
// الان:
1403/09/30 13:00 (Iran) → 2024-12-20 09:30 (UTC)

// NextExecutionDate باید باشد:
1403/10/01 12:55 (Iran) → 2024-12-21 09:25 (UTC)
```

### 4. **بررسی Logs:**
```
🔍 Schedule enabled: True
🔍 Schedule type: Daily
🔍 Schedule time: 12:55
✅ NextExecutionDate calculated: 2024-12-21 09:25:00 UTC
```

---

## ✅ انتظارات نهایی:

| سناریو | ورودی | NextExecutionDate (Iran) | NextExecutionDate (UTC) |
|--------|-------|-------------------------|------------------------|
| روزانه 12:55 | امروز 13:00 | فردا 12:55 | +3.5h |
| هفتگی شنبه 14:00 | یکشنبه 15:00 | شنبه آینده 14:00 | +3.5h |
| ماهانه روز 10 | 30ام 10:00 | ماه بعد روز 10 | +3.5h |
| یکبار 1403/10/05 16:00 | - | 1403/10/05 16:00 | +3.5h |

---

## 📞 در صورت مشکل:

1. Console Output کامل را بررسی کنید
2. NextExecutionDate در دیتابیس را با UTC مقایسه کنید
3. Iran Time را با UTC + 3:30 محاسبه کنید
4. بررسی کنید که `ScheduledTime` درست ارسال شده

---

✅ **همه تغییرات اعمال شد! آماده برای تست است.** 🚀
