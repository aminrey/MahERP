# 📊 نمودارها و دیاگرام‌های سیستم MahERP

## 📋 فهرست
1. [نمودار ERD کلی دیتابیس](#نمودار-erd-کلی)
2. [نمودار سیستم دسترسی](#نمودار-سیستم-دسترسی)
3. [نمودار سیستم تسک](#نمودار-سیستم-تسک)
4. [نمودار سیستم نظارت بر تسک‌ها](#نمودار-سیستم-نظارت-بر-تسکها) ⭐ **جدید**
5. [نمودار سیستم تسک‌های زمان‌بندی شده](#نمودار-سیستم-تسکهای-زمانبندی-شده) 🆕 **جدیدترین**
6. [نمودار سیستم اعلان‌رسانی](#نمودار-سیستم-اعلانرسانی)
7. [نمودار Background Services و زمان‌بندی](#نمودار-background-services-و-زمانبندی) ⭐ **به‌روزرسانی شده**
8. [نمودار جریان کاربر](#نمودار-جریان-کاربر)

---

## 🗄️ نمودار ERD کلی

### ساختار کلی جداول

```mermaid
erDiagram
    %% ========== Users & Authentication ==========
    AppUsers ||--o{ UserRole : has
    AppUsers ||--o{ UserPermission : has
    AppUsers ||--o{ BranchUser : "works in"
    AppUsers ||--o{ TeamMember : "member of"
    AppUsers ||--o{ Tasks : creates
    AppUsers ||--o{ TaskAssignment : "assigned to"
    AppUsers ||--o{ CoreNotification : receives
    
    %% ========== Permissions ==========
    Permission ||--o{ Permission : "parent-child"
    Permission ||--o{ RolePermission : "used in"
    Permission ||--o{ UserPermission : "assigned to"
    
    Role ||--o{ RolePermission : contains
    Role ||--o{ UserRole : "assigned via"
    
    %% ========== Organization Structure ==========
    Branch ||--o{ BranchUser : employs
    Branch ||--o{ BranchContact : has
    Branch ||--o{ BranchOrganization : has
    Branch ||--o{ Tasks : manages
    
    Team ||--o{ Team : "parent-child (sub-teams)"
    Team ||--|| AppUsers : "managed by"
    Team ||--o{ TeamMember : contains
    Team ||--o{ TeamPosition : has
    Team ||--o{ Tasks : owns
    
    TeamMember ||--|| AppUsers : is
    TeamMember ||--o| TeamPosition : "has position"
    
    TeamPosition ||--o{ TeamMember : fills
    
    %% ========== Contacts & Organizations (NEW) ==========
    Contact ||--o{ ContactPhone : has
    Contact ||--o{ ContactGroupMember : "member of"
    Contact ||--o{ BranchContact : "in branch"
    Contact ||--o{ OrganizationContact : "works at"
    Contact ||--o{ Tasks : "related to"
    
    Organization ||--o{ OrganizationDepartment : has
    Organization ||--o{ OrganizationContact : employs
    Organization ||--o{ OrganizationGroupMember : "member of"
    Organization ||--o{ BranchOrganization : "in branch"
    Organization ||--o{ Tasks : "related to"
    
    OrganizationDepartment ||--o{ DepartmentPosition : has
    DepartmentPosition ||--o{ DepartmentMember : fills
    
    ContactGroup ||--o{ ContactGroupMember : contains
    OrganizationGroup ||--o{ OrganizationGroupMember : contains
    
    %% ========== Tasks & Supervision (UPDATED) ==========
    Tasks ||--o{ TaskAssignment : "assigned via"
    Tasks ||--o{ TaskOperation : contains
    Tasks ||--o{ TaskComment : has
    Tasks ||--o{ TaskAttachment : has
    Tasks ||--o{ TaskReminder : has
    Tasks ||--o{ TaskWorkLog : tracks
    Tasks ||--o{ TaskViewer : "visible to (carbon copy)"
    Tasks ||--o{ CoreNotification : generates
    
    Tasks ||--o| ScheduledTaskCreation : "created by schedule 🆕"
    
    TaskAssignment ||--|| AppUsers : "assigned to"
    TaskAssignment ||--o| Team : "in team context"
    
    TaskViewer ||--|| AppUsers : "viewer user"
    TaskViewer ||--|| AppUsers : "added by user"
    
    TaskViewPermission ||--|| AppUsers : "grantee user"
    TaskViewPermission ||--o| AppUsers : "target user"
    TaskViewPermission ||--o| Team : "target team"
    
    TaskOperation ||--o{ TaskOperationWorkLog : tracks
    
    TaskCategory ||--o{ Tasks : categorizes
    TaskSchedule ||--o{ Tasks : "generates automatically"
    
    %% ========== Scheduled Task Creation (NEW) 🆕 ==========
    ScheduledTaskCreation ||--|| AppUsers : "created by"
    ScheduledTaskCreation ||--|| AppUsers : "modified by"
    ScheduledTaskCreation ||--o{ Tasks : "generates tasks 🆕"
    
    %% ========== Notifications ==========
    CoreNotification ||--o{ CoreNotificationDetail : "has details"
    CoreNotification ||--o{ CoreNotificationDelivery : "delivered via"
    
    NotificationTemplate ||--o{ NotificationTemplateRecipient : "sent to"
    NotificationTemplate ||--o{ NotificationTemplateVariable : uses
    NotificationTemplate ||--o{ NotificationTemplateHistory : tracks
    
    %% ========== Communication ==========
    EmailQueue ||--|| EmailLog : "logged as"
    SmsQueue ||--|| SmsLog : "logged as"
    
    EmailTemplate ||--o{ EmailTemplateRecipient : "sent to"
    SmsTemplate ||--o{ SmsTemplateRecipient : "sent to"
    
    SmsProvider ||--o{ SmsQueue : "sends via"
    
    %% ========== CRM ==========
    CRMInteraction ||--o{ CRMComment : has
    CRMInteraction ||--o{ CRMAttachment : has
    CRMInteraction ||--o{ CRMParticipant : involves
    CRMInteraction ||--o{ CRMTeam : "managed by"
```

---

## 🔐 نمودار سیستم دسترسی

### نحوه کارکرد Permission System

```mermaid
graph TB
    %% Main Flow
    User[👤 User] -->|has| UserRole[UserRole]
    User -->|has| UserPermission[Direct Permission]
    
    UserRole -->|belongs to| Role[🎭 Role]
    Role -->|contains| RolePermission[RolePermission]
    
    UserPermission -->|grants| Permission[🔑 Permission]
    RolePermission -->|grants| Permission
    
    Permission -->|has parent| Permission
    
    %% Priority Check
    CheckAccess{Check Access?} --> IsAdmin{Is Admin?}
    IsAdmin -->|Yes ✅| GrantAccess[✅ Grant Access]
    IsAdmin -->|No| HasDirectPermission{Has Direct Permission?}
    
    HasDirectPermission -->|Yes + Active| GrantAccess
    HasDirectPermission -->|Yes + Inactive| DenyAccess[❌ Deny Access]
    HasDirectPermission -->|No| HasRolePermission{Has Role Permission?}
    
    HasRolePermission -->|Yes + Active| GrantAccess
    HasRolePermission -->|No| DenyAccess
    
    %% Styling
    style User fill:#4A90E2,stroke:#333,stroke-width:2px,color:#fff
    style Permission fill:#50C878,stroke:#333,stroke-width:2px,color:#fff
    style Role fill:#FF6B6B,stroke:#333,stroke-width:2px,color:#fff
    style GrantAccess fill:#28a745,stroke:#333,stroke-width:3px,color:#fff
    style DenyAccess fill:#dc3545,stroke:#333,stroke-width:3px,color:#fff
```

### ساختار درختی Permissions

```
CORE (هسته مرکزی)
├── CORE.VIEW (مشاهده)
├── CORE.PERMISSION (مدیریت دسترسی‌ها)
│   ├── CORE.PERMISSION.VIEW
│   ├── CORE.PERMISSION.CREATE
│   ├── CORE.PERMISSION.EDIT
│   └── CORE.PERMISSION.DELETE
├── CORE.ROLE (مدیریت نقش‌ها)
│   ├── CORE.ROLE.VIEW
│   ├── CORE.ROLE.CREATE
│   └── ...
├── CORE.USER (مدیریت کاربران)
│   └── ...
└── CORE.BRANCH (مدیریت شعب)
    └── ...

TASK (تسکینگ)
├── TASK.VIEW
├── TASK.CREATE
├── TASK.EDIT
├── TASK.DELETE
├── TASK.ASSIGN (اختصاص تسک)
├── TASK.COMPLETE (تکمیل تسک)
├── TASK.SUPERVISE ⭐ (نظارت بر تسک‌های دیگران) - جدید
└── TASK.OPERATION
    ├── TASK.OPERATION.CREATE
    └── ...

CRM (مدیریت ارتباط با مشتری)
├── CRM.VIEW
├── CRM.CREATE
├── CRM.EDIT
└── ...
```

---

## 📋 نمودار سیستم تسک

### جریان ایجاد و مدیریت تسک

```mermaid
flowchart TD
    Start([شروع]) --> CreateTask[ایجاد تسک]
    
    CreateTask --> SelectType{نوع تسک؟}
    
    SelectType -->|Individual| AssignUser[اختصاص به کاربر]
    SelectType -->|Team| AssignTeam[اختصاص به تیم]
    SelectType -->|Multiple| AssignMultiple[اختصاص به چند نفر]
    
    AssignUser --> SetDetails[تنظیم جزئیات]
    AssignTeam --> SetDetails
    AssignMultiple --> SetDetails
    
    SetDetails --> AddOperations{عملیات دارد؟}
    AddOperations -->|بله| CreateOperations[ایجاد عملیات‌ها]
    AddOperations -->|خیر| AddReminders
    
    CreateOperations --> AddReminders{یادآور دارد؟}
    AddReminders -->|بله| CreateReminders[ایجاد یادآورها]
    AddReminders -->|خیر| AddCarbonCopy
    
    CreateReminders --> AddCarbonCopy{افزودن رونوشت؟}
    AddCarbonCopy -->|بله| AddViewers[👁️ افزودن ناظران رونوشتی]
    AddCarbonCopy -->|خیر| SaveTask
    
    AddViewers --> SaveTask[ذخیره تسک]
    
    SaveTask --> SendNotification[ارسال اعلان]
    
    SendNotification --> TaskSaved([تسک ذخیره شد])
    
    %% Task Execution Flow
    TaskSaved --> UserReceives[کاربر اعلان دریافت می‌کند]
    UserReceives --> StartWork[شروع کار روی تسک]
    
    StartWork --> CompleteOperations{عملیات‌ها}
    CompleteOperations -->|در حال انجام| LogWork[ثبت گزارش کار]
    CompleteOperations -->|تکمیل شده| CheckCompletion{همه عملیات تکمیل شد؟}
    
    LogWork --> CompleteOperations
    
    CheckCompletion -->|بله| CompleteTask[تکمیل تسک]
    CheckCompletion -->|خیر| CompleteOperations
    
    CompleteTask --> NeedApproval{نیاز به تایید؟}
    
    NeedApproval -->|بله| SupervisorApproval[👁️ تایید ناظر/سرپرست]
    NeedApproval -->|خیر| TaskCompleted
    
    SupervisorApproval --> ManagerApproval[تایید مدیر]
    ManagerApproval --> TaskCompleted([تسک تکمیل و تایید شد])
    
    %% Styling
    style Start fill:#4CAF50,stroke:#333,stroke-width:2px,color:#fff
    style TaskSaved fill:#2196F3,stroke:#333,stroke-width:2px,color:#fff
    style TaskCompleted fill:#4CAF50,stroke:#333,stroke-width:3px,color:#fff
    style AddViewers fill:#FF9800,stroke:#333,stroke-width:2px,color:#fff
    style SupervisorApproval fill:#FF9800,stroke:#333,stroke-width:2px,color:#fff
```

### رابطه Task با Entities دیگر

```mermaid
graph LR
    Task[📋 Task] --> Creator[👤 Creator User]
    Task --> Branch[🏢 Branch]
    Task --> Team[👥 Team]
    Task --> Category[📁 Category]
    Task --> Contact[👤 Contact NEW]
    Task --> Organization[🏢 Organization NEW]
    Task --> Contract[📄 Contract]
    
    Task --> Assignments[👥 Assignments]
    Assignments --> AssignedUser1[User 1]
    Assignments --> AssignedUser2[User 2]
    Assignments -.->|in team| AssignedInTeam[⭐ Team Context]
    
    Task --> Viewers[👁️ Viewers Carbon Copy] ⭐
    Viewers --> Viewer1[Viewer 1]
    Viewers --> Viewer2[Viewer 2]
    
    Task --> ViewPermissions[🔑 View Permissions] ⭐
    ViewPermissions --> SpecialPermission1[Permission 1]
    ViewPermissions --> SpecialPermission2[Permission 2]
    
    Task --> Operations[⚙️ Operations]
    Operations --> WorkLogs[📝 Work Logs]
    
    Task --> Comments[💬 Comments]
    Task --> Attachments[📎 Attachments]
    Task --> Reminders[⏰ Reminders]
    Task --> Notifications[🔔 Notifications]
    
    %% Styling
    style Task fill:#FF6B6B,stroke:#333,stroke-width:3px,color:#fff
    style Contact fill:#50C878,stroke:#333,stroke-width:2px,color:#fff
    style Organization fill:#50C878,stroke:#333,stroke-width:2px,color:#fff
    style Viewers fill:#FF9800,stroke:#333,stroke-width:2px,color:#fff
    style ViewPermissions fill:#9C27B0,stroke:#333,stroke-width:2px,color:#fff
    style AssignedInTeam fill:#2196F3,stroke:#333,stroke-width:2px,color:#fff
```

---

## 👁️ نمودار سیستم نظارت بر تسک‌ها

### انواع نظارت

```mermaid
mindmap
  root((Task Supervision))
    System Supervision
      Position Based
        PowerLevel
          Lower = Higher Authority
        CanViewSubordinateTasks
        CanViewPeerTasks
      Formal Supervision
        MembershipType = 1
        Supervisor Role
      Team Management
        Team Manager
        Sub-team Manager
      Task Creator
        Automatic Supervisor
    Carbon Copy
      Manual Assignment
      TaskViewer_Tbl
      Start/End Date
      Added By User
    Special Permission
      User Tasks Permission
        PermissionType = 0
      Team Tasks Permission
        PermissionType = 1
      Team + SubTeams Permission
        PermissionType = 2
```

### جریان بررسی قابلیت مشاهده تسک

```mermaid
flowchart TD
    Start([آیا کاربر می‌تواند تسک را ببیند؟]) --> CheckPrivate{تسک خصوصی است؟}
    
    CheckPrivate -->|بله| IsCreatorOrAssigned{سازنده یا منتصب شده؟}
    IsCreatorOrAssigned -->|بله| GrantAccess[✅ دسترسی تایید]
    IsCreatorOrAssigned -->|خیر| DenyAccess[❌ دسترسی رد]
    
    CheckPrivate -->|خیر| CheckCarbonCopy{رونوشت شده؟<br/>TaskViewer}
    
    CheckCarbonCopy -->|بله + فعال| GrantAccess
    CheckCarbonCopy -->|خیر| CheckCreator{سازنده تسک؟}
    
    CheckCreator -->|بله| GrantAccess
    CheckCreator -->|خیر| CheckAssigned{منتصب شده؟}
    
    CheckAssigned -->|بله| GrantAccess
    CheckAssigned -->|خیر| CheckTeamManager{مدیر تیم تسک؟}
    
    CheckTeamManager -->|بله| GrantAccess
    CheckTeamManager -->|خیر| CheckPosition{سمت بالاتر در تیم؟}
    
    CheckPosition --> GetUserTeams[دریافت تیم‌های کاربر]
    GetUserTeams --> GetUserPosition[دریافت سمت در تیم]
    GetUserPosition --> ComparePosition{مقایسه PowerLevel}
    
    ComparePosition -->|کمتر = بالاتر| CheckSubordinates{آیا زیردست دارد؟}
    ComparePosition -->|مساوی| CheckPeers{آیا همسطح دارد؟}
    ComparePosition -->|بیشتر = پایین‌تر| CheckFormalSupervisor
    
    CheckSubordinates -->|بله + CanViewSubordinateTasks| CheckTaskTeam{تسک در همان تیم؟}
    CheckSubordinates -->|خیر| CheckFormalSupervisor
    
    CheckPeers -->|بله + CanViewPeerTasks| CheckTaskTeam
    CheckPeers -->|خیر| CheckFormalSupervisor
    
    CheckTaskTeam -->|بله<br/>AssignedInTeamId = TeamId| GrantAccess
    CheckTaskTeam -->|خیر| CheckFormalSupervisor
    
    CheckFormalSupervisor{ناظر رسمی؟<br/>MembershipType=1}
    CheckFormalSupervisor -->|بله| CheckTeamMembers{تسک به عضو تیم؟}
    CheckFormalSupervisor -->|خیر| CheckSpecialPermission
    
    CheckTeamMembers -->|بله| GrantAccess
    CheckTeamMembers -->|خیر| CheckSpecialPermission
    
    CheckSpecialPermission{مجوز خاص؟<br/>TaskViewPermission}
    CheckSpecialPermission -->|بله| CheckPermissionType{نوع مجوز؟}
    CheckSpecialPermission -->|خیر| CheckVisibilityLevel
    
    CheckPermissionType -->|0: تسک‌های کاربر| GrantAccess
    CheckPermissionType -->|1: تسک‌های تیم| GrantAccess
    CheckPermissionType -->|2: تسک‌های تیم + زیرتیم| GrantAccess
    
    CheckVisibilityLevel{VisibilityLevel >= 3?}
    CheckVisibilityLevel -->|بله<br/>تسک عمومی| GrantAccess
    CheckVisibilityLevel -->|خیر| DenyAccess
    
    %% Styling
    style Start fill:#4A90E2,stroke:#333,stroke-width:2px,color:#fff
    style GrantAccess fill:#28a745,stroke:#333,stroke-width:3px,color:#fff
    style DenyAccess fill:#dc3545,stroke:#333,stroke-width:3px,color:#fff
    style CheckTaskTeam fill:#FF9800,stroke:#333,stroke-width:2px,color:#fff
```

### الگوریتم GetVisibleTaskIdsAsync

```mermaid
sequenceDiagram
    participant Caller as 🔍 Caller
    participant Repo as TaskVisibilityRepository
    participant DB as 🗄️ Database
    participant Result as 📊 Result Set
    
    Caller->>Repo: GetVisibleTaskIdsAsync(userId)
    
    Repo->>DB: 1️⃣ دریافت شعبه‌های کاربر
    DB-->>Repo: branchIds[]
    
    Repo->>DB: 2️⃣ تسک‌های ساخته شده توسط کاربر
    DB-->>Result: createdTasks[]
    
    Repo->>DB: 3️⃣ تسک‌های منتصب شده به کاربر
    DB-->>Result: assignedTasks[]
    
    Repo->>DB: 4️⃣ تسک‌های تیم‌های مدیریت شده
    DB-->>Result: managedTeamTasks[]
    
    rect rgb(255, 200, 100)
        note right of Repo: ⭐ بخش کلیدی: نظارت بر اساس سمت
        
        loop برای هر تیم کاربر
            Repo->>DB: دریافت عضویت و سمت
            DB-->>Repo: membership + position
            
            alt اگر CanViewSubordinateTasks
                Repo->>DB: subordinateUserIds (PowerLevel >)
                Repo->>DB: تسک‌های آنها که AssignedInTeamId = تیم فعلی ⭐
                DB-->>Result: subordinateTasks[]
            end
            
            alt اگر CanViewPeerTasks
                Repo->>DB: peerUserIds (PowerLevel =)
                Repo->>DB: تسک‌های آنها که AssignedInTeamId = تیم فعلی ⭐
                DB-->>Result: peerTasks[]
            end
            
            alt اگر MembershipType = 1
                Repo->>DB: normalMemberIds (MembershipType = 0)
                Repo->>DB: تسک‌های آنها که AssignedInTeamId = تیم فعلی ⭐
                DB-->>Result: supervisedTasks[]
            end
        end
    end
    
    Repo->>DB: 5️⃣ تسک‌های با مجوز خاص
    DB-->>Result: specialPermissionTasks[]
    
    Repo->>DB: 6️⃣ تسک‌های رونوشت (TaskViewer)
    DB-->>Result: carbonCopyTasks[]
    
    Repo->>Result: ترکیب و حذف تکراری
    Result-->>Caller: visibleTaskIds[]
```

### ساختار جداول نظارت

```mermaid
erDiagram
    Tasks ||--o{ TaskAssignment : "assigned via"
    Tasks ||--o{ TaskViewer : "carbon copy"
    
    TaskAssignment ||--|| AppUsers : "assigned to"
    TaskAssignment ||--o| Team : "in team context ⭐"
    
    Team ||--|| AppUsers : "managed by"
    Team ||--o{ Team : "parent-child"
    Team ||--o{ TeamMember : "contains"
    Team ||--o{ TeamPosition : "has positions"
    
    TeamMember ||--|| AppUsers : "is"
    TeamMember ||--o| TeamPosition : "has position"
    TeamMember {
        byte MembershipType "0=عادی, 1=ناظر ⭐"
    }
    
    TeamPosition {
        int PowerLevel "کمتر = بالاتر ⭐"
        bool CanViewSubordinateTasks "⭐"
        bool CanViewPeerTasks "⭐"
    }
    
    TaskViewer ||--|| AppUsers : "viewer user"
    TaskViewer ||--|| AppUsers : "added by"
    TaskViewer {
        bool IsActive
        DateTime StartDate
        DateTime EndDate
        string Note
    }
    
    TaskViewPermission ||--|| AppUsers : "grantee"
    TaskViewPermission ||--o| AppUsers : "target user"
    TaskViewPermission ||--o| Team : "target team"
    TaskViewPermission {
        byte PermissionType "0=کاربر, 1=تیم, 2=تیم+زیر ⭐"
        bool IsActive
        DateTime StartDate
        DateTime EndDate
    }
```

### مثال عملی: نظارت بر اساس سمت

```mermaid
graph TD
    subgraph "تیم بازاریابی (ID=5)"
        Manager[👔 مدیر: علی<br/>PowerLevel=1<br/>CanViewSubordinateTasks=true]
        Supervisor[👤 سرپرست: حسین<br/>PowerLevel=2]
        Employee[👨‍💻 کارمند: مهدی<br/>PowerLevel=3]
        
        Manager -->|می‌تواند ببیند| Supervisor
        Manager -->|می‌تواند ببیند| Employee
        Supervisor -->|می‌تواند ببیند| Employee
    end
    
    subgraph "تیم فروش (ID=6)"
        SalesMember[👤 عضو: حسین<br/>همان حسین]
    end
    
    Task[📋 تسک: تماس با مشتری<br/>AssignedUserId = حسین<br/>AssignedInTeamId = 5 ⭐]
    
    Task -.->|assign شده در تیم بازاریابی| Supervisor
    
    Manager -.->|✅ می‌تواند ببیند<br/>چون AssignedInTeamId = 5| Task
    SalesMember -.->|❌ نمی‌تواند ببیند<br/>چون AssignedInTeamId ≠ 6| Task
    
    style Task fill:#FF6B6B,stroke:#333,stroke-width:3px,color:#fff
    style Manager fill:#4CAF50,stroke:#333,stroke-width:2px,color:#fff
    style SalesMember fill:#F44336,stroke:#333,stroke-width:2px,color:#fff
```

---

## 🕐 نمودار سیستم تسک‌های زمان‌بندی شده

### جریان کامل Scheduled Task Creation

```mermaid
flowchart TD
    Start([👤 کاربر می‌خواهد<br/>تسک تکرارشونده بسازد]) --> OpenForm[باز کردن فرم<br/>Create Scheduled Task]
    
    OpenForm --> FillBasicInfo[📝 وارد کردن اطلاعات پایه]
    
    FillBasicInfo --> BasicFields[• عنوان زمان‌بندی<br/>• توضیحات<br/>• تاریخ شروع/پایان<br/>• حداکثر تعداد اجرا]
    
    BasicFields --> SelectScheduleType{نوع زمان‌بندی}
    
    SelectScheduleType -->|یکبار| OneTime[📅 ScheduleType = 0<br/>• فقط یک بار اجرا<br/>• تاریخ و ساعت مشخص]
    SelectScheduleType -->|روزانه| Daily[📅 ScheduleType = 1<br/>• هر روز<br/>• ساعت اجرا]
    SelectScheduleType -->|هفتگی| Weekly[📅 ScheduleType = 2<br/>• روزهای هفته<br/>• ساعت اجرا]
    SelectScheduleType -->|ماهانه| Monthly[📅 ScheduleType = 3<br/>• روز ماه<br/>• ساعت اجرا]
    
    OneTime --> FillTaskTemplate
    Daily --> FillTaskTemplate
    Weekly --> FillTaskTemplate
    Monthly --> FillTaskTemplate[📋 پر کردن قالب تسک]
    
    FillTaskTemplate --> TaskFields[• عنوان تسک<br/>• توضیحات<br/>• اولویت<br/>• برآورد زمان<br/>• شعبه<br/>• دسته‌بندی]
    
    TaskFields --> AddOperations[➕ افزودن عملیات‌ها]
    AddOperations --> OperationsList[لیست عملیات:<br/>1. عملیات 1<br/>2. عملیات 2<br/>3. ...]
    
    OperationsList --> AddAssignments[👥 اختصاص کاربران]
    AddAssignments --> AssignmentsList[• کاربر 1 در تیم A<br/>• کاربر 2 در تیم B<br/>• ...]
    
    AssignmentsList --> AddCarbonCopy{رونوشت؟}
    AddCarbonCopy -->|بله| AddViewers[👁️ افزودن ناظران]
    AddCarbonCopy -->|خیر| SaveSchedule
    
    AddViewers --> SaveSchedule[💾 ذخیره زمان‌بندی]
    
    SaveSchedule --> CreateJSON[ساخت TaskTemplateJson]
    CreateJSON --> CalculateNext[⏰ محاسبه NextExecutionDate]
    
    CalculateNext --> SaveToDB[(💾 ذخیره در دیتابیس<br/>ScheduledTaskCreation_Tbl)]
    
    SaveToDB --> ScheduleSaved([✅ زمان‌بندی ذخیره شد])
    
    ScheduleSaved --> WaitForExecution[⏳ در انتظار اجرا...]
    
    WaitForExecution --> BGServiceCheck[⚙️ Background Service<br/>هر 1 دقیقه چک می‌کند]
    
    BGServiceCheck --> CheckTime{زمان رسیده؟<br/>NextExecutionDate <= Now}
    
    CheckTime -->|خیر| WaitForExecution
    CheckTime -->|بله| ExecuteSchedule[🚀 اجرای زمان‌بندی]
    
    ExecuteSchedule --> ParseJSON[Parse TaskTemplateJson]
    ParseJSON --> ReplaceVariables[جایگزینی متغیرهای پویا<br/>{{Date}}, {{DateTime}}, ...]
    
    ReplaceVariables --> CreateTask[📋 ساخت تسک جدید]
    CreateTask --> SaveTask[(💾 ذخیره در Tasks_Tbl<br/>ScheduleId = schedule.Id)]
    
    SaveTask --> CreateOps[⚙️ ثبت عملیات‌ها<br/>TaskOperation_Tbl]
    CreateOps --> CreateAssigns[👥 ثبت اختصاص‌ها<br/>TaskAssignment_Tbl]
    CreateAssigns --> CreateViewers[👁️ ثبت ناظران<br/>TaskViewer_Tbl]
    
    CreateViewers --> SendNotif[📧 ارسال اعلان به<br/>انجام‌دهندگان]
    
    SendNotif --> UpdateSchedule[🔄 بروزرسانی زمان‌بندی]
    
    UpdateSchedule --> UpdateFields[• LastExecutionDate = Now<br/>• ExecutionCount++<br/>• NextExecutionDate = محاسبه بعدی]
    
    UpdateFields --> CheckConditions{بررسی شرایط}
    
    CheckConditions -->|ExecutionCount >= MaxOccurrences| Disable1[IsScheduleEnabled = false]
    CheckConditions -->|Now >= EndDate| Disable2[IsScheduleEnabled = false]
    CheckConditions -->|ScheduleType = 0| Disable3[IsScheduleEnabled = false<br/>یکبار بود]
    CheckConditions -->|ادامه دارد| SaveUpdate
    
    Disable1 --> SaveUpdate
    Disable2 --> SaveUpdate
    Disable3 --> SaveUpdate[(💾 ذخیره تغییرات)]
    
    SaveUpdate --> LogSuccess[📝 ثبت لاگ موفقیت]
    
    LogSuccess --> CheckEnabled{IsScheduleEnabled?}
    
    CheckEnabled -->|بله| WaitForExecution
    CheckEnabled -->|خیر| EndSchedule([⏹️ زمان‌بندی متوقف شد])
    
    %% Styling
    style Start fill:#4CAF50,stroke:#333,stroke-width:2px,color:#fff
    style ScheduleSaved fill:#2196F3,stroke:#333,stroke-width:2px,color:#fff
    style ExecuteSchedule fill:#FF9800,stroke:#333,stroke-width:3px,color:#fff
    style CreateTask fill:#9C27B0,stroke:#333,stroke-width:2px,color:#fff
    style EndSchedule fill:#F44336,stroke:#333,stroke-width:2px,color:#fff
```

### ساختار Entity: ScheduledTaskCreation

```mermaid
erDiagram
    ScheduledTaskCreation {
        int Id PK
        string ScheduleTitle "عنوان زمان‌بندی"
        string ScheduleDescription "توضیحات"
        byte ScheduleType "0=یکبار، 1=روزانه، 2=هفتگی، 3=ماهانه"
        string ScheduledTime "مثال: 09:00"
        string ScheduledDaysOfWeek "مثال: 1,3,5"
        int ScheduledDayOfMonth "مثال: 15"
        DateTime StartDate "تاریخ شروع"
        string StartDatePersian "1403/10/15"
        DateTime EndDate "تاریخ پایان (اختیاری)"
        string EndDatePersian
        int MaxOccurrences "حداکثر تعداد"
        DateTime NextExecutionDate "⭐ زمان بعدی"
        string NextExecutionDatePersian
        DateTime LastExecutionDate "آخرین اجرا"
        string LastExecutionDatePersian
        int ExecutionCount "تعداد دفعات اجرا شده"
        bool IsScheduleEnabled "فعال/غیرفعال"
        bool IsActive "حذف نرم"
        string TaskTemplateJson "⭐ قالب تسک (JSON)"
        string CreatorUserId FK
        DateTime CreateDate
        string ModifierUserId FK
        DateTime ModifyDate
    }
    
    Tasks {
        int Id PK
        string Title
        string Description
        int ScheduleId FK "⭐ ارتباط با زمان‌بندی"
        byte CreationMode "0=دستی، 1=خودکار"
    }
    
    AppUsers {
        string Id PK
        string FirstName
        string LastName
    }
    
    ScheduledTaskCreation ||--|| AppUsers : "created by"
    ScheduledTaskCreation ||--o| AppUsers : "modified by"
    ScheduledTaskCreation ||--o{ Tasks : "generates"
    
    Tasks ||--o| ScheduledTaskCreation : "created by schedule"
```

### الگوریتم محاسبه NextExecutionDate

```mermaid
flowchart TD
    Start([محاسبه NextExecutionDate]) --> GetNow[دریافت زمان ایران<br/>TimeZoneInfo]
    
    GetNow --> ParseTime[Parse ساعت و دقیقه<br/>از ScheduledTime]
    
    ParseTime --> CheckType{ScheduleType}
    
    %% یکبار
    CheckType -->|0: یکبار| CheckFirstTime{ExecutionCount == 0?}
    CheckFirstTime -->|بله| ReturnStart[return StartDate<br/>با ساعت ScheduledTime]
    CheckFirstTime -->|خیر| ReturnNull1[return null<br/>فقط یک بار بود]
    
    %% روزانه
    CheckType -->|1: روزانه| CalcDaily[nextExec = امروز<br/>با ساعت ScheduledTime]
    CalcDaily --> CheckDailyPassed{nextExec <= now?}
    CheckDailyPassed -->|بله| AddOneDay[nextExec = nextExec.AddDays 1]
    CheckDailyPassed -->|خیر| ReturnDaily[return nextExec]
    AddOneDay --> ReturnDaily
    
    %% هفتگی
    CheckType -->|2: هفتگی| ParseDays[Parse ScheduledDaysOfWeek<br/>مثال: 1,3,5]
    ParseDays --> GetToday[today = int DayOfWeek]
    GetToday --> CheckTodayInList{امروز در لیست؟<br/>+ ساعت نگذشته؟}
    CheckTodayInList -->|بله| ReturnToday[return امروز<br/>با ساعت ScheduledTime]
    CheckTodayInList -->|خیر| FindNextDay[حلقه 1 تا 7 روز]
    FindNextDay --> CheckNextDay{روز بعدی<br/>در لیست؟}
    CheckNextDay -->|بله| ReturnNextDay[return آن روز<br/>با ساعت]
    CheckNextDay -->|خیر| FindNextDay
    
    %% ماهانه
    CheckType -->|3: ماهانه| CalcMonthly[nextExec = این ماه<br/>روز ScheduledDayOfMonth<br/>با ساعت ScheduledTime]
    CalcMonthly --> CheckMonthlyPassed{nextExec <= now?}
    CheckMonthlyPassed -->|خیر| ReturnMonthly[return nextExec]
    CheckMonthlyPassed -->|بله| CalcNextMonth[ماه بعد]
    CalcNextMonth --> CheckDaysInMonth[بررسی تعداد روز<br/>مثال: 30 روزه؟]
    CheckDaysInMonth --> AdjustDay[day = Min ScheduledDayOfMonth,<br/>daysInMonth]
    AdjustDay --> ReturnNextMonth[return ماه بعد]
    
    %% نتایج
    ReturnStart --> LogResult[📝 ثبت لاگ]
    ReturnNull1 --> End
    ReturnDaily --> LogResult
    ReturnToday --> LogResult
    ReturnNextDay --> LogResult
    ReturnMonthly --> LogResult
    ReturnNextMonth --> LogResult
    
    LogResult --> End([پایان])
    
    %% Styling
    style Start fill:#4CAF50,stroke:#333,stroke-width:2px,color:#fff
    style CheckType fill:#2196F3,stroke:#333,stroke-width:2px,color:#fff
    style LogResult fill:#FF9800,stroke:#333,stroke-width:2px,color:#fff
    style End fill:#4CAF50,stroke:#333,stroke-width:2px,color:#fff
```

### نمونه TaskTemplateJson

```mermaid
graph TB
    subgraph TaskTemplateJson
        Root[TaskTemplate Object]
        
        Root --> BasicInfo[اطلاعات پایه]
        BasicInfo --> Title["Title: گزارش روزانه - {{Date}}"]
        BasicInfo --> Description["Description: ..."]
        BasicInfo --> Priority["Priority: 1"]
        BasicInfo --> Important["Important: true"]
        BasicInfo --> BranchId["BranchId: 5"]
        BasicInfo --> CategoryId["CategoryId: 10"]
        
        Root --> Operations[Operations Array]
        Operations --> Op1["Op1:<br/>• Title: جمع‌آوری<br/>• Order: 1<br/>• Hours: 1.0"]
        Operations --> Op2["Op2:<br/>• Title: تحلیل<br/>• Order: 2<br/>• Hours: 1.0"]
        Operations --> Op3["Op3:<br/>• Title: ارسال<br/>• Order: 3<br/>• Hours: 0.5"]
        
        Root --> Assignments[Assignments Array]
        Assignments --> Assign1["Assign1:<br/>• UserId: user-123<br/>• TeamId: 5"]
        Assignments --> Assign2["Assign2:<br/>• UserId: user-456<br/>• TeamId: 5"]
        
        Root --> Viewers[CarbonCopies Array]
        Viewers --> View1["Viewer1:<br/>• UserId: manager-789"]
    end
    
    style Root fill:#2196F3,stroke:#333,stroke-width:3px,color:#fff
    style BasicInfo fill:#4CAF50,stroke:#333,stroke-width:2px,color:#fff
    style Operations fill:#FF9800,stroke:#333,stroke-width:2px,color:#fff
    style Assignments fill:#9C27B0,stroke:#333,stroke-width:2px,color:#fff
    style Viewers fill:#F44336,stroke:#333,stroke-width:2px,color:#fff
```

### مقایسه انواع زمان‌بندی

```mermaid
graph LR
    subgraph "0️⃣ یکبار (One-Time)"
        OT[یک بار اجرا<br/>تاریخ و ساعت مشخص]
        OT --> OTEx["مثال:<br/>1403/10/25 14:00<br/>فقط یک بار"]
    end
    
    subgraph "1️⃣ روزانه (Daily)"
        D[هر روز<br/>ساعت مشخص]
        D --> DEx["مثال:<br/>هر روز 09:00<br/>تا ابد یا تاریخ پایان"]
    end
    
    subgraph "2️⃣ هفتگی (Weekly)"
        W[روزهای خاص هفته<br/>ساعت مشخص]
        W --> WEx["مثال:<br/>دوشنبه، چهارشنبه، جمعه<br/>ساعت 10:00"]
    end
    
    subgraph "3️⃣ ماهانه (Monthly)"
        M[روز خاص ماه<br/>ساعت مشخص]
        M --> MEx["مثال:<br/>روز 15 هر ماه<br/>ساعت 10:00"]
    end
    
    style OT fill:#9E9E9E,stroke:#333,stroke-width:2px,color:#fff
    style D fill:#4CAF50,stroke:#333,stroke-width:2px,color:#fff
    style W fill:#2196F3,stroke:#333,stroke-width:2px,color:#fff
    style M fill:#FF9800,stroke:#333,stroke-width:2px,color:#fff
```

### Sequence Diagram: اجرای Background Service

```mermaid
sequenceDiagram
    participant BG as ⚙️ Background Service
    participant DB as 🗄️ Database
    participant Calc as 📊 Calculator
    participant Task as 📋 TaskRepository
    participant Notif as 🔔 NotificationService
    
    Note over BG: هر 1 دقیقه
    
    BG->>DB: Query: IsScheduleEnabled + NextExecution <= Now
    DB-->>BG: [Schedule1, Schedule2, ...]
    
    loop برای هر Schedule
        BG->>BG: Parse TaskTemplateJson
        
        BG->>BG: جایگزینی متغیرهای پویا<br/>{{Date}} → 1403/10/20
        
        BG->>Calc: CalculateDueDate(schedule)
        Calc-->>BG: DueDate محاسبه شده
        
        BG->>Task: CreateTaskAsync(taskData)
        Task->>DB: INSERT INTO Tasks_Tbl
        DB-->>Task: taskId
        
        Task->>DB: INSERT INTO TaskOperation_Tbl
        Task->>DB: INSERT INTO TaskAssignment_Tbl
        Task->>DB: INSERT INTO TaskViewer_Tbl
        
        Task-->>BG: ✅ تسک ساخته شد
        
        BG->>Notif: SendTaskAssignedNotification()
        Notif-->>BG: ✅ اعلان ارسال شد
        
        BG->>Calc: CalculateNextExecutionDate(schedule)
        Calc-->>BG: NextExecutionDate جدید
        
        BG->>DB: UPDATE ScheduledTaskCreation:<br/>LastExecutionDate = Now<br/>ExecutionCount++<br/>NextExecutionDate = جدید
        
        alt MaxOccurrences رسیده
            BG->>DB: UPDATE: IsScheduleEnabled = false
        else EndDate رسیده
            BG->>DB: UPDATE: IsScheduleEnabled = false
        else ScheduleType = 0
            BG->>DB: UPDATE: IsScheduleEnabled = false
        end
        
        DB-->>BG: ✅ بروزرسانی شد
        
        BG->>BG: 📝 لاگ موفقیت
    end
    
    Note over BG: صبر تا دقیقه بعد...
```

---

## ⏰ نمودار Background Services و زمان‌بندی

### معماری Background Services

```mermaid
graph TB
    subgraph "Background Services Layer"
        NotifProcessing[NotificationProcessingBackgroundService<br/>⏱️ هر 10 ثانیه]
        ScheduledNotif[ScheduledNotificationBackgroundService<br/>⏱️ هر 1 دقیقه ⭐]
        ScheduledTask[ScheduledTaskCreationBackgroundService<br/>⏱️ هر 1 دقیقه 🆕]
        EmailBG[EmailBackgroundService<br/>⏱️ هر 30 ثانیه]
        SmsBG[SmsBackgroundService<br/>⏱️ هر 20 ثانیه]
        TelegramBG[TelegramPollingBackgroundService<br/>⏱️ هر 5 ثانیه]
        ModuleTracking[ModuleTrackingBackgroundService<br/>⏱️ هر 5 دقیقه]
        RoleCleanup[ExpiredRoleCleanupBackgroundService<br/>⏱️ روزانه 02:00]
    end
    
    subgraph "Database Tables"
        CoreNotif[(CoreNotification_Tbl)]
        NotifTemplate[(NotificationTemplate_Tbl)]
        ScheduledTaskTbl[(ScheduledTaskCreation_Tbl 🆕)]
        TasksTbl[(Tasks_Tbl)]
        EmailQueue[(EmailQueue_Tbl)]
        SmsQueue[(SmsQueue_Tbl)]
    end
    
    subgraph "External Services"
        Email[📧 SMTP Server]
        SMS[📱 SMS Provider]
        Telegram[✈️ Telegram API]
    end
    
    NotifProcessing --> CoreNotif
    ScheduledNotif --> NotifTemplate
    ScheduledTask --> ScheduledTaskTbl
    ScheduledTask --> TasksTbl
    EmailBG --> EmailQueue
    SmsBG --> SmsQueue
    
    EmailBG --> Email
    SmsBG --> SMS
    TelegramBG --> Telegram
    
    ScheduledNotif -.->|ارسال مستقیم| Email
    ScheduledNotif -.->|ارسال مستقیم| SMS
    ScheduledNotif -.->|ارسال مستقیم| Telegram
    
    ScheduledTask -.->|ارسال اعلان| CoreNotif
    
    style ScheduledNotif fill:#FF9800,stroke:#333,stroke-width:3px,color:#fff
    style ScheduledTask fill:#4CAF50,stroke:#333,stroke-width:3px,color:#fff
    style NotifTemplate fill:#4CAF50,stroke:#333,stroke-width:2px,color:#fff
    style ScheduledTaskTbl fill:#2196F3,stroke:#333,stroke-width:2px,color:#fff
```

### نمودار Gantt: زمان‌بندی Background Services

```mermaid
gantt
    title Background Services زمان‌بندی (24 ساعته)
    dateFormat HH:mm
    axisFormat %H:%M
    
    section Email
    EmailBackgroundService :active, email, 00:00, 24h
    
    section SMS
    SmsBackgroundService :active, sms, 00:00, 24h
    
    section Telegram
    TelegramPollingService :active, telegram, 00:00, 24h
    
    section Notifications
    NotificationProcessing :active, notify, 00:00, 24h
    ScheduledNotifications :crit, scheduled, 00:00, 24h
    
    section Scheduled Tasks 🆕
    ScheduledTaskCreation :crit, schtask, 00:00, 24h
    
    section Maintenance
    ExpiredRoleCleanup :done, cleanup, 02:00, 1h
    ModuleTracking :active, track, 00:00, 24h
    
    section ⭐ مثال Daily Task
    FirstExecution :milestone, first, 09:00, 0
    SecondExecution :milestone, second, 09:00, 0
    ThirdExecution :milestone, third, 09:00, 0
```

### مقایسه: Scheduled Notification vs Scheduled Task

```mermaid
graph TB
    subgraph "Scheduled Notification (قدیمی)"
        SN[NotificationTemplate]
        SN --> SN1[📧 ارسال اعلان/ایمیل/SMS]
        SN --> SN2[🔔 فقط اطلاع‌رسانی]
        SN --> SN3[❌ تسک ساخته نمی‌شود]
    end
    
    subgraph "Scheduled Task Creation 🆕"
        ST[ScheduledTaskCreation]
        ST --> ST1[📋 ساخت تسک کامل]
        ST --> ST2[⚙️ شامل Operations]
        ST --> ST3[👥 شامل Assignments]
        ST --> ST4[👁️ شامل Viewers]
        ST --> ST5[🔔 اعلان خودکار]
        ST --> ST6[✅ قابل پیگیری]
    end
    
    style SN fill:#FF9800,stroke:#333,stroke-width:2px,color:#fff
    style ST fill:#4CAF50,stroke:#333,stroke-width:3px,color:#fff
```

---

## 📈 نمودارهای آماری

### توزیع استفاده از ماژول‌ها

```mermaid
pie title توزیع استفاده از ماژول‌ها
    "Tasking" : 50
    "Core" : 30
    "CRM" : 20
```

### کانال‌های ارسال اعلان

```mermaid
pie title کانال‌های ارسال اعلان
    "In-App" : 100
    "Email" : 60
    "Telegram" : 40
    "SMS" : 30
```

### انواع نظارت بر تسک‌ها ⭐

```mermaid
pie title انواع نظارت بر تسک‌ها
    "نظارت سیستمی (بر اساس سمت)" : 60
    "نظارت رونوشتی (دستی)" : 25
    "مجوز خاص" : 15
```

### ⭐ نوع زمان‌بندی قالب‌های اعلان

```mermaid
pie title توزیع نوع زمان‌بندی قالب‌های اعلان
    "روزانه (Daily)" : 60
    "هفتگی (Weekly)" : 25
    "ماهانه (Monthly)" : 15
```

### 🆕 نوع زمان‌بندی تسک‌ها (جدید)

```mermaid
pie title توزیع نوع زمان‌بندی تسک‌ها
    "روزانه (Daily)" : 50
    "هفتگی (Weekly)" : 30
    "ماهانه (Monthly)" : 15
    "یکبار (One-Time)" : 5
```

### 🆕 مقایسه تسک‌های دستی vs خودکار

```mermaid
graph LR
    subgraph "تسک‌های دستی"
        M1[👤 کاربر ایجاد می‌کند]
        M2[CreationMode = 0]
        M3[ScheduleId = null]
    end
    
    subgraph "تسک‌های خودکار 🆕"
        A1[🤖 Background Service ایجاد می‌کند]
        A2[CreationMode = 1]
        A3[ScheduleId = X]
        A4[قابل ردیابی به زمان‌بندی]
    end
    
    style M1 fill:#2196F3,stroke:#333,stroke-width:2px,color:#fff
    style A1 fill:#4CAF50,stroke:#333,stroke-width:2px,color:#fff
```

---

## 🐛 نمودار رفع مشکلات (Troubleshooting)

### جریان دیباگ: تسک خودکار ساخته نمی‌شود 🆕

```mermaid
flowchart TD
    Start([مشکل: تسک خودکار ساخته نمی‌شود]) --> CheckLogs[بررسی لاگ‌های<br/>ScheduledTaskCreationBackgroundService]
    
    CheckLogs --> LogPattern{الگوی لاگ}
    
    LogPattern -->|❌ خطا در Parse JSON| CheckJSON[بررسی TaskTemplateJson]
    LogPattern -->|⚠️ زمان‌بندی یافت نشد| CheckDB[بررسی دیتابیس]
    LogPattern -->|✅ اجرا موفق| ProblemSolved[✅ مشکل حل شده]
    
    CheckJSON --> ValidateJSON{JSON معتبر است؟}
    ValidateJSON -->|خیر ❌| FixJSON[🔧 اصلاح JSON<br/>استفاده از JSON Validator]
    ValidateJSON -->|بله ✅| CheckDB
    
    CheckDB --> QueryDB[اجرای Query تست]
    
    QueryDB --> CheckFields{بررسی فیلدها}
    
    CheckFields --> CheckEnabled{IsScheduleEnabled?}
    CheckEnabled -->|false ❌| EnableIt[🔧 فعال کردن زمان‌بندی]
    CheckEnabled -->|true ✅| CheckActive{IsActive?}
    
    CheckActive -->|false ❌| ActivateIt[🔧 فعال کردن رکورد]
    CheckActive -->|true ✅| CheckNext{NextExecutionDate<br/>در گذشته است؟}
    
    CheckNext -->|خیر ❌| WaitMore[⏳ صبر تا زمان رسیدن]
    CheckNext -->|بله ✅| CheckBGService{Background Service<br/>در حال اجرا است؟}
    
    CheckBGService -->|خیر ❌| RestartApp[🔄 Restart اپلیکیشن]
    CheckBGService -->|بله ✅| ManualExecute[⚙️ اجرای دستی<br/>ExecuteScheduleAsync]
    
    FixJSON --> TestAgain[تست مجدد]
    EnableIt --> TestAgain
    ActivateIt --> TestAgain
    RestartApp --> TestAgain
    ManualExecute --> TestAgain
    
    TestAgain --> Solved{مشکل حل شد؟}
    
    Solved -->|بله ✅| ProblemSolved
    Solved -->|خیر ❌| DeepDebug[🔍 دیباگ عمیق<br/>با Breakpoint در Background Service]
    
    DeepDebug --> ContactSupport[📞 تماس با پشتیبانی<br/>با ارسال لاگ‌ها]
    
    ProblemSolved --> End([✅ سیستم عادی شد])
    WaitMore --> End
    ContactSupport --> End
    
    %% Styling
    style Start fill:#F44336,stroke:#333,stroke-width:2px,color:#fff
    style ProblemSolved fill:#4CAF50,stroke:#333,stroke-width:3px,color:#fff
    style FixJSON fill:#FF9800,stroke:#333,stroke-width:2px,color:#fff
    style RestartApp fill:#2196F3,stroke:#333,stroke-width:2px,color:#fff
```

### Query تست برای دیباگ 🆕

```sql
-- بررسی وضعیت زمان‌بندی‌های تسک
SELECT 
    Id,
    ScheduleTitle,
    ScheduleType,
    CASE ScheduleType
        WHEN 0 THEN 'یکبار'
        WHEN 1 THEN 'روزانه'
        WHEN 2 THEN 'هفتگی'
        WHEN 3 THEN 'ماهانه'
    END AS ScheduleTypeText,
    ScheduledTime,
    LastExecutionDate,
    NextExecutionDate,
    DATEDIFF(MINUTE, LastExecutionDate, GETDATE()) AS MinutesSinceLastExecution,
    ExecutionCount,
    MaxOccurrences,
    IsScheduleEnabled,
    IsActive,
    CASE 
        WHEN NextExecutionDate IS NULL THEN '⚠️ NextExecution خالی'
        WHEN NOT IsScheduleEnabled THEN '⏹️ غیرفعال'
        WHEN NOT IsActive THEN '🗑️ حذف شده'
        WHEN NextExecutionDate <= GETDATE() THEN '⚡ آماده اجرا'
        ELSE '⏳ در انتظار'
    END AS Status,
    CASE 
        WHEN MaxOccurrences IS NOT NULL AND ExecutionCount >= MaxOccurrences THEN '🛑 به حداکثر رسیده'
        WHEN GETDATE() >= EndDate THEN '🛑 تاریخ پایان رسیده'
        ELSE '✅ در حال اجرا'
    END AS ExecutionStatus
FROM ScheduledTaskCreation_Tbl
ORDER BY NextExecutionDate;

-- بررسی تسک‌های ساخته شده توسط زمان‌بندی
SELECT 
    t.Id AS TaskId,
    t.TaskCode,
    t.Title,
    t.CreationMode,
    t.ScheduleId,
    s.ScheduleTitle,
    t.CreateDate
FROM Tasks_Tbl t
LEFT JOIN ScheduledTaskCreation_Tbl s ON t.ScheduleId = s.Id
WHERE t.CreationMode = 1  -- خودکار
ORDER BY t.CreateDate DESC;
```

---

## 📝 نتیجه‌گیری

این دیاگرام‌ها نشان‌دهنده **ساختار پیچیده و جامع سیستم MahERP** هستند. نکات کلیدی:

✅ معماری لایه‌ای (Layered Architecture)
✅ جداسازی نگرانی‌ها (Separation of Concerns)
✅ استفاده از الگوهای طراحی (Repository, Unit of Work)
✅ **سیستم نظارت هوشمند بر تسک‌ها** ⭐ **جدید**
✅ **سیستم تسک‌های زمان‌بندی شده با قابلیت‌های پیشرفته** 🆕 **جدیدترین**
✅ سیستم اعلان‌رسانی چندکاناله
✅ **پشتیبانی از Background Services با Anti-duplicate** ⭐ **به‌روزرسانی شده**
✅ مدل داده مدرن (Contact/Organization جایگزین Stakeholder)
✅ **فیلتر محدود به تیم (Team-scoped)** ⭐ **جدید**
✅ **سیستم زمان‌بندی پیشرفته با TimeZone ایران** ⭐ **جدید**
✅ **محاسبه خودکار NextExecutionDate** 🆕 **جدیدترین**
✅ **پشتیبانی از متغیرهای پویا در قالب‌ها** 🆕 **جدیدترین**

---

**نسخه مستند:** 3.0.0 🆕 **(به‌روزرسانی شده با دیاگرام‌های کامل Scheduled Task Creation)**
**تاریخ:** آذر 1403 (اضافه شدن نمودارهای سیستم تسک‌های زمان‌بندی شده)
