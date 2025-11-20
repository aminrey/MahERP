using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// سلسله مراتب تیمی و گروه‌بندی تسک‌ها
    /// </summary>
    public partial class TaskRepository
    {
        #region Hierarchical Task Methods

        /// <summary>
        /// دریافت تسک‌ها گروه‌بندی شده بر اساس سلسله مراتب تیمی
        /// </summary>
        public async Task<TaskGroupedViewModel> GetHierarchicalTasksForUserAsync(string userId)
        {
            var result = new TaskGroupedViewModel();

            // 1. تسک‌های شخصی کاربر
            var myTasks = GetTasksByUser(userId, includeAssigned: true, includeCreated: true);
            result.MyTasks = myTasks.Select(MapToTaskViewModel).ToList();

            // 2. تسک‌های اعضای تیم‌هایی که کاربر مدیر آن‌هاست
            await LoadTeamMemberTasks(result, userId);

            // 3. تسک‌های تیم‌های زیرمجموعه
            await LoadSubTeamTasks(result, userId);

            return result;
        }

        /// <summary>
        /// بارگذاری تسک‌های اعضای تیم
        /// </summary>
        private async Task LoadTeamMemberTasks(TaskGroupedViewModel result, string userId)
        {
            var managedTeams = _context.Team_Tbl.Where(t => t.ManagerUserId == userId && t.IsActive).ToList();

            foreach (var team in managedTeams)
            {
                var teamMembers = _context.TeamMember_Tbl
                    .Include(tm => tm.User)
                    .Where(tm => tm.TeamId == team.Id && tm.IsActive && tm.UserId != userId)
                    .ToList();

                foreach (var member in teamMembers)
                {
                    var memberTasks = GetTasksByUser(member.UserId, includeAssigned: true, includeCreated: true);
                    if (memberTasks.Any())
                    {
                        var memberName = $"{member.User.FirstName} {member.User.LastName}";
                        result.TeamMemberTasks[memberName] = memberTasks.Select(MapToTaskViewModel).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// بارگذاری تسک‌های زیرتیم‌ها
        /// </summary>
        private async Task LoadSubTeamTasks(TaskGroupedViewModel result, string userId)
        {
            var managedTeams = _context.Team_Tbl.Where(t => t.ManagerUserId == userId && t.IsActive).ToList();

            foreach (var parentTeam in managedTeams)
            {
                await LoadSubTeamTasksRecursive(result, parentTeam.Id, parentTeam.Title);
            }
        }

        /// <summary>
        /// بارگذاری بازگشتی تسک‌های زیرتیم‌ها
        /// </summary>
        private async Task LoadSubTeamTasksRecursive(TaskGroupedViewModel result, int parentTeamId, string parentTeamTitle)
        {
            var subTeams = _context.Team_Tbl.Where(t => t.ParentTeamId == parentTeamId && t.IsActive).ToList();

            foreach (var subTeam in subTeams)
            {
                var subTeamTasks = new List<TaskViewModel>();

                // تسک‌های مدیر زیرتیم
                if (!string.IsNullOrEmpty(subTeam.ManagerUserId))
                {
                    var managerTasks = GetTasksByUser(subTeam.ManagerUserId, includeAssigned: true, includeCreated: true);
                    subTeamTasks.AddRange(managerTasks.Select(MapToTaskViewModel));
                }

                // تسک‌های اعضای زیرتیم
                var subTeamMembers = _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == subTeam.Id && tm.IsActive)
                    .ToList();

                foreach (var member in subTeamMembers)
                {
                    var memberTasks = GetTasksByUser(member.UserId, includeAssigned: true, includeCreated: true);
                    subTeamTasks.AddRange(memberTasks.Select(MapToTaskViewModel));
                }

                if (subTeamTasks.Any())
                {
                    // حذف تکرارها
                    var uniqueTasks = subTeamTasks.GroupBy(t => t.Id).Select(g => g.First()).ToList();
                    result.SubTeamTasks[$"{parentTeamTitle} > {subTeam.Title}"] = uniqueTasks;
                }

                // بررسی زیرتیم‌های بیشتر (بازگشتی)
                await LoadSubTeamTasksRecursive(result, subTeam.Id, $"{parentTeamTitle} > {subTeam.Title}");
            }
        }

        /// <summary>
        /// تبدیل Task Entity به TaskViewModel - اصلاح شده برای حذف Stakeholder
        /// </summary>
        private TaskViewModel MapToTaskViewModel(Tasks task)
        {
            // دریافت انتساب‌های تسک
            var assignments = _context.TaskAssignment_Tbl
                .Include(ta => ta.AssignedUser)
                .Where(ta => ta.TaskId == task.Id)
                .ToList();

            // دریافت اطلاعات دسته‌بندی
            var category = _context.TaskCategory_Tbl.FirstOrDefault(c => c.Id == task.TaskCategoryId);

            // ⭐⭐⭐ دریافت اطلاعات Contact
            Contact contact = null;
            if (task.ContactId.HasValue)
            {
                contact = _context.Contact_Tbl.FirstOrDefault(c => c.Id == task.ContactId.Value);
            }

            // ⭐⭐⭐ دریافت اطلاعات Organization
            Organization organization = null;
            if (task.OrganizationId.HasValue)
            {
                organization = _context.Organization_Tbl.FirstOrDefault(o => o.Id == task.OrganizationId.Value);
            }

            return new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TaskCode = task.TaskCode,
                CreateDate = task.CreateDate,
                DueDate = task.DueDate,
                ManagerApprovedDate = task.ManagerApprovedDate,
                SupervisorApprovedDate = task.SupervisorApprovedDate,
                IsActive = task.IsActive,
                IsDeleted = task.IsDeleted,
                BranchId = task.BranchId,
                CreatorUserId = task.CreatorUserId,

                // ⭐⭐⭐ OLD - Stakeholder (Deprecated - حذف شده)
                StakeholderId = task.StakeholderId,
                StakeholderName = null, // ⚠️ دیگر Stakeholder وجود ندارد

                // ⭐⭐⭐ NEW - Contact & Organization
                SelectedContactId = task.ContactId,
                ContactFullName = contact != null
                    ? $"{contact.FirstName} {contact.LastName}"
                    : null,

                SelectedOrganizationId = task.OrganizationId,
                OrganizationName = organization?.DisplayName,

                // بقیه فیلدها...
                TaskType = task.TaskType,
                CategoryId = task.TaskCategoryId,
                CategoryTitle = category?.Title,
                Priority = task.Priority,
                Important = task.Important,
                Status = task.Status,
                VisibilityLevel = task.VisibilityLevel,
                LastUpdateDate = task.LastUpdateDate,
                TaskTypeInput = task.TaskTypeInput,
                CreationMode = task.CreationMode,

                // Assignments
                AssignmentsTaskUser = assignments
                    .Where(a => !string.IsNullOrEmpty(a.AssignedUserId))
                    .Select(a => new TaskAssignmentViewModel
                    {
                        Id = a.Id,
                        TaskId = a.TaskId,
                        AssignedUserId = a.AssignedUserId,
                        AssignedUserName = a.AssignedUser != null
                            ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                            : "نامشخص",
                        AssignerUserId = a.AssignerUserId,
                        AssignDate = a.AssignmentDate,
                        CompletionDate = a.CompletionDate,
                        Description = a.Description,
                        IsFocused = a.IsFocused,
                        FocusedDate = a.FocusedDate
                    }).ToList()
            };
        }

        #endregion
    }
}
