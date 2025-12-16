# 📚 مستندات سیستم CRM - MahERP

خوش آمدید به مستندات کامل ماژول CRM (Customer Relationship Management) سیستم MahERP.

---

## 📋 فهرست مستندات

این پوشه شامل مستندات کامل و جامع سیستم CRM است که برای مخاطبان مختلف طراحی شده:

### 1️⃣ [CRM-System-Overview.md](./CRM-System-Overview.md)
**مخاطب:** مدیران، توسعه‌دهندگان، معماران نرم‌افزار

**محتوا:**
- ✅ معرفی کلی سیستم CRM
- ✅ معماری و لایه‌بندی
- ✅ مفاهیم پایه (Lead, Customer, Interaction, Goal, Referral)
- ✅ Entities (موجودیت‌ها) و ساختار آن‌ها
- ✅ Enums
- ✅ Repositories
- ✅ ViewModels
- ✅ Controllers و Views
- ✅ فرآیندهای کاری
- ✅ Integration با سایر ماژول‌ها
- ✅ Best Practices

**زمان مطالعه:** ~45 دقیقه

---

### 2️⃣ [CRM-Database-Schema.md](./CRM-Database-Schema.md)
**مخاطب:** توسعه‌دهندگان Backend، Database Administrators

**محتوا:**
- 🗄️ ساختار کامل دیتابیس
- 🗄️ شرح تمام جداول و ستون‌ها
- 🗄️ روابط (Foreign Keys) و Constraints
- 🗄️ Indexes و Performance Optimization
- 🗄️ Seed Data (داده‌های پیش‌فرض)
- 🗄️ Migrations
- 🗄️ ERD (نمودار ER)
- 🗄️ Query Optimization Tips
- 🗄️ Backup و Maintenance

**زمان مطالعه:** ~30 دقیقه

---

### 3️⃣ [CRM-Development-Guide.md](./CRM-Development-Guide.md)
**مخاطب:** توسعه‌دهندگان (Backend و Frontend)

**محتوا:**
- 💻 راهنمای نصب و راه‌اندازی
- 💻 ساختار پروژه و فایل‌ها
- 💻 Naming Conventions و Code Style
- 💻 Repository Pattern (نحوه استفاده)
- 💻 ViewModel Pattern
- 💻 Controller Development (الگوهای Action)
- 💻 View Development (فرم‌ها، Select2، DatePicker و ...)
- 💻 Unit Testing
- 💻 Debugging
- 💻 Common Scenarios (سناریوهای رایج توسعه)

**زمان مطالعه:** ~60 دقیقه

---

### 4️⃣ [CRM-User-Guide.md](./CRM-User-Guide.md)
**مخاطب:** کاربران نهایی، کارشناسان فروش، مدیران فروش

**محتوا:**
- 👤 معرفی سیستم CRM
- 👤 مفاهیم کلیدی (به زبان ساده)
- 👤 راهنمای قدم به قدم مدیریت تعاملات
- 👤 راهنمای مدیریت اهداف فروش
- 👤 راهنمای مدیریت ارجاعات
- 👤 تنظیمات سیستم
- 👤 نکات و ترفندها
- 👤 سوالات متداول (FAQ)
- 👤 اطلاعات پشتیبانی

**زمان مطالعه:** ~40 دقیقه

---

### 5️⃣ [CRM-API-Documentation.md](./CRM-API-Documentation.md)
**مخاطب:** توسعه‌دهندگان API، توسعه‌دهندگان Frontend

**محتوا:**
- 🔌 مستندات کامل API ها
- 🔌 Endpoints (تمام آدرس‌ها)
- 🔌 Request/Response Samples
- 🔌 Parameters و Body Structure
- 🔌 ViewModels (ساختار داده)
- 🔌 Error Handling
- 🔌 Response Codes
- 🔌 نکات احراز هویت و امنیت

**زمان مطالعه:** ~35 دقیقه

---

## 🚀 شروع سریع

### برای کاربران
1. ابتدا [CRM-User-Guide.md](./CRM-User-Guide.md) را بخوانید
2. بخش "شروع کار" را دنبال کنید
3. با مثال‌های عملی آشنا شوید

### برای توسعه‌دهندگان
1. [CRM-System-Overview.md](./CRM-System-Overview.md) را برای درک کلی بخوانید
2. [CRM-Development-Guide.md](./CRM-Development-Guide.md) را برای شروع کدنویسی مطالعه کنید
3. از [CRM-API-Documentation.md](./CRM-API-Documentation.md) برای مرجع استفاده کنید
4. برای کار با دیتابیس [CRM-Database-Schema.md](./CRM-Database-Schema.md) را ببینید

### برای معماران
1. [CRM-System-Overview.md](./CRM-System-Overview.md) - معماری کلی
2. [CRM-Database-Schema.md](./CRM-Database-Schema.md) - طراحی دیتابیس
3. بخش "Best Practices" در Overview

---

## 📊 نمودار روابط اسناد

```
                    ┌────────────────────────┐
                    │  CRM-System-Overview   │
                    │  (شروع اینجا)          │
                    └───────────┬────────────┘
                                │
                ┌───────────────┼───────────────┐
                │               │               │
                ▼               ▼               ▼
    ┌──────────────────┐ ┌─────────────┐ ┌──────────────┐
    │ Database Schema  │ │   Dev Guide │ │  User Guide  │
    │ (DBA ها)         │ │(Developers) │ │  (Users)     │
    └──────────────────┘ └──────┬──────┘ └──────────────┘
                                 │
                                 ▼
                        ┌─────────────────┐
                        │ API Documentation│
                        │  (API Devs)     │
                        └─────────────────┘
```

---

## 🎯 برای چه کسی؟

| نقش | اسناد پیشنهادی | اولویت |
|-----|----------------|---------|
| **کاربر فروش** | User Guide | ⭐⭐⭐ |
| **مدیر فروش** | User Guide, System Overview | ⭐⭐⭐ |
| **Backend Developer** | Dev Guide, System Overview, Database Schema | ⭐⭐⭐ |
| **Frontend Developer** | API Documentation, Dev Guide | ⭐⭐⭐ |
| **Full-stack Developer** | همه اسناد | ⭐⭐⭐ |
| **DBA** | Database Schema | ⭐⭐⭐ |
| **معمار نرم‌افزار** | System Overview, Database Schema | ⭐⭐⭐ |
| **تستر** | User Guide, API Documentation | ⭐⭐ |
| **مدیر محصول** | System Overview, User Guide | ⭐⭐ |

---

## 🔍 جستجوی سریع

### من می‌خواهم...

#### بدانم CRM چیست
→ [System Overview - معرفی کلی](./CRM-System-Overview.md#معرفی-کلی)

#### بدانم Lead و Customer چه فرقی دارند
→ [System Overview - مفاهیم پایه](./CRM-System-Overview.md#مفاهیم-پایه)

#### یک تعامل ثبت کنم
→ [User Guide - ثبت تعامل جدید](./CRM-User-Guide.md#ثبت-تعامل-جدید)

#### ساختار جداول دیتابیس را ببینم
→ [Database Schema - جداول اصلی](./CRM-Database-Schema.md#جداول-اصلی)

#### یک Repository جدید بسازم
→ [Dev Guide - Repository Pattern](./CRM-Development-Guide.md#repository-pattern)

#### یک Controller جدید بسازم
→ [Dev Guide - Controller Development](./CRM-Development-Guide.md#controller-development)

#### API ایجاد Interaction را ببینم
→ [API Documentation - Create Interaction](./CRM-API-Documentation.md#3-create-interaction)

#### بدانم چگونه هدف را به خرید تبدیل کنم
→ [User Guide - تبدیل به خرید](./CRM-User-Guide.md#تبدیل-به-خرید)

#### Foreign Key ها را ببینم
→ [Database Schema - روابط و Foreign Keys](./CRM-Database-Schema.md#روابط-و-foreign-keys)

#### نحوه Test نوشتن را یاد بگیرم
→ [Dev Guide - Testing](./CRM-Development-Guide.md#testing)

---

## 📝 نکات مهم

### 🔄 به‌روزرسانی اسناد
این اسناد با هر آپدیت سیستم به‌روز می‌شوند. همیشه از آخرین نسخه استفاده کنید.

**نسخه فعلی:** 1.0  
**تاریخ آخرین بروزرسانی:** 1403/08/20

### 🐛 گزارش خطا در اسناد
اگر خطا یا نقصی در مستندات یافتید:
- Issue در GitHub باز کنید
- یا به ایمیل `docs@maherp.com` گزارش دهید

### 💡 پیشنهادات
برای بهبود مستندات، پیشنهادات خود را ارسال کنید.

---

## 📚 منابع اضافی

### لینک‌های مفید

- 🌐 [وبسایت MahERP](https://maherp.com)
- 📖 [Wiki](https://github.com/aminrey/MahERP/wiki)
- 💬 [انجمن پشتیبانی](https://forum.maherp.com)
- 🎥 [ویدیوهای آموزشی](https://youtube.com/maherp)
- 📧 [ایمیل پشتیبانی](mailto:support@maherp.com)

### تکنولوژی‌های استفاده شده

- **Backend:** ASP.NET Core 9.0
- **ORM:** Entity Framework Core
- **Database:** SQL Server 2019+
- **Frontend:** Razor Pages, Bootstrap 5, jQuery
- **Date/Time:** Persian Calendar Support (PersianDateTime)
- **Authentication:** ASP.NET Core Identity
- **Architecture:** Clean Architecture, Repository Pattern

---

## 🤝 مشارکت

برای مشارکت در بهبود مستندات:

1. Repository را Fork کنید
2. تغییرات خود را اعمال کنید
3. Pull Request ارسال کنید

**راهنمای Contribution:**
```markdown
- از Markdown استفاده کنید
- زبان واضح و ساده بنویسید
- مثال‌های عملی اضافه کنید
- Screenshots در صورت نیاز
- بخش FAQ را به‌روز کنید
```

---

## 📞 پشتیبانی

### ساعات کاری
- شنبه تا چهارشنبه: 9:00 - 17:00
- پنجشنبه: 9:00 - 13:00

### راه‌های ارتباطی
- 📧 Email: support@maherp.com
- 📞 Phone: 021-12345678
- 💬 Live Chat: در وبسایت
- 🐛 Bug Report: GitHub Issues

---

## 📜 License

این مستندات تحت لایسنس [MIT](../LICENSE) منتشر شده‌اند.

---

## ✍️ نویسندگان

- **تیم توسعه MahERP**
- نسخه: 1.0
- تاریخ: 1403

---

<div align="center">

**🌟 اگر این مستندات مفید بودند، حتماً Star بزنید! 🌟**

[⬆ بازگشت به بالا](#-مستندات-سیستم-crm---maherp)

</div>
