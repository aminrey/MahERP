# ⚡ QuickAdd - Quick Fix Guide

## 🚨 **خطای "openQuickAddModal is not defined"**

### ✅ **راه‌حل سریع:**

1. **باز کنید:** `Interaction/Create.cshtml` یا `Goal/SelectTarget.cshtml`

2. **پیدا کنید:** بخش `@section Scripts`

3. **اضافه کنید:** این خط رو **قبل از** `<script>`:

```razor
@section Scripts {
    <!-- ⭐ این خط رو اضافه کنید -->
    <script src="~/js/crm/quickadd-helper.js"></script>
    
    <script>
        // بقیه scripts...
    </script>
}
```

4. **Save** و **Refresh** صفحه

---

## 🚨 **خطای "XML Parsing Error"**

### ✅ **راه‌حل سریع:**

#### **مرحله 1: چک کنید URL**
باز کنید: `F12 → Network → XHR`

اگر URL اینطوری هست:
```
❌ /CrmArea/CRM/QuickAddContactPartial
```

**مشکل از فایل قدیمی است!**

#### **مرحله 2: جستجو کنید**
در VS Code جستجو کنید: `QuickAddContactPartial`

#### **مرحله 3: اصلاح کنید**
تغییر بدید به:
```javascript
url: `/CrmArea/QuickAdd/QuickAddContactModal?branchId=${branchId}`
```

---

## 🚨 **دکمه کار نمیکنه**

### ✅ **راه‌حل سریع:**

1. **چک کنید:** Console خطا داره؟ `F12 → Console`

2. **اگر خطای Permission داره:**
   - در `QuickAddController.cs` اضافه کنید:
   ```csharp
   [AllowAnonymous] // موقتاً
   public IActionResult SelectTypeModal(...)
   ```

3. **اگر BranchId خالیه:**
   - چک کنید: `$('#selectedBranchId').val()`
   - باید مقدار داشته باشه

---

## 🚨 **Modal سفید (خالی)**

### ✅ **راه‌حل سریع:**

1. **باز کنید:** `F12 → Network → Response Preview`

2. **اگر خطا داره:**
   - ViewBag.BranchName خالیه
   - Repository null هست
   
3. **اضافه کنید:** Try-Catch:
   ```csharp
   try {
       ViewBag.BranchName = _branchRepo.GetBranchById(branchId)?.Name ?? "نامشخص";
       return PartialView("_QuickAddContactModal", model);
   } catch (Exception ex) {
       return Content($"Error: {ex.Message}");
   }
   ```

---

## 📋 **چک‌لیست 30 ثانیه‌ای:**

```bash
# 1. Script لود شده؟
F12 → Network → search: quickadd-helper.js → ✅ Status: 200

# 2. Function تعریف شده؟
Console → typeof openQuickAddModal → "function" ✅

# 3. BranchId داریم؟
Console → $('#selectedBranchId').val() → باید عددی برگردونه ✅

# 4. Permission داریم؟
اگر 403 میده → [AllowAnonymous] اضافه کن

# 5. URL درست هست؟
Network → XHR → باید شامل QuickAdd باشه (نه CRM)
```

---

## 🎯 **تست سریع:**

در Console بزنید:
```javascript
openQuickAddModal(2, null);
```

- اگر Modal باز شد → ✅ کار می‌کنه
- اگر خطا داد → ❌ مراجعه به TROUBLESHOOTING.md

---

## 📞 **هنوز کار نمیکنه؟**

1. مطمئن بشید Build موفق بوده
2. Refresh سخت بزنید: `Ctrl+Shift+R`
3. Cache رو پاک کنید
4. مستندات کامل رو ببینید: `TROUBLESHOOTING.md`
