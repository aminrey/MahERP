# استفاده از مودال ایجاد هدف سریع

## نحوه استفاده در هر صفحه

### 1️⃣ **استفاده ساده (بدون ReturnUrl)**

```javascript
// در صفحه Interaction/Create
function openQuickGoalModal() {
    var contactId = $('#selectedContactId').val();
    var organizationId = $('#selectedOrganizationId').val();
    
    createAndShowModal({
        url: '@Url.Action("CreateQuickModal", "Goal", new { area = "CrmArea" })' + 
             '?contactId=' + contactId + 
             '&organizationId=' + organizationId
    });
}
```

### 2️⃣ **استفاده با ReturnUrl (برای استفاده مجدد)**

```javascript
// مثال: در صفحه Dashboard
function addNewGoal() {
    var currentUrl = window.location.href;
    
    createAndShowModal({
        url: '@Url.Action("CreateQuickModal", "Goal", new { area = "CrmArea" })' + 
             '?contactId=' + contactId + 
             '&returnUrl=' + encodeURIComponent(currentUrl)
    });
}

// Callback بعد از ثبت موفق
window.onGoalCreated = function(goalId, returnUrl) {
    console.log('هدف جدید ایجاد شد: ' + goalId);
    
    // اقدامات دلخواه...
    // مثلاً بروزرسانی widget در Dashboard
    refreshGoalsWidget();
};
```

### 3️⃣ **استفاده در فرم تعامل (کاربرد فعلی)**

```javascript
function openQuickGoalModal() {
    var contactId = $('#selectedContactId').val() || @Model.ContactId;
    var organizationId = $('#selectedOrganizationId').val() || @(Model.OrganizationId ?? 0);
    
    createAndShowModal({
        url: '@Url.Action("CreateQuickModal", "Goal", new { area = "CrmArea" })' + 
             '?contactId=' + contactId + 
             '&organizationId=' + organizationId,
        onHidden: function() {
            // بعد از بستن مودال
        }
    });
}
```

## ویژگی‌ها

✅ **جداکننده 3 رقمی**: قیمت‌ها به صورت خودکار فرمت می‌شن (1,000,000)  
✅ **واحد پولی تومان**: همه مبالغ به تومان هستن  
✅ **ReturnUrl**: می‌تونه در جاهای مختلف استفاده بشه  
✅ **بروزرسانی خودکار**: لیست اهداف بدون reload بروز میشه  
✅ **انتخاب خودکار**: هدف جدید خودکار انتخاب میشه  

## پارامترها

| پارامتر | نوع | الزامی | توضیح |
|---------|-----|--------|-------|
| `contactId` | int? | خیر | شناسه فرد |
| `organizationId` | int? | خیر | شناسه سازمان |
| `returnUrl` | string? | خیر | آدرس بازگشت بعد از ثبت |

**نکته**: حداقل یکی از `contactId` یا `organizationId` باید مقدار داشته باشه.

## Response

```json
{
    "status": "success",
    "message": [
        {
            "status": "success",
            "text": "هدف با موفقیت ایجاد شد"
        }
    ],
    "goalId": 123,
    "goalsHtml": "<div>...</div>",
    "returnUrl": "/dashboard"
}
```
