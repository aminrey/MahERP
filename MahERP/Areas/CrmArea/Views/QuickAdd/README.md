# 🚀 QuickAdd System Documentation

## 📋 **خلاصه**
سیستم افزودن سریع Contact و Organization برای استفاده در فرم‌های CRM (Interaction Create, Goal Create)

---

## 🏗️ **ساختار فایل‌ها**

```
MahERP/
├── Areas/CrmArea/
│   ├── Controllers/
│   │   ├── QuickAddController.cs           ← کنترلر اصلی (Modals)
│   │   └── QuickAddController.CRUD.cs      ← CRUD Actions
│   └── Views/QuickAdd/
│       ├── _SelectTypeModal.cshtml         ← انتخاب Contact/Organization
│       ├── _QuickAddContactModal.cshtml    ← فرم افزودن Contact
│       └── _QuickAddOrganizationModal.cshtml ← فرم افزودن Organization
├── wwwroot/js/crm/
│   └── quickadd-helper.js                  ← توابع JavaScript
└── MahERP.DataModelLayer/
    └── ViewModels/CRMViewModels/
        └── NewCrmViewModels.cs              ← QuickAdd ViewModels
```

---

## 📝 **ViewModels**

### 1️⃣ **QuickAddContactViewModel**
```csharp
- LastName (required)
- FirstName (optional)
- PhoneNumber (optional)
- Email (optional)
- BranchId (required)
- OrganizationId (optional) // اگر به سازمان لینک بشه
```

### 2️⃣ **QuickAddOrganizationViewModel**
```csharp
- Name (required)
- Brand (optional)
- PhoneNumber (optional)
- Email (optional)
- BranchId (required)
- OrganizationType (default: 0 = شرکت)
```

### 3️⃣ **QuickAddResponseViewModel**
```csharp
- Status (success/error)
- Message
- ContactId / ContactName
- OrganizationId / OrganizationName
- SelectValue / SelectText (برای Select2)
```

---

## 🎯 **نحوه استفاده**

### **مرحله 1: اضافه کردن Script**
```html
@section Scripts {
    <script src="~/js/crm/quickadd-helper.js"></script>
    <script>
        // Override callback
        window.onQuickAddComplete = function(type, response) {
            if (type === 'contact') {
                selectNewContact(response.contactId, response.contactName);
            } else if (type === 'organization') {
                selectNewOrganization(response.organizationId, response.organizationName);
            }
        };
    </script>
}
```

### **مرحله 2: اضافه کردن دکمه**
```html
<button type="button" 
        class="btn btn-sm btn-success" 
        onclick="openQuickAddModal(getBranchId(), getOrganizationId())">
    <i class="fa fa-plus me-1"></i>
    افزودن سریع
</button>
```

### **مرحله 3: تابع دریافت BranchId**
```javascript
function getBranchId() {
    return $('#selectedBranchId').val() || $('#branchSelector').val();
}

function getOrganizationId() {
    return $('#selectedOrganizationId').val() || null;
}
```

---

## 🔄 **Workflow**

### **حالت 1: افزودن Contact ساده**
```
User → [افزودن سریع] → [انتخاب Contact] → [فرم Contact]
     → [ذخیره] → Contact Created + Phone + BranchContact
     → [انتخاب در Select2]
```

### **حالت 2: افزودن Organization**
```
User → [افزودن سریع] → [انتخاب Organization] → [فرم Organization]
     → [ذخیره] → Organization Created + Phone + BranchOrganization
     → [انتخاب در Select2]
```

### **حالت 3: افزودن Contact در Organization انتخاب شده**
```
User → [سازمان انتخاب شده] → [افزودن سریع] → [فرم Contact]
     → [ذخیره] → Contact + Phone + BranchContact + ContactOrganization
     → [انتخاب در Select2]
```

---

## ✅ **فیلدهای الزامی**

### Contact:
- ✅ LastName (الزامی)
- ❌ FirstName (اختیاری)
- ❌ PhoneNumber (اختیاری - ولی سیستم ذخیره داره)
- ❌ Email (اختیاری)
- ✅ BranchId (الزامی)

### Organization:
- ✅ Name (الزامی)
- ❌ Brand (اختیاری)
- ❌ PhoneNumber (اختیاری - ولی سیستم ذخیره داره)
- ❌ Email (اختیاری)
- ✅ BranchId (الزامی)

---

## 🔧 **توابع JavaScript**

### 1. **openQuickAddModal(branchId, organizationId)**
باز کردن مودال انتخاب نوع

### 2. **openQuickAddContact(branchId, organizationId)**
باز کردن مودال افزودن Contact

### 3. **openQuickAddOrganization(branchId)**
باز کردن مودال افزودن Organization

### 4. **window.onQuickAddComplete(type, response)**
Callback بعد از ساخت موفق (باید Override بشه)

### 5. **selectNewContact(contactId, contactName)**
انتخاب خودکار Contact در Select2

### 6. **selectNewOrganization(orgId, orgName)**
انتخاب خودکار Organization در Select2

---

## 🎨 **مثال کامل**

```javascript
// در Interaction Create یا Goal Create:

$(document).ready(function() {
    // Override callback
    window.onQuickAddComplete = function(type, response) {
        if (type === 'contact') {
            // انتخاب در Select2
            selectNewContact(response.contactId, response.contactName);
            
            // بارگذاری اهداف (اگر نیاز باشه)
            if (typeof loadContactGoals === 'function') {
                loadContactGoals(response.contactId);
            }
        } else if (type === 'organization') {
            selectNewOrganization(response.organizationId, response.organizationName);
        }
    };
});

function getBranchId() {
    return $('#selectedBranchId').val() || $('#branchSelector').val();
}

function getOrganizationId() {
    return $('#selectedOrganizationId').val() || null;
}
```

---

## 📌 **نکات مهم**

1. ✅ **Branch الزامی است** - بدون شعبه نمی‌شه افزودن کرد
2. ✅ **Phone اختیاری است** - ولی اگر وارد بشه، اعتبارسنجی و نرمال‌سازی انجام میشه
3. ✅ **Organization Link** - اگر سازمان انتخاب شده باشه، Contact بهش لینک میشه
4. ✅ **ActivityLog** - همه اعمال لاگ میشن
5. ✅ **Select2 Integration** - مقدار جدید خودکار انتخاب میشه

---

## 🐛 **Debugging**

```javascript
// چک کردن BranchId
console.log('BranchId:', getBranchId());

// چک کردن OrganizationId
console.log('OrganizationId:', getOrganizationId());

// چک کردن Select2
console.log('Contact Select2:', $('#contactSelector').length);
console.log('Organization Select2:', $('#organizationSelector').length);
```

---

## 🚀 **آماده استفاده!**

سیستم QuickAdd آماده است و می‌تونید در **Interaction Create** و **Goal Create** ازش استفاده کنید.
