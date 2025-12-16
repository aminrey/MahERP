# Task Detail Partials - ساختار Partial Viewها

این فولدر شامل تمام partial viewهای مربوط به صفحه جزئیات تسک (`Details.cshtml`) است.

## 📁 ساختار فایل‌ها

### 1. **Navigation & Structure**
- **`_TaskDetailTabs.cshtml`**: تب‌های ناوبری بالای صفحه (Overview, WorkLogs, Operations, etc.)

### 2. **Tab Content Partials**

#### اطلاعات و گزارش‌ها
- **`_WorkLogsTab.cshtml`**: تب گزارش‌های کاری (Work Logs)
- **`_OperationsTab.cshtml`**: تب عملیات‌ها (Operations/Checklist)

#### ارتباطات و اعضا
- **`_TeamTab.cshtml`**: تب اعضای تسک
- **`_ChatTab.cshtml`**: تب گفتگوها و کامنت‌ها
- **`_ViewersTab.cshtml`**: تب ناظرین (سیستمی و رونوشت)

#### تنظیمات و پیوست‌ها
- **`_AttachmentsTab.cshtml`**: تب فایل‌های پیوست
- **`_RemindersTab.cshtml`**: تب یادآوری‌ها
- **`_TimelineTab.cshtml`**: تب تاریخچه تسک
- **`_SettingsTab.cshtml`**: تب تنظیمات تسک

### 3. **Scripts**
- **`_TaskDetailScripts.cshtml`**: تمام JavaScript Config و توابع مربوط به تب‌ها

## 🔧 نحوه استفاده

### در فایل اصلی `Details.cshtml`:

```razor
@model TaskViewModel

<!-- Hero Section -->
@await Html.PartialAsync("_TaskHeroSection", Model)

<!-- Tab Navigation -->
@await Html.PartialAsync("TaskDetailPartials/_TaskDetailTabs", Model)

<!-- Tab Contents -->
@await Html.PartialAsync("TaskDetailPartials/_WorkLogsTab", Model)
@await Html.PartialAsync("TaskDetailPartials/_OperationsTab", Model)
// ... سایر تب‌ها

@section Scripts {
    @await Html.PartialAsync("TaskDetailPartials/_TaskDetailScripts", Model)
}
```

## 📊 متغیرهای ViewBag مورد نیاز

تمام partial viewها از متغیرهای زیر در `ViewBag` استفاده می‌کنند:

```csharp
ViewBag.CurrentUserId = currentUserId;
ViewBag.IsAssignedToCurrentUser = isAssignedToCurrentUser;
ViewBag.IsCreator = isCreator;
ViewBag.IsManager = isManager;
ViewBag.IsSupervisor = isSupervisor;
ViewBag.IsCompletedByMe = isCompletedByMe;
ViewBag.CanAddMembers = canAddMembers;
ViewBag.CanRemoveMembers = canRemoveMembers;
```

این متغیرها در فایل اصلی `Details.cshtml` تنظیم می‌شوند.

## ✅ مزایای این ساختار

1. **مدولار**: هر تب در یک فایل جداگانه
2. **قابل نگهداری**: تغییرات راحت‌تر و هدفمندتر
3. **قابل استفاده مجدد**: امکان استفاده در صفحات دیگر
4. **سازماندهی بهتر**: کد تمیزتر و خواناتر
5. **دیباگ آسان‌تر**: یافتن و رفع مشکلات سریع‌تر

## 🔄 Workflow توسعه

1. برای تغییر در یک تب، فقط فایل partial مربوطه را ویرایش کنید
2. برای افزودن تب جدید:
   - یک partial جدید در این فولدر بسازید
   - آن را به `_TaskDetailTabs.cshtml` اضافه کنید
   - آن را در `Details.cshtml` include کنید

## 📝 نکات مهم

- همه partial viewها `@model TaskViewModel` دارند
- همه از متغیرهای `ViewBag` برای دسترسی‌ها استفاده می‌کنند
- JavaScript Config در `_TaskDetailScripts.cshtml` متمرکز شده است
- AJAX callها از طریق `window.TaskDetailConfig.urls` انجام می‌شوند

## 🎯 مثال: افزودن تب جدید

```razor
@* 1. ایجاد فایل TaskDetailPartials/_NewTab.cshtml *@
@model TaskViewModel
@{
    var isCreator = ViewBag.IsCreator ?? false;
}

<div class="tab-pane fade" id="tab-new" role="tabpanel">
    <div class="block block-rounded block-fx-shadow">
        <!-- محتوای تب -->
    </div>
</div>

@* 2. افزودن به _TaskDetailTabs.cshtml *@
<li class="nav-item">
    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-new" type="button" role="tab">
        <i class="fa fa-star d-block mb-1"></i>
        <span class="tab-title">تب جدید</span>
    </button>
</li>

@* 3. Include در Details.cshtml *@
@await Html.PartialAsync("TaskDetailPartials/_NewTab", Model)
```

## 📞 پشتیبانی

برای سؤالات یا پیشنهادات، با تیم توسعه تماس بگیرید.

---
**تاریخ ایجاد**: 2024  
**نسخه**: 1.0  
**توسعه‌دهنده**: MahERP Team
