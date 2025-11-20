using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت تکمیل تسک‌ها
    /// </summary>
    public partial class TaskRepository
    {
        #region Task Completion Management

        /// <summary>
        /// آماده‌سازی مودال تکمیل تسک
        /// </summary>
        public async Task<CompleteTaskViewModel> PrepareCompleteTaskModalAsync(int taskId, string userId)
        {
            try
            {
                var task = await _context.Tasks_Tbl
                    .Include(t => t.TaskOperations)
                    .Include(t => t.TaskAssignments)
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.IsActive);

                if (task == null)
                    return null;

                // بررسی دسترسی
                var hasAccess = await CanUserViewTaskAsync(userId, taskId);
                if (!hasAccess)
                    return null;

                // بررسی Assignment
                var assignment = task.TaskAssignments
                    .FirstOrDefault(a => a.AssignedUserId == userId);

                if (assignment == null)
                    return null;

                // شمارش عملیات و اعضا
                var pendingOperationsCount = task.TaskOperations
                    ?.Count(o => !o.IsCompleted && !o.IsDeleted) ?? 0;

                var totalMembers = task.TaskAssignments.Count;
                var completedMembers = task.TaskAssignments.Count(a => a.CompletionDate.HasValue);

                var model = new CompleteTaskViewModel
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    TaskCode = task.TaskCode,
                    AllOperationsCompleted = pendingOperationsCount == 0,
                    PendingOperationsCount = pendingOperationsCount,
                    IsAlreadyCompleted = assignment.CompletionDate.HasValue,

                    // ⭐⭐⭐ اطلاعات جدید
                    IsIndependentCompletion = task.IsIndependentCompletion,
                    TotalMembers = totalMembers,
                    CompletedMembers = completedMembers
                };

                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ثبت تکمیل تسک - با پشتیبانی از دو نوع تکمیل (مستقل و مشترک)
        /// </summary>
        public async Task<(bool IsSuccess, string ErrorMessage, bool IsFullyCompleted)> CompleteTaskAsync(
            CompleteTaskViewModel model,
            string userId)
        {
            try
            {
                // ⭐ بارگذاری تسک با اطلاعات کامل
                var task = await _context.Tasks_Tbl
                    .Include(t => t.TaskAssignments)
                    .Include(t => t.TaskOperations)
                    .FirstOrDefaultAsync(t => t.Id == model.TaskId && t.IsActive);

                if (task == null)
                    return (false, "تسک یافت نشد", false);

                // ⭐ بررسی وجود Assignment برای کاربر
                var assignment = task.TaskAssignments
                    .FirstOrDefault(a => a.AssignedUserId == userId);

                if (assignment == null)
                    return (false, "شما به این تسک اختصاص داده نشده‌اید", false);

                // ⭐ بررسی تکمیل قبلی
                if (assignment.CompletionDate.HasValue)
                {
                    // ⭐ فقط بروزرسانی گزارش
                    assignment.UserReport = model.CompletionReport;
                    assignment.ReportDate = DateTime.Now;

                    _context.TaskAssignment_Tbl.Update(assignment);
                    await _context.SaveChangesAsync();

                    return (true, "گزارش تکمیل بروزرسانی شد", true);
                }

                var completionDate = DateTime.Now;

                // ⭐ 1. بروزرسانی Assignment کاربر فعلی
                assignment.CompletionDate = completionDate;
                assignment.UserReport = model.CompletionReport;
                assignment.ReportDate = completionDate;
                assignment.Status = 2; // تکمیل شده
                _context.TaskAssignment_Tbl.Update(assignment);

                // ⭐ 2. بررسی نوع تکمیل تسک
                bool isFullyCompleted = false;

                if (task.IsIndependentCompletion)
                {
                    // ========================================
                    // ⭐⭐⭐ حالت مستقل (Independent)
                    // ========================================

                    Console.WriteLine($"✅ تکمیل مستقل: فقط برای کاربر {userId}");

                    // بررسی آیا همه کاربران تکمیل کردند
                    var totalAssignments = task.TaskAssignments.Count;
                    var completedAssignments = task.TaskAssignments.Count(a => a.CompletionDate.HasValue);

                    if (completedAssignments == totalAssignments)
                    {
                        // ⭐ همه افراد تکمیل کردند - تکمیل نهایی تسک
                        task.Status = 2; // تکمیل شده
                        task.LastUpdateDate = completionDate;
                        isFullyCompleted = true;

                        // ⭐ تکمیل عملیات باقیمانده (اگر وجود دارد)
                        await CompleteRemainingOperationsAsync(task, userId, completionDate);

                        // ⭐ غیرفعال کردن یادآوری‌ها
                        await DeactivateTaskRemindersAsync(task.Id);

                        Console.WriteLine($"✅ همه افراد تکمیل کردند - تسک کامل شد");
                    }
                    else
                    {
                        // ⭐ هنوز برخی افراد تکمیل نکرده‌اند
                        task.LastUpdateDate = completionDate;
                        isFullyCompleted = false;

                        Console.WriteLine($"⏳ {completedAssignments}/{totalAssignments} نفر تکمیل کردند");
                    }
                }
                else
                {
                    // ========================================
                    // ⭐⭐⭐ حالت مشترک (Shared)
                    // ========================================

                    Console.WriteLine($"✅ تکمیل مشترک: یک نفر تکمیل کرد، همه تکمیل می‌شوند");

                    // ⭐ تکمیل برای همه Assignments
                    foreach (var otherAssignment in task.TaskAssignments)
                    {
                        if (otherAssignment.Id != assignment.Id && !otherAssignment.CompletionDate.HasValue)
                        {
                            otherAssignment.CompletionDate = completionDate;
                            otherAssignment.Status = 2;
                            otherAssignment.UserReport = $"تکمیل شده توسط {assignment.AssignedUser?.FirstName ?? "همکار"}";
                            _context.TaskAssignment_Tbl.Update(otherAssignment);
                        }
                    }

                    // ⭐ تکمیل کل تسک
                    task.Status = 2;
                    task.LastUpdateDate = completionDate;
                    isFullyCompleted = true;

                    // ⭐ تکمیل همه عملیات باقیمانده
                    await CompleteRemainingOperationsAsync(task, userId, completionDate);

                    // ⭐ غیرفعال کردن یادآوری‌ها
                    await DeactivateTaskRemindersAsync(task.Id);
                }

                // ⭐ ذخیره تغییرات
                _context.Tasks_Tbl.Update(task);
                await _context.SaveChangesAsync();

                // ⭐ ثبت در تاریخچه
                await _taskHistoryRepository.LogTaskCompletedAsync(
                    task.Id,
                    userId,
                    task.Title,
                    task.TaskCode,
                    isFullyCompleted);

                // ⭐ پیام مناسب
                string message = task.IsIndependentCompletion
                    ? (isFullyCompleted
                        ? "✅ تسک با موفقیت تکمیل شد - همه افراد تکمیل کردند"
                        : "✅ تسک برای شما تکمیل شد - منتظر تکمیل سایر افراد")
                    : "✅ تسک با موفقیت تکمیل و برای همه قفل شد";

                return (true, message, isFullyCompleted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in CompleteTaskAsync: {ex.Message}\n{ex.StackTrace}");
                return (false, $"خطا در ثبت تکمیل: {ex.Message}", false);
            }
        }

       

        #endregion
    }
}
