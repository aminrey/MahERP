# 🐛 مشکل: زمان اجرای بعدی تنظیم نشده - تسک ماهانه با چند روز

## 🔍 مشکلات شناسایی شده:

### 1️⃣ **مشکل JavaScript:**
**علت**: JavaScript فقط یک روز (`ScheduledDayOfMonth`) را می‌خواند، نه چند روز (`ScheduledDaysOfMonth`)

**قبل:**
```javascript
// ❌ فقط یک روز
const dayOfMonth = $('input[name="TaskSchedule.ScheduledDayOfMonth"]').val();
```

**بعد:**
```javascript
// ✅ چند روز
const selectedMonthDays = [];
$('input[name="dayOfMonth"]:checked').each(function() {
    selectedMonthDays.push(parseInt($(this).val()));
});
```

### 2️⃣ **فقدان Debug Logs:**
**مشکل**: نمی‌شد فهمید که چرا `NextExecutionDate` null است

**راه‌حل**: اضافه شدن Debug Logs کامل به `CalculateNextExecutionDate`

---

## ✅ تغییرات اعمال شده:

### 1️⃣ **فایل: `task-schedule-manager.js`**

#### اضافه شدن Event Handler برای روزهای ماه:
```javascript
// ⭐⭐⭐ بروزرسانی چک‌باکس‌های روزهای ماه
$('input[name="dayOfMonth"]').on('change', function() {
    const selectedDays = [];
    $('input[name="dayOfMonth"]:checked').each(function() {
        selectedDays.push($(this).val());
    });
    $('#scheduledDaysOfMonth').val(selectedDays.join(','));
    updateSchedulePreview();
    
    console.log('📆 Selected month days:', selectedDays);
});
```

#### بهبود Preview برای ماهانه:
```javascript
case 3: // ماهانه
    // ⭐⭐⭐ FIX: بررسی چند روز
    const selectedMonthDays = [];
    $('input[name="dayOfMonth"]:checked').each(function() {
        selectedMonthDays.push(parseInt($(this).val()));
    });
    
    if (selectedMonthDays.length > 0 && scheduledTime) {
        previewHtml += ` روزهای ${selectedMonthDays.sort((a, b) => a - b).join('، ')} هر ماه ساعت ${scheduledTime}`;
    }
    break;
```

### 2️⃣ **فایل: `TaskRepository.ScheduledTasks.cs`**

#### اضافه شدن Debug Logs کامل:
```csharp
✅ Log ورودی: ScheduleType, Time, NowIran
✅ Log OneTime: تاریخ اجرا
✅ Log Daily: امروز یا فردا
✅ Log Weekly: روزهای انتخابی
✅ Log Monthly (Multiple): روزهای انتخابی، محاسبه
✅ Log Monthly (Single): روز تکی
✅ Log خروجی: Iran Time → UTC
```

**مثال Log:**
```
🔍 CalculateNextExecutionDate: ScheduleType=3, Time=14:0, NowIran=2024-12-20 13:30
🔍 Monthly (Multiple): ScheduledDaysOfMonth=1,15
🔍 Monthly (Multiple): روزهای انتخابی: 1, 15
✅ Monthly (Multiple): NextExecution=2024-12-21 14:00
✅ تبدیل نهایی: Iran=2024-12-21 14:00 → UTC=2024-12-21 10:30
```

---

## 🧪 مراحل Testing:

### تست 1: ماهانه روز 1 و 15 ساعت 14:00
```
الان: 1403/09/30 13:30
روزهای انتخابی: 1، 15
ساعت: 14:00

نتیجه مورد انتظار:
- اگر امروز قبل از روز 1 است → 1 همین ماه 14:00
- اگر امروز بعد از روز 1 و قبل از 15 است → 15 همین ماه 14:00
- اگر امروز بعد از روز 15 است → 1 ماه بعد 14:00
```

### بررسی در Console Output:
```
✅ Task Schedule Manager initialized
📆 Selected month days: ["1", "15"]
🔍 CalculateNextExecutionDate: ScheduleType=3, Time=14:0, NowIran=...
🔍 Monthly (Multiple): ScheduledDaysOfMonth=1,15
🔍 Monthly (Multiple): روزهای انتخابی: 1, 15
✅ Monthly (Multiple): NextExecution=...
✅ تبدیل نهایی: Iran=... → UTC=...
```

### بررسی در دیتابیس:
```sql
SELECT 
    Id,
    ScheduleTitle,
    ScheduleType,
    ScheduledTime,
    ScheduledDaysOfMonth,  -- باید "1,15" باشد
    NextExecutionDate,     -- باید مقدار داشته باشد (UTC)
    DATEADD(HOUR, 3, DATEADD(MINUTE, 30, NextExecutionDate)) AS NextExecutionIranTime,
    IsActive,
    IsScheduleEnabled
FROM ScheduledTaskCreation_Tbl
WHERE ScheduleType = 3
ORDER BY CreatedDate DESC;
```

**انتظار:**
- `ScheduledDaysOfMonth = "1,15"`
- `NextExecutionDate` مقدار داشته باشد (نه NULL)
- Iran Time = UTC + 3:30

---

## 🔍 نکات Debugging:

### 1. بررسی Form Data در Browser:
```javascript
// در Console مرورگر بعد از Submit:
$('#scheduledDaysOfMonth').val()
// باید برگرداند: "1,15"
```

### 2. بررسی Console Output در Visual Studio:
```
Output → Debug → Show output from: Debug

دنبال این پیام‌ها بگردید:
🔍 CalculateNextExecutionDate: ...
🔍 Monthly (Multiple): ScheduledDaysOfMonth=1,15
✅ Monthly (Multiple): NextExecution=...
```

### 3. بررسی Network Tab:
```
Developer Tools → Network → [Submit Request] → Payload

باید ببینید:
TaskSchedule.ScheduledDaysOfMonth: "1,15"
```

---

## ⚠️ مشکلات احتمالی و راه‌حل:

### مشکل 1: باز هم NextExecutionDate نال است
**بررسی:**
1. Console Output چه چیزی نشان می‌دهد؟
2. `ScheduledDaysOfMonth` در دیتابیس چیست؟
3. آیا JavaScript درست اجرا شده؟

**راه‌حل:**
```javascript
// در Console مرورگر:
$('input[name="dayOfMonth"]:checked').length  // باید > 0 باشد
$('#scheduledDaysOfMonth').val()              // باید "1,15" باشد
```

### مشکل 2: فقط یک روز ذخیره می‌شود
**بررسی:**
```sql
SELECT ScheduledDaysOfMonth FROM ScheduledTaskCreation_Tbl WHERE Id = [آخرین رکورد]
```

**اگر فقط یک عدد است:**
- مشکل در JavaScript است
- Hidden input درست set نشده

### مشکل 3: NextExecutionDate تاریخ اشتباه
**بررسی:**
```
Console Output:
✅ Monthly (Multiple): NextExecution=2024-12-21 14:00

اگر اشتباه است:
- بررسی کنید الان چه تاریخی است
- بررسی کنید روز 1 یا 15 گذشته یا نه
```

---

## 📝 چک‌لیست نهایی:

- [x] JavaScript Event Handler برای `dayOfMonth` اضافه شد
- [x] JavaScript Preview برای چند روز اصلاح شد
- [x] Debug Logs کامل به `CalculateNextExecutionDate` اضافه شد
- [x] Build موفق
- [ ] **تست با ساخت Schedule ماهانه روز 1 و 15**
- [ ] **بررسی Console Output**
- [ ] **بررسی دیتابیس**

---

## 🚀 مراحل بعدی:

### 1. Stop Debugging و Run مجدد

### 2. ساخت Schedule ماهانه:
```
- نوع: ماهانه
- روزهای انتخابی: 1، 15
- ساعت: 14:00
- Submit
```

### 3. بررسی Console:
```
Output Window → Debug

دنبال:
🔍 Monthly (Multiple): ScheduledDaysOfMonth=1,15
✅ Monthly (Multiple): NextExecution=...
```

### 4. بررسی View:
```
/ScheduledTasks/Index

باید ببینید:
✅ زمان اجرای بعدی: 1403/10/01 14:00 (مثلاً)
```

---

## 📊 نمونه Output موفق:

```
✅ Task Schedule Manager initialized
📆 Selected month days: ["1", "15"]
🔍 TaskSchedule.IsScheduled: True
🔍 TaskSchedule.CreateImmediately: False
🔍 TaskSchedule.ScheduleType: 3
🔍 TaskSchedule.ScheduledDaysOfMonth: 1,15
🔍 CalculateNextExecutionDate: ScheduleType=3, Time=14:0, NowIran=2024-12-20 13:30
🔍 Monthly (Multiple): ScheduledDaysOfMonth=1,15
🔍 Monthly (Multiple): روزهای انتخابی: 1, 15
✅ Monthly (Multiple): NextExecution=2025-01-01 14:00
✅ تبدیل نهایی: Iran=2025-01-01 14:00 → UTC=2025-01-01 10:30
✅ Found 1 schedules
📋 Schedule #5: تست ماهانه (Type: 3, IsActive: True, IsEnabled: True)
```

---

✅ **همه تغییرات اعمال شد! آماده برای تست است.** 🚀

اگر مشکل ادامه داشت، Console Output کامل را برای من بفرستید.
