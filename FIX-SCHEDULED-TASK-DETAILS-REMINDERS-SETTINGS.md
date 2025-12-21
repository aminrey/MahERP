# 🔧 اصلاح صفحه Details تسک‌های دوره‌ای

## 🎯 **مشکلات:**

1. ❌ **یادآوری‌ها لود نمیشه** - ارور میده
2. ❌ **تب تنظیمات وجود ندارد** - باید مثل Details تسک عادی باشه

---

## ✅ **راه‌حل:**

### 1️⃣ **اصلاح تب یادآوری‌ها**

#### مشکل فعلی (خط 745-770):
```razor
<!-- Tab: یادآوری -->
<div class="tab-pane fade" id="tab-reminders" role="tabpanel">
    <div class="block block-rounded block-fx-shadow">
        <div class="block-header block-header-default">
            <h3 class="block-title">
                <i class="fa fa-bell text-warning me-2"></i>یادآوری‌های تسک
            </h3>
            <div class="block-options">
                <button type="button" 
                        class="btn btn-sm btn-warning"
                        data-toggle="modal-ajax"
                        data-url="@Url.Action("AddCustomReminderModal", "Tasks", ...)"  ← ❌ AddCustomReminderModal وجود ندارد
                        data-size="modal-lg">
                    <i class="fa fa-bell-o me-1"></i> افزودن یادآوری
                </button>
            </div>
        </div>
        <div class="block-content" id="reminders-list-container">
            <!-- محتوا از AJAX بارگذاری می‌شود -->
            <div class="text-center py-3">
                <i class="fa fa-spinner fa-spin fa-2x text-muted"></i>
                <p class="mt-2 text-muted">در حال بارگذاری یادآوری‌ها...</p>
            </div>
        </div>
    </div>
</div>
```

#### راه‌حل (جایگزینی):
```razor
<!-- Tab: یادآوری -->
<div class="tab-pane fade" id="tab-reminders" role="tabpanel">
    <div class="block block-rounded block-fx-shadow">
        <div class="block-header bg-warning text-white">  ← ✅ استایل یکسان با تسک عادی
            <h3 class="block-title text-white mb-0">
                <i class="fa fa-bell me-2"></i>یادآوری‌های تسک
            </h3>
            <div class="block-options">
                @if (Model.CanEdit && Model.Schedule.IsActive && Model.Schedule.IsScheduleEnabled)
                {
                    <button type="button" 
                            class="btn btn-sm btn-light"  ← ✅ دکمه سفید روی پس‌زمینه زرد
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
        <div class="block-content" id="reminders-list-container">
            <!-- محتوا از AJAX بارگذاری می‌شود -->
            <div class="text-center py-3">
                <i class="fa fa-spinner fa-spin fa-2x text-muted"></i>
                <p class="mt-2 text-muted">در حال بارگذاری یادآوری‌ها...</p>
            </div>
        </div>
    </div>
</div>
```

---

### 2️⃣ **اضافه کردن تب Settings**

#### محل: بعد از تب reminders (خط 770)، قبل از تب timeline

```razor
<!-- Tab: تنظیمات -->
<div class="tab-pane fade" id="tab-settings" role="tabpanel">
    <div class="block block-rounded block-fx-shadow">
        <div class="block-header bg-secondary text-white">
            <h3 class="block-title text-white mb-0">
                <i class="fa fa-cog me-2"></i>تنظیمات تسک
            </h3>
        </div>
        <div class="block-content">
            @if (Model.CanEdit)
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
                                   disabled>
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

### 3️⃣ **اضافه کردن تب Settings به Navigation**

#### محل: خط 220 (Tabs Navigation)

```razor
<li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-reminders"
            type="button" role="tab">
        <i class="fa fa-bell d-block mb-1"></i>
        <span class="tab-title">یادآوری</span>
    </button>
</li>
<!-- ⭐⭐⭐ NEW: تب تنظیمات -->
<li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-settings"
            type="button" role="tab">
        <i class="fa fa-cog d-block mb-1"></i>
        <span class="tab-title">تنظیمات</span>
    </button>
</li>
<li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-timeline"
            type="button" role="tab">
        <i class="fa fa-history d-block mb-1"></i>
        <span class="tab-title">تاریخچه</span>
    </button>
</li>
```

---

### 4️⃣ **اصلاح JavaScript برای load یادآوری‌ها**

#### محل: بخش Scripts (خط 900+)

```javascript
// ⭐⭐⭐ یادآوری‌ها
function loadReminders() {
    $('#reminders-list-container').html(`
        <div class="text-center py-3">
            <i class="fa fa-spinner fa-spin fa-2x text-muted"></i>
            <p class="mt-2 text-muted">در حال بارگذاری یادآوری‌ها...</p>
        </div>
    `);

    $.ajax({
        url: '@Url.Action("GetTaskReminders", "Tasks")',  // ⭐ استفاده از Controller تسک عادی
        type: 'GET',
        data: { taskId: scheduleId },
        success: function(html) {
            if (html && html.trim() !== '') {
                $('#reminders-list-container').html(html);
            } else {
                showEmptyRemindersState();
            }
        },
        error: function() {
            $('#reminders-list-container').html(`
                <div class="alert alert-danger">
                    <i class="fa fa-exclamation-triangle me-2"></i>
                    خطا در بارگذاری یادآوری‌ها
                </div>
            `);
        }
    });
}

function showEmptyRemindersState() {
    $('#reminders-list-container').html(`
        <div class="text-center py-5 text-muted">
            <i class="fa fa-bell-slash fa-3x opacity-25 mb-3"></i>
            <p class="mb-0">یادآوری‌ای تعریف نشده است</p>
            <button type="button" 
                    class="btn btn-warning mt-3"
                    data-toggle="modal-ajax"
                    href="@Url.Action("AddReminderModal", "Tasks")?taskId=${scheduleId}">
                <i class="fa fa-plus me-1"></i> افزودن اولین یادآوری
            </button>
        </div>
    `);
}

// بارگذاری یادآوری‌ها هنگام باز شدن تب
$('button[data-bs-target="#tab-reminders"]').on('shown.bs.tab', function () {
    loadReminders();
});
```

---

## 📊 **قبل و بعد:**

### قبل:
```
[تب یادآوری]
❌ ارور: AddCustomReminderModal not found
❌ استایل متفاوت با تسک عادی
❌ تب تنظیمات وجود ندارد
```

### بعد:
```
[تب یادآوری]
✅ کار می‌کند (استفاده از AddReminderModal)
✅ استایل یکسان با تسک عادی
✅ دکمه قفل برای تسک‌های تیمی

[تب تنظیمات] (جدید)
✅ نمایش تمام تنظیمات
✅ Switch های disabled برای نمایش
✅ پیام مناسب برای عضو تیم
```

---

## 🔧 **چک‌لیست تغییرات:**

- [ ] **یادآوری‌ها:** تغییر `AddCustomReminderModal` به `AddReminderModal`
- [ ] **یادآوری‌ها:** تغییر استایل header به `bg-warning text-white`
- [ ] **یادآوری‌ها:** اضافه کردن شرط `CanEdit`
- [ ] **تنظیمات:** اضافه کردن تب جدید `tab-settings`
- [ ] **تنظیمات:** اضافه کردن navigation button
- [ ] **JavaScript:** تغییر AJAX URL از `GetTaskRemindersListPartial` به `GetTaskReminders`
- [ ] **JavaScript:** اضافه کردن `showEmptyRemindersState()`

---

## 🚀 **نتیجه نهایی:**

### صفحه Details تسک دوره‌ای:
```
[Tabs]
├─ نگاه کلی ✅
├─ عملیات ✅
├─ اعضا ✅
├─ یادآوری ✅ (اصلاح شده)
├─ تنظیمات ✅ (جدید)
└─ تاریخچه ✅
```

### UI یادآوری‌ها:
```
┌─────────────────────────────────────────────┐
│ 🔔 یادآوری‌های تسک        [+ افزودن یادآوری]│
├─────────────────────────────────────────────┤
│ [یادآوری 1]                                 │
│ [یادآوری 2]                                 │
│ [یادآوری 3]                                 │
└─────────────────────────────────────────────┘
```

### UI تنظیمات:
```
┌─────────────────────────────────────────────┐
│ ⚙️ تنظیمات تسک                              │
├─────────────────────────────────────────────┤
│ ℹ️ برای تغییر تنظیمات، از دکمه ویرایش...    │
│                                             │
│ [✅ فعال بودن تسک]     [✅ زمان‌بندی فعال]   │
│ [⭐ تسک مهم]           [🔁 تکرار دوره‌ای]   │
│                                             │
│ نوع: تسک | اولویت: مهم | شعبه: تهران        │
└─────────────────────────────────────────────┘
```

---

✅ **آماده برای اعمال تغییرات!**

این فایل راهنمای کامل تغییرات است. برای اعمال:
1. باز کردن `Details.cshtml`
2. پیدا کردن خط 745 (تب یادآوری)
3. جایگزینی کد
4. اضافه کردن تب Settings بعد از reminders
5. اضافه کردن JavaScript

🎉 پس از اعمال، تست کنید!
