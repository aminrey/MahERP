# مرجع سریع فنی - CRM Module

## 🎯 Quick Reference for Developers

---

## Entities Quick Reference

### Interaction
```csharp
// Path: MahERP.DataModelLayer/Entities/Crm/Interaction.cs
// Table: Crm_Interactions

Key Properties:
- ContactId (FK → Contacts)
- InteractionTypeId (FK → Crm_InteractionTypes)
- PostPurchaseStageId (FK → Crm_PostPurchaseStages) - Nullable
- InteractionDate
- NextActionDate - Nullable
```

### Goal
```csharp
// Path: MahERP.DataModelLayer/Entities/Crm/Goal.cs
// Table: Crm_Goals

Key Properties:
- ContactId (FK → Contacts) - Nullable
- OrganizationId (FK → Organizations) - Nullable
- CurrentLeadStageStatusId (FK → Crm_LeadStageStatuses)
- EstimatedValue, ActualValue
- IsConverted, ConversionDate
```

### Referral
```csharp
// Path: MahERP.DataModelLayer/Entities/Crm/Referral.cs
// Table: Crm_Referrals

Key Properties:
- ReferrerContactId (معرف - FK → Contacts)
- ReferredContactId (معرفی‌شده - FK → Contacts)
- Status (Enum: ReferralStatus)
```

---

## Enums Quick Reference

```csharp
// Path: MahERP.DataModelLayer/Enums/CrmEnums.cs

public enum LeadStageType
{
    Awareness = 1,       // آگاهی
    Interest = 2,        // علاقه‌مندی
    Consideration = 3,   // بررسی
    Intent = 4,          // قصد خرید
    Evaluation = 5,      // ارزیابی
    Purchase = 6,        // خرید
    Lost = 7             // از دست رفته
}

public enum PostPurchaseStageType
{
    Onboarding = 1,  // راه‌اندازی
    Active = 2,      // فعال
    Support = 3,     // پشتیبانی
    Renewal = 4,     // تمدید
    Upsell = 5,      // ارتقا
    Churn = 6        // ریزش
}

public enum ReferralStatus
{
    Pending = 1,     // در انتظار
    Contacted = 2,   // تماس گرفته شده
    Successful = 3,  // موفق
    Failed = 4       // ناموفق
}
```

---

## Repository Methods Quick Reference

### IInteractionRepository

```csharp
// Get
Task<Interaction?> GetByIdAsync(int id, bool includeRelations = true)
Task<List<Interaction>> GetAllAsync(InteractionFilterViewModel? filters = null)
Task<List<Interaction>> GetByContactIdAsync(int contactId)
Task<int> GetCountAsync(InteractionFilterViewModel? filters = null)

// CUD
Task<Interaction> CreateAsync(Interaction interaction)
Task UpdateAsync(Interaction interaction)
Task DeleteAsync(int id) // Soft Delete
```

### IGoalRepository

```csharp
// Get
Task<Goal?> GetByIdAsync(int id, bool includeRelations = true)
Task<List<Goal>> GetAllAsync(GoalFilterViewModel? filters = null)
Task<List<Goal>> GetByContactIdAsync(int contactId)

// CUD
Task<Goal> CreateAsync(Goal goal)
Task UpdateAsync(Goal goal)
Task MarkAsConvertedAsync(int id, decimal? actualValue = null)
Task DeactivateAsync(int id)

// Stats
Task<GoalStatisticsViewModel> GetStatisticsAsync()
```

### IReferralRepository

```csharp
// Get
Task<Referral?> GetByIdAsync(int id)
Task<List<Referral>> GetByReferrerIdAsync(int referrerId)
Task<List<Referral>> GetByReferredIdAsync(int referredId)

// CUD
Task<Referral> CreateAsync(Referral referral)
Task UpdateStatusAsync(int id, ReferralStatus status)
Task DeleteAsync(int id)
```

---

## Controller Actions Quick Reference

### InteractionController

```csharp
// Path: MahERP/Areas/CrmArea/Controllers/InteractionController.cs

[HttpGet]
Index(int page, int pageSize, InteractionFilterViewModel? filters)

[HttpGet]
Create(int? contactId)

[HttpPost]
Create(InteractionCreateViewModel viewModel)

[HttpGet]
Details(int id)

[HttpGet]
ByContact(int contactId)

[HttpPost]
Delete(int id, int? contactId)
```

### GoalController

```csharp
// Path: MahERP/Areas/CrmArea/Controllers/GoalController.cs

[HttpGet]
Index(GoalFilterViewModel? filters)

[HttpGet]
Create(int? contactId)

[HttpPost]
Create(GoalCreateViewModel viewModel)

[HttpGet]
Details(int id)

[HttpGet]
ByContact(int contactId)

[HttpPost]
MarkAsConverted(int id, decimal? actualValue)
```

### ReferralController

```csharp
// Path: MahERP/Areas/CrmArea/Controllers/ReferralController.cs

[HttpGet]
Index(ReferralFilterViewModel? filters)

[HttpGet]
Details(int id)

[HttpGet]
ByReferrer(int contactId)

[HttpGet]
ByReferred(int contactId)

[HttpPost]
MarkAsSuccessful(int id)

[HttpPost]
MarkAsFailed(int id)
```

---

## Database Tables Quick Reference

```sql
-- Lead Stage Statuses (مراحل قیف فروش)
Crm_LeadStageStatuses
    Id, StageType, Title, ColorCode, DisplayOrder, IsActive

-- Post-Purchase Stages (مراحل بعد از خرید)
Crm_PostPurchaseStages
    Id, StageType, Title, ColorCode, DisplayOrder, IsActive

-- Interaction Types (انواع تعامل)
Crm_InteractionTypes
    Id, Title, LeadStageStatusId, DisplayOrder, ColorCode, Icon, IsActive

-- Interactions (تعاملات)
Crm_Interactions
    Id, ContactId, InteractionTypeId, PostPurchaseStageId, 
    Subject, Description, InteractionDate, DurationMinutes,
    Result, NextAction, NextActionDate, IsActive

-- Goals (اهداف فروش)
Crm_Goals
    Id, Title, ContactId, OrganizationId, ProductName,
    CurrentLeadStageStatusId, EstimatedValue, ActualValue,
    IsConverted, ConversionDate, IsActive

-- Referrals (ارجاعات)
Crm_Referrals
    Id, ReferrerContactId, ReferredContactId,
    ReferrerInteractionId, ReferredInteractionId,
    ReferralDate, Status, StatusChangeDate

-- Interaction-Goal (Many-to-Many)
Crm_InteractionGoals
    InteractionId, GoalId, CreatedDate
```

---

## Common Code Patterns

### Creating an Interaction with Goals and Referral

```csharp
public async Task<IActionResult> Create(InteractionCreateViewModel viewModel)
{
    if (!ModelState.IsValid)
        return View(viewModel);

    try
    {
        // 1. Create Interaction
        var interaction = new Interaction
        {
            ContactId = viewModel.ContactId,
            InteractionTypeId = viewModel.InteractionTypeId,
            PostPurchaseStageId = viewModel.PostPurchaseStageId,
            Subject = viewModel.Subject,
            Description = viewModel.Description,
            InteractionDate = _persianDateHelper.ToGregorianDate(
                viewModel.InteractionDatePersian) ?? DateTime.Now,
            DurationMinutes = viewModel.DurationMinutes,
            Result = viewModel.Result,
            NextAction = viewModel.NextAction,
            NextActionDate = string.IsNullOrEmpty(viewModel.NextActionDatePersian)
                ? null
                : _persianDateHelper.ToGregorianDate(viewModel.NextActionDatePersian),
            CreatorUserId = GetCurrentUserId(),
            CreatedDate = DateTime.Now,
            IsActive = true
        };
        
        await _interactionRepo.CreateAsync(interaction);
        
        // 2. Link Goals (if any)
        if (viewModel.GoalIds?.Any() == true)
        {
            foreach (var goalId in viewModel.GoalIds)
            {
                var interactionGoal = new InteractionGoal
                {
                    InteractionId = interaction.Id,
                    GoalId = goalId,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = GetCurrentUserId()
                };
                await _context.InteractionGoals.AddAsync(interactionGoal);
            }
        }
        
        // 3. Create Referral (if applicable)
        if (viewModel.HasReferral && viewModel.ReferredContactId.HasValue)
        {
            var referral = new Referral
            {
                ReferrerContactId = viewModel.ContactId,
                ReferredContactId = viewModel.ReferredContactId.Value,
                ReferrerInteractionId = interaction.Id,
                ReferralDate = interaction.InteractionDate,
                Status = ReferralStatus.Pending,
                CreatorUserId = GetCurrentUserId(),
                CreatedDate = DateTime.Now
            };
            await _referralRepo.CreateAsync(referral);
        }
        
        if (viewModel.IsReferred && viewModel.ReferrerContactId.HasValue)
        {
            var referral = new Referral
            {
                ReferrerContactId = viewModel.ReferrerContactId.Value,
                ReferredContactId = viewModel.ContactId,
                ReferredInteractionId = interaction.Id,
                ReferralDate = interaction.InteractionDate,
                Status = ReferralStatus.Contacted,
                CreatorUserId = GetCurrentUserId(),
                CreatedDate = DateTime.Now
            };
            await _referralRepo.CreateAsync(referral);
        }
        
        // 4. Commit
        await _uow.CommitAsync();
        
        // 5. Log Activity
        await _activityLogger.LogActivityAsync(
            "CRM", "Interaction", "Create",
            $"ثبت تعامل جدید با {await GetContactNameAsync(viewModel.ContactId)}"
        );
        
        TempData["SuccessMessage"] = "تعامل با موفقیت ثبت شد";
        return RedirectToAction(nameof(Details), new { id = interaction.Id });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating interaction");
        ModelState.AddModelError("", "خطا در ثبت اطلاعات");
        return View(viewModel);
    }
}
```

### Filtering with IQueryable

```csharp
public async Task<List<Interaction>> GetAllAsync(InteractionFilterViewModel? filters)
{
    IQueryable<Interaction> query = _context.Interactions
        .Include(i => i.Contact)
        .Include(i => i.InteractionType)
            .ThenInclude(it => it.LeadStageStatus)
        .Include(i => i.PostPurchaseStage)
        .Where(i => i.IsActive);
    
    if (filters != null)
    {
        // Contact Filter
        if (filters.ContactId.HasValue)
        {
            query = query.Where(i => i.ContactId == filters.ContactId.Value);
        }
        
        // Interaction Type Filter
        if (filters.InteractionTypeId.HasValue)
        {
            query = query.Where(i => i.InteractionTypeId == filters.InteractionTypeId.Value);
        }
        
        // Search Term
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var searchTerm = filters.SearchTerm.Trim();
            query = query.Where(i =>
                i.Subject.Contains(searchTerm) ||
                i.Description.Contains(searchTerm)
            );
        }
        
        // Date Range
        if (filters.FromDate.HasValue)
        {
            query = query.Where(i => i.InteractionDate >= filters.FromDate.Value);
        }
        
        if (filters.ToDate.HasValue)
        {
            var toDateEnd = filters.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(i => i.InteractionDate <= toDateEnd);
        }
        
        // Has Next Action
        if (filters.HasNextAction == true)
        {
            query = query.Where(i => i.NextActionDate.HasValue);
        }
        
        // Lead Stage
        if (filters.LeadStageStatusId.HasValue)
        {
            query = query.Where(i => 
                i.InteractionType.LeadStageStatusId == filters.LeadStageStatusId.Value
            );
        }
        
        // Post-Purchase Stage
        if (filters.PostPurchaseStageId.HasValue)
        {
            query = query.Where(i => 
                i.PostPurchaseStageId == filters.PostPurchaseStageId.Value
            );
        }
    }
    
    return await query
        .OrderByDescending(i => i.InteractionDate)
        .ToListAsync();
}
```

### Converting Goal to Purchase

```csharp
public async Task<IActionResult> MarkAsConverted(int id, decimal? actualValue)
{
    try
    {
        var goal = await _goalRepo.GetByIdAsync(id, includeRelations: false);
        
        if (goal == null)
        {
            TempData["ErrorMessage"] = "هدف مورد نظر یافت نشد";
            return RedirectToAction(nameof(Index));
        }
        
        if (goal.IsConverted)
        {
            TempData["InfoMessage"] = "این هدف قبلاً تبدیل شده است";
            return RedirectToAction(nameof(Details), new { id });
        }
        
        // Mark as converted
        goal.IsConverted = true;
        goal.ConversionDate = DateTime.Now;
        goal.ActualValue = actualValue ?? goal.EstimatedValue ?? 0;
        
        await _goalRepo.UpdateAsync(goal);
        
        // Convert Contact to Customer (if it was a Lead)
        if (goal.ContactId.HasValue)
        {
            var contact = await _contactRepo.GetByIdAsync(goal.ContactId.Value);
            if (contact != null && contact.ContactType == ContactType.Lead)
            {
                contact.ContactType = ContactType.Customer;
                await _contactRepo.UpdateAsync(contact);
            }
        }
        
        await _uow.CommitAsync();
        
        await _activityLogger.LogActivityAsync(
            "CRM", "Goal", "Convert",
            $"تبدیل هدف '{goal.Title}' به خرید با مبلغ {goal.ActualValue:N0} ریال"
        );
        
        TempData["SuccessMessage"] = "هدف با موفقیت به خرید تبدیل شد";
        return RedirectToAction(nameof(Details), new { id });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error converting goal {GoalId}", id);
        TempData["ErrorMessage"] = "خطا در تبدیل هدف";
        return RedirectToAction(nameof(Details), new { id });
    }
}
```

---

## Configuration & Seed Data

### Entity Configuration

```csharp
// Path: MahERP.DataModelLayer/Configurations/CrmEntitiesConfiguration.cs

public void Configure(EntityTypeBuilder<Interaction> builder)
{
    builder.ToTable("Crm_Interactions");
    
    builder.HasKey(i => i.Id);
    
    // Foreign Keys
    builder.HasOne(i => i.Contact)
        .WithMany()
        .HasForeignKey(i => i.ContactId)
        .OnDelete(DeleteBehavior.Restrict);
    
    builder.HasOne(i => i.InteractionType)
        .WithMany(it => it.Interactions)
        .HasForeignKey(i => i.InteractionTypeId)
        .OnDelete(DeleteBehavior.Restrict);
    
    builder.HasOne(i => i.PostPurchaseStage)
        .WithMany(ps => ps.Interactions)
        .HasForeignKey(i => i.PostPurchaseStageId)
        .OnDelete(DeleteBehavior.SetNull);
    
    // Properties
    builder.Property(i => i.Subject)
        .HasMaxLength(300);
    
    builder.Property(i => i.Description)
        .IsRequired();
    
    builder.Property(i => i.InteractionDate)
        .IsRequired()
        .HasDefaultValueSql("GETDATE()");
    
    // Indexes
    builder.HasIndex(i => i.ContactId);
    builder.HasIndex(i => i.InteractionTypeId);
    builder.HasIndex(i => i.InteractionDate);
    builder.HasIndex(i => i.NextActionDate);
}
```

### Seed Data

```csharp
// Path: MahERP.DataModelLayer/StaticClasses/CrmSeedData.cs

public static async Task SeedCrmDataAsync(AppDbContext context, int systemUserId)
{
    // Seed Lead Stage Statuses
    if (!await context.LeadStageStatuses.AnyAsync())
    {
        var stages = new[]
        {
            new LeadStageStatus { StageType = LeadStageType.Awareness, Title = "آگاهی", 
                TitleEnglish = "Awareness", ColorCode = "#FFC107", DisplayOrder = 1, /*...*/ },
            new LeadStageStatus { StageType = LeadStageType.Interest, Title = "علاقه‌مندی", 
                TitleEnglish = "Interest", ColorCode = "#2196F3", DisplayOrder = 2, /*...*/ },
            // ... more stages
        };
        
        await context.LeadStageStatuses.AddRangeAsync(stages);
        await context.SaveChangesAsync();
    }
    
    // Seed Post-Purchase Stages
    if (!await context.PostPurchaseStages.AnyAsync())
    {
        var stages = new[]
        {
            new PostPurchaseStage { StageType = PostPurchaseStageType.Onboarding, 
                Title = "راه‌اندازی", ColorCode = "#17A2B8", DisplayOrder = 1, /*...*/ },
            // ... more stages
        };
        
        await context.PostPurchaseStages.AddRangeAsync(stages);
        await context.SaveChangesAsync();
    }
    
    // Seed Interaction Types
    if (!await context.InteractionTypes.AnyAsync())
    {
        var types = new[]
        {
            new InteractionType { Title = "تماس تلفنی اولیه", 
                LeadStageStatusId = 1, DisplayOrder = 1, /*...*/ },
            // ... more types
        };
        
        await context.InteractionTypes.AddRangeAsync(types);
        await context.SaveChangesAsync();
    }
}
```

---

## DI Registration

```csharp
// Path: MahERP/Program.cs

// Register CRM Repositories
builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IReferralRepository, ReferralRepository>();
builder.Services.AddScoped<IInteractionTypeRepository, InteractionTypeRepository>();
builder.Services.AddScoped<IStageRepository, StageRepository>();
```

---

## Useful SQL Queries

### Get Interactions with Full Details
```sql
SELECT 
    i.Id,
    i.InteractionDate,
    c.FirstName + ' ' + c.LastName AS ContactName,
    it.Title AS InteractionType,
    lss.Title AS LeadStage,
    pps.Title AS PostPurchaseStage,
    i.Subject,
    i.NextActionDate
FROM Crm_Interactions i
INNER JOIN Contacts c ON i.ContactId = c.Id
INNER JOIN Crm_InteractionTypes it ON i.InteractionTypeId = it.Id
INNER JOIN Crm_LeadStageStatuses lss ON it.LeadStageStatusId = lss.Id
LEFT JOIN Crm_PostPurchaseStages pps ON i.PostPurchaseStageId = pps.Id
WHERE i.IsActive = 1
ORDER BY i.InteractionDate DESC;
```

### Goal Conversion Rate by Stage
```sql
SELECT 
    lss.Title AS Stage,
    COUNT(*) AS TotalGoals,
    SUM(CASE WHEN g.IsConverted = 1 THEN 1 ELSE 0 END) AS ConvertedGoals,
    CAST(SUM(CASE WHEN g.IsConverted = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS ConversionRate
FROM Crm_Goals g
LEFT JOIN Crm_LeadStageStatuses lss ON g.CurrentLeadStageStatusId = lss.Id
WHERE g.IsActive = 1
GROUP BY lss.Title, lss.DisplayOrder
ORDER BY lss.DisplayOrder;
```

### Referral Success Rate by Contact
```sql
SELECT 
    c.FirstName + ' ' + c.LastName AS ReferrerName,
    COUNT(*) AS TotalReferrals,
    SUM(CASE WHEN r.Status = 3 THEN 1 ELSE 0 END) AS SuccessfulReferrals,
    CAST(SUM(CASE WHEN r.Status = 3 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS SuccessRate
FROM Crm_Referrals r
INNER JOIN Contacts c ON r.ReferrerContactId = c.Id
GROUP BY c.FirstName, c.LastName
HAVING COUNT(*) >= 2
ORDER BY SuccessfulReferrals DESC;
```

---

## Frontend Components

### Select2 for Contact Search
```javascript
$("#contactSelect").select2({
    placeholder: "جستجوی فرد...",
    ajax: {
        url: '@Url.Action("SearchContacts", "CrmAjax")',
        dataType: 'json',
        delay: 250,
        data: function (params) {
            return { term: params.term };
        },
        processResults: function (data) {
            return {
                results: data.results.map(item => ({
                    id: item.id,
                    text: item.fullName + ' (' + item.typeName + ')'
                }))
            };
        }
    },
    minimumInputLength: 2,
    language: 'fa'
});
```

### Persian DatePicker
```javascript
$(".date-picker").persianDatepicker({
    format: 'YYYY/MM/DD',
    autoClose: true,
    initialValue: true,
    calendar: {
        persian: {
            locale: 'fa'
        }
    },
    navigator: {
        enabled: true
    },
    toolbox: {
        calendarSwitch: {
            enabled: false
        }
    }
});
```

### Confirmation with SweetAlert2
```javascript
function confirmDelete(formId) {
    Swal.fire({
        title: 'آیا مطمئن هستید؟',
        text: "این عملیات قابل بازگشت نیست!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'بله، حذف شود',
        cancelButtonText: 'انصراف'
    }).then((result) => {
        if (result.isConfirmed) {
            document.getElementById(formId).submit();
        }
    });
}
```

---

## Testing Checklist

### Unit Tests
- [ ] Repository methods return correct data
- [ ] Filters work correctly
- [ ] Soft delete sets IsActive = false
- [ ] Create methods set required fields

### Integration Tests
- [ ] Controller actions return correct views
- [ ] POST actions validate ModelState
- [ ] Redirects work correctly
- [ ] TempData messages are set

### End-to-End Tests
- [ ] Create interaction flow
- [ ] Create goal and link to interaction
- [ ] Convert goal to purchase
- [ ] Create referral
- [ ] Mark referral as successful

---

## Performance Tips

1. **Use AsNoTracking for read-only queries**
```csharp
var interactions = await _context.Interactions
    .AsNoTracking()
    .ToListAsync();
```

2. **Project only needed fields**
```csharp
var summary = await _context.Goals
    .Select(g => new { g.Id, g.Title, g.EstimatedValue })
    .ToListAsync();
```

3. **Use pagination**
```csharp
var page = await _context.Interactions
    .OrderByDescending(i => i.InteractionDate)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

4. **Avoid N+1 queries with Include**
```csharp
var interactions = await _context.Interactions
    .Include(i => i.Contact)
    .Include(i => i.InteractionType)
    .ToListAsync();
```

---

## Troubleshooting

### Common Issues

**Issue:** Null reference when accessing navigation properties
```csharp
// ❌ Wrong
var interaction = await _context.Interactions.FindAsync(id);
var contactName = interaction.Contact.FullName; // NullReferenceException

// ✅ Correct
var interaction = await _context.Interactions
    .Include(i => i.Contact)
    .FirstOrDefaultAsync(i => i.Id == id);
var contactName = interaction?.Contact?.FullName ?? "Unknown";
```

**Issue:** Persian date not parsing
```csharp
// ✅ Always validate Persian dates
var gregorianDate = _persianDateHelper.ToGregorianDate(persianDate);
if (gregorianDate == null)
{
    ModelState.AddModelError("DateField", "تاریخ نامعتبر است");
    return View();
}
```

**Issue:** Circular reference in JSON
```csharp
// ✅ Use ViewModels, not Entities
public IActionResult GetInteraction(int id)
{
    var interaction = await _repo.GetByIdAsync(id);
    var viewModel = interaction.ToViewModel(); // Map to ViewModel
    return Json(viewModel);
}
```

---

**نسخه:** 1.0  
**آخرین بروزرسانی:** 1403  
**این مرجع برای دسترسی سریع به اطلاعات فنی طراحی شده است.**
