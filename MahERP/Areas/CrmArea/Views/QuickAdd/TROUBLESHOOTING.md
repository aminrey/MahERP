# 🐛 Troubleshooting Guide - QuickAdd System

## ❌ **خطاهای رایج و راه‌حل‌ها**

---

## 1️⃣ **خطا: "Uncaught ReferenceError: openQuickAddModal is not defined"**

### 🔍 **علت:**
فایل `quickadd-helper.js` لود نشده است!

### ✅ **راه‌حل:**
در بخش `@section Scripts` این خط رو اضافه کنید:

```razor
@section Scripts {
    <!-- ⭐ QuickAdd Helper Script -->
    <script src="~/js/crm/quickadd-helper.js"></script>
    
    <script>
        // Your scripts...
    </script>
}
```

### 📌 **چک کنید:**
1. باز کنید: `F12 → Network → JS`
2. ببینید آیا `quickadd-helper.js` لود شده؟
3. اگر 404 میده → مسیر فایل اشتباه هست

---

## 2️⃣ **خطا: "XML Parsing Error: no root element found"**

### 🔍 **دلایل احتمالی:**

#### A) **Permission Issue**
```
خطا: کاربر دسترسی CRM ندارد
راه‌حل: بررسی Permission در PermissionRequired("CRM")
```

**چک کنید:**
- آیا کاربر `Permission: CRM` دارد؟
- آیا `[AllowAnonymous]` برای تست اضافه شده؟

#### B) **Routing Issue**
```
خطا: URL اشتباه است
راه‌حل: بررسی Area و Controller Name
```

**URL صحیح:**
```
/CrmArea/QuickAdd/SelectTypeModal?branchId=2
/CrmArea/QuickAdd/QuickAddContactModal?branchId=2
/CrmArea/QuickAdd/QuickAddOrganizationModal?branchId=2
```

**URL اشتباه (قدیمی):**
```
/CrmArea/CRM/QuickAddContactPartial?branchId=2  ❌
```

#### C) **View Not Found**
```
خطا: View پیدا نمیشه
راه‌حل: بررسی مسیر View
```

**مسیر صحیح:**
```
MahERP/Areas/CrmArea/Views/QuickAdd/_SelectTypeModal.cshtml
MahERP/Areas/CrmArea/Views/QuickAdd/_QuickAddContactModal.cshtml
MahERP/Areas/CrmArea/Views/QuickAdd/_QuickAddOrganizationModal.cshtml
```

#### D) **Repository Null**
```
خطا: _branchRepo.GetBranchById() throws NullReferenceException
راه‌حل: بررسی Dependency Injection
```

---

## 3️⃣ **خطا: Modal باز میشه ولی خالیه (سفید)**

### 🔍 **علت:**
View error داره یا ViewBag خالیه

### ✅ **راه‌حل:**
1. باز کنید: `F12 → Network → XHR`
2. Response Preview رو ببینید
3. اگر خطا هست → خطای View رو بخونید

---

## 4️⃣ **خطا: دکمه "افزودن سریع" کار نمیکنه**

### 🔍 **علت:**
`openQuickAddModalForInteraction()` یا `openQuickAddModalForGoal()` تعریف نشده

### ✅ **راه‌حل:**
مطمئن بشید این توابع در Scripts تعریف شدن:

```javascript
// برای Interaction Create
function openQuickAddModalForInteraction() {
    var branchId = $('#selectedBranchId').val();
    var organizationId = $('#selectedOrganizationId').val() || null;
    
    if (!branchId) {
        NotificationHelper.error('لطفاً ابتدا شعبه را انتخاب کنید');
        return;
    }
    
    openQuickAddModal(branchId, organizationId);
}

// برای Goal SelectTarget
function openQuickAddModalForGoal() {
    var branchId = getSelectedBranchId();
    if (!branchId) {
        NotificationHelper.warning('لطفاً ابتدا شعبه را انتخاب کنید');
        return;
    }

    openQuickAddModal(branchId, null);
}
```

---

## 5️⃣ **خطا: بعد از Submit هیچ اتفاقی نمیفته**

### 🔍 **علت:**
Callback تعریف نشده یا اشتباه هست

### ✅ **راه‌حل:**
مطمئن بشید `window.onQuickAddComplete` override شده:

```javascript
window.onQuickAddComplete = function(type, response) {
    if (type === 'contact') {
        // Reload صفحه با Contact جدید
        window.location.href = '@Url.Action("Create", "Interaction")' + 
                              '?contactId=' + response.contactId;
    } else if (type === 'organization') {
        // Reload با Organization
        window.location.href = '@Url.Action("Create", "Interaction")' + 
                              '?organizationId=' + response.organizationId;
    }
};
```

---

## 🔧 **مراحل Debugging:**

### **مرحله 1: تست مستقیم URL**
در مرورگر بزنید:
```
https://localhost:44390/CrmArea/QuickAdd/SelectTypeModal?branchId=2
```

**نتیجه مورد انتظار:** HTML مودال برگردونده بشه

**اگر 404:** مشکل Routing
**اگر 403:** مشکل Permission
**اگر 500:** مشکل در Controller/View

---

### **مرحله 2: چک کردن Browser Console**
باز کنید: `F12 → Console`

**ببینید:**
- چه خطایی هست؟
- Status Code چیه؟ (200, 403, 404, 500)
- Response چیه؟

---

### **مرحله 3: چک کردن Network Tab**
باز کنید: `F12 → Network → XHR`

**ببینید:**
- Request URL چیه؟
- Status Code؟
- Response Preview؟

---

### **مرحله 4: اضافه کردن `[AllowAnonymous]`**
در `QuickAddController.cs`:

```csharp
[HttpGet]
[AllowAnonymous] // ⭐ موقتاً برای تست
public IActionResult SelectTypeModal(int branchId, int? organizationId = null)
{
    // ...
}
```

اگر کار کرد → مشکل از Permission

---

### **مرحله 5: چک کردن Log ها**
در Output Window:

```
Debug → Windows → Output
```

ببینید چه خطایی logged شده

---

## ✅ **راه‌حل‌های رایج:**

### **1. اضافه کردن Permission**
اگر `PermissionRequired("CRM")` مشکل ایجاد می‌کنه:

```csharp
// Option A: حذف موقت
// [PermissionRequired("CRM")]

// Option B: اضافه کردن AllowAnonymous
[AllowAnonymous]
public IActionResult SelectTypeModal(...)
```

### **2. اصلاح URL در JavaScript**
اگر URL اشتباه هست:

```javascript
// ❌ اشتباه
url: '/QuickAdd/SelectTypeModal'
url: '/CrmArea/CRM/QuickAddContactPartial'  // قدیمی!

// ✅ درست
url: '/CrmArea/QuickAdd/SelectTypeModal'
url: '/CrmArea/QuickAdd/QuickAddContactModal'
```

### **3. چک کردن ViewBag**
در View:

```razor
@{
    var branchId = ViewBag.BranchId as int? ?? 0;
    var branchName = ViewBag.BranchName as string ?? "نامشخص";
}

<!-- Debug -->
<div>BranchId: @branchId</div>
<div>BranchName: @branchName</div>
```

### **4. اضافه کردن Try-Catch**
در Controller:

```csharp
[HttpGet]
public IActionResult SelectTypeModal(int branchId, int? organizationId = null)
{
    try
    {
        ViewBag.BranchId = branchId;
        // ...
        return PartialView("_SelectTypeModal");
    }
    catch (Exception ex)
    {
        // Return error به جای exception
        return Content($"Error: {ex.Message}");
    }
}
```

---

## 🎯 **چک‌لیست نهایی:**

### **در صفحه View:**
- [ ] `quickadd-helper.js` در Scripts include شده؟
- [ ] `openQuickAddModalForInteraction()` تعریف شده؟
- [ ] `window.onQuickAddComplete` override شده؟
- [ ] BranchId به درستی گرفته میشه؟

### **در Browser:**
- [ ] Console خطایی نداره؟
- [ ] Network Tab status 200 رو نشون میده؟
- [ ] quickadd-helper.js لود شده؟

### **در Backend:**
- [ ] Permission `CRM` داده شده؟
- [ ] Controller Methods وجود دارن؟
- [ ] Views در مسیر صحیح هستن؟
- [ ] Dependency Injection درست کار می‌کنه؟

---

## 📞 **اگر مشکل حل نشد:**

1. Screenshot از Browser Console بگیرید
2. Screenshot از Network Tab بگیرید
3. Log های Output Window رو چک کنید
4. مطمئن بشید که Build موفق بوده

---

## 🚀 **بعد از حل مشکل:**

1. `[AllowAnonymous]` رو حذف کنید
2. Try-Catch های اضافی رو حذف کنید (اختیاری)
3. Debug log ها رو حذف کنید
4. Test کنید که همه چیز کار می‌کنه

---

## 🎉 **تست نهایی:**

```javascript
// در Console بزنید:
console.log(typeof openQuickAddModal);  // باید "function" برگردونه
console.log(typeof window.onQuickAddComplete);  // باید "function" برگردونه
```

اگر هر دو "function" برگردوندن → سیستم آماده است! ✅
