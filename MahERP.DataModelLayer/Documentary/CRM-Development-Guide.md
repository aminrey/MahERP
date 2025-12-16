# راهنمای توسعه‌دهنده - CRM Module

## نسخه: 1.0
## مخاطب: توسعه‌دهندگان Backend و Frontend

---

## فهرست

1. [شروع کار](#شروع-کار)
2. [ساختار پروژه](#ساختار-پروژه)
3. [راهنمای کد نویسی](#راهنمای-کد-نویسی)
4. [Repository Pattern](#repository-pattern)
5. [ViewModel Pattern](#viewmodel-pattern)
6. [Controller Development](#controller-development)
7. [View Development](#view-development)
8. [Testing](#testing)
9. [Debugging](#debugging)
10. [Common Scenarios](#common-scenarios)

---

## شروع کار

### پیش‌نیازها
```bash
- .NET 9 SDK
- SQL Server 2019+
- Visual Studio 2022 / VS Code
- Git
```

### نصب و راه‌اندازی

1. **Clone کردن پروژه:**
```bash
git clone https://github.com/aminrey/MahERP.git
cd MahERP
```

2. **Restore Dependencies:**
```bash
dotnet restore
```

3. **تنظیم Connection String:**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MahERP;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

4. **اجرای Migrations:**
```bash
dotnet ef database update --project MahERP.DataModelLayer --startup-project MahERP
```

5. **اجرای برنامه:**
```bash
cd MahERP
dotnet run
```

---

## ساختار پروژه

```
MahERP/
├── MahERP/                          # پروژه اصلی Web
│   ├── Areas/
│   │   └── CrmArea/
│   │       ├── Controllers/         # کنترلرهای CRM
│   │       │   ├── InteractionController.cs
│   │       │   ├── GoalController.cs
│   │       │   ├── ReferralController.cs
│   │       │   └── StageController.cs
│   │       └── Views/               # View های CRM
│   │           ├── Interaction/
│   │           ├── Goal/
│   │           ├── Referral/
│   │           ├── Stage/
│   │           └── Shared/
│   │               └── _CrmLayout.cshtml
│   └── Program.cs                   # تنظیمات و DI
│
├── MahERP.DataModelLayer/           # پروژه Data Layer
│   ├── Entities/
│   │   └── Crm/                     # Entity های CRM
│   │       ├── Interaction.cs
│   │       ├── Goal.cs
│   │       ├── InteractionType.cs
│   │       ├── LeadStageStatus.cs
│   │       ├── PostPurchaseStage.cs
│   │       ├── Referral.cs
│   │       └── InteractionGoal.cs
│   ├── Repository/
│   │   └── CrmRepository/           # Repository ها
│   │       ├── INewCrmRepositories.cs
│   │       ├── InteractionRepository.cs
│   │       ├── GoalRepository.cs
│   │       ├── ReferralRepository.cs
│   │       └── StageRepositories.cs
│   ├── ViewModels/
│   │   └── CrmViewModels/           # ViewModel ها
│   │       └── NewCrmViewModels.cs
│   ├── Configurations/
│   │   └── CrmEntitiesConfiguration.cs
│   ├── Enums/
│   │   └── CrmEnums.cs
│   ├── StaticClasses/
│   │   └── CrmSeedData.cs
│   └── AppDbContext.cs
│
└── MahERP.DataModelLayer/Documentary/   # مستندات
    ├── CRM-System-Overview.md
    ├── CRM-Database-Schema.md
    └── CRM-Development-Guide.md (این فایل)
```

---

## راهنمای کد نویسی

### Naming Conventions

#### C# Classes و Methods
```csharp
// ✅ درست
public class InteractionController : BaseController
{
    public async Task<IActionResult> GetByContactIdAsync(int contactId)
    {
        // ...
    }
}

// ❌ غلط
public class interactioncontroller
{
    public async Task<IActionResult> get_by_contact_id(int contact_id)
    {
        // ...
    }
}
```

#### Variables
```csharp
// ✅ درست
private readonly IInteractionRepository _interactionRepo;
var totalCount = await _interactionRepo.GetCountAsync();

// ❌ غلط
private readonly IInteractionRepository interactionRepo;
var TotalCount = await interactionRepo.GetCountAsync();
```

#### Database Objects
```csharp
// ✅ درست - جداول
Crm_Interactions
Crm_Goals
Crm_InteractionTypes

// ✅ درست - ستون‌ها
ContactId
InteractionDate
IsActive
CreatedDate

// ❌ غلط
crm_interactions
contact_id
isActive
```

### Code Style

#### استفاده از var
```csharp
// ✅ زمانی که نوع واضح است
var interaction = new Interaction();
var interactions = await _interactionRepo.GetAllAsync();

// ❌ زمانی که نوع مشخص نیست
var result = ProcessData(); // چه نوعی؟

// ✅ بهتر است
InteractionViewModel result = ProcessData();
```

#### Async/Await
```csharp
// ✅ درست - همه متدهای دیتابیس Async
public async Task<IActionResult> Index()
{
    var interactions = await _interactionRepo.GetAllAsync();
    return View(interactions);
}

// ❌ غلط - Blocking
public IActionResult Index()
{
    var interactions = _interactionRepo.GetAllAsync().Result; // ❌
    return View(interactions);
}
```

#### Null Handling
```csharp
// ✅ درست - استفاده از Null-conditional operator
var contact = await _contactRepo.GetByIdAsync(id);
if (contact == null)
{
    return NotFound();
}
var name = contact.FirstName ?? "نامشخص";

// ✅ استفاده از Pattern Matching
if (contact is null)
{
    return NotFound();
}

// ✅ استفاده از Null-coalescing assignment
interaction.Subject ??= "بدون موضوع";
```

---

## Repository Pattern

### ساختار Repository

```csharp
// Interface
public interface IInteractionRepository
{
    Task<Interaction?> GetByIdAsync(int id, bool includeRelations = true);
    Task<List<Interaction>> GetAllAsync(InteractionFilterViewModel? filters = null);
    Task<Interaction> CreateAsync(Interaction interaction);
    Task UpdateAsync(Interaction interaction);
    Task DeleteAsync(int id);
}

// Implementation
public class InteractionRepository : IInteractionRepository
{
    private readonly AppDbContext _context;
    
    public InteractionRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Interaction?> GetByIdAsync(int id, bool includeRelations = true)
    {
        IQueryable<Interaction> query = _context.Interactions;
        
        if (includeRelations)
        {
            query = query
                .Include(i => i.Contact)
                .Include(i => i.InteractionType)
                    .ThenInclude(it => it.LeadStageStatus)
                .Include(i => i.PostPurchaseStage)
                .Include(i => i.InteractionGoals)
                    .ThenInclude(ig => ig.Goal);
        }
        
        return await query.FirstOrDefaultAsync(i => i.Id == id);
    }
    
    // ... سایر متدها
}
```

### استفاده از Repository در Controller

```csharp
public class InteractionController : BaseController
{
    private readonly IInteractionRepository _interactionRepo;
    private readonly IUnitOfWork _uow;
    
    public InteractionController(
        IInteractionRepository interactionRepo,
        IUnitOfWork uow)
    {
        _interactionRepo = interactionRepo;
        _uow = uow;
    }
    
    public async Task<IActionResult> Details(int id)
    {
        // ✅ استفاده از Repository
        var interaction = await _interactionRepo.GetByIdAsync(id);
        
        if (interaction == null)
        {
            return NotFound();
        }
        
        // Map to ViewModel
        var viewModel = MapToViewModel(interaction);
        
        return View(viewModel);
    }
}
```

### Best Practices

#### 1. همیشه از IQueryable برای Filtering استفاده کنید

```csharp
// ✅ درست - Filtering در دیتابیس
public async Task<List<Interaction>> GetAllAsync(InteractionFilterViewModel? filters)
{
    IQueryable<Interaction> query = _context.Interactions
        .Include(i => i.Contact)
        .Include(i => i.InteractionType);
    
    if (filters != null)
    {
        if (filters.ContactId.HasValue)
        {
            query = query.Where(i => i.ContactId == filters.ContactId.Value);
        }
        
        if (!string.IsNullOrEmpty(filters.SearchTerm))
        {
            query = query.Where(i => 
                i.Subject.Contains(filters.SearchTerm) ||
                i.Description.Contains(filters.SearchTerm));
        }
        
        if (filters.FromDate.HasValue)
        {
            query = query.Where(i => i.InteractionDate >= filters.FromDate.Value);
        }
    }
    
    return await query
        .OrderByDescending(i => i.InteractionDate)
        .ToListAsync();
}

// ❌ غلط - Filtering در Memory
public async Task<List<Interaction>> GetAllAsync(InteractionFilterViewModel? filters)
{
    var allInteractions = await _context.Interactions.ToListAsync(); // همه را می‌آورد!
    
    if (filters?.ContactId.HasValue == true)
    {
        allInteractions = allInteractions
            .Where(i => i.ContactId == filters.ContactId.Value)
            .ToList();
    }
    
    return allInteractions;
}
```

#### 2. استفاده از AsNoTracking برای Read-Only

```csharp
// ✅ برای لیست‌ها و گزارش‌ها
public async Task<List<Interaction>> GetAllAsync()
{
    return await _context.Interactions
        .AsNoTracking() // بهبود Performance
        .Include(i => i.Contact)
        .ToListAsync();
}

// ✅ برای Edit کردن
public async Task<Interaction?> GetByIdForEditAsync(int id)
{
    return await _context.Interactions // بدون AsNoTracking
        .Include(i => i.InteractionGoals)
        .FirstOrDefaultAsync(i => i.Id == id);
}
```

#### 3. Eager Loading vs Lazy Loading

```csharp
// ✅ Eager Loading - برای روابط که همیشه نیاز است
var interaction = await _context.Interactions
    .Include(i => i.Contact)           // همیشه نیاز است
    .Include(i => i.InteractionType)   // همیشه نیاز است
    .FirstOrDefaultAsync(i => i.Id == id);

// ✅ Explicit Loading - برای روابط اختیاری
var interaction = await _context.Interactions
    .FirstOrDefaultAsync(i => i.Id == id);

if (needGoals)
{
    await _context.Entry(interaction)
        .Collection(i => i.InteractionGoals)
        .LoadAsync();
}
```

---

## ViewModel Pattern

### ساختار ViewModel

```csharp
/// <summary>
/// ViewModel برای نمایش تعامل
/// </summary>
public class InteractionViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "فرد")]
    public int ContactId { get; set; }
    public string? ContactName { get; set; }
    
    [Required(ErrorMessage = "نوع تعامل الزامی است")]
    [Display(Name = "نوع تعامل")]
    public int InteractionTypeId { get; set; }
    public string? InteractionTypeName { get; set; }
    
    [MaxLength(300)]
    [Display(Name = "موضوع")]
    public string? Subject { get; set; }
    
    [Required(ErrorMessage = "شرح تعامل الزامی است")]
    [Display(Name = "شرح تعامل")]
    public string Description { get; set; } = string.Empty;
    
    [Display(Name = "تاریخ")]
    public DateTime InteractionDate { get; set; }
    public string? InteractionDatePersian { get; set; } // برای نمایش
    
    // ... سایر Property ها
    
    // برای Dropdown ها
    public List<InteractionTypeViewModel> InteractionTypes { get; set; } = new();
}
```

### Mapping Entity به ViewModel

```csharp
// ✅ استفاده از Extension Method
public static class InteractionExtensions
{
    public static InteractionViewModel ToViewModel(
        this Interaction interaction, 
        PersianDateHelper persianDateHelper)
    {
        return new InteractionViewModel
        {
            Id = interaction.Id,
            ContactId = interaction.ContactId,
            ContactName = interaction.Contact?.GetFullName(),
            InteractionTypeId = interaction.InteractionTypeId,
            InteractionTypeName = interaction.InteractionType?.Title,
            Subject = interaction.Subject,
            Description = interaction.Description,
            InteractionDate = interaction.InteractionDate,
            InteractionDatePersian = persianDateHelper.ToPersianDate(interaction.InteractionDate),
            // ... سایر mapping ها
        };
    }
    
    public static Interaction ToEntity(this InteractionViewModel viewModel)
    {
        return new Interaction
        {
            Id = viewModel.Id,
            ContactId = viewModel.ContactId,
            InteractionTypeId = viewModel.InteractionTypeId,
            Subject = viewModel.Subject,
            Description = viewModel.Description,
            // ... سایر mapping ها
        };
    }
}

// استفاده در Controller
var viewModel = interaction.ToViewModel(_persianDateHelper);
```

### Validation

```csharp
// در ViewModel
public class InteractionCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "انتخاب فرد الزامی است")]
    public int ContactId { get; set; }
    
    [Required(ErrorMessage = "نوع تعامل الزامی است")]
    public int InteractionTypeId { get; set; }
    
    [MaxLength(300, ErrorMessage = "موضوع نباید بیش از 300 کاراکتر باشد")]
    public string? Subject { get; set; }
    
    // Custom Validation
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HasReferral && ReferredContactId == null)
        {
            yield return new ValidationResult(
                "در صورت معرفی، باید فرد معرفی‌شده را انتخاب کنید",
                new[] { nameof(ReferredContactId) }
            );
        }
        
        if (IsReferred && ReferrerContactId == null)
        {
            yield return new ValidationResult(
                "معرف را انتخاب کنید",
                new[] { nameof(ReferrerContactId) }
            );
        }
    }
}
```

---

## Controller Development

### ساختار پایه Controller

```csharp
[Area("CrmArea")]
[Authorize]
[PermissionRequired("CRM")]
public class InteractionController : BaseController
{
    private readonly IInteractionRepository _interactionRepo;
    private readonly IContactRepository _contactRepo;
    private readonly IUnitOfWork _uow;
    private readonly PersianDateHelper _persianDateHelper;
    private readonly ActivityLoggerService _activityLogger;
    
    public InteractionController(
        IInteractionRepository interactionRepo,
        IContactRepository contactRepo,
        IUnitOfWork uow,
        UserManager<AppUsers> userManager,
        PersianDateHelper persianDateHelper,
        IMemoryCache memoryCache,
        ActivityLoggerService activityLogger,
        IUserManagerRepository userRepository,
        IBaseRepository baseRepository,
        IModuleTrackingService moduleTracking,
        IModuleAccessService moduleAccessService)
        : base(uow, userManager, persianDateHelper, memoryCache, 
               activityLogger, userRepository, baseRepository, 
               moduleTracking, moduleAccessService)
    {
        _interactionRepo = interactionRepo;
        _contactRepo = contactRepo;
        _uow = uow;
        _persianDateHelper = persianDateHelper;
        _activityLogger = activityLogger;
    }
    
    // ... Actions
}
```

### Action Patterns

#### 1. Index (لیست)
```csharp
[HttpGet]
public async Task<IActionResult> Index(
    int page = 1,
    int pageSize = 20,
    InteractionFilterViewModel? filters = null)
{
    try
    {
        // دریافت داده‌ها
        var interactions = await _interactionRepo.GetAllAsync(filters);
        var totalCount = await _interactionRepo.GetCountAsync(filters);
        
        // ایجاد ViewModel
        var viewModel = new InteractionListViewModel
        {
            Interactions = interactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => i.ToViewModel(_persianDateHelper))
                .ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            Filters = filters
        };
        
        return View(viewModel);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطا در دریافت لیست تعاملات");
        TempData["ErrorMessage"] = "خطا در دریافت اطلاعات";
        return View(new InteractionListViewModel());
    }
}
```

#### 2. Create (GET)
```csharp
[HttpGet]
public async Task<IActionResult> Create(int? contactId)
{
    try
    {
        var viewModel = new InteractionCreateViewModel
        {
            ContactId = contactId ?? 0,
            InteractionTypes = await GetInteractionTypesAsync(),
            PostPurchaseStages = await GetPostPurchaseStagesAsync()
        };
        
        if (contactId.HasValue)
        {
            var contact = await _contactRepo.GetByIdAsync(contactId.Value);
            if (contact != null)
            {
                ViewBag.ContactName = contact.GetFullName();
            }
        }
        
        return View(viewModel);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطا در نمایش فرم ایجاد تعامل");
        return RedirectToAction(nameof(Index));
    }
}
```

#### 3. Create (POST)
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(InteractionCreateViewModel viewModel)
{
    if (!ModelState.IsValid)
    {
        // پر کردن لیست‌های Dropdown
        viewModel.InteractionTypes = await GetInteractionTypesAsync();
        viewModel.PostPurchaseStages = await GetPostPurchaseStagesAsync();
        return View(viewModel);
    }
    
    try
    {
        // تبدیل ViewModel به Entity
        var interaction = new Interaction
        {
            ContactId = viewModel.ContactId,
            InteractionTypeId = viewModel.InteractionTypeId,
            PostPurchaseStageId = viewModel.PostPurchaseStageId,
            Subject = viewModel.Subject,
            Description = viewModel.Description,
            InteractionDate = _persianDateHelper.ToGregorianDate(
                viewModel.InteractionDatePersian) 
                ?? DateTime.Now,
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
        
        // ذخیره Interaction
        await _interactionRepo.CreateAsync(interaction);
        
        // لینک کردن Goals
        if (viewModel.GoalIds?.Any() == true)
        {
            await LinkGoalsToInteractionAsync(interaction.Id, viewModel.GoalIds);
        }
        
        // بررسی Referral
        if (viewModel.HasReferral && viewModel.ReferredContactId.HasValue)
        {
            await CreateReferralAsync(
                viewModel.ContactId,
                viewModel.ReferredContactId.Value,
                interaction.Id
            );
        }
        
        if (viewModel.IsReferred && viewModel.ReferrerContactId.HasValue)
        {
            await CreateReferralAsync(
                viewModel.ReferrerContactId.Value,
                viewModel.ContactId,
                null,
                interaction.Id
            );
        }
        
        // Commit تراکنش
        await _uow.CommitAsync();
        
        // ثبت Activity Log
        await _activityLogger.LogActivityAsync(
            "CRM",
            "Interaction",
            "Create",
            $"ثبت تعامل جدید با {await GetContactNameAsync(viewModel.ContactId)}"
        );
        
        TempData["SuccessMessage"] = "تعامل با موفقیت ثبت شد";
        return RedirectToAction(nameof(Details), new { id = interaction.Id });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطا در ثبت تعامل");
        ModelState.AddModelError("", "خطا در ثبت اطلاعات. لطفاً دوباره تلاش کنید.");
        
        viewModel.InteractionTypes = await GetInteractionTypesAsync();
        viewModel.PostPurchaseStages = await GetPostPurchaseStagesAsync();
        return View(viewModel);
    }
}
```

#### 4. Details
```csharp
[HttpGet]
public async Task<IActionResult> Details(int id)
{
    try
    {
        var interaction = await _interactionRepo.GetByIdAsync(id);
        
        if (interaction == null)
        {
            TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد";
            return RedirectToAction(nameof(Index));
        }
        
        var viewModel = interaction.ToViewModel(_persianDateHelper);
        
        return View(viewModel);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطا در نمایش جزئیات تعامل {InteractionId}", id);
        TempData["ErrorMessage"] = "خطا در نمایش اطلاعات";
        return RedirectToAction(nameof(Index));
    }
}
```

#### 5. Delete
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id, int? contactId)
{
    try
    {
        var interaction = await _interactionRepo.GetByIdAsync(id, includeRelations: false);
        
        if (interaction == null)
        {
            TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد";
            return RedirectToAction(nameof(Index));
        }
        
        // Soft Delete
        interaction.IsActive = false;
        await _interactionRepo.UpdateAsync(interaction);
        await _uow.CommitAsync();
        
        // Activity Log
        await _activityLogger.LogActivityAsync(
            "CRM",
            "Interaction",
            "Delete",
            $"حذف تعامل شماره {id}"
        );
        
        TempData["SuccessMessage"] = "تعامل با موفقیت حذف شد";
        
        if (contactId.HasValue)
        {
            return RedirectToAction(nameof(ByContact), new { contactId });
        }
        
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطا در حذف تعامل {InteractionId}", id);
        TempData["ErrorMessage"] = "خطا در حذف تعامل";
        return RedirectToAction(nameof(Index));
    }
}
```

### Helper Methods

```csharp
private async Task<List<InteractionTypeViewModel>> GetInteractionTypesAsync()
{
    var types = await _interactionTypeRepo.GetAllActiveAsync();
    return types.Select(t => new InteractionTypeViewModel
    {
        Id = t.Id,
        Title = t.Title,
        LeadStageStatusTitle = t.LeadStageStatus?.Title
    }).ToList();
}

private int GetCurrentUserId()
{
    return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}

private async Task<string> GetContactNameAsync(int contactId)
{
    var contact = await _contactRepo.GetByIdAsync(contactId);
    return contact?.GetFullName() ?? "نامشخص";
}
```

---

## View Development

### Layout Usage

```cshtml
@{
    ViewData["Title"] = "لیست تعاملات";
    Layout = "~/Areas/CrmArea/Views/Shared/_CrmLayout.cshtml";
}
```

### Form Patterns

#### Select2 برای جستجوی Contact
```cshtml
<div class="mb-3">
    <label asp-for="ContactId" class="form-label">فرد <span class="text-danger">*</span></label>
    <select asp-for="ContactId" class="form-select" id="contactSelect" required>
        <option value="">-- انتخاب کنید --</option>
    </select>
    <span asp-validation-for="ContactId" class="text-danger"></span>
</div>

@section Scripts {
    <script>
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
                    return { results: data.results };
                }
            },
            minimumInputLength: 2
        });
    </script>
}
```

#### Persian DatePicker
```cshtml
<div class="mb-3">
    <label asp-for="InteractionDatePersian" class="form-label">
        تاریخ <span class="text-danger">*</span>
    </label>
    <input asp-for="InteractionDatePersian" class="form-control date-picker" required />
    <span asp-validation-for="InteractionDatePersian" class="text-danger"></span>
</div>

@section Scripts {
    <script>
        $(".date-picker").persianDatepicker({
            format: 'YYYY/MM/DD',
            autoClose: true,
            initialValue: true,
            minDate: new persianDate().subtract('year', 1),
            maxDate: new persianDate().add('year', 1)
        });
    </script>
}
```

#### Confirmation Dialog
```cshtml
<form asp-action="Delete" asp-route-id="@item.Id" method="post" 
      onsubmit="return confirm('آیا از حذف این تعامل مطمئن هستید؟');">
    @Html.AntiForgeryToken()
    <button type="submit" class="btn btn-outline-danger btn-sm">
        <i class="fa fa-trash"></i>
    </button>
</form>
```

---

## Testing

### Unit Testing

```csharp
public class InteractionRepositoryTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new AppDbContext(options);
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsInteraction_WhenExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new InteractionRepository(context);
        
        var interaction = new Interaction
        {
            ContactId = 1,
            InteractionTypeId = 1,
            Description = "Test",
            CreatorUserId = 1,
            CreatedDate = DateTime.Now
        };
        
        context.Interactions.Add(interaction);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repo.GetByIdAsync(interaction.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Description);
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new InteractionRepository(context);
        
        // Act
        var result = await repo.GetByIdAsync(999);
        
        // Assert
        Assert.Null(result);
    }
}
```

---

## Debugging

### Log Queries
```csharp
// در appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### SQL Profiler
```sql
-- فیلتر برای CRM queries
SELECT * FROM sys.dm_exec_requests
WHERE command LIKE '%Crm_%'
```

---

## Common Scenarios

### Scenario 1: اضافه کردن فیلتر جدید

```csharp
// 1. اضافه کردن به ViewModel
public class InteractionFilterViewModel
{
    // ... فیلترهای موجود
    public int? NewFilterId { get; set; }
}

// 2. اضافه کردن به Repository
public async Task<List<Interaction>> GetAllAsync(InteractionFilterViewModel? filters)
{
    IQueryable<Interaction> query = _context.Interactions;
    
    if (filters?.NewFilterId.HasValue == true)
    {
        query = query.Where(i => i.NewFilterId == filters.NewFilterId.Value);
    }
    
    return await query.ToListAsync();
}

// 3. اضافه کردن به View
<div class="col-md-3">
    <label class="form-label">فیلتر جدید</label>
    <select name="Filters.NewFilterId" class="form-select">
        <option value="">همه</option>
        @* Options *@
    </select>
</div>
```

### Scenario 2: اضافه کردن ستون جدید به Entity

```csharp
// 1. اضافه کردن به Entity
public class Interaction
{
    // ... properties موجود
    public string? NewProperty { get; set; }
}

// 2. اضافه کردن Migration
dotnet ef migrations add AddNewPropertyToInteraction

// 3. اعمال Migration
dotnet ef database update

// 4. اضافه کردن به ViewModel
public class InteractionViewModel
{
    public string? NewProperty { get; set; }
}

// 5. اضافه کردن به View
<div class="mb-3">
    <label asp-for="NewProperty" class="form-label">عنوان جدید</label>
    <input asp-for="NewProperty" class="form-control" />
</div>
```

---

**نسخه:** 1.0  
**آخرین بروزرسانی:** 1403
