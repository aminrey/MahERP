using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.EntityFrameworkCore; // اضافه کردن این using برای ToListAsync
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
{
    public class UserManagerRepository : IUserManagerRepository
    {

        private readonly AppDbContext _Context;


        public UserManagerRepository(AppDbContext Context)
        {
            _Context = Context;

        }

        /// <summary>
        /// دریافت لیست کاربران بر اساس شناسه شعبه - کاربرانی که به شعبه مشخص اختصاص نداده شده‌اند
        /// </summary>
        /// <param name="branchId">شناسه شعبه - اگر 0 باشد همه کاربران برگردانده می‌شود</param>
        /// <returns>لیست کاربران فعال</returns>
        public List<UserViewModelFull> GetUserListBybranchId(int branchId)
        {
            List<UserViewModelFull> user;

            if (branchId == 0)
            {
                // اگر branchId برابر 0 باشد، همه کاربران فعال را برگردان (بایگانی شده‌ها نمایش داده نمی‌شوند)
                user = _Context.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser && !u.IsCompletelyDeleted)
                    .Select(u => new UserViewModelFull
                    {
                        Id = u.Id,
                        FullNamesString = u.FirstName + " " + u.LastName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        IsActive = u.IsActive,
                        UserName = u.UserName
                    })
                    .ToList();
            }
            else
            {
                // کاربرانی که به شعبه مشخص شده اختصاص نداده شده‌اند
                var assignedUserIds = _Context.BranchUser_Tbl
                    .Where(bu => bu.BranchId == branchId && bu.IsActive)
                    .Select(bu => bu.UserId)
                    .ToList();

                user = _Context.Users
                    .Where(u => u.IsActive && !u.IsRemoveUser && !u.IsCompletelyDeleted && !assignedUserIds.Contains(u.Id))
                    .Select(u => new UserViewModelFull
                    {
                        Id = u.Id,
                        FullNamesString = u.FirstName + " " + u.LastName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        IsActive = u.IsActive,
                        UserName = u.UserName
                    })
                    .ToList();
            }

            return user;
        }

        /// <summary>
        /// دریافت لیست کاربرانی که به شعبه مشخص اختصاص داده شده‌اند
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="includeInactive">شامل کاربران غیرفعال</param>
        /// <returns>لیست کاربران اختصاص داده شده به شعبه</returns>
        public List<UserViewModelFull> GetAssignedUsersByBranchId(int branchId, bool includeInactive = false)
        {
            var query = from bu in _Context.BranchUser_Tbl
                        join u in _Context.Users on bu.UserId equals u.Id
                        where bu.BranchId == branchId && u.IsActive && !u.IsRemoveUser && !u.IsCompletelyDeleted
                        select new { BranchUser = bu, User = u };

            if (!includeInactive)
            {
                query = query.Where(x => x.BranchUser.IsActive);
            }

            var result = query.Select(x => new UserViewModelFull
            {
                Id = x.User.Id,
                FullNamesString = x.User.FirstName + " " + x.User.LastName,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                Email = x.User.Email,
                PhoneNumber = x.User.PhoneNumber,
                IsActive = x.User.IsActive,
                UserName = x.User.UserName
            }).ToList();

            return result;
        }

        /// <summary>
        /// ای دی کاربر بده و اطلاعات کامل بگیر
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public UserViewModelFull GetUserInfoData(string UserId)
        {
            UserViewModelFull? User = (from us in _Context.Users
                                       where us.Id == UserId
                                       select new UserViewModelFull
                                       {
                                           Id = us.Id,
                                           CompanyName = us.CompanyName,
                                           Address = us.Address,
                                           City = us.City,
                                           MelliCode = us.MelliCode,
                                           FirstName = us.FirstName,
                                           Email = us.Email,
                                           IsActive = us.IsActive,
                                           FullNamesString = us.FirstName + " " + us.LastName,
                                           PhoneNumber = us.PhoneNumber,
                                           LastName = us.LastName,
                                           UserName = us.UserName,
                                           Gender = us.Gender,
                                           Province = us.Province,
                                           ProfileImagePath = us.ProfileImagePath,
                                           RegisterDate = us.RegisterDate

                                       }).FirstOrDefault();
            return User;
        }

        /// <summary>
        /// بایگانی کاربر - کاربر از تمام شعبه‌ها و تیم‌ها خارج می‌شود و بایگانی می‌شود
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns></returns>
        public async Task<bool> ArchiveUserAsync(string userId)
        {
            try
            {
                var user = await _Context.Users.FindAsync(userId);
                if (user == null) return false;

                // بایگانی کاربر
                user.IsRemoveUser = true;
                user.ArchivedDate = DateTime.Now;
                user.IsActive = false;

                // حذف کاربر از تمام شعبه‌ها
                var branchUsers = _Context.BranchUser_Tbl.Where(bu => bu.UserId == userId);
                foreach (var branchUser in branchUsers)
                {
                    branchUser.IsActive = false;
                }

                // حذف کاربر از تمام تیم‌ها
                var teamMembers = _Context.TeamMember_Tbl.Where(tm => tm.UserId == userId);
                foreach (var teamMember in teamMembers)
                {
                    teamMember.IsActive = false;
                }

                await _Context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// حذف کامل کاربر - کاربر از همه جا حذف می‌شود و اطلاعاتش در تسک‌ها ثبت می‌شود
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns></returns>
        public async Task<bool> CompletelyDeleteUserAsync(string userId)
        {
            try
            {
                var user = await _Context.Users.FindAsync(userId);
                if (user == null) return false;

                var userFullName = $"{user.FirstName} {user.LastName}";
                var deletedUserInfo = $"یوزرنیم: {user.UserName} - نام: {userFullName}";

                // به‌روزرسانی تسک‌هایی که این کاربر در آنها نقش داشته (Creator)
                var userTasks = await _Context.Tasks_Tbl
                    .Where(t => t.CreatorUserId == userId)
                    .ToListAsync();

                foreach (var task in userTasks)
                {
                    task.DeletedUserInfo = deletedUserInfo;
                    task.CreatorUserId = null; // حذف رابطه
                }

                // به‌روزرسانی TaskAssignments - کاربرانی که تسک به آنها اختصاص داده شده
                var assignedTaskAssignments = await _Context.TaskAssignment_Tbl
                    .Where(ta => ta.AssignedUserId == userId)
                    .ToListAsync();

                foreach (var assignment in assignedTaskAssignments)
                {
                    // ثبت اطلاعات کاربر حذف شده در یک فیلد جدید (باید به TaskAssignment اضافه شود)
                    // فعلاً assignment را حذف می‌کنیم
                    _Context.TaskAssignment_Tbl.Remove(assignment);
                }

                // به‌روزرسانی TaskAssignments - کاربرانی که تسک را اختصاص داده‌اند (Assigner)
                var assignerTaskAssignments = await _Context.TaskAssignment_Tbl
                    .Where(ta => ta.AssignerUserId == userId)
                    .ToListAsync();

                foreach (var assignment in assignerTaskAssignments)
                {
                    // ثبت اطلاعات کاربر حذف شده در یک فیلد جدید (باید به TaskAssignment اضافه شود)
                    // فعلاً assignment را حذف می‌کنیم
                    _Context.TaskAssignment_Tbl.Remove(assignment);
                }

                // حذف کامل از BranchUser_Tbl
                var branchUsers = _Context.BranchUser_Tbl.Where(bu => bu.UserId == userId);
                _Context.BranchUser_Tbl.RemoveRange(branchUsers);

                // حذف کامل از TeamMembers
                var teamMembers = _Context.TeamMember_Tbl.Where(tm => tm.UserId == userId);
                _Context.TeamMember_Tbl.RemoveRange(teamMembers);

                // علامت‌گذاری کاربر به عنوان حذف شده کامل
                user.IsCompletelyDeleted = true;
                user.CompletelyDeletedDate = DateTime.Now;
                user.IsRemoveUser = true;
                user.IsActive = false;

                await _Context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// بررسی وجود یوزرنیم (حتی در کاربران حذف شده کامل)
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <param name="excludeUserId">شناسه کاربر جهت استثنا (برای ویرایش)</param>
        /// <returns></returns>
        public async Task<bool> IsUsernameExistsAsync(string username, string excludeUserId = null)
        {
            var query = _Context.Users.Where(u => u.UserName == username);
            
            if (!string.IsNullOrEmpty(excludeUserId))
            {
                query = query.Where(u => u.Id != excludeUserId);
            }

            return await query.AnyAsync();
        }
    }
}
