# 👁️ نمودار سیستم نظارت بر تسک‌ها

## معرفی

سیستم نظارت MahERP یک **سیستم هوشمند و چندسطحی** است که به صورت خودکار مشخص می‌کند چه کسانی می‌توانند تسک‌های دیگران را مشاهده کنند.

---

## 🎯 انواع نظارت

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

---

## 🔍 جریان بررسی قابلیت مشاهده تسک

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

---

## ⚙️ الگوریتم GetVisibleTaskIdsAsync

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

---

## 📊 ساختار جداول نظارت

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

---

## 🎭 Entity: TeamPosition

```csharp
public class TeamPosition
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string TitleFa { get; set; }
    public string TitleEn { get; set; }
    
    // ⭐ کلیدی برای نظارت
    public int PowerLevel { get; set; }                    // کمتر = بالاتر
    public bool CanViewSubordinateTasks { get; set; }      // مشاهده زیردستان
    public bool CanViewPeerTasks { get; set; }             // مشاهده همسطح
    
    public bool IsActive { get; set; }
    
    // Navigation
    public Team Team { get; set; }
    public ICollection<TeamMember> Members { get; set; }
}
```

### 📌 منطق PowerLevel

```
PowerLevel کمتر = قدرت بیشتر

مثال:
- مدیر تیم: PowerLevel = 1
- سرپرست: PowerLevel = 2
- کارمند: PowerLevel = 3

→ مدیر می‌تواند تسک‌های سرپرست و کارمند را ببیند
→ سرپرست می‌تواند فقط تسک‌های کارمند را ببیند
```

---

## 🎭 Entity: TeamMember

```csharp
public class TeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string UserId { get; set; }
    public int? PositionId { get; set; }
    
    // ⭐ کلیدی برای نظارت رسمی
    public byte MembershipType { get; set; }  // 0=عضو عادی, 1=ناظر رسمی
    
    public DateTime JoinDate { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation
    public Team Team { get; set; }
    public AppUsers User { get; set; }
    public TeamPosition Position { get; set; }
}
```

### 📌 منطق MembershipType

```
MembershipType = 0: عضو عادی
MembershipType = 1: ناظر رسمی

→ ناظر رسمی می‌تواند تمام تسک‌های اعضای عادی (MembershipType=0) را ببیند
```

---

## 🎭 Entity: TaskViewer (رونوشت)

```csharp
public class TaskViewer
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string UserId { get; set; }              // کاربر رونوشت شده
    public string AddedByUserId { get; set; }       // کسی که رونوشت را اضافه کرده
    
    public DateTime AddedDate { get; set; }
    public string Note { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }        // تاریخ شروع اعتبار
    public DateTime? EndDate { get; set; }          // تاریخ پایان اعتبار
    
    // Navigation
    public Tasks Task { get; set; }
    public AppUsers User { get; set; }
    public AppUsers AddedBy { get; set; }
}
```

### 📌 نحوه استفاده

```csharp
// افزودن ناظر رونوشتی
var viewer = new TaskViewer
{
    TaskId = taskId,
    UserId = viewerUserId,
    AddedByUserId = currentUserId,
    AddedDate = DateTime.Now,
    Note = "رونوشت برای اطلاع",
    IsActive = true,
    StartDate = DateTime.Now,
    EndDate = DateTime.Now.AddMonths(1)  // یک ماه اعتبار
};
```

---

## 🎭 Entity: TaskViewPermission (مجوز خاص)

```csharp
public class TaskViewPermission
{
    public int Id { get; set; }
    
    public string GranteeUserId { get; set; }       // کاربر گیرنده مجوز
    
    // نوع مجوز
    public byte PermissionType { get; set; }
    // 0 = مشاهده تسک‌های یک کاربر خاص
    // 1 = مشاهده تسک‌های یک تیم خاص
    // 2 = مشاهده تسک‌های تیم و زیرتیم‌ها
    
    public string TargetUserId { get; set; }        // برای نوع 0
    public int? TargetTeamId { get; set; }          // برای نوع 1 و 2
    
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public string GrantedByUserId { get; set; }
    public DateTime GrantDate { get; set; }
    
    // Navigation
    public AppUsers Grantee { get; set; }
    public AppUsers TargetUser { get; set; }
    public Team TargetTeam { get; set; }
    public AppUsers GrantedBy { get; set; }
}
```

---

## 🎨 مثال عملی: نظارت بر اساس سمت

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

### توضیحات سناریو

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

نتیجه:
✅ علی می‌تواند تسک را ببیند چون:
   - مدیر تیم بازاریابی است
   - سمت بالاتر از حسین دارد (PowerLevel: 1 < 2)
   
❌ مدیر تیم فروش نمی‌تواند این تسک را ببیند چون:
   - AssignedInTeamId = تیم بازاریابی
   - تسک در تیم فروش assign نشده
```

---

## 📝 نمایش دلیل نظارت

### متد: GetSupervisionTypeAndReason

```csharp
public (string supervisionType, string supervisionReason) 
    GetSupervisionTypeAndReason(int taskId, string userId)
{
    // خروجی نمونه:
    // ("system", "شما مدیر تیم «بازاریابی» هستید")
    // ("system", "شما در تیم «بازاریابی» سمت بالاتر از حسین، علی دارید")
    // ("system", "شما ناظر رسمی تیم «فروش» هستید")
    // ("carbon-copy", "شما توسط مدیر در تاریخ 1403/10/15 به این تسک رونوشت شده‌اید")
}
```

### الگوریتم

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

---

## 🔍 Query های کاربردی

### تسک‌های نظارتی کاربر

```sql
-- تسک‌هایی که کاربر می‌تواند ببیند ولی assign نشده
SELECT t.*
FROM Tasks_Tbl t
WHERE t.Id IN (
    -- لیست شناسه‌های قابل مشاهده از GetVisibleTaskIdsAsync
)
AND t.Id NOT IN (
    SELECT TaskId 
    FROM TaskAssignment_Tbl 
    WHERE AssignedUserId = @UserId AND IsActive = 1
)
AND t.CreatorUserId != @UserId
ORDER BY t.CreateDate DESC;
```

### دریافت ناظران یک تسک

```sql
-- ناظران رونوشتی
SELECT u.FirstName, u.LastName, tv.AddedDate, tv.Note
FROM TaskViewer_Tbl tv
INNER JOIN AspNetUsers u ON tv.UserId = u.Id
WHERE tv.TaskId = @TaskId
  AND tv.IsActive = 1
  AND (tv.EndDate IS NULL OR tv.EndDate >= GETDATE());

-- ناظران سیستمی (نیاز به پردازش در کد)
EXEC GetTaskSupervisorsAsync @TaskId, @IncludeCreator;
```

---

## 💡 نکات کلیدی

### ✅ **فیلتر تیم (Team-Scoped)**

```csharp
// ❌ قبل از اصلاح:
var tasks = TaskAssignment که AssignedUserId در subordinateUserIds

// ✅ بعد از اصلاح:
var tasks = TaskAssignment که:
  - AssignedUserId در subordinateUserIds
  - AssignedInTeamId = تیم فعلی  ⭐
```

### ✅ **چرا فیلتر تیم مهم است؟**

```
مثال:
- شما: مدیر حسین در تیم "بازاریابی"
- حسین: عضو تیم "بازاریابی" و "فروش"

❌ بدون فیلتر تیم:
   → می‌بینید: تسک‌های حسین در "بازاریابی" + "فروش"

✅ با فیلتر تیم:
   → می‌بینید: فقط تسک‌های حسین در "بازاریابی"
```

---

**نسخه مستند:** 3.0.0  
**آخرین بروزرسانی:** آذر 1403  
**وضعیت:** ✅ Active & Updated

---

[🔙 بازگشت به فهرست](README.md) | [➡️ بعدی: تسک‌های زمان‌بندی شده](05-Scheduled-Task-Diagram.md)
