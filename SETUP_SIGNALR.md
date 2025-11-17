# 🔧 Register Background Jobs & SignalR Services

برای فعال‌سازی کامل سیستم Background Jobs با SignalR، کدهای زیر را به `Program.cs` یا `Startup.cs` اضافه کنید:

---

## 1️⃣ **Add Services (در ConfigureServices یا builder.Services)**

```csharp
// ========== Background Jobs Services ==========
builder.Services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
builder.Services.AddScoped<IBackgroundJobNotificationService, BackgroundJobNotificationService>();

// ========== SignalR ==========
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
```

---

## 2️⃣ **Configure Middleware (در Configure یا app.Use)**

```csharp
// ========== SignalR Hub Mapping ==========
app.MapHub<BackgroundJobHub>("/hubs/backgroundjob");
```

---

## 3️⃣ **کد کامل برای Program.cs (.NET 6+)**

```csharp
using MahERP.Hubs;
using MahERP.Services;
using MahERP.DataModelLayer.Repository;

var builder = WebApplication.CreateBuilder(args);

// ... سایر سرویس‌ها ...

// ========== Background Jobs & SignalR ==========
builder.Services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
builder.Services.AddScoped<IBackgroundJobNotificationService, BackgroundJobNotificationService>();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// ... سایر middleware ها ...

// ========== SignalR Hub ==========
app.MapHub<BackgroundJobHub>("/hubs/backgroundjob");

app.Run();
```

---

## 4️⃣ **تست اتصال SignalR**

بعد از اضافه کردن کدها، می‌توانید در Console مرورگر (F12) بررسی کنید:

```javascript
// باید این پیام را ببینید:
✅ BackgroundJob SignalR connected
```

---

## 5️⃣ **Troubleshooting**

### مشکل: SignalR وصل نمی‌شود

**راه حل:**
1. بررسی کنید `/hubs/backgroundjob` در مرورگر قابل دسترسی باشد
2. CORS را در صورت نیاز فعال کنید:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSignalR", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

app.UseCors("AllowSignalR");
```

### مشکل: Realtime بروزرسانی نمی‌شود

**راه حل:**
1. بررسی کنید `IBackgroundJobNotificationService` در SmsSendController Inject شده باشد
2. در هنگام Debug، در Console مرورگر پیام‌های SignalR را بررسی کنید

---

## 6️⃣ **نکات مهم**

- ✅ SignalR فقط برای کاربر ایجادکننده Job کار می‌کند
- ✅ اتصال SignalR به صورت خودکار Reconnect می‌شود
- ✅ هر کاربر فقط Job های خودش را می‌بیند
- ✅ وقتی هیچ Job فعالی نباشد، دکمه مخفی می‌شود

---

## 7️⃣ **تست عملکرد**

1. وارد صفحه ارسال پیامک شوید: `/CrmArea/SmsSend/Index`
2. افراد و پیام را انتخاب کنید
3. روی "ارسال پیامک" کلیک کنید
4. دکمه Background Jobs باید ظاهر شود
5. Progress Bar باید به صورت Realtime بروز شود
6. پس از اتمام، Notification نمایش داده شود

---

**✅ تمام! سیستم آماده است!**
