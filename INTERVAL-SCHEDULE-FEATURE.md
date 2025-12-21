# ⭐ قابلیت جدید: زمان‌بندی با فاصله (Interval-Based Schedule)

## 🎯 **هدف:**
اضافه کردن نوع جدید زمان‌بندی که بتواند هر X روز یکبار تسک بسازد، با یا بدون محدودیت روز هفته.

---

## 📋 **ویژگی‌ها:**

### 1️⃣ **حالت ساده:**
```
✅ هر 14 روز یکبار
✅ هر 7 روز یکبار
✅ هر 30 روز یکبار
```

### 2️⃣ **حالت پیشرفته (با روز هفته):**
```
✅ هر 14 روز، فقط شنبه‌ها
✅ هر 10 روز، فقط دوشنبه‌ها
✅ هر 21 روز، فقط جمعه‌ها
```

---

## 🗂️ **تغییرات فایل‌ها:**

### 1️⃣ **Entity: `ScheduledTaskCreation.cs`**
```csharp
✅ اضافه شد: IntervalDays (int?)
✅ اضافه شد: IntervalDayOfWeek (int?)
✅ بروز شد: ScheduleType - اضافه شدن case 4
```

### 2️⃣ **ViewModel: `TaskScheduleViewModel.cs`**
```csharp
✅ اضافه شد: IntervalDays (int?)
✅ اضافه شد: IntervalDayOfWeek (int?)
```

### 3️⃣ **Repository: `TaskRepository.ScheduledTasks.cs`**
```csharp
✅ اضافه شد: case 4 در CalculateNextExecutionDate
✅ اضافه شد: FindNextIntervalExecution()
✅ اضافه شد: FindNextDayOfWeek()
✅ اضافه شد: FindNextIntervalWithDayOfWeek()
```

### 4️⃣ **View: `_AutoScheduleSection.cshtml`**
```html
✅ اضافه شد: option value="4" (Interval)
✅ اضافه شد: intervalOptions section
✅ اضافه شد: intervalDayOfWeekRow
```

### 5️⃣ **JavaScript: `task-schedule-manager.js`**
```javascript
✅ اضافه شد: case 4 در updateScheduleTypeOptions
✅ اضافه شد: case 4 در updateSchedulePreview
```

### 6️⃣ **Migration: `Add-Interval-Schedule-Type.sql`**
```sql
✅ ایجاد شد: ALTER TABLE برای IntervalDays
✅ ایجاد شد: ALTER TABLE برای IntervalDayOfWeek
```

---

## 🧪 **سناریوهای تست:**

### تست 1: هر 14 روز (بدون محدودیت روز)
```
📝 ورودی:
- نوع: تکرار با فاصله
- فاصله: 14 روز
- ساعت: 14:00
- تاریخ شروع: 1403/09/30
- روز هفته: خالی (هر روزی)

📊 نتیجه مورد انتظار:
- اجرای اول: 1403/09/30 14:00 (امروز)
- اجرای دوم: 1403/10/14 14:00 (14 روز بعد)
- اجرای سوم: 1403/10/28 14:00 (28 روز بعد)
```

### تست 2: هر 14 روز، فقط شنبه‌ها
```
📝 ورودی:
- نوع: تکرار با فاصله
- فاصله: 14 روز
- ساعت: 14:00
- تاریخ شروع: 1403/09/30 (یکشنبه)
- روز هفته: 6 (شنبه)

📊 نتیجه مورد انتظار:
- اجرای اول: 1403/10/05 14:00 (اولین شنبه بعد از 14 روز)
- اجرای دوم: 1403/10/19 14:00 (14 روز بعد، شنبه)
- اجرای سوم: 1403/11/04 14:00 (14 روز بعد، شنبه)
```

### تست 3: هر 7 روز، فقط دوشنبه‌ها
```
📝 ورودی:
- نوع: تکرار با فاصله
- فاصله: 7 روز
- ساعت: 09:00
- تاریخ شروع: 1403/09/30 (یکشنبه)
- روز هفته: 1 (دوشنبه)

📊 نتیجه مورد انتظار:
- اجرای اول: 1403/10/01 09:00 (دوشنبه، 1 روز بعد)
- اجرای دوم: 1403/10/08 09:00 (7 روز بعد، دوشنبه)
- اجرای سوم: 1403/10/15 09:00 (7 روز بعد، دوشنبه)
```

---

## 🔧 **مراحل اجرا:**

### مرحله 1: اجرای Migration
```sql
-- اجرای فایل Database\Migrations\Add-Interval-Schedule-Type.sql
-- یا اجرای دستی:

ALTER TABLE ScheduledTaskCreation_Tbl ADD IntervalDays INT NULL;
ALTER TABLE ScheduledTaskCreation_Tbl ADD IntervalDayOfWeek INT NULL;
```

### مرحله 2: Build پروژه
```bash
dotnet build
```

### مرحله 3: Run و تست
```
1. Stop Debugging
2. Build
3. Run
4. رفتن به صفحه Create Task
5. فعال کردن زمان‌بندی
6. انتخاب "تکرار با فاصله"
7. وارد کردن داده‌های تست
8. ذخیره
```

---

## 📊 **الگوریتم محاسبه:**

### حالت 1: بدون روز هفته (ساده)
```csharp
1. محاسبه تعداد دوره‌های گذشته از StartDate
2. محاسبه تاریخ دوره بعدی:
   NextDate = StartDate + ((Cycles + 1) * IntervalDays)
3. برگرداندن NextDate
```

### حالت 2: با روز هفته (پیشرفته)
```csharp
1. محاسبه تاریخ بعدی (مثل حالت 1)
2. اگر NextDate روز هفته مورد نظر نیست:
   - پیدا کردن نزدیک‌ترین روز هفته بعد از NextDate
3. بررسی: آیا تاریخ جدید مضرب IntervalDays است؟
4. اگر نه، جستجو برای یافتن نزدیک‌ترین تاریخ معتبر:
   - باید مضرب IntervalDays باشد
   - باید در روز هفته مورد نظر باشد
```

---

## 📝 **مثال‌های واقعی:**

### مثال 1: پیگیری مشتریان هر 2 هفته
```
عنوان: پیگیری مشتریان مهم
نوع: تکرار با فاصله
فاصله: 14 روز
ساعت: 10:00
روز هفته: دوشنبه (1)
تاریخ شروع: 1403/10/01

→ هر 2 هفته یکبار، دوشنبه‌ها ساعت 10 صبح
```

### مثال 2: بک‌آپ هفتگی
```
عنوان: بک‌آپ سیستم
نوع: تکرار با فاصله
فاصله: 7 روز
ساعت: 02:00
روز هفته: شنبه (6)
تاریخ شروع: 1403/10/05

→ هر هفته شنبه ساعت 2 بامداد
```

### مثال 3: گزارش سه‌ماهه
```
عنوان: گزارش سه‌ماهه
نوع: تکرار با فاصله
فاصله: 90 روز
ساعت: 15:00
روز هفته: (خالی)
تاریخ شروع: 1403/01/01

→ هر 90 روز یکبار
```

---

## 🔍 **بررسی در دیتابیس:**

```sql
-- بررسی Schedule های Interval
SELECT 
    Id,
    ScheduleTitle,
    ScheduleType,
    IntervalDays,
    IntervalDayOfWeek,
    ScheduledTime,
    StartDate,
    NextExecutionDate,
    DATEADD(HOUR, 3, DATEADD(MINUTE, 30, NextExecutionDate)) AS NextExecutionIranTime,
    ExecutionCount,
    IsActive,
    IsScheduleEnabled
FROM ScheduledTaskCreation_Tbl
WHERE ScheduleType = 4 -- Interval
ORDER BY CreatedDate DESC;
```

---

## 🐛 **Debugging:**

### بررسی Console Output:
```
🔍 FindNextIntervalExecution: Start=2024-12-20 14:00, Interval=14, DayOfWeek=6
🔍 Calculated next date (without day restriction): 2025-01-03 14:00
🔍 Adjusted for day of week: 2025-01-04 14:00
🔍 Adjusted to next valid interval: 2025-01-04 14:00
✅ Final next execution: 2025-01-04 14:00
```

### بررسی Form Data:
```javascript
// در Console مرورگر:
$('input[name="TaskSchedule.IntervalDays"]').val()  // باید عدد باشد
$('#intervalDayOfWeek').val()                        // باید 0-6 یا empty
```

---

## ⚠️ **نکات مهم:**

1. ✅ **StartDate الزامی است** برای Interval (برای محاسبه دوره‌ها)
2. ✅ **IntervalDays باید > 0** باشد
3. ✅ **IntervalDayOfWeek اختیاری است** (0-6 یا null)
4. ✅ **الگوریتم جستجو محدود به 365 روز است** (برای جلوگیری از لوپ بی‌نهایت)
5. ✅ **تبدیل Iran Time → UTC** به درستی انجام می‌شود

---

## 📈 **مقایسه با انواع دیگر:**

| نوع | مثال | کاربرد |
|-----|------|--------|
| Daily | هر روز 14:00 | کارهای روزمره |
| Weekly | هر شنبه 14:00 | جلسات هفتگی |
| Monthly | روز 1 و 15 هر ماه | پرداخت‌ها |
| **Interval** | **هر 14 روز** | **پیگیری‌های منظم** |
| **Interval + Day** | **هر 14 روز، شنبه** | **جلسات دوره‌ای** |

---

## ✅ **چک‌لیست نهایی:**

- [x] Entity: اضافه شدن فیلدها
- [x] ViewModel: اضافه شدن فیلدها
- [x] Repository: الگوریتم محاسبه
- [x] View: UI برای ورود داده
- [x] JavaScript: پشتیبانی از Interval
- [x] Migration: آماده برای اجرا
- [x] Documentation: کامل
- [ ] **Migration اجرا شود**
- [ ] **Build موفق**
- [ ] **تست سناریو 1**
- [ ] **تست سناریو 2**
- [ ] **تست سناریو 3**

---

## 🚀 **آماده برای استفاده!**

اول Migration رو اجرا کن، بعد Build و Run کن. 
سپس تست کن و نتیجه رو بهم بگو! 🎉
