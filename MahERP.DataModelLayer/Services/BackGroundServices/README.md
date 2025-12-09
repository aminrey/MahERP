# 🎯 Background Services مستندات

این پوشه شامل تمام **Background Services** سیستم MahERP است.

## 📁 ساختار پوشه‌ها

```
BackgroundServices/
├── Notifications/          # سرویس‌های مربوط به اعلان‌ها
├── Communications/         # سرویس‌های ارتباطی (Email, SMS, Telegram)
├── TaskManagement/         # سرویس‌های مدیریت تسک
├── System/                 # سرویس‌های سیستمی
└── BackgroundServicesMonitor.cs (مانیتورینگ)
```

---

## 🔔 **Notifications/**

### **1. NotificationProcessingBackgroundService**
- **مسئولیت:** پردازش صف اعلان‌های تسک
- **زمان اجرا:** هر 2 ثانیه
- **ورودی:** ConcurrentQueue<NotificationQueueItem>
- **خروجی:** ارسال اعلان به کاربران

### **2. ScheduledNotificationBackgroundService**
- **مسئولیت:** اجرای قالب‌های اعلان زمان‌بندی شده
- **زمان اجرا:** هر 1 دقیقه
- **ورودی:** NotificationTemplate (Scheduled)
- **خروجی:** ارسال پیام به کاربران

### **3. TelegramQueueProcessingBackgroundService**
- **مسئولیت:** پردازش صف تلگرام
- **زمان اجرا:** هر 5 ثانیه
- **ورودی:** TelegramNotificationQueue
- **خروجی:** ارسال پیام به تلگرام

---

## 📧 **Communications/**

### **1. EmailBackgroundService**
- **مسئولیت:** پردازش صف ایمیل
- **زمان اجرا:** هر 1 دقیقه
- **وضعیت:** 🚧 TODO (پیاده‌سازی نشده)

### **2. SmsBackgroundService**
- **مسئولیت:** ارسال پیامک‌های صف
- **زمان اجرا:** هر 30 ثانیه
- **ورودی:** SmsQueue
- **خروجی:** ارسال پیامک

### **3. SmsDeliveryCheckService**
- **مسئولیت:** بررسی وضعیت تحویل پیامک‌ها
- **زمان اجرا:** هر 1 ساعت
- **تأخیر اولیه:** 5 دقیقه

### **4. TelegramPollingBackgroundService**
- **مسئولیت:** دریافت پیام‌های تلگرام (Webhook)
- **زمان اجرا:** مداوم (Long Polling)
- **دستورات:** /start, /help, /status

---

## 📋 **TaskManagement/**

### **1. TaskReminderBackgroundService**
- **مسئولیت:** پردازش یادآوری‌های تسک
- **زمان اجرا:** هر 1 دقیقه
- **ورودی:** TaskReminderSchedule
- **خروجی:** تولید TaskReminderEvent

### **2. ScheduledTaskCreationBackgroundService**
- **مسئولیت:** ساخت خودکار تسک‌های زمان‌بندی شده
- **زمان اجرا:** هر 1 دقیقه
- **ورودی:** ScheduledTaskCreation
- **خروجی:** تسک جدید

---

## ⚙️ **System/**

### **1. ExpiredRoleCleanupService**
- **مسئولیت:** پاکسازی نقش‌های منقضی شده
- **زمان اجرا:** هر 1 ساعت
- **تأخیر اولیه:** 1 دقیقه

### **2. ModuleTrackingBackgroundService**
- **مسئولیت:** ذخیره آخرین ماژول استفاده شده کاربران
- **زمان اجرا:** On-Demand (Event-Driven)
- **پترن:** ConcurrentQueue + Semaphore

### **3. SystemSeedDataBackgroundService**
- **مسئولیت:** اطمینان از وجود Seed Data
- **زمان اجرا:** یکبار در ابتدا (5 ثانیه بعد از شروع)

### **4. PermissionSyncBackgroundService** ⭐ NEW
- **مسئولیت:** همگام‌سازی Permission ها از فایل JSON با دیتابیس
- **زمان اجرا:** هر 1 ساعت + یکبار در ابتدا (10 ثانیه بعد از شروع)
- **ورودی:** `Data/SeedData/Permissions.json`
- **خروجی:** Permission های جدید در دیتابیس
- **ویژگی‌ها:**
  - فقط Permission های جدید اضافه می‌شوند
  - Permission های موجود ویرایش/حذف نمی‌شوند
  - پشتیبانی از ساختار سلسله‌مراتبی (Parent-Child)

---

## 🔧 **نحوه ثبت در Program.cs**

```csharp
// ⭐ Notifications
builder.Services.AddHostedService<NotificationProcessingBackgroundService>();
builder.Services.AddHostedService<ScheduledNotificationBackgroundService>();
builder.Services.AddHostedService<TelegramQueueProcessingBackgroundService>();

// ⭐ Communications
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<SmsBackgroundService>();
builder.Services.AddHostedService<SmsDeliveryCheckService>();
builder.Services.AddHostedService<TelegramPollingBackgroundService>();

// ⭐ TaskManagement
builder.Services.AddHostedService<TaskReminderBackgroundService>();
builder.Services.AddHostedService<ScheduledTaskCreationBackgroundService>();

// ⭐ System
builder.Services.AddHostedService<ExpiredRoleCleanupService>();
builder.Services.AddHostedService<ModuleTrackingBackgroundService>();
builder.Services.AddHostedService<SystemSeedDataBackgroundService>();

// ⭐⭐⭐ مانیتور (اختیاری)
builder.Services.AddHostedService<BackgroundServicesMonitor>();
```

---

## 📊 **آمار عملکرد**

| سرویس | فاصله زمانی | وضعیت |
|------|-------------|-------|
| NotificationProcessing | 2s | ✅ فعال |
| ScheduledNotification | 1m | ✅ فعال |
| TelegramQueue | 5s | ✅ فعال |
| Email | 1m | 🚧 TODO |
| SMS | 30s | ✅ فعال |
| SmsDelivery | 1h | ✅ فعال |
| TelegramPolling | مداوم | ✅ فعال |
| TaskReminder | 1m | ✅ فعال |
| ScheduledTask | 1m | ✅ فعال |
| ExpiredRole | 1h | ✅ فعال |
| ModuleTracking | On-Demand | ✅ فعال |
| SystemSeedData | یکبار | ✅ فعال |
| **PermissionSync** | **1h** | ✅ **فعال** |

---

## ⚠️ **نکات مهم**

### **1. تداخل زمانی:**
- همه سرویس‌ها در **Thread های جداگانه** اجرا می‌شوند
- از **Scoped Services** استفاده می‌کنند (IServiceProvider)
- هر سرویس **DbContext** خودش را دارد

### **2. اولویت‌بندی:**
- **Critical:** NotificationProcessing, TelegramQueue
- **High:** ScheduledNotification, TaskReminder
- **Medium:** SMS, Email
- **Low:** ExpiredRole, SystemSeedData

### **3. خطایابی:**
- همه سرویس‌ها **ILogger** دارند
- در صورت خطا، **سرویس متوقف نمی‌شود**
- **Retry** اتوماتیک برای سرویس‌های ارتباطی

---

## 🚀 **توسعه آینده**

- [ ] افزودن **Health Check** برای هر سرویس
- [ ] **Dashboard** برای مانیتورینگ Real-Time
- [ ] **Pause/Resume** قابلیت برای هر سرویس
- [ ] **Metrics** (Prometheus/Grafana)
- [ ] **Distributed Locking** برای Scale-Out

---

## 📝 **مثال استفاده**

### **Enqueue Notification:**
```csharp
NotificationProcessingBackgroundService.EnqueueTaskNotification(
    taskId: 123,
    senderUserId: "user-1",
    eventType: NotificationEventType.TaskAssigned,
    priority: 1
);
```

### **Module Tracking:**
```csharp
var trackingService = scope.ServiceProvider
    .GetRequiredService<ModuleTrackingBackgroundService>();

trackingService.EnqueueModuleTracking(
    userId: "user-1",
    moduleType: ModuleType.Tasking
);
```

---

## 📞 **پشتیبانی**

برای سوالات و مشکلات، به بخش **Issues** در GitHub مراجعه کنید.
