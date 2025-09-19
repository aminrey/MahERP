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

        public TeamController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            ITeamRepository teamRepository,
            IBranchRepository branchRepository,
            IUserManagerRepository userRepository,
            IMapper mapper) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger)
        {
            _teamRepository = teamRepository;
            _branchRepository = branchRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        #region Index and Chart Views

        /// <summary>
        /// نمایش لیست تیم‌ها
        /// </summary>
        public IActionResult Index(int branchId = 0)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);

            if (branchId == 0)
                branchId = userBranches.FirstOrDefault()?.Id ?? 0;

            if (branchId == 0 || !userBranches.Any(b => b.Id == branchId))
                return RedirectToAction("ErrorView", "Home");

            var teams = _teamRepository.GetTeamsByBranchId(branchId);
            ViewBag.BranchId = branchId;
            ViewBag.BranchName = userBranches.FirstOrDefault(b => b.Id == branchId)?.Name;
            ViewBag.UserBranches = new SelectList(userBranches, "Id", "Name", branchId);

            return View(teams);
        }

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
                return RedirectToAction("Index");

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

                team.CreatorUserId = _userManager.GetUserId(User);
                team.CreateDate = DateTime.Now;
                team.LastUpdaterUserId = currentUserId; // این خط اضافه شده
                team.LastUpdateDate = DateTime.Now; // این خط هم اضافه شده

                var teamId = _teamRepository.CreateTeam(team);

                TempData["SuccessMessage"] = "تیم با موفقیت ایجاد شد.";
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
            var team = _teamRepository.GetTeamById(id);
            if (team == null)
                return NotFound();

            // بررسی دسترسی کاربر به این شعبه
            var currentUserId = _userManager.GetUserId(User);
            var userBranches = _branchRepository.GetBrnachListByUserId(currentUserId);
            if (!userBranches.Any(b => b.Id == team.BranchId))
                return RedirectToAction("ErrorView", "Home");

            // دریافت اعضای تیم
            team.TeamMembers = _teamRepository.GetTeamMembers(id);

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
        /// تغییر وضعیت فعال/غیرفعال تیم
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
                    return Json(new { success = true, message = "مدیر تیم با موفقیت انتخاب شد." });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در انتخاب مدیر تیم." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا: " + ex.Message });
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
                    return Json(new { success = true, message = "مدیر تیم با موفقیت حذف شد." });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در حذف مدیر تیم." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا: " + ex.Message });
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

            PrepareTeamMemberViewBags(team.BranchId);
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
            // دریافت تیم‌های والد ممکن
            var parentTeams = _teamRepository.GetTeamsByBranchId(branchId)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Title
                }).ToList();

            parentTeams.Insert(0, new SelectListItem { Value = "", Text = "-- بدون تیم والد --" });
            ViewBag.ParentTeams = parentTeams;

            // دریافت کاربران موجود برای انتخاب مدیر
            var availableUsers = _teamRepository.GetAvailableUsersForBranch(branchId)
                .Select(u => new SelectListItem
                {
                    Value = u.UserId,
                    Text = u.FullName + (string.IsNullOrEmpty(u.Position) ? "" : $" ({u.Position})")
                }).ToList();

            availableUsers.Insert(0, new SelectListItem { Value = "", Text = "-- انتخاب مدیر --" });
            ViewBag.AvailableUsers = availableUsers;

            ViewBag.AccessLevels = new SelectList(new[]
            {
                new { Value = 0, Text = "عمومی" },
                new { Value = 1, Text = "محدود" }
            }, "Value", "Text");
        }

        private void PrepareTeamMemberViewBags(int branchId)
        {
            var availableUsers = _teamRepository.GetAvailableUsersForBranch(branchId)
                .Select(u => new SelectListItem
                {
                    Value = u.UserId,
                    Text = u.FullName + (string.IsNullOrEmpty(u.Position) ? "" : $" ({u.Position})")
                }).ToList();

            ViewBag.AvailableUsers = availableUsers;

            ViewBag.MembershipTypes = new SelectList(new[]
            {
                new { Value = 0, Text = "عضو عادی" },
                new { Value = 1, Text = "عضو ویژه" },
                new { Value = 2, Text = "مدیر تیم" }
            }, "Value", "Text");
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

                PrepareTeamMemberViewBags(team.BranchId);
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
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = "داده‌های ورودی نامعتبر است.",
                    errors = errors
                });
            }

            try
            {
                var member = _mapper.Map<TeamMember>(model);
                member.AddedByUserId = _userManager.GetUserId(User);

                var memberId = _teamRepository.AddTeamMember(member);

                // بازگرداندن محتوای بروزرسانی شده برای جدول اعضا
                var updatedTeam = _teamRepository.GetTeamById(model.TeamId);
                updatedTeam.TeamMembers = _teamRepository.GetTeamMembers(model.TeamId);

                return Json(new
                {
                    success = true,
                    message = "عضو با موفقیت به تیم اضافه شد.",
                    status = "update-view",
                    viewList = new[] {
                new {
                    elementId = "team-members-table",
                    view = new {
                        result = this.RenderViewToStringAsync("_TeamMembersTable", updatedTeam.TeamMembers)
                    }
                }
            }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "خطا در اضافه کردن عضو: " + ex.Message
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
                    message = "داده‌های ورودی نامعتبر است."
                });
            }

            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var success = _teamRepository.AssignManager(model.TeamId, model.ManagerUserId, currentUserId);

                if (success)
                {
                    // بازگرداندن محتوای بروزرسانی شده
                    var updatedTeam = _teamRepository.GetTeamById(model.TeamId);

                    return Json(new
                    {
                        success = true,
                        message = "مدیر تیم با موفقیت انتخاب شد.",
                        status = "update-view",
                        viewList = new[] {
                    new {
                        elementId = "manager-info-card",
                        view = new {
                            result =   this.RenderViewToStringAsync("_ManagerCard", updatedTeam)
                        }
                    }
                }
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "خطا در انتخاب مدیر تیم."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "خطا: " + ex.Message
                });
            }
        }

        /// <summary>
        /// نمایش مودال تایید حذف تیم
        /// </summary>
        [HttpGet]
        public IActionResult DeleteConfirmModal(int teamId)
        {
            try
            {
                var team = _teamRepository.GetTeamById(teamId);
                if (team == null)
                    return NotFound();

                var canDelete = _teamRepository.CanDeleteTeam(teamId);

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
        public IActionResult DeleteSubmit(int id)
        {
            try
            {
                if (!_teamRepository.CanDeleteTeam(id))
                {
                    return Json(new
                    {
                        success = false,
                        message = "امکان حذف این تیم وجود ندارد. ابتدا تیم‌های فرزند و اعضا را حذف کنید."
                    });
                }

                var success = _teamRepository.DeleteTeam(id);
                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "تیم با موفقیت حذف شد.",
                        status = "redirect",
                        redirectUrl = Url.Action("Index")
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "خطا در حذف تیم."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "خطا در حذف تیم: " + ex.Message
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
        public IActionResult RemoveManagerSubmit(int teamId)
        {
            try
            {
                var success = _teamRepository.RemoveManager(teamId);

                if (success)
                {
                    return Json(new {
                        success = true,
                        message = "مدیر تیم با موفقیت حذف شد.",
                        status = "redirect",
                        redirectUrl = Url.Action("OrganizationalChart", new { branchId = ViewBag.BranchId })
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "خطا در حذف مدیر تیم." 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "خطا: " + ex.Message 
                });
            }
        }

    }
}