using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// Repository برای مدیریت تنظیمات نمایش تسک در شعبه
    /// </summary>
    public interface IBranchTaskVisibilitySettingsRepository
    {
        Task<BranchTaskVisibilitySettingsViewModel> GetSettingsViewModelAsync(int branchId, string? managerId = null);
        Task<BranchTaskVisibilitySettings?> GetSettingsAsync(int branchId, string? managerId = null);
        Task<bool> SaveSettingsAsync(BranchTaskVisibilitySettingsViewModel model, string currentUserId);
        Task<bool> DeleteSettingsAsync(int settingsId, string currentUserId);
        Task<List<BranchTaskVisibilitySettings>> GetAllBranchSettingsAsync(int branchId);
    }

    /// <summary>
    /// پیاده‌سازی Repository
    /// </summary>
    public class BranchTaskVisibilitySettingsRepository : IBranchTaskVisibilitySettingsRepository
    {
        private readonly AppDbContext _context;

        public BranchTaskVisibilitySettingsRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// دریافت ViewModel برای نمایش/ویرایش
        /// </summary>
        public async Task<BranchTaskVisibilitySettingsViewModel> GetSettingsViewModelAsync(int branchId, string? managerId = null)
        {
            var branch = await _context.Branch_Tbl.FindAsync(branchId);
            if (branch == null)
                throw new Exception("شعبه یافت نشد");

            var settings = await GetSettingsAsync(branchId, managerId);

            var model = new BranchTaskVisibilitySettingsViewModel
            {
                Id = settings?.Id,
                BranchId = branchId,
                BranchName = branch.Name,
                ManagerUserId = managerId,
                ShowAllSubTeamsByDefault = settings?.ShowAllSubTeamsByDefault ?? false,
                MaxTasksToShow = settings?.MaxTasksToShow ?? 0,
                IsActive = settings?.IsActive ?? true
            };

            // دریافت تیم‌های شعبه
            model.AvailableTeams = await GetBranchTeamsAsync(branchId);

            // تیم‌های انتخابی
            if (settings != null && !string.IsNullOrEmpty(settings.DefaultVisibleTeamIds))
            {
                model.SelectedTeamIds = settings.GetVisibleTeamIds();
                
                foreach (var team in model.AvailableTeams)
                {
                    team.IsSelected = model.SelectedTeamIds.Contains(team.Id);
                }
            }

            // دریافت مدیران
            model.AvailableManagers = await GetBranchManagersAsync(branchId);

            // نام مدیر
            if (!string.IsNullOrEmpty(managerId))
            {
                var manager = await _context.Users.FindAsync(managerId);
                model.ManagerFullName = manager != null ? $"{manager.FirstName} {manager.LastName}" : null;
            }

            return model;
        }

        /// <summary>
        /// دریافت تنظیمات
        /// </summary>
        public async Task<BranchTaskVisibilitySettings?> GetSettingsAsync(int branchId, string? managerId = null)
        {
            var query = _context.BranchTaskVisibilitySettings_Tbl
                .Where(s => s.BranchId == branchId && s.IsActive);

            if (string.IsNullOrEmpty(managerId))
            {
                // تنظیمات پیش‌فرض
                return await query.FirstOrDefaultAsync(s => s.ManagerUserId == null);
            }
            else
            {
                // تنظیمات شخصی
                return await query.FirstOrDefaultAsync(s => s.ManagerUserId == managerId);
            }
        }

        /// <summary>
        /// ذخیره تنظیمات
        /// </summary>
        public async Task<bool> SaveSettingsAsync(BranchTaskVisibilitySettingsViewModel model, string currentUserId)
        {
            try
            {
                BranchTaskVisibilitySettings settings;

                if (model.Id.HasValue)
                {
                    // ویرایش
                    settings = await _context.BranchTaskVisibilitySettings_Tbl.FindAsync(model.Id.Value);
                    if (settings == null)
                        return false;

                    settings.LastUpdaterUserId = currentUserId;
                    settings.LastUpdateDate = DateTime.Now;
                }
                else
                {
                    // ایجاد جدید
                    settings = new BranchTaskVisibilitySettings
                    {
                        BranchId = model.BranchId,
                        ManagerUserId = model.ManagerUserId,
                        CreatorUserId = currentUserId,
                        CreateDate = DateTime.Now
                    };
                    _context.BranchTaskVisibilitySettings_Tbl.Add(settings);
                }

                // بروزرسانی فیلدها
                settings.ShowAllSubTeamsByDefault = model.ShowAllSubTeamsByDefault;
                settings.MaxTasksToShow = model.MaxTasksToShow;
                settings.IsActive = model.IsActive;

                // تنظیم تیم‌های قابل نمایش
                if (model.SelectedTeamIds != null && model.SelectedTeamIds.Any())
                {
                    settings.SetVisibleTeamIds(model.SelectedTeamIds);
                }
                else
                {
                    settings.DefaultVisibleTeamIds = null;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SaveSettingsAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// حذف تنظیمات
        /// </summary>
        public async Task<bool> DeleteSettingsAsync(int settingsId, string currentUserId)
        {
            try
            {
                var settings = await _context.BranchTaskVisibilitySettings_Tbl.FindAsync(settingsId);
                if (settings == null)
                    return false;

                settings.IsActive = false;
                settings.LastUpdaterUserId = currentUserId;
                settings.LastUpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// دریافت همه تنظیمات شعبه
        /// </summary>
        public async Task<List<BranchTaskVisibilitySettings>> GetAllBranchSettingsAsync(int branchId)
        {
            return await _context.BranchTaskVisibilitySettings_Tbl
                .Include(s => s.Manager)
                .Where(s => s.BranchId == branchId && s.IsActive)
                .OrderBy(s => s.ManagerUserId == null ? 0 : 1) // پیش‌فرض اول
                .ToListAsync();
        }

        #region Private Helpers

        /// <summary>
        /// دریافت تیم‌های شعبه به صورت سلسله‌مراتبی
        /// </summary>
        private async Task<List<TeamItemViewModel>> GetBranchTeamsAsync(int branchId)
        {
            var teams = await _context.Team_Tbl
                .Where(t => t.BranchId == branchId && t.IsActive)
                .Include(t => t.Manager)
                .OrderBy(t => t.ParentTeamId ?? 0)
                .ThenBy(t => t.DisplayOrder)
                .ToListAsync();

            var result = new List<TeamItemViewModel>();

            // ساخت سلسله‌مراتب
            await BuildTeamHierarchy(teams, null, 0, result);

            return result;
        }

        /// <summary>
        /// ساخت بازگشتی سلسله‌مراتب تیم‌ها
        /// </summary>
        private async Task BuildTeamHierarchy(
            List<Team> allTeams,
            int? parentId,
            int level,
            List<TeamItemViewModel> result)
        {
            var children = allTeams.Where(t => t.ParentTeamId == parentId).ToList();

            foreach (var team in children)
            {
                result.Add(new TeamItemViewModel
                {
                    Id = team.Id,
                    Title = new string('─', level * 2) + (level > 0 ? " " : "") + team.Title,
                    ManagerName = team.Manager != null
                        ? $"{team.Manager.FirstName} {team.Manager.LastName}"
                        : null,
                    Level = level,
                    ParentTeamId = team.ParentTeamId
                });

                // بازگشتی برای زیرتیم‌ها
                await BuildTeamHierarchy(allTeams, team.Id, level + 1, result);
            }
        }

        /// <summary>
        /// دریافت مدیران شعبه
        /// </summary>
        private async Task<List<ManagerItemViewModel>> GetBranchManagersAsync(int branchId)
        {
            var managers = await _context.Team_Tbl
                .Where(t => t.BranchId == branchId && t.IsActive && t.ManagerUserId != null)
                .Include(t => t.Manager)
                .GroupBy(t => t.ManagerUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Manager = g.First().Manager,
                    Teams = g.Select(t => t.Title).ToList()
                })
                .ToListAsync();

            return managers.Select(m => new ManagerItemViewModel
            {
                UserId = m.UserId,
                FullName = $"{m.Manager.FirstName} {m.Manager.LastName}",
                Email = m.Manager.Email,
                ManagedTeams = m.Teams
            }).ToList();
        }

        #endregion
    }
}
