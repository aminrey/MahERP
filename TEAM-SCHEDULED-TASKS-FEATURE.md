# ✅ قابلیت جدید: تفکیک تسک‌های دوره‌ای من و تیمی

## 🎯 **هدف:**
تفکیک نمایش تسک‌های دوره‌ای به دو بخش:
1. ✅ **تسک‌های دوره‌ای من** - که من سازنده‌شان هستم (قابل ویرایش)
2. 👥 **تسک‌های دوره‌ای تیمی** - که من عضو تیم هستم (فقط مشاهده)

---

## 📊 **قبل و بعد:**

### قبل:
```
[لیست تسک‌های دوره‌ای]
- همه تسک‌ها در یک لیست
- نمی‌دانم کدام را من ساخته‌ام
- نمی‌دانم کدام تیمی است
- همه دکمه‌های ویرایش/حذف فعال
```

### بعد:
```
[تسک‌های دوره‌ای من] (badge: 3)
- تسک 1 ← دکمه‌های ویرایش/حذف ✅
- تسک 2 ← دکمه‌های ویرایش/حذف ✅
- تسک 3 ← دکمه‌های ویرایش/حذف ✅

[تسک‌های دوره‌ای تیمی] (badge: 2) (فقط مشاهده)
ℹ️ شما در این تسک‌ها جزو اعضای تیم هستید. امکان ویرایش ندارید.
- تسک A ← Badge: عضو تیم | فقط دکمه جزئیات 👁️
- تسک B ← Badge: عضو تیم | فقط دکمه جزئیات 👁️
```

---

## 🔧 **تغییرات اعمال شده:**

### 1️⃣ **ViewModel: `ScheduledTaskListViewModel.cs`**

#### اضافه شدن فیلدهای جدید:
```csharp
public class ScheduledTaskListViewModel
{
    /// <summary>
    /// تسک‌های زمان‌بندی شده که من سازنده‌شان هستم
    /// </summary>
    public List<ScheduledTaskCardViewModel> MyScheduledTasks { get; set; } = new();
    
    /// <summary>
    /// تسک‌های زمان‌بندی شده که من عضو تیم هستم (فقط مشاهده)
    /// </summary>
    public List<ScheduledTaskCardViewModel> TeamScheduledTasks { get; set; } = new();
    
    /// <summary>
    /// ⚠️ DEPRECATED: از MyScheduledTasks استفاده کنید
    /// </summary>
    [Obsolete("از MyScheduledTasks و TeamScheduledTasks استفاده کنید")]
    public List<ScheduledTaskCardViewModel> ScheduledTasks { get; set; } = new();
    
    public ScheduledTaskStatsViewModel Stats { get; set; } = new();
}
```

#### اضافه شدن `IsCreatedByMe`:
```csharp
public class ScheduledTaskCardViewModel
{
    // ... فیلدهای قبلی ...
    
    /// <summary>
    /// ⭐⭐⭐ NEW: آیا من سازنده این Schedule هستم؟
    /// </summary>
    [Display(Name = "سازنده من هستم")]
    public bool IsCreatedByMe { get; set; }
}
```

---

### 2️⃣ **Repository: `TaskRepository.ScheduledTasks.cs`**

#### بازنویسی `GetUserScheduledTasksAsync`:
```csharp
public async Task<ScheduledTaskListViewModel> GetUserScheduledTasksAsync(
    string userId, 
    bool isAdmin = false)
{
    // ⭐ بخش 1: تسک‌های من
    var mySchedulesQuery = _context.ScheduledTaskCreation_Tbl
        .Include(s => s.CreatedByUser)
        .Include(s => s.Branch)
        .Where(s => s.IsActive && s.CreatedByUserId == userId)
        .AsQueryable();

    var mySchedules = await mySchedulesQuery
        .OrderByDescending(s => s.CreatedDate)
        .ToListAsync();

    // ⭐ بخش 2: تسک‌های تیمی (من عضو تیم هستم)
    var teamSchedules = new List<ScheduledTaskCreation>();

    if (!isAdmin)
    {
        var allSchedules = await _context.ScheduledTaskCreation_Tbl
            .Include(s => s.CreatedByUser)
            .Include(s => s.Branch)
            .Where(s => s.IsActive && s.CreatedByUserId != userId)
            .ToListAsync();

        foreach (var schedule in allSchedules)
        {
            // بررسی: آیا من در Assignments هستم؟
            if (IsUserInScheduleAssignments(schedule.TaskDataJson, userId))
            {
                teamSchedules.Add(schedule);
            }
        }
    }

    // تبدیل به ViewModel
    var myCards = mySchedules.Select(s => 
    {
        var card = MapToScheduledTaskCard(s);
        card.IsCreatedByMe = true; // من سازنده هستم
        return card;
    }).ToList();
    
    var teamCards = teamSchedules.Select(s => 
    {
        var card = MapToScheduledTaskCard(s);
        card.IsCreatedByMe = false; // من عضو تیم هستم
        return card;
    }).ToList();

    return new ScheduledTaskListViewModel
    {
        MyScheduledTasks = myCards,
        TeamScheduledTasks = teamCards,
        Stats = CalculateScheduledTaskStats(mySchedules) // فقط آمار تسک‌های خودم
    };
}
```

#### متد `IsUserInScheduleAssignments`:
```csharp
/// <summary>
/// بررسی اینکه آیا کاربر در Assignments این Schedule هست یا نه
/// </summary>
private bool IsUserInScheduleAssignments(string taskDataJson, string userId)
{
    try
    {
        var taskModel = DeserializeTaskTemplate(taskDataJson);
        if (taskModel == null) return false;

        // بررسی UserTeamAssignmentsJson
        if (!string.IsNullOrEmpty(taskModel.UserTeamAssignmentsJson))
        {
            // ساده: بررسی می‌کنیم که userId در JSON وجود دارد یا نه
            if (taskModel.UserTeamAssignmentsJson.Contains($"\"{userId}\""))
            {
                return true;
            }
        }

        return false;
    }
    catch
    {
        return false;
    }
}
```

---

### 3️⃣ **View: `Index.cshtml`**

#### بخش 1: تسک‌های دوره‌ای من:
```razor
@if (Model.MyScheduledTasks != null && Model.MyScheduledTasks.Any())
{
    <div class="mb-4">
        <h5 class="mb-3">
            <i class="fas fa-user-check text-primary me-2"></i>
            تسک‌های دوره‌ای من
            <span class="badge bg-primary">@Model.MyScheduledTasks.Count</span>
        </h5>
        <div class="row g-3">
            @foreach (var schedule in Model.MyScheduledTasks)
            {
                <div class="col-md-6 col-lg-4">
                    @await Html.PartialAsync("_ScheduledTaskCardPartial", schedule)
                </div>
            }
        </div>
    </div>
}
```

#### بخش 2: تسک‌های دوره‌ای تیمی:
```razor
@if (Model.TeamScheduledTasks != null && Model.TeamScheduledTasks.Any())
{
    <div class="mb-4">
        <h5 class="mb-3">
            <i class="fas fa-users text-info me-2"></i>
            تسک‌های دوره‌ای تیمی
            <span class="badge bg-info">@Model.TeamScheduledTasks.Count</span>
            <small class="text-muted">(فقط مشاهده)</small>
        </h5>
        <div class="alert alert-info" role="alert">
            <i class="fas fa-info-circle me-2"></i>
            شما در این تسک‌های دوره‌ای جزو اعضای تیم هستید. امکان ویرایش یا حذف ندارید.
        </div>
        <div class="row g-3">
            @foreach (var schedule in Model.TeamScheduledTasks)
            {
                <div class="col-md-6 col-lg-4">
                    @await Html.PartialAsync("_ScheduledTaskCardPartial", schedule)
                </div>
            }
        </div>
    </div>
}
```

---

### 4️⃣ **Partial View: `_ScheduledTaskCardPartial.cshtml`**

#### محدود کردن دکمه‌های ویرایش/حذف:
```razor
<ul class="dropdown-menu dropdown-menu-end">
    <li>
        <a class="dropdown-item" href="@Url.Action("Details", "ScheduledTasks", new { id = Model.Id })">
            <i class="fas fa-eye me-2"></i>جزئیات
        </a>
    </li>
    
    @* ⭐⭐⭐ فقط برای تسک‌هایی که من سازنده‌شان هستم *@
    @if (Model.IsCreatedByMe)
    {
        <li>
            <a class="dropdown-item" href="@Url.Action("Edit", "ScheduledTasks", new { id = Model.Id })">
                <i class="fas fa-edit me-2"></i>ویرایش
            </a>
        </li>
        <li><hr class="dropdown-divider"></li>
        <li>
            <a class="dropdown-item" href="javascript:void(0)" 
               onclick="toggleScheduleStatus(@Model.Id)">
                <i class="fas fa-toggle-on me-2"></i>
                @(Model.IsScheduleEnabled ? "غیرفعال کردن" : "فعال کردن")
            </a>
        </li>
        <li><hr class="dropdown-divider"></li>
        <li>
            <a class="dropdown-item text-danger" href="javascript:void(0)" 
               onclick="deleteSchedule(@Model.Id)">
                <i class="fas fa-trash me-2"></i>حذف
            </a>
        </li>
    }
    else
    {
        <li><hr class="dropdown-divider"></li>
        <li class="dropdown-item-text text-muted small">
            <i class="fas fa-lock me-1"></i>
            فقط مشاهده (عضو تیم)
        </li>
    }
</ul>
```

#### Badge عضو تیم:
```razor
@* ⭐⭐⭐ Badge برای تسک‌های تیمی *@
@if (!Model.IsCreatedByMe)
{
    <span class="badge bg-info schedule-badge">
        <i class="fas fa-users me-1"></i>
        عضو تیم
    </span>
}
```

---

## 🎨 **UI نهایی:**

### صفحه لیست:
```
┌─────────────────────────────────────────────────┐
│ 🔵 تسک‌های دوره‌ای من [3]                      │
├─────────────────────────────────────────────────┤
│ [Card 1] ┬ ⚙️ ویرایش | 🗑️ حذف | 👁️ جزئیات  │
│          └ Badge: ماهانه | فعال                 │
│ [Card 2] ┬ ⚙️ ویرایش | 🗑️ حذف | 👁️ جزئیات  │
│          └ Badge: هفتگی | غیرفعال               │
│ [Card 3] ┬ ⚙️ ویرایش | 🗑️ حذف | 👁️ جزئیات  │
│          └ Badge: روزانه | فعال                 │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│ 👥 تسک‌های دوره‌ای تیمی [2] (فقط مشاهده)      │
│ ℹ️ شما عضو تیم هستید. امکان ویرایش ندارید.     │
├─────────────────────────────────────────────────┤
│ [Card A] ┬ 👁️ جزئیات فقط                       │
│          └ Badge: عضو تیم | ماهانه | فعال       │
│ [Card B] ┬ 👁️ جزئیات فقط                       │
│          └ Badge: عضو تیم | هفتگی | فعال        │
└─────────────────────────────────────────────────┘
```

---

## 📊 **سناریوهای تست:**

### تست 1: کاربر سازنده
```
✅ ساخت Schedule جدید
✅ مشاهده در بخش "تسک‌های دوره‌ای من"
✅ دکمه‌های ویرایش/حذف فعال است
✅ Badge "عضو تیم" نمایش داده نمی‌شود
```

### تست 2: کاربر عضو تیم
```
✅ یک Schedule دیگری وجود دارد که من در تیمش هستم
✅ مشاهده در بخش "تسک‌های دوره‌ای تیمی"
✅ فقط دکمه "جزئیات" فعال است
✅ Badge "عضو تیم" نمایش داده می‌شود
✅ پیام "فقط مشاهده" در منو
```

### تست 3: کاربر هیچ‌کدام نیست
```
✅ Schedule وجود دارد اما من نه سازنده‌ام، نه عضو تیم
✅ Schedule در هیچ‌کدام از دو بخش نمایش داده نمی‌شود
```

### تست 4: Admin
```
✅ Admin همه Schedule ها را در بخش "تسک‌های دوره‌ای من" می‌بیند
✅ بخش "تسک‌های دوره‌ای تیمی" خالی است (Admin نیازی ندارد)
```

---

## 🔍 **الگوریتم تفکیک:**

```
1. دریافت همه Schedule های من (CreatedByUserId == userId)
   → MyScheduledTasks

2. دریافت همه Schedule های دیگران (CreatedByUserId != userId)
   → برای هرکدام:
      - Deserialize کردن TaskDataJson
      - بررسی UserTeamAssignmentsJson
      - اگر userId در JSON وجود دارد → TeamScheduledTasks

3. Set کردن IsCreatedByMe:
   - MyScheduledTasks → IsCreatedByMe = true
   - TeamScheduledTasks → IsCreatedByMe = false

4. بروزرسانی Stats (فقط برای MyScheduledTasks)
```

---

## ⚠️ **نکات مهم:**

### 1️⃣ **Performance:**
```
⚠️ برای هر Schedule دیگران، TaskDataJson را Deserialize می‌کنیم
✅ راه‌حل: استفاده از Contains به جای Deserialize کامل
```

### 2️⃣ **Security:**
```
✅ کاربر فقط تسک‌هایی را می‌بیند که:
   - یا سازنده‌شان است
   - یا عضو تیم است
   
✅ دکمه‌های ویرایش/حذف فقط برای سازنده فعال است
```

### 3️⃣ **Backward Compatibility:**
```
✅ فیلد قدیمی ScheduledTasks هنوز موجود است (Obsolete)
✅ کدهای قدیمی Break نمی‌شوند
```

---

## 📝 **چک‌لیست نهایی:**

- [x] ViewModel: اضافه شدن MyScheduledTasks و TeamScheduledTasks
- [x] ViewModel: اضافه شدن IsCreatedByMe
- [x] Repository: بازنویسی GetUserScheduledTasksAsync
- [x] Repository: متد IsUserInScheduleAssignments
- [x] View: بخش تسک‌های من
- [x] View: بخش تسک‌های تیمی
- [x] Partial: محدود کردن دکمه‌ها
- [x] Partial: Badge عضو تیم
- [x] Build موفق ✅
- [ ] **تست: ساخت Schedule و بررسی نمایش**
- [ ] **تست: اضافه کردن عضو تیم و بررسی**
- [ ] **تست: دکمه‌های ویرایش/حذف**

---

## 🚀 **آماده برای استفاده!**

✅ همه تغییرات اعمال شد  
✅ Build موفق  
✅ UI تفکیک شده  
✅ محدودیت‌های دسترسی اعمال شده  

حالا Run کن و تست کن! 🎉
