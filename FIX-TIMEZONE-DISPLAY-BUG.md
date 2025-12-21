# 🐛 FIX: مشکل نمایش ساعت اجرای بعدی (5:30 به جای 9:00)

## ❌ **مشکل:**

### ورودی کاربر:
```
ساعت اجرا: 09:00 صبح (Iran Time)
```

### نمایش در UI:
```
اجرای بعدی: 05:30 ❌ اشتباه!
```

---

## 🔍 **علت مشکل:**

### مرحله 1: ذخیره در دیتابیس (✅ صحیح)
```csharp
// محاسبه NextExecutionDate
var nextExecutionIran = new DateTime(2024, 12, 21, 9, 0, 0); // 09:00 Iran

// تبدیل به UTC
var nextExecutionUtc = TimeZoneInfo.ConvertTimeToUtc(nextExecutionIran, IranTimeZone);
// Result: 2024-12-21 05:30:00 UTC ✅

// ذخیره در دیتابیس
schedule.NextExecutionDate = nextExecutionUtc; // 05:30 UTC ✅
```

### مرحله 2: نمایش در UI (❌ اشتباه)
```csharp
// کد قبلی (اشتباه)
NextExecutionDatePersian = schedule.NextExecutionDate.HasValue
    ? ConvertDateTime.ConvertMiladiToShamsi(
        schedule.NextExecutionDate.Value, // ⚠️ این UTC است!
        "yyyy/MM/dd HH:mm")
    : null;

// Result: 1403/10/01 05:30 ❌ (UTC بدون تبدیل)
```

**مشکل:** UTC را مستقیماً به شمسی تبدیل کرده، بدون اینکه اول به Iran Time تبدیل کنه!

---

## ✅ **راه‌حل:**

### کد اصلاح شده:
```csharp
NextExecutionDatePersian = schedule.NextExecutionDate.HasValue
    ? ConvertDateTime.ConvertMiladiToShamsi(
        TimeZoneInfo.ConvertTimeFromUtc(schedule.NextExecutionDate.Value, IranTimeZone), // ⭐ اول تبدیل به Iran
        "yyyy/MM/dd HH:mm")
    : null;
```

### جریان صحیح:
```
UTC در دیتابیس: 2024-12-21 05:30:00
         ↓ TimeZoneInfo.ConvertTimeFromUtc
Iran Time: 2024-12-21 09:00:00
         ↓ ConvertMiladiToShamsi
Persian: 1403/10/01 09:00
```

---

## 🔧 **فایل‌های اصلاح شده:**

### 1️⃣ **`TaskRepository.ScheduledTasks.cs`**

#### در متد `MapToScheduledTaskCard`:
```csharp
// ⭐⭐⭐ FIX: StartDate
StartDatePersian = schedule.StartDate.HasValue
    ? ConvertDateTime.ConvertMiladiToShamsi(
        TimeZoneInfo.ConvertTimeFromUtc(schedule.StartDate.Value, IranTimeZone),
        "yyyy/MM/dd HH:mm")
    : null,

// ⭐⭐⭐ FIX: EndDate
EndDatePersian = schedule.EndDate.HasValue
    ? ConvertDateTime.ConvertMiladiToShamsi(
        TimeZoneInfo.ConvertTimeFromUtc(schedule.EndDate.Value, IranTimeZone),
        "yyyy/MM/dd HH:mm")
    : null,

// ⭐⭐⭐ FIX: NextExecutionDate
NextExecutionDatePersian = schedule.NextExecutionDate.HasValue
    ? ConvertDateTime.ConvertMiladiToShamsi(
        TimeZoneInfo.ConvertTimeFromUtc(schedule.NextExecutionDate.Value, IranTimeZone),
        "yyyy/MM/dd HH:mm")
    : null,

// ⭐⭐⭐ FIX: LastExecutionDate
LastExecutionDatePersian = schedule.LastExecutionDate.HasValue
    ? ConvertDateTime.ConvertMiladiToShamsi(
        TimeZoneInfo.ConvertTimeFromUtc(schedule.LastExecutionDate.Value, IranTimeZone),
        "yyyy/MM/dd HH:mm")
    : null,

// ⭐⭐⭐ FIX: CreatedDate
CreatedDatePersian = ConvertDateTime.ConvertMiladiToShamsi(
    TimeZoneInfo.ConvertTimeFromUtc(schedule.CreatedDate, IranTimeZone),
    "yyyy/MM/dd HH:mm"),
```

#### در متد `GetScheduledTaskForEditAsync`:
```csharp
// ⭐⭐⭐ FIX: StartDatePersian
StartDatePersian = schedule.StartDate.HasValue
    ? ConvertDateTime.ConvertMiladiToShamsi(
        TimeZoneInfo.ConvertTimeFromUtc(schedule.StartDate.Value, IranTimeZone),
        "yyyy/MM/dd HH:mm")
    : null,

// ⭐⭐⭐ FIX: EndDatePersian
EndDatePersian = schedule.EndDate.HasValue
    ? ConvertDateTime.ConvertMiladiToShamsi(
        TimeZoneInfo.ConvertTimeFromUtc(schedule.EndDate.Value, IranTimeZone),
        "yyyy/MM/dd HH:mm")
    : null,
```

---

## 📊 **مقایسه قبل و بعد:**

### قبل از Fix:
| فیلد دیتابیس (UTC) | نمایش در UI | ❌ مشکل |
|-------------------|-------------|---------|
| 05:30:00 UTC | 05:30 | اشتباه! |
| 09:00:00 UTC | 09:00 | اشتباه! |

### بعد از Fix:
| فیلد دیتابیس (UTC) | Iran Time | نمایش در UI | ✅ صحیح |
|-------------------|-----------|-------------|---------|
| 05:30:00 UTC | 09:00 Iran | 09:00 | درست! |
| 09:00:00 UTC | 12:30 Iran | 12:30 | درست! |

---

## 🧪 **تست:**

### سناریو 1: ساخت Schedule با ساعت 09:00
```
1. ساخت Schedule
   - نوع: روزانه
   - ساعت: 09:00
   
2. ذخیره در دیتابیس:
   SELECT NextExecutionDate FROM ScheduledTaskCreation_Tbl
   Result: 2024-12-21 05:30:00 (UTC) ✅

3. نمایش در UI:
   Result: 1403/10/01 09:00 ✅ (بعد از Fix)
```

### سناریو 2: ساخت Schedule با ساعت 14:00
```
1. ساخت Schedule
   - نوع: هفتگی
   - ساعت: 14:00
   
2. ذخیره در دیتابیس:
   Result: 2024-12-21 10:30:00 (UTC) ✅

3. نمایش در UI:
   Result: 1403/10/01 14:00 ✅
```

---

## 🔍 **نکات مهم:**

### 1️⃣ **همیشه UTC در دیتابیس:**
```
✅ دیتابیس: UTC
✅ محاسبات: Iran Time
✅ نمایش: Iran Time → Persian
```

### 2️⃣ **TimeZone Offset:**
```
Iran Standard Time:
- زمستان: UTC + 3:30
- تابستان: UTC + 4:30 (DST)

مثال:
09:00 Iran = 05:30 UTC (زمستان)
09:00 Iran = 04:30 UTC (تابستان)
```

### 3️⃣ **جریان صحیح داده:**
```
User Input (Iran) → Calculation (Iran) → Database (UTC) → Display (Iran → Persian)
```

---

## ⚠️ **خطاهای رایج:**

### خطا 1: فراموش کردن TimeZoneInfo.ConvertTimeFromUtc
```csharp
// ❌ اشتباه
ConvertMiladiToShamsi(schedule.NextExecutionDate.Value, ...)

// ✅ درست
ConvertMiladiToShamsi(
    TimeZoneInfo.ConvertTimeFromUtc(schedule.NextExecutionDate.Value, IranTimeZone),
    ...)
```

### خطا 2: استفاده از DateTime.Now به جای UTC
```csharp
// ❌ اشتباه
schedule.NextExecutionDate = DateTime.Now;

// ✅ درست
schedule.NextExecutionDate = TimeZoneInfo.ConvertTimeToUtc(nextExecutionIran, IranTimeZone);
```

### خطا 3: مخلوط کردن UTC و Local Time
```csharp
// ❌ اشتباه
var now = DateTime.Now; // Local time
var nextExecution = now.AddDays(1); // همچنان Local
schedule.NextExecutionDate = nextExecution; // ⚠️ باید UTC باشد!

// ✅ درست
var nowUtc = DateTime.UtcNow;
var nowIran = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, IranTimeZone);
var nextExecutionIran = nowIran.AddDays(1);
schedule.NextExecutionDate = TimeZoneInfo.ConvertTimeToUtc(nextExecutionIran, IranTimeZone);
```

---

## 📝 **چک‌لیست نهایی:**

- [x] MapToScheduledTaskCard: NextExecutionDatePersian اصلاح شد
- [x] MapToScheduledTaskCard: LastExecutionDatePersian اصلاح شد
- [x] MapToScheduledTaskCard: StartDatePersian اصلاح شد
- [x] MapToScheduledTaskCard: EndDatePersian اصلاح شد
- [x] MapToScheduledTaskCard: CreatedDatePersian اصلاح شد
- [x] GetScheduledTaskForEditAsync: StartDatePersian اصلاح شد
- [x] GetScheduledTaskForEditAsync: EndDatePersian اصلاح شد
- [x] Build موفق ✅
- [ ] **تست: ساخت Schedule و بررسی ساعت نمایش**
- [ ] **تست: Edit Schedule و بررسی ساعت‌ها**

---

## 🚀 **اقدامات بعدی:**

### 1️⃣ **Run و تست:**
```
1. Stop Debugging
2. Build
3. Run
4. ساخت Schedule با ساعت 09:00
5. بررسی نمایش: باید 09:00 باشد (نه 05:30)
```

### 2️⃣ **بررسی دیتابیس:**
```sql
SELECT 
    ScheduleTitle,
    ScheduledTime,
    NextExecutionDate, -- باید UTC باشد (05:30)
    DATEADD(HOUR, 3, DATEADD(MINUTE, 30, NextExecutionDate)) AS IranTime -- باید 09:00 باشد
FROM ScheduledTaskCreation_Tbl
ORDER BY CreatedDate DESC;
```

### 3️⃣ **بررسی UI:**
```
صفحه لیست:
✅ اجرای بعدی: 1403/10/01 09:00

صفحه جزئیات:
✅ ساعت اجرا: 09:00
✅ اجرای بعدی: 1403/10/01 09:00
```

---

## ✅ **خلاصه:**

| مشکل | علت | راه‌حل |
|------|-----|--------|
| نمایش 05:30 به جای 09:00 | UTC بدون تبدیل | TimeZoneInfo.ConvertTimeFromUtc |
| تاریخ اشتباه | UTC بدون تبدیل | TimeZoneInfo.ConvertTimeFromUtc |
| ساعت ایجاد اشتباه | UTC بدون تبدیل | TimeZoneInfo.ConvertTimeFromUtc |

---

✅ **FIX اعمال شد! Build موفق! آماده برای تست!** 🎉
