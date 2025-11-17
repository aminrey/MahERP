using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BackgroundJobEntity = MahERP.DataModelLayer.Entities.BackgroundJobs.BackgroundJob;

namespace MahERP.DataModelLayer.Repository
{
    public interface IBackgroundJobRepository
    {
        Task<int> CreateJobAsync(BackgroundJobEntity job);
        Task UpdateJobAsync(BackgroundJobEntity job);
        Task UpdateProgressAsync(int jobId, int progress, int processed, int success, int failed);
        Task CompleteJobAsync(int jobId, bool isSuccess, string? errorMessage = null);
        Task<List<BackgroundJobEntity>> GetUserActiveJobsAsync(string userId);
        Task<List<BackgroundJobEntity>> GetUserJobsAsync(string userId, int take = 10);
        Task<BackgroundJobEntity?> GetJobByIdAsync(int jobId);
        Task DeleteOldJobsAsync(int daysOld = 7);
    }

    public class BackgroundJobRepository : IBackgroundJobRepository
    {
        private readonly AppDbContext _context;

        public BackgroundJobRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateJobAsync(BackgroundJobEntity job)
        {
            _context.BackgroundJob_Tbl.Add(job);
            await _context.SaveChangesAsync();
            return job.Id;
        }

        public async Task UpdateJobAsync(BackgroundJobEntity job)
        {
            _context.BackgroundJob_Tbl.Update(job);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProgressAsync(int jobId, int progress, int processed, int success, int failed)
        {
            var job = await _context.BackgroundJob_Tbl.FindAsync(jobId);
            if (job != null)
            {
                job.Progress = progress;
                job.ProcessedItems = processed;
                job.SuccessCount = success;
                job.FailedCount = failed;
                job.Status = 1; // Running
                await _context.SaveChangesAsync();
            }
        }

        public async Task CompleteJobAsync(int jobId, bool isSuccess, string? errorMessage = null)
        {
            var job = await _context.BackgroundJob_Tbl.FindAsync(jobId);
            if (job != null)
            {
                job.Status = isSuccess ? (byte)2 : (byte)3; // Completed : Failed
                job.Progress = 100;
                job.CompletedDate = DateTime.Now;
                job.ErrorMessage = errorMessage;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<BackgroundJobEntity>> GetUserActiveJobsAsync(string userId)
        {
            return await _context.BackgroundJob_Tbl
                .Where(j => j.CreatedByUserId == userId && 
                           (j.Status == 0 || j.Status == 1)) // Pending or Running
                .OrderByDescending(j => j.StartDate)
                .ToListAsync();
        }

        public async Task<List<BackgroundJobEntity>> GetUserJobsAsync(string userId, int take = 10)
        {
            return await _context.BackgroundJob_Tbl
                .Where(j => j.CreatedByUserId == userId)
                .OrderByDescending(j => j.StartDate)
                .Take(take)
                .ToListAsync();
        }

        public async Task<BackgroundJobEntity?> GetJobByIdAsync(int jobId)
        {
            return await _context.BackgroundJob_Tbl
                .Include(j => j.CreatedBy)
                .FirstOrDefaultAsync(j => j.Id == jobId);
        }

        public async Task DeleteOldJobsAsync(int daysOld = 7)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysOld);
            var oldJobs = await _context.BackgroundJob_Tbl
                .Where(j => j.StartDate < cutoffDate && 
                           (j.Status == 2 || j.Status == 3)) // Completed or Failed
                .ToListAsync();

            _context.BackgroundJob_Tbl.RemoveRange(oldJobs);
            await _context.SaveChangesAsync();
        }
    }
}
