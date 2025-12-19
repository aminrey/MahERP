# 🎯 Nested Modal System - راهنمای استفاده

## 📌 **مشکل: Modal داخل Modal**

وقتی می‌خواهیم داخل یک Modal باز، Modal دیگری را باز کنیم (مثلاً QuickAdd داخل Interaction Create)، با خطا مواجه می‌شویم:

```
❌ Another modal is already open
```

---

## ✅ **راه‌حل: `createAndShowNestedModal`**

تابع جدیدی در `Methods.js` اضافه شده که اجازه باز شدن Modal داخل Modal را می‌دهد.

---

## 🔧 **تفاوت‌ها با `createAndShowModal`:**

| ویژگی | createAndShowModal | createAndShowNestedModal |
|-------|-------------------|-------------------------|
| **Backdrop** | `true` (سیاه) | `false` یا `'dim'` (شفاف) |
| **z-index** | ثابت (1050) | پویا (parent + 10) |
| **بررسی Modal باز** | ✅ چک می‌کنه | ❌ چک نمی‌کنه |
| **استفاده** | Modal اصلی | Modal داخل Modal |

---

## 📝 **نحوه استفاده:**

### **مثال 1: باز کردن QuickAdd در Interaction Create**

```javascript
// ⭐ به جای createAndShowModal
createAndShowNestedModal({
    url: '/CrmArea/QuickAdd/SelectTypeModal?branchId=2',
    backdrop: 'dim', // backdrop شفاف
    onShown: function(modalInstance, $modal) {
        console.log('Modal opened');
    },
    onHidden: function() {
        console.log('Modal closed');
    }
});
```

---

### **مثال 2: استفاده در quickadd-helper.js**

```javascript
function openQuickAddModal(branchId, organizationId = null) {
    createAndShowNestedModal({
        url: `/CrmArea/QuickAdd/SelectTypeModal?branchId=${branchId}`,
        backdrop: 'dim',
        onHidden: function() {
            console.log('QuickAdd modal closed');
        }
    }).catch(err => {
        console.error('Error:', err);
        Swal.fire('خطا', err.message, 'error');
    });
}
```

---

## 🎨 **گزینه‌های Backdrop:**

### **1. بدون backdrop:**
```javascript
createAndShowNestedModal({
    url: '...',
    backdrop: false // هیچ backdrop نداره
});
```

### **2. Backdrop شفاف (پیشنهادی):**
```javascript
createAndShowNestedModal({
    url: '...',
    backdrop: 'dim' // backdrop شفاف (30% opacity)
});
```

### **3. Backdrop کامل:**
```javascript
createAndShowNestedModal({
    url: '...',
    backdrop: true // backdrop سیاه (مثل modal عادی)
});
```

---

## ⚙️ **تنظیمات پیش‌فرض:**

```javascript
{
    url: null,              // URL برای load کردن محتوا
    modalId: 'nested-modal-...', // ID یکتا
    backdrop: false,        // بدون backdrop
    keyboard: true,         // ESC برای بستن
    removeOnHide: true,     // حذف از DOM بعد از بسته شدن
    onShown: null,          // Callback بعد از باز شدن
    onHidden: null,         // Callback بعد از بسته شدن
    onSubmitSuccess: null,  // Callback بعد از submit موفق
    onLoadError: null       // Callback در صورت خطا
}
```

---

## 🔍 **z-index Management:**

سیستم به صورت خودکار z-index را مدیریت می‌کند:

```
Parent Modal: z-index = 1050
└── Nested Backdrop (optional): z-index = 1059
    └── Nested Modal: z-index = 1060
```

---

## 📦 **Dependencies:**

تابع نیاز به این توابع دارد:

- ✅ `setupModalFormHandler()` - مدیریت فرم در Modal
- ✅ `ModalUtils.processUrlsInContainer()` - پردازش URL ها
- ✅ `DynamicSelect2Manager.reinitializeSelect2InDiv()` - راه‌اندازی Select2
- ✅ `getAjaxErrorMessage()` - دریافت پیام خطا

---

## 🐛 **Troubleshooting:**

### **خطا: "createAndShowNestedModal is not defined"**

**راه‌حل:**
مطمئن شوید `Methods.js` لود شده:

```html
<script src="~/assets/js/Methods.js"></script>
```

---

### **خطا: Modal پشت Parent Modal قرار می‌گیرد**

**راه‌حل:**
z-index به صورت خودکار تنظیم می‌شود. اگر مشکل دارید:

```javascript
$modal.on('shown.bs.modal', function () {
    $modal.css('z-index', 9999); // Force z-index
});
```

---

### **خطا: Backdrop سیاه شده**

**راه‌حل:**
مطمئن شوید `backdrop: 'dim'` یا `backdrop: false` استفاده کردید:

```javascript
createAndShowNestedModal({
    url: '...',
    backdrop: 'dim' // ← اینجا
});
```

---

## 🎯 **مثال کامل: QuickAdd در Interaction Create**

### **1. در View (Interaction/Create.cshtml):**

```html
@section Scripts {
    <script src="~/assets/js/Methods.js"></script>
    <script src="~/js/crm/quickadd-helper.js"></script>
    
    <script>
        // Override callback
        window.onQuickAddComplete = function(type, response) {
            if (type === 'contact') {
                // Reload با Contact جدید
                window.location.href = '@Url.Action("Create")' + 
                                      '?contactId=' + response.contactId;
            }
        };
        
        // تابع باز کردن QuickAdd
        function openQuickAddModalForInteraction() {
            var branchId = $('#selectedBranchId').val();
            var organizationId = $('#selectedOrganizationId').val() || null;
            
            if (!branchId) {
                alert('لطفاً ابتدا شعبه را انتخاب کنید');
                return;
            }
            
            openQuickAddModal(branchId, organizationId);
        }
    </script>
}
```

### **2. در HTML:**

```html
<button type="button" 
        class="btn btn-success" 
        onclick="openQuickAddModalForInteraction()">
    <i class="fa fa-plus me-1"></i>
    افزودن سریع
</button>
```

---

## 📊 **Performance:**

- ✅ Modal از DOM حذف می‌شود (`removeOnHide: true`)
- ✅ Event Listeners پاکسازی می‌شوند
- ✅ Select2 و Datepicker راه‌اندازی می‌شوند
- ✅ Form Handler به صورت خودکار attach می‌شود

---

## 🚀 **بهترین روش‌ها:**

1. ✅ **همیشه** `backdrop: 'dim'` استفاده کنید برای Nested Modal
2. ✅ **همیشه** `removeOnHide: true` فعال باشد
3. ✅ **Error handling** با `.catch()` اضافه کنید
4. ✅ **Callback** ها را override کنید در صفحه اصلی
5. ✅ **Console log** فعال نگه دارید برای debugging

---

## 🎉 **خلاصه:**

```javascript
// ✅ برای Modal عادی
createAndShowModal({ url: '...' });

// ✅ برای Modal داخل Modal
createAndShowNestedModal({ 
    url: '...', 
    backdrop: 'dim' 
});
```

**حالا می‌تونید تا 3-4 لایه Modal داخل هم باز کنید!** 🚀
