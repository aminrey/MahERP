# 🎯 سیستم Background Jobs

سیستم مدیریت کارهای پس‌زمینه (Background Jobs) برای پردازش‌های طولانی مدت مانند ارسال انبوه پیامک، ایمیل، تولید گزارش و ...

---

## 📋 **فهرست مطالب**

1. [نصب و راه‌اندازی](#نصب-و-راه‌اندازی)
2. [ساختار](#ساختار)
3. [نحوه استفاده](#نحوه-استفاده)
4. [API Documentation](#api-documentation)
5. [UI Components](#ui-components)

---

## 🚀 **نصب و راه‌اندازی**

### 1. اجرای Migration

```bash
Update-Database
```

### 2. Register در Program.cs / Startup.cs

```csharp
// Add to ConfigureServices
services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
```

### 3. Inject در Controller

```csharp
private readonly IBackgroundJobRepository _jobRepo;

public YourController(IBackgroundJobRepository jobRepo)
{
    _jobRepo = jobRepo;
}
```

---

## 🏗️ **ساختار**

### **Entity: BackgroundJob**

```csharp
public class BackgroundJob
{
    public int Id { get; set; }
    public byte JobType { get; set; }      // 0=SMS, 1=Email, 2=Report, 3=Export
    public string Title { get; set; }
    public string Description { get; set; }
    public byte Status { get; set; }       // 0=Pending, 1=Running, 2=Completed, 3=Failed, 4=Cancelled
    public int Progress { get; set; }      // 0-100
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string CreatedByUserId { get; set; }
    public string Metadata { get; set; }   // JSON format
}
```

### **Repository: IBackgroundJobRepository**

```csharp
Task<int> CreateJobAsync(BackgroundJob job);
Task UpdateJobAsync(BackgroundJob job);
Task UpdateProgressAsync(int jobId, int progress, int processed, int success, int failed);
Task CompleteJobAsync(int jobId, bool isSuccess, string errorMessage = null);
Task<List<BackgroundJob>> GetUserActiveJobsAsync(string userId);
Task<List<BackgroundJob>> GetUserJobsAsync(string userId, int take = 10);
Task<BackgroundJob> GetJobByIdAsync(int jobId);
Task DeleteOldJobsAsync(int daysOld = 7);
```

---

## 💡 **نحوه استفاده**

### **مثال 1: ارسال انبوه پیامک**

```csharp
[HttpPost]
public async Task<IActionResult> SendBulkSms(List<string> contacts, string message)
{
    var currentUser = await _userManager.GetUserAsync(User);
    
    // 1️⃣ ایجاد Job
    var job = new BackgroundJob
    {
        JobType = 0, // SMS
        Title = $"ارسال پیامک به {contacts.Count} شماره",
        Description = message,
        Status = 0, // Pending
        TotalItems = contacts.Count,
        CreatedByUserId = currentUser.Id,
        StartDate = DateTime.Now
    };
    
    var jobId = await _jobRepo.CreateJobAsync(job);
    
    // 2️⃣ پردازش در Background
    _ = Task.Run(async () =>
    {
        try
        {
            // شروع Job
            await _jobRepo.UpdateJobAsync(new BackgroundJob
            {
                Id = jobId,
                Status = 1 // Running
            });
            
            int processed = 0;
            int success = 0;
            int failed = 0;
            
            foreach (var contact in contacts)
            {
                try
                {
                    // ارسال پیامک
                    var result = await _smsService.SendAsync(contact, message);
                    
                    if (result.IsSuccess)
                        success++;
                    else
                        failed++;
                }
                catch
                {
                    failed++;
                }
                
                processed++;
                
                // بروزرسانی Progress
                var progress = (int)((processed / (double)contacts.Count) * 100);
                await _jobRepo.UpdateProgressAsync(jobId, progress, processed, success, failed);
                
                await Task.Delay(200); // Rate limiting
            }
            
            // اتمام Job
            await _jobRepo.CompleteJobAsync(jobId, true);
        }
        catch (Exception ex)
        {
            await _jobRepo.CompleteJobAsync(jobId, false, ex.Message);
        }
    });
    
    return Json(new
    {
        success = true,
        message = "ارسال پیامک شروع شد",
        jobId = jobId
    });
}
```

### **مثال 2: تولید گزارش**

```csharp
[HttpPost]
public async Task<IActionResult> GenerateReport(DateTime fromDate, DateTime toDate)
{
    var currentUser = await _userManager.GetUserAsync(User);
    
    var job = new BackgroundJob
    {
        JobType = 2, // Report
        Title = "تولید گزارش فروش",
        Description = $"از تاریخ {fromDate:yyyy/MM/dd} تا {toDate:yyyy/MM/dd}",
        Status = 0,
        TotalItems = 100, // تعداد صفحات یا رکوردها
        CreatedByUserId = currentUser.Id,
        StartDate = DateTime.Now
    };
    
    var jobId = await _jobRepo.CreateJobAsync(job);
    
    _ = Task.Run(async () =>
    {
        try
        {
            await _jobRepo.UpdateJobAsync(new BackgroundJob { Id = jobId, Status = 1 });
            
            // تولید گزارش
            var report = await _reportService.GenerateAsync(fromDate, toDate);
            
            await _jobRepo.CompleteJobAsync(jobId, true);
        }
        catch (Exception ex)
        {
            await _jobRepo.CompleteJobAsync(jobId, false, ex.Message);
        }
    });
    
    return Json(new { success = true, jobId = jobId });
}
```

---

## 📡 **API Documentation**

### **GET: /BackgroundJob/GetActiveJobs**

دریافت Job های فعال کاربر

**Response:**
```json
{
  "success": true,
  "jobs": [
    {
      "id": 1,
      "title": "ارسال پیامک به 150 شماره",
      "description": "متن پیام...",
      "jobType": 0,
      "jobTypeText": "ارسال انبوه پیامک",
      "status": 1,
      "statusText": "در حال اجرا",
      "statusBadgeClass": "bg-primary",
      "progress": 65,
      "totalItems": 150,
      "processedItems": 97,
      "successCount": 95,
      "failedCount": 2,
      "startDate": "2024/11/17 14:30"
    }
  ]
}
```

### **GET: /BackgroundJob/GetJobHistory**

دریافت تاریخچه Job ها

**Response:**
```json
{
  "success": true,
  "jobs": [
    {
      "id": 2,
      "title": "ارسال پیامک به 50 شماره",
      "statusText": "تکمیل شده",
      "statusBadgeClass": "bg-success",
      "successCount": 48,
      "failedCount": 2,
      "startDate": "2024/11/17 12:00",
      "completedDate": "2024/11/17 12:05",
      "duration": "05:23"
    }
  ]
}
```

### **GET: /BackgroundJob/GetJobDetails/{id}**

دریافت جزئیات یک Job

**Response:**
```json
{
  "success": true,
  "job": {
    "id": 1,
    "title": "ارسال پیامک به 150 شماره",
    "description": "متن پیام...",
    "jobTypeText": "ارسال انبوه پیامک",
    "statusText": "تکمیل شده",
    "progress": 100,
    "totalItems": 150,
    "processedItems": 150,
    "successCount": 148,
    "failedCount": 2,
    "startDate": "2024/11/17 14:30",
    "completedDate": "2024/11/17 14:45",
    "duration": "15:30",
    "errorMessage": null
  }
}
```

---

## 🎨 **UI Components**

### **دکمه در Header**

دکمه Background Jobs فقط در CrmArea و فقط زمانی نمایش داده می‌شود که Job فعال وجود داشته باشد.

```html
<button id="page-header-jobs-dropdown" class="btn btn-alt-secondary">
    <i class="fa fa-tasks"></i>
    <span id="headerJobsBadge" class="badge bg-primary">2</span>
</button>
```

### **Dropdown محتوا**

```html
<div class="dropdown-menu">
    <!-- Active Jobs -->
    <div class="job-item">
        <div class="fw-semibold">ارسال پیامک به 150 شماره</div>
        <div class="progress">
            <div class="progress-bar" style="width: 65%"></div>
        </div>
        <small>65% - 97/150</small>
    </div>
    
    <!-- Recent Jobs -->
    <div class="job-history-item">
        <div class="fw-semibold">ارسال پیامک به 50 شماره</div>
        <span class="badge bg-success">تکمیل شده</span>
    </div>
</div>
```

### **JavaScript Auto-refresh**

```javascript
// بروزرسانی خودکار هر 5 ثانیه
setInterval(function() {
    if (hasActiveJobs) {
        loadBackgroundJobs();
    }
}, 5000);
```

---

## 🔄 **Workflow**

```
1. کاربر درخواست ارسال می‌دهد
   ↓
2. Job ایجاد می‌شود (Status = Pending)
   ↓
3. Task.Run شروع می‌شود
   ↓
4. Status = Running
   ↓
5. پردازش آیتم‌ها (با بروزرسانی Progress)
   ↓
6. Status = Completed یا Failed
   ↓
7. نمایش در UI
```

---

## 🎯 **Best Practices**

1. **Rate Limiting**: از `await Task.Delay()` برای محدود کردن سرعت استفاده کنید
2. **Error Handling**: همیشه از try-catch استفاده کنید
3. **Progress Update**: هر 5-10 آیتم یکبار Progress را بروز کنید
4. **Cleanup**: Job های قدیمی را پاک کنید (بیش از 7 روز)
5. **Security**: فقط کاربر ایجادکننده می‌تواند Job را مشاهده کند

---

## 📊 **Job Types**

| Type | Value | Description |
|------|-------|-------------|
| SMS Bulk Send | 0 | ارسال انبوه پیامک |
| Email Bulk Send | 1 | ارسال انبوه ایمیل |
| Report Generation | 2 | تولید گزارش |
| Data Export | 3 | خروجی اطلاعات |

## 📈 **Status Types**

| Status | Value | Description |
|--------|-------|-------------|
| Pending | 0 | در انتظار |
| Running | 1 | در حال اجرا |
| Completed | 2 | تکمیل شده |
| Failed | 3 | ناموفق |
| Cancelled | 4 | لغو شده |

---

## 🛠️ **Maintenance**

### پاک‌سازی Job های قدیمی

```csharp
// اجرای روزانه (در Background Service یا Scheduled Task)
await _jobRepo.DeleteOldJobsAsync(daysOld: 7);
```

---

## 🎓 **مثال‌های بیشتر**

در فایل‌های زیر می‌توانید مثال‌های کامل را مشاهده کنید:

- `SmsSendController.cs` - ارسال انبوه پیامک
- `BackgroundJobController.cs` - مدیریت Job ها
- `_BackgroundJobsPartial.cshtml` - UI Components

---

**تاریخ ایجاد:** 1403/08/27  
**نسخه:** 1.0.0  
**نویسنده:** MahERP Development Team
