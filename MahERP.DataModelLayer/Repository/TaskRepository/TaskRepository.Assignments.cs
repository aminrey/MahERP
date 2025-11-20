using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت انتساب‌ها و افراد تسک (Task Assignments)
    /// شامل: اضافه کردن، حذف، گرفتن لیست انتساب‌ها
    /// </summary>
    public partial class TaskRepository
    {
        #region Task Assignments CRUD

        public List<TaskAssignment> GetTaskAssignments(int taskId)
        {
            return _context.TaskAssignment_Tbl
                .Include(a => a.AssignedUser)
                .Where(a => a.TaskId == taskId)
                .ToList();
        }

        public TaskAssignment GetTaskAssignmentById(int id)
        {
            return _context.TaskAssignment_Tbl
                .Include(a => a.AssignedUser)
                .Include(a => a.Task)
                .FirstOrDefault(a => a.Id == id);
        }

        public async Task<TaskAssignment> GetTaskAssignmentByIdAsync(int assignmentId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .Include(a => a.Task)
                    .Include(a => a.AssignedUser)
                    .Include(a => a.AssignerUser)
                    .FirstOrDefaultAsync(a => a.Id == assignmentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTaskAssignmentByIdAsync: {ex.Message}");
                return null;
            }
        }

        public TaskAssignment GetTaskAssignmentByUserAndTask(string userId, int taskId)
        {
            return _context.TaskAssignment_Tbl
                .FirstOrDefault(a => a.AssignedUserId == userId && a.TaskId == taskId);
        }

        public async Task<TaskAssignment> GetTaskAssignmentByUserAndTaskAsync(string userId, int taskId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedUserId == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTaskAssignmentByUserAndTaskAsync: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Task Assignment Operations

        public async Task HandleTaskAssignmentsAsync(Tasks task, TaskViewModel model, string currentUserId)
        {
            try
            {
                var assignedUserIds = model.AssignmentsSelectedTaskUserArraysString ?? new List<string>();
                var assignedTeamIds = model.AssignmentsSelectedTeamIds ?? new List<int>();

                // کاربران از تیم‌ها
                var teamUserIds = await GetUsersFromTeamsAsync(assignedTeamIds);
                var allAssignedUserIds = assignedUserIds.Union(teamUserIds).Distinct().ToList();

                // اختصاص به سایرین
                foreach (var assignedUserId in allAssignedUserIds)
                {
                    var assignment = new TaskAssignment
                    {
                        TaskId = task.Id,
                        AssignedUserId = assignedUserId,
                        AssignerUserId = currentUserId,
                        AssignmentType = 0,
                        AssignmentDate = DateTime.Now,
                        Description = assignedUserIds.Contains(assignedUserId) ? "انتصاب مستقیم" : "انتصاب از طریق تیم",
                        Status = 0,
                    };
                    _unitOfWork.TaskAssignmentUW.Create(assignment);
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در مدیریت انتصاب‌ها: {ex.Message}", ex);
            }
        }

        public async Task HandleTaskAssignmentsBulkAsync(Tasks task, TaskViewModel model, string currentUserId)
        {
            try
            {
                var assignedUserIds = model.AssignmentsSelectedTaskUserArraysString ?? new List<string>();
                var assignedTeamIds = model.AssignmentsSelectedTeamIds ?? new List<int>();

                // پاکسازی userId ها
                assignedUserIds = assignedUserIds
                    .SelectMany(id => {
                        if (string.IsNullOrWhiteSpace(id)) return Enumerable.Empty<string>();

                        var cleaned = id
                            .Trim()
                            .Trim('[', ']', '/', ' ', '\t', '\n', '\r', '"', '\'', '`')
                            .Replace("[", "").Replace("]", "")
                            .Replace("/", "").Replace("\\", "")
                            .Replace("\"", "").Replace("'", "")
                            .Trim();

                        if (cleaned.Contains(","))
                        {
                            return cleaned.Split(',')
                                .Select(s => s.Trim().Trim('"', '\'', '[', ']'))
                                .Where(s => !string.IsNullOrWhiteSpace(s));
                        }

                        return new[] { cleaned };
                    })
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                // Parse User-Team Map
                Dictionary<string, int> userTeamMap = new Dictionary<string, int>();
                if (!string.IsNullOrEmpty(model.UserTeamAssignmentsJson))
                {
                    try
                    {
                        var rawMap = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(
                            model.UserTeamAssignmentsJson) ?? new Dictionary<string, int>();

                        foreach (var kvp in rawMap)
                        {
                            var cleanedKey = kvp.Key
                                .Trim('[', ']', '/', ' ', '"', '\'')
                                .Replace("[", "").Replace("]", "").Replace("/", "")
                                .Trim();

                            if (!string.IsNullOrWhiteSpace(cleanedKey))
                            {
                                userTeamMap[cleanedKey] = kvp.Value;
                            }
                        }
                    }
                    catch
                    {
                        // Silent fail - userTeamMap remains empty
                    }
                }

                // دریافت کاربران از تیم‌ها
                var teamUserIds = await GetUsersFromTeamsAsync(assignedTeamIds);
                teamUserIds = teamUserIds
                    .Select(id => id?.Trim('[', ']', '/', ' ', '"').Replace("[", "").Replace("]", "").Trim())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                var allAssignedUserIds = assignedUserIds.Union(teamUserIds).Distinct().ToList();

                if (!allAssignedUserIds.Any())
                {
                    return;
                }

                // اعتبارسنجی در دیتابیس
                var existingUserIds = await _context.Users
                    .AsNoTracking()
                    .Where(u => allAssignedUserIds.Contains(u.Id))
                    .Select(u => u.Id)
                    .ToListAsync();

                var invalidUserIds = allAssignedUserIds.Except(existingUserIds).ToList();
                if (invalidUserIds.Any())
                {
                    var invalidUsersStr = string.Join(", ", invalidUserIds.Select(id => $"'{id}'"));
                    throw new InvalidOperationException($"کاربران زیر یافت نشدند: {invalidUsersStr}");
                }

                // ایجاد assignments
                var assignments = new List<TaskAssignment>();
                var assignmentDate = DateTime.Now;

                foreach (var assignedUserId in existingUserIds)
                {
                    int? assignedInTeamId = null;

                    // تعیین تیم مربوطه
                    if (userTeamMap.ContainsKey(assignedUserId))
                    {
                        var teamId = userTeamMap[assignedUserId];
                        assignedInTeamId = teamId == 0 ? null : (int?)teamId;
                    }
                    else if (teamUserIds.Contains(assignedUserId))
                    {
                        var userTeams = await GetUserTeamsInBranchAsync(assignedUserId, task.BranchId ?? 0);
                        assignedInTeamId = userTeams.FirstOrDefault()?.Id;
                    }

                    var assignment = new TaskAssignment
                    {
                        TaskId = task.Id,
                        AssignedUserId = assignedUserId,
                        AssignerUserId = currentUserId,
                        AssignmentType = 0,
                        AssignmentDate = assignmentDate,
                        Description = assignedUserIds.Contains(assignedUserId)
                            ? "انتصاب مستقیم"
                            : "انتصاب از طریق تیم",
                        Status = 0,
                        AssignedInTeamId = assignedInTeamId,
                        DueDate = task.DueDate,
                        StartDate = task.StartDate,
                        IsRead = false,
                        IsFavorite = false,
                        IsMyDay = false
                    };

                    assignments.Add(assignment);
                }

                if (assignments.Any())
                {
                    _context.TaskAssignment_Tbl.AddRange(assignments);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در مدیریت انتصاب‌ها: {ex.Message}", ex);
            }
        }

        public async Task<bool> AssignUserToTaskAsync(
            int taskId,
            string userId,
            string assignerUserId,
            int? teamId = null,
            string description = null)
        {
            try
            {
                var assignment = new TaskAssignment
                {
                    TaskId = taskId,
                    AssignedUserId = userId,
                    AssignerUserId = assignerUserId,
                    AssignedInTeamId = teamId == 0 ? null : teamId,
                    AssignmentDate = DateTime.Now,
                    Description = description ?? "تخصیص مستقیم",
                    Status = 0,
                    AssignmentType = 0,
                    IsRead = false,
                    IsFavorite = false,
                    IsMyDay = false
                };

                _context.TaskAssignment_Tbl.Add(assignment);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignUserToTaskAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveTaskAssignmentAsync(int assignmentId)
        {
            try
            {
                var assignment = await _context.TaskAssignment_Tbl.FindAsync(assignmentId);

                if (assignment == null)
                    return false;

                _context.TaskAssignment_Tbl.Remove(assignment);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveTaskAssignmentAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetTaskAssignedUserIdsAsync(int taskId)
        {
            return await _context.TaskAssignment_Tbl
                .Where(a => a.TaskId == taskId)
                .Select(a => a.AssignedUserId)
                .Distinct()
                .ToListAsync();
        }

        #endregion

        #region Personal Dates Management

        public async Task<TaskAssignment> GetTaskAssignmentForPersonalDatesAsync(int taskId, string userId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .Include(ta => ta.Task)
                    .Include(ta => ta.AssignedUser)
                    .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.AssignedUserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت انتساب تسک برای تاریخ‌های شخصی: {ex.Message}", ex);
            }
        }

        public async Task<TaskAssignment> GetTaskAssignmentByIdForPersonalDatesAsync(int assignmentId, string userId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .Include(ta => ta.Task)
                    .Include(ta => ta.AssignedUser)
                    .FirstOrDefaultAsync(ta => ta.Id == assignmentId && ta.AssignedUserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت انتساب تسک بر اساس شناسه: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePersonalDatesAsync(int assignmentId, string userId, DateTime? personalStartDate, DateTime? personalEndDate, string personalTimeNote)
        {
            try
            {
                var assignment = await _context.TaskAssignment_Tbl
                    .Include(ta => ta.Task)
                    .FirstOrDefaultAsync(ta => ta.Id == assignmentId && ta.AssignedUserId == userId);

                if (assignment == null)
                {
                    return false;
                }

                // بررسی امکان تغییر تاریخ‌ها (فقط قبل از تکمیل)
                if (assignment.Status >= 3)
                {
                    return false;
                }

                assignment.PersonalStartDate = personalStartDate;
                assignment.PersonalEndDate = personalEndDate;
                assignment.PersonalTimeNote = personalTimeNote;
                assignment.PersonalDatesUpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در بروزرسانی تاریخ‌های شخصی: {ex.Message}", ex);
            }
        }

        public async Task<List<TaskAssignment>> GetTaskAssignmentsWithPersonalDatesAsync(int taskId)
        {
            try
            {
                return await _context.TaskAssignment_Tbl
                    .Include(ta => ta.AssignedUser)
                    .Include(ta => ta.Task)
                    .Where(ta => ta.TaskId == taskId &&
                               (ta.PersonalStartDate.HasValue || ta.PersonalEndDate.HasValue))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت انتساب‌های تسک با تاریخ‌های شخصی: {ex.Message}", ex);
            }
        }

        #endregion

        #region Helper Methods - Private

        private async Task<List<Team>> GetUserTeamsInBranchAsync(string userId, int branchId)
        {
            return await _context.TeamMember_Tbl
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId &&
                            tm.IsActive &&
                            tm.Team.BranchId == branchId &&
                            tm.Team.IsActive)
                .Select(tm => tm.Team)
                .Distinct()
                .ToListAsync();
        }

        #endregion
    }
}
