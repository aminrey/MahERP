# 🐛 NullReferenceException در ConvertShamsiToMiladi

## ❌ **مشکل:**

```
Exception Type: System.NullReferenceException
Exception Message: Object reference not set to an instance of an object.
Location: MahERP.CommonLayer\PublicClasses\ConvertDateTime.cs
Line: PersianDateTime persianDateTime = PersianDateTime.Parse(date);
```

### 🔍 **علت:**

در خط **52** و **53** فایل `TaskRepository.ScheduledTasks.cs`:

```csharp
StartDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladi(
    model.TaskSchedule?.StartDatePersian),  // ⚠️ می‌تواند null باشد
EndDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladi(
    model.TaskSchedule?.EndDatePersian),    // ⚠️ می‌تواند null باشد
```

**مشکل:** `StartDatePersian` و `EndDatePersian` برای تسک‌های روزانه/هفتگی/ماهانه **اختیاری** هستند و می‌توانند `null` باشند. اما متد `ConvertShamsiToMiladi` نمی‌تواند با `null` کار کند.

---

## ✅ **راه‌حل اعمال شده:**

### 1️⃣ **اضافه کردن متد Nullable-Safe**

در فایل `ConvertDateTime.cs`:

```csharp
/// <summary>
/// ⭐⭐⭐ تبدیل تاریخ شمسی (nullable string) به میلادی (nullable DateTime)
/// </summary>
public static DateTime? ConvertShamsiToMiladiNullable(string? date)
{
    if (string.IsNullOrWhiteSpace(date))
    {
        return null;
    }

    try
    {
        PersianDateTime persianDateTime = PersianDateTime.Parse(date);
        return persianDateTime.ToDateTime();
    }
    catch
    {
        return null;
    }
}
```

### 2️⃣ **بهبود متد اصلی**

```csharp
public static DateTime ConvertShamsiToMiladi(string date)
{
    if (string.IsNullOrWhiteSpace(date))
    {
        throw new ArgumentNullException(nameof(date), "تاریخ نمی‌تواند خالی باشد");
    }

    PersianDateTime persianDateTime = PersianDateTime.Parse(date);
    return persianDateTime.ToDateTime();
}
```

### 3️⃣ **بروزرسانی Repository**

در `TaskRepository.ScheduledTasks.cs`:

#### متد `CreateScheduledTaskAsync`:
```csharp
// ⭐⭐⭐ استفاده از متد nullable-safe
StartDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(
    model.TaskSchedule?.StartDatePersian),
EndDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(
    model.TaskSchedule?.EndDatePersian),
```

#### متد `UpdateScheduledTaskAsync`:
```csharp
// ⭐⭐⭐ استفاده از متد nullable-safe
schedule.StartDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(
    taskModel.TaskSchedule?.StartDatePersian);
schedule.EndDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(
    taskModel.TaskSchedule?.EndDatePersian);
```

---

## 📝 **فایل‌های تغییر یافته:**

1. ✅ `MahERP.CommonLayer\PublicClasses\ConvertDateTime.cs`
   - اضافه شدن متد `ConvertShamsiToMiladiNullable`
   - بهبود متد `ConvertShamsiToMiladi` با بررسی null

2. ✅ `MahERP.DataModelLayer\Repository\TaskRepository\TaskRepository.ScheduledTasks.cs`
   - بروزرسانی `CreateScheduledTaskAsync`
   - بروزرسانی `UpdateScheduledTaskAsync`

---

## 🧪 **تست:**

### سناریو 1: تسک روزانه بدون تاریخ شروع/پایان ✅
```
- نوع: روزانه
- ساعت: 09:00
- StartDate: خالی ✅
- EndDate: خالی ✅
- نتیجه: باید بدون خطا ذخیره شود
```

### سناریو 2: تسک یکبار با تاریخ مشخص ✅
```
- نوع: یکبار
- ساعت: 14:00
- تاریخ اجرا: 1403/10/25 ✅
- نتیجه: باید بدون خطا ذخیره شود
```

### سناریو 3: تسک هفتگی با بازه زمانی ✅
```
- نوع: هفتگی
- روزها: دوشنبه، چهارشنبه، جمعه
- ساعت: 10:00
- StartDate: 1403/10/01 ✅
- EndDate: 1403/12/30 ✅
- نتیجه: باید بدون خطا ذخیره شود
```

---

## 🚀 **مراحل بعدی:**

1. ✅ **Stop Debugging** و **Build** مجدد پروژه
2. ✅ **Run** برنامه
3. ✅ تست سناریوهای بالا
4. ✅ بررسی Console Output
5. ✅ بررسی دیتابیس

---

## 📊 **بررسی در دیتابیس:**

```sql
SELECT 
    Id,
    ScheduleTitle,
    ScheduleType,
    StartDate,
    EndDate,
    NextExecutionDate,
    IsActive,
    IsScheduleEnabled
FROM ScheduledTaskCreation_Tbl
ORDER BY CreatedDate DESC;
```

**انتظار:**
- `StartDate` می‌تواند NULL باشد ✅
- `EndDate` می‌تواند NULL باشد ✅
- `NextExecutionDate` باید مقدار داشته باشد ✅

---

## ⚠️ **نکات مهم:**

1. **تاریخ شروع/پایان اختیاری است** برای تسک‌های تکراری
2. **فقط `NextExecutionDate` الزامی است** برای اجرای Schedule
3. **متد جدید `ConvertShamsiToMiladiNullable`** باید در جاهای دیگر هم استفاده شود که احتمال null وجود دارد

---

## 🔍 **اگر مشکل ادامه داشت:**

1. مطمئن شوید Build موفق بوده
2. Clean Solution و Build مجدد
3. مطمئن شوید Debugger متوقف شده و برنامه مجدد اجرا شده
4. Console Output را بررسی کنید
5. Breakpoint در متد `ConvertShamsiToMiladiNullable` بگذارید

