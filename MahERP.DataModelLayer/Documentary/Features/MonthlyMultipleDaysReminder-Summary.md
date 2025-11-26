# 📋 خلاصه تغییرات: یادآوری ماهانه با چند روز

## ✅ تغییرات اعمال شده

### 1️⃣ **Entity Layer**
- ✅ `TaskReminderSchedule.cs` → اضافه شدن فیلد `ScheduledDaysOfMonth`

### 2️⃣ **ViewModel Layer**
- ✅ `TaskReminderViewModel.cs` → اضافه شدن فیلد `ScheduledDaysOfMonth`

### 3️⃣ **Repository Layer**
- ✅ `TaskRepository.Reminders.cs`:
  - `CreateReminderAsync`: پشتیبانی از `ScheduledDaysOfMonth`
  - `GetTaskRemindersListAsync`: نمایش `ScheduledDaysOfMonth`

### 4️⃣ **Background Service**
- ✅ `TaskReminderBackgroundService.cs`:
  - `IsOneTimeReminderType`: شناسایی نوع 4 به عنوان تکراری
  - `CalculateNextExecutionTime`: محاسبه زمان بعدی برای نوع 4
  - `FindNextMonthlyExecution`: الگوریتم پیدا کردن اولین اجرای بعدی

### 5️⃣ **View Layer**
- ✅ `_AddReminderModal.cshtml`:
  - اضافه شدن radio button نوع 4
  - اضافه شدن UI انتخاب 31 روز ماه
  - JavaScript برای مدیریت انتخاب روزها
  - پیش‌نمایش زنده
  - استایل‌های مخصوص

### 6️⃣ **Database Migration**
- ✅ Migration C#: `20241220000001_AddScheduledDaysOfMonthToReminder.cs`
- ✅ Manual SQL Script: `AddScheduledDaysOfMonthToReminder.sql`

### 7️⃣ **Documentation**
- ✅ `MonthlyMultipleDaysReminder.md` → راهنمای کامل استفاده

---

## 🎯 قابلیت‌های جدید

### ✨ یادآوری ماهانه با چند روز
- انتخاب 1 تا 31 روز در هر ماه
- ارسال خودکار در روزهای مشخص شده
- تنظیم ساعت دقیق ارسال
- محدودیت تعداد ارسال (اختیاری)
- مدیریت هوشمند ماه‌های کوتاه‌تر

---

## 📊 ساختار دیتابیس

```sql
ALTER TABLE TaskReminderSchedule_Tbl
ADD ScheduledDaysOfMonth NVARCHAR(100) NULL;
```

**مثال داده:**
```
ScheduledDaysOfMonth = "10,20,25"
```

---

## 🚀 نحوه استفاده

### 1. از طریق UI:
1. باز کردن مودال "افزودن یادآوری"
2. انتخاب نوع **"ماهانه - چند روز"**
3. انتخاب روزهای مورد نظر از گرید
4. تنظیم ساعت ارسال
5. (اختیاری) تعیین حداکثر تعداد ارسال
6. ذخیره

### 2. از طریق کد:
```csharp
var reminder = new TaskReminderViewModel
{
    TaskId = taskId,
    Title = "یادآوری ماهانه",
    ReminderType = 4,
    ScheduledDaysOfMonth = "10,20,25",
    NotificationTime = new TimeSpan(9, 0, 0),
    MaxSendCount = 12,
    IsActive = true
};

await _taskRepository.CreateReminderAsync(reminder, userId);
```

---

## ⚙️ منطق Background Service

```
هر 1 دقیقه:
  └─ دریافت یادآوری‌های فعال با ReminderType = 4
      └─ Parse ScheduledDaysOfMonth
          └─ محاسبه زمان بعدی
              └─ اگر زمان رسیده:
                  ├─ ارسال اعلان
                  ├─ افزایش SentCount
                  └─ بررسی MaxSendCount
```

---

## 🧪 تست

### تست 1: یادآوری ساده
```
روزهای انتخاب شده: 15
ساعت: 10:00

انتظار: هر ماه روز 15 راس ساعت 10:00 یادآوری ارسال شود
```

### تست 2: یادآوری چند روزه
```
روزهای انتخاب شده: 10, 20, 25
ساعت: 09:00

انتظار: هر ماه در روزهای 10، 20، 25 راس ساعت 09:00 یادآوری ارسال شود
```

### تست 3: یادآوری با محدودیت
```
روزهای انتخاب شده: 10, 20
ساعت: 14:00
حداکثر تعداد: 6

انتظار: 3 ماه کار کند (2 روز در ماه × 3 ماه = 6 بار)
```

### تست 4: روز 31 در ماه کوتاه
```
روزهای انتخاب شده: 31
ساعت: 10:00
ماه جاری: فوریه (28/29 روز)

انتظار: یادآوری در فوریه ارسال نشود، ماه بعدی منتظر بماند
```

---

## 📝 Query های مفید

### مشاهده یادآوری‌های ماهانه:
```sql
SELECT * 
FROM TaskReminderSchedule_Tbl 
WHERE ReminderType = 4 AND IsActive = 1;
```

### بررسی وضعیت ارسال:
```sql
SELECT 
    Title,
    ScheduledDaysOfMonth,
    SentCount,
    MaxSendCount,
    LastExecuted,
    CASE 
        WHEN MaxSendCount IS NULL THEN 'نامحدود'
        WHEN SentCount >= MaxSendCount THEN 'تمام شده'
        ELSE CONCAT(SentCount, '/', MaxSendCount)
    END AS Progress
FROM TaskReminderSchedule_Tbl
WHERE ReminderType = 4;
```

---

## 🎉 نتیجه

✅ قابلیت یادآوری ماهانه با انتخاب چند روز به طور کامل پیاده‌سازی شد!

**فایل‌های تغییر یافته:**
1. `TaskReminderSchedule.cs`
2. `TaskReminderViewModel.cs`
3. `TaskRepository.Reminders.cs`
4. `TaskReminderBackgroundService.cs`
5. `_AddReminderModal.cshtml`
6. Migration & SQL Script
7. Documentation

**قدم بعدی:**
- اجرای Migration
- تست در محیط Development
- بررسی عملکرد Background Service
- تست با سناریوهای مختلف
