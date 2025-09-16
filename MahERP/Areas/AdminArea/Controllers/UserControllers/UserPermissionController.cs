using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Linq;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize(Roles = "Admin,Manager")]
    public class UserPermissionController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IRoleRepository _roleRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public UserPermissionController(
            IUnitOfWork uow,
            IRoleRepository roleRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger)
        {
            _uow = uow;
            _roleRepository = roleRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        // لیست کاربران با دسترسی‌ها
        public IActionResult Index()
        {
            var users = _userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToList();

            var userPermissions = users.Select(user => new UserPermissionViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PositionName = user.PositionName,
                IsActive = user.IsActive,
                RegisterDate = user.RegisterDate,
                SystemRoles = _userManager.GetRolesAsync(user).Result.ToList(),
                RolePatterns = _roleRepository.GetUserRolePatterns(user.Id)
                    .Select(urp => new UserRolePatternInfo
                    {
                        Id = urp.Id,
                        RolePatternId = urp.RolePatternId,
                        PatternName = urp.RolePattern.PatternName,
                        Description = urp.RolePattern.Description,
                        AssignDate = urp.AssignDate,
                        AssignedByName = $"{urp.AssignedByUser.FirstName} {urp.AssignedByUser.LastName}",
                        StartDate = urp.StartDate,
                        EndDate = urp.EndDate,
                        IsActive = urp.IsActive,
                        Notes = urp.Notes
                    }).ToList()
            }).ToList();

            return View(userPermissions);
        }

        // تخصیص الگوی نقش به کاربر - نمایش فرم
        [HttpGet]
        public IActionResult AssignRolePattern(string userId = null)
        {
            PopulateDropdowns(userId);
            
            var model = new AssignRolePatternViewModel();
            if (!string.IsNullOrEmpty(userId))
            {
                var user = _userManager.FindByIdAsync(userId).Result;
                if (user != null)
                {
                    model.UserId = userId;
                    model.UserName = $"{user.FirstName} {user.LastName}";
                }
            }

            return PartialView("_AssignRolePattern", model);
        }

        // تخصیص الگوی نقش به کاربر - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignRolePattern(AssignRolePatternViewModel model)
        {
            if (ModelState.IsValid)
            {
                // بررسی اینکه کاربر قبلاً این الگوی نقش را ندارد
                var existingAssignment = _roleRepository.GetUserRolePatterns(model.UserId)
                    .FirstOrDefault(urp => urp.RolePatternId == model.RolePatternId && urp.IsActive);

                if (existingAssignment != null)
                {
                    ModelState.AddModelError("RolePatternId", "این الگوی نقش قبلاً به کاربر تخصیص داده شده است");
                    PopulateDropdowns(model.UserId);
                    return PartialView("_AssignRolePattern", model);
                }

                var userRolePattern = new UserRolePattern
                {
                    UserId = model.UserId,
                    RolePatternId = model.RolePatternId,
                    AssignedByUserId = _userManager.GetUserId(User),
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Notes = model.Notes,
                    IsActive = model.IsActive
                };

                if (_roleRepository.AssignRolePatternToUser(userRolePattern))
                {
                    return Json(new { 
                        success = true, 
                        message = new[] { new { status = "success", text = "الگوی نقش با موفقیت تخصیص داده شد" } }
                    });
                }

                ModelState.AddModelError("", "خطا در تخصیص الگوی نقش");
            }

            // در صورت خطا، مودال را دوباره نمایش دهید
            PopulateDropdowns(model.UserId);
            return PartialView("_AssignRolePattern", model);
        }

        // Updated the call to `_uow.UserRolePatternUW.Get` to remove the `includeProperties` parameter
        // and replaced it with a LINQ query to include related entities.

        [HttpGet]
        public IActionResult RemoveRolePattern(int id)
        {
            var userRolePattern = _uow.UserRolePatternUW.Get(
                urp => urp.Id == id
            ).FirstOrDefault();

            if (userRolePattern != null)
            {
                // Manually include related entities
                userRolePattern.User = _uow.UserManagerUW.GetById(userRolePattern.UserId);
                userRolePattern.RolePattern = _uow.RolePatternUW.GetById(userRolePattern.RolePatternId);
                userRolePattern.AssignedByUser = _uow.UserManagerUW.GetById(userRolePattern.AssignedByUserId);
            }

            if (userRolePattern == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";
            ViewBag.ViewTitle = "حذف تخصیص الگوی نقش";

            var viewModel = new UserRolePatternInfo
            {
                Id = userRolePattern.Id,
                PatternName = userRolePattern.RolePattern.PatternName,
                AssignedByName = $"{userRolePattern.AssignedByUser.FirstName} {userRolePattern.AssignedByUser.LastName}",
                AssignDate = userRolePattern.AssignDate
            };

            return PartialView("_RemoveRolePattern", viewModel);
        }

        // حذف تخصیص الگوی نقش - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveRolePatternPost(int id)
        {
            if (_roleRepository.RemoveRolePatternFromUser(id))
            {
                return Json(new { success = true, message = "تخصیص الگوی نقش با موفقیت حذف شد" });
            }

            return Json(new { success = false, message = "خطا در حذف تخصیص الگوی نقش" });
        }

        // مشاهده دسترسی‌های کاربر
        public IActionResult UserPermissions(string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null)
                return RedirectToAction("ErrorView", "Home");

            var permissions = _roleRepository.GetUserPermissions(userId);
            var rolePatterns = _roleRepository.GetUserRolePatterns(userId);

            var viewModel = new UserPermissionDetailViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                FullName = $"{user.FirstName} {user.LastName}",
                SystemRoles = _userManager.GetRolesAsync(user).Result.ToList(),
                RolePatterns = _mapper.Map<List<UserRolePatternInfo>>(rolePatterns),
                Permissions = permissions.GroupBy(p => p.Split('.')[0])
                    .ToDictionary(g => g.Key, g => g.ToList())
            };

            return View(viewModel);
        }

        // لاگ دسترسی‌ها
        public IActionResult PermissionLogs(string userId = null)
        {
            var logs = _roleRepository.GetPermissionLogs(userId, DateTime.Now.AddDays(-30), DateTime.Now);
            return View(logs);
        }

        // توابع کمکی
        private void PopulateDropdowns(string selectedUserId = null)
        {
            var users = _userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .AsEnumerable() // Switch to client-side evaluation
                .Select(u => new { 
                    Id = u.Id, 
                    Name = $"{u.FirstName} {u.LastName} ({u.UserName})" 
                })
                .OrderBy(u => u.Name)
                .ToList();

            ViewBag.Users = new SelectList(users, "Id", "Name", selectedUserId);

            var rolePatterns = _roleRepository.GetAllRolePatterns()
                .Select(rp => new { 
                    Id = rp.Id, 
                    Name = $"{rp.PatternName} - {rp.Description}" 
                })
                .ToList();

            ViewBag.RolePatterns = new SelectList(rolePatterns, "Id", "Name");
        }
    }

}