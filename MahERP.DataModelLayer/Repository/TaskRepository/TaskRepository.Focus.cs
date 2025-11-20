using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت فوکوس تسک (Task Focus)
    /// شامل: تنظیم فوکوس، حذف فوکوس، دریافت تسک فوکوس شده
    /// </summary>
    public partial class TaskRepository
    {
        #region Task Focus Methods

        /// <summary>
        /// تنظیم فوکوس کاربر روی یک تسک (فقط یک تسک می‌تواند فوکوس باشد)
        /// </summary>
        public async Task<(bool Success, string Message)> SetTaskFocusAsync(
            int taskId,
            string userId)
        {
            try
            {
                // بررسی اینکه کاربر عضو تسک است
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);

                if (assignment == null)
                {
                    return (false, "شما عضو این تسک نیستید");
                }

                // حذف فوکوس از تسک‌های قبلی
                var previousFocusedAssignments = await _context.TaskAssignment_Tbl
                    .Where(ta => ta.AssignedUserId == userId && ta.IsFocused)
                    .ToListAsync();

                foreach (var prevAssignment in previousFocusedAssignments)
                {
                    prevAssignment.IsFocused = false;
                    prevAssignment.FocusedDate = null;
                }

                // تنظیم فوکوس جدید
                assignment.IsFocused = true;
                assignment.FocusedDate = DateTime.Now;

                _context.TaskAssignment_Tbl.UpdateRange(previousFocusedAssignments);
                _context.TaskAssignment_Tbl.Update(assignment);
                await _context.SaveChangesAsync();

                return (true, "تسک به عنوان فوکوس تنظیم شد");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SetTaskFocusAsync: {ex.Message}");
                return (false, $"خطا در تنظیم فوکوس: {ex.Message}");
            }
        }

        /// <summary>
        /// حذف فوکوس از یک تسک
        /// </summary>
        public async Task<(bool Success, string Message)> RemoveTaskFocusAsync(
            int taskId,
            string userId)
        {
            try
            {
                var assignment = await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(ta => ta.TaskId == taskId && 
                                              ta.AssignedUserId == userId && 
                                              ta.IsFocused);

                if (assignment == null)
                {
                    return (false, "این تسک فوکوس نیست");
                }

                assignment.IsFocused = false;
                assignment.FocusedDate = null;

                _context.TaskAssignment_Tbl.Update(assignment);
                await _context.SaveChangesAsync();

                return (true, "فوکوس از تسک حذف شد");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in RemoveTaskFocusAsync: {ex.Message}");
                return (false, $"خطا در حذف فوکوس: {ex.Message}");
            }
        }

        /// <summary>
        /// دریافت شناسه تسک فوکوس شده کاربر
        /// </summary>
        public async Task<int?> GetUserFocusedTaskIdAsync(string userId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .Where(ta => ta.AssignedUserId == userId && ta.IsFocused)
                    .Select(ta => (int?)ta.TaskId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetUserFocusedTaskIdAsync: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
