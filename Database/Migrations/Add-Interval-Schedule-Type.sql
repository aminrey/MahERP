-- Migration: Add Interval Schedule Type Support
-- تاریخ: 2024-12-20
-- توضیح: اضافه کردن فیلدهای IntervalDays و IntervalDayOfWeek برای پشتیبانی از زمان‌بندی با فاصله

-- ⭐⭐⭐ اضافه کردن فیلد IntervalDays
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ScheduledTaskCreation_Tbl' 
    AND COLUMN_NAME = 'IntervalDays'
)
BEGIN
    ALTER TABLE ScheduledTaskCreation_Tbl
    ADD IntervalDays INT NULL;
    
    PRINT '✅ فیلد IntervalDays اضافه شد';
END
ELSE
BEGIN
    PRINT 'ℹ️ فیلد IntervalDays از قبل وجود دارد';
END
GO

-- ⭐⭐⭐ اضافه کردن فیلد IntervalDayOfWeek
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ScheduledTaskCreation_Tbl' 
    AND COLUMN_NAME = 'IntervalDayOfWeek'
)
BEGIN
    ALTER TABLE ScheduledTaskCreation_Tbl
    ADD IntervalDayOfWeek INT NULL;
    
    PRINT '✅ فیلد IntervalDayOfWeek اضافه شد';
END
ELSE
BEGIN
    PRINT 'ℹ️ فیلد IntervalDayOfWeek از قبل وجود دارد';
END
GO

-- ⭐⭐⭐ اضافه کردن Comment/Description
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'فاصله زمانی به روز (برای ScheduleType=4)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ScheduledTaskCreation_Tbl',
    @level2type = N'COLUMN', @level2name = N'IntervalDays';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'روز هفته برای فاصله (0=Sunday, 6=Saturday, null=هر روز)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ScheduledTaskCreation_Tbl',
    @level2type = N'COLUMN', @level2name = N'IntervalDayOfWeek';
GO

-- ⭐⭐⭐ بروزرسانی Comment برای ScheduleType
EXEC sp_updateextendedproperty 
    @name = N'MS_Description', 
    @value = N'0=OneTime, 1=Daily, 2=Weekly, 3=Monthly, 4=Interval', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'ScheduledTaskCreation_Tbl',
    @level2type = N'COLUMN', @level2name = N'ScheduleType';
GO

PRINT '✅✅✅ Migration اعمال شد - Interval Schedule Type اضافه شد';
GO

-- ⭐⭐⭐ نمونه Query برای تست
/*
-- ساخت یک Schedule با Interval
INSERT INTO ScheduledTaskCreation_Tbl (
    ScheduleTitle, ScheduleDescription, TaskDataJson,
    ScheduleType, ScheduledTime, IntervalDays, IntervalDayOfWeek,
    StartDate, IsRecurring, IsScheduleEnabled, IsActive,
    ExecutionCount, IsExecuted, CreatedByUserId, CreatedDate, BranchId
) VALUES (
    'تست هر 14 روز - شنبه‌ها',
    'تست Schedule با فاصله 14 روز فقط شنبه‌ها',
    '{}',
    4, -- Interval
    '14:00',
    14, -- هر 14 روز
    6, -- شنبه (Saturday)
    GETUTCDATE(),
    1, -- تکراری
    1, -- فعال
    1, -- اکتیو
    0, -- تعداد اجرا
    0, -- اجرا نشده
    'SYSTEM',
    GETUTCDATE(),
    NULL
);

-- بررسی
SELECT TOP 1 * 
FROM ScheduledTaskCreation_Tbl 
WHERE ScheduleType = 4 
ORDER BY CreatedDate DESC;
*/
