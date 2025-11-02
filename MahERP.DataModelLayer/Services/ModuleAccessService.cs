using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.ViewModels.ModuleAccessViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// سرویس مدیریت دسترسی به ماژول‌های سیستم
    /// </summary>
    public class ModuleAccessService : IModuleAccessService
    {
        private readonly AppDbContext _context; // ⭐ استفاده مستقیم از DbContext
        private readonly IMemoryCache _cache;
        private const string CACHE_PREFIX_ACCESS = "ModuleAccess_";
        private const string CACHE_PREFIX_PREFERENCE = "ModulePreference_";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

        public ModuleAccessService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        #region ✅ بررسی دسترسی - Access Checking

        /// <summary>
        /// بررسی دسترسی کاربر به یک ماژول خاص
        /// Priority: Admin > User > Team > Branch > None
        /// </summary>
        public async Task<ModuleAccessResult> CheckUserModuleAccessAsync(string userId, ModuleType moduleType)
        {
            // ✅ بررسی کش
            var cacheKey = $"{CACHE_PREFIX_ACCESS}{userId}_{(byte)moduleType}";
            if (_cache.TryGetValue(cacheKey, out ModuleAccessResult cachedResult))
            {
                return cachedResult;
            }

            var result = new ModuleAccessResult
            {
                ModuleType = moduleType,
                HasAccess = false,
                AccessSource = "None",
                Message = "دسترسی وجود ندارد"
            };

            // ⭐⭐⭐ Priority 0: Admin Check (بالاترین اولویت)
            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.IsAdmin)
            {
                result.HasAccess = true;
                result.AccessSource = "Admin";
                result.Message = "دسترسی کامل به عنوان Admin";

                _cache.Set(cacheKey, result, CACHE_DURATION);
                return result;
            }

            // ⭐ Priority 1: User-Level (Direct Override)
            var userPermission = await _context.UserModulePermission_Tbl
                .FirstOrDefaultAsync(ump => ump.UserId == userId && ump.ModuleType == (byte)moduleType);

            if (userPermission != null)
            {
                result.HasAccess = userPermission.IsEnabled;
                result.AccessSource = "Direct";
                result.SourceId = userPermission.Id;
                result.Message = userPermission.IsEnabled
                    ? "دسترسی مستقیم فعال"
                    : "دسترسی مستقیم غیرفعال (Override)";

                _cache.Set(cacheKey, result, CACHE_DURATION);
                return result;
            }

            // ⭐ Priority 2: Team-Level
            var userTeamIds = await _context.TeamMember_Tbl
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.TeamId)
                .ToListAsync();

            if (userTeamIds.Any())
            {
                var teamPermissions = await _context.TeamModulePermission_Tbl
                    .Where(tmp => userTeamIds.Contains(tmp.TeamId) &&
                                  tmp.ModuleType == (byte)moduleType &&
                                  tmp.IsEnabled)
                    .ToListAsync();

                if (teamPermissions.Any())
                {
                    var firstTeam = teamPermissions.First();
                    result.HasAccess = true;
                    result.AccessSource = "Team";
                    result.SourceId = firstTeam.Id;
                    result.Message = $"دسترسی از طریق تیم";

                    _cache.Set(cacheKey, result, CACHE_DURATION);
                    return result;
                }
            }

            // ⭐ Priority 3: Branch-Level
            var userBranchIds = await _context.BranchUser_Tbl
                .Where(bu => bu.UserId == userId && bu.IsActive)
                .Select(bu => bu.BranchId)
                .ToListAsync();

            if (userBranchIds.Any())
            {
                var branchPermissions = await _context.BranchModulePermission_Tbl
                    .Where(bmp => userBranchIds.Contains(bmp.BranchId) &&
                                  bmp.ModuleType == (byte)moduleType &&
                                  bmp.IsEnabled)
                    .ToListAsync();

                if (branchPermissions.Any())
                {
                    var firstBranch = branchPermissions.First();
                    result.HasAccess = true;
                    result.AccessSource = "Branch";
                    result.SourceId = firstBranch.Id;
                    result.Message = $"دسترسی از طریق شعبه";

                    _cache.Set(cacheKey, result, CACHE_DURATION);
                    return result;
                }
            }

            // ❌ No Access
            _cache.Set(cacheKey, result, CACHE_DURATION);
            return result;
        }

        /// <summary>
        /// دریافت لیست ماژول‌های فعال برای کاربر
        /// </summary>
        public async Task<List<ModuleType>> GetUserEnabledModulesAsync(string userId)
        {
            var enabledModules = new List<ModuleType>();

            foreach (ModuleType module in Enum.GetValues(typeof(ModuleType)))
            {
                var accessResult = await CheckUserModuleAccessAsync(userId, module);
                if (accessResult.HasAccess)
                {
                    enabledModules.Add(module);
                }
            }

            return enabledModules;
        }

        /// <summary>
        /// دریافت ماژول پیش‌فرض برای ریدایرکت بعد از لاگین
        /// </summary>
        public async Task<ModuleType?> GetDefaultModuleForLoginAsync(string userId)
        {
            // ✅ Debug Log
            Console.WriteLine($"[DEBUG] GetDefaultModuleForLoginAsync called for userId: {userId}");

            // ✅ بررسی کش اول
            var cacheKey = $"{CACHE_PREFIX_PREFERENCE}{userId}_default";
            if (_cache.TryGetValue(cacheKey, out ModuleType? cachedModule))
            {
                Console.WriteLine($"[DEBUG] Cache hit: {cachedModule}");
                return cachedModule;
            }

            // ⭐ دریافت preference از دیتابیس (فقط یک بار)
            var preference = await _context.UserModulePreference_Tbl
                .FirstOrDefaultAsync(ump => ump.UserId == userId);

            // ✅ Debug Log
            if (preference != null)
            {
                Console.WriteLine($"[DEBUG] Preference found: LastUsedModule={preference.LastUsedModule}, DefaultModule={preference.DefaultModule}");
            }
            else
            {
                Console.WriteLine($"[DEBUG] No preference found for userId: {userId}");
            }

            // 1. بررسی تنظیمات کاربر (DefaultModule)
            if (preference?.DefaultModule != null)
            {
                var defaultModule = (ModuleType)preference.DefaultModule.Value;
                _cache.Set(cacheKey, defaultModule, CACHE_DURATION);
                return defaultModule;
            }

            // 2. استفاده از آخرین ماژول استفاده شده
            if (preference?.LastUsedModule != null)
            {
                var lastUsedModule = (ModuleType)preference.LastUsedModule;

                // بررسی دسترسی به آخرین ماژول
                var accessResult = await CheckUserModuleAccessAsync(userId, lastUsedModule);
                if (accessResult.HasAccess)
                {
                    _cache.Set(cacheKey, lastUsedModule, CACHE_DURATION);
                    return lastUsedModule;
                }
            }

            // 3. انتخاب اولین ماژول قابل دسترسی
            var enabledModules = await GetUserEnabledModulesAsync(userId);
            var firstModule = enabledModules.FirstOrDefault();

            if (firstModule != default(ModuleType))
            {
                _cache.Set(cacheKey, firstModule, CACHE_DURATION);
                return firstModule;
            }

            // ❌ هیچ ماژولی فعال نیست
            return null;
        }

        #endregion

        #region ✅ مدیریت دسترسی کاربران - User Access Management

        public async Task<bool> GrantModuleAccessToUserAsync(
            string userId,
            ModuleType moduleType,
            string grantedByUserId,
            string notes = null)
        {
            try
            {
                var existing = await _context.UserModulePermission_Tbl
                    .FirstOrDefaultAsync(ump => ump.UserId == userId && ump.ModuleType == (byte)moduleType);

                if (existing != null)
                {
                    existing.IsEnabled = true;
                    existing.GrantedDate = DateTime.Now;
                    existing.GrantedByUserId = grantedByUserId;
                    existing.Notes = notes;
                    _context.UserModulePermission_Tbl.Update(existing);
                }
                else
                {
                    var newPermission = new UserModulePermission
                    {
                        UserId = userId,
                        ModuleType = (byte)moduleType,
                        IsEnabled = true,
                        GrantedDate = DateTime.Now,
                        GrantedByUserId = grantedByUserId,
                        Notes = notes
                    };
                    await _context.UserModulePermission_Tbl.AddAsync(newPermission);
                }

                await _context.SaveChangesAsync();
                ClearUserAccessCache(userId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeModuleAccessFromUserAsync(string userId, ModuleType moduleType)
        {
            try
            {
                var permission = await _context.UserModulePermission_Tbl
                    .FirstOrDefaultAsync(ump => ump.UserId == userId && ump.ModuleType == (byte)moduleType);

                if (permission != null)
                {
                    _context.UserModulePermission_Tbl.Remove(permission);
                    await _context.SaveChangesAsync();
                    ClearUserAccessCache(userId);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserModuleAccessViewModel>> GetUserDirectAccessesAsync(string userId)
        {
            var permissions = await _context.UserModulePermission_Tbl
                .Include(ump => ump.User)
                .Include(ump => ump.GrantedByUser)
                .Where(ump => ump.UserId == userId)
                .ToListAsync();

            return permissions.Select(p => new UserModuleAccessViewModel
            {
                Id = p.Id,
                UserId = p.UserId,
                UserFullName = $"{p.User?.FirstName} {p.User?.LastName}",
                UserName = p.User?.UserName,
                ModuleType = p.ModuleType,
                ModuleName = ((ModuleType)p.ModuleType).GetDisplayName(),
                ModuleIcon = ((ModuleType)p.ModuleType).GetIcon(),
                ModuleColor = ((ModuleType)p.ModuleType).GetColor(),
                IsEnabled = p.IsEnabled,
                GrantedDate = p.GrantedDate,
                GrantedByUserId = p.GrantedByUserId,
                GrantedByUserName = p.GrantedByUser != null
                    ? $"{p.GrantedByUser.FirstName} {p.GrantedByUser.LastName}"
                    : "",
                Notes = p.Notes
            }).ToList();
        }

        #endregion

        #region ✅ مدیریت دسترسی تیم‌ها - Team Access Management

        public async Task<bool> GrantModuleAccessToTeamAsync(
            int teamId,
            ModuleType moduleType,
            string grantedByUserId,
            string notes = null)
        {
            try
            {
                var existing = await _context.TeamModulePermission_Tbl
                    .FirstOrDefaultAsync(tmp => tmp.TeamId == teamId && tmp.ModuleType == (byte)moduleType);

                if (existing != null)
                {
                    existing.IsEnabled = true;
                    existing.GrantedDate = DateTime.Now;
                    existing.GrantedByUserId = grantedByUserId;
                    existing.Notes = notes;
                    _context.TeamModulePermission_Tbl.Update(existing);
                }
                else
                {
                    var newPermission = new TeamModulePermission
                    {
                        TeamId = teamId,
                        ModuleType = (byte)moduleType,
                        IsEnabled = true,
                        GrantedDate = DateTime.Now,
                        GrantedByUserId = grantedByUserId,
                        Notes = notes
                    };
                    await _context.TeamModulePermission_Tbl.AddAsync(newPermission);
                }

                await _context.SaveChangesAsync();

                // پاکسازی کش تمام اعضای تیم
                var teamMembers = await _context.TeamMember_Tbl
                    .Where(tm => tm.TeamId == teamId && tm.IsActive)
                    .Select(tm => tm.UserId)
                    .ToListAsync();

                foreach (var memberId in teamMembers)
                {
                    ClearUserAccessCache(memberId);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeModuleAccessFromTeamAsync(int teamId, ModuleType moduleType)
        {
            try
            {
                var permission = await _context.TeamModulePermission_Tbl
                    .FirstOrDefaultAsync(tmp => tmp.TeamId == teamId && tmp.ModuleType == (byte)moduleType);

                if (permission != null)
                {
                    _context.TeamModulePermission_Tbl.Remove(permission);
                    await _context.SaveChangesAsync();

                    var teamMembers = await _context.TeamMember_Tbl
                        .Where(tm => tm.TeamId == teamId)
                        .Select(tm => tm.UserId)
                        .ToListAsync();

                    foreach (var memberId in teamMembers)
                    {
                        ClearUserAccessCache(memberId);
                    }

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<TeamModuleAccessViewModel>> GetTeamAccessesAsync(int teamId)
        {
            var result = await _context.TeamModulePermission_Tbl
                .Include(tmp => tmp.Team)
                .Include(tmp => tmp.GrantedByUser)
                .Where(tmp => tmp.TeamId == teamId)
                .Select(p => new
                {
                    Permission = p,
                    MembersCount = _context.TeamMember_Tbl
                        .Count(tm => tm.TeamId == p.TeamId && tm.IsActive)
                })
                .ToListAsync();

            return result.Select(x => new TeamModuleAccessViewModel
            {
                Id = x.Permission.Id,
                TeamId = x.Permission.TeamId,
                TeamTitle = x.Permission.Team?.Title,
                ModuleType = x.Permission.ModuleType,
                ModuleName = ((ModuleType)x.Permission.ModuleType).GetDisplayName(),
                ModuleIcon = ((ModuleType)x.Permission.ModuleType).GetIcon(),
                ModuleColor = ((ModuleType)x.Permission.ModuleType).GetColor(),
                IsEnabled = x.Permission.IsEnabled,
                GrantedDate = x.Permission.GrantedDate,
                GrantedByUserId = x.Permission.GrantedByUserId,
                GrantedByUserName = x.Permission.GrantedByUser != null
                    ? $"{x.Permission.GrantedByUser.FirstName} {x.Permission.GrantedByUser.LastName}"
                    : "",
                Notes = x.Permission.Notes,
                MembersCount = x.MembersCount
            }).ToList();
        }
        #endregion

        #region ✅ مدیریت دسترسی شعب - Branch Access Management

        public async Task<bool> GrantModuleAccessToBranchAsync(
            int branchId,
            ModuleType moduleType,
            string grantedByUserId,
            string notes = null)
        {
            try
            {
                var existing = await _context.BranchModulePermission_Tbl
                    .FirstOrDefaultAsync(bmp => bmp.BranchId == branchId && bmp.ModuleType == (byte)moduleType);

                if (existing != null)
                {
                    existing.IsEnabled = true;
                    existing.GrantedDate = DateTime.Now;
                    existing.GrantedByUserId = grantedByUserId;
                    existing.Notes = notes;
                    _context.BranchModulePermission_Tbl.Update(existing);
                }
                else
                {
                    var newPermission = new BranchModulePermission
                    {
                        BranchId = branchId,
                        ModuleType = (byte)moduleType,
                        IsEnabled = true,
                        GrantedDate = DateTime.Now,
                        GrantedByUserId = grantedByUserId,
                        Notes = notes
                    };
                    await _context.BranchModulePermission_Tbl.AddAsync(newPermission);
                }

                await _context.SaveChangesAsync();

                var branchUsers = await _context.BranchUser_Tbl
                    .Where(bu => bu.BranchId == branchId && bu.IsActive)
                    .Select(bu => bu.UserId)
                    .ToListAsync();

                foreach (var userId in branchUsers)
                {
                    ClearUserAccessCache(userId);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeModuleAccessFromBranchAsync(int branchId, ModuleType moduleType)
        {
            try
            {
                var permission = await _context.BranchModulePermission_Tbl
                    .FirstOrDefaultAsync(bmp => bmp.BranchId == branchId && bmp.ModuleType == (byte)moduleType);

                if (permission != null)
                {
                    _context.BranchModulePermission_Tbl.Remove(permission);
                    await _context.SaveChangesAsync();

                    var branchUsers = await _context.BranchUser_Tbl
                        .Where(bu => bu.BranchId == branchId)
                        .Select(bu => bu.UserId)
                        .ToListAsync();

                    foreach (var userId in branchUsers)
                    {
                        ClearUserAccessCache(userId);
                    }

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<BranchModuleAccessViewModel>> GetBranchAccessesAsync(int branchId)
        {
            var result = await _context.BranchModulePermission_Tbl
                .Include(bmp => bmp.Branch)
                .Include(bmp => bmp.GrantedByUser)
                .Where(bmp => bmp.BranchId == branchId)
                .Select(p => new
                {
                    Permission = p,
                    UsersCount = _context.BranchUser_Tbl
                        .Count(bu => bu.BranchId == p.BranchId && bu.IsActive)
                })
                .ToListAsync();

            return result.Select(x => new BranchModuleAccessViewModel
            {
                Id = x.Permission.Id,
                BranchId = x.Permission.BranchId,
                BranchName = x.Permission.Branch?.Name,
                ModuleType = x.Permission.ModuleType,
                ModuleName = ((ModuleType)x.Permission.ModuleType).GetDisplayName(),
                ModuleIcon = ((ModuleType)x.Permission.ModuleType).GetIcon(),
                ModuleColor = ((ModuleType)x.Permission.ModuleType).GetColor(),
                IsEnabled = x.Permission.IsEnabled,
                GrantedDate = x.Permission.GrantedDate,
                GrantedByUserId = x.Permission.GrantedByUserId,
                GrantedByUserName = x.Permission.GrantedByUser != null
                    ? $"{x.Permission.GrantedByUser.FirstName} {x.Permission.GrantedByUser.LastName}"
                    : "",
                Notes = x.Permission.Notes,
                UsersCount = x.UsersCount
            }).ToList();
        }

        #endregion

        #region ✅ مدیریت تنظیمات کاربر - User Preferences

        public async Task SaveLastUsedModuleAsync(string userId, ModuleType moduleType)
        {
            try
            {
                var preference = await _context.UserModulePreference_Tbl
                    .FirstOrDefaultAsync(ump => ump.UserId == userId);

                if (preference != null)
                {
                    preference.LastUsedModule = (byte)moduleType;
                    preference.LastAccessDate = DateTime.Now;
                    _context.UserModulePreference_Tbl.Update(preference);
                }
                else
                {
                    var newPreference = new UserModulePreference
                    {
                        UserId = userId,
                        LastUsedModule = (byte)moduleType,
                        LastAccessDate = DateTime.Now
                    };
                    await _context.UserModulePreference_Tbl.AddAsync(newPreference);
                }

                await _context.SaveChangesAsync();
                ClearUserPreferenceCache(userId);

                Console.WriteLine($"[DEBUG] SaveLastUsedModule: userId={userId}, module={moduleType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SaveLastUsedModule failed: {ex.Message}");
            }
        }

        public async Task SetUserDefaultModuleAsync(string userId, ModuleType moduleType)
        {
            try
            {
                var preference = await _context.UserModulePreference_Tbl
                    .FirstOrDefaultAsync(ump => ump.UserId == userId);

                if (preference != null)
                {
                    preference.DefaultModule = (byte)moduleType;
                    _context.UserModulePreference_Tbl.Update(preference);
                }
                else
                {
                    var newPreference = new UserModulePreference
                    {
                        UserId = userId,
                        LastUsedModule = (byte)moduleType,
                        DefaultModule = (byte)moduleType,
                        LastAccessDate = DateTime.Now
                    };
                    await _context.UserModulePreference_Tbl.AddAsync(newPreference);
                }

                await _context.SaveChangesAsync();
                ClearUserPreferenceCache(userId);
            }
            catch
            {
                // Silent fail
            }
        }

        public async Task<UserModulePreferenceViewModel> GetUserModulePreferenceAsync(string userId)
        {
            var preference = await _context.UserModulePreference_Tbl
                .FirstOrDefaultAsync(ump => ump.UserId == userId);

            if (preference == null)
                return null;

            return new UserModulePreferenceViewModel
            {
                Id = preference.Id,
                UserId = preference.UserId,
                LastUsedModule = preference.LastUsedModule,
                LastUsedModuleName = ((ModuleType)preference.LastUsedModule).GetDisplayName(),
                LastAccessDate = preference.LastAccessDate,
                DefaultModule = preference.DefaultModule,
                DefaultModuleName = preference.DefaultModule.HasValue
                    ? ((ModuleType)preference.DefaultModule.Value).GetDisplayName()
                    : null
            };
        }

        #endregion

        #region ✅ گزارشات - Reports

        public async Task<ModuleAccessReportViewModel> GetModuleAccessReportAsync(ModuleType moduleType)
        {
            var report = new ModuleAccessReportViewModel
            {
                ModuleType = (byte)moduleType,
                ModuleName = moduleType.GetDisplayName(),
                ModuleIcon = moduleType.GetIcon(),
                ModuleColor = moduleType.GetColor()
            };

            var directUsers = await _context.UserModulePermission_Tbl
                .Include(ump => ump.User)
                .Where(ump => ump.ModuleType == (byte)moduleType && ump.IsEnabled)
                .Select(ump => $"{ump.User.FirstName} {ump.User.LastName}")
                .ToListAsync();

            report.DirectUsers = directUsers;

            var teams = await _context.TeamModulePermission_Tbl
                .Include(tmp => tmp.Team)
                .Where(tmp => tmp.ModuleType == (byte)moduleType && tmp.IsEnabled)
                .Select(tmp => tmp.Team.Title)
                .ToListAsync();

            report.Teams = teams;

            var branches = await _context.BranchModulePermission_Tbl
                .Include(bmp => bmp.Branch)
                .Where(bmp => bmp.ModuleType == (byte)moduleType && bmp.IsEnabled)
                .Select(bmp => bmp.Branch.Name)
                .ToListAsync();

            report.Branches = branches;

            return report;
        }

        public async Task<AllModulesReportViewModel> GetAllModulesAccessReportAsync()
        {
            var allReports = new AllModulesReportViewModel();

            foreach (ModuleType module in Enum.GetValues(typeof(ModuleType)))
            {
                var report = await GetModuleAccessReportAsync(module);
                allReports.Reports.Add(report);
            }

            return allReports;
        }

        public async Task<List<UserModuleAccessViewModel>> GetUsersWithModuleAccessAsync(ModuleType moduleType)
        {
            var permissions = await _context.UserModulePermission_Tbl
                .Include(ump => ump.User)
                .Include(ump => ump.GrantedByUser)
                .Where(ump => ump.ModuleType == (byte)moduleType && ump.IsEnabled)
                .ToListAsync();

            return permissions.Select(p => new UserModuleAccessViewModel
            {
                Id = p.Id,
                UserId = p.UserId,
                UserFullName = $"{p.User?.FirstName} {p.User?.LastName}",
                UserName = p.User?.UserName,
                ModuleType = p.ModuleType,
                ModuleName = ((ModuleType)p.ModuleType).GetDisplayName(),
                ModuleIcon = ((ModuleType)p.ModuleType).GetIcon(),
                ModuleColor = ((ModuleType)p.ModuleType).GetColor(),
                IsEnabled = p.IsEnabled,
                GrantedDate = p.GrantedDate,
                GrantedByUserId = p.GrantedByUserId,
                GrantedByUserName = p.GrantedByUser != null
                    ? $"{p.GrantedByUser.FirstName} {p.GrantedByUser.LastName}"
                    : "",
                Notes = p.Notes
            }).ToList();
        }

        #endregion

        #region ✅ Cache Management

        public void ClearUserAccessCache(string userId)
        {
            foreach (ModuleType module in Enum.GetValues(typeof(ModuleType)))
            {
                var cacheKey = $"{CACHE_PREFIX_ACCESS}{userId}_{(byte)module}";
                _cache.Remove(cacheKey);
            }

            _cache.Remove($"{CACHE_PREFIX_PREFERENCE}{userId}_default");
        }

        public void ClearUserPreferenceCache(string userId)
        {
            _cache.Remove($"{CACHE_PREFIX_PREFERENCE}{userId}_default");
        }

        #endregion
    }
}