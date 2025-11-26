# 📋 راهنمای اضافه کردن قابلیت‌های یادآوری، عملیات و اعضا به Details

## 🎯 تغییرات مورد نیاز

### 1️⃣ **Tab یادآوری‌ها** (Tab: `#tab-reminders`)

**جایگزین کردن محتوای فعلی با:**

```razor
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
                        data-url="@Url.Action("AddReminderModal", "Tasks", new { taskId = Model.TaskModel.Id })"
                        data-size="modal-lg">
                    <i class="fa fa-bell-o me-1"></i> افزودن یادآوری
                </button>
            </div>
        </div>
        <div class="block-content" id="reminders-list-container">
            <!-- محتوا از Partial View بارگذاری می‌شود -->
            <div class="text-center py-3">
                <i class="fa fa-spinner fa-spin fa-2x text-muted"></i>
                <p class="mt-2 text-muted">در حال بارگذاری یادآوری‌ها...</p>
            </div>
        </div>
    </div>
</div>
```

### 2️⃣ **Tab عملیات** (Tab: `#tab-operations`)

**جایگزین کردن محتوای فعلی با:**

```razor
<div class="tab-pane fade" id="tab-operations" role="tabpanel">
    <div class="block block-rounded block-fx-shadow">
        <div class="block-header block-header-default">
            <h3 class="block-title">
                <i class="fa fa-cogs text-primary me-2"></i>عملیات‌های تسک
            </h3>
            <div class="block-options">
                <button type="button" 
                        class="btn btn-sm btn-primary"
                        data-toggle="modal-ajax"
                        data-url="@Url.Action("AddOperationModal", "Tasks", new { taskId = Model.TaskModel.Id })"
                        data-size="modal-lg">
                    <i class="fa fa-plus me-1"></i> افزودن عملیات
                </button>
            </div>
        </div>
        <div class="block-content" id="operations-list-container">
            <!-- محتوا از Partial View بارگذاری می‌شود -->
            @if (Model.TaskModel.Operations?.Any() == true)
            {
                <div class="list-group">
                    @foreach (var operation in Model.TaskModel.Operations.OrderBy(o => o.OperationOrder))
                    {
                        <div class="list-group-item">
                            <div class="d-flex align-items-center justify-content-between">
                                <div class="d-flex align-items-center flex-grow-1">
                                    <span class="badge bg-primary me-3">@operation.OperationOrder</span>
                                    <div class="flex-grow-1">
                                        <h6 class="mb-1">@operation.Title</h6>
                                        @if (!string.IsNullOrEmpty(operation.Description))
                                        {
                                            <p class="text-muted mb-0 small">@operation.Description</p>
                                        }
                                    </div>
                                    @if (operation.EstimatedHours.HasValue)
                                    {
                                        <span class="badge bg-info me-3">
                                            <i class="fa fa-clock me-1"></i>
                                            @operation.EstimatedHours ساعت
                                        </span>
                                    }
                                </div>
                                <div class="btn-group btn-group-sm">
                                    <button type="button" 
                                            class="btn btn-sm btn-outline-primary"
                                            data-toggle="modal-ajax"
                                            data-url="@Url.Action("EditOperationModal", "Tasks", new { operationId = operation.Id })"
                                            title="ویرایش">
                                        <i class="fa fa-edit"></i>
                                    </button>
                                    <button type="button" 
                                            class="btn btn-sm btn-outline-danger"
                                            onclick="deleteOperation(@operation.Id)"
                                            title="حذف">
                                        <i class="fa fa-trash"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            }
            else
            {
                <div class="text-center py-5 text-muted">
                    <i class="fa fa-cogs fa-3x opacity-25 mb-3"></i>
                    <p class="mb-0">عملیاتی تعریف نشده است</p>
                    <button type="button" 
                            class="btn btn-primary mt-3"
                            data-toggle="modal-ajax"
                            data-url="@Url.Action("AddOperationModal", "Tasks", new { taskId = Model.TaskModel.Id })">
                        <i class="fa fa-plus me-1"></i> افزودن اولین عملیات
                    </button>
                </div>
            }
        </div>
    </div>
</div>
```

### 3️⃣ **Tab اعضا** (Tab: `#tab-team`)

**جایگزین کردن محتوای فعلی با:**

```razor
<div class="tab-pane fade" id="tab-team" role="tabpanel">
    <div class="block block-rounded block-fx-shadow">
        <div class="block-header block-header-default">
            <h3 class="block-title">
                <i class="fa fa-users text-info me-2"></i>اعضای منتصب شده
            </h3>
            <div class="block-options">
                <button type="button" 
                        class="btn btn-sm btn-info"
                        data-toggle="modal-ajax"
                        data-url="@Url.Action("AssignUserToTaskModal", "Tasks", new { taskId = Model.TaskModel.Id })"
                        data-size="modal-lg">
                    <i class="fa fa-user-plus me-1"></i> اضافه کردن عضو
                </button>
            </div>
        </div>
        <div class="block-content" id="team-list-container">
            @if (Model.TaskModel.AssignmentsTaskUser?.Any() == true)
            {
                <div class="row g-3">
                    @foreach (var user in Model.TaskModel.AssignmentsTaskUser)
                    {
                        <div class="col-md-6">
                            <div class="card border">
                                <div class="card-body p-3">
                                    <div class="d-flex align-items-center justify-content-between">
                                        <div class="d-flex align-items-center flex-grow-1">
                                            <img src="@(user.AssignedUserProfileImage ?? "/images/default-avatar.png")"
                                                 class="rounded-circle me-3"
                                                 style="width: 48px; height: 48px; object-fit: cover;"
                                                 alt="@user.AssignedUserName">
                                            <div class="flex-grow-1">
                                                <h6 class="mb-1">@user.AssignedUserName</h6>
                                                @if (!string.IsNullOrEmpty(user.TeamName))
                                                {
                                                    <small class="text-muted">
                                                        <i class="fa fa-users me-1"></i>@user.TeamName
                                                    </small>
                                                }
                                            </div>
                                        </div>
                                        <button type="button" 
                                                class="btn btn-sm btn-outline-danger"
                                                onclick="removeAssignment(@user.Id)"
                                                title="حذف از تسک">
                                            <i class="fa fa-times"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            }
            else
            {
                <div class="text-center py-5 text-muted">
                    <i class="fa fa-users-slash fa-3x opacity-25 mb-3"></i>
                    <p class="mb-0">هیچ کاربری منتصب نشده است</p>
                    <button type="button" 
                            class="btn btn-info mt-3"
                            data-toggle="modal-ajax"
                            data-url="@Url.Action("AssignUserToTaskModal", "Tasks", new { taskId = Model.TaskModel.Id })">
                        <i class="fa fa-user-plus me-1"></i> اضافه کردن اولین عضو
                    </button>
                </div>
            }
        </div>
    </div>
</div>
```

---

## 🔧 **افزودن JavaScript ها**

**در بخش `@section Scripts`، اضافه کنید:**

```javascript
// ⭐⭐⭐ بارگذاری یادآوری‌ها هنگام کلیک روی تب
$('button[data-bs-target="#tab-reminders"]').on('shown.bs.tab', function () {
    loadReminders();
});

// ⭐ بارگذاری یادآوری‌ها
function loadReminders() {
    $.ajax({
        url: '@Url.Action("GetTaskReminders", "Tasks", new { taskId = Model.TaskModel.Id })',
        type: 'GET',
        success: function(html) {
            $('#reminders-list-container').html(html);
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

// ⭐ حذف یادآوری
function deleteReminder(reminderId) {
    if (!confirm('آیا از حذف این یادآوری اطمینان دارید؟')) {
        return;
    }

    $.ajax({
        url: '@Url.Action("DeleteTaskReminder", "Tasks")',
        type: 'POST',
        data: {
            id: reminderId,
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.status === 'success' || response.success) {
                toastr.success('یادآوری حذف شد');
                loadReminders(); // بارگذاری مجدد
            } else {
                toastr.error('خطا در حذف یادآوری');
            }
        },
        error: function() {
            toastr.error('خطا در حذف یادآوری');
        }
    });
}

// ⭐ حذف عملیات
function deleteOperation(operationId) {
    if (!confirm('آیا از حذف این عملیات اطمینان دارید؟')) {
        return;
    }

    $.ajax({
        url: '@Url.Action("DeleteOperation", "Tasks")',
        type: 'POST',
        data: {
            id: operationId,
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.status === 'success' || response.success) {
                toastr.success('عملیات حذف شد');
                location.reload(); // رفرش صفحه
            } else {
                toastr.error('خطا در حذف عملیات');
            }
        },
        error: function() {
            toastr.error('خطا در حذف عملیات');
        }
    });
}

// ⭐ حذف تخصیص کاربر
function removeAssignment(assignmentId) {
    if (!confirm('آیا از حذف این کاربر از تسک اطمینان دارید؟')) {
        return;
    }

    $.ajax({
        url: '@Url.Action("RemoveAssignment", "Tasks")',
        type: 'POST',
        data: {
            assignmentId: assignmentId,
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.status === 'success' || response.success) {
                toastr.success('کاربر از تسک حذف شد');
                location.reload(); // رفرش صفحه
            } else {
                toastr.error('خطا در حذف کاربر');
            }
        },
        error: function() {
            toastr.error('خطا در حذف کاربر');
        }
    });
}
```

---

## 📌 **نکات مهم**

### ✅ **متدهای استفاده شده (از همان Controller فعلی `TasksController`):**

| متد | استفاده |
|-----|---------|
| `AddReminderModal` | نمایش مودال افزودن یادآوری |
| `SaveReminder` | ذخیره یادآوری جدید |
| `GetTaskReminders` | دریافت لیست یادآوری‌ها |
| `DeleteTaskReminder` | حذف یادآوری |
| `AssignUserToTaskModal` | نمایش مودال اضافه کردن کاربر |
| `AssignUserToTask` | ذخیره تخصیص کاربر |
| `RemoveAssignment` | حذف تخصیص کاربر |

### ⚠️ **متدهای مورد نیاز برای عملیات (باید اضافه شوند):**

```csharp
// در TasksController

[HttpGet]
public async Task<IActionResult> AddOperationModal(int taskId)
{
    var task = await _taskRepository.GetTaskByIdAsync(taskId);
    if (task == null)
    {
        return Json(new { status = "error", message = "تسک یافت نشد" });
    }

    var model = new TaskOperationViewModel
    {
        TaskId = taskId,
        OperationOrder = (await _taskRepository.GetTaskOperationsAsync(taskId)).Count + 1
    };

    return PartialView("_AddOperationModal", model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SaveOperation(TaskOperationViewModel model)
{
    try
    {
        var operationId = await _taskRepository.CreateOperationAsync(model);

        return Json(new
        {
            status = "success",
            message = "عملیات با موفقیت اضافه شد"
        });
    }
    catch (Exception ex)
    {
        return Json(new
        {
            status = "error",
            message = "خطا در ذخیره عملیات"
        });
    }
}

[HttpPost]
public async Task<IActionResult> DeleteOperation(int id)
{
    try
    {
        await _taskRepository.DeleteOperationAsync(id);

        return Json(new
        {
            status = "success",
            message = "عملیات حذف شد"
        });
    }
    catch (Exception ex)
    {
        return Json(new
        {
            status = "error",
            message = "خطا در حذف عملیات"
        });
    }
}
```

---

## 🎯 **خلاصه**

✅ Tab یادآوری‌ها: دکمه افزودن + لیست با قابلیت حذف  
✅ Tab عملیات: دکمه افزودن + لیست با قابلیت ویرایش/حذف  
✅ Tab اعضا: دکمه افزودن + لیست با قابلیت حذف  
✅ استفاده از همان متدها و مودال‌های موجود  
✅ بارگذاری دینامیک با AJAX  

**نسخه:** 1.0.0  
**تاریخ:** دی 1403
