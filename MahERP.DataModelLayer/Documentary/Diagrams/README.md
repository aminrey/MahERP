# 📊 نمودارها و دیاگرام‌های سیستم MahERP

این پوشه شامل **نمودارهای تخصصی** و **دیاگرام‌های مفصل** سیستم MahERP است که از فایل اصلی [`MahERP-Database-Diagrams.md`](../MahERP-Database-Diagrams.md) جداسازی شده‌اند.

---

## 📁 فهرست فایل‌ها

### ✅ **فایل‌های آماده**

| # | فایل | عنوان | وضعیت | توضیحات |
|---|------|-------|--------|---------|
| 1 | [`01-ERD-Diagram.md`](01-ERD-Diagram.md) | نمودار ERD کلی | ✅ آماده | نمودار روابط موجودیت‌ها |
| 2 | [`02-Permission-System-Diagram.md`](02-Permission-System-Diagram.md) | سیستم دسترسی | ✅ آماده | معماری Permission-based |
| 3 | [`03-Task-System-Diagram.md`](03-Task-System-Diagram.md) | سیستم تسک‌ها | ✅ آماده | معماری Task Management |
| 4 | [`04-Task-Supervision-Diagram.md`](04-Task-Supervision-Diagram.md) | سیستم نظارت بر تسک‌ها | ✅ آماده | نظارت هوشمند و چندسطحی ⭐ |
| 5 | [`05-Scheduled-Task-Diagram.md`](05-Scheduled-Task-Diagram.md) | تسک‌های زمان‌بندی شده | ✅ آماده | ساخت خودکار تسک‌ها 🆕 |
| 6 | [`06-Notification-System-Diagram.md`](06-Notification-System-Diagram.md) | سیستم اعلان‌رسانی | ✅ آماده | اعلان‌رسانی چندکاناله پیشرفته ⭐ |
| 7 | [`07-Background-Services-Diagram.md`](07-Background-Services-Diagram.md) | Background Services | ✅ آماده | 8 سرویس پس‌زمینه و زمان‌بندی ⚙️ |
| 8 | [`08-Statistics-Troubleshooting.md`](08-Statistics-Troubleshooting.md) | آمار و رفع مشکلات | ✅ آماده | Query ها، آمار و راهنمای رفع مشکل 📊 |

---

## 🎯 راهنمای استفاده

### برای یادگیری سریع:
```
1️⃣ شروع: 01-ERD-Diagram.md          → درک کلی ساختار
2️⃣ ادامه: 02-Permission-System       → سیستم دسترسی
3️⃣ تسک‌ها: 03-Task-System           → معماری Task Management
4️⃣ نظارت: 04-Task-Supervision        → سیستم نظارت پیشرفته ⭐
```

### برای توسعه‌دهندگان:
```
🆕 جدیدترین: 05-Scheduled-Task       → تسک‌های خودکار
⭐ پیشرفته: 06-Notification-System    → اعلان‌رسانی چندکاناله
⚙️ پشت صحنه: 07-Background-Services  → سرویس‌های پس‌زمینه
🐛 رفع مشکل: 08-Statistics-Troubleshooting → آمار و دیباگ
```

---

## 🆕 جدیدترین تغییرات (آذر 1403)

### ✨ ویژگی‌های جدید اضافه شده:

#### 📋 **Scheduled Task Creation System** 🆕
- ✅ ساخت خودکار تسک‌ها بر اساس زمان‌بندی
- ✅ 4 نوع: یکبار، روزانه، هفتگی، ماهانه
- ✅ قالب JSON با متغیرهای پویا
- ✅ Background Service اختصاصی
- ✅ پشتیبانی کامل از TimeZone ایران
- 📄 **فایل:** [`05-Scheduled-Task-Diagram.md`](05-Scheduled-Task-Diagram.md)

#### 🔔 **Advanced Notification System** ⭐
- ✅ زمان‌بندی خودکار اعلان‌ها (روزانه، هفتگی، ماهانه)
- ✅ قالب‌های پویا با 20+ متغیر
- ✅ 3 کانال: Email, SMS, Telegram
- ✅ SignalR برای Real-time
- ✅ مکانیزم جلوگیری از اجرای مکرر
- 📄 **فایل:** [`06-Notification-System-Diagram.md`](06-Notification-System-Diagram.md)

#### 👁️ **Task Supervision System** ⭐
- ✅ نظارت خودکار بر اساس سمت (PowerLevel)
- ✅ نظارت رونوشتی (Carbon Copy)
- ✅ مجوزهای خاص نظارتی
- ✅ فیلتر محدود به تیم (Team-scoped)
- ✅ تشخیص دلیل نظارت
- 📄 **فایل:** [`04-Task-Supervision-Diagram.md`](04-Task-Supervision-Diagram.md)

#### ⚙️ **Background Services Optimization**
- ✅ 8 سرویس پس‌زمینه
- ✅ زمان‌بندی بهینه شده
- ✅ پشتیبانی از TimeZone ایران
- ✅ Retry Logic و Error Handling
- 📄 **فایل:** [`07-Background-Services-Diagram.md`](07-Background-Services-Diagram.md)

---

## 📖 توضیحات تکمیلی

### چرا جداسازی شدند؟

فایل اصلی `MahERP-Database-Diagrams.md` به دلیل **حجم زیاد** (5000+ خط) به فایل‌های کوچک‌تر تقسیم شد:

✅ **خوانایی بهتر** - هر موضوع در فایل جداگانه  
✅ **دسترسی سریع‌تر** - مستقیم به موضوع مورد نظر  
✅ **مدیریت آسان‌تر** - بروزرسانی‌های محلی  
✅ **سازماندهی حرفه‌ای** - ساختار مدولار  

---

## 🔍 جستجوی سریع

### نیاز دارید به:

| موضوع | فایل مرتبط |
|-------|-----------|
| ساختار کلی دیتابیس | 📊 [01-ERD-Diagram](01-ERD-Diagram.md) |
| سیستم احراز هویت و دسترسی | 🔐 [02-Permission-System](02-Permission-System-Diagram.md) |
| نحوه کار تسک‌ها | 📋 [03-Task-System](03-Task-System-Diagram.md) |
| چگونگی نظارت بر تسک‌ها | 👁️ [04-Task-Supervision](04-Task-Supervision-Diagram.md) |
| ساخت خودکار تسک‌ها 🆕 | 🕐 [05-Scheduled-Task](05-Scheduled-Task-Diagram.md) |
| ارسال اعلان‌ها | 🔔 [06-Notification-System](06-Notification-System-Diagram.md) |
| Background Services | ⚙️ [07-Background-Services](07-Background-Services-Diagram.md) |
| رفع مشکلات و آمار | 🐛 [08-Statistics-Troubleshooting](08-Statistics-Troubleshooting.md) |

---

## 💡 نکات مهم

### برای توسعه‌دهندگان جدید:

1. **ابتدا ERD را مطالعه کنید** - برای درک کلی ساختار
2. **سپس Permission System** - برای فهم امنیت و دسترسی
3. **سپس Task System** - قلب تپنده سیستم
4. **سپس Supervision** - نظارت هوشمند ⭐
5. **در نهایت Advanced Features** - زمان‌بندی و اعلان‌ها 🆕

### برای رفع مشکلات:

مستقیماً به [`08-Statistics-Troubleshooting.md`](08-Statistics-Troubleshooting.md) مراجعه کنید 🐛

---

## 🔗 لینک‌های مرتبط

- 📘 [مستندات اصلی سیستم](../MahERP-System-Documentation.md)
- 📊 [نمودارهای دیتابیس (Index)](../MahERP-Database-Diagrams.md)
- 🏠 [صفحه اصلی پروژه](../../../../README.md)

---

**نسخه:** 3.0.0  
**تاریخ:** آذر 1403  
**وضعیت:** ✅ Complete - همه فایل‌ها آماده

---

*برای هرگونه سوال یا پیشنهاد، لطفاً با تیم توسعه تماس بگیرید.*
