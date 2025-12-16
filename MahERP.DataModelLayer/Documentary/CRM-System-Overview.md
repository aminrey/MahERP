# مستندات سیستم CRM (مدیریت ارتباط با مشتری)

## نسخه: 1.0
## تاریخ: 1403
## نویسنده: تیم توسعه MahERP

---

## فهرست مطالب

1. [معرفی کلی](#معرفی-کلی)
2. [معماری سیستم](#معماری-سیستم)
3. [مفاهیم پایه](#مفاهیم-پایه)
4. [ساختار دیتابیس](#ساختار-دیتابیس)
5. [Entities (موجودیت‌ها)](#entities-موجودیتها)
6. [Enums](#enums)
7. [Repositories](#repositories)
8. [ViewModels](#viewmodels)
9. [Controllers](#controllers)
10. [Views](#views)
11. [فرآیندهای کاری](#فرآیندهای-کاری)
12. [قابلیت‌ها و ویژگی‌ها](#قابلیتها-و-ویژگیها)
13. [Integration با سایر ماژول‌ها](#integration-با-سایر-ماژولها)
14. [Security و Permissions](#security-و-permissions)
15. [Best Practices](#best-practices)

---

## معرفی کلی

### هدف سیستم
سیستم CRM (Customer Relationship Management) برای مدیریت جامع ارتباطات با مشتریان، سرنخ‌های فروش (Leads)، و پیگیری فرآیند فروش طراحی شده است.

### ویژگی‌های کلیدی
- ✅ مدیریت تعاملات (Interactions) با مشتریان و سرنخ‌ها
- ✅ تعریف و پیگیری اهداف فروش (Goals/Opportunities)
- ✅ پیگیری مراحل قیف فروش (Sales Funnel)
- ✅ مدیریت ارجاعات و توصیه‌ها (Referrals)
- ✅ تعریف انواع تعامل قابل تنظیم
- ✅ مدیریت مراحل بعد از خرید
- ✅ گزارش‌گیری و آمار فروش

### تکنولوژی‌های استفاده شده
- **Framework:** ASP.NET Core 9.0
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Architecture:** Repository Pattern, Unit of Work
- **UI:** Razor Pages, Bootstrap 5, jQuery
- **Date/Time:** Persian Calendar Support

---

## معماری سیستم

### لایه‌بندی (Layering)

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  (MahERP/Areas/CrmArea)                │
│  - Controllers                          │
│  - Views                                │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│         Business Logic Layer            │
│  (MahERP.DataModelLayer)               │
│  - Services                             │
│  - Repositories                         │
│  - ViewModels                           │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│         Data Access Layer               │
│  - Entities                             │
│  - DbContext                            │
│  - Configurations                       │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│         Database (SQL Server)           │
└─────────────────────────────────────────┘
```

### الگوهای طراحی (Design Patterns)

1. **Repository Pattern**
   - تمام دسترسی‌های دیتابیس از طریق Repository ها
   - جداسازی منطق دیتابیس از منطق کسب و کار

2. **Unit of Work Pattern**
   - مدیریت تراکنش‌ها
   - Commit یکجای تغییرات

3. **Dependency Injection**
   - استفاده از DI Container برای مدیریت Dependencies

4. **ViewModel Pattern**
   - جداسازی Data Transfer از Entities

---

## مفاهیم پایه

### 1. Lead (سرنخ فروش)
- شخص یا سازمانی که هنوز مشتری نشده اما پتانسیل خرید دارد
- در مراحل مختلف قیف فروش حرکت می‌کند
- Contact یا Organization با ContactType = Lead

### 2. Customer (مشتری)
- شخص یا سازمانی که حداقل یک بار خرید انجام داده
- نیاز به پیگیری و خدمات پس از فروش دارد
- Contact یا Organization با ContactType = Customer

### 3. Interaction (تعامل)
- هر نوع ارتباط با مشتری یا سرنخ
- شامل: تماس تلفنی، جلسه، ایمیل، پیام و ...
- تعیین‌کننده مرحله فعلی در قیف فروش

### 4. Goal (هدف فروش)
- یک فرصت فروش بالقوه
- مربوط به یک محصول یا خدمت خاص
- دارای ارزش تخمینی و ارزش واقعی (بعد از تبدیل)

### 5. Referral (ارجاع)
- زمانی که یک مشتری، شخص دیگری را معرفی می‌کند
- برای تشویق و جایزه‌دهی به مشتریان وفادار

### 6. Sales Funnel (قیف فروش)
مراحل مختلفی که یک Lead طی می‌کند:
```
آگاهی → علاقه‌مندی → بررسی → مذاکره → بسته شدن
```

---

## ساختار دیتابیس

### نمودار ERD (Entity Relationship Diagram)

```
┌─────────────────┐
│    Contact      │
│  (ContactArea)  │
└────────┬────────┘
         │
         │ 1:N
         ↓
┌─────────────────┐      N:1     ┌──────────────────┐
│  Interaction    │◄──────────────┤ InteractionType  │
└────────┬────────┘               └──────────────────┘
         │                               │
         │ N:1                           │ N:1
         ↓                               ↓
┌─────────────────┐               ┌──────────────────┐
│PostPurchaseStage│               │ LeadStageStatus  │
└─────────────────┘               └──────────────────┘
         
         
┌─────────────────┐      N:N      ┌──────────────────┐
│  Interaction    │◄──────────────►│      Goal        │
└─────────────────┘               └──────────────────┘
         ↑                               ↑
         │                               │
         │                               │
         └───────────┬───────────────────┘
                     │
                 ┌───┴────┐
                 │Contact │
                 └────────┘


┌─────────────────┐               ┌──────────────────┐
│    Referral     │               │    Contact       │
│                 │──ReferrerId──►│   (Referrer)     │
│                 │               └──────────────────┘
│                 │               ┌──────────────────┐
│                 │──ReferredId──►│    Contact       │
└─────────────────┘               │   (Referred)     │
                                  └──────────────────┘
```

### جداول اصلی

| جدول | توضیحات | تعداد ستون‌ها |
|------|---------|---------------|
| `Crm_Interactions` | ذخیره تمام تعاملات | ~20 |
| `Crm_Goals` | اهداف و فرصت‌های فروش | ~15 |
| `Crm_InteractionTypes` | انواع تعامل | ~10 |
| `Crm_LeadStageStatuses` | مراحل قیف فروش | ~10 |
| `Crm_PostPurchaseStages` | مراحل بعد از خرید | ~10 |
| `Crm_Referrals` | ارجاعات و توصیه‌ها | ~12 |
| `Crm_InteractionGoals` | رابطه M:N بین Interaction و Goal | 4 |

---

## Entities (موجودیت‌ها)

### 1. Interaction (تعامل)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/Interaction.cs`

```csharp
public class Interaction : BaseEntity
{
    // ارتباط با Contact
    public int ContactId { get; set; }
    public Contact Contact { get; set; }
    
    // نوع تعامل
    public int InteractionTypeId { get; set; }
    public InteractionType InteractionType { get; set; }
    
    // جزئیات تعامل
    public string? Subject { get; set; }
    public string Description { get; set; }
    public DateTime InteractionDate { get; set; }
    public int? DurationMinutes { get; set; }
    
    // نتیجه و پیگیری
    public string? Result { get; set; }
    public string? NextAction { get; set; }
    public DateTime? NextActionDate { get; set; }
    
    // مرحله بعد از خرید (فقط برای Customer)
    public int? PostPurchaseStageId { get; set; }
    public PostPurchaseStage? PostPurchaseStage { get; set; }
    
    // اهداف مرتبط (M:N)
    public ICollection<InteractionGoal> InteractionGoals { get; set; }
    
    // ارجاعات مرتبط
    public ICollection<Referral> ReferrerReferrals { get; set; }
    public ICollection<Referral> ReferredReferrals { get; set; }
}
```

**نکات کلیدی:**
- هر Interaction حتماً به یک Contact مرتبط است
- نوع Interaction تعیین می‌کند که Lead در چه مرحله‌ای قرار دارد
- PostPurchaseStage فقط برای مشتریان (ContactType = Customer) پر می‌شود
- یک Interaction می‌تواند به چندین Goal مرتبط باشد

---

### 2. Goal (هدف فروش)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/Goal.cs`

```csharp
public class Goal : BaseEntity
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? ProductName { get; set; }
    
    // Target: Contact یا Organization
    public int? ContactId { get; set; }
    public Contact? Contact { get; set; }
    
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    
    // ارزش‌گذاری
    public decimal? EstimatedValue { get; set; }
    public decimal? ActualValue { get; set; }
    
    // وضعیت
    public int? CurrentLeadStageStatusId { get; set; }
    public LeadStageStatus? CurrentLeadStageStatus { get; set; }
    
    public bool IsConverted { get; set; }
    public DateTime? ConversionDate { get; set; }
    public bool IsActive { get; set; }
    
    // روابط
    public ICollection<InteractionGoal> InteractionGoals { get; set; }
}
```

**مفاهیم:**
- **EstimatedValue:** ارزش پیش‌بینی شده معامله
- **ActualValue:** ارزش واقعی (وقتی به خرید تبدیل می‌شود)
- **IsConverted:** آیا به خرید تبدیل شده؟
- **CurrentLeadStageStatusId:** در چه مرحله‌ای از قیف فروش است؟

---

### 3. InteractionType (نوع تعامل)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/InteractionType.cs`

```csharp
public class InteractionType : BaseEntity
{
    public string Title { get; set; }
    public string? Description { get; set; }
    
    // هر نوع تعامل به یک مرحله از قیف فروش نگاشت می‌شود
    public int LeadStageStatusId { get; set; }
    public LeadStageStatus LeadStageStatus { get; set; }
    
    // نمایش
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Interaction> Interactions { get; set; }
}
```

**مثال‌ها:**
- تماس تلفنی اولیه → مرحله "آگاهی"
- جلسه حضوری → مرحله "علاقه‌مندی"
- ارسال پیشنهاد قیمت → مرحله "مذاکره"
- امضای قرارداد → مرحله "بسته شدن موفق"

---

### 4. LeadStageStatus (مراحل قیف فروش)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/LeadStageStatus.cs`

```csharp
public class LeadStageStatus : BaseEntity
{
    public LeadStageType StageType { get; set; }
    public string Title { get; set; }
    public string? TitleEnglish { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string ColorCode { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<InteractionType> InteractionTypes { get; set; }
    public ICollection<Goal> Goals { get; set; }
}
```

**مراحل پیش‌فرض:**
1. **Awareness** (آگاهی) - زرد
2. **Interest** (علاقه‌مندی) - آبی
3. **Consideration** (بررسی) - نارنجی
4. **Intent** (قصد خرید) - بنفش
5. **Evaluation** (ارزیابی) - فیروزه‌ای
6. **Purchase** (خرید) - سبز
7. **Lost** (از دست رفته) - قرمز

---

### 5. PostPurchaseStage (مراحل بعد از خرید)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/PostPurchaseStage.cs`

```csharp
public class PostPurchaseStage : BaseEntity
{
    public PostPurchaseStageType StageType { get; set; }
    public string Title { get; set; }
    public string? TitleEnglish { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string ColorCode { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Interaction> Interactions { get; set; }
}
```

**مراحل پیش‌فرض:**
1. **Onboarding** (راه‌اندازی)
2. **Active** (استفاده فعال)
3. **Support** (پشتیبانی)
4. **Renewal** (تمدید)
5. **Upsell** (ارتقا)
6. **Churn** (ریزش)

---

### 6. Referral (ارجاع)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/Referral.cs`

```csharp
public class Referral : BaseEntity
{
    // معرف (Customer)
    public int ReferrerContactId { get; set; }
    public Contact ReferrerContact { get; set; }
    
    // معرفی شده (Lead)
    public int ReferredContactId { get; set; }
    public Contact ReferredContact { get; set; }
    
    // تعاملات مرتبط
    public int? ReferrerInteractionId { get; set; }
    public Interaction? ReferrerInteraction { get; set; }
    
    public int? ReferredInteractionId { get; set; }
    public Interaction? ReferredInteraction { get; set; }
    
    // جزئیات
    public DateTime ReferralDate { get; set; }
    public string? Notes { get; set; }
    
    // وضعیت
    public ReferralStatus Status { get; set; }
    public DateTime? StatusChangeDate { get; set; }
}
```

**وضعیت‌های Referral:**
- **Pending:** در انتظار بررسی
- **Contacted:** تماس گرفته شده
- **Successful:** تبدیل به مشتری شد
- **Failed:** تبدیل به مشتری نشد

---

### 7. InteractionGoal (رابطه Many-to-Many)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/InteractionGoal.cs`

```csharp
public class InteractionGoal
{
    public int InteractionId { get; set; }
    public Interaction Interaction { get; set; }
    
    public int GoalId { get; set; }
    public Goal Goal { get; set; }
    
    public DateTime CreatedDate { get; set; }
}
```

این entity برای ارتباط چند به چند بین Interaction و Goal است.

---

## Enums

**مسیر:** `MahERP.DataModelLayer/Enums/CrmEnums.cs`

### LeadStageType
```csharp
public enum LeadStageType
{
    Awareness = 1,      // آگاهی
    Interest = 2,       // علاقه‌مندی
    Consideration = 3,  // بررسی
    Intent = 4,         // قصد خرید
    Evaluation = 5,     // ارزیابی
    Purchase = 6,       // خرید
    Lost = 7            // از دست رفته
}
```

### PostPurchaseStageType
```csharp
public enum PostPurchaseStageType
{
    Onboarding = 1,  // راه‌اندازی
    Active = 2,      // استفاده فعال
    Support = 3,     // پشتیبانی
    Renewal = 4,     // تمدید
    Upsell = 5,      // ارتقا
    Churn = 6        // ریزش
}
```

### ReferralStatus
```csharp
public enum ReferralStatus
{
    Pending = 1,     // در انتظار
    Contacted = 2,   // تماس گرفته شده
    Successful = 3,  // موفق
    Failed = 4       // ناموفق
}
```

---

## Repositories

**مسیر:** `MahERP.DataModelLayer/Repository/CrmRepository/`

### IInteractionRepository
```csharp
public interface IInteractionRepository
{
    Task<Interaction?> GetByIdAsync(int id, bool includeRelations = true);
    Task<List<Interaction>> GetAllAsync(InteractionFilterViewModel? filters = null);
    Task<List<Interaction>> GetByContactIdAsync(int contactId);
    Task<List<Interaction>> GetByOrganizationIdAsync(int organizationId);
    Task<int> GetCountAsync(InteractionFilterViewModel? filters = null);
    Task<Interaction> CreateAsync(Interaction interaction);
    Task UpdateAsync(Interaction interaction);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

### IGoalRepository
```csharp
public interface IGoalRepository
{
    Task<Goal?> GetByIdAsync(int id, bool includeRelations = true);
    Task<List<Goal>> GetAllAsync(GoalFilterViewModel? filters = null);
    Task<List<Goal>> GetByContactIdAsync(int contactId);
    Task<List<Goal>> GetByOrganizationIdAsync(int organizationId);
    Task<Goal> CreateAsync(Goal goal);
    Task UpdateAsync(Goal goal);
    Task MarkAsConvertedAsync(int id, decimal? actualValue = null);
    Task DeactivateAsync(int id);
    Task<GoalStatisticsViewModel> GetStatisticsAsync();
}
```

### IReferralRepository
```csharp
public interface IReferralRepository
{
    Task<Referral?> GetByIdAsync(int id);
    Task<List<Referral>> GetByReferrerIdAsync(int referrerId);
    Task<List<Referral>> GetByReferredIdAsync(int referredId);
    Task<Referral> CreateAsync(Referral referral);
    Task UpdateStatusAsync(int id, ReferralStatus status);
    Task DeleteAsync(int id);
}
```

---

## ViewModels

**مسیر:** `MahERP.DataModelLayer/ViewModels/CrmViewModels/NewCrmViewModels.cs`

### InteractionViewModel
برای نمایش تعامل در View ها

### InteractionCreateViewModel
برای ایجاد تعامل جدید

### InteractionListViewModel
برای نمایش لیست تعاملات با Pagination

### GoalViewModel
برای نمایش هدف فروش

### GoalCreateViewModel
برای ایجاد هدف جدید

### ReferralViewModel
برای نمایش ارجاع

**همه ViewModels شامل:**
- Data Annotations برای Validation
- Persian Property Names
- Formatted Values برای نمایش

---

## Controllers

**مسیر:** `MahERP/Areas/CrmArea/Controllers/`

### InteractionController
- `Index()` - لیست تعاملات
- `Create()` - فرم ایجاد تعامل
- `ByContact(int contactId)` - تعاملات یک فرد
- `Details(int id)` - جزئیات تعامل
- `Delete(int id)` - حذف تعامل

### GoalController
- `Index()` - لیست اهداف
- `Create()` - فرم ایجاد هدف
- `ByContact(int contactId)` - اهداف یک فرد
- `Details(int id)` - جزئیات هدف
- `MarkAsConverted(int id)` - تبدیل به خرید

### ReferralController
- `Index()` - لیست ارجاعات
- `ByReferrer(int contactId)` - ارجاعات یک معرف
- `ByReferred(int contactId)` - معرف یک فرد
- `Details(int id)` - جزئیات ارجاع
- `MarkAsSuccessful(int id)` - علامت‌گذاری موفق
- `MarkAsFailed(int id)` - علامت‌گذاری ناموفق

---

## Views

**مسیر:** `MahERP/Areas/CrmArea/Views/`

### Shared/_CrmLayout.cshtml
- Layout مشترک برای همه صفحات CRM
- منوی ناوبری
- Styles و Scripts مشترک

### Interaction/
- `Index.cshtml` - لیست با فیلتر
- `Create.cshtml` - فرم ایجاد
- `ByContact.cshtml` - Timeline نمایش
- `Details.cshtml` - جزئیات کامل

### Goal/
- `Index.cshtml` - لیست با آمار
- `Create.cshtml` - فرم ایجاد
- `ByContact.cshtml` - کارت‌های اهداف
- `Details.cshtml` - جزئیات + Modal تبدیل

### Referral/
- `Index.cshtml` - لیست ارجاعات
- `Details.cshtml` - جزئیات ارجاع

### Stage/
- `Index.cshtml` - مدیریت مراحل

---

## فرآیندهای کاری

### 1. فرآیند تبدیل Lead به Customer

```
1. ایجاد Contact با ContactType = Lead
2. ثبت اولین Interaction (مثلاً تماس تلفنی)
   → LeadStageStatus = Awareness
3. ایجاد Goal برای این Lead
4. ثبت Interaction های بعدی
   → تغییر LeadStageStatus در هر مرحله
5. زمانی که خرید انجام شد:
   - Goal.IsConverted = true
   - Goal.ActualValue = مبلغ واقعی
   - Contact.ContactType = Customer
6. بعد از خرید:
   - ثبت Interaction با PostPurchaseStage
```

### 2. فرآیند ارجاع (Referral)

```
1. مشتری A کسی را معرفی می‌کند
2. ثبت Interaction برای مشتری A با HasReferral = true
3. ایجاد Contact جدید برای فرد معرفی شده (Lead)
4. ایجاد Referral:
   - ReferrerContactId = A
   - ReferredContactId = فرد جدید
   - Status = Pending
5. ثبت Interaction برای فرد جدید با IsReferred = true
6. پیگیری فرد جدید
7. در صورت تبدیل به مشتری:
   - Referral.Status = Successful
```

### 3. فرآیند مدیریت Goal

```
1. ایجاد Goal برای یک Contact
2. تعریف EstimatedValue
3. هر Interaction مرتبط با Goal را لینک کنیم
4. Goal به صورت خودکار CurrentLeadStageStatus را از آخرین Interaction می‌گیرد
5. زمان تبدیل:
   - MarkAsConverted()
   - ثبت ActualValue
   - ConversionDate = الان
```

---

## قابلیت‌ها و ویژگی‌ها

### ✅ قابلیت‌های پیاده‌سازی شده

1. **مدیریت تعاملات**
   - ثبت انواع مختلف تعامل
   - پیوست اهداف به تعاملات
   - نمایش Timeline
   - فیلتر و جستجو

2. **مدیریت اهداف**
   - تعریف اهداف فروش
   - پیگیری ارزش تخمینی و واقعی
   - تبدیل به خرید
   - آمار و گزارش

3. **مدیریت ارجاعات**
   - ثبت ارجاعات
   - پیگیری وضعیت
   - لیست معرفین و معرفی‌شده‌ها

4. **قیف فروش**
   - مراحل قابل تنظیم
   - رنگ‌بندی مراحل
   - انتقال خودکار بین مراحل

5. **بعد از خرید**
   - مراحل خدمات پس از فروش
   - پیگیری مشتریان

### 🔄 قابلیت‌های قابل توسعه

1. **Dashboard و گزارش‌ها**
   - نمودار قیف فروش
   - گزارش عملکرد فروش
   - پیش‌بینی فروش

2. **یادآوری‌ها**
   - یادآوری NextActionDate
   - یادآوری پیگیری

3. **Workflow Automation**
   - اتوماسیون ایمیل
   - اتوماسیون تغییر وضعیت

4. **Integration**
   - با سیستم حسابداری
   - با سیستم انبارداری

---

## Integration با سایر ماژول‌ها

### با ماژول Contact
```csharp
// تمام تعاملات به Contact ها وابسته است
public class Interaction
{
    public int ContactId { get; set; }
    public Contact Contact { get; set; }
}
```

### با ماژول Organization
```csharp
// اهداف می‌تواند برای Organization تعریف شود
public class Goal
{
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
```

### با ماژول User Management
- همه Entity ها CreatorUserId دارند
- Activity Logging
- Permission Based Access

---

## Security و Permissions

### دسترسی‌ها
```csharp
[PermissionRequired("CRM")]
public class InteractionController : BaseController
{
    // فقط کاربران با دسترسی CRM
}
```

### سطوح دسترسی پیشنهادی
1. **CRM.View** - مشاهده
2. **CRM.Create** - ایجاد
3. **CRM.Edit** - ویرایش
4. **CRM.Delete** - حذف
5. **CRM.Reports** - گزارش‌ها
6. **CRM.Admin** - مدیریت تنظیمات

---

## Best Practices

### 1. استفاده از Repository
```csharp
// ✅ درست
var interaction = await _interactionRepo.GetByIdAsync(id);

// ❌ غلط
var interaction = await _context.Interactions.FindAsync(id);
```

### 2. استفاده از ViewModel
```csharp
// ✅ درست
var viewModel = new InteractionViewModel
{
    // Map entity to viewmodel
};
return View(viewModel);

// ❌ غلط
return View(interaction); // Entity را مستقیم به View ندهید
```

### 3. Persian Date Handling
```csharp
// ✅ درست
viewModel.InteractionDatePersian = 
    _persianDateHelper.ToPersianDate(interaction.InteractionDate);

// تبدیل از Persian به Gregorian
interaction.InteractionDate = 
    _persianDateHelper.ToGregorianDate(viewModel.InteractionDatePersian);
```

### 4. Transaction Management
```csharp
// ✅ درست - استفاده از UnitOfWork
await _interactionRepo.CreateAsync(interaction);
await _referralRepo.CreateAsync(referral);
await _uow.CommitAsync(); // همه با هم commit می‌شوند

// ❌ غلط - Save کردن تک تک
await _context.SaveChangesAsync();
```

### 5. Soft Delete
```csharp
// ✅ درست
interaction.IsActive = false;
await _interactionRepo.UpdateAsync(interaction);

// به جای
await _interactionRepo.DeleteAsync(id);
```

---

## نکات مهم

### Performance
1. از `.AsNoTracking()` برای Read-Only استفاده کنید
2. Eager Loading برای روابط که نیاز دارید
3. Pagination برای لیست‌ها

### Data Integrity
1. همیشه از Transaction استفاده کنید
2. Validation در سطح Entity و ViewModel
3. Foreign Key Constraints

### User Experience
1. Persian Calendar
2. RTL Support
3. Toast Notifications
4. Confirmation Dialogs

---

## پشتیبانی و مستندات بیشتر

برای مستندات تکمیلی به فایل‌های زیر مراجعه کنید:

- [Database Schema](./CRM-Database-Schema.md)
- [API Documentation](./CRM-API-Documentation.md)
- [User Guide](./CRM-User-Guide.md)
- [Development Guide](./CRM-Development-Guide.md)

---

**نسخه:** 1.0  
**آخرین بروزرسانی:** 1403  
**وضعیت:** Release
