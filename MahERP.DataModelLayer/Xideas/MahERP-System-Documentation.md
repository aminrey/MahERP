# 📘 مستندات جامع سیستم MahERP

## 📋 فهرست مطالب
1. [معرفی سیستم](#معرفی-سیستم)
2. [معماری و ساختار پروژه](#معماری-و-ساختار-پروژه)
3. [ساختار دیتابیس](#ساختار-دیتابیس)
4. [ماژول‌های سیستم](#ماژولهای-سیستم)
5. [سیستم مدیریت کاربران و دسترسی](#سیستم-مدیریت-کاربران-و-دسترسی)
6. [سیستم نظارت بر تسک‌ها](#سیستم-نظارت-بر-تسکها) ⭐ **جدید**
7. [سیستم اعلان‌رسانی](#سیستم-اعلانرسانی)
8. [سیستم ارتباطات](#سیستم-ارتباطات)
9. [نحوه راه‌اندازی](#نحوه-راهاندازی)

---

## 🎯 معرفی سیستم

### نام سیستم
**MahERP** - سیستم یکپارچه مدیریت منابع سازمانی

### نسخه
**.NET 9.0** (آخرین نسخه)

### نوع پروژه
**ASP.NET Core MVC** با **Razor Pages**

### توضیحات کلی
MahERP یک سیستم ERP مدرن و یکپارچه است که برای مدیریت فرآیندهای سازمانی طراحی شده است. این سیستم شامل ماژول‌های مختلفی از جمله مدیریت تسک‌ها، CRM، مدیریت کاربران، سیستم اعلان‌رسانی پیشرفته و ارتباطات (ایمیل، SMS، تلگرام) می‌باشد.

### ویژگی‌های کلیدی
- ✅ معماری لایه‌ای (Layered Architecture)
- ✅ استفاده از Entity Framework Core 9
- ✅ سیستم احراز هویت و مجوزدهی پیشرفته
- ✅ **سیستم نظارت هوشمند بر تسک‌ها** ⭐ **جدید**
- ✅ سیستم اعلان‌رسانی چندکاناله (Email, SMS, Telegram)
- ✅ مدیریت تسک‌ها با قابلیت‌های پیشرفته
- ✅ سیستم CRM یکپارچه
- ✅ پشتیبانی کامل از زبان فارسی و تاریخ شمسی
- ✅ Background Services برای پردازش خودکار

---

## 🏗️ معماری و ساختار پروژه

### ساختار کلی پروژه

```
MahERP/
│
├── MahERP/                                    # لایه Presentation (Main Web App)
│   ├── Areas/
│   │   ├── AppCoreArea/                       # ماژول هسته مرکزی
│   │   │   ├── Controllers/
│   │   │   │   ├── BaseControllers/
│   │   │   │   ├── CoreControllers/
│   │   │   │   ├── ContactControllers/
│   │   │   │   ├── OrganizationControllers/
│   │   │   │   ├── PermissionControllers/
│   │   │   │   └── UserControllers/
│   │   │   └── Views/
│   │   │
│   │   ├── TaskingArea/                       # ماژول تسکینگ
│   │   │   ├── Controllers/
│   │   │   │   ├── TaskControllers/
│   │   │   │   ├── BaseControllers/
│   │   │   │   └── NotificationController.cs
│   │   │   └── Views/
│   │   │
│   │   └── CrmArea/                           # ماژول CRM
│   │       ├── Controllers/
│   │       │   ├── CRMControllers/
│   │       │   ├── CommunicationControllers/
│   │       │   └── BaseControllers/
│   │       └── Views/
│   │
│   ├── Controllers/                           # کنترلرهای اصلی
│   │   ├── AccountController.cs
│   │   ├── HomeController.cs
│   │   └── TelegramWebhookController.cs
│   │
│   ├── Services/                              # سرویس‌های اصلی
│   │   ├── INotificationService.cs
│   │   └── NotificationService.cs
│   │
│   ├── Hubs/
│   │   └── NotificationHub.cs                 # SignalR Hub برای اعلان‌های لحظه‌ای
│   │
│   ├── Attributes/
│   │   └── PermissionRequiredAttribute.cs     # Attribute بررسی دسترسی
│   │
│   ├── Extensions/
│   │   ├── NotificationExtensions.cs
│   │   └── PermissionExtensions.cs
│   │
│   ├── Helpers/
│   │   └── VersionHelper.cs
│   │
│   ├── Program.cs                             # Entry Point
│   └── appsettings.json
│
├── MahERP.DataModelLayer/                     # لایه Data & Business Logic
│   ├── Entities/                              # Entity Classes
│   │   ├── AcControl/                         # موجودیت‌های کنترل دسترسی
│   │   │   ├── AppUsers.cs
│   │   │   ├── AppRoles.cs
│   │   │   ├── Permission.cs
│   │   │   ├── Role.cs
│   │   │   ├── UserRole.cs
│   │   │   ├── UserPermission.cs
│   │   │   ├── RolePermission.cs
│   │   │   ├── Branch.cs
│   │   │   ├── BranchUser.cs
│   │   │   ├── Contract.cs
│   │   │   ├── Stakeholder.cs (⚠️ Obsolete)
│   │   │   └── ...
│   │   │
│   │   ├── Contacts/                          # موجودیت‌های مدیریت افراد و سازمان‌ها
│   │   │   ├── Contact.cs                     # ⭐ جدید
│   │   │   ├── ContactPhone.cs
│   │   │   ├── ContactGroup.cs
│   │   │   ├── ContactGroupMember.cs
│   │   │   ├── BranchContact.cs
│   │   │   ├── BranchContactGroup.cs
│   │   │   ├── Organization.cs                # ⭐ جدید
│   │   │   ├── OrganizationDepartment.cs
│   │   │   ├── OrganizationContact.cs
│   │   │   ├── OrganizationGroup.cs
│   │   │   └── ...
│   │   │
│   │   ├── TaskManagement/                    # موجودیت‌های مدیریت تسک
│   │   │   ├── Tasks.cs
│   │   │   ├── TaskAssignment.cs
│   │   │   ├── TaskOperation.cs
│   │   │   ├── TaskComment.cs
│   │   │   ├── TaskAttachment.cs
│   │   │   ├── TaskReminder.cs
│   │   │   ├── TaskMyDay.cs
│   │   │   ├── TaskWorkLog.cs
│   │   │   ├── TaskCategory.cs
│   │   │   ├── TaskSchedule.cs
│   │   │   └── ...
│   │   │
│   │   ├── Core/                              # موجودیت‌های هسته مرکزی
│   │   │   ├── Team.cs
│   │   │   ├── TeamMember.cs
│   │   │   ├── TeamPosition.cs
│   │   │   ├── ActivityBase.cs
│   │   │   ├── ActivityTask.cs
│   │   │   ├── ActivityCRM.cs
│   │   │   ├── UserActivityLog.cs
│   │   │   ├── CoreNotification.cs            # اعلان‌های سیستمی
│   │   │   ├── CoreNotificationDetail.cs
│   │   │   ├── CoreNotificationDelivery.cs
│   │   │   └── Settings.cs
│   │   │
│   │   ├── Notifications/                     # سیستم اعلان‌رسانی پیشرفته
│   │   │   ├── NotificationTemplate.cs        # ⭐ قالب‌های اعلان
│   │   │   ├── NotificationTemplateRecipient.cs
│   │   │   ├── NotificationTemplateVariable.cs
│   │   │   ├── NotificationScheduledMessage.cs
│   │   │   ├── NotificationModuleConfig.cs
│   │   │   ├── NotificationTypeConfig.cs
│   │   │   ├── NotificationBlacklist.cs
│   │   │   └── UserNotificationPreference.cs
│   │   │
│   │   ├── Crm/                               # موجودیت‌های CRM
│   │   │   ├── CRMInteraction.cs
│   │   │   ├── CRMComment.cs
│   │   │   ├── CRMAttachment.cs
│   │   │   ├── CRMParticipant.cs
│   │   │   └── ...
│   │   │
│   │   ├── Email/                             # سیستم ایمیل
│   │   │   ├── EmailQueue.cs
│   │   │   ├── EmailLog.cs
│   │   │   ├── EmailTemplate.cs
│   │   │   └── EmailTemplateRecipient.cs
│   │   │
│   │   └── Sms/                               # سیستم پیامک
│   │       ├── SmsQueue.cs
│   │       ├── SmsLog.cs
│   │       ├── SmsProvider.cs
│   │       ├── SmsTemplate.cs
│   │       └── SmsTemplateRecipient.cs
│   │
│   ├── Repository/                            # Repository Pattern
│   │   ├── IBaseRepository.cs
│   │   ├── BaseRepository.cs
│   │   ├── GenereicClass.cs
│   │   ├── IUnitOfWork.cs
│   │   ├── UnitOfWork.cs
│   │   ├── TaskRepository/
│   │   ├── ContactRepository/
│   │   ├── OrganizationRepository/
│   │   ├── ContactGroupRepository/
│   │   ├── Notifications/
│   │   └── ...
│   │
│   ├── Services/                              # Business Logic Services
│   │   ├── IPermissionService.cs
│   │   ├── IUserRoleRepository.cs
│   │   ├── IModuleAccessService.cs
│   │   ├── ModuleAccessService.cs
│   │   ├── NotificationManagementService.cs   # ⭐ سرویس جامع اعلان‌ها
│   │   ├── ActivityLoggerService.cs
│   │   ├── EmailService.cs
│   │   ├── SmsService.cs
│   │   ├── BackGroundServices/
│   │   │   ├── NotificationProcessingBackgroundService.cs
│   │   │   ├── ScheduledNotificationBackgroundService.cs
│   │   │   ├── TelegramPollingBackgroundService.cs
│   │   │   ├── EmailBackgroundService.cs
│   │   │   ├── SmsBackgroundService.cs
│   │   │   └── ModuleTrackingBackgroundService.cs
│   │   └── SmsProviders/
│   │       ├── ISmsProvider.cs
│   │       └── SunWaySmsProvider.cs
│   │
│   ├── ViewModels/                            # View Models
│   │   ├── UserViewModels/
│   │   ├── TaskViewModels/
│   │   ├── ContactViewModels/
│   │   ├── OrganizationViewModels/
│   │   ├── NotificationViewModels/
│   │   └── ...
│   │
│   ├── Enums/                                 # Enumerations
│   │   ├── ModuleType.cs
│   │   └── NotificationEventType.cs
│   │
│   ├── Extensions/                            # Extension Methods
│   │   ├── PersianDate.cs
│   │   ├── TaskCodeGenerator.cs
│   │   └── ...
│   │
│   ├── Configurations/                        # EF Configurations
│   │   ├── NotificationEntitiesConfiguration.cs
│   │   ├── ContactOrganizationConfiguration.cs
│   │   ├── CoreEntitiesConfiguration.cs
│   │   └── SeedDataConfiguration.cs
│   │
│   ├── Migrations/                            # EF Migrations
│   ├── AppDbContext.cs                        # DbContext اصلی
│   └── ...
│
└── MahERP.CommonLayer/                        # لایه Common Utilities
    ├── Repository/
    │   └── TelegramBotSendNotification.cs     # سرویس تلگرام
    ├── Interface/
    │   └── ITelegramBotSendNotification.cs
    ├── Helpers/
    │   └── PhoneNumberHelper.cs
    └── PublicClasses/
        ├── ResponseMessage.cs
        ├── ConvertDateTime.cs                  # تبدیل تاریخ شمسی/میلادی
        └── TempDataHelper.cs
```

---

## 🗄️ ساختار دیتابیس

### خلاصه جداول دیتابیس

#### 1️⃣ **احراز هویت و کاربران** (Identity)
```
AspNetUsers              → کاربران سیستم
AspNetRoles              → نقش‌ها
AppUsers                 → کاربران اپلیکیشن (توسعه یافته از Identity)
AppRoles                 → نقش‌های اپلیکیشن
```

#### 2️⃣ **مدیریت دسترسی** (Access Control)
```
Permission_Tbl           → دسترسی‌ها (ساختار درختی)
Role_Tbl                 → نقش‌های سفارشی
UserRole_Tbl             → نقش‌های کاربران
RolePermission_Tbl       → دسترسی‌های هر نقش
UserPermission_Tbl       → دسترسی‌های مستقیم کاربران
PermissionLog_Tbl        → لاگ دسترسی‌ها
PermissionChangeLog_Tbl  → لاگ تغییرات دسترسی
```

#### 3️⃣ **ساختار سازمانی** (Organization Structure)
```
Branch_Tbl               → شعب
BranchUser_Tbl           → کاربران شعبه
Team_Tbl                 → تیم‌ها
TeamMember_Tbl           → اعضای تیم
TeamPosition_Tbl         → سمت‌ها در تیم
Contract_Tbl             → قراردادها
```

#### 4️⃣ **مدیریت افراد و سازمان‌ها** (Contacts & Organizations) ⭐ NEW
```
Contact_Tbl              → افراد
ContactPhone_Tbl         → شماره تلفن‌های افراد
ContactGroup_Tbl         → گروه‌بندی افراد (سطح سیستم)
ContactGroupMember_Tbl   → اعضای گروه
BranchContact_Tbl        → افراد مرتبط با شعبه
BranchContactGroup_Tbl   → گروه‌های شعبه

Organization_Tbl         → سازمان‌ها
OrganizationDepartment_Tbl    → دپارتمان‌های سازمان
DepartmentPosition_Tbl   → سمت‌های دپارتمان
DepartmentMember_Tbl     → اعضای دپارتمان
OrganizationContact_Tbl  → ارتباط سازمان و افراد
OrganizationGroup_Tbl    → گروه‌بندی سازمان‌ها (سطح سیستم)
OrganizationGroupMember_Tbl → اعضای گروه سازمان
BranchOrganization_Tbl   → سازمان‌های مرتبط با شعبه
BranchOrganizationGroup_Tbl → گروه‌های سازمانی شعبه

Stakeholder_Tbl          → ⚠️ OBSOLETE (حفظ شده برای backward compatibility)
```

#### 5️⃣ **مدیریت تسک‌ها** (Task Management)
```
Tasks_Tbl                → تسک‌ها
TaskAssignment_Tbl       → اختصاص تسک به کاربران
  - AssignedInTeamId     → ⭐ تیمی که کاربر در آن assign شده
TaskOperation_Tbl        → عملیات‌های تسک
TaskOperationWorkLog_Tbl → گزارش کار عملیات
TaskComment_Tbl          → نظرات تسک
TaskAttachment_Tbl       → فایل‌های پیوست
TaskReminder_Tbl         → یادآورهای تسک
TaskReminderSchedule_Tbl → زمان‌بندی یادآورها
TaskMyDay_Tbl            → تسک‌های "روز من"
TaskCategory_Tbl         → دسته‌بندی تسک‌ها
BranchTaskCategory_Tbl   → دسته‌بندی‌های شعبه
TaskHistory_Tbl          → تاریخچه تغییرات
TaskWorkLog_Tbl          → گزارش کار روی تسک
TaskSchedule_Tbl         → زمان‌بندی خودکار تسک‌ها
TaskTemplate_Tbl         → قالب‌های تسک
TaskViewer_Tbl           → بینندگان تسک (رونوشت‌ها) ⭐
TaskViewPermission_Tbl   → دسترسی مشاهده تسک (مجوزهای خاص) ⭐
```

#### 6️⃣ **CRM**
```
CRMInteraction_Tbl       → تعاملات CRM
CRMComment_Tbl           → نظرات CRM
CRMAttachment_Tbl        → فایل‌های پیوست
CRMParticipant_Tbl       → شرکت‌کنندگان
CRMTeam_Tbl              → تیم‌های CRM
StakeholderCRM_Tbl       → ارتباط CRM با Stakeholder
TaskCRMDetails_Tbl       → ارتباط تسک و CRM
```

#### 7️⃣ **سیستم اعلان‌رسانی** (Notifications) ⭐ ADVANCED
```
CoreNotification_Tbl              → اعلان‌های سیستمی
CoreNotificationDetail_Tbl        → جزئیات اعلان
CoreNotificationDelivery_Tbl      → وضعیت تحویل (Email/SMS/Telegram)
CoreNotificationSetting_Tbl       → تنظیمات اعلان کاربران

NotificationTemplate_Tbl          → قالب‌های اعلان
NotificationTemplateRecipient_Tbl → دریافت‌کنندگان قالب
NotificationTemplateVariable_Tbl  → متغیرهای قالب
NotificationTemplateHistory_Tbl   → تاریخچه استفاده از قالب
NotificationScheduledMessage_Tbl  → پیام‌های زمان‌بندی شده
NotificationModuleConfig_Tbl      → تنظیمات ماژول‌ها
NotificationTypeConfig_Tbl        → تنظیمات انواع اعلان
NotificationBlacklist_Tbl         → لیست سیاه
UserNotificationPreference_Tbl    → ترجیحات کاربر
NotificationDeliveryStats_Tbl     → آمار ارسال
```

#### 8️⃣ **ایمیل** (Email System)
```
EmailQueue_Tbl           → صف ایمیل‌ها
EmailLog_Tbl             → لاگ ارسال ایمیل
EmailTemplate_Tbl        → قالب‌های ایمیل
EmailTemplateRecipient_Tbl → دریافت‌کنندگان قالب ایمیل
```

#### 9️⃣ **پیامک** (SMS System)
```
SmsQueue_Tbl             → صف پیامک‌ها
SmsLog_Tbl               → لاگ ارسال پیامک
SmsProvider_Tbl          → ارائه‌دهندگان پیامک
SmsTemplate_Tbl          → قالب‌های پیامک
SmsTemplateRecipient_Tbl → دریافت‌کنندگان قالب پیامک
```

#### 🔟 **فعالیت‌ها** (Activity Tracking)
```
ActivityBase_Tbl         → فعالیت‌های پایه
ActivityTask_Tbl         → فعالیت‌های تسک
ActivityCRM_Tbl          → فعالیت‌های CRM
ActivityHistory_Tbl      → تاریخچه فعالیت
ActivityComment_Tbl      → نظرات فعالیت
ActivityAttachment_Tbl   → پیوست‌های فعالیت
UserActivityLog_Tbl      → لاگ فعالیت کاربران
```

#### 1️⃣1️⃣ **دسترسی ماژول‌ها** (Module Access)
```
UserModulePermission_Tbl → دسترسی کاربر به ماژول‌ها
TeamModulePermission_Tbl → دسترسی تیم به ماژول‌ها
BranchModulePermission_Tbl → دسترسی شعبه به ماژول‌ها
UserModulePreference_Tbl → تنظیمات ماژول کاربر
```

#### 1️⃣2️⃣ **تنظیمات** (Settings)
```
Settings_Tbl             → تنظیمات سیستم
```

---

### روابط کلیدی دیتابیس

#### ⭐ **رابطه Contact و Organization**
```sql
-- یک Contact می‌تواند در چند Organization عضو باشد
OrganizationContact (Many-to-Many)
  - ContactId → Contact_Tbl
  - OrganizationId → Organization_Tbl
  - Position (سمت در سازمان)
```

#### ⭐ **رابطه Task و Contact/Organization**
```sql
-- تسک می‌تواند به یک Contact یا یک Organization مرتبط باشد
Tasks_Tbl
  - ContactId → Contact_Tbl (Nullable)
  - OrganizationId → Organization_Tbl (Nullable)
  - StakeholderId → Stakeholder_Tbl (Obsolete, برای backward compatibility)
```

#### ⭐ **رابطه User و Permission**
```sql
-- روش 1: دسترسی از طریق نقش (Role-Based)
User → UserRole → Role → RolePermission → Permission

-- روش 2: دسترسی مستقیم (Direct Permission)
User → UserPermission → Permission

-- اولویت: دسترسی مستقیم > دسترسی از طریق نقش
```

#### ⭐ **رابطه Notification و Delivery**
```sql
-- یک اعلان می‌تواند از طریق چند کانال ارسال شود
CoreNotification → CoreNotificationDelivery
  - DeliveryMethod: 1=Email, 2=SMS, 3=Telegram
  - DeliveryStatus: 0=Pending, 1=Sent, 2=Delivered, 3=Failed
```

---

## 📦 ماژول‌های سیستم

سیستم MahERP دارای **3 ماژول اصلی** است:

### 1️⃣ **Core Module** (ماژول هسته مرکزی)
- **URL**: `/AppCoreArea/Dashboard/Index`
- **رنگ**: Primary (آبی)
- **آیکون**: `fa fa-home`

#### امکانات:
- ✅ مدیریت کاربران و نقش‌ها
- ✅ مدیریت شعب و قراردادها
- ✅ مدیریت افراد و سازمان‌ها
- ✅ مدیریت تیم‌ها و سمت‌ها سازمانی
- ✅ مدیریت دسترسی‌ها (Permission System)
- ✅ گزارش فعالیت‌های کاربران
- ✅ تنظیمات سیستم
- ✅ مدیریت دسترسی به ماژول‌ها

### 2️⃣ **Tasking Module** (ماژول مدیریت تسک‌ها)
- **URL**: `/TaskingArea/Dashboard/Index`
- **رنگ**: Success (سبز)
- **آیکون**: `fa fa-tasks`

#### امکانات:
- ✅ ایجاد و مدیریت تسک‌ها
- ✅ اختصاص تسک به کاربران/تیم‌ها
- ✅ مدیریت عملیات‌های تسک
- ✅ گزارش کار (Work Log)
- ✅ نظرات و پیوست‌ها
- ✅ یادآورهای تسک
- ✅ "روز من" (My Day) - مدیریت تسک‌های روزانه
- ✅ زمان‌بندی خودکار تسک‌ها
- ✅ تسک‌های قالب‌بندی شده (Template)
- ✅ دسته‌بندی تسک‌ها
- ✅ تقویم و نمای Kanban
- ✅ گزارش‌گیری و آمار

### 3️⃣ **CRM Module** (ماژول مدیریت ارتباط با مشتری)
- **URL**: `/CrmArea/Dashboard/Index`
- **رنگ**: Info (آبی روشن)
- **آیکون**: `fa fa-chart-line`

#### امکانات:
- ✅ مدیریت تعاملات با مشتریان
- ✅ ثبت نظرات و فایل‌های پیوست
- ✅ مدیریت شرکت‌کنندگان در تعاملات
- ✅ ارسال ایمیل و پیامک دسته‌جمعی
- ✅ قالب‌های ایمیل و پیامک
- ✅ آمار و گزارش‌های CRM
- ✅ یکپارچگی با تسک‌ها

---

## 👁️ سیستم نظارت بر تسک‌ها

### معرفی

سیستم MahERP دارای یک **سیستم نظارت هوشمند و چندسطحی** است که به صورت خودکار مشخص می‌کند چه کسانی می‌توانند تسک‌های دیگران را مشاهده کنند.

### انواع نظارت

#### ✅ **1. نظارت سیستمی (System Supervision)**
نظارتی که به صورت خودکار بر اساس ساختار سازمانی تعیین می‌شود:

##### 🔹 **الف) نظارت بر اساس سمت در تیم (Position-Based)**
```csharp
// مثال: مدیر تیم می‌تواند تسک‌های اعضای تیم را ببیند
TeamPosition
  - PowerLevel: 1           // سطح قدرت (کمتر = بالاتر)
  - CanViewSubordinateTasks: true   // مشاهده تسک‌های زیردستان
  - CanViewPeerTasks: true          // مشاهده تسک‌های همسطح
```

**منطق:**
```
PowerLevel کمتر = قدرت بیشتر

مثال:
- مدیر تیم: PowerLevel = 1
- سرپرست: PowerLevel = 2
- کارمند: PowerLevel = 3

→ مدیر می‌تواند تسک‌های سرپرست و کارمند را ببیند
→ سرپرست می‌تواند فقط تسک‌های کارمند را ببیند
```

##### 🔹 **ب) نظارت رسمی (Formal Supervision)**
```csharp
TeamMember
  - MembershipType: 1  // 0=عضو عادی, 1=ناظر رسمی
```

**منطق:**
```
اگر MembershipType = 1 → ناظر رسمی
→ می‌تواند تمام تسک‌های اعضای عادی (MembershipType=0) تیم را ببیند
```

##### 🔹 **ج) مدیریت تیم (Team Management)**
```csharp
Team
  - ManagerUserId: "user-id"  // مدیر تیم
```

**منطق:**
```
مدیر تیم می‌تواند:
✅ تمام تسک‌های تیم خودش را ببیند
✅ تسک‌های زیرتیم‌ها را ببیند (اگر تیم زیرتیم داشته باشد)
```

##### 🔹 **د) سازنده تسک (Task Creator)**
```
هر کسی که تسک را ایجاد کرده، به صورت خودکار ناظر آن است.
```

#### ✅ **2. نظارت رونوشتی (Carbon Copy Supervision)**
نظارتی که به صورت دستی توسط سازنده تسک تعیین می‌شود:

```csharp
TaskViewer_Tbl
  - TaskId          → شناسه تسک
  - UserId          → کاربر رونوشت شده
  - AddedByUserId   → کسی که رونوشت را اضافه کرده
  - AddedDate       → تاریخ اضافه شدن
  - Note            → یادداشت
  - IsActive        → فعال/غیرفعال
  - StartDate       → تاریخ شروع اعتبار
  - EndDate         → تاریخ پایان اعتبار
```

**منطق:**
```
سازنده تسک می‌تواند کاربران دیگری را به صورت دستی به عنوان ناظر اضافه کند.
→ این افراد تسک را می‌بینند اما لزوماً دسترسی ویرایش ندارند.
```

#### ✅ **3. مجوز خاص (Special Permission)**
دسترسی ویژه برای مشاهده تسک‌های خاص:

```csharp
TaskViewPermission_Tbl
  - GranteeUserId       → کاربر گیرنده دسترسی
  - PermissionType      → نوع مجوز:
      • 0 = مشاهده تسک‌های یک کاربر خاص
      • 1 = مشاهده تسک‌های یک تیم خاص
      • 2 = مشاهده تسک‌های تیم و زیرتیم‌ها
  - TargetUserId        → کاربر هدف (برای نوع 0)
  - TargetTeamId        → تیم هدف (برای نوع 1 و 2)
  - IsActive            → فعال/غیرفعال
  - StartDate           → تاریخ شروع
  - EndDate             → تاریخ پایان
```

### الگوریتم محاسبه ناظران یک تسک

```csharp
// متد: GetVisibleTaskIdsAsync

// مراحل:
1️⃣ دریافت شعبه‌های کاربر
   → BranchUser_Tbl

2️⃣ تسک‌های ساخته شده توسط کاربر
   → Creator = userId

3️⃣ تسک‌های منتصب شده به کاربر
   → TaskAssignment.AssignedUserId = userId

4️⃣ تسک‌های تیم‌های تحت مدیریت مستقیم
   → Team.ManagerUserId = userId

5️⃣ تسک‌های بر اساس سمت در تیم ⭐ **کلیدی**
   foreach (تیم کاربر)
   {
       // 5.1 زیردستان
       if (Position.CanViewSubordinateTasks)
       {
           subordinateUserIds = کاربران با PowerLevel > من
           tasks = TaskAssignment که:
             - AssignedUserId در subordinateUserIds
             - AssignedInTeamId = تیم فعلی  ⭐ **فیلتر تیم**
       }
       
       // 5.2 همسطح‌ها
       if (Position.CanViewPeerTasks)
       {
           peerUserIds = کاربران با PowerLevel = من
           tasks = TaskAssignment که:
             - AssignedUserId در peerUserIds
             - AssignedInTeamId = تیم فعلی  ⭐ **فیلتر تیم**
       }
       
       // 5.3 ناظر رسمی
       if (MembershipType = 1)
       {
           normalMemberIds = اعضای عادی تیم
           tasks = TaskAssignment که:
             - AssignedUserId در normalMemberIds
             - AssignedInTeamId = تیم فعلی  ⭐ **فیلتر تیم**
       }
   }

6️⃣ تسک‌های با مجوز خاص
   → TaskViewPermission_Tbl

7️⃣ تسک‌های رونوشت
   → TaskViewer_Tbl (IsActive + تاریخ اعتبار)
```

### نکته کلیدی: فیلتر تیم

```csharp
// ❌ قبل از اصلاح:
var tasks = TaskAssignment که AssignedUserId در subordinateUserIds

// ✅ بعد از اصلاح:
var tasks = TaskAssignment که:
  - AssignedUserId در subordinateUserIds
  - AssignedInTeamId = تیم فعلی
```

**چرا؟**
```
مثال:
- شما: مدیر حسین در تیم "بازاریابی"
- حسین: عضو تیم "بازاریابی" و "فروش"

❌ بدون فیلتر تیم:
   → می‌بینید: تسک‌های حسین در "بازاریابی" + "فروش"

✅ با فیلتر تیم:
   → می‌بینید: فقط تسک‌های حسین در "بازاریابی"
```

### کلاس‌های Repository

#### 🔹 **1. TaskVisibilityRepository**
مسئول منطق نظارت:

```csharp
// متدهای کلیدی:
Task<bool> CanUserViewTaskAsync(string userId, int taskId)
Task<List<int>> GetVisibleTaskIdsAsync(string userId, int? branchId, int? teamId)
Task<bool> CanViewBasedOnPositionAsync(string userId, Tasks task)
Task<bool> IsUserTeamManagerAsync(string userId, int teamId)
Task<List<string>> GetTaskSupervisorsAsync(int taskId, bool includeCreator)
Task<List<string>> GetUserSupervisorsInTeamAsync(string userId, int teamId, int branchId)
```

#### 🔹 **2. TaskFilteringRepository**
فیلتر کردن تسک‌ها بر اساس نوع نمایش:

```csharp
// متدهای کلیدی:
Task<List<Tasks>> GetMyTasksAsync(string userId, TaskFilterViewModel filters)
Task<List<Tasks>> GetAssignedByMeTasksAsync(string userId, TaskFilterViewModel filters)
Task<List<Tasks>> GetSupervisedTasksAsync(string userId, TaskFilterViewModel filters)  ⭐
Task<List<Tasks>> GetAllVisibleTasksAsync(string userId, TaskFilterViewModel filters)
```

#### 🔹 **3. TaskGroupingRepository**
گروه‌بندی و نمایش تسک‌ها:

```csharp
// متدهای کلیدی:
Task<List<TaskGroupViewModel>> GroupTasksAsync(List<Tasks> tasks, TaskGroupingType grouping, string currentUserId, TaskViewType? viewType)
(string supervisionType, string supervisionReason) GetSupervisionTypeAndReason(int taskId, string userId)  ⭐
```

### نمایش دلیل نظارت

```csharp
// متد: GetSupervisionTypeAndReason

// خروجی:
("system", "شما مدیر تیم «بازاریابی» هستید")
("system", "شما در تیم «بازاریابی» سمت بالاتر از حسین، علی دارید")
("system", "شما ناظر رسمی تیم «فروش» هستید")
("carbon-copy", "شما توسط مدیر در تاریخ 1403/10/15 به این تسک رونوشت شده‌اید")
```

**الگوریتم:**
```
1️⃣ بررسی رونوشت (TaskViewer)
   → اگر یافت شد: return ("carbon-copy", توضیحات)

2️⃣ بررسی نظارت سیستمی:
   
   ✅ سازنده تسک
   ✅ مدیر تیم تسک
   ✅ مدیر تیم اعضای assigned
   ✅ سمت بالاتر از اعضای assigned
   ✅ ناظر رسمی تیم اعضای assigned
   ✅ سمت بالاتر در تیم‌های کاربر (حتی اگر assigned نباشند)
   ✅ ناظر رسمی در تیم‌های کاربر

3️⃣ ترکیب دلایل:
   → اگر چند دلیل: "دلیل1 و دلیل2 و ..."
   → اگر هیچ دلیلی: "ناظر سیستمی"
```

### مثال عملی

#### سناریو:
```
تیم بازاریابی:
- مدیر: علی (PowerLevel = 1, CanViewSubordinateTasks = true)
- سرپرست: حسین (PowerLevel = 2)
- کارمند: مهدی (PowerLevel = 3)

تیم فروش:
- عضو: حسین (همان حسین)

تسک: "تماس با مشتری X"
- Assigned to: حسین
- AssignedInTeamId: تیم بازاریابی (ID=5)
```

#### نتیجه:
```
✅ علی می‌تواند تسک را ببیند چون:
   - مدیر تیم بازاریابی است
   - سمت بالاتر از حسین دارد (PowerLevel: 1 < 2)
   
❌ مدیر تیم فروش نمی‌تواند این تسک را ببیند چون:
   - AssignedInTeamId = تیم بازاریابی
   - تسک در تیم فروش assign نشده
```

### انواع نمایش تسک‌ها (TaskViewType)

```csharp
public enum TaskViewType
{
    MyTasks = 0,           // تسک‌های من (assigned به من + ساخته شده توسط من)
    AssignedByMe = 1,      // تسک‌های اختصاص داده شده توسط من
    Supervised = 2,        // تسک‌های نظارتی (تسک‌های دیگران که می‌توانم ببینم) ⭐
    AllVisible = 3,        // همه تسک‌های قابل مشاهده
    AssignedToMe = 4,      // فقط تسک‌های assign شده به من
    TeamTasks = 5          // تسک‌های تیمی
}
```

### Controller و Action

```csharp
// نمایش تسک‌های نظارتی
[HttpGet]
public async Task<IActionResult> SupervisedTasks(TaskFilterViewModel filters)
{
    var userId = _userManager.GetUserId(User);
    
    var model = await _taskRepository.GetTaskListAsync(
        userId, 
        TaskViewType.Supervised,  // ⭐ نمایش نظارتی
        TaskGroupingType.Team, 
        filters
    );
    
    return View(model);
}
```

### View Model

```csharp
public class TaskCardViewModel
{
    // ...existing properties...
    
    // ⭐ برای تسک‌های نظارتی:
    public string SupervisionType { get; set; }       // "system" | "carbon-copy"
    public string SupervisionReason { get; set; }     // توضیحات دلیل نظارت
}
```

### نمایش در UI

```html
<!-- کارت تسک -->
@if (Model.ViewType == TaskViewType.Supervised && !string.IsNullOrEmpty(task.SupervisionReason))
{
    <div class="supervision-badge">
        <i class="fa fa-eye"></i>
        <span>@task.SupervisionReason</span>
    </div>
}
```

---

## 🔐 سیستم مدیریت کاربران و دسترسی

### معماری سیستم دسترسی

سیستم MahERP از یک **سیستم دسترسی سلسله‌مراتبی و چندسطحی** استفاده می‌کند:

#### ✅ **1. سطح اول: Permission (دسترسی‌ها)**
```csharp
// ساختار درختی دسترسی‌ها
Permission
  - Id
  - Code (مانند: "TASK.CREATE", "USER.VIEW")
  - NameEn
  - NameFa
  - ParentId (برای ساختار درختی)
  - IsActive
  - IsSystemPermission (دسترسی سیستمی - قابل حذف نیست)
```

**مثال‌های دسترسی:**
```
CORE.VIEW           → مشاهده ماژول Core
CORE.PERMISSION     → مدیریت دسترسی‌ها
CORE.ROLE           → مدیریت نقش‌ها
TASK.CREATE         → ایجاد تسک
TASK.EDIT           → ویرایش تسک
TASK.DELETE         → حذف تسک
TASK.SUPERVISE      → ⭐ نظارت بر تسک‌های دیگران (جدید)
CRM.VIEW            → مشاهده ماژول CRM
```

#### ✅ **2. سطح دوم: Role (نقش‌ها)**
```csharp
Role
  - Id
  - NameEn
  - NameFa
  - Description
  - Priority
  - Color
  - Icon
  - IsSystemRole (نقش سیستمی)
  - IsActive
```

**نمونه نقش‌ها:**
- 👔 **مدیر کل**: دسترسی کامل به همه بخش‌ها
- 👤 **مدیر تیم**: مدیریت تسک‌ها و اعضای تیم + نظارت بر تسک‌های زیرستان
- 👨‍💼 **سرپرست**: نظارت محدود بر تسک‌های تیم
- 👨‍💻 **کاربر عادی**: دسترسی محدود به تسک‌های خود
- 📞 **کارشناس CRM**: دسترسی به ماژول CRM

#### ✅ **3. سطح سوم: RolePermission**
تعیین می‌کند هر نقش چه دسترسی‌هایی دارد:
```csharp
RolePermission
  - RoleId
  - PermissionId
  - AssignDate
  - AssignedByUserId
```

#### ✅ **4. سطح چهارم: UserRole**
کاربران به نقش‌ها اختصاص داده می‌شوند:
```csharp
UserRole
  - UserId
  - RoleId
  - IsActive
  - StartDate (تاریخ شروع اعتبار)
  - EndDate (تاریخ پایان اعتبار)
  - AssignDate
```

#### ✅ **5. سطح پنجم: UserPermission (دسترسی مستقیم)**
دسترسی‌های ویژه برای یک کاربر خاص:
```csharp
UserPermission
  - UserId
  - PermissionId
  - IsActive
  - AssignDate
```

### الگوریتم بررسی دسترسی

```
1. آیا کاربر Admin است؟
   → بله: دسترسی کامل ✅
   → خیر: ادامه

2. آیا دسترسی مستقیم (UserPermission) دارد؟
   → بله: بررسی IsActive
     → Active: دسترسی تایید ✅
     → Inactive: دسترسی رد ❌
   → خیر: ادامه

3. آیا از طریق نقش (Role) دسترسی دارد؟
   → بله: بررسی IsActive در UserRole و RolePermission
     → Active: دسترسی تایید ✅
     → Inactive: دسترسی رد ❌
   → خیر: دسترسی رد ❌
```

### Attribute بررسی دسترسی

```csharp
// روش 1: ساده
[PermissionRequired("TASK.CREATE")]
public IActionResult CreateTask() { ... }

// روش 2: کامل (با پارامترها)
[Permission("Task", "Create", 1)] // 1=Create
public IActionResult CreateTask() { ... }
```

### مدیریت دسترسی ماژول‌ها

علاوه بر Permission System، یک لایه دیگر برای مدیریت دسترسی به ماژول‌ها وجود دارد:

```csharp
// بررسی دسترسی کاربر به یک ماژول
var accessResult = await moduleAccessService.CheckUserModuleAccessAsync(
    userId, 
    ModuleType.Tasking
);

// اولویت: User > Team > Branch
// 1. بررسی UserModulePermission
// 2. بررسی TeamModulePermission
// 3. بررسی BranchModulePermission
```

---

## 🔔 سیستم اعلان‌رسانی

سیستم MahERP دارای یک **سیستم اعلان‌رسانی پیشرفته و چندکاناله** است.

### معماری سیستم اعلان

#### 🔹 **1. CoreNotification (اعلان‌های سیستمی)**
```csharp
CoreNotification
  - SystemId (1-7: مالی، منابع انسانی، CRM، تدارکات، انبار، تولید، تسکینگ)
  - RecipientUserId
  - SenderUserId
  - NotificationTypeGeneral (0-10: اطلاع‌رسانی، ایجاد، ویرایش، حذف، تایید، هشدار، ...)
  - Title
  - Message
  - ActionUrl
  - RelatedRecordId
  - Priority (0-3: عادی، مهم، فوری، بحرانی)
  - IsRead
  - IsClicked
```

#### 🔹 **2. CoreNotificationDelivery (وضعیت ارسال)**
```csharp
CoreNotificationDelivery
  - CoreNotificationId
  - DeliveryMethod (1=Email, 2=SMS, 3=Telegram)
  - DeliveryAddress
  - DeliveryStatus (0=Pending, 1=Sent, 2=Delivered, 3=Failed)
  - AttemptCount
  - DeliveryDate
  - ErrorMessage
```

#### 🔹 **3. NotificationTemplate (قالب‌های اعلان)**
```csharp
NotificationTemplate
  - TemplateName
  - NotificationEventType (1-20: TaskAssigned, TaskCompleted, ...)
  - Channel (1=Email, 2=SMS, 3=Telegram)
  - Subject
  - MessageTemplate (با متغیرهای پویا: {{TaskTitle}}, {{UserFullName}}, ...)
  - RecipientMode (0=همه, 1=خاص, 2=همه به جز...)
  - ScheduleType (0=بلافاصله, 1=روزانه, 2=هفتگی)
  - IsActive
```

### انواع رویدادهای اعلان

```csharp
public enum NotificationEventType
{
    TaskAssigned = 1,           // تسک اختصاص داده شد
    TaskCompleted = 2,          // تسک تکمیل شد
    TaskDeadlineReminder = 3,   // یادآوری سررسید
    TaskCommentAdded = 4,       // کامنت جدید
    TaskUpdated = 5,            // تسک ویرایش شد
    TaskDeleted = 6,            // تسک حذف شد
    TaskReassigned = 7,         // تسک مجدداً اختصاص داده شد
    TaskStatusChanged = 8,      // تغییر وضعیت
    TaskOperationCompleted = 9, // عملیات تکمیل شد
    OperationAssigned = 10,     // عملیات اختصاص داده شد
    CommentMentioned = 11,      // منشن در کامنت
    TaskPriorityChanged = 12,   // تغییر اولویت
    DailyTaskDigest = 13,       // خلاصه روزانه تسک‌ها
    TaskWorkLog = 14,           // گزارش کار ثبت شد
    // ... و 20+ رویداد دیگر
}
```

### کانال‌های ارسال

#### 📧 **1. Email**
- صف ارسال: `EmailQueue_Tbl`
- Background Service: `EmailBackgroundService`
- قالب‌ها: `EmailTemplate_Tbl`

```csharp
// ارسال ایمیل
await _emailRepository.SendToMultipleContactsAsync(
    contactIds, 
    subject, 
    body, 
    userId, 
    isHtml: true
);
```

#### 📱 **2. SMS**
- صف ارسال: `SmsQueue_Tbl`
- Background Service: `SmsBackgroundService`
- ارائه‌دهندگان: `SmsProvider_Tbl` (مثلاً SunWay)

```csharp
// ارسال پیامک
await _smsService.SendToMultipleContactsAsync(
    contactIds, 
    message, 
    userId, 
    providerId
);
```

#### ✈️ **3. Telegram**
- Webhook: `/TelegramWebhook/Update`
- Polling Service: `TelegramPollingBackgroundService`
- دکمه‌های پویا (Dynamic Buttons) برای تسک‌ها

```csharp
// ارسال تلگرام با دکمه‌های پویا
await _telegramService.SendNotificationAsync(
    message, 
    chatId, 
    botToken, 
    notificationContext  // شامل TaskId, EventType, ...
);
```

### Background Services

#### 🔄 **1. NotificationProcessingBackgroundService**
- پردازش اعلان‌های رویدادی (Event-based)
- ارسال فوری اعلان‌ها

#### ⏰ **2. ScheduledNotificationBackgroundService**
- پردازش اعلان‌های زمان‌بندی شده
- اجرای دوره‌ای (Daily, Weekly, Monthly)
- مثال: خلاصه روزانه تسک‌ها

```csharp
// تنظیم زمان‌بندی روزانه
NotificationTemplate
{
    ScheduleType = 1,  // Daily
    ScheduledTime = new TimeSpan(9, 0, 0),  // 9:00 AM
    DailyFrequency = 1
}
```

### متغیرهای پویا در قالب‌ها

```
{{RecipientFullName}}    → نام کامل دریافت‌کننده
{{TaskTitle}}            → عنوان تسک
{{TaskCode}}             → کد تسک
{{TaskDueDate}}          → مهلت تسک
{{TaskPriority}}         → اولویت تسک
{{SenderName}}           → نام فرستنده
{{ActionUrl}}            → لینک تسک
{{Date}}                 → تاریخ امروز
{{Time}}                 → ساعت فعلی
{{PendingTasks}}         → لیست تسک‌های انجام نشده (برای Digest)
```

**مثال قالب:**
```
سلام {{RecipientFullName}} عزیز،

تسک «{{TaskTitle}}» توسط {{SenderName}} به شما اختصاص داده شد.

📅 مهلت: {{TaskDueDate}}
⚡ اولویت: {{TaskPriority}}

🔗 مشاهده تسک: {{ActionUrl}}
```

### SignalR (Real-time Notifications)

```javascript
// اتصال به Hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// دریافت اعلان
connection.on("ReceiveNotification", (notification) => {
    // نمایش اعلان
});
```

---

## 📊 آمار و ویژگی‌های سیستم

### خلاصه آمار

```
📁 تعداد کل جداول دیتابیس: 100+
🎨 تعداد کنترلرها: 50+
📄 تعداد View ها: 200+
🔧 تعداد Repository ها: 35+
⚙️ تعداد Background Service ها: 7
📨 کانال‌های ارتباطی: 3 (Email, SMS, Telegram)
🌐 زبان‌های پشتیبانی شده: فارسی (RTL)
🗓️ پشتیبانی تاریخ: میلادی و شمسی
👁️ سیستم نظارت: 3 نوع (سیستمی، رونوشتی، مجوز خاص) ⭐
```

### نکات امنیتی

```
✅ استفاده از ASP.NET Core Identity
✅ سیستم Authorization سفارشی (Permission-based)
✅ Protection از CSRF Attacks
✅ Password Hashing (SHA-256)
✅ ثبت تمام فعالیت‌ها در UserActivityLog
✅ ثبت تمام تغییرات دسترسی در PermissionLog
✅ محدودیت نظارت بر اساس تیم (Team-scoped supervision) ⭐
```

---

## 🚀 امکانات پیشرفته

### ✅ **1. Task Management**
- اختصاص چندگانه (به کاربران یا تیم‌ها)
- **نظارت هوشمند بر اساس ساختار سازمانی** ⭐ **جدید**
- **نظارت رونوشتی (Carbon Copy)** ⭐ **جدید**
- **تشخیص خودکار دلیل نظارت** ⭐ **جدید**
- مدیریت عملیات‌های تسک
- گزارش کار (Work Log)
- "روز من" (My Day)
- تقویم و Timeline
- یادآورهای هوشمند
- تسک‌های زمان‌بندی شده
- قالب‌های تسک
- فیلتر تسک‌ها بر اساس نوع نظارت

### ✅ **2. CRM**
- مدیریت تعاملات
- ارسال دسته‌جمعی
- قالب‌های پیش‌فرض
- آمار و گزارش‌ها

### ✅ **3. Notification System**
- سیستم قالب‌بندی پیشرفته
- متغیرهای پویا
- زمان‌بندی خودکار
- ارسال چندکاناله
- لیست سیاه (Blacklist)
- ترجیحات کاربر

### ✅ **4. Module Access Management**
- مدیریت دسترسی سطح ماژول
- اولویت‌بندی (User > Team > Branch)
- گزارش دسترسی‌ها

### ✅ **5. Task Supervision System** ⭐ **جدید**
- نظارت خودکار بر اساس سمت
- نظارت بر اساس مدیریت تیم
- نظارت رونوشتی (دستی)
- مجوزهای خاص نظارتی
- تشخیص و نمایش دلیل نظارت
- فیلتر محدود به تیم (Team-scoped)

---

## 📝 نتیجه‌گیری

سیستم MahERP یک **سیستم ERP جامع و مدرن** است که با استفاده از **آخرین تکنولوژی‌های .NET** طراحی شده است. این سیستم قابلیت‌های زیر را ارائه می‌دهد:

✅ معماری لایه‌ای و قابل توسعه
✅ سیستم احراز هویت و مجوزدهی پیشرفته
✅ **سیستم نظارت هوشمند بر تسک‌ها** ⭐ **جدید**
✅ مدیریت تسک‌ها با امکانات کامل
✅ CRM یکپارچه
✅ سیستم اعلان‌رسانی چندکاناله
✅ پشتیبانی کامل از زبان فارسی
✅ قابلیت سفارشی‌سازی بالا

---

**📌 نکته:** این مستند بر اساس کد منبع پروژه MahERP تهیه شده و ممکن است در نسخه‌های آینده تغییرات اعمال شود.

**نسخه مستند:** 2.0.0 ⭐ **(به‌روزرسانی شده با سیستم نظارت)**
**تاریخ بروزرسانی:** دی ماه 1403 (اضافه شدن سیستم نظارت پیشرفته)
