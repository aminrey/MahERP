# 📊 نمودارها و دیاگرام‌های دیتابیس MahERP

> **🚀 نسخه:** 3.0.0  
> **📅 آخرین بروزرسانی:** آذر 1403  
> **✅ وضعیت:** Complete - تقسیم شده به فایل‌های جداگانه

---

## 📢 اطلاعیه مهم

این فایل به عنوان **Index** و **راهنمای کلی** عمل می‌کند. برای دسترسی به **نمودارها و دیاگرام‌های کامل**، لطفاً به پوشه [`Diagrams/`](Diagrams/) مراجعه کنید.

---

## 📁 ساختار مستندات

```
Documentary/
├── MahERP-System-Documentation.md      📘 مستندات جامع سیستم
├── MahERP-Database-Diagrams.md         📊 این فایل (Index)
└── Diagrams/                            📂 پوشه نمودارها
    ├── README.md                        📄 راهنمای کامل
    ├── 01-ERD-Diagram.md               🗄️ نمودار ERD کلی
    ├── 02-Permission-System-Diagram.md 🔐 سیستم دسترسی
    ├── 03-Task-System-Diagram.md       📋 سیستم تسک‌ها
    ├── 04-Task-Supervision-Diagram.md  👁️ سیستم نظارت (⭐ جدید)
    ├── 05-Scheduled-Task-Diagram.md    🕐 تسک‌های زمان‌بندی شده (🆕 جدیدترین)
    ├── 06-Notification-System-Diagram.md 🔔 سیستم اعلان‌رسانی (⭐ پیشرفته)
    ├── 07-Background-Services-Diagram.md ⚙️ Background Services
    └── 08-Statistics-Troubleshooting.md 📊 آمار و رفع مشکلات
```

---

## 🎯 دسترسی سریع

### 📂 [مشاهده همه نمودارها در پوشه Diagrams](Diagrams/)

### یا دسترسی مستقیم:

| موضوع | فایل | وضعیت |
|-------|------|--------|
| 🗄️ **نمودار ERD کلی** | [01-ERD-Diagram.md](Diagrams/01-ERD-Diagram.md) | ✅ |
| 🔐 **سیستم دسترسی** | [02-Permission-System-Diagram.md](Diagrams/02-Permission-System-Diagram.md) | ✅ |
| 📋 **سیستم تسک‌ها** | [03-Task-System-Diagram.md](Diagrams/03-Task-System-Diagram.md) | ✅ |
| 👁️ **سیستم نظارت بر تسک‌ها** ⭐ | [04-Task-Supervision-Diagram.md](Diagrams/04-Task-Supervision-Diagram.md) | ✅ جدید |
| 🕐 **تسک‌های زمان‌بندی شده** 🆕 | [05-Scheduled-Task-Diagram.md](Diagrams/05-Scheduled-Task-Diagram.md) | ✅ جدیدترین |
| 🔔 **سیستم اعلان‌رسانی** ⭐ | [06-Notification-System-Diagram.md](Diagrams/06-Notification-System-Diagram.md) | ✅ پیشرفته |
| ⚙️ **Background Services** | [07-Background-Services-Diagram.md](Diagrams/07-Background-Services-Diagram.md) | ✅ |
| 📊 **آمار و رفع مشکلات** | [08-Statistics-Troubleshooting.md](Diagrams/08-Statistics-Troubleshooting.md) | ✅ |

---

## 🆕 جدیدترین تغییرات

### ✨ ویژگی‌های جدید (آذر 1403)

#### 🕐 **Scheduled Task Creation System** 🆕 **جدیدترین**
- ساخت خودکار تسک‌ها بر اساس زمان‌بندی
- پشتیبانی از 4 نوع: یکبار، روزانه، هفتگی، ماهانه
- قالب JSON با متغیرهای پویا
- Background Service اختصاصی
- **مستندات:** [05-Scheduled-Task-Diagram.md](Diagrams/05-Scheduled-Task-Diagram.md)

#### 🔔 **Advanced Notification System** ⭐
- زمان‌بندی خودکار اعلان‌ها
- قالب‌های پویا با 20+ متغیر
- 3 کانال ارسال: Email, SMS, Telegram
- مکانیزم جلوگیری از اجرای مکرر
- **مستندات:** [06-Notification-System-Diagram.md](Diagrams/06-Notification-System-Diagram.md)

#### 👁️ **Task Supervision System** ⭐
- نظارت خودکار بر اساس سمت
- نظارت رونوشتی (Carbon Copy)
- مجوزهای خاص نظارتی
- فیلتر محدود به تیم
- **مستندات:** [04-Task-Supervision-Diagram.md](Diagrams/04-Task-Supervision-Diagram.md)

---

## 📖 خلاصه محتوا

### 1️⃣ ERD Diagram
- نمودار ERD کلی سیستم
- روابط بین جداول
- ساختار کلیدهای اصلی و خارجی

### 2️⃣ Permission System
- معماری سیستم دسترسی
- نقش‌ها و مجوزها
- الگوریتم بررسی دسترسی

### 3️⃣ Task System
- جریان کامل تسک‌ها
- عملیات و اختصاص‌ها
- یادآورها و گزارش کار

### 4️⃣ Task Supervision ⭐ **جدید**
- انواع نظارت (سیستمی، رونوشتی، مجوز خاص)
- الگوریتم محاسبه ناظران
- نمایش دلیل نظارت

### 5️⃣ Scheduled Tasks 🆕 **جدیدترین**
- ساخت خودکار تسک‌ها
- انواع زمان‌بندی
- قالب JSON و متغیرهای پویا

### 6️⃣ Notification System ⭐ **پیشرفته**
- سیستم اعلان‌رسانی چندکاناله
- قالب‌های زمان‌بندی شده
- Background Service اعلان‌ها

### 7️⃣ Background Services
- 8 سرویس پس‌زمینه
- زمان‌بندی و بهینه‌سازی
- پشتیبانی از TimeZone ایران

### 8️⃣ Statistics & Troubleshooting
- Query های آماری
- راهنمای رفع مشکلات
- نکات بهینه‌سازی

---

## 🎓 راهنمای مطالعه

### برای شروع:
1. ابتدا [`README.md`](Diagrams/README.md) در پوشه Diagrams را بخوانید
2. سپس [`01-ERD-Diagram.md`](Diagrams/01-ERD-Diagram.md) برای درک کلی
3. بعد [`02-Permission-System-Diagram.md`](Diagrams/02-Permission-System-Diagram.md) برای امنیت

### برای توسعه‌دهندگان:
- 📋 **Task Development:** فایل‌های 03, 04, 05
- 🔔 **Notification Development:** فایل 06
- ⚙️ **Background Services:** فایل 07
- 🐛 **Debugging:** فایل 08

---

## 💡 چرا جداسازی شد؟

این فایل قبلاً **5000+ خط** داشت و برای **خوانایی بهتر** و **دسترسی سریع‌تر** به فایل‌های کوچک‌تر تقسیم شد.

### مزایا:
✅ خوانایی بهتر  
✅ دسترسی سریع‌تر  
✅ مدیریت آسان‌تر  
✅ بروزرسانی محلی  
✅ ساختار مدولار  

---

## 🔗 لینک‌های مرتبط

- 📘 [مستندات جامع سیستم](MahERP-System-Documentation.md)
- 📂 [پوشه نمودارها](Diagrams/)
- 🏠 [صفحه اصلی پروژه](../../README.md)

---

## 📊 آمار سریع

```
✅ تعداد فایل‌های نمودار: 8
✅ تعداد جداول دیتابیس: 105+
✅ تعداد Background Services: 8
✅ کانال‌های ارسال اعلان: 3 (Email, SMS, Telegram)
✅ انواع زمان‌بندی: 4 (یکبار، روزانه، هفتگی، ماهانه)
```

---

**نسخه:** 3.0.0  
**تاریخ:** آذر 1403  
**وضعیت:** ✅ Complete

---

*برای جزئیات بیشتر، لطفاً به پوشه [`Diagrams/`](Diagrams/) مراجعه کنید.*
