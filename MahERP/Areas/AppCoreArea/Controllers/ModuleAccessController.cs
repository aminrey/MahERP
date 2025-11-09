using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.DataModelLayer.ViewModels.ModuleAccessViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers
{
    [Area("AppCoreArea")]
    [Authorize]
    [PermissionRequired("CORE.MODULE.ACCESS")]
    public class ModuleAccessController : BaseController
    {
        private readonly IModuleAccessService _moduleAccessService;
        private readonly ITeamRepository _teamRepository;
        private readonly IBranchRepository _branchRepository;

        public ModuleAccessController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            ITeamRepository teamRepository,
            IBranchRepository branchRepository, ModuleTrackingBackgroundService moduleTracking,
            IModuleAccessService moduleAccessService)


 : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _moduleAccessService = moduleAccessService;
            _teamRepository = teamRepository;
            _branchRepository = branchRepository;
        }

        #region ✅ صفحه اصلی - Index

        /// <summary>
        /// صفحه اصلی مدیریت دسترسی‌های ماژول
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(byte? moduleFilter = null, string targetTypeFilter = null)
        {
            try
            {
                var viewModel = new ModuleAccessIndexViewModel
                {
                    ModuleFilter = moduleFilter,
                    TargetTypeFilter = targetTypeFilter
                };

                // دریافت تمام دسترسی‌های کاربران
                var userAccesses = _uow.UserModulePermissionUW
                    .Get(null, null, "User,GrantedByUser")
                    .Where(ump => !moduleFilter.HasValue || ump.ModuleType == moduleFilter.Value)
                    .Select(ump => new ModuleAccessListItemViewModel
                    {
                        Id = ump.Id,
                        TargetType = "User",
                        TargetName = $"{ump.User.FirstName} {ump.User.LastName}",
                        TargetIdentifier = ump.UserId,
                        ModuleType = ump.ModuleType,
                        ModuleName = ((ModuleType)ump.ModuleType).GetDisplayName(),
                        ModuleIcon = ((ModuleType)ump.ModuleType).GetIcon(),
                        IsEnabled = ump.IsEnabled,
                        GrantedDate = ump.GrantedDate,
                        GrantedByUserName = ump.GrantedByUser != null 
                            ? $"{ump.GrantedByUser.FirstName} {ump.GrantedByUser.LastName}" 
                            : ""
                    })
                    .ToList();

                // دریافت تمام دسترسی‌های تیم‌ها
                var teamAccesses = _uow.TeamModulePermissionUW
                    .Get(null, null, "Team,GrantedByUser")
                    .Where(tmp => !moduleFilter.HasValue || tmp.ModuleType == moduleFilter.Value)
                    .Select(tmp => new ModuleAccessListItemViewModel
                    {
                        Id = tmp.Id,
                        TargetType = "Team",
                        TargetName = tmp.Team.Title,
                        TargetIdentifier = tmp.TeamId.ToString(),
                        ModuleType = tmp.ModuleType,
                        ModuleName = ((ModuleType)tmp.ModuleType).GetDisplayName(),
                        ModuleIcon = ((ModuleType)tmp.ModuleType).GetIcon(),
                        IsEnabled = tmp.IsEnabled,
                        GrantedDate = tmp.GrantedDate,
                        GrantedByUserName = tmp.GrantedByUser != null 
                            ? $"{tmp.GrantedByUser.FirstName} {tmp.GrantedByUser.LastName}" 
                            : ""
                    })
                    .ToList();

                // دریافت تمام دسترسی‌های شعب
                var branchAccesses = _uow.BranchModulePermissionUW
                    .Get(null, null, "Branch,GrantedByUser")
                    .Where(bmp => !moduleFilter.HasValue || bmp.ModuleType == moduleFilter.Value)
                    .Select(bmp => new ModuleAccessListItemViewModel
                    {
                        Id = bmp.Id,
                        TargetType = "Branch",
                        TargetName = bmp.Branch.Name,
                        TargetIdentifier = bmp.BranchId.ToString(),
                        ModuleType = bmp.ModuleType,
                        ModuleName = ((ModuleType)bmp.ModuleType).GetDisplayName(),
                        ModuleIcon = ((ModuleType)bmp.ModuleType).GetIcon(),
                        IsEnabled = bmp.IsEnabled,
                        GrantedDate = bmp.GrantedDate,
                        GrantedByUserName = bmp.GrantedByUser != null 
                            ? $"{bmp.GrantedByUser.FirstName} {bmp.GrantedByUser.LastName}" 
                            : ""
                    })
                    .ToList();

                // ترکیب همه
                var allAccesses = new List<ModuleAccessListItemViewModel>();
                allAccesses.AddRange(userAccesses);
                allAccesses.AddRange(teamAccesses);
                allAccesses.AddRange(branchAccesses);

                // فیلتر بر اساس نوع هدف
                if (!string.IsNullOrEmpty(targetTypeFilter))
                {
                    allAccesses = allAccesses.Where(a => a.TargetType == targetTypeFilter).ToList();
                }

                viewModel.AccessList = allAccesses.OrderByDescending(a => a.GrantedDate).ToList();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "ModuleAccess",
                    "Index",
                    "مشاهده لیست دسترسی‌های ماژول"
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "ModuleAccess",
                    "Index",
                    "خطا در دریافت لیست دسترسی‌ها",
                    ex
                );
                return RedirectToAction("ErrorView", "Home");
            }
        }

        #endregion

        #region ✅ افزودن دسترسی - Grant Access

        /// <summary>
        /// نمایش فرم افزودن دسترسی
        /// </summary>
        [HttpGet]
        public IActionResult GrantAccess()
        {
            PrepareGrantAccessViewBag();
            return View(new GrantModuleAccessViewModel());
        }

        /// <summary>
        /// پردازش افزودن دسترسی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantAccess(GrantModuleAccessViewModel model, List<byte> SelectedModules)
        {
            if (!ModelState.IsValid)
            {
                PrepareGrantAccessViewBag();
                return View(model);
            }

            // ⭐ بررسی انتخاب حداقل یک ماژول
            if (SelectedModules == null || !SelectedModules.Any())
            {
                ModelState.AddModelError("", "حداقل یک ماژول را انتخاب کنید");
                PrepareGrantAccessViewBag();
                return View(model);
            }

            try
            {
                var currentUserId = _userManager.GetUserId(User);
                int successCount = 0;
                int failCount = 0;
                var failedModules = new List<string>();

                // ⭐⭐⭐ حلقه روی تمام ماژول‌های انتخاب شده
                foreach (var moduleTypeByte in SelectedModules)
                {
                    var moduleType = (ModuleType)moduleTypeByte;
                    bool success = false;

                    switch (model.TargetType)
                    {
                        case "User":
                            success = await _moduleAccessService.GrantModuleAccessToUserAsync(
                                model.TargetId, moduleType, currentUserId, model.Notes);
                            break;

                        case "Team":
                            success = await _moduleAccessService.GrantModuleAccessToTeamAsync(
                                int.Parse(model.TargetId), moduleType, currentUserId, model.Notes);
                            break;

                        case "Branch":
                            success = await _moduleAccessService.GrantModuleAccessToBranchAsync(
                                int.Parse(model.TargetId), moduleType, currentUserId, model.Notes);
                            break;
                    }

                    if (success)
                    {
                        successCount++;
                        
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "ModuleAccess",
                            "GrantAccess",
                            $"اعطای دسترسی {moduleType.GetDisplayName()} به {model.TargetType}: {model.TargetId}",
                            entityType: "ModuleAccess"
                        );
                    }
                    else
                    {
                        failCount++;
                        failedModules.Add(moduleType.GetDisplayName());
                    }
                }

                // ⭐ نمایش پیام نتیجه
                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"دسترسی به {successCount} ماژول با موفقیت اعطا شد";
                }

                if (failCount > 0)
                {
                    TempData["WarningMessage"] = $"خطا در اعطای دسترسی به {failCount} ماژول: {string.Join(", ", failedModules)}";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "ModuleAccess",
                    "GrantAccess",
                    "خطا در اعطای دسترسی",
                    ex
                );
                ModelState.AddModelError("", $"خطا در اعطای دسترسی: {ex.Message}");
            }

            PrepareGrantAccessViewBag();
            return View(model);
        }

        #endregion

        #region ✅ حذف دسترسی - Revoke Access

        /// <summary>
        /// نمایش مودال تأیید حذف دسترسی
        /// </summary>
        [HttpGet]
        public IActionResult RevokeAccess(string targetType, string targetId, byte moduleType)
        {
            var viewModel = new RevokeModuleAccessViewModel
            {
                TargetType = targetType,
                TargetId = targetId,
                ModuleType = moduleType,
                ModuleName = ((ModuleType)moduleType).GetDisplayName()
            };

            // دریافت نام هدف
            switch (targetType)
            {
                case "User":
                    var user = _uow.UserManagerUW.GetById(targetId);
                    viewModel.TargetName = user != null ? $"{user.FirstName} {user.LastName}" : "";
                    break;

                case "Team":
                    var team = _uow.TeamUW.GetById(int.Parse(targetId));
                    viewModel.TargetName = team?.Title ?? "";
                    break;

                case "Branch":
                    var branch = _uow.BranchUW.GetById(int.Parse(targetId));
                    viewModel.TargetName = branch?.Name ?? "";
                    break;
            }

            return PartialView("_RevokeAccessModal", viewModel);
        }

        /// <summary>
        /// پردازش حذف دسترسی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeAccessConfirmed(string targetType, string targetId, byte moduleType)
        {
            try
            {
                var module = (ModuleType)moduleType;
                bool success = false;

                switch (targetType)
                {
                    case "User":
                        success = await _moduleAccessService.RevokeModuleAccessFromUserAsync(targetId, module);
                        break;

                    case "Team":
                        success = await _moduleAccessService.RevokeModuleAccessFromTeamAsync(int.Parse(targetId), module);
                        break;

                    case "Branch":
                        success = await _moduleAccessService.RevokeModuleAccessFromBranchAsync(int.Parse(targetId), module);
                        break;
                }

                if (success)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "ModuleAccess",
                        "RevokeAccess",
                        $"لغو دسترسی {module.GetDisplayName()} از {targetType}: {targetId}",
                        entityType: "ModuleAccess"
                    );

                    return Json(new
                    {
                        status = "success",
                        message = "دسترسی با موفقیت لغو شد"
                    });
                }

                return Json(new
                {
                    status = "error",
                    message = "خطا در لغو دسترسی"
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "ModuleAccess",
                    "RevokeAccessConfirmed",
                    "خطا در لغو دسترسی",
                    ex
                );

                return Json(new
                {
                    status = "error",
                    message = $"خطا: {ex.Message}"
                });
            }
        }

        #endregion

        #region ✅ گزارش دسترسی‌ها - Report

        /// <summary>
        /// نمایش گزارش دسترسی‌های یک ماژول
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ModuleReport(byte moduleType)
        {
            try
            {
                var module = (ModuleType)moduleType;
                var report = await _moduleAccessService.GetModuleAccessReportAsync(module);

                return PartialView("_ModuleReportModal", report);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "ModuleAccess",
                    "ModuleReport",
                    "خطا در دریافت گزارش ماژول",
                    ex
                );

                return Json(new
                {
                    status = "error",
                    message = "خطا در دریافت گزارش"
                });
            }
        }

        /// <summary>
        /// نمایش گزارش کامل تمام ماژول‌ها
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AllModulesReport()
        {
            try
            {
                var report = await _moduleAccessService.GetAllModulesAccessReportAsync();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "ModuleAccess",
                    "AllModulesReport",
                    "مشاهده گزارش کامل دسترسی‌های ماژول‌ها"
                );

                return View(report);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "ModuleAccess",
                    "AllModulesReport",
                    "خطا در دریافت گزارش کامل",
                    ex
                );
                return RedirectToAction("ErrorView", "Home");
            }
        }

        #endregion

        #region ✅ AJAX Endpoints

        /// <summary>
        /// دریافت لیست کاربران برای Select2
        /// </summary>
        [HttpGet]
        public IActionResult GetUsers(string search)
        {
            var users = _uow.UserManagerUW
                .Get(u => u.IsActive && !u.IsRemoveUser &&
                         (string.IsNullOrEmpty(search) || 
                          u.FirstName.Contains(search) || 
                          u.LastName.Contains(search) ||
                          u.UserName.Contains(search)))
                .Take(20)
                .Select(u => new
                {
                    id = u.Id,
                    text = $"{u.FirstName} {u.LastName} ({u.UserName})"
                })
                .ToList();

            return Json(users);
        }

        /// <summary>
        /// دریافت لیست تیم‌ها برای Select2
        /// </summary>
        [HttpGet]
        public IActionResult GetTeams(string search, int? branchId = null)
        {
            var query = _uow.TeamUW.Get(t => t.IsActive);

            if (branchId.HasValue)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.Contains(search));
            }

            var teams = query
                .Take(20)
                .Select(t => new
                {
                    id = t.Id,
                    text = t.Title
                })
                .ToList();

            return Json(teams);
        }

        /// <summary>
        /// دریافت لیست شعب برای Select2
        /// </summary>
        [HttpGet]
        public IActionResult GetBranches(string search)
        {
            var branches = _uow.BranchUW
                .Get(b => b.IsActive &&
                         (string.IsNullOrEmpty(search) || b.Name.Contains(search)))
                .Take(20)
                .Select(b => new
                {
                    id = b.Id,
                    text = b.Name
                })
                .ToList();

            return Json(branches);
        }

        #endregion

        #region ✅ Helper Methods

        /// <summary>
        /// آماده‌سازی ViewBag برای فرم افزودن دسترسی
        /// </summary>
        private void PrepareGrantAccessViewBag()
        {
            // لیست انواع ماژول
            ViewBag.Modules = new SelectList(
                Enum.GetValues(typeof(ModuleType))
                    .Cast<ModuleType>()
                    .Select(m => new
                    {
                        Value = (byte)m,
                        Text = m.GetDisplayName(),
                        Icon = m.GetIcon()
                    }),
                "Value",
                "Text"
            );

            // لیست انواع هدف
            ViewBag.TargetTypes = new SelectList(
                new[]
                {
                    new { Value = "User", Text = "کاربر" },
                    new { Value = "Team", Text = "تیم" },
                    new { Value = "Branch", Text = "شعبه" }
                },
                "Value",
                "Text"
            );
        }

        #endregion
    }

    #region ✅ ViewModels for Controller

    /// <summary>
    /// ViewModel صفحه اصلی
    /// </summary>
    public class ModuleAccessIndexViewModel
    {
        public List<ModuleAccessListItemViewModel> AccessList { get; set; } = new();
        public byte? ModuleFilter { get; set; }
        public string TargetTypeFilter { get; set; }
    }

    /// <summary>
    /// ViewModel فرم افزودن دسترسی
    /// </summary>
    public class GrantModuleAccessViewModel
    {
        [Required(ErrorMessage = "نوع هدف الزامی است")]
        public string TargetType { get; set; } // User, Team, Branch

        [Required(ErrorMessage = "انتخاب هدف الزامی است")]
        public string TargetId { get; set; }

        [Required(ErrorMessage = "انتخاب ماژول الزامی است")]
        public byte ModuleType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ViewModel حذف دسترسی
    /// </summary>
    public class RevokeModuleAccessViewModel
    {
        public string TargetType { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public byte ModuleType { get; set; }
        public string ModuleName { get; set; }
    }

    #endregion
}