# 🎨 Custom Tooltip System - راهنمای استفاده (نسخه 2.0)

## ✨ ویژگی‌های بهبود یافته

- **انیمیشن نرم و روان**: بدون لرزش، با transition های بهینه شده
- **استایل حرفه‌ای**: Gradient های زیبا با shadow های چند لایه
- **Padding مناسب**: فضای کافی برای خوانایی بهتر
- **تم‌های رنگی متنوع**: 5 تم با رنگ‌های استاندارد
- **موقعیت هوشمند**: Auto-adjustment برای جلوگیری از خروج از صفحه
- **Performance بهینه**: استفاده از RAF برای انیمیشن‌های smooth
- **RTL Support**: پشتیبانی کامل از راست‌چین
- **Responsive**: سازگار کامل با موبایل

## 🚀 استفاده ساده

### نحوه 1: با Attribute (توصیه می‌شود)

```html
<!-- ساده -->
<button data-tooltip="راهنما">
    <i class="fa fa-info"></i>
</button>

<!-- با موقعیت -->
<img src="avatar.jpg" 
     data-tooltip="علی رضایی"
     data-tooltip-position="top"
     alt="پروفایل">

<!-- با تم رنگی -->
<button data-tooltip="حذف شد"
        data-tooltip-theme="success">
    <i class="fa fa-check"></i>
</button>
```

## 🎨 تم‌های رنگی

| تم | Attribute | رنگ | کاربرد |
|---|---|---|---|
| Dark (پیش‌فرض) | `theme-dark` | خاکستری تیره | عمومی |
| Primary | `theme-primary` | آبی | اطلاعات |
| Success | `theme-success` | سبز | موفقیت |
| Danger | `theme-danger` | قرمز | خطر/حذف |
| Warning | `theme-warning` | نارنجی | هشدار |

## 📍 موقعیت‌ها

- `top` - بالای المنت (پیش‌فرض)
- `bottom` - پایین المنت
- `left` - سمت چپ
- `right` - سمت راست

## 💡 مثال‌های کاربردی

### آواتار تیم

```html
<div class="d-flex gap-2">
    <img src="avatar1.jpg"
         class="rounded-circle"
         style="width: 32px; height: 32px;"
         data-tooltip="علی رضایی - برنامه‌نویس"
         data-tooltip-position="top"
         data-tooltip-theme="dark"
         alt="علی">
    
    <img src="avatar2.jpg"
         class="rounded-circle"
         style="width: 32px; height: 32px;"
         data-tooltip="سارا احمدی - طراح UI/UX"
         data-tooltip-position="top"
         data-tooltip-theme="dark"
         alt="سارا">
</div>
```

### دکمه‌های عملیاتی

```html
<div class="btn-group">
    <!-- نمایش -->
    <button class="btn btn-sm btn-secondary"
            data-tooltip="نمایش جزئیات"
            data-tooltip-position="top"
            data-tooltip-theme="dark">
        <i class="fa fa-eye"></i>
    </button>
    
    <!-- ویرایش -->
    <button class="btn btn-sm btn-primary"
            data-tooltip="ویرایش"
            data-tooltip-position="top"
            data-tooltip-theme="primary">
        <i class="fa fa-edit"></i>
    </button>
    
    <!-- حذف -->
    <button class="btn btn-sm btn-danger"
            data-tooltip="حذف دائمی"
            data-tooltip-position="top"
            data-tooltip-theme="danger">
        <i class="fa fa-trash"></i>
    </button>
</div>
```

### Tooltip چند خطی

```html
<button data-tooltip="خط اول راهنما&#10;خط دوم راهنما&#10;خط سوم راهنما"
        data-tooltip-position="right"
        data-tooltip-theme="dark">
    <i class="fa fa-question-circle"></i>
</button>
```

## 🔧 استفاده پیشرفته با JavaScript

```javascript
// ایجاد instance جدید
const tooltip = new CustomTooltip({
    position: 'right',
    theme: 'success',
    delay: 200,
    hideDelay: 50,
    multiline: true
});

// نمایش
const button = document.querySelector('#myButton');
tooltip.show(button, 'پیام راهنما');

// مخفی کردن
tooltip.hide();

// به‌روزرسانی متن
tooltip.updateText(button, 'متن جدید');

// حذف
tooltip.destroy();
```

## 🔄 Initialize دستی (برای محتوای AJAX)

```javascript
// بعد از load کردن محتوای Dynamic
$.ajax({
    url: '/api/data',
    success: function(html) {
        $('#container').html(html);
        
        // Initialize tooltips در container جدید
        CustomTooltip.init('#container [data-tooltip]');
    }
});

// یا برای کل صفحه
CustomTooltip.init();

// حذف همه tooltips
CustomTooltip.destroyAll();
```

## ⚙️ تنظیمات پیشرفته

```javascript
CustomTooltip.init('[data-tooltip]', {
    position: 'top',        // موقعیت پیش‌فرض
    theme: 'dark',          // تم پیش‌فرض
    delay: 150,             // تاخیر نمایش (ms)
    hideDelay: 50,          // تاخیر مخفی شدن (ms)
    offset: 10,             // فاصله از المنت (px)
    multiline: false,       // چند خطی بودن
    className: 'my-class',  // کلاس سفارشی
    container: document.body // container
});
```

## 🎯 نکات مهم

### ✅ بهترین روش‌ها

1. **تم Dark برای حالت عمومی** - خوانایی بهتر
2. **موقعیت Top برای دکمه‌ها** - راحت‌تر دیده می‌شود
3. **متن کوتاه** - حداکثر 2-3 کلمه
4. **برای توضیحات طولانی از موقعیت Right استفاده کنید**

### ⚡ Performance

- Tooltip ها فقط هنگام نیاز ساخته می‌شوند
- استفاده از `requestAnimationFrame` برای انیمیشن smooth
- Auto cleanup هنگام حذف المنت از DOM

### 📱 موبایل

- اندازه فونت و padding در موبایل کوچکتر می‌شود
- Touch events پشتیبانی نمی‌شوند (فقط hover/focus)

## 🐛 عیب‌یابی

### Tooltip نمایش داده نمی‌شود

1. فایل‌های CSS و JS را چک کنید
2. مطمئن شوید `data-tooltip` مقدار دارد
3. Console را برای خطا چک کنید

### Tooltip در محتوای AJAX کار نمی‌کند

```javascript
// بعد از load محتوا
$('#container').load('/url', function() {
    CustomTooltip.init('#container [data-tooltip]');
});
```

### انیمیشن لرزش دارد

این مشکل در نسخه 2.0 برطرف شده است با:
- استفاده از `visibility` به جای `display`
- Double `requestAnimationFrame`
- Transform های بهینه شده

## 🎨 سفارشی‌سازی استایل

```css
/* تغییر رنگ تم Dark */
.custom-tooltip.theme-dark {
    background: linear-gradient(135deg, #your-color-1, #your-color-2);
}

/* تغییر padding */
.custom-tooltip {
    padding: 12px 18px;
}

/* تغییر font */
.custom-tooltip {
    font-family: 'IRANSans', sans-serif;
    font-size: 15px;
}
```

## 📦 فایل‌های مورد نیاز

✅ `custom-tooltip.css` - استایل‌ها
✅ `custom-tooltip.js` - منطق کار

## 🌟 مثال کامل

```html
<!DOCTYPE html>
<html dir="rtl" lang="fa">
<head>
    <link href="/assets/css/custom-tooltip.css" rel="stylesheet" />
</head>
<body>
    
    <div class="d-flex gap-3 p-4">
        <button class="btn btn-primary"
                data-tooltip="ذخیره تغییرات"
                data-tooltip-theme="primary">
            <i class="fa fa-save"></i> ذخیره
        </button>
        
        <button class="btn btn-danger"
                data-tooltip="حذف دائمی رکورد"
                data-tooltip-theme="danger">
            <i class="fa fa-trash"></i> حذف
        </button>
        
        <button class="btn btn-success"
                data-tooltip="عملیات موفق بود"
                data-tooltip-theme="success">
            <i class="fa fa-check"></i> تایید
        </button>
    </div>

    <script src="/assets/js/custom-tooltip.js"></script>
</body>
</html>
```

## 🔥 تغییرات نسخه 2.0

- ✅ **انیمیشن بهبود یافته** - بدون لرزش
- ✅ **Padding مناسب** - 10px-16px
- ✅ **Shadow چند لایه** - عمق بیشتر
- ✅ **Positioning هوشمند** - boundary checking
- ✅ **Performance بهتر** - double RAF
- ✅ **کد تمیزتر** - refactored

---

**نسخه 2.0 - ساخته شده با ❤️ برای MahERP**
