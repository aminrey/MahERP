# 📖 سیستم مدیریت Return URL (بازگشت به صفحه مبدا)

## 🎯 هدف
ایجاد یک سیستم یکپارچه برای مدیریت بازگشت کاربر به صفحه مبدا بعد از انجام عملیات (مانند ثبت، ویرایش، حذف و...)

---

## 📐 معماری سیستم

### 1️⃣ **ViewModel مشترک**
```csharp
ReturnValueViewModel
├── ReturnUrl: string?               // URL بازگشت
├── ReturnToSamePage: bool          // بازگشت به همان صفحه؟
├── ReturnParams: string?            // پارامترهای اضافی
├── SourcePage: string?              // نام صفحه مبدا
└── GetFullReturnUrl(): string      // ساخت URL کامل
```

### 2️⃣ **Interface برای ViewModelها**
```csharp
public interface IHasReturnValue
{
    string? ReturnUrl { get; set; }
    string? SourcePage { get; set; }
}
```

### 3️⃣ **Helper Methods**
```csharp
ReturnUrlHelper
├── GetSafeReturnUrl()              // ساخت URL امن
├── CreateReturnUrlResponse()        // ساخت JSON Response
├── AddReturnUrl()                   // افزودن به RouteValues
├── GetReturnUrlFromRequest()        // دریافت از Request
└── GetSourcePageFromRequest()       // دریافت Source Page
```

---

## 🚀 نحوه استفاده

### ✅ **مرحله 1: پیاده‌سازی Interface در ViewModel**

```csharp
public class AddToMyDayViewModel : IHasReturnValue
{
    public int TaskAssignmentId { get; set; }
    public int? TaskId { get; set; }
    public bool FromList { get; set; }
    
    // ⭐ فیلدهای Return Value
    public string? ReturnUrl { get; set; }
    public string? SourcePage { get; set; }
}
```

---

### ✅ **مرحله 2: به‌روزرسانی Controller Action (GET)**

```csharp
[HttpGet]
public async Task<IActionResult> AddToMyDayModal(
    int taskId, 
    bool fromList = false,
    string? returnUrl = null,          // ⭐ اضافه شد
    string? sourcePage = null)         // ⭐ اضافه شد
{
    var model = new AddToMyDayViewModel
    {
        TaskAssignmentId = assignment.Id,
        TaskId = taskId,
        FromList = fromList,
        ReturnUrl = returnUrl,         // ⭐ اضافه شد
        SourcePage = sourcePage         // ⭐ اضافه شد
    };

    return PartialView("_AddToMyDayModal", model);
}
```

---

### ✅ **مرحله 3: به‌روزرسانی Controller Action (POST)**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddToMyDay(AddToMyDayViewModel model)
{
    // ... انجام عملیات ...
    
    if (result.Success)
    {
        // ⭐ استفاده از Helper برای تعیین URL بازگشت
        var returnUrl = this.GetSafeReturnUrl(
            model.ReturnUrl,
            defaultAction: "Index",
            defaultController: "MyDayTask",
            defaultArea: "TaskingArea"
        );

        return Json(new
        {
            status = "redirect",
            redirectUrl = returnUrl,
            message = new[] { new { status = "success", text = result.Message } }
        });
    }
    
    return Json(new
    {
        status = "error",
        message = new[] { new { status = "error", text = result.Message } }
    });
}
```

---

### ✅ **مرحله 4: به‌روزرسانی View (مودال)**

```razor
<form asp-action="AddToMyDay" asp-controller="MyDayTask" method="post">
    @Html.AntiForgeryToken()
    
    <input type="hidden" asp-for="TaskAssignmentId" />
    <input type="hidden" asp-for="TaskId" />
    
    @* ⭐ فیلدهای Return Value *@
    <input type="hidden" asp-for="ReturnUrl" />
    <input type="hidden" asp-for="SourcePage" />
    
    <!-- فیلدهای فرم -->
    
    <button type="button" class="btn btn-primary" data-save="modal-ajax-save">
        افزودن
    </button>
</form>
```

---

### ✅ **مرحله 5: ارسال Return URL از صفحات مختلف**

#### 📄 **از TaskCard (لیست کارتی)**
```razor
<button type="button"
        class="btn btn-sm text-white add-to-myday-btn"
        data-toggle="modal-ajax"
        href="@Url.Action("AddToMyDayModal", "MyDayTask", new { 
            taskId = Model.Id, 
            fromList = true,
            returnUrl = Context.Request.Path + Context.Request.QueryString,
            sourcePage = "TaskCard"
        })">
    <i class="fa fa-calendar-alt"></i>
</button>
```

#### 📄 **از TaskRow (لیست جدولی)**
```razor
<a class="dropdown-item" 
   href="@Url.Action("AddToMyDayModal", "MyDayTask", new { 
       taskId = Model.Id, 
       fromList = true,
       returnUrl = Context.Request.Path + Context.Request.QueryString,
       sourcePage = "TaskRow"
   })"
   data-toggle="modal-ajax">
    <i class="fa fa-calendar-plus me-2"></i>افزودن به روز من
</a>
```

#### 📄 **از Details (جزئیات تسک)**
```razor
<button type="button"
        class="dropdown-item"
        data-toggle="modal-ajax"
        href="@Url.Action("AddToMyDayModal", "MyDayTask", new { 
            taskId = Model.Id,
            returnUrl = Context.Request.Path,
            sourcePage = "TaskDetails"
        })">
    <i class="fa fa-calendar-alt me-2"></i>
    افزودن به روز من
</button>
```

---

## 🎨 سناریوهای مختلف

### ✨ **سناریو 1: بازگشت به لیست تسک‌ها**
```
کاربر از: /TaskingArea/Tasks/Index
    ↓
عملیات: افزودن به روز من
    ↓
بازگشت به: /TaskingArea/Tasks/Index
```

### ✨ **سناریو 2: بازگشت به جزئیات تسک**
```
کاربر از: /TaskingArea/Tasks/Details/123
    ↓
عملیات: افزودن به روز من
    ↓
بازگشت به: /TaskingArea/Tasks/Details/123
```

### ✨ **سناریو 3: بازگشت به روز من**
```
کاربر از: /TaskingArea/MyDayTask/Index
    ↓
عملیات: افزودن تسک جدید
    ↓
بازگشت به: /TaskingArea/MyDayTask/Index
```

### ✨ **سناریو 4: بدون Return URL (پیش‌فرض)**
```
کاربر از: هر جایی
    ↓
عملیات: افزودن به روز من (بدون ReturnUrl)
    ↓
بازگشت به: /TaskingArea/MyDayTask/Index (پیش‌فرض)
```

---

## 🔒 امنیت

### ⚠️ **بررسی امنیت URL**
```csharp
private static bool IsLocalUrl(string url)
{
    if (string.IsNullOrWhiteSpace(url))
        return false;

    // باید با / یا ~/ شروع شود
    if (!url.StartsWith("/") && !url.StartsWith("~/"))
        return false;

    // نباید // داشته باشد (پیشگیری از Open Redirect)
    if (url.StartsWith("//") || url.StartsWith("/\\"))
        return false;

    return true;
}
```

### 🛡️ **نکات امنیتی**
✅ فقط URL های محلی (Local) مجاز هستند  
✅ جلوگیری از Open Redirect Vulnerability  
✅ Validation در سمت سرور  
✅ استفاده از AntiXSS  

---

## 📊 Debug و Troubleshooting

### 🔍 **لاگ در Controller**
```csharp
Console.WriteLine($"✅ AddToMyDay Success - SourcePage: {model.SourcePage}, ReturnUrl: {returnUrl}");
```

### 🔍 **لاگ در Modal (JavaScript)**
```javascript
var returnUrl = '@Model.ReturnUrl';
var sourcePage = '@Model.SourcePage';
console.log('📍 Return Value Info:', { returnUrl, sourcePage });
```

### 🔍 **نمایش اطلاعات در Modal (فقط Development)**
```razor
@if (!string.IsNullOrEmpty(Model.SourcePage))
{
    <div class="alert alert-info mb-3" style="font-size: 0.85rem;">
        <i class="fa fa-info-circle me-2"></i>
        <strong>صفحه مبدا:</strong> @Model.SourcePage
        @if (!string.IsNullOrEmpty(Model.ReturnUrl))
        {
            <br/>
            <small class="text-muted">بازگشت به: @Model.ReturnUrl</small>
        }
    </div>
}
```

---

## 🧩 مثال کامل: افزودن به سبد خرید

```csharp
// 1️⃣ ViewModel
public class AddToCartViewModel : IHasReturnValue
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? ReturnUrl { get; set; }
    public string? SourcePage { get; set; }
}

// 2️⃣ Controller GET
[HttpGet]
public async Task<IActionResult> AddToCartModal(
    int productId,
    string? returnUrl = null,
    string? sourcePage = null)
{
    var model = new AddToCartViewModel
    {
        ProductId = productId,
        ReturnUrl = returnUrl,
        SourcePage = sourcePage
    };
    
    return PartialView("_AddToCartModal", model);
}

// 3️⃣ Controller POST
[HttpPost]
public async Task<IActionResult> AddToCart(AddToCartViewModel model)
{
    // ... انجام عملیات ...
    
    var returnUrl = this.GetSafeReturnUrl(
        model.ReturnUrl,
        defaultAction: "Index",
        defaultController: "Products"
    );
    
    return Json(new
    {
        status = "redirect",
        redirectUrl = returnUrl,
        message = "محصول به سبد اضافه شد"
    });
}

// 4️⃣ View Usage
<a href="@Url.Action("AddToCartModal", "Cart", new { 
       productId = product.Id,
       returnUrl = Context.Request.Path + Context.Request.QueryString,
       sourcePage = "ProductList"
   })"
   data-toggle="modal-ajax">
    افزودن به سبد
</a>
```

---

## 📝 چک‌لیست پیاده‌سازی

برای اضافه کردن Return URL به یک عملیات جدید:

- [ ] ViewModel را از `IHasReturnValue` ارث‌بری کنید
- [ ] در Action GET، پارامترهای `returnUrl` و `sourcePage` را اضافه کنید
- [ ] در Action POST، از `GetSafeReturnUrl` برای تعیین URL بازگشت استفاده کنید
- [ ] در View، فیلدهای hidden برای `ReturnUrl` و `SourcePage` اضافه کنید
- [ ] در لینک‌ها/دکمه‌ها، `returnUrl` و `sourcePage` را ارسال کنید
- [ ] تست کنید از صفحات مختلف

---

## 🎓 نکات پیشرفته

### 🔄 **بازگشت با پارامترها**
```csharp
var returnUrl = this.GetSafeReturnUrl(
    model.ReturnUrl,
    defaultAction: "Index",
    defaultController: "Tasks"
);

// اضافه کردن پارامتر به URL
if (!string.IsNullOrEmpty(returnUrl))
{
    var separator = returnUrl.Contains("?") ? "&" : "?";
    returnUrl += $"{separator}success=true&id={newId}";
}
```

### 🎯 **بازگشت به Tab خاص**
```csharp
var returnUrl = model.ReturnUrl;
if (!string.IsNullOrEmpty(returnUrl) && !returnUrl.Contains("#"))
{
    returnUrl += "#tab-operations"; // بازگشت به تب عملیات
}
```

### 📱 **بازگشت با Toast Message**
```javascript
// در صفحه مقصد
$(document).ready(function() {
    var urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('success') === 'true') {
        toastr.success('عملیات با موفقیت انجام شد');
        
        // پاک کردن پارامتر از URL
        window.history.replaceState({}, '', window.location.pathname);
    }
});
```

---

## 📚 مثال‌های بیشتر

### ویرایش رکورد
```csharp
public async Task<IActionResult> EditRecord(EditRecordViewModel model)
{
    // ... ویرایش ...
    
    var returnUrl = this.GetSafeReturnUrl(
        model.ReturnUrl,
        defaultAction: "Details",
        defaultController: "Records",
        defaultArea: null
    ).AddQueryParam("id", model.Id);
    
    return Json(new { status = "redirect", redirectUrl = returnUrl });
}
```

### حذف رکورد
```csharp
public async Task<IActionResult> DeleteRecord(int id, string? returnUrl = null)
{
    // ... حذف ...
    
    var finalReturnUrl = this.GetSafeReturnUrl(
        returnUrl,
        defaultAction: "Index",
        defaultController: "Records"
    );
    
    return Json(new { status = "redirect", redirectUrl = finalReturnUrl });
}
```

---

**نسخه:** 1.0.0  
**تاریخ:** دی 1403  
**نویسنده:** سیستم مدیریت MahERP

---

## 🔗 لینک‌های مرتبط
- `ReturnValueViewModel.cs` - ViewModel اصلی
- `ReturnUrlHelper.cs` - متدهای کمکی
- `IHasReturnValue.cs` - Interface مشترک
