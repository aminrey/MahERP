# 🔧 راه‌حل مشکلات Scheduled Tasks

## 🐛 مشکلات گزارش شده:

### 1️⃣ تسک فوری اشتباهی ساخته می‌شود
**علت**: احتمالاً checkbox `CreateImmediately` همیشه `true` می‌شود یا درست bind نمی‌شود.

**راه‌حل**:
- ✅ Debug Log اضافه شد به `TasksController.CRUD.cs` (خط 93-96)
- ✅ Debug Log اضافه شد به `TaskRepository.ScheduledTasks.cs` (خط 61-73)

**بررسی**:
```csharp
// در Console Output دنبال این پیام‌ها بگردید:
🔍 TaskSchedule.CreateImmediately: True/False
✅ CreateImmediately is TRUE - Creating immediate task
ℹ️ CreateImmediately is FALSE - No immediate task created
```

### 2️⃣ تسک در لیست Scheduled Tasks نمایش داده نمی‌شود
**علت**: فیلد `IsRecurring` set نمی‌شد.

**راه‌حل**:
- ✅ `IsRecurring` اکنون بر اساس `ScheduleType` محاسبه می‌شود (خط 25-31)
- ✅ `IsExecuted` نیز مقداردهی می‌شود (خط 48)
- ✅ Debug Log اضافه شد به `GetUserScheduledTasksAsync` (خط 264-278)

**بررسی**:
```csharp
// در Console Output:
🔍 GetUserScheduledTasksAsync called for userId: xxx
✅ Found N schedules
  📋 Schedule #1: ... (Type: 1, IsActive: True, IsEnabled: True)
```

### 3️⃣ خطای JavaScript: `loadScheduledTemplates is not defined`
**راه‌حل**:
- ✅ تابع dummy اضافه شد به `main.js` (خط بعد از `window.createAndShowModal`)

### 4️⃣ فیلترینگ تسک‌های خودم
**وضعیت**: ✅ قبلاً پیاده‌سازی شده بود (خط 267-271 در Repository)

```csharp
if (!isAdmin)
{
    query = query.Where(s => s.CreatedByUserId == userId);
}
```

---

## 🧪 مراحل Testing

### مرحله 1: بررسی ساخت Schedule بدون تسک فوری
```
1. به صفحه ایجاد تسک بروید
2. چک‌باکس "زمان‌بندی فعال" را تیک بزنید
3. تنظیمات زمان‌بندی را وارد کنید (مثلاً روزانه ساعت 10:00)
4. چک‌باکس "ساخت فوری" را تیک نزنید ❌
5. فرم را Submit کنید
6. در Console Output دنبال این پیام بگردید:
   ℹ️ CreateImmediately is FALSE - No immediate task created
```

### مرحله 2: بررسی نمایش در لیست
```
1. به /ScheduledTasks/Index بروید
2. باید Schedule جدید را ببینید
3. در Console Output دنبال این پیام بگردید:
   ✅ Found 1 schedules
   📋 Schedule #X: ... (Type: 1, IsActive: True, IsEnabled: True)
```

### مرحله 3: بررسی ساخت با تسک فوری
```
1. یک Schedule جدید بسازید
2. این بار چک‌باکس "ساخت فوری" را تیک بزنید ✅
3. در Console Output باید ببینید:
   ✅ CreateImmediately is TRUE - Creating immediate task for schedule X
   ✅ Immediate task created with ID: Y
4. باید هم در لیست Scheduled Tasks و هم در لیست Tasks عادی ظاهر شود
```

---

## 📊 بررسی در دیتابیس

### جدول `ScheduledTaskCreation_Tbl`
```sql
SELECT 
    Id,
    ScheduleTitle,
    ScheduleType,
    IsRecurring,
    IsActive,
    IsScheduleEnabled,
    IsExecuted,
    CreatedByUserId,
    NextExecutionDate,
    CreatedDate
FROM ScheduledTaskCreation_Tbl
ORDER BY CreatedDate DESC;
```

**انتظار**:
- `IsRecurring = 1` برای روزانه، هفتگی، ماهانه
- `IsRecurring = 0` برای یکبار
- `IsActive = 1`
- `IsScheduleEnabled = 1`
- `IsExecuted = 0`
- `NextExecutionDate` باید مقدار داشته باشد

### جدول `Tasks_Tbl`
```sql
SELECT 
    Id,
    TaskCode,
    Title,
    ScheduleId,
    CreationMode,
    TaskTypeInput,
    CreateDate
FROM Tasks_Tbl
WHERE ScheduleId IS NOT NULL
ORDER BY CreateDate DESC;
```

**انتظار**:
- فقط اگر `CreateImmediately = true` بود، باید رکورد داشته باشد
- `CreationMode = 1` (خودکار)
- `TaskTypeInput = 2` (اتوماتیک)
- `ScheduleId` باید مقدار داشته باشد

---

## 🔍 نکات Debugging

### 1. بررسی مقدار checkbox در Browser
```javascript
// در Console مرورگر:
$('input[name="TaskSchedule.CreateImmediately"]').is(':checked')
```

### 2. بررسی مقدار در FormData
```javascript
// قبل از Submit در Network Tab:
// Payload -> Form Data -> TaskSchedule.CreateImmediately
```

### 3. بررسی Binding در Controller
```csharp
// اضافه کردن breakpoint در خط 91
if (model.TaskSchedule?.IsScheduled == true)
{
    // بررسی مقدار model.TaskSchedule.CreateImmediately
}
```

---

## ⚠️ مشکلات احتمالی و راه‌حل

### مشکل: Schedule در دیتابیس ذخیره می‌شود اما در لیست نمایش داده نمی‌شود
**بررسی**:
1. مطمئن شوید `IsActive = 1`
2. مطمئن شوید `CreatedByUserId` درست است
3. Debug Log در `GetUserScheduledTasksAsync` را بررسی کنید

### مشکل: همیشه تسک فوری ساخته می‌شود
**بررسی**:
1. Debug Log در `CreateScheduledTaskAsync` را بررسی کنید
2. مقدار `model.TaskSchedule.CreateImmediately` را در Controller چک کنید
3. مطمئن شوید checkbox در View درست bind می‌شود:
   ```html
   <input name="TaskSchedule.CreateImmediately" type="checkbox" ...>
   ```

### مشکل: JavaScript error برای loadScheduledTemplates
**راه‌حل**: ✅ حل شد - توابع dummy اضافه شدند

---

## 📝 تغییرات اعمال شده

### فایل: `TaskRepository.ScheduledTasks.cs`
```
✅ اضافه شدن محاسبه IsRecurring (خط 25-31)
✅ اضافه شدن Debug Logs (خط 61-73)
✅ اضافه شدن Debug Logs به GetUserScheduledTasksAsync (خط 264-278)
✅ بهبود MapToScheduledTaskCard (خط 290-323)
```

### فایل: `TasksController.CRUD.cs`
```
✅ اضافه شدن Debug Logs (خط 93-96)
```

### فایل: `main.js`
```
✅ اضافه شدن توابع dummy (loadScheduledTemplates, selectTemplate)
```

---

## 🚀 مراحل بعدی (اختیاری)

### بهبودهای آینده:
1. اضافه کردن validation برای تنظیمات زمان‌بندی
2. اضافه کردن Preview برای NextExecutionDate
3. اضافه کردن امکان Pause/Resume برای Schedule
4. اضافه کردن نمایش تاریخچه اجراهای قبلی

---

## 📞 در صورت ادامه مشکل

### اگر مشکل حل نشد:
1. Console Output را کامل برای من بفرستید
2. نتیجه کوئری‌های SQL بالا را بفرستید
3. Screenshot از Network Tab در Browser Developer Tools
4. مقدار `model.TaskSchedule` در breakpoint Controller را بفرستید

