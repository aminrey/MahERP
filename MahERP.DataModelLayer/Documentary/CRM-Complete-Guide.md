# 📘 راهنمای کامل سیستم CRM و ارتباطات

## 🎯 مقدمه

این مستند شامل توضیح کامل سیستم CRM و نحوه ارتباط آن با **افراد (Contact)**، **سازمان‌ها (Organization)** و **افراد داخل سازمان‌ها** است.

---

## 📊 معماری کلی سیستم

```
┌─────────────────────────────────────────────────────────────────┐
│                         MahERP CRM System                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   ┌──────────────┐         ┌──────────────┐                    │
│   │   Contact    │◄────────┤ Organization │                    │
│   │   (افراد)    │  works  │  (سازمان‌ها) │                    │
│   └──────┬───────┘   at    └──────┬───────┘                    │
│          │                         │                             │
│          │                         │                             │
│   ┌──────▼────────────────────────▼────────┐                   │
│   │          Goal (اهداف فروش)            │                   │
│   └──────────────────┬────────────────────┘                   │
│                      │                                          │
│                      │ M:N via InteractionGoal                 │
│                      │                                          │
│   ┌──────────────────▼───────────────────┐                    │
│   │     Interaction (تعاملات CRM)        │                    │
│   └──────────────────────────────────────┘                    │
│          │                                                      │
│          ├──► InteractionType (نوع تعامل)                     │
│          ├──► LeadStageStatus (مرحله لید)                     │
│          └──► PostPurchaseStage (مرحله بعد از خرید)          │
│                                                                  │
│   ┌──────────────────────────────────────┐                    │
│   │     Referral (ارجاع/معرفی)          │                    │
│   └──────────────────────────────────────┘                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📌 بخش 1: ساختار پایه - افراد و سازمان‌ها

### 1.1 Contact (افراد)

**مسیر:** `MahERP.DataModelLayer/Entities/Contacts/Contact.cs`  
**جدول:** `Contact_Tbl`

#### 📝 ویژگی‌های کلیدی:

```csharp
public class Contact
{
    public int Id { get; set; }
    
    // نوع فرد در سیستم CRM
    public ContactType ContactType { get; set; } = ContactType.Lead;
    // Lead = سرنخ (هنوز خرید نکرده)
    // Customer = مشتری (خرید کرده)
    // Partner = شریک تجاری
    // Other = سایر
    
    // اطلاعات شخصی
    public string? FirstName { get; set; }
    public string LastName { get; set; }
    public string? NationalCode { get; set; }  // یکتا
    
    // اطلاعات تماس
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? PrimaryAddress { get; set; }
    
    // اطلاعات تکمیلی
    public DateTime? BirthDate { get; set; }
    public byte? Gender { get; set; }  // 0=مرد, 1=زن, 2=سایر
    public string? ProfileImagePath { get; set; }
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual ICollection<ContactPhone> Phones { get; set; }
    public virtual ICollection<OrganizationContact> OrganizationRelations { get; set; }
    public virtual ICollection<DepartmentMember> DepartmentMemberships { get; set; }
}
```

#### 🔗 ارتباطات Contact:

1. **شماره تلفن‌ها**: یک Contact می‌تواند چندین شماره تلفن داشته باشد
   - جدول: `ContactPhone_Tbl`
   - رابطه: One-to-Many

2. **ارتباط با سازمان‌ها**: یک Contact می‌تواند با چندین سازمان ارتباط داشته باشد
   - جدول: `OrganizationContact_Tbl`
   - رابطه: Many-to-Many
   - انواع: کارمند، مشتری، تامین‌کننده، شریک، مشاور

3. **عضویت در بخش‌های سازمانی**: یک Contact می‌تواند عضو بخش‌های مختلف باشد
   - جدول: `DepartmentMember_Tbl`
   - رابطه: Many-to-Many
   - شامل: سمت سازمانی، تاریخ پیوستن، نوع استخدام

4. **ارتباط با شعبه**: یک Contact می‌تواند به چندین شعبه متصل باشد
   - جدول: `BranchContact_Tbl`
   - رابطه: Many-to-Many
   - انواع: مشتری، تامین‌کننده، همکار، سایر

---

### 1.2 Organization (سازمان‌ها)

**مسیر:** `MahERP.DataModelLayer/Entities/Contacts/Organization.cs`  
**جدول:** `Organization_Tbl`

#### 📝 ویژگی‌های کلیدی:

```csharp
public class Organization
{
    public int Id { get; set; }
    
    // اطلاعات اصلی
    public string Name { get; set; }
    public string? Brand { get; set; }
    public string? RegistrationNumber { get; set; }  // شماره ثبت (یکتا)
    public string? EconomicCode { get; set; }        // کد اقتصادی (یکتا)
    public DateTime? RegistrationDate { get; set; }
    
    // اطلاعات تماس
    public string? Website { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    
    // نوع سازمان
    public byte OrganizationType { get; set; }  // 0=شرکت, 1=سازمان, 2=موسسه, 3=نهاد
    
    // اطلاعات اضافی
    public string? LegalRepresentative { get; set; }  // نماینده قانونی
    public string? LogoPath { get; set; }
    public string? Description { get; set; }
    
    // Navigation Properties
    public virtual ICollection<OrganizationDepartment> Departments { get; set; }
    public virtual ICollection<OrganizationContact> Contacts { get; set; }
    public virtual ICollection<OrganizationPhone> Phones { get; set; }
}
```

#### 🔗 ارتباطات Organization:

1. **بخش‌های سازمانی**: یک Organization دارای چارت سازمانی است
   - جدول: `OrganizationDepartment_Tbl`
   - رابطه: One-to-Many
   - ساختار: درختی (Parent-Child)

2. **افراد مرتبط**: یک Organization می‌تواند با چندین Contact ارتباط داشته باشد
   - جدول: `OrganizationContact_Tbl`
   - رابطه: Many-to-Many
   - موارد استفاده: کارمندان، مشتریان، تامین‌کنندگان، شرکا

3. **شماره تلفن‌ها**: یک Organization می‌تواند چندین شماره تلفن داشته باشد
   - جدول: `OrganizationPhone_Tbl`
   - رابطه: One-to-Many

4. **ارتباط با شعبه**: یک Organization می‌تواند به چندین شعبه متصل باشد
   - جدول: `BranchOrganization_Tbl`
   - رابطه: Many-to-Many

---

### 1.3 OrganizationContact (ارتباط فرد با سازمان)

**مسیر:** `MahERP.DataModelLayer/Entities/Contacts/OrganizationContact.cs`  
**جدول:** `OrganizationContact_Tbl`

این جدول برای ثبت ارتباط افراد با سازمان‌ها **خارج از چارت سازمانی** استفاده می‌شود.

```csharp
public class OrganizationContact
{
    public int Id { get; set; }
    
    // ارتباطات
    public int OrganizationId { get; set; }
    public int ContactId { get; set; }
    
    // نوع رابطه
    public byte RelationType { get; set; }
    // 0 = کارمند
    // 1 = مشتری (نماینده مشتری)
    // 2 = تامین‌کننده
    // 3 = شریک
    // 4 = مشاور
    
    // اطلاعات تکمیلی
    public string? JobTitle { get; set; }        // عنوان شغلی
    public string? Department { get; set; }      // نام بخش (بدون چارت)
    public bool IsPrimary { get; set; }          // تماس اصلی؟
    public bool IsDecisionMaker { get; set; }    // تصمیم‌گیرنده؟
    public byte ImportanceLevel { get; set; }    // 0-3 (پایین تا خیلی بالا)
    
    // تاریخ‌ها
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public string? Notes { get; set; }
}
```

#### 💡 مثال کاربرد:
- **شرکت ABC** (Organization) دارای یک **نماینده فروش** به نام **علی احمدی** (Contact) است
- این ارتباط در `OrganizationContact_Tbl` ثبت می‌شود
- `RelationType = 1` (مشتری/نماینده)
- `IsPrimary = true` (تماس اصلی)
- `IsDecisionMaker = true` (تصمیم‌گیرنده)

---

### 1.4 DepartmentMember (عضویت در بخش سازمانی)

**مسیر:** `MahERP.DataModelLayer/Entities/Contacts/DepartmentMember.cs`  
**جدول:** `DepartmentMember_Tbl`

این جدول برای ثبت افراد **داخل چارت سازمانی** استفاده می‌شود.

```csharp
public class DepartmentMember
{
    public int Id { get; set; }
    
    // ارتباطات
    public int DepartmentId { get; set; }           // بخش سازمانی
    public int ContactId { get; set; }              // فرد
    public int? PositionId { get; set; }            // سمت سازمانی
    
    // اطلاعات استخدام
    public DateTime JoinDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public byte EmploymentType { get; set; }
    // 0 = تمام‌وقت
    // 1 = پاره‌وقت
    // 2 = قراردادی
    // 3 = پروژه‌ای
    
    public bool IsSupervisor { get; set; }          // ناظر است؟
    public string? Notes { get; set; }
}
```

#### 💡 مثال کاربرد:
- **شرکت XYZ** (Organization) دارای بخش **فروش** (OrganizationDepartment) است
- **رضا محمدی** (Contact) عضو این بخش است
- سمت: **مدیر فروش** (DepartmentPosition)
- این اطلاعات در `DepartmentMember_Tbl` ثبت می‌شود

---

## 📌 بخش 2: سیستم CRM

### 2.1 Goal (اهداف فروش)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/Goal.cs`  
**جدول:** `Goal_Tbl`

هر **Contact** یا **Organization** می‌تواند چندین **هدف فروش** داشته باشد.

```csharp
public class Goal
{
    public int Id { get; set; }
    
    // اطلاعات هدف
    public string Title { get; set; }               // عنوان هدف
    public string? Description { get; set; }
    public string? ProductName { get; set; }        // محصول/خدمت
    
    // ارتباط با Contact یا Organization
    public int? ContactId { get; set; }             // یکی از این دو باید مقدار داشته باشد
    public int? OrganizationId { get; set; }
    
    // وضعیت هدف در قیف فروش
    public int? CurrentLeadStageStatusId { get; set; }  // مرحله فعلی
    
    // تبدیل به مشتری
    public bool IsConverted { get; set; }           // آیا به خرید منجر شده؟
    public DateTime? ConversionDate { get; set; }   // تاریخ تبدیل
    
    // ارزش مالی
    public decimal? EstimatedValue { get; set; }    // ارزش تخمینی (ریال)
    public decimal? ActualValue { get; set; }       // ارزش واقعی پس از خرید (ریال)
    
    // Navigation
    public virtual ICollection<InteractionGoal> InteractionGoals { get; set; }
}
```

#### 💡 مثال:
- **Contact**: علی احمدی
- **Goal 1**: خرید سایت فروشگاهی - EstimatedValue: 50,000,000 ریال
- **Goal 2**: خرید اپلیکیشن موبایل - EstimatedValue: 80,000,000 ریال

---

### 2.2 Interaction (تعاملات)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/Interaction.cs`  
**جدول:** `Interaction_Tbl`

هر تماس، جلسه، ایمیل، پیام و... با مشتری یک **Interaction** است.

```csharp
public class Interaction
{
    public int Id { get; set; }
    
    // فرد مرتبط (الزامی)
    public int ContactId { get; set; }
    
    // سازمان مرتبط (اختیاری)
    public int? OrganizationId { get; set; }
    
    // نوع تعامل (تماس تلفنی، جلسه، ایمیل، ...)
    public int InteractionTypeId { get; set; }
    
    // وضعیت بعد از خرید (فقط برای مشتریان)
    public int? PostPurchaseStageId { get; set; }
    
    // جزئیات تعامل
    public string? Subject { get; set; }
    public string Description { get; set; }
    public DateTime InteractionDate { get; set; }
    public int? DurationMinutes { get; set; }
    
    // نتیجه و اقدام بعدی
    public string? Result { get; set; }
    public string? NextAction { get; set; }
    public DateTime? NextActionDate { get; set; }
    
    // ارجاع/معرفی
    public bool HasReferral { get; set; }           // آیا شامل معرفی است؟
    public bool IsReferred { get; set; }            // آیا این فرد معرفی شده؟
    
    // Navigation
    public virtual ICollection<InteractionGoal> InteractionGoals { get; set; }
}
```

#### 🔗 ارتباط Interaction با Goal (M:N):

یک تعامل می‌تواند مربوط به **چند هدف** باشد و یک هدف می‌تواند **چند تعامل** داشته باشد.

```csharp
// جدول واسط: InteractionGoal_Tbl
public class InteractionGoal
{
    public int Id { get; set; }
    public int InteractionId { get; set; }
    public int GoalId { get; set; }
    
    public virtual Interaction Interaction { get; set; }
    public virtual Goal Goal { get; set; }
}
```

#### 💡 مثال کامل:

```
🧑 Contact: علی احمدی (فرد)
🏢 Organization: شرکت ABC (سازمان)
📋 OrganizationContact: علی احمدی نماینده شرکت ABC است

🎯 Goal: خرید سایت فروشگاهی برای شرکت ABC
   - ContactId: علی احمدی
   - OrganizationId: شرکت ABC
   - EstimatedValue: 50,000,000 ریال

📞 Interaction #1: تماس تلفنی با علی احمدی
   - ContactId: علی احمدی
   - OrganizationId: شرکت ABC
   - InteractionTypeId: تماس تلفنی
   - Description: معرفی خدمات و مشاوره اولیه
   - InteractionGoals: [Goal: خرید سایت]

🤝 Interaction #2: جلسه حضوری
   - ContactId: علی احمدی
   - OrganizationId: شرکت ABC
   - InteractionTypeId: جلسه حضوری
   - Description: ارائه دمو و بررسی نیازها
   - InteractionGoals: [Goal: خرید سایت]
```

---

### 2.3 InteractionType (نوع تعامل)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/InteractionType.cs`  
**جدول:** `InteractionType_Tbl`

هر نوع تعامل به یک **مرحله در قیف فروش** (LeadStageStatus) وصل است.

```csharp
public class InteractionType
{
    public int Id { get; set; }
    public string Title { get; set; }               // عنوان (تماس تلفنی، جلسه، ...)
    public string? Description { get; set; }
    
    // هر تعامل منجر به یک مرحله خاص می‌شود
    public int LeadStageStatusId { get; set; }
    
    // ظاهر UI
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }          // #28a745
    public string? Icon { get; set; }               // fa-phone
}
```

#### 💡 مثال انواع تعامل:

| نوع تعامل         | مرحله قیف فروش    | رنگ       |
|-------------------|-------------------|-----------|
| تماس اولیه        | آگاهی             | #17a2b8   |
| ارسال پیشنهاد     | علاقه‌مندی        | #ffc107   |
| جلسه دمو          | ارزیابی           | #fd7e14   |
| مذاکره قرارداد    | تصمیم‌گیری        | #6610f2   |
| امضای قرارداد     | خرید              | #28a745   |

---

### 2.4 LeadStageStatus (مراحل قیف فروش)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/LeadStageStatus.cs`  
**جدول:** `LeadStageStatus_Tbl`

این جدول **استاتیک** است و توسط Seed Data پر می‌شود.

```csharp
public class LeadStageStatus
{
    public int Id { get; set; }
    public LeadStageType StageType { get; set; }    // Enum
    public string Title { get; set; }               // آگاهی، علاقه‌مندی، ...
    public string? TitleEnglish { get; set; }
    public int DisplayOrder { get; set; }
    public string ColorCode { get; set; }
    public string? Icon { get; set; }
}
```

#### 📊 مراحل قیف فروش (Sales Funnel):

```
┌─────────────────────────────────────────────┐
│ 1. Awareness (آگاهی)                       │ ← تماس اولیه
├─────────────────────────────────────────────┤
│ 2. Interest (علاقه‌مندی)                   │ ← ارسال پیشنهاد
├─────────────────────────────────────────────┤
│ 3. Consideration (بررسی)                   │ ← بررسی پیشنهاد
├─────────────────────────────────────────────┤
│ 4. Intent (قصد خرید)                       │ ← درخواست جلسه
├─────────────────────────────────────────────┤
│ 5. Evaluation (ارزیابی)                    │ ← جلسه دمو
├─────────────────────────────────────────────┤
│ 6. Purchase (خرید) ✅                       │ ← امضای قرارداد
├─────────────────────────────────────────────┤
│ 7. Lost (از دست رفته) ❌                   │ ← عدم موفقیت
└─────────────────────────────────────────────┘
```

---

### 2.5 PostPurchaseStage (مراحل بعد از خرید)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/PostPurchaseStage.cs`  
**جدول:** `PostPurchaseStage_Tbl`

این جدول برای مدیریت **مشتریان بعد از خرید** استفاده می‌شود.

```csharp
public class PostPurchaseStage
{
    public int Id { get; set; }
    public PostPurchaseStageType StageType { get; set; }    // Enum
    public string Title { get; set; }
    public int DisplayOrder { get; set; }
    public string ColorCode { get; set; }
    public string? Icon { get; set; }
}
```

#### 📊 مراحل بعد از خرید:

| مرحله                  | هدف                                    |
|------------------------|----------------------------------------|
| Onboarding (راه‌اندازی)| شروع به کار با محصول/خدمت              |
| Active (فعال)          | استفاده عادی از سرویس                  |
| Support (پشتیبانی)     | نیاز به پشتیبانی                        |
| Renewal (تمدید)        | نزدیک به اتمام قرارداد                  |
| Upsell (ارتقا)         | فروش محصولات/خدمات بیشتر                |
| Churn (ریزش)           | قطع همکاری                             |

---

### 2.6 Referral (ارجاع/معرفی)

**مسیر:** `MahERP.DataModelLayer/Entities/Crm/Referral.cs`  
**جدول:** `Referral_Tbl`

```csharp
public class Referral
{
    public int Id { get; set; }
    
    // طرفین
    public int ReferrerContactId { get; set; }      // معرف (باید Customer باشد)
    public int ReferredContactId { get; set; }      // معرفی‌شده (معمولاً Lead جدید)
    
    // تعاملات مرتبط
    public int? ReferrerInteractionId { get; set; }     // تعامل با معرف
    public int? ReferredInteractionId { get; set; }     // اولین تعامل با معرفی‌شده
    
    // اطلاعات
    public DateTime ReferralDate { get; set; }
    public string? Notes { get; set; }
    public ReferralStatus Status { get; set; }      // Pending, Contacted, Successful, Failed
    public DateTime? StatusChangeDate { get; set; }
    
    // آینده (بازاریابی)
    public byte ReferralType { get; set; }          // 0=مشتری, 1=بازاریاب
    public string? MarketerUserId { get; set; }
}
```

#### 💡 مثال:
- **مشتری فعلی**: علی احمدی (ContactType = Customer)
- **معرفی شده**: حسین رضایی (ContactType = Lead)
- **Referral** ثبت می‌شود: علی احمدی → حسین رضایی
- **Status**: Pending (در انتظار تماس)

---

## 📌 بخش 3: سناریوهای کاربردی

### سناریو 1: ثبت لید جدید از طریق تماس تلفنی

```csharp
// 1. ایجاد Contact
var contact = new Contact
{
    FirstName = "محمد",
    LastName = "احمدی",
    ContactType = ContactType.Lead,
    PrimaryEmail = "mohammad@example.com"
};
await _contactRepo.CreateAsync(contact);

// 2. ایجاد شماره تلفن
var phone = new ContactPhone
{
    ContactId = contact.Id,
    Number = "09121234567",
    Type = 0,  // موبایل
    IsDefault = true
};
await _context.ContactPhone_Tbl.AddAsync(phone);

// 3. اتصال به شعبه
var branchContact = new BranchContact
{
    BranchId = 1,
    ContactId = contact.Id,
    RelationType = 0,  // مشتری
    AssignedByUserId = userId
};
await _context.BranchContact_Tbl.AddAsync(branchContact);

// 4. ایجاد هدف فروش
var goal = new Goal
{
    ContactId = contact.Id,
    Title = "خرید سایت فروشگاهی",
    ProductName = "سایت فروشگاهی پیشرفته",
    EstimatedValue = 50000000,
    CurrentLeadStageStatusId = 1  // آگاهی
};
await _goalRepo.CreateAsync(goal);

// 5. ثبت اولین تعامل
var interaction = new Interaction
{
    ContactId = contact.Id,
    InteractionTypeId = 1,  // تماس تلفنی
    Subject = "تماس اولیه - معرفی خدمات",
    Description = "مشتری از طریق تبلیغات گوگل تماس گرفت",
    InteractionDate = DateTime.Now,
    DurationMinutes = 15,
    Result = "مشتری علاقه‌مند به دریافت پیشنهاد",
    NextAction = "ارسال پیشنهاد قیمت",
    NextActionDate = DateTime.Now.AddDays(2),
    CreatorUserId = userId
};

await _interactionRepo.CreateAsync(interaction, goalIds: new List<int> { goal.Id });
```

---

### سناریو 2: ثبت تعامل با نماینده سازمان

```csharp
// 1. سازمان از قبل وجود دارد
var organization = await _organizationRepo.GetByIdAsync(5);

// 2. نماینده سازمان
var contact = await _contactRepo.GetByIdAsync(10);

// 3. بررسی ارتباط Contact با Organization
var orgContact = await _context.OrganizationContact_Tbl
    .FirstOrDefaultAsync(oc => 
        oc.OrganizationId == organization.Id && 
        oc.ContactId == contact.Id);

if (orgContact == null)
{
    // ایجاد ارتباط
    orgContact = new OrganizationContact
    {
        OrganizationId = organization.Id,
        ContactId = contact.Id,
        RelationType = 1,  // نماینده مشتری
        JobTitle = "مدیر خرید",
        IsPrimary = true,
        IsDecisionMaker = true,
        ImportanceLevel = 3  // خیلی بالا
    };
    await _context.OrganizationContact_Tbl.AddAsync(orgContact);
}

// 4. ایجاد هدف برای سازمان
var goal = new Goal
{
    OrganizationId = organization.Id,
    ContactId = contact.Id,  // نماینده
    Title = "خرید سیستم CRM",
    EstimatedValue = 200000000,
    CurrentLeadStageStatusId = 3  // ارزیابی
};
await _goalRepo.CreateAsync(goal);

// 5. ثبت تعامل
var interaction = new Interaction
{
    ContactId = contact.Id,
    OrganizationId = organization.Id,  // ⭐ تعامل با سازمان
    InteractionTypeId = 5,  // جلسه حضوری
    Subject = "جلسه دمو محصول",
    Description = "ارائه دمو سیستم CRM در دفتر مشتری",
    InteractionDate = DateTime.Now,
    DurationMinutes = 90,
    Result = "مشتری راضی بود و درخواست پیشنهاد قیمت داد",
    NextAction = "ارسال پیشنهاد نهایی",
    NextActionDate = DateTime.Now.AddDays(3)
};

await _interactionRepo.CreateAsync(interaction, goalIds: new List<int> { goal.Id });
```

---

### سناریو 3: تبدیل لید به مشتری

```csharp
// 1. دریافت هدف
var goal = await _goalRepo.GetByIdAsync(15);

// 2. علامت‌گذاری هدف به عنوان تبدیل شده
await _goalRepo.MarkAsConvertedAsync(goal.Id, actualValue: 50000000);

// 3. ثبت تعامل خرید
var interaction = new Interaction
{
    ContactId = goal.ContactId.Value,
    OrganizationId = goal.OrganizationId,
    InteractionTypeId = 10,  // امضای قرارداد (LeadStageStatus = Purchase)
    Subject = "امضای قرارداد خرید",
    Description = "قرارداد خرید سایت فروشگاهی امضا شد",
    InteractionDate = DateTime.Now,
    Result = "پروژه به تیم فنی تحویل داده شد"
};

await _interactionRepo.CreateAsync(interaction, goalIds: new List<int> { goal.Id });

// 4. ContactType به صورت خودکار تغییر می‌کند
// در InteractionRepository.UpdateContactTypeIfPurchaseAsync()
// Contact.ContactType = ContactType.Customer
```

---

### سناریو 4: مدیریت معرفی مشتری

```csharp
// 1. مشتری فعلی (معرف)
var referrer = await _contactRepo.GetByIdAsync(20);  // ContactType = Customer

// 2. لید جدید (معرفی‌شده)
var referred = new Contact
{
    FirstName = "حسین",
    LastName = "رضایی",
    ContactType = ContactType.Lead,
    PrimaryEmail = "hossein@example.com"
};
await _contactRepo.CreateAsync(referred);

// 3. ثبت ارجاع
var referral = new Referral
{
    ReferrerContactId = referrer.Id,
    ReferredContactId = referred.Id,
    ReferralDate = DateTime.Now,
    Status = ReferralStatus.Pending,
    Notes = "مشتری در تعامل تلفنی این فرد را معرفی کرد"
};
await _referralRepo.CreateAsync(referral);

// 4. ثبت تعامل با معرفی‌شده
var interaction = new Interaction
{
    ContactId = referred.Id,
    IsReferred = true,  // ⭐ این فرد معرفی شده است
    InteractionTypeId = 1,  // تماس تلفنی
    Subject = "تماس با لید معرفی‌شده",
    Description = $"لید توسط {referrer.FullName} معرفی شده است"
};

var createdInteraction = await _interactionRepo.CreateAsync(interaction, goalIds: null);

// 5. بروزرسانی Referral
referral.ReferredInteractionId = createdInteraction.Id;
referral.Status = ReferralStatus.Contacted;
await _referralRepo.UpdateStatusAsync(referral.Id, ReferralStatus.Contacted);
```

---

## 📌 بخش 4: Query های کاربردی

### 4.1 دریافت تمام تعاملات یک Contact

```csharp
var interactions = await _context.Interaction_Tbl
    .Include(i => i.Contact)
    .Include(i => i.Organization)
    .Include(i => i.InteractionType)
        .ThenInclude(t => t.LeadStageStatus)
    .Include(i => i.PostPurchaseStage)
    .Include(i => i.InteractionGoals)
        .ThenInclude(ig => ig.Goal)
    .Where(i => i.ContactId == contactId && i.IsActive)
    .OrderByDescending(i => i.InteractionDate)
    .ToListAsync();
```

---

### 4.2 دریافت تمام اهداف یک Organization

```csharp
var goals = await _context.Goal_Tbl
    .Include(g => g.Contact)
    .Include(g => g.Organization)
    .Include(g => g.CurrentLeadStageStatus)
    .Include(g => g.InteractionGoals)
        .ThenInclude(ig => ig.Interaction)
    .Where(g => g.OrganizationId == organizationId && g.IsActive)
    .OrderByDescending(g => g.CreatedDate)
    .ToListAsync();
```

---

### 4.3 دریافت افراد داخل یک سازمان (چارت سازمانی)

```csharp
var members = await _context.DepartmentMember_Tbl
    .Include(dm => dm.Contact)
    .Include(dm => dm.Department)
        .ThenInclude(d => d.Organization)
    .Include(dm => dm.Position)
    .Where(dm => 
        dm.Department.OrganizationId == organizationId && 
        dm.IsActive)
    .OrderBy(dm => dm.Department.DisplayOrder)
        .ThenBy(dm => dm.Contact.LastName)
    .ToListAsync();
```

---

### 4.4 دریافت نمایندگان اصلی یک سازمان

```csharp
var primaryContacts = await _context.OrganizationContact_Tbl
    .Include(oc => oc.Contact)
    .Include(oc => oc.Organization)
    .Where(oc => 
        oc.OrganizationId == organizationId && 
        oc.IsPrimary && 
        oc.IsActive)
    .OrderByDescending(oc => oc.ImportanceLevel)
    .ToListAsync();
```

---

### 4.5 دریافت تعاملات یک Organization (تمام نمایندگان)

```csharp
// روش 1: از طریق ContactId های مرتبط
var contactIds = await _context.OrganizationContact_Tbl
    .Where(oc => oc.OrganizationId == organizationId && oc.IsActive)
    .Select(oc => oc.ContactId)
    .ToListAsync();

var interactions = await _context.Interaction_Tbl
    .Include(i => i.Contact)
    .Include(i => i.Organization)
    .Include(i => i.InteractionType)
    .Where(i => 
        contactIds.Contains(i.ContactId) && 
        i.IsActive)
    .OrderByDescending(i => i.InteractionDate)
    .ToListAsync();

// روش 2: فقط تعاملاتی که OrganizationId دارند
var organizationInteractions = await _context.Interaction_Tbl
    .Include(i => i.Contact)
    .Include(i => i.Organization)
    .Include(i => i.InteractionType)
    .Where(i => 
        i.OrganizationId == organizationId && 
        i.IsActive)
    .OrderByDescending(i => i.InteractionDate)
    .ToListAsync();
```

---

## 📌 بخش 5: نکات مهم

### ✅ تفاوت OrganizationContact و DepartmentMember

| ویژگی                 | OrganizationContact                    | DepartmentMember                        |
|----------------------|----------------------------------------|-----------------------------------------|
| **کاربرد**            | ارتباطات خارج از چارت                  | چارت سازمانی رسمی                       |
| **مثال**              | مشتری، تامین‌کننده، شریک              | کارمندان داخلی سازمان                  |
| **سمت**               | JobTitle (رشته متنی)                   | Position (Foreign Key)                  |
| **بخش**               | Department (رشته متنی یا null)         | DepartmentId (Foreign Key الزامی)      |
| **ساختار**            | مسطح (Flat)                            | سلسله‌مراتبی (Hierarchical)            |

---

### ✅ چرخه حیات یک Lead

```
1. Contact ایجاد می‌شود (ContactType = Lead)
2. Goal برای Contact ایجاد می‌شود
3. Interaction ها ثبت می‌شوند
4. Goal.CurrentLeadStageStatus بروزرسانی می‌شود
5. وقتی InteractionType با LeadStageStatus=Purchase ثبت شود:
   - Goal.IsConverted = true
   - Contact.ContactType = Customer
6. از این به بعد Interaction ها با PostPurchaseStage ثبت می‌شوند
```

---

### ✅ ارتباط Interaction با Organization

```csharp
// Interaction همیشه یک ContactId دارد (الزامی)
// اما OrganizationId اختیاری است

// حالت 1: تعامل با فرد مستقل
ContactId = 10
OrganizationId = null

// حالت 2: تعامل با نماینده سازمان
ContactId = 10
OrganizationId = 5

// حالت 3: ❌ غیرمجاز
ContactId = null
OrganizationId = 5
```

---

## 📊 خلاصه روابط

```
Contact (افراد)
├── ContactPhone (1:N)
├── BranchContact (N:M via junction)
├── OrganizationContact (N:M via junction) → Organization
├── DepartmentMember (N:M via junction) → OrganizationDepartment
├── Goal (1:N)
├── Interaction (1:N)
└── Referral (1:N as Referrer or Referred)

Organization (سازمان‌ها)
├── OrganizationPhone (1:N)
├── BranchOrganization (N:M via junction)
├── OrganizationDepartment (1:N)
│   └── DepartmentMember (N:M) → Contact
├── OrganizationContact (N:M) → Contact
├── Goal (1:N)
└── Interaction (1:N via OrganizationId)

Goal (اهداف)
├── Contact (N:1 - optional)
├── Organization (N:1 - optional)
├── LeadStageStatus (N:1)
└── InteractionGoal (N:M) → Interaction

Interaction (تعاملات)
├── Contact (N:1 - required)
├── Organization (N:1 - optional)
├── InteractionType (N:1)
├── PostPurchaseStage (N:1 - optional)
└── InteractionGoal (N:M) → Goal

Referral (ارجاع)
├── ReferrerContact (N:1)
├── ReferredContact (N:1)
├── ReferrerInteraction (N:1 - optional)
└── ReferredInteraction (N:1 - optional)
```

---

**نسخه مستند:** 1.0.0  
**تاریخ:** آذر 1403  
**وضعیت:** ✅ کامل و به‌روز
