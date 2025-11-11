using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.TaskManagement;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.TaskRepository
{
    public class TaskCarbonCopyRepository : ITaskCarbonCopyRepository
    {
        private readonly AppDbContext _context;

        public TaskCarbonCopyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskCarbonCopy> AddCarbonCopyAsync(int taskId, string userId, string addedByUserId, string note = null)
        {
            // بررسی تکراری نبودن
            var exists = await _context.TaskCarbonCopy_Tbl
                .AnyAsync(cc => cc.TaskId == taskId && cc.UserId == userId && cc.IsActive);

            if (exists)
            {
                throw new InvalidOperationException("این کاربر قبلاً به عنوان ناظر اضافه شده است");
            }

            var carbonCopy = new TaskCarbonCopy
            {
                TaskId = taskId,
                UserId = userId,
                AddedByUserId = addedByUserId,
                AddedDate = DateTime.Now,
                Note = note,
                IsActive = true
            };

            _context.TaskCarbonCopy_Tbl.Add(carbonCopy);
            await _context.SaveChangesAsync();

            return carbonCopy;
        }

        public async Task<bool> RemoveCarbonCopyAsync(int carbonCopyId, string requestingUserId)
        {
            var carbonCopy = await _context.TaskCarbonCopy_Tbl
                .Include(cc => cc.Task)
                .FirstOrDefaultAsync(cc => cc.Id == carbonCopyId);

            if (carbonCopy == null)
                return false;

            // فقط کسی که اضافه کرده یا ادمین/مدیر تسک می‌تواند حذف کند
            if (carbonCopy.AddedByUserId != requestingUserId)
            {
                // بررسی مجوز اضافی (ادمین، مدیر، سازنده تسک)
                var task = carbonCopy.Task;
                var isCreator = task.CreatorUserId == requestingUserId;
                var isAdmin = false; // باید از UserManager بررسی شود

                if (!isCreator && !isAdmin)
                    return false;
            }

            carbonCopy.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// ⭐ نام صحیح برای مطابقت با Interface
        /// </summary>
        public async Task<List<TaskCarbonCopy>> GetTaskCarbonCopiesAsync(int taskId)
        {
            return await _context.TaskCarbonCopy_Tbl
                .Where(cc => cc.TaskId == taskId && cc.IsActive)
                .Include(cc => cc.User)
                .Include(cc => cc.AddedByUser)
                .OrderBy(cc => cc.AddedDate)
                .ToListAsync();
        }

        public async Task<bool> IsUserCarbonCopyAsync(string userId, int taskId)
        {
            return await _context.TaskCarbonCopy_Tbl
                .AnyAsync(cc => cc.UserId == userId && cc.TaskId == taskId && cc.IsActive);
        }

        public async Task<List<int>> GetUserCarbonCopyTaskIdsAsync(string userId)
        {
            return await _context.TaskCarbonCopy_Tbl
                .Where(cc => cc.UserId == userId && cc.IsActive)
                .Select(cc => cc.TaskId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> CanRemoveCarbonCopyAsync(int carbonCopyId, string userId)
        {
            var carbonCopy = await _context.TaskCarbonCopy_Tbl
                .Include(cc => cc.Task)
                .FirstOrDefaultAsync(cc => cc.Id == carbonCopyId);

            if (carbonCopy == null || !carbonCopy.IsActive)
                return false;

            // کسی که اضافه کرده می‌تواند حذف کند
            if (carbonCopy.AddedByUserId == userId)
                return true;

            // سازنده تسک می‌تواند حذف کند
            if (carbonCopy.Task?.CreatorUserId == userId)
                return true;

            return false;
        }
    }
}
