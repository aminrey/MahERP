using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// متدهای کمکی عمومی
    /// شامل: تبدیل‌ها، محاسبات، فرمت‌دهی
    /// </summary>
    public partial class TaskRepository 
    {
        #region String Helpers

        public string GetUserInitials(string firstName, string lastName)
        {
            var initials = "";
            if (!string.IsNullOrEmpty(firstName)) initials += firstName[0];
            if (!string.IsNullOrEmpty(lastName)) initials += lastName[0];
            return string.IsNullOrEmpty(initials) ? "کاربر" : initials;
        }

        #endregion

        #region Task Status Helpers

        public string GetTaskStatusText(byte status)
        {
            return status switch
            {
                0 => "ایجاد شده",
                1 => "در حال انجام",
                2 => "تکمیل شده",
                3 => "تأیید شده",
                4 => "رد شده",
                5 => "در انتظار",
                _ => "نامشخص"
            };
        }

        public string GetTaskStatusBadgeClass(byte status)
        {
            return status switch
            {
                0 => "bg-secondary",
                1 => "bg-warning",
                2 => "bg-success",
                3 => "bg-info",
                4 => "bg-danger",
                5 => "bg-primary",
                _ => "bg-dark"
            };
        }



        #endregion

        #region Calendar Helpers

        public string GetTaskStatusColor(TaskCalendarViewModel task)
        {
            if (task.IsCompleted) return "#28a745"; // سبز
            if (task.IsOverdue) return "#dc3545";   // قرمز
            return "#007bff";                       // آبی
        }

        public string GetTaskStatusTextForCalendar(TaskCalendarViewModel task)
        {
            if (task.IsCompleted) return "تکمیل شده";
            if (task.IsOverdue) return "عقب افتاده";
            return "در حال انجام";
        }

        #endregion

        #region Activity Helpers

        private string GetTaskStakeholderName(int taskId)
        {
            try
            {
                var stakeholderName = _context.Tasks_Tbl
                    .Where(t => t.Id == taskId)
                    .Join(_context.Stakeholder_Tbl,
                          t => t.StakeholderId,
                          s => s.Id,
                          (t, s) => new { s.FirstName, s.LastName, s.CompanyName })
                    .Select(s => !string.IsNullOrEmpty(s.CompanyName) ? s.CompanyName : $"{s.FirstName} {s.LastName}")
                    .FirstOrDefault();

                return stakeholderName ?? "ندارد";
            }
            catch (Exception)
            {
                return "ندارد";
            }
        }

        private string GetActivityTitle(byte status)
        {
            return status switch
            {
                0 => "تسک ایجاد شد",
                1 => "تسک در حال انجام است",
                2 => "تسک تکمیل شد",
                3 => "تسک تایید شد",
                4 => "تسک رد شد",
                5 => "تسک در انتظار است",
                _ => "وضعیت تسک تغییر کرد"
            };
        }

        private string CalculateTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "اکنون";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ساعت پیش";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} روز پیش";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} ماه پیش";
            
            return $"{(int)(timeSpan.TotalDays / 365)} سال پیش";
        }

        #endregion

        #region Statistics Helpers

        private async Task<int> GetActiveRemindersCountAsync(string userId)
        {
            try
            {
                return await _context.TaskReminderEvent_Tbl
                    .Where(r => r.RecipientUserId == userId && 
                               !r.IsRead && 
                               r.ScheduledDateTime <= DateTime.Now)
                    .CountAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        #region Task Relations Helpers

        public async Task<List<string>> GetTaskRelatedUserIdsAsync(int taskId)
        {
            var userIds = new HashSet<string>();

            // دریافت اطلاعات تسک
            var task = await _context.Tasks_Tbl
                .Where(t => t.Id == taskId)
                .Select(t => new { t.CreatorUserId, t.BranchId })
                .FirstOrDefaultAsync();

            if (task == null) return new List<string>();

            // سازنده تسک
            if (!string.IsNullOrEmpty(task.CreatorUserId))
            {
                userIds.Add(task.CreatorUserId);
            }

            // اعضای تسک (TaskAssignments)
            var assignedUserIds = await _context.TaskAssignment_Tbl
                .Where(ta => ta.TaskId == taskId)
                .Select(ta => ta.AssignedUserId)
                .ToListAsync();

            foreach (var userId in assignedUserIds)
            {
                userIds.Add(userId);
            }

            // ناظران تسک
            var supervisors = await GetTaskSupervisorsAsync(taskId, includeCreator: false);
            foreach (var supervisorId in supervisors)
            {
                userIds.Add(supervisorId);
            }

            return userIds.ToList();
        }

        #endregion

        #region Progress Calculation

        private int CalculateTaskProgress(Tasks task)
        {
            if (task.Status >= 2) return 100; // تکمیل شده یا بالاتر

            var totalOperations = task.TaskOperations?.Count ?? 0;
            if (totalOperations == 0) return task.Status * 25; // بر اساس وضعیت

            var completedOperations = task.TaskOperations?.Count(x => x.IsCompleted) ?? 0;
            return (int)((double)completedOperations / totalOperations * 100);
        }

        #endregion

      
    }
}
