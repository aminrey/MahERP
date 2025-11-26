-- =============================================
-- Script: Add ScheduledDaysOfMonth to TaskReminderSchedule
-- Date: 2024-12-20
-- Description: اضافه کردن فیلد ScheduledDaysOfMonth برای پشتیبانی از یادآوری ماهانه با چند روز
-- =============================================

USE [MahERP_DB]
GO

-- بررسی وجود ستون قبل از اضافه کردن
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'TaskReminderSchedule_Tbl' 
    AND COLUMN_NAME = 'ScheduledDaysOfMonth'
)
BEGIN
    PRINT '✅ Adding column ScheduledDaysOfMonth to TaskReminderSchedule_Tbl...'
    
    ALTER TABLE [dbo].[TaskReminderSchedule_Tbl]
    ADD [ScheduledDaysOfMonth] NVARCHAR(100) NULL
    
    PRINT '✅ Column added successfully!'
END
ELSE
BEGIN
    PRINT 'ℹ️ Column ScheduledDaysOfMonth already exists in TaskReminderSchedule_Tbl'
END
GO

-- اضافه کردن Extended Property برای توضیحات
EXEC sys.sp_addextendedproperty 
    @name=N'MS_Description', 
    @value=N'روزهای ماه برای یادآوری ماهانه (مثال: "10,20,25" برای روزهای 10، 20، 25 هر ماه)' , 
    @level0type=N'SCHEMA',@level0name=N'dbo', 
    @level1type=N'TABLE',@level1name=N'TaskReminderSchedule_Tbl', 
    @level2type=N'COLUMN',@level2name=N'ScheduledDaysOfMonth'
GO

PRINT ''
PRINT '========================================='
PRINT '✅ Migration completed successfully!'
PRINT '========================================='
PRINT ''
PRINT '📋 نمونه استفاده:'
PRINT '   - ReminderType = 4 (ماهانه - چند روز)'
PRINT '   - ScheduledDaysOfMonth = "10,20,25"'
PRINT '   - NotificationTime = "09:00:00"'
PRINT ''
PRINT '📌 این یادآوری در روزهای 10، 20، 25 هر ماه راس ساعت 09:00 ارسال خواهد شد'
PRINT ''
GO
