# مستندات API - CRM Module

## نسخه: 1.0
## Base URL: `/CrmArea`

---

## فهرست

1. [احراز هویت](#احراز-هویت)
2. [Interactions API](#interactions-api)
3. [Goals API](#goals-api)
4. [Referrals API](#referrals-api)
5. [InteractionTypes API](#interactiontypes-api)
6. [Stages API](#stages-api)
7. [Error Handling](#error-handling)
8. [Response Codes](#response-codes)

---

## احراز هویت

همه API ها نیاز به احراز هویت دارند.

### Authentication
```http
Authorization: Bearer {token}
```

### Permissions
```csharp
[PermissionRequired("CRM")]
```

کاربر باید دسترسی CRM داشته باشد.

---

## Interactions API

### Base Path: `/CrmArea/Interaction`

---

### 1. Get All Interactions

لیست تمام تعاملات با فیلتر و صفحه‌بندی

**Endpoint:**
```http
GET /CrmArea/Interaction/Index
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `page` | int | No | شماره صفحه (پیش‌فرض: 1) |
| `pageSize` | int | No | تعداد در هر صفحه (پیش‌فرض: 20) |
| `filters.ContactId` | int | No | فیلتر بر اساس فرد |
| `filters.InteractionTypeId` | int | No | فیلتر بر اساس نوع تعامل |
| `filters.SearchTerm` | string | No | جستجو در موضوع و توضیحات |
| `filters.FromDate` | string | No | از تاریخ (Persian: YYYY/MM/DD) |
| `filters.ToDate` | string | No | تا تاریخ (Persian: YYYY/MM/DD) |
| `filters.HasNextAction` | bool | No | دارای اقدام بعدی |
| `filters.LeadStageStatusId` | int | No | مرحله قیف فروش |
| `filters.PostPurchaseStageId` | int | No | مرحله بعد از خرید |

**Example Request:**
```http
GET /CrmArea/Interaction/Index?page=1&pageSize=10&filters.ContactId=5
```

**Example Response:**
```html
<!-- HTML View with InteractionListViewModel -->
```

**ViewModel:**
```csharp
public class InteractionListViewModel
{
    public List<InteractionViewModel> Interactions { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public InteractionFilterViewModel? Filters { get; set; }
}
```

---

### 2. Get Interaction by ID

دریافت جزئیات یک تعامل

**Endpoint:**
```http
GET /CrmArea/Interaction/Details/{id}
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | شناسه تعامل |

**Example Request:**
```http
GET /CrmArea/Interaction/Details/123
```

**Response (200 OK):**
```html
<!-- HTML View with InteractionViewModel -->
```

**Response (404 Not Found):**
```
Redirect to Index with ErrorMessage
```

**ViewModel:**
```csharp
public class InteractionViewModel
{
    public int Id { get; set; }
    
    // Contact Info
    public int ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? ContactType { get; set; }
    
    // Interaction Type
    public int InteractionTypeId { get; set; }
    public string? InteractionTypeName { get; set; }
    public string? InteractionTypeColor { get; set; }
    public string? InteractionTypeIcon { get; set; }
    
    // Lead Stage (for Leads)
    public int? LeadStageStatusId { get; set; }
    public string? LeadStageStatusTitle { get; set; }
    public string? LeadStageStatusColor { get; set; }
    
    // Post-Purchase Stage (for Customers)
    public int? PostPurchaseStageId { get; set; }
    public string? PostPurchaseStageTitle { get; set; }
    public string? PostPurchaseStageColor { get; set; }
    
    // Details
    public string? Subject { get; set; }
    public string Description { get; set; }
    
    // Date/Time
    public DateTime InteractionDate { get; set; }
    public string? InteractionDatePersian { get; set; }
    public string? InteractionTime { get; set; }
    public int? DurationMinutes { get; set; }
    
    // Result & Follow-up
    public string? Result { get; set; }
    public string? NextAction { get; set; }
    public DateTime? NextActionDate { get; set; }
    public string? NextActionDatePersian { get; set; }
    
    // Related Goals
    public List<GoalViewModel> Goals { get; set; }
    
    // Referral Info
    public bool HasReferral { get; set; }
    public string? ReferredContactName { get; set; }
    public bool IsReferred { get; set; }
    public string? ReferrerContactName { get; set; }
    
    // Metadata
    public string? CreatorName { get; set; }
    public string? CreatedDatePersian { get; set; }
}
```

---

### 3. Create Interaction

ایجاد تعامل جدید

**Endpoint:**
```http
POST /CrmArea/Interaction/Create
```

**Content-Type:** `application/x-www-form-urlencoded` یا `multipart/form-data`

**Request Body:**

```csharp
public class InteractionCreateViewModel
{
    [Required(ErrorMessage = "انتخاب فرد الزامی است")]
    public int ContactId { get; set; }
    
    [Required(ErrorMessage = "نوع تعامل الزامی است")]
    public int InteractionTypeId { get; set; }
    
    public int? PostPurchaseStageId { get; set; }
    
    [MaxLength(300)]
    public string? Subject { get; set; }
    
    [Required(ErrorMessage = "شرح تعامل الزامی است")]
    public string Description { get; set; }
    
    [Required(ErrorMessage = "تاریخ الزامی است")]
    public string InteractionDatePersian { get; set; }
    
    public string? InteractionTime { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? DurationMinutes { get; set; }
    
    public string? Result { get; set; }
    
    public string? NextAction { get; set; }
    public string? NextActionDatePersian { get; set; }
    
    // Goals
    public List<int>? GoalIds { get; set; }
    
    // Referral
    public bool HasReferral { get; set; }
    public int? ReferredContactId { get; set; }
    
    public bool IsReferred { get; set; }
    public int? ReferrerContactId { get; set; }
}
```

**Example Request:**
```http
POST /CrmArea/Interaction/Create
Content-Type: application/x-www-form-urlencoded

ContactId=5&
InteractionTypeId=3&
Subject=جلسه%20معرفی%20محصول&
Description=معرفی%20پکیج%20پیشرفته&
InteractionDatePersian=1403/08/15&
InteractionTime=10:30&
DurationMinutes=60&
Result=مشتری%20علاقه‌مند%20است&
NextAction=ارسال%20پروپوزال&
NextActionDatePersian=1403/08/20&
GoalIds=12&GoalIds=15
```

**Response (Success):**
```http
302 Redirect
Location: /CrmArea/Interaction/Details/{newId}
TempData["SuccessMessage"] = "تعامل با موفقیت ثبت شد"
```

**Response (Validation Error):**
```http
200 OK
<!-- Returns View with ValidationErrors in ModelState -->
```

**Response (Server Error):**
```http
200 OK
ModelState.AddModelError("", "خطا در ثبت اطلاعات...")
<!-- Returns View -->
```

---

### 4. Get Interactions by Contact

تعاملات یک فرد خاص (Timeline)

**Endpoint:**
```http
GET /CrmArea/Interaction/ByContact/{contactId}
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `contactId` | int | Yes | شناسه فرد |

**Example Request:**
```http
GET /CrmArea/Interaction/ByContact/5
```

**Response:**
```html
<!-- HTML View with Timeline Display -->
```

**ViewModel:**
```csharp
public class InteractionByContactViewModel
{
    public int ContactId { get; set; }
    public string ContactName { get; set; }
    public string ContactType { get; set; }
    
    public List<InteractionViewModel> Interactions { get; set; }
    
    public int TotalInteractions { get; set; }
    public DateTime? LastInteractionDate { get; set; }
    public DateTime? NextActionDate { get; set; }
}
```

---

### 5. Delete Interaction

حذف (غیرفعال کردن) تعامل

**Endpoint:**
```http
POST /CrmArea/Interaction/Delete
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | شناسه تعامل |
| `contactId` | int | No | برای Redirect بعد از حذف |

**Example Request:**
```http
POST /CrmArea/Interaction/Delete
Content-Type: application/x-www-form-urlencoded

id=123&contactId=5
```

**Response (Success):**
```http
302 Redirect
Location: /CrmArea/Interaction/ByContact/5
TempData["SuccessMessage"] = "تعامل با موفقیت حذف شد"
```

**Response (Not Found):**
```http
302 Redirect
Location: /CrmArea/Interaction/Index
TempData["ErrorMessage"] = "تعامل مورد نظر یافت نشد"
```

---

## Goals API

### Base Path: `/CrmArea/Goal`

---

### 1. Get All Goals

لیست اهداف فروش

**Endpoint:**
```http
GET /CrmArea/Goal/Index
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `filters.ContactId` | int | No | فیلتر بر اساس فرد |
| `filters.OrganizationId` | int | No | فیلتر بر اساس سازمان |
| `filters.IsConverted` | bool | No | تبدیل شده به خرید |
| `filters.IsActive` | bool | No | فعال/غیرفعال |
| `filters.LeadStageStatusId` | int | No | مرحله قیف فروش |
| `filters.SearchTerm` | string | No | جستجو در عنوان |

**Example Request:**
```http
GET /CrmArea/Goal/Index?filters.IsActive=true&filters.IsConverted=false
```

**Response:**
```html
<!-- HTML View with GoalListViewModel -->
```

**ViewModel:**
```csharp
public class GoalListViewModel
{
    public List<GoalViewModel> Goals { get; set; }
    
    // Statistics
    public int TotalGoals { get; set; }
    public int ActiveGoals { get; set; }
    public int ConvertedGoals { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public decimal TotalActualValue { get; set; }
    
    public GoalFilterViewModel? Filters { get; set; }
}
```

---

### 2. Get Goal by ID

جزئیات یک هدف

**Endpoint:**
```http
GET /CrmArea/Goal/Details/{id}
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | شناسه هدف |

**Example Request:**
```http
GET /CrmArea/Goal/Details/25
```

**Response:**
```html
<!-- HTML View with GoalViewModel -->
```

**ViewModel:**
```csharp
public class GoalViewModel
{
    public int Id { get; set; }
    
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? ProductName { get; set; }
    
    // Target
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    
    // Stage
    public int? CurrentLeadStageStatusId { get; set; }
    public string? CurrentLeadStageStatusTitle { get; set; }
    public string? CurrentLeadStageStatusColor { get; set; }
    
    // Values
    public decimal? EstimatedValue { get; set; }
    public string? EstimatedValueFormatted { get; set; }
    
    public decimal? ActualValue { get; set; }
    public string? ActualValueFormatted { get; set; }
    
    // Status
    public bool IsConverted { get; set; }
    public DateTime? ConversionDate { get; set; }
    public string? ConversionDatePersian { get; set; }
    
    public bool IsActive { get; set; }
    
    // Related Interactions
    public List<InteractionViewModel> RelatedInteractions { get; set; }
    public int InteractionCount { get; set; }
    
    // Metadata
    public string? CreatorName { get; set; }
    public string? CreatedDatePersian { get; set; }
}
```

---

### 3. Create Goal

ایجاد هدف جدید

**Endpoint:**
```http
POST /CrmArea/Goal/Create
```

**Request Body:**

```csharp
public class GoalCreateViewModel
{
    [Required(ErrorMessage = "عنوان الزامی است")]
    [MaxLength(200)]
    public string Title { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(200)]
    public string? ProductName { get; set; }
    
    // یکی از این دو باید پر باشد
    public int? ContactId { get; set; }
    public int? OrganizationId { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? EstimatedValue { get; set; }
}
```

**Example Request:**
```http
POST /CrmArea/Goal/Create
Content-Type: application/x-www-form-urlencoded

Title=فروش%20پکیج%20ERP&
ProductName=پکیج%20پیشرفته&
ContactId=5&
EstimatedValue=50000000
```

**Response:**
```http
302 Redirect
Location: /CrmArea/Goal/Details/{newId}
TempData["SuccessMessage"] = "هدف با موفقیت ثبت شد"
```

---

### 4. Mark Goal as Converted

تبدیل هدف به خرید

**Endpoint:**
```http
POST /CrmArea/Goal/MarkAsConverted
```

**Request Body:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | شناسه هدف |
| `actualValue` | decimal | No | ارزش واقعی (اگر خالی باشد از EstimatedValue استفاده می‌شود) |

**Example Request:**
```http
POST /CrmArea/Goal/MarkAsConverted
Content-Type: application/x-www-form-urlencoded

id=25&actualValue=45000000
```

**Response:**
```http
302 Redirect
Location: /CrmArea/Goal/Details/25
TempData["SuccessMessage"] = "هدف با موفقیت به خرید تبدیل شد"
```

---

### 5. Get Goals by Contact

اهداف یک فرد خاص

**Endpoint:**
```http
GET /CrmArea/Goal/ByContact/{contactId}
```

**Example Request:**
```http
GET /CrmArea/Goal/ByContact/5
```

**Response:**
```html
<!-- HTML View with List of Goals for Contact -->
```

---

## Referrals API

### Base Path: `/CrmArea/Referral`

---

### 1. Get All Referrals

لیست ارجاعات

**Endpoint:**
```http
GET /CrmArea/Referral/Index
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `filters.ReferrerContactId` | int | No | معرف |
| `filters.ReferredContactId` | int | No | معرفی‌شده |
| `filters.Status` | int | No | وضعیت (1=Pending, 2=Contacted, 3=Successful, 4=Failed) |
| `filters.FromDate` | string | No | از تاریخ |
| `filters.ToDate` | string | No | تا تاریخ |

**Example Request:**
```http
GET /CrmArea/Referral/Index?filters.Status=1
```

**Response:**
```html
<!-- HTML View with ReferralListViewModel -->
```

---

### 2. Get Referral Details

جزئیات یک ارجاع

**Endpoint:**
```http
GET /CrmArea/Referral/Details/{id}
```

**Response ViewModel:**
```csharp
public class ReferralViewModel
{
    public int Id { get; set; }
    
    // Referrer (معرف)
    public int ReferrerContactId { get; set; }
    public string ReferrerContactName { get; set; }
    
    // Referred (معرفی‌شده)
    public int ReferredContactId { get; set; }
    public string ReferredContactName { get; set; }
    
    // Interactions
    public int? ReferrerInteractionId { get; set; }
    public string? ReferrerInteractionSubject { get; set; }
    
    public int? ReferredInteractionId { get; set; }
    public string? ReferredInteractionSubject { get; set; }
    
    // Details
    public DateTime ReferralDate { get; set; }
    public string ReferralDatePersian { get; set; }
    
    public string? Notes { get; set; }
    
    // Status
    public int Status { get; set; }
    public string StatusTitle { get; set; }
    public string StatusBadgeClass { get; set; }
    
    public DateTime? StatusChangeDate { get; set; }
    public string? StatusChangeDatePersian { get; set; }
}
```

---

### 3. Mark Referral as Successful

علامت‌گذاری ارجاع به عنوان موفق

**Endpoint:**
```http
POST /CrmArea/Referral/MarkAsSuccessful
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | int | Yes | شناسه ارجاع |

**Example Request:**
```http
POST /CrmArea/Referral/MarkAsSuccessful
Content-Type: application/x-www-form-urlencoded

id=10
```

**Response:**
```http
302 Redirect
TempData["SuccessMessage"] = "ارجاع به عنوان موفق ثبت شد"
```

---

### 4. Mark Referral as Failed

علامت‌گذاری ارجاع به عنوان ناموفق

**Endpoint:**
```http
POST /CrmArea/Referral/MarkAsFailed
```

**Parameters:** مشابه MarkAsSuccessful

---

## InteractionTypes API

### Base Path: `/CrmArea/Stage`

---

### 1. Get All Interaction Types

**Endpoint:**
```http
GET /CrmArea/Stage/Index
```

**Response:**
```html
<!-- HTML View with all InteractionTypes, LeadStageStatuses, PostPurchaseStages -->
```

---

### 2. Create Interaction Type

**Endpoint:**
```http
POST /CrmArea/Stage/CreateInteractionType
```

**Request Body:**
```csharp
public class InteractionTypeCreateViewModel
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public int LeadStageStatusId { get; set; }
    
    public int DisplayOrder { get; set; }
    
    [MaxLength(20)]
    public string? ColorCode { get; set; }
    
    [MaxLength(50)]
    public string? Icon { get; set; }
}
```

---

## Stages API

### 1. Get All Lead Stage Statuses

**Endpoint:**
```http
GET /api/crm/lead-stages
```

**Response:**
```json
[
  {
    "id": 1,
    "stageType": 1,
    "title": "آگاهی",
    "titleEnglish": "Awareness",
    "displayOrder": 1,
    "colorCode": "#FFC107"
  }
]
```

---

### 2. Get All Post-Purchase Stages

**Endpoint:**
```http
GET /api/crm/post-purchase-stages
```

**Response:**
```json
[
  {
    "id": 1,
    "stageType": 1,
    "title": "راه‌اندازی",
    "titleEnglish": "Onboarding",
    "displayOrder": 1,
    "colorCode": "#17A2B8"
  }
]
```

---

## Error Handling

### Validation Errors

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "ContactId": [
      "انتخاب فرد الزامی است"
    ],
    "InteractionTypeId": [
      "نوع تعامل الزامی است"
    ]
  }
}
```

### Server Errors

```json
{
  "error": "Internal Server Error",
  "message": "خطا در ثبت اطلاعات",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## Response Codes

| Code | Description |
|------|-------------|
| `200` | موفق - داده برگشت داده شد |
| `302` | Redirect - معمولاً بعد از POST های موفق |
| `400` | Bad Request - خطای Validation |
| `401` | Unauthorized - نیاز به Login |
| `403` | Forbidden - عدم دسترسی |
| `404` | Not Found - رکورد یافت نشد |
| `500` | Server Error - خطای سرور |

---

## نکات مهم

### 1. Persian Date Format
همه تاریخ‌های Persian به صورت `YYYY/MM/DD` هستند.

```
1403/08/15
```

### 2. CSRF Token
برای تمام POST ها باید Anti-Forgery Token ارسال شود:

```html
@Html.AntiForgeryToken()
```

یا در Header:
```http
RequestVerificationToken: {token}
```

### 3. Soft Delete
حذف‌ها Soft Delete هستند:
- `IsActive = false`
- رکورد در دیتابیس باقی می‌ماند

### 4. Includes
برای بهبود Performance، از `includeRelations` استفاده کنید:

```csharp
GetByIdAsync(id, includeRelations: false) // سریع‌تر
```

---

**نسخه:** 1.0  
**آخرین بروزرسانی:** 1403
