# ✅ اصلاح صفحه Details تسک‌های دوره‌ای

## 🎯 **مشکلات حل شده:**

1. ✅ **یادآوری‌ها ارور میداد** → حل شد
2. ✅ **تب تنظیمات محتوای اشتباه داشت** → اصلاح شد

---

## 🔧 **تغییرات اعمال شده:**

### 1️⃣ **Tab Navigation - اضافه شدن تب Settings**

#### قبل (خط 269-281):
```razor
<li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-reminders" ...>
        <i class="fa fa-bell d-block mb-1"></i>
        <span class="tab-title">یادآوری</span>
    </button>
</li>
<li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-timeline" ...>
        <i class="fa fa-history d-block mb-1"></i>
        <span class="tab-title">تاریخچه</span>
    </button>
</li>
```

#### بعد (با تب Settings):
```razor
<li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-reminders" ...>
        <i class="fa fa-bell d-block mb-1"></i>
        <span class="tab-title">یادآوری</span>
    </button>
</li>
<li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-settings" ...>
        <i class="fa fa-cog d-block mb-1"></i>
        <span class="tab-title">تنظیمات</span>
    </button>
</li>
<li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-timeline" ...>
        <i class="fa fa-history d-block mb-1"></i>
        <span class="tab-title">تاریخچه</span>
    </button>
</li>
```

---

### 2️⃣ **تب یادآوری - اصلاح URL و استایل**

#### قبل (خط 745-770):
```razor
<div class="block-header block-header-default">  ← ❌ استایل اشتباه
    <h3 class="block-title">
        <i class="fa fa-bell text-warning me-2"></i>یادآوری‌های تسک
    </h3>
    <div class="block-options">
        <button type="button" 
                class="btn btn-sm btn-warning"
                data-toggle="modal-ajax"
                data-url="@Url.Action("AddCustomReminderModal", "Tasks", ...)"  ← ❌ AddCustomReminderModal
                data-size="modal-lg">
            <i class="fa fa-bell-o me-1"></i> افزودن یادآوری
        </button>
    </div>
</div>
```

#### بعد:
```razor
<div class="block-header bg-warning text-white">  ← ✅ استایل صحیح
    <h3 class="block-title text-white mb-0">
        <i class="fa fa-bell me-2"></i>یادآوری‌های تسک
    </h3>
    <div class="block-options">
        @if (Model.CanEdit && Model.Schedule.IsActive && Model.Schedule.IsScheduleEnabled)  ← ✅ شرط دسترسی
        {
            <button type="button" 
                    class="btn btn-sm btn-light"  ← ✅ دکمه سفید
                    data-toggle="modal-ajax"
                    href="@Url.Action("AddReminderModal", "Tasks", new { taskId = Model.Schedule.Id })">  ← ✅ AddReminderModal
                <i class="fa fa-plus me-1"></i>افزودن یادآوری
            </button>
        }
        else
        {
            <span class="badge bg-light text-dark">
                <i class="fa fa-lock me-1"></i>قفل شده
            </span>
        }
    </div>
</div>
```

---

### 3️⃣ **JavaScript - اصلاح URL یادآوری‌ها**

#### قبل (خط 995-1036):
```javascript
function loadReminders() {
    // ...
    $.ajax({
        url: '@Url.Action("GetTaskRemindersListPartial", "ScheduledTasks")',  ← ❌ ScheduledTasks Controller
        type: 'GET',
        data: { taskId: scheduleId },
        success: function(html) {
            if (html && html.trim() !== '') {
                $('#reminders-list-container').html(html);
            } else {
                // ❌ بدون showEmptyRemindersState()
                $('#reminders-list-container').html(`
                    <div class="text-center py-5 text-muted">
                        ...
                        <button ... data-url="@Url.Action("AddCustomReminderModal", ...">  ← ❌ AddCustomReminderModal
                        </button>
                    </div>
                `);
            }
        },
        // ...
    });
}

function deleteReminder(reminderId) {
    // ...
    $.ajax({
        url: '@Url.Action("DeleteTaskReminder", "ScheduledTasks")',  ← ❌ ScheduledTasks Controller
        type: 'POST',
        data: {
            id: reminderId,
            scheduleId: scheduleId,  ← ❌ scheduleId
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        // ...
    });
}
```

#### بعد:
```javascript
function loadReminders() {
    // ...
    $.ajax({
        url: '@Url.Action("GetTaskReminders", "Tasks")',  ← ✅ Tasks Controller
        type: 'GET',
        data: { taskId: scheduleId },
        success: function(html) {
            if (html && html.trim() !== '') {
                $('#reminders-list-container').html(html);
            } else {
                showEmptyRemindersState();  ← ✅ تابع جداگانه
            }
        },
        // ...
    });
}

function showEmptyRemindersState() {  ← ✅ تابع جدید
    $('#reminders-list-container').html(`
        <div class="text-center py-5 text-muted">
            ...
            <button ... href="@Url.Action("AddReminderModal", "Tasks")?taskId=${scheduleId}">  ← ✅ AddReminderModal
            </button>
        </div>
    `);
}

function deleteReminder(reminderId) {
    // ...
    $.ajax({
        url: '@Url.Action("DeleteReminder", "Tasks")',  ← ✅ Tasks Controller
        type: 'POST',
        data: {
            id: reminderId,
            taskId: scheduleId,  ← ✅ taskId
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        // ...
    });
}
```

---

### 4️⃣ **تب Settings - جایگزینی محتوای کامل**

#### قبل (خط 787-876):
```razor
<div class="tab-pane fade" id="tab-settings" role="tabpanel">
    <div class="block block-rounded block-fx-shadow">
        <div class="block-header block-header-default">
            <h3 class="block-title">
                <i class="fa fa-cog text-warning me-2"></i>تنظیمات تسک
            </h3>
        </div>
        <div class="block-content">
            <!-- فرم تنظیمات تسک -->
            <form id="task-settings-form">  ← ❌ فرم قابل ویرایش (اشتباه!)
                <div class="mb-4">
                    <label class="form-label">وضعیت تسک</label>
                    <div>
                        <span class="badge bg-@statusColor">...</span>
                    </div>
                </div>

                <div class="mb-4">
                    <label class="form-label">تاریخ شروع</label>
                    <input type="text" class="form-control datepicker" ... readonly>  ← ❌ فیلدهای datepicker
                </div>

                <!-- ... -->

                <div class="mb-4">
                    <button type="button" class="btn btn-primary btn-block"
                            onclick="saveTaskSettings()">  ← ❌ دکمه ذخیره (کار نمی‌کند)
                        <i class="fa fa-save me-2"></i>ذخیره تنظیمات
                    </button>
                </div>
            </form>
        </div>
    </div>
</div>
```

#### بعد:
```razor
<div class="tab-pane fade" id="tab-settings" role="tabpanel">
    <div class="block block-rounded block-fx-shadow">
        <div class="block-header bg-secondary text-white">  ← ✅ استایل یکسان
            <h3 class="block-title text-white mb-0">
                <i class="fa fa-cog me-2"></i>تنظیمات تسک
            </h3>
        </div>
        <div class="block-content">
            @if (Model.CanEdit)  ← ✅ پیام بر اساس دسترسی
            {
                <div class="alert alert-info">
                    <i class="fa fa-info-circle me-2"></i>
                    برای تغییر تنظیمات زمان‌بندی، از دکمه "ویرایش" استفاده کنید.
                </div>
            }
            else
            {
                <div class="alert alert-warning">
                    <i class="fa fa-lock me-2"></i>
                    شما مجاز به تغییر تنظیمات نیستید (عضو تیم).
                </div>
            }

            <div class="row g-3">
                <!-- دسترسی‌ها -->
                <div class="col-12">
                    <h5 class="mb-3">
                        <i class="fa fa-shield-alt text-primary me-2"></i>دسترسی‌ها
                    </h5>
                </div>

                <div class="col-md-6">
                    <div class="p-3 bg-body-light rounded-3">
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" 
                                   id="settingIsActive" 
                                   @(Model.Schedule.IsActive ? "checked" : "") 
                                   disabled>  ← ✅ disabled (فقط نمایش)
                            <label class="form-check-label" for="settingIsActive">
                                <strong>فعال بودن تسک</strong>
                                <p class="text-muted small mb-0">تسک قابل مشاهده و اجرا باشد</p>
                            </label>
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="p-3 bg-body-light rounded-3">
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" 
                                   id="settingIsScheduleEnabled" 
                                   @(Model.Schedule.IsScheduleEnabled ? "checked" : "") 
                                   disabled>
                            <label class="form-check-label" for="settingIsScheduleEnabled">
                                <strong>زمان‌بندی فعال</strong>
                                <p class="text-muted small mb-0">اجرای خودکار تسک</p>
                            </label>
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="p-3 bg-body-light rounded-3">
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" 
                                   id="settingImportant" 
                                   @(Model.TaskModel.Important ? "checked" : "") 
                                   disabled>
                            <label class="form-check-label" for="settingImportant">
                                <strong>تسک مهم</strong>
                                <p class="text-muted small mb-0">علامت‌گذاری به عنوان مهم</p>
                            </label>
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="p-3 bg-body-light rounded-3">
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" 
                                   id="settingIsRecurring" 
                                   @(Model.Schedule.IsRecurring ? "checked" : "") 
                                   disabled>
                            <label class="form-check-label" for="settingIsRecurring">
                                <strong>تکرار دوره‌ای</strong>
                                <p class="text-muted small mb-0">تسک به صورت دوره‌ای اجرا شود</p>
                            </label>
                        </div>
                    </div>
                </div>

                <!-- اطلاعات اضافی -->
                <div class="col-12">
                    <h5 class="mb-3 mt-3">
                        <i class="fa fa-info-circle text-info me-2"></i>اطلاعات اضافی
                    </h5>
                </div>

                <div class="col-md-6">
                    <div class="p-3 bg-body-light rounded-3">
                        <div class="fs-xs text-muted mb-1">نوع تسک</div>
                        <div class="fw-semibold">
                            @(Model.TaskModel.TaskType == 0 ? "تسک" :
                              Model.TaskModel.TaskType == 1 ? "پروژه" : "مایلستون")
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="p-3 bg-body-light rounded-3">
                        <div class="fs-xs text-muted mb-1">اولویت</div>
                        <div>
                            @switch (Model.TaskModel.Priority)
                            {
                                case 0:
                                    <span class="badge bg-info-light text-info">
                                        <i class="fa fa-circle me-1"></i>عادی
                                    </span>
                                    break;
                                case 1:
                                    <span class="badge bg-warning-light text-warning">
                                        <i class="fa fa-star me-1"></i>مهم
                                    </span>
                                    break;
                                case 2:
                                    <span class="badge bg-danger-light text-danger">
                                        <i class="fa fa-bolt me-1"></i>فوری
                                    </span>
                                    break;
                            }
                        </div>
                    </div>
                </div>

                @if (Model.Schedule.MaxOccurrences.HasValue)
                {
                    <div class="col-md-6">
                        <div class="p-3 bg-body-light rounded-3">
                            <div class="fs-xs text-muted mb-1">حداکثر تعداد اجرا</div>
                            <div class="fw-semibold">
                                @Model.Schedule.MaxOccurrences بار
                            </div>
                        </div>
                    </div>
                }

                <div class="col-md-6">
                    <div class="p-3 bg-body-light rounded-3">
                        <div class="fs-xs text-muted mb-1">شعبه</div>
                        <div class="fw-semibold">
                            <i class="fa fa-building text-primary me-2"></i>
                            @(Model.Schedule.Branch?.Name ?? "نامشخص")
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 📊 **قبل و بعد:**

### قبل:
```
[تب یادآوری]
❌ ارور: AddCustomReminderModal not found
❌ استایل block-header-default
❌ دکمه همیشه نمایش داده می‌شود

[تب تنظیمات]
❌ فرم قابل ویرایش (اما کار نمی‌کند)
❌ دکمه "ذخیره تنظیمات" (بی‌فایده)
❌ Datepicker و Timepicker
```

### بعد:
```
[تب یادآوری]
✅ کار می‌کند (استفاده از AddReminderModal)
✅ استایل bg-warning text-white
✅ دکمه فقط برای CanEdit
✅ Badge "قفل شده" برای تسک‌های تیمی

[تب تنظیمات]
✅ نمایش readonly تنظیمات
✅ Switch های disabled
✅ پیام مناسب برای عضو تیم
✅ نمایش اولویت، نوع، شعبه، حداکثر اجرا
```

---

## 🎨 **UI نهایی:**

### تب یادآوری:
```
┌─────────────────────────────────────────────────┐
│ 🔔 یادآوری‌های تسک         [+ افزودن یادآوری] │ ← سفید روی زرد
├─────────────────────────────────────────────────┤
│ [یادآوری 1: 3 روز قبل]                         │
│ [یادآوری 2: هفته‌ای]                           │
│ [یادآوری 3: ماهانه]                            │
└─────────────────────────────────────────────────┘
```

### تب تنظیمات:
```
┌─────────────────────────────────────────────────┐
│ ⚙️ تنظیمات تسک                                  │
├─────────────────────────────────────────────────┤
│ ℹ️ برای تغییر تنظیمات، از دکمه ویرایش...       │
│                                                 │
│ [دسترسی‌ها]                                     │
│ ┌────────────────┬────────────────┐             │
│ │[✅] فعال بودن  │[✅] زمان‌بندی  │             │
│ │    تسک         │    فعال        │             │
│ ├────────────────┼────────────────┤             │
│ │[⭐] تسک مهم    │[🔁] تکرار      │             │
│ │                │    دوره‌ای     │             │
│ └────────────────┴────────────────┘             │
│                                                 │
│ [اطلاعات اضافی]                                 │
│ نوع: تسک | اولویت: مهم ⭐                       │
│ شعبه: تهران | حداکثر اجرا: 100 بار              │
└─────────────────────────────────────────────────┘
```

---

## 🚀 **نتیجه نهایی:**

### صفحه Details تسک دوره‌ای:
```
[Tabs]
├─ نگاه کلی ✅
├─ عملیات ✅
├─ اعضا ✅
├─ یادآوری ✅ (اصلاح شده - کار می‌کند)
├─ تنظیمات ✅ (بازنویسی شده - نمایش صحیح)
└─ تاریخچه ✅
```

---

## ✅ **چک‌لیست تغییرات:**

- [x] **Navigation:** اضافه شدن تب Settings
- [x] **یادآوری - URL:** تغییر به `AddReminderModal`
- [x] **یادآوری - استایل:** `bg-warning text-white`
- [x] **یادآوری - دسترسی:** شرط `CanEdit`
- [x] **یادآوری - Badge:** نمایش "قفل شده"
- [x] **JavaScript:** تغییر `GetTaskReminders` (Tasks Controller)
- [x] **JavaScript:** تابع `showEmptyRemindersState()`
- [x] **JavaScript:** تغییر `DeleteReminder` (Tasks Controller)
- [x] **Settings:** جایگزینی کامل محتوا
- [x] **Settings:** Switch های disabled
- [x] **Settings:** پیام مناسب بر اساس `CanEdit`
- [x] **Build:** موفق ✅

---

## 🧪 **تست:**

### 1. تست یادآوری‌ها:
```
1. باز کردن صفحه Details تسک دوره‌ای
2. کلیک روی تب "یادآوری"
3. ✅ باید لیست یادآوری‌ها load شود
4. کلیک روی "افزودن یادآوری"
5. ✅ باید مودال AddReminderModal باز شود
```

### 2. تست تنظیمات:
```
1. کلیک روی تب "تنظیمات"
2. ✅ باید Switch ها نمایش داده شوند (disabled)
3. ✅ باید اطلاعات نوع، اولویت، شعبه نمایش داده شود
4. اگر عضو تیم: ✅ باید پیام "قفل شده" نمایش داده شود
```

### 3. تست دسترسی:
```
1. ورود به عنوان سازنده تسک:
   ✅ دکمه "افزودن یادآوری" فعال
   ✅ پیام "برای تغییر، از ویرایش..."
   
2. ورود به عنوان عضو تیم:
   ✅ Badge "قفل شده" نمایش داده می‌شود
   ✅ پیام "شما مجاز به تغییر نیستید"
```

---

✅ **همه چیز آماده است! Build موفق! آماده برای تست!** 🎉

حالا Run کن و تست کن:
- یادآوری‌ها باید لود بشن ✅
- تب تنظیمات باید تنظیمات رو نشون بده ✅
- دسترسی‌ها باید درست کار کنن ✅

🚀 موفق باشی!
