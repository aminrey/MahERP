# مستندات ساختار دیتابیس - CRM Module

## نسخه: 1.0
## تاریخ: 1403

---

## فهرست

1. [معماری دیتابیس](#معماری-دیتابیس)
2. [جداول اصلی](#جداول-اصلی)
3. [روابط و Foreign Keys](#روابط-و-foreign-keys)
4. [Indexes](#indexes)
5. [Seed Data](#seed-data)
6. [Migrations](#migrations)

---

## معماری دیتابیس

### Schema
تمام جداول CRM در Schema پیش‌فرض (`dbo`) قرار دارند و با پیشوند `Crm_` شروع می‌شوند.

### Naming Convention
```
Crm_<EntityName>       : جداول اصلی
Crm_<Entity1><Entity2> : جداول رابطه M:N
```

---

## جداول اصلی

### 1. Crm_LeadStageStatuses (مراحل قیف فروش)

**توضیح:** تعریف مراحل مختلف که یک Lead در مسیر تبدیل به Customer طی می‌کند.

| ستون | نوع | Null | توضیحات |
|------|-----|------|---------|
| `Id` | int | NOT NULL | کلید اصلی (PK) |
| `StageType` | int | NOT NULL | نوع مرحله (Enum: LeadStageType) |
| `Title` | nvarchar(150) | NOT NULL | عنوان فارسی |
| `TitleEnglish` | nvarchar(150) | NULL | عنوان انگلیسی |
| `Description` | nvarchar(500) | NULL | توضیحات |
| `DisplayOrder` | int | NOT NULL | ترتیب نمایش |
| `ColorCode` | nvarchar(20) | NOT NULL | کد رنگ (HEX) - پیش‌فرض: #6c757d |
| `Icon` | nvarchar(50) | NULL | نام آیکون Font Awesome |
| `IsActive` | bit | NOT NULL | فعال/غیرفعال - پیش‌فرض: 1 |
| `CreatedDate` | datetime2 | NOT NULL | تاریخ ایجاد |
| `CreatorUserId` | int | NOT NULL | کاربر ایجادکننده (FK → Users) |

**Constraints:**
- PRIMARY KEY: `PK_Crm_LeadStageStatuses` on `Id`
- UNIQUE: `UK_Crm_LeadStageStatuses_StageType` on `StageType`
- CHECK: `ColorCode` باید با # شروع شود
- INDEX: `IX_Crm_LeadStageStatuses_DisplayOrder` on `DisplayOrder`

**Data پیش‌فرض:**
```sql
INSERT INTO Crm_LeadStageStatuses (StageType, Title, TitleEnglish, ColorCode, DisplayOrder) VALUES
(1, 'آگاهی', 'Awareness', '#FFC107', 1),
(2, 'علاقه‌مندی', 'Interest', '#2196F3', 2),
(3, 'بررسی', 'Consideration', '#FF9800', 3),
(4, 'قصد خرید', 'Intent', '#9C27B0', 4),
(5, 'ارزیابی', 'Evaluation', '#00BCD4', 5),
(6, 'خرید', 'Purchase', '#4CAF50', 6),
(7, 'از دست رفته', 'Lost', '#F44336', 7);
```

---

### 2. Crm_PostPurchaseStages (مراحل بعد از خرید)

**توضیح:** مراحل خدمات پس از فروش و پیگیری مشتری.

| ستون | نوع | Null | توضیحات |
|------|-----|------|---------|
| `Id` | int | NOT NULL | کلید اصلی (PK) |
| `StageType` | int | NOT NULL | نوع مرحله (Enum: PostPurchaseStageType) |
| `Title` | nvarchar(150) | NOT NULL | عنوان فارسی |
| `TitleEnglish` | nvarchar(150) | NULL | عنوان انگلیسی |
| `Description` | nvarchar(500) | NULL | توضیحات |
| `DisplayOrder` | int | NOT NULL | ترتیب نمایش |
| `ColorCode` | nvarchar(20) | NOT NULL | کد رنگ (HEX) - پیش‌فرض: #28a745 |
| `Icon` | nvarchar(50) | NULL | نام آیکون |
| `IsActive` | bit | NOT NULL | فعال/غیرفعال - پیش‌فرض: 1 |
| `CreatedDate` | datetime2 | NOT NULL | تاریخ ایجاد |
| `CreatorUserId` | int | NOT NULL | کاربر ایجادکننده (FK → Users) |

**Constraints:**
- PRIMARY KEY: `PK_Crm_PostPurchaseStages` on `Id`
- UNIQUE: `UK_Crm_PostPurchaseStages_StageType` on `StageType`
- INDEX: `IX_Crm_PostPurchaseStages_DisplayOrder` on `DisplayOrder`

**Data پیش‌فرض:**
```sql
INSERT INTO Crm_PostPurchaseStages (StageType, Title, TitleEnglish, ColorCode, DisplayOrder) VALUES
(1, 'راه‌اندازی', 'Onboarding', '#17A2B8', 1),
(2, 'استفاده فعال', 'Active', '#28A745', 2),
(3, 'پشتیبانی', 'Support', '#FFC107', 3),
(4, 'تمدید', 'Renewal', '#007BFF', 4),
(5, 'ارتقا', 'Upsell', '#6F42C1', 5),
(6, 'ریزش', 'Churn', '#DC3545', 6);
```

---

### 3. Crm_InteractionTypes (انواع تعامل)

**توضیح:** تعریف انواع مختلف تعامل با مشتریان (تماس، جلسه، ایمیل و ...).

| ستون | نوع | Null | توضیحات |
|------|-----|------|---------|
| `Id` | int | NOT NULL | کلید اصلی (PK) |
| `Title` | nvarchar(150) | NOT NULL | عنوان (مثلاً "تماس تلفنی") |
| `Description` | nvarchar(500) | NULL | توضیحات |
| `LeadStageStatusId` | int | NOT NULL | مرحله‌ای که این تعامل Lead را به آن می‌رساند (FK → Crm_LeadStageStatuses) |
| `DisplayOrder` | int | NOT NULL | ترتیب نمایش - پیش‌فرض: 0 |
| `ColorCode` | nvarchar(20) | NULL | رنگ نمایش |
| `Icon` | nvarchar(50) | NULL | آیکون |
| `IsActive` | bit | NOT NULL | فعال/غیرفعال - پیش‌فرض: 1 |
| `CreatedDate` | datetime2 | NOT NULL | تاریخ ایجاد |
| `CreatorUserId` | int | NOT NULL | کاربر ایجادکننده (FK → Users) |

**Constraints:**
- PRIMARY KEY: `PK_Crm_InteractionTypes` on `Id`
- FOREIGN KEY: `FK_Crm_InteractionTypes_LeadStageStatus` 
  - `LeadStageStatusId` → `Crm_LeadStageStatuses.Id`
  - ON DELETE: RESTRICT
- INDEX: `IX_Crm_InteractionTypes_LeadStageStatusId` on `LeadStageStatusId`
- INDEX: `IX_Crm_InteractionTypes_IsActive` on `IsActive`

**مثال Data:**
```sql
INSERT INTO Crm_InteractionTypes (Title, LeadStageStatusId, DisplayOrder) VALUES
('تماس تلفنی اولیه', 1, 1),        -- Awareness
('جلسه حضوری', 2, 2),              -- Interest
('ارسال پیشنهاد قیمت', 4, 3),      -- Intent
('جلسه مذاکره', 5, 4),             -- Evaluation
('امضای قرارداد', 6, 5);           -- Purchase
```

---

### 4. Crm_Interactions (تعاملات)

**توضیح:** ذخیره تمام تعاملات با مشتریان و سرنخ‌ها.

| ستون | نوع | Null | توضیحات |
|------|-----|------|---------|
| `Id` | int | NOT NULL | کلید اصلی (PK) |
| `ContactId` | int | NOT NULL | شناسه فرد (FK → Contacts) |
| `InteractionTypeId` | int | NOT NULL | نوع تعامل (FK → Crm_InteractionTypes) |
| `PostPurchaseStageId` | int | NULL | مرحله بعد از خرید - فقط برای Customer (FK → Crm_PostPurchaseStages) |
| `Subject` | nvarchar(300) | NULL | موضوع تعامل |
| `Description` | nvarchar(MAX) | NOT NULL | شرح کامل تعامل |
| `InteractionDate` | datetime2 | NOT NULL | تاریخ و زمان تعامل - پیش‌فرض: GETDATE() |
| `DurationMinutes` | int | NULL | مدت زمان به دقیقه |
| `Result` | nvarchar(1000) | NULL | نتیجه تعامل |
| `NextAction` | nvarchar(500) | NULL | اقدام بعدی مورد نیاز |
| `NextActionDate` | datetime2 | NULL | تاریخ اقدام بعدی |
| `IsActive` | bit | NOT NULL | فعال/غیرفعال - پیش‌فرض: 1 |
| `CreatedDate` | datetime2 | NOT NULL | تاریخ ثبت |
| `CreatorUserId` | int | NOT NULL | کاربر ثبت‌کننده (FK → Users) |

**Constraints:**
- PRIMARY KEY: `PK_Crm_Interactions` on `Id`
- FOREIGN KEY: `FK_Crm_Interactions_Contact`
  - `ContactId` → `Contacts.Id`
  - ON DELETE: RESTRICT
- FOREIGN KEY: `FK_Crm_Interactions_InteractionType`
  - `InteractionTypeId` → `Crm_InteractionTypes.Id`
  - ON DELETE: RESTRICT
- FOREIGN KEY: `FK_Crm_Interactions_PostPurchaseStage`
  - `PostPurchaseStageId` → `Crm_PostPurchaseStages.Id`
  - ON DELETE: SET NULL
- CHECK: `DurationMinutes >= 0`
- CHECK: `NextActionDate` نباید در گذشته باشد

**Indexes:**
```sql
CREATE INDEX IX_Crm_Interactions_ContactId ON Crm_Interactions(ContactId);
CREATE INDEX IX_Crm_Interactions_InteractionTypeId ON Crm_Interactions(InteractionTypeId);
CREATE INDEX IX_Crm_Interactions_InteractionDate ON Crm_Interactions(InteractionDate DESC);
CREATE INDEX IX_Crm_Interactions_NextActionDate ON Crm_Interactions(NextActionDate) 
    WHERE NextActionDate IS NOT NULL;
CREATE INDEX IX_Crm_Interactions_IsActive ON Crm_Interactions(IsActive);
```

---

### 5. Crm_Goals (اهداف فروش)

**توضیح:** فرصت‌های فروش و اهداف مرتبط با هر Lead یا Customer.

| ستون | نوع | Null | توضیحات |
|------|-----|------|---------|
| `Id` | int | NOT NULL | کلید اصلی (PK) |
| `Title` | nvarchar(200) | NOT NULL | عنوان هدف |
| `Description` | nvarchar(1000) | NULL | توضیحات |
| `ProductName` | nvarchar(200) | NULL | نام محصول/خدمت |
| `ContactId` | int | NULL | شناسه فرد (FK → Contacts) |
| `OrganizationId` | int | NULL | شناسه سازمان (FK → Organizations) |
| `CurrentLeadStageStatusId` | int | NULL | مرحله فعلی در قیف فروش (FK → Crm_LeadStageStatuses) |
| `EstimatedValue` | decimal(18,2) | NULL | ارزش تخمینی (ریال) |
| `ActualValue` | decimal(18,2) | NULL | ارزش واقعی (بعد از تبدیل) |
| `IsConverted` | bit | NOT NULL | تبدیل به خرید شده؟ - پیش‌فرض: 0 |
| `ConversionDate` | datetime2 | NULL | تاریخ تبدیل به خرید |
| `IsActive` | bit | NOT NULL | فعال/غیرفعال - پیش‌فرض: 1 |
| `CreatedDate` | datetime2 | NOT NULL | تاریخ ایجاد |
| `CreatorUserId` | int | NOT NULL | کاربر ایجادکننده (FK → Users) |

**Constraints:**
- PRIMARY KEY: `PK_Crm_Goals` on `Id`
- FOREIGN KEY: `FK_Crm_Goals_Contact`
  - `ContactId` → `Contacts.Id`
  - ON DELETE: RESTRICT
- FOREIGN KEY: `FK_Crm_Goals_Organization`
  - `OrganizationId` → `Organizations.Id`
  - ON DELETE: RESTRICT
- FOREIGN KEY: `FK_Crm_Goals_LeadStageStatus`
  - `CurrentLeadStageStatusId` → `Crm_LeadStageStatuses.Id`
  - ON DELETE: SET NULL
- CHECK: حداقل یکی از `ContactId` یا `OrganizationId` باید پر باشد
- CHECK: `EstimatedValue >= 0`
- CHECK: `ActualValue >= 0`
- CHECK: اگر `IsConverted = 1` آنگاه `ConversionDate` و `ActualValue` باید پر باشند

**Indexes:**
```sql
CREATE INDEX IX_Crm_Goals_ContactId ON Crm_Goals(ContactId);
CREATE INDEX IX_Crm_Goals_OrganizationId ON Crm_Goals(OrganizationId);
CREATE INDEX IX_Crm_Goals_IsConverted ON Crm_Goals(IsConverted);
CREATE INDEX IX_Crm_Goals_IsActive ON Crm_Goals(IsActive);
CREATE INDEX IX_Crm_Goals_CurrentLeadStageStatusId ON Crm_Goals(CurrentLeadStageStatusId);
```

---

### 6. Crm_InteractionGoals (رابطه M:N)

**توضیح:** رابطه Many-to-Many بین Interactions و Goals.

| ستون | نوع | Null | توضیحات |
|------|-----|------|---------|
| `InteractionId` | int | NOT NULL | شناسه تعامل (FK → Crm_Interactions) |
| `GoalId` | int | NOT NULL | شناسه هدف (FK → Crm_Goals) |
| `CreatedDate` | datetime2 | NOT NULL | تاریخ لینک - پیش‌فرض: GETDATE() |
| `CreatorUserId` | int | NOT NULL | کاربر لینک‌کننده (FK → Users) |

**Constraints:**
- PRIMARY KEY: `PK_Crm_InteractionGoals` on (`InteractionId`, `GoalId`)
- FOREIGN KEY: `FK_Crm_InteractionGoals_Interaction`
  - `InteractionId` → `Crm_Interactions.Id`
  - ON DELETE: CASCADE
- FOREIGN KEY: `FK_Crm_InteractionGoals_Goal`
  - `GoalId` → `Crm_Goals.Id`
  - ON DELETE: CASCADE
- INDEX: `IX_Crm_InteractionGoals_GoalId` on `GoalId`

---

### 7. Crm_Referrals (ارجاعات)

**توضیح:** ذخیره اطلاعات معرفی مشتریان توسط سایر مشتریان.

| ستون | نوع | Null | توضیحات |
|------|-----|------|---------|
| `Id` | int | NOT NULL | کلید اصلی (PK) |
| `ReferrerContactId` | int | NOT NULL | معرف (Customer) (FK → Contacts) |
| `ReferredContactId` | int | NOT NULL | معرفی‌شده (Lead) (FK → Contacts) |
| `ReferrerInteractionId` | int | NULL | تعامل معرف (FK → Crm_Interactions) |
| `ReferredInteractionId` | int | NULL | تعامل معرفی‌شده (FK → Crm_Interactions) |
| `ReferralDate` | datetime2 | NOT NULL | تاریخ ارجاع - پیش‌فرض: GETDATE() |
| `Notes` | nvarchar(1000) | NULL | یادداشت |
| `Status` | int | NOT NULL | وضعیت (Enum: ReferralStatus) - پیش‌فرض: 1 (Pending) |
| `StatusChangeDate` | datetime2 | NULL | تاریخ تغییر وضعیت |
| `CreatedDate` | datetime2 | NOT NULL | تاریخ ثبت |
| `CreatorUserId` | int | NOT NULL | کاربر ثبت‌کننده (FK → Users) |

**Constraints:**
- PRIMARY KEY: `PK_Crm_Referrals` on `Id`
- FOREIGN KEY: `FK_Crm_Referrals_ReferrerContact`
  - `ReferrerContactId` → `Contacts.Id`
  - ON DELETE: RESTRICT
- FOREIGN KEY: `FK_Crm_Referrals_ReferredContact`
  - `ReferredContactId` → `Contacts.Id`
  - ON DELETE: RESTRICT
- FOREIGN KEY: `FK_Crm_Referrals_ReferrerInteraction`
  - `ReferrerInteractionId` → `Crm_Interactions.Id`
  - ON DELETE: SET NULL
- FOREIGN KEY: `FK_Crm_Referrals_ReferredInteraction`
  - `ReferredInteractionId` → `Crm_Interactions.Id`
  - ON DELETE: SET NULL
- CHECK: `ReferrerContactId <> ReferredContactId`
- UNIQUE: `UK_Crm_Referrals_Unique` on (`ReferrerContactId`, `ReferredContactId`)

**Indexes:**
```sql
CREATE INDEX IX_Crm_Referrals_ReferrerContactId ON Crm_Referrals(ReferrerContactId);
CREATE INDEX IX_Crm_Referrals_ReferredContactId ON Crm_Referrals(ReferredContactId);
CREATE INDEX IX_Crm_Referrals_Status ON Crm_Referrals(Status);
CREATE INDEX IX_Crm_Referrals_ReferralDate ON Crm_Referrals(ReferralDate DESC);
```

---

## روابط و Foreign Keys

### نمودار روابط کلی

```
Contacts (1) ──────────► (N) Interactions
                              │
                              │ (N)
                              ↓
                         InteractionTypes (N) ──► (1) LeadStageStatuses
                              │
                              │ (1)
                              ↓
                         PostPurchaseStages


Contacts/Organizations (1) ──► (N) Goals
                                    │
                                    │ (N)
                                    ↓
                               LeadStageStatuses


Interactions (N) ◄────M:N────► (N) Goals
                (via InteractionGoals)


Contacts (Referrer) ──► (N) Referrals ◄── (N) Contacts (Referred)
     │                        │
     └────────────────────────┴─────────► Interactions
```

### جدول Foreign Keys

| جدول مبدا | ستون | جدول مقصد | ON DELETE |
|-----------|------|-----------|-----------|
| Crm_InteractionTypes | LeadStageStatusId | Crm_LeadStageStatuses | RESTRICT |
| Crm_Interactions | ContactId | Contacts | RESTRICT |
| Crm_Interactions | InteractionTypeId | Crm_InteractionTypes | RESTRICT |
| Crm_Interactions | PostPurchaseStageId | Crm_PostPurchaseStages | SET NULL |
| Crm_Goals | ContactId | Contacts | RESTRICT |
| Crm_Goals | OrganizationId | Organizations | RESTRICT |
| Crm_Goals | CurrentLeadStageStatusId | Crm_LeadStageStatuses | SET NULL |
| Crm_InteractionGoals | InteractionId | Crm_Interactions | CASCADE |
| Crm_InteractionGoals | GoalId | Crm_Goals | CASCADE |
| Crm_Referrals | ReferrerContactId | Contacts | RESTRICT |
| Crm_Referrals | ReferredContactId | Contacts | RESTRICT |
| Crm_Referrals | ReferrerInteractionId | Crm_Interactions | SET NULL |
| Crm_Referrals | ReferredInteractionId | Crm_Interactions | SET NULL |

---

## Indexes

### Index Strategy

1. **Primary Keys:** خودکار ایجاد می‌شوند
2. **Foreign Keys:** برای بهبود JOIN ها
3. **Filtered Indexes:** برای ستون‌های با شرط (مثلاً WHERE IsActive = 1)
4. **Covering Indexes:** برای کوئری‌های پرتکرار

### لیست کامل Indexes

```sql
-- LeadStageStatuses
CREATE INDEX IX_Crm_LeadStageStatuses_DisplayOrder 
    ON Crm_LeadStageStatuses(DisplayOrder);
CREATE INDEX IX_Crm_LeadStageStatuses_IsActive 
    ON Crm_LeadStageStatuses(IsActive) WHERE IsActive = 1;

-- PostPurchaseStages
CREATE INDEX IX_Crm_PostPurchaseStages_DisplayOrder 
    ON Crm_PostPurchaseStages(DisplayOrder);
CREATE INDEX IX_Crm_PostPurchaseStages_IsActive 
    ON Crm_PostPurchaseStages(IsActive) WHERE IsActive = 1;

-- InteractionTypes
CREATE INDEX IX_Crm_InteractionTypes_LeadStageStatusId 
    ON Crm_InteractionTypes(LeadStageStatusId);
CREATE INDEX IX_Crm_InteractionTypes_IsActive 
    ON Crm_InteractionTypes(IsActive) WHERE IsActive = 1;

-- Interactions
CREATE INDEX IX_Crm_Interactions_ContactId 
    ON Crm_Interactions(ContactId) 
    INCLUDE (InteractionDate, InteractionTypeId);
CREATE INDEX IX_Crm_Interactions_InteractionDate 
    ON Crm_Interactions(InteractionDate DESC);
CREATE INDEX IX_Crm_Interactions_NextActionDate 
    ON Crm_Interactions(NextActionDate) 
    WHERE NextActionDate IS NOT NULL AND IsActive = 1;

-- Goals
CREATE INDEX IX_Crm_Goals_ContactId 
    ON Crm_Goals(ContactId) 
    WHERE IsActive = 1;
CREATE INDEX IX_Crm_Goals_IsConverted_IsActive 
    ON Crm_Goals(IsConverted, IsActive) 
    INCLUDE (EstimatedValue, ActualValue);

-- Referrals
CREATE INDEX IX_Crm_Referrals_ReferrerContactId_Status 
    ON Crm_Referrals(ReferrerContactId, Status);
CREATE INDEX IX_Crm_Referrals_ReferredContactId 
    ON Crm_Referrals(ReferredContactId);
```

---

## Seed Data

### LeadStageStatuses
```sql
SET IDENTITY_INSERT Crm_LeadStageStatuses ON;

INSERT INTO Crm_LeadStageStatuses 
(Id, StageType, Title, TitleEnglish, Description, DisplayOrder, ColorCode, Icon, IsActive, CreatedDate, CreatorUserId)
VALUES
(1, 1, N'آگاهی', 'Awareness', N'مشتری بالقوه با محصول آشنا شده', 1, '#FFC107', 'fa-lightbulb', 1, GETDATE(), 1),
(2, 2, N'علاقه‌مندی', 'Interest', N'علاقه نشان داده و اطلاعات بیشتری می‌خواهد', 2, '#2196F3', 'fa-star', 1, GETDATE(), 1),
(3, 3, N'بررسی', 'Consideration', N'در حال مقایسه و بررسی گزینه‌ها', 3, '#FF9800', 'fa-search', 1, GETDATE(), 1),
(4, 4, N'قصد خرید', 'Intent', N'قصد جدی برای خرید دارد', 4, '#9C27B0', 'fa-handshake', 1, GETDATE(), 1),
(5, 5, N'ارزیابی', 'Evaluation', N'در حال ارزیابی نهایی', 5, '#00BCD4', 'fa-clipboard-check', 1, GETDATE(), 1),
(6, 6, N'خرید', 'Purchase', N'خرید انجام شده', 6, '#4CAF50', 'fa-check-circle', 1, GETDATE(), 1),
(7, 7, N'از دست رفته', 'Lost', N'فرصت از دست رفته', 7, '#F44336', 'fa-times-circle', 1, GETDATE(), 1);

SET IDENTITY_INSERT Crm_LeadStageStatuses OFF;
```

### PostPurchaseStages
```sql
SET IDENTITY_INSERT Crm_PostPurchaseStages ON;

INSERT INTO Crm_PostPurchaseStages 
(Id, StageType, Title, TitleEnglish, Description, DisplayOrder, ColorCode, Icon, IsActive, CreatedDate, CreatorUserId)
VALUES
(1, 1, N'راه‌اندازی', 'Onboarding', N'مشتری در حال راه‌اندازی و آشنایی با محصول', 1, '#17A2B8', 'fa-rocket', 1, GETDATE(), 1),
(2, 2, N'استفاده فعال', 'Active', N'استفاده فعال از محصول/خدمت', 2, '#28A745', 'fa-check-circle', 1, GETDATE(), 1),
(3, 3, N'پشتیبانی', 'Support', N'نیازمند پشتیبانی و راهنمایی', 3, '#FFC107', 'fa-life-ring', 1, GETDATE(), 1),
(4, 4, N'تمدید', 'Renewal', N'زمان تمدید قرارداد', 4, '#007BFF', 'fa-sync', 1, GETDATE(), 1),
(5, 5, N'ارتقا', 'Upsell', N'فرصت فروش محصول بیشتر یا ارتقا', 5, '#6F42C1', 'fa-arrow-up', 1, GETDATE(), 1),
(6, 6, N'ریزش', 'Churn', N'مشتری از دست رفته', 6, '#DC3545', 'fa-user-times', 1, GETDATE(), 1);

SET IDENTITY_INSERT Crm_PostPurchaseStages OFF;
```

### InteractionTypes (نمونه)
```sql
INSERT INTO Crm_InteractionTypes 
(Title, Description, LeadStageStatusId, DisplayOrder, ColorCode, Icon, IsActive, CreatedDate, CreatorUserId)
VALUES
(N'تماس تلفنی اولیه', N'اولین تماس با مشتری بالقوه', 1, 1, '#2196F3', 'fa-phone', 1, GETDATE(), 1),
(N'ارسال ایمیل معرفی', N'ارسال ایمیل معرفی محصول', 1, 2, '#00BCD4', 'fa-envelope', 1, GETDATE(), 1),
(N'جلسه حضوری', N'جلسه رودررو با مشتری', 2, 3, '#9C27B0', 'fa-users', 1, GETDATE(), 1),
(N'ارسال پیشنهاد قیمت', N'ارسال پروپوزال و پیشنهاد قیمت', 4, 4, '#FF9800', 'fa-file-invoice-dollar', 1, GETDATE(), 1),
(N'جلسه مذاکره', N'مذاکره در مورد شرایط و قیمت', 5, 5, '#673AB7', 'fa-handshake', 1, GETDATE(), 1),
(N'امضای قرارداد', N'امضا و نهایی شدن قرارداد', 6, 6, '#4CAF50', 'fa-file-signature', 1, GETDATE(), 1);
```

---

## Migrations

### Initial Migration
```csharp
public partial class AddCrmModule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create LeadStageStatuses table
        migrationBuilder.CreateTable(
            name: "Crm_LeadStageStatuses",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                StageType = table.Column<int>(nullable: false),
                Title = table.Column<string>(maxLength: 150, nullable: false),
                // ... سایر ستون‌ها
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Crm_LeadStageStatuses", x => x.Id);
            });
            
        // ... سایر جداول
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Crm_Referrals");
        migrationBuilder.DropTable(name: "Crm_InteractionGoals");
        migrationBuilder.DropTable(name: "Crm_Goals");
        migrationBuilder.DropTable(name: "Crm_Interactions");
        migrationBuilder.DropTable(name: "Crm_InteractionTypes");
        migrationBuilder.DropTable(name: "Crm_PostPurchaseStages");
        migrationBuilder.DropTable(name: "Crm_LeadStageStatuses");
    }
}
```

### Migration Commands
```bash
# ایجاد Migration
dotnet ef migrations add AddCrmModule --project MahERP.DataModelLayer --startup-project MahERP

# اعمال Migration
dotnet ef database update --project MahERP.DataModelLayer --startup-project MahERP

# حذف آخرین Migration
dotnet ef migrations remove --project MahERP.DataModelLayer --startup-project MahERP

# تولید SQL Script
dotnet ef migrations script --project MahERP.DataModelLayer --startup-project MahERP --output CrmMigration.sql
```

---

## نکات عملکردی (Performance Tips)

### 1. Query Optimization
```sql
-- بد
SELECT * FROM Crm_Interactions WHERE ContactId = @ContactId;

-- خوب
SELECT Id, Subject, InteractionDate, InteractionTypeId 
FROM Crm_Interactions 
WHERE ContactId = @ContactId AND IsActive = 1
ORDER BY InteractionDate DESC;
```

### 2. Covering Index Usage
```sql
-- برای کوئری پرتکرار لیست تعاملات
CREATE INDEX IX_Crm_Interactions_ContactId_Covering
ON Crm_Interactions(ContactId)
INCLUDE (InteractionDate, Subject, InteractionTypeId, IsActive);
```

### 3. Partitioning (برای دیتای زیاد)
```sql
-- Partition بر اساس سال
CREATE PARTITION FUNCTION PF_InteractionDate (datetime2)
AS RANGE RIGHT FOR VALUES 
    ('2023-01-01', '2024-01-01', '2025-01-01');
```

---

## Backup و Maintenance

### Backup Strategy
```sql
-- Full Backup روزانه
BACKUP DATABASE [MahERP]
TO DISK = 'C:\Backups\MahERP_Full.bak'
WITH COMPRESSION, STATS = 10;

-- Differential Backup هر 6 ساعت
BACKUP DATABASE [MahERP]
TO DISK = 'C:\Backups\MahERP_Diff.bak'
WITH DIFFERENTIAL, COMPRESSION;
```

### Index Maintenance
```sql
-- Rebuild Indexes هفتگی
ALTER INDEX ALL ON Crm_Interactions REBUILD 
WITH (ONLINE = ON, SORT_IN_TEMPDB = ON);

-- Update Statistics
UPDATE STATISTICS Crm_Interactions WITH FULLSCAN;
```

---

**نسخه:** 1.0  
**آخرین بروزرسانی:** 1403  
**وضعیت:** Production Ready
