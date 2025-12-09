# 📝 Custom Tooltip System - تاریخچه تغییرات

## نسخه 2.3 - اصلاح Positioning (تاریخ فعلی)

### 🐛 رفع باگ‌ها:
- ✅ **اصلاح positioning با `position: fixed`**
  - حذف `scrollTop` و `scrollLeft` از محاسبات
  - استفاده مستقیم از `getBoundingClientRect()` 
  - tooltip حالا دقیقاً در محل درست ظاهر می‌شود

### 🎯 بهبودها:
- بهبود boundary checking
- Auto-flip هوشمندتر (از top به bottom و بالعکس)
- Performance بهتر با محاسبات ساده‌تر

---

## نسخه 2.2 - اصلاح Name Conflict (قبلی)

### 🐛 رفع باگ‌ها:
- ✅ **رفع خطای `this.position is not a function`**
  - تغییر نام property از `position` به `tooltipPosition`
  - تغییر نام method از `position()` به `positionTooltip()`
  - حل تداخل اسمی بین property و method

### 📝 تغییرات کد:
```javascript
// ❌ قبلی - خطا
this.position = 'top';
this.position(); // ERROR!

// ✅ حالا - درست
this.tooltipPosition = 'top';
this.positionTooltip(); // OK!
```

---

## نسخه 2.1 - ساده‌سازی و Cache Busting (قبلی)

### ✨ ویژگی‌های جدید:
- اضافه کردن version query string برای cache busting
- لود خودکار بهبود یافته

### 🔧 تغییرات:
- ساده‌سازی کد
- حذف کدهای غیرضروری
- بهبود error handling

---

## نسخه 2.0 - بازنویسی کامل (اولیه)

### ✨ ویژگی‌های اصلی:
- انیمیشن smooth و روان
- 5 تم رنگی (dark, primary, success, danger, warning)
- 4 موقعیت (top, bottom, left, right)
- Padding و spacing مناسب
- RTL Support کامل
- Responsive برای موبایل
- Auto-initialize
- WeakMap برای مدیریت instances

### 🎨 استایل:
- Gradient backgrounds زیبا
- Multi-layer shadows
- Arrow با drop-shadow
- Smooth transitions
- Glow effect

### 📱 Responsive:
- Font size و padding کوچکتر در موبایل
- Arrow های کوچکتر
- Max-width محدودتر

---

## 📦 فایل‌ها

- `custom-tooltip.css` - استایل‌ها
- `custom-tooltip.js` - منطق اصلی
- `CUSTOM-TOOLTIP-GUIDE.md` - راهنمای جامع
- `tooltip-demo.html` - صفحه demo کامل
- `tooltip-test-simple.html` - صفحه تست ساده

---

## 🚀 نحوه استفاده

### ساده:
```html
<button data-tooltip="راهنما">دکمه</button>
```

### پیشرفته:
```html
<button data-tooltip="ذخیره شد" 
        data-tooltip-theme="success"
        data-tooltip-position="top">
    ذخیره
</button>
```

### JavaScript:
```javascript
// Initialize
CustomTooltip.init('[data-tooltip]');

// Destroy all
CustomTooltip.destroyAll();
```

---

## 🐛 مشکلات شناخته شده

### حل شده:
- ✅ Position conflict - نسخه 2.2
- ✅ Positioning با scroll - نسخه 2.3
- ✅ Cache issues - نسخه 2.1

### در حال بررسی:
- هیچ مشکلی گزارش نشده

---

## 📞 پشتیبانی

اگر مشکلی پیش اومد:

1. Console رو چک کنید
2. Hard refresh کنید (`Ctrl+F5`)
3. Version رو چک کنید در Console:
```javascript
console.log(typeof CustomTooltip);
```

---

**ساخته شده با ❤️ برای MahERP**
