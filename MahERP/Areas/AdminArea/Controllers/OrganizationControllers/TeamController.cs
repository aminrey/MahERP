using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Organization;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using MahERP.Extentions;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;

namespace MahERP.Areas.AdminArea.Controllers.OrganizationControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class TeamController : BaseController
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IUserManagerRepository _userRepository;
        private readonly IMapper _mapper;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly ITaskVisibilityRepository _taskVisibilityRepository;

        public TeamController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            ITeamRepository teamRepository,
            IBranchRepository branchRepository,
            IUserManagerRepository userRepository,
            IMapper mapper,
            ITaskVisibilityRepository taskVisibilityRepository) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger)
        {
            _teamRepository = teamRepository;
            _branchRepository = branchRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
            _taskVisibilityRepository = taskVisibilityRepository;
        }

        #region Chart Views


        /// <summary>
        /// نمایش چارت سازمانی
        /// </summary>
        public IActionResult OrganizationalChart(int branchId = 0)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);

            if (branchId == 0)
                branchId = userBranches.FirstOrDefault()?.Id ?? 0;

            if (branchId == 0 || !userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("ErrorView", "Home");

            var chart = _teamRepository.GetOrganizationalChart(branchId);
            ViewBag.UserBranches = new SelectList(userBranches, "Id", "Name", branchId);

            return View(chart);
        }

        /// <summary>
        /// نمایش چارت قدرت مشاهده تسک‌ها
        /// </summary>
        public async Task<IActionResult> TaskVisibilityChart(int branchId = 0)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);

            if (branchId == 0)
                branchId = userBranches.FirstOrDefault()?.Id ?? 0;

            if (branchId == 0 || !userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("ErrorView", "Home");

            // استفاده از repository به جای service
            var chart = await _taskVisibilityRepository.GenerateVisibilityChartAsync(branchId);

            if (chart == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.UserBranches = new SelectList(userBranches, "Id", "Name", branchId);
            ViewBag.AvailableUsers = _teamRepository.GetAvailableUsersForBranch(branchId);
            ViewBag.AvailableTeams = _teamRepository.GetTeamsByBranchId(branchId);

            await _activityLogger.LogActivityAsync(
                ActivityTypeEnum.View,
                "Team",
                "TaskVisibilityChart",
                $"مشاهده چارت قدرت مشاهده تسک‌ها برای شعبه: {chart.BranchName}",
                recordId: branchId.ToString()
            );

            return View(chart);
        }

        #endregion

        #region Team CRUD

        /// <summary>
        /// نمایش فرم ایجاد تیم جدید
        /// </summary>
        public IActionResult Create(int branchId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);

            if (!userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("OrganizationalChart");

            var model = new TeamViewModel
            {
                BranchId = branchId,
                IsActive = true,
                DisplayOrder = 1
            };

            PrepareTeamViewBags(branchId);
            return View(model);
        }

        /// <summary>
        /// ایجاد تیم جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TeamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PrepareTeamViewBags(model.BranchId);
                return View(model);
            }

            try
            {
                var team = _mapper.Map<Team>(model);
                var currentUserId = _userManager.GetUserId(User);

                team.CreatorUserId = currentUserId;
                team.CreateDate = DateTime.Now;
                team.LastUpdaterUserId = currentUserId;
                team.LastUpdateDate = DateTime.Now;

                var teamId = _teamRepository.CreateTeam(team);

                // اگر مدیر انتخاب شده است، آن را تنظیم کن
                if (!string.IsNullOrEmpty(model.ManagerUserId))
                {
                    var managerAssignmentSuccess = _teamRepository.AssignManager(teamId, model.ManagerUserId, currentUserId);
                    if (!managerAssignmentSuccess)
                    {
                        // اگر انتصاب مدیر ناموفق بود، تیم را حذف کن
                        _teamRepository.DeleteTeam(teamId);
                        ModelState.AddModelError("", "خطا در انتصاب مدیر به تیم. لطفاً دوباره تلاش کنید.");
                        PrepareTeamViewBags(model.BranchId);
                        return View(model);
                    }
                }

                TempData["SuccessMessage"] = !string.IsNullOrEmpty(model.ManagerUserId) 
                    ? "تیم با موفقیت ایجاد شد و مدیر تیم تنظیم گردید."
                    : "تیم با موفقیت ایجاد شد.";
                
                return RedirectToAction("Details", new { id = teamId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطا در ایجاد تیم: " + ex.Message);
                PrepareTeamViewBags(model.BranchId);
                return View(model);
            }
        }

        /// <summary>
        /// نمایش جزئیات تیم
        /// </summary>
        public IActionResult Details(int id)
        {
            // دریافت تیم با اعضا و سمت‌ها در یک بار اتصال
            var team = _teamRepository.GetTeamById(id, includePositions: true, includeMembers: true);
            if (team == null)
                return NotFound();

            // بررسی دسترسی کاربر به این شعبه
            var currentUserId = _userManager.GetUserId(User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            if (!userBranches.Any(b => b.Id == team.BranchId))
                return RedirectToAction("ErrorView", "Home");



            return View(team);
        }

        /// <summary>
        /// نمایش فرم ویرایش تیم
        /// </summary>
        public IActionResult Edit(int id)
        {
            var team = _teamRepository.GetTeamById(id);
            if (team == null)
                return NotFound();

            // بررسی دسترسی
            var currentUserId = _userManager.GetUserId(User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            if (!userBranches.Any(b => b.Id == team.BranchId))
                return RedirectToAction("ErrorView", "Home");

            PrepareTeamViewBags(team.BranchId);
            return View(team);
        }

        /// <summary>
        /// ویرایش تیم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TeamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PrepareTeamViewBags(model.BranchId);
                return View(model);
            }

            try
            {
                var team = _mapper.Map<Team>(model);
                team.LastUpdaterUserId = _userManager.GetUserId(User);

                var success = _teamRepository.UpdateTeam(team);
                if (success)
                {
                    TempData["SuccessMessage"] = "تیم با موفقیت ویرایش شد.";
                    return RedirectToAction("Details", new { id = model.Id });
                }
                else
                {
                    ModelState.AddModelError("", "خطا در ویرایش تیم.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطا در ویرایش تیم: " + ex.Message);
            }

            PrepareTeamViewBags(model.BranchId);
            return View(model);
        }

        /// <summary>
        /// حذف تیم
        /// </summary>
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                if (!_teamRepository.CanDeleteTeam(id))
                {
                    return Json(new { success = false, message = "امکان حذف این تیم وجود ندارد. ابتدا تیم‌های فرزند و اعضا را حذف کنید." });
                }

                var success = _teamRepository.DeleteTeam(id);
                if (success)
                {
                    return Json(new { success = true, message = "تیم با موفقیت حذف شد." });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در حذف تیم." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در حذف تیم: " + ex.Message });
            }
        }

        /// <summary>
        /// تغییر وضعیت active/inactive تیم
        /// </summary>
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            try
            {
                var success = _teamRepository.ToggleTeamStatus(id);
                if (success)
                {
                    return Json(new { success = true, message = "وضعیت تیم با موفقیت تغییر یافت." });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در تغییر وضعیت تیم." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا: " + ex.Message });
            }
        }

        #endregion

        #region Manager Assignment
        /// <summary>
        /// انتخاب مدیر تیم
        /// </summary>
        [HttpPost]
        public IActionResult AssignManager(int teamId, string managerUserId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var success = _teamRepository.AssignManager(teamId, managerUserId, currentUserId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = new[] {
                            new { status = "success", text = "مدیر تیم با موفقیت انتخاب شد." }
                        }
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "خطا در انتخاب مدیر تیم." }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = new[] {
                        new { status = "error", text = "خطا: " + ex.Message }
                    }
                });
            }
        }
        /// <summary>
        /// حذف مدیر تیم
        /// </summary>
        [HttpPost]
        public IActionResult RemoveManager(int teamId)
        {
            try
            {
                var success = _teamRepository.RemoveManager(teamId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = new[] {
                            new { status = "success", text = "مدیر تیم با موفقیت حذف شد." }
                        }
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "خطا در حذف مدیر تیم." }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = new[] {
                        new { status = "error", text = "خطا: " + ex.Message }
                    }
                });
            }
        }

        #endregion

        #region Team Members

        /// <summary>
        /// نمایش فرم اضافه کردن عضو به تیم
        /// </summary>
        public IActionResult AddMember(int teamId)
        {
            var team = _teamRepository.GetTeamById(teamId);
            if (team == null)
                return NotFound();

            var model = new TeamMemberViewModel
            {
                TeamId = teamId,
                TeamTitle = team.Title,
                IsActive = true,
                MembershipType = 0
            };

            PrepareTeamMemberViewBags(team.BranchId, teamId);
            return PartialView("_AddMemberModal", model);
        }

        /// <summary>
        /// اضافه کردن عضو به تیم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMember(TeamMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "داده‌های ورودی نامعتبر است." });
            }

            try
            {
                var member = _mapper.Map<TeamMember>(model);
                member.AddedByUserId = _userManager.GetUserId(User);

                var memberId = _teamRepository.AddTeamMember(member);

                return Json(new { success = true, message = "عضو با موفقیت به تیم اضافه شد." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در اضافه کردن عضو: " + ex.Message });
            }
        }

        /// <summary>
        /// حذف عضو از تیم
        /// </summary>
        [HttpPost]
        public IActionResult RemoveMember(int memberId)
        {
            try
            {
                var success = _teamRepository.RemoveTeamMember(memberId);

                if (success)
                {
                    return Json(new { success = true, message = "عضو با موفقیت از تیم حذف شد." });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در حذف عضو از تیم." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا: " + ex.Message });
            }
        }

        #endregion

        #region Helper Methods

        private void PrepareTeamViewBags(int branchId)
        {
            var data = _teamRepository.PrepareTeamViewBags(branchId);
            
            ViewBag.ParentTeams = data.ParentTeams;
            ViewBag.AvailableUsers = data.AvailableUsers;
            ViewBag.AccessLevels = data.AccessLevels;
        }

        private void PrepareTeamMemberViewBags(int branchId, int teamId)
        {
            var data = _teamRepository.PrepareTeamMemberViewBags(branchId, teamId);
            
            ViewBag.AvailableUsers = data.AvailableUsers;
            ViewBag.AvailablePositions = data.AvailablePositions;
            ViewBag.MembershipTypes = data.MembershipTypes;
        }

        /// <summary>
        /// آماده‌سازی ViewBag برای ویرایش عضو
        /// </summary>
        private void PrepareEditMemberViewBags(int branchId, int teamId)
        {
            var data = _teamRepository.PrepareTeamMemberViewBags(branchId, teamId);
            
            ViewBag.AvailableUsers = data.AvailableUsers;
            ViewBag.AvailablePositions = data.AvailablePositions;
            
            // فقط یک گزینه "بدون سمت" اضافه می‌کنیم
            ViewBag.AvailablePositions.Insert(0, new SelectListItem { Value = "", Text = "-- بدون سمت --" });
            ViewBag.MembershipTypes = data.MembershipTypes;
        }

        #endregion


        /// <summary>
        /// دریافت کاربران موجود برای انتخاب مدیر (AJAX)
        /// </summary>
        [HttpGet]
        public IActionResult GetAvailableUsers(int branchId)
        {
            try
            {
                var users = _teamRepository.GetAvailableUsersForBranch(branchId);
                return Json(users);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        /// <summary>
        /// نمایش مودال اضافه کردن عضو به تیم
        /// </summary>
        public IActionResult AddMemberModal(int teamId)
        {
            try
            {
                var team = _teamRepository.GetTeamById(teamId);
                if (team == null)
                    return NotFound();

                var model = new TeamMemberViewModel
                {
                    TeamId = teamId,
                    TeamTitle = team.Title,
                    IsActive = true,
                    MembershipType = 0
                };

                PrepareTeamMemberViewBags(team.BranchId, teamId);
                return PartialView("_AddMemberModal", model);
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }

        /// <summary>
        /// اضافه کردن عضو به تیم با پاسخ JSON
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMemberSubmit(TeamMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => new { status = "error", text = e.ErrorMessage })
                    .ToArray();

                return Json(new
                {
                    success = false,
                    message = errors
                });
            }

            var currentUserId = _userManager.GetUserId(User);
            var result = _teamRepository.CreateTeamMemberWithValidation(model, currentUserId);

            if (result.IsSuccess)
            {
                return Json(new
                {
                    success = true,
                    message = new[] {
                        new { status = "success", text = result.Message }
                    },
                    status = "redirect",
                    redirectUrl = Url.Action("Details", new { id = model.TeamId })
                });
            }
            else
            {
                var errorMessages = new List<object>
                {
                    new { status = "error", text = result.Message }
                };

                foreach (var error in result.Errors)
                {
                    errorMessages.Add(new { status = "error", text = error });
                }

                return Json(new
                {
                    success = false,
                    message = errorMessages.ToArray()
                });
            }
        }

        /// <summary>
        /// نمایش مودال انتخاب مدیر تیم
        /// </summary>
        public IActionResult AssignManagerModal(int teamId)
        {
            try
            {
                var team = _teamRepository.GetTeamById(teamId);
                if (team == null)
                    return NotFound();

                var model = new AssignManagerViewModel
                {
                    TeamId = teamId,
                    TeamTitle = team.Title,
                    ManagerUserId = team.ManagerUserId,
                    CurrentManagerName = team.ManagerFullName
                };

                var availableUsers = _teamRepository.GetAvailableUsersForBranch(team.BranchId);
                ViewBag.AvailableUsers = availableUsers;

                return PartialView("_AssignManagerModal", model);
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }

        /// <summary>
        /// ثبت انتخاب مدیر تیم با پاسخ JSON
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignManagerSubmit(AssignManagerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = new[] {
                new { status = "error", text = "داده‌های ورودی نامعتبر است." }
            }
                });
            }

            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // بررسی وجود تیم قبل از انجام عملیات
                var existingTeam = _teamRepository.GetTeamById(model.TeamId);
                if (existingTeam == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                    new { status = "error", text = "تیم مورد نظر یافت نشد." }
                }
                    });
                }

                var success = _teamRepository.AssignManager(model.TeamId, model.ManagerUserId, currentUserId);

                if (success)
                {
                    // بروزرسانی اطلاعات تیم بدون استفاده از AutoMapper
                    var updatedTeam = _teamRepository.GetTeamById(model.TeamId);

                    return Json(new
                    {
                        success = true,
                        message = new[] {
                    new { status = "success", text = "مدیر تیم با موفقیت انتخاب شد و سمت مدیریت ایجاد گردید." }
                },
                        status = "redirect",
                        redirectUrl = Url.Action("OrganizationalChart", new { branchId = updatedTeam.BranchId })
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                    new { status = "error", text = "خطا در انتخاب مدیر تیم. ممکن است کاربر انتخاب شده قبلاً مدیر تیم دیگری باشد." }
                }
                    });
                }
            }
            catch (Exception ex)
            {
                // بهبود نمایش خطا برای دیباگ
                var errorMessage = ex.InnerException?.Message ?? ex.Message;

                return Json(new
                {
                    success = false,
                    message = new[] {
                new { status = "error", text = $"خطا در انتخاب مدیر: {errorMessage}" }
            }
                });
            }
        }
        /// <summary>
        /// نمایش مودال تایید حذف تیم
        /// </summary>
        [HttpGet]
        public IActionResult DeleteConfirmModal(int id) // تغییر از teamId به id
        {
            try
            {
                var team = _teamRepository.GetTeamById(id); // تغییر از teamId به id
                if (team == null)
                    return NotFound();

                var canDelete = _teamRepository.CanDeleteTeam(id); // تغییر از teamId به id

                ViewBag.CanDelete = canDelete;
                ViewBag.DeleteMessage = canDelete
                    ? $"آیا از حذف تیم '{team.Title}' اطمینان دارید؟"
                    : "امکان حذف این تیم وجود ندارد. ابتدا تیم‌های فرزند و اعضا را حذف کنید.";

                return PartialView("_DeleteConfirmModal", team);
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }
        /// <summary>
        /// حذف تیم با پاسخ JSON
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSubmit(int id, int? branchId = null)
        {
            try
            {
                if (!_teamRepository.CanDeleteTeam(id))
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "امکان حذف این تیم وجود ندارد. ابتدا تیم‌های فرزند و اعضا را حذف کنید." }
                        }
                    });
                }

                // اگر branchId ارسال نشده، از اطلاعات تیم استفاده کن
                if (!branchId.HasValue)
                {
                    var teamInfo = _teamRepository.GetTeamById(id);
                    if (teamInfo == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = new[] {
                                new { status = "error", text = "تیم مورد نظر یافت نشد." }
                            }
                        });
                    }
                    branchId = teamInfo.BranchId;
                }

                var success = _teamRepository.DeleteTeam(id);
                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = new[] {
                            new { status = "success", text = "تیم با موفقیت حذف شد." }
                        },
                        status = "redirect",
                        redirectUrl = Url.Action("OrganizationalChart", new { branchId = branchId.Value })
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "خطا در حذف تیم." }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = new[] {
                        new { status = "error", text = "خطا در حذف تیم: " + ex.Message }
                    }
                });
            }
        }
        /// <summary>
        /// نمایش مودال تایید حذف مدیر تیم
        /// </summary>
        public IActionResult RemoveManagerModal(int teamId)
        {
            try
            {
                var team = _teamRepository.GetTeamById(teamId);
                if (team == null)
                    return NotFound();

                if (!team.HasManager)
                {
                    return BadRequest("این تیم مدیری ندارد.");
                }

                return PartialView("_RemoveManagerModal", team);
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }
        /// <summary>
        /// حذف مدیر تیم با پاسخ JSON
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveManagerSubmit(int teamId, int? branchId = null)
        {
            try
            {
                // اگر branchId ارسال نشده، از اطلاعات تیم استفاده کن
                if (!branchId.HasValue)
                {
                    var teamInfo = _teamRepository.GetTeamById(teamId);
                    if (teamInfo == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = new[] {
                                new { status = "error", text = "تیم مورد نظر یافت نشد." }
                            }
                        });
                    }
                    branchId = teamInfo.BranchId;
                }

                var success = _teamRepository.RemoveManager(teamId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = new[] {
                            new { status = "success", text = "مدیر تیم با موفقیت حذف شد." }
                        },
                        status = "redirect",
                        redirectUrl = Url.Action("OrganizationalChart", new { branchId = branchId.Value })
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                            new { status = "error", text = "خطا در حذف مدیر تیم. ممکن است این تیم مدیری نداشته باشد یا عملیات قبلاً انجام شده باشد." }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = new[] {
                        new { status = "error", text = "خطا: " + ex.Message }
                    }
                });
            }
        }

        #region Team Positions

        /// <summary>
        /// نمایش مودال ایجاد سمت جدید برای تیم
        /// </summary>
        public async Task<IActionResult> CreatePositionModal(int teamId)
        {
            try
            {
                var team = _teamRepository.GetTeamById(teamId);
                if (team == null)
                    return NotFound();

                // بررسی دسترسی کاربر
                var currentUserId = _userManager.GetUserId(User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == team.BranchId))
                    return Forbid();

                var model = new CreateTeamPositionViewModel
                {
                    TeamId = teamId,
                    TeamTitle = team.Title,
                    IsActive = true,
                    CanViewSubordinateTasks = true,
                    CanViewPeerTasks = false
                };

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Team",
                    "CreatePositionModal",
                    $"نمایش فرم ایجاد سمت برای تیم: {team.Title}",
                    recordId: teamId.ToString()
                );

                return PartialView("_CreatePositionModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "CreatePositionModal",
                    "خطا در نمایش فرم ایجاد سمت",
                    ex,
                    recordId: teamId.ToString()
                );
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }
        /// <summary>
        /// ایجاد سمت جدید برای تیم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePositionSubmit(CreateTeamPositionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = "داده‌های ورودی نامعتبر است.",
                        errors = errors
                    });
                }

                // بررسی دسترسی
                var team = _teamRepository.GetTeamById(model.TeamId);
                if (team == null)
                {
                    return Json(new { success = false, message = "تیم مورد نظر یافت نشد." });
                }

                var currentUserId = _userManager.GetUserId(User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == team.BranchId))
                {
                    return Json(new { success = false, message = "شما به این تیم دسترسی ندارید." });
                }

                // بررسی تکراری نبودن عنوان
                if (!_teamRepository.IsPositionTitleUnique(model.TeamId, model.Title))
                {
                    return Json(new { success = false, message = "سمت با این عنوان قبلاً در این تیم تعریف شده است." });
                }

                // بررسی تکراری نبودن سمت پیش‌فرض
                if (model.IsDefault && !_teamRepository.IsDefaultPositionUnique(model.TeamId))
                {
                    return Json(new { success = false, message = "در این تیم قبلاً یک سمت پیش‌فرض تعریف شده است. لطفاً ابتدا آن را غیرفعال کنید." });
                }

                // ایجاد سمت جدید
                var position = new TeamPosition
                {
                    TeamId = model.TeamId,
                    Title = model.Title,
                    Description = model.Description,
                    PowerLevel = model.PowerLevel,
                    CanViewSubordinateTasks = model.CanViewSubordinateTasks,
                    CanViewPeerTasks = model.CanViewPeerTasks,
                    MaxMembers = model.MaxMembers,
                    IsDefault = model.IsDefault,
                    IsActive = model.IsActive,
                    DisplayOrder = model.PowerLevel,
                    CreatorUserId = currentUserId
                };

                var positionId = _teamRepository.CreateTeamPosition(position);

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Team",
                    "CreatePositionSubmit",
                    $"ایجاد سمت جدید: {position.Title} با سطح قدرت {position.PowerLevel} برای تیم: {team.Title}",
                    recordId: positionId.ToString(),
                    entityType: "TeamPosition",
                    recordTitle: position.Title
                );

                return Json(new
                {
                    success = true,
                    message = "سمت با موفقیت ایجاد شد.",
                    status = "redirect",
                    redirectUrl = Url.Action("Details", new { id = model.TeamId })
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "CreatePositionSubmit",
                    "خطا در ایجاد سمت جدید",
                    ex,
                    recordId: model.TeamId.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = "خطا در ایجاد سمت: " + ex.Message
                });
            }
        }

        /// <summary>
        /// نمایش مودال ویرایش سمت
        /// </summary>
        public async Task<IActionResult> EditPositionModal(int id)
        {
            try
            {
                var position = _teamRepository.GetTeamPositionById(id);
                if (position == null)
                    return NotFound();

                // بررسی دسترسی
                var currentUserId = _userManager.GetUserId(User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == position.Team.BranchId))
                    return Forbid();

                var model = new CreateTeamPositionViewModel
                {
                    TeamId = position.TeamId,
                    TeamTitle = position.Team.Title,
                    Title = position.Title,
                    Description = position.Description,
                    PowerLevel = position.PowerLevel,
                    CanViewSubordinateTasks = position.CanViewSubordinateTasks,
                    CanViewPeerTasks = position.CanViewPeerTasks,
                    MaxMembers = position.MaxMembers,
                    IsDefault = position.IsDefault,
                    IsActive = position.IsActive
                };

                ViewBag.PositionId = id;

                return PartialView("_EditPositionModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "EditPositionModal",
                    "خطا در نمایش فرم ویرایش سمت",
                    ex,
                    recordId: id.ToString()
                );
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }
        /// <summary>
        /// ویرایش سمت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPositionSubmit(int id, CreateTeamPositionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = "داده‌های ورودی نامعتبر است.",
                        errors = errors
                    });
                }

                var position = _teamRepository.GetTeamPositionById(id);
                if (position == null)
                {
                    return Json(new { success = false, message = "سمت مورد نظر یافت نشد." });
                }

                // بررسی تکراری نبودن عنوان
                if (!_teamRepository.IsPositionTitleUnique(model.TeamId, model.Title, id))
                {
                    return Json(new { success = false, message = "سمت با این عنوان قبلاً در این تیم تعریف شده است." });
                }

                // بررسی تکراری نبودن سمت پیش‌فرض
                if (model.IsDefault && !_teamRepository.IsDefaultPositionUnique(model.TeamId, id))
                {
                    return Json(new { success = false, message = "در این تیم قبلاً یک سمت پیش‌فرض تعریف شده است. لطفاً ابتدا آن را غیرفعال کنید." });
                }

                // بروزرسانی
                position.Title = model.Title;
                position.Description = model.Description;
                position.PowerLevel = model.PowerLevel;
                position.CanViewSubordinateTasks = model.CanViewSubordinateTasks;
                position.CanViewPeerTasks = model.CanViewPeerTasks;
                position.MaxMembers = model.MaxMembers;
                position.IsDefault = model.IsDefault;
                position.IsActive = model.IsActive;
                position.LastUpdaterUserId = _userManager.GetUserId(User);

                var success = _teamRepository.UpdateTeamPosition(position);

                if (success)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "Team",
                        "EditPositionSubmit",
                        $"ویرایش سمت: {position.Title} با سطح قدرت {position.PowerLevel}",
                        recordId: position.Id.ToString(),
                        entityType: "TeamPosition",
                        recordTitle: position.Title
                    );

                    return Json(new
                    {
                        success = true,
                        message = "سمت با موفقیت ویرایش شد.",
                        status = "redirect",
                        redirectUrl = Url.Action("Details", new { id = model.TeamId })
                    });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در ویرایش سمت." });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "EditPositionSubmit",
                    "خطا در ویرایش سمت",
                    ex,
                    recordId: id.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = "خطا در ویرایش سمت: " + ex.Message
                });
            }
        }
        /// <summary>
        /// حذف سمت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePosition(int id)
        {
            try
            {
                var position = _teamRepository.GetTeamPositionById(id);
                if (position == null)
                {
                    return Json(new { success = false, message = "سمت مورد نظر یافت نشد." });
                }

                // بررسی امکان حذف
                if (!_teamRepository.CanDeletePosition(id))
                {
                    return Json(new { success = false, message = "امکان حذف این سمت وجود ندارد. ابتدا اعضا را از این سمت خارج کنید." });
                }

                var success = _teamRepository.DeleteTeamPosition(id);

                if (success)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Delete,
                        "Team",
                        "DeletePosition",
                        $"حذف سمت: {position.Title}",
                        recordId: position.Id.ToString(),
                        entityType: "TeamPosition",
                        recordTitle: position.Title
                    );

                    return Json(new
                    {
                        success = true,
                        message = "سمت با موفقیت حذف شد.",
                        status = "redirect",
                        redirectUrl = Url.Action("Details", new { id = position.TeamId })
                    });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در حذف سمت." });
                }
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "DeletePosition",
                    "خطا در حذف سمت",
                    ex,
                    recordId: id.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = "خطا در حذف سمت: " + ex.Message
                });
            }
        }
        /// <summary>
        /// دریافت درخت سمت‌های تیم برای نمایش در partial view
        /// </summary>
        public async Task<IActionResult> GetTeamPositionsHierarchy(int teamId)
        {
            try
            {
                var hierarchyData = _teamRepository.GetTeamPositionHierarchy(teamId);

                // ارسال داده‌ها به ViewBag برای استفاده در partial view
                ViewBag.TeamHierarchy = hierarchyData;
                ViewBag.TeamId = teamId;

                return PartialView("_TeamPositionsHierarchy", hierarchyData);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "GetTeamPositionsHierarchy",
                    "خطا در دریافت درخت سمت‌ها",
                    ex,
                    recordId: teamId.ToString()
                );
                return PartialView("_TeamPositionsHierarchy", new TeamHierarchyViewModel { TeamId = teamId });
            }
        }

        #endregion


        /// <summary>
        /// نمایش مودال مدیریت تبصره‌های مشاهده تسک‌های تیم
        /// </summary>
        public async Task<IActionResult> ManageTaskViewersModal(int teamId)
        {
            try
            {
                var team = _teamRepository.GetTeamById(teamId);
                if (team == null)
                    return NotFound();

                // بررسی دسترسی کاربر
                var currentUserId = _userManager.GetUserId(User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == team.BranchId))
                    return Forbid();

                // دریافت اطلاعات مجوزهای فعلی
                var model = new ManageTeamTaskViewersViewModel
                {
                    TeamId = teamId,
                    TeamTitle = team.Title,
                    BranchId = team.BranchId,
                    TeamMembers = _teamRepository.GetTeamMembers(teamId).Where(m => m.IsActive).ToList(),
                    ExistingViewers = new List<TaskViewerViewModel>(),
                    AvailableUsers = _teamRepository.GetAvailableUsersForBranch(team.BranchId),
                    AvailableTeams = _teamRepository.GetTeamsByBranchId(team.BranchId)
                };

                // دریافت مجوزهای موجود
                if (_uow.TaskViewerUW != null)
                {
                    var existingViewers = _uow.TaskViewerUW.Get(tv => tv.TeamId == teamId && tv.IsActive);
                    model.ExistingViewers = existingViewers.Select(tv => new TaskViewerViewModel
                    {
                        Id = tv.Id,
                        UserId = tv.UserId,
                        UserFullName = tv.User != null ? $"{tv.User.FirstName} {tv.User.LastName}" : "نامشخص",
                        AccessType = tv.AccessType,
                        AccessTypeText = GetAccessTypeText(tv.AccessType),
                        Description = tv.Description,
                        AddedDate = tv.AddedDate,
                        IsActive = tv.IsActive
                    }).ToList();
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Team",
                    "ManageTaskViewersModal",
                    $"نمایش مدیریت تبصره‌های تسک برای تیم: {team.Title}",
                    recordId: teamId.ToString()
                );

                return PartialView("_ManageTaskViewersModal", model);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "ManageTaskViewersModal",
                    "خطا در نمایش مدیریت تبصره‌های تسک",
                    ex,
                    recordId: teamId.ToString()
                );
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }

        /// <summary>
        /// اعطای مجوز خاص مشاهده تسک‌ها
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantSpecialPermissionSubmit(GrantSpecialPermissionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = "داده‌های ورودی نامعتبر است.",
                        errors = errors
                    });
                }

                // بررسی دسترسی
                var team = _teamRepository.GetTeamById(model.TeamId);
                if (team == null)
                {
                    return Json(new { success = false, message = "تیم مورد نظر یافت نشد." });
                }

                var currentUserId = _userManager.GetUserId(User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == team.BranchId))
                {
                    return Json(new { success = false, message = "شما به این تیم دسترسی ندارید." });
                }

                // اعطای مجوز با استفاده از TaskViewerRepository (اگر در دسترسی باشد)
                int createdCount = 0;
                
                // فعلاً از UnitOfWork استفاده می‌کنیم
                var taskViewer = new TaskViewer
                {
                    TeamId = model.TeamId,
                    UserId = model.GranteeUserId,
                    AccessType = model.AccessType,
                    SpecialPermissionType = model.PermissionType,
                    AddedByUserId = currentUserId,
                    AddedDate = DateTime.Now,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    IsActive = true,
                    Description = model.Description
                };

                _uow.TaskViewerUW.Create(taskViewer);
                _uow.Save();
                createdCount = 1;

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Team",
                    "GrantSpecialPermissionSubmit",
                    $"اعطای مجوز مشاهده تسک برای کاربر در تیم: {team.Title}",
                    recordId: taskViewer.Id.ToString(),
                    entityType: "TaskViewer"
                );

                return Json(new
                {
                    success = true,
                    message = $"مجوز با موفقیت اعطا شد. {createdCount} مجوز ایجاد شد.",
                    status = "refresh-modal",
                    refreshUrl = Url.Action("ManageTaskViewersModal", new { teamId = model.TeamId })
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "GrantSpecialPermissionSubmit",
                    "خطا در اعطای مجوز مشاهده تسک",
                    ex,
                    recordId: model.TeamId.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = "خطا در اعطای مجوز: " + ex.Message
                });
            }
        }

        /// <summary>
        /// حذف مجوز مشاهده تسک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTaskViewerPermission(int viewerId)
        {
            try
            {
                var viewer = _uow.TaskViewerUW.GetById(viewerId);
                if (viewer == null)
                {
                    return Json(new { success = false, message = "مجوز مورد نظر یافت نشد." });
                }

                // بررسی دسترسی
                var team = _teamRepository.GetTeamById(viewer.TeamId ?? 0);
                if (team == null)
                {
                    return Json(new { success = false, message = "تیم مرتبط یافت نشد." });
                }

                var currentUserId = _userManager.GetUserId(User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == team.BranchId))
                {
                    return Json(new { success = false, message = "شما به این عملیات دسترسی ندارید." });
                }

                _uow.TaskViewerUW.Delete(viewer);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Delete,
                    "Team",
                    "RemoveTaskViewerPermission",
                    $"حذف مجوز مشاهده تسک از تیم: {team.Title}",
                    recordId: viewerId.ToString(),
                    entityType: "TaskViewer"
                );

                return Json(new
                {
                    success = true,
                    message = "مجوز با موفقیت حذف شد.",
                    status = "refresh-modal",
                    refreshUrl = Url.Action("ManageTaskViewersModal", new { teamId = team.Id })
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "RemoveTaskViewerPermission",
                    "خطا در حذف مجوز مشاهده تسک",
                    ex,
                    recordId: viewerId.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = "خطا در حذف مجوز: " + ex.Message
                });
            }
        }

        /// <summary>
        /// تولید خودکار مجوزهای مشاهده بر اساس سمت‌های تیم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePositionBasedViewers(int teamId)
        {
            try
            {
                var team = _teamRepository.GetTeamById(teamId);
                if (team == null)
                {
                    return Json(new { success = false, message = "تیم مورد نظر یافت نشد." });
                }

                // بررسی دسترسی
                var currentUserId = _userManager.GetUserId(User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == team.BranchId))
                {
                    return Json(new { success = false, message = "شما به این عملیات دسترسی ندارید." });
                }

                // دریافت سمت‌های تیم و اعضا
                var positions = _teamRepository.GetTeamPositions(teamId);
                int createdCount = 0;

                foreach (var position in positions.Where(p => p.CanViewSubordinateTasks || p.CanViewPeerTasks))
                {
                    var membersInPosition = position.TeamMembers.Where(tm => tm.IsActive);
                    
                    foreach (var member in membersInPosition)
                    {
                        // بررسی عدم وجود مجوز مشابه
                        var existingViewer = _uow.TaskViewerUW.Get(tv => 
                            tv.UserId == member.UserId && 
                            tv.TeamId == teamId && 
                            tv.AccessType == 2 && // عضو تیم
                            tv.IsActive).FirstOrDefault();

                        if (existingViewer == null)
                        {
                            var taskViewer = new TaskViewer
                            {
                                TeamId = teamId,
                                UserId = member.UserId,
                                AccessType = 2, // عضو تیم
                                AddedByUserId = currentUserId,
                                AddedDate = DateTime.Now,
                                IsActive = true,
                                Description = $"تولید خودکار بر اساس سمت: {position.Title}"
                            };

                            _uow.TaskViewerUW.Create(taskViewer);
                            createdCount++;
                        }
                    }
                }

                if (createdCount > 0)
                {
                    _uow.Save();
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Create,
                    "Team",
                    "GeneratePositionBasedViewers",
                    $"تولید خودکار {createdCount} مجوز مشاهده تسک برای تیم: {team.Title}",
                    recordId: teamId.ToString()
                );

                return Json(new
                {
                    success = true,
                    message = $"تولید خودکار مجوزها انجام شد. {createdCount} مجوز جدید ایجاد شد.",
                    status = "refresh-modal",
                    refreshUrl = Url.Action("ManageTaskViewersModal", new { teamId = teamId })
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "GeneratePositionBasedViewers",
                    "خطا در تولید خودکار مجوزهای مشاهده",
                    ex,
                    recordId: teamId.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = "خطا در تولید خودکار مجوزها: " + ex.Message
                });
            }
        }
        /// <summary>
        /// نمایش مودال تأیید حذف عضو از تیم
        /// </summary>
        [HttpGet]
        public IActionResult RemoveMemberModal(int memberId)
        {
            try
            {
                var member = _teamRepository.GetTeamMemberById(memberId);
                if (member == null)
                    return NotFound();

                return PartialView("_RemoveMemberModal", member);
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }

        /// <summary>
        /// حذف عضو از تیم با پاسخ JSON
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveMemberSubmit(int memberId)
        {
            try
            {
                var member = _teamRepository.GetTeamMemberById(memberId);
                if (member == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = new[] {
                    new { status = "error", text = "عضو مورد نظر یافت نشد." }
                }
                    });
                }

                var result = _teamRepository.DeleteTeamMemberWithValidation(memberId);

                if (result.IsSuccess)
                {
                    return Json(new
                    {
                        success = true,
                        message = new[] {
                    new { status = "success", text = result.Message }
                },
                        status = "redirect",
                        redirectUrl = Url.Action("Details", new { id = member.TeamId })
                    });
                }
                else
                {
                    var errorMessages = new List<object>
                    {
                        new { status = "error", text = result.Message }
                    };

                    // اضافه کردن خطاهای اضافی اگر وجود دارد
                    foreach (var error in result.Errors)
                    {
                        errorMessages.Add(new { status = "error", text = error });
                    }

                    return Json(new
                    {
                        success = false,
                        message = errorMessages.ToArray()
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = new[] {
                        new { status = "error", text = "خطا: " + ex.Message }
                    }
                });
            }
        }

        /// <summary>
        /// نمایش مودال تأیید حذف عضو از سمت
        /// </summary>
        [HttpGet]
        public IActionResult RemoveMemberFromPositionModal(int memberId)
        {
            try
            {
                var member = _teamRepository.GetTeamMemberById(memberId);
                if (member == null)
                    return NotFound();

                if (!member.PositionId.HasValue)
                {
                    return BadRequest("این عضو در هیچ سمتی قرار ندارد.");
                }

                return PartialView("_RemoveMemberFromPositionModal", member);
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }

        /// <summary>
        /// حذف عضو از سمت (نه از تیم)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveMemberFromPositionSubmit(int memberId)
        {
            try
            {
                var member = _teamRepository.GetTeamMemberById(memberId);
                if (member == null)
                {
                    return Json(new { success = false, message = "عضو مورد نظر یافت نشد." });
                }

                if (!member.PositionId.HasValue)
                {
                    return Json(new { success = false, message = "این عضو در هیچ سمتی قرار ندارد." });
                }

                var success = _teamRepository.RemoveMemberFromPosition(memberId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "عضو با موفقیت از سمت خارج شد.",
                        status = "redirect",
                        redirectUrl = Url.Action("Details", new { id = member.TeamId })
                    });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در خارج کردن عضو از سمت." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا: " + ex.Message });
            }
        }
        /// <summary>
        /// نمایش مودال ویرایش عضو تیم
        /// </summary>
        [HttpGet]
        public IActionResult EditMemberModal(int id)
        {
            try
            {
                var member = _teamRepository.GetTeamMemberById(id);
                if (member == null)
                    return NotFound();

                // بررسی دسترسی کاربر
                var team = _teamRepository.GetTeamById(member.TeamId);
                if (team == null)
                    return NotFound();

                var currentUserId = _userManager.GetUserId(User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == team.BranchId))
                    return Forbid();

                // آماده‌سازی ViewBag ها
                PrepareTeamMemberViewBags(team.BranchId, member.TeamId);

                return PartialView("_EditMemberModal", member);
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }

        /// <summary>
        /// ویرایش عضو تیم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditMemberSubmit(TeamMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => new { status = "error", text = e.ErrorMessage })
                    .ToArray();

                return Json(new
                {
                    success = false,
                    message = errors
                });
            }

            var result = _teamRepository.UpdateTeamMemberWithValidation(model);

    if (result.IsSuccess)
    {
        return Json(new
        {
            success = true,
            message = new[] {
                new { status = "success", text = result.Message }
            },
            status = "redirect",
            redirectUrl = Url.Action("Details", new { id = model.TeamId })
        });
    }
    else
    {
        var errorMessages = new List<object>
        {
            new { status = "error", text = result.Message }
        };

        foreach (var error in result.Errors)
        {
            errorMessages.Add(new { status = "error", text = error });
        }

        return Json(new
        {
            success = false,
            message = errorMessages.ToArray()
        });
    }
}

        /// <summary>
        /// نمایش مودال تخصیص سمت به عضو
        /// </summary>
        [HttpGet]
        public IActionResult AssignPositionModal(int memberId)
        {
            try
            {
                var member = _teamRepository.GetTeamMemberById(memberId);
                if (member == null)
                    return NotFound();

                // بررسی اینکه عضو قبلاً سمت ندارد
                if (member.PositionId.HasValue)
                {
                    return BadRequest("این عضو قبلاً دارای سمت است.");
                }

                var team = _teamRepository.GetTeamById(member.TeamId);
                if (team == null)
                    return NotFound();

                // دریافت سمت‌های موجود در تیم
                var availablePositions = _teamRepository.GetTeamPositions(member.TeamId)
                    .Where(p => p.IsActive && (!p.MaxMembers.HasValue || p.TeamMembers.Count(tm => tm.IsActive) < p.MaxMembers.Value))
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.Title} (قدرت: {p.PowerLevel})"
                    }).ToList();

                ViewBag.AvailablePositions = availablePositions;

                var model = new AssignPositionViewModel
                {
                    MemberId = memberId,
                    TeamId = member.TeamId,
                    UserFullName = member.UserFullName,
                    TeamTitle = team.Title
                };

                return PartialView("_AssignPositionModal", model);
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }
        /// <summary>
        /// تخصیص سمت به عضو
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignPositionSubmit(AssignPositionViewModel model,
            string customTitle, int? customPowerLevel, string customDescription,
            bool customCanViewSubordinateTasks = true, bool customCanViewPeerTasks = false, int? customLevel = null)
        {
            try
            {
                int positionId;

                // بررسی اینکه آیا سمت دستی انتخاب شده یا نه
                if (model.PositionId.ToString() == "CUSTOM")
                {
                    // ایجاد سمت جدید برای این عضو
                    if (string.IsNullOrWhiteSpace(customTitle))
                    {
                        return Json(new { success = false, message = "عنوان سمت الزامی است." });
                    }

                    // اولویت با customLevel، سپس customPowerLevel
                    int finalPowerLevel;
                    if (customLevel.HasValue)
                    {
                        finalPowerLevel = customLevel.Value;
                    }
                    else if (customPowerLevel.HasValue)
                    {
                        finalPowerLevel = customPowerLevel.Value;
                    }
                    else
                    {
                        return Json(new { success = false, message = "سطح قدرت الزامی است." });
                    }

                    // بررسی محدوده سطح قدرت
                    if (finalPowerLevel < 0 || finalPowerLevel > 100)
                    {
                        return Json(new { success = false, message = "سطح قدرت باید بین 0 تا 100 باشد." });
                    }

                    // بررسی تکراری نبودن عنوان
                    if (!_teamRepository.IsPositionTitleUnique(model.TeamId, customTitle))
                    {
                        return Json(new { success = false, message = "سمت با این عنوان قبلاً در این تیم تعریف شده است." });
                    }

                    // ایجاد سمت جدید
                    var newPosition = new TeamPosition
                    {
                        TeamId = model.TeamId,
                        Title = customTitle,
                        Description = customDescription ?? $"سمت دستی ایجاد شده برای {model.UserFullName}",
                        PowerLevel = finalPowerLevel,
                        CanViewSubordinateTasks = customCanViewSubordinateTasks,
                        CanViewPeerTasks = customCanViewPeerTasks,
                        MaxMembers = 1, // فقط برای همین عضو
                        IsDefault = false,
                        IsActive = true,
                        DisplayOrder = finalPowerLevel,
                        CreatorUserId = _userManager.GetUserId(User)
                    };

                    positionId = _teamRepository.CreateTeamPosition(newPosition);
                }
                else
                {
                    // استفاده از سمت موجود
                    positionId = model.PositionId;
                }

                var success = _teamRepository.AssignMemberToPosition(model.MemberId, positionId);

                if (success)
                {
                    var message = model.PositionId.ToString() == "CUSTOM"
                        ? $"سمت جدید '{customTitle}' با سطح قدرت " +
                          (customLevel.HasValue ? $"{customLevel.Value} (سفارشی)" : $"{customPowerLevel.Value}") +
                          " ایجاد شد و به عضو تخصیص یافت."
                        : "سمت با موفقیت تخصیص یافت.";

                    return Json(new
                    {
                        success = true,
                        message = message,
                        status = "redirect",
                        redirectUrl = Url.Action("Details", new { id = model.TeamId })
                    });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در تخصیص سمت." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا: " + ex.Message });
            }
        }

        /// <summary>
        /// نمایش مودال اضافه کردن عضو به سمت خاص
        /// </summary>
        [HttpGet]
        public IActionResult AddMemberToPositionModal(int teamId, int positionId)
        {
            try
            {
                var team = _teamRepository.GetTeamById(teamId);
                var position = _teamRepository.GetTeamPositionById(positionId);
        
                if (team == null || position == null)
                    return NotFound();

                // بررسی ظرفیت سمت
                if (position.MaxMembers.HasValue && position.TeamMembers.Count(tm => tm.IsActive) >= position.MaxMembers.Value)
                {
                    return BadRequest("ظرفیت این سمت تکمیل شده است.");
                }

                // دریافت اعضای تیم که سمت ندارند یا سمت متفاوتی دارند
                var availableMembers = _teamRepository.GetTeamMembers(teamId)
                    .Where(m => m.IsActive && (!m.PositionId.HasValue || m.PositionId != positionId))
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.UserFullName
                    }).ToList();

                ViewBag.AvailableMembers = availableMembers;

                var model = new AddMemberToPositionViewModel
                {
                    TeamId = teamId,
                    PositionId = positionId,
                    TeamTitle = team.Title,
                    PositionTitle = position.Title
                };

                return PartialView("_AddMemberToPositionModal", model);
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در بارگذاری فرم: " + ex.Message);
            }
        }

        /// <summary>
        /// اضافه کردن عضو به سمت خاص
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMemberToPositionSubmit(AddMemberToPositionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "داده‌های ورودی نامعتبر است." });
            }

            try
            {
                var success = _teamRepository.AssignMemberToPosition(model.MemberId, model.PositionId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "عضو با موفقیت به سمت اضافه شد.",
                        status = "redirect",
                        redirectUrl = Url.Action("Details", new { id = model.TeamId })
                    });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در اضافه کردن عضو به سمت." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا: " + ex.Message });
            }
        }

        /// <summary>
        /// فعال‌سازی مجدد تبصره غیرفعال
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateTaskViewerPermission(int viewerId)
        {
            try
            {
                var viewer = _uow.TaskViewerUW.GetById(viewerId);
                if (viewer == null)
                {
                    return Json(new { success = false, message = "تبصره مورد نظر یافت نشد." });
                }

                // بررسی دسترسی
                var team = _teamRepository.GetTeamById(viewer.TeamId ?? 0);
                if (team == null)
                {
                    return Json(new { success = false, message = "تیم مرتبط یافت نشد." });
                }

                var currentUserId = _userManager.GetUserId(User);
                var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
                if (!userBranches.Any(b => b.Id == team.BranchId))
                {
                    return Json(new { success = false, message = "شما به این عملیات دسترسی ندارید." });
                }

                // فعال‌سازی مجدد
                viewer.IsActive = true;
                viewer.LastUpdateDate = DateTime.Now;
                viewer.LastUpdaterUserId = currentUserId;

                _uow.TaskViewerUW.Update(viewer);
                _uow.Save();

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.Edit,
                    "Team",
                    "ReactivateTaskViewerPermission",
                    $"فعال‌سازی مجدد تبصره مشاهده تسک برای کاربر در تیم: {team.Title}",
                    recordId: viewerId.ToString(),
                    entityType: "TaskViewer"
                );

                return Json(new
                {
                    success = true,
                    message = "تبصره با موفقیت فعال شد."
                });
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync(
                    "Team",
                    "ReactivateTaskViewerPermission",
                    "خطا در فعال‌سازی مجدد تبصره",
                    ex,
                    recordId: viewerId.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = "خطا در فعال‌سازی تبصره: " + ex.Message
                });
            }
        }

        /// <summary>
        /// تکمیل شدن متد GetAccessTypeText که در TaskViewer استفاده می‌شود
        /// </summary>
        private string GetAccessTypeText(byte accessType)
        {
            return accessType switch
            {
                0 => "مجوز خاص",
                1 => "مدیر تیم",
                2 => "عضو تیم",
                3 => "دسترسی عمومی",
                4 => "سازنده",
                5 => "منتصب",
                _ => "نامشخص"
            };
        }
    }
    }
