using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.RoleViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.Areas.AppCoreArea.Controllers.PermissionControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("CORE.ROLE")]
    public class RoleController : BaseController
    {
        private readonly IUserRoleRepository _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IMapper _mapper;

        public RoleController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IUserRoleRepository roleService,
            IPermissionService permissionService,
            IMapper mapper, IBaseRepository BaseRepository)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository , BaseRepository)
        {
            _roleService = roleService;
            _permissionService = permissionService;
            _mapper = mapper;
        }

        // GET: Role
        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                var viewModels = new List<RoleViewModel>();

                foreach (var role in roles)
                {
                    var viewModel = _mapper.Map<RoleViewModel>(role);
                    viewModel.UsersCount = await _roleService.GetRoleUsersCountAsync(role.Id);
                    viewModel.PermissionsCount = await _roleService.GetRolePermissionsCountAsync(role.Id);
                    viewModels.Add(viewModel);
                }

                await _activityLogger.LogActivityAsync(
                    ActivityTypeEnum.View,
                    "Role",
                    "Index",
                    "مشاهده لیست نقش‌ها"
                );

                return View(viewModels);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Role", "Index", "خطا در دریافت لیست نقش‌ها", ex);
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // GET: Role/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ تولید خودکار NameEn از NameFa
                    if (string.IsNullOrEmpty(model.NameEn))
                    {
                        model.NameEn = GenerateNameEn(model.NameFa);
                    }

                    // بررسی تکراری نبودن
                    if (await _roleService.RoleExistsAsync(model.NameEn))
                    {
                        // اگر تکراری بود، یک suffix اضافه کن
                        model.NameEn = model.NameEn + "_" + DateTime.Now.Ticks;
                    }

                    // ✅ تنظیم مقادیر پیش‌فرض
                    model.Priority = 5;
                    model.Color = "#007bff";
                    model.Icon = "fa fa-user-shield";
                    model.IsActive = true;
                    model.IsSystemRole = false;

                    var currentUserId = _userManager.GetUserId(User);
                    var result = await _roleService.CreateRoleAsync(model, currentUserId);

                    if (result)
                    {
                        await _activityLogger.LogActivityAsync(
                            ActivityTypeEnum.Create,
                            "Role",
                            "Create",
                            $"ایجاد نقش جدید: {model.NameFa}",
                            recordId: model.Id.ToString(),
                            entityType: "Role",
                            recordTitle: model.NameFa
                        );

                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "خطا در ایجاد نقش");
                }
                catch (Exception ex)
                {
                    await _activityLogger.LogErrorAsync("Role", "Create", "خطا در ایجاد نقش", ex);
                    ModelState.AddModelError("", "خطا در ایجاد نقش");
                }
            }

            return View(model);
        }

        // ✅ Helper method برای تولید NameEn
        private string GenerateNameEn(string nameFa)
        {
            if (string.IsNullOrEmpty(nameFa))
                return "Role_" + DateTime.Now.Ticks;

            // حذف فاصله‌ها و کاراکترهای خاص
            var nameEn = new string(nameFa
                .Where(c => char.IsLetterOrDigit(c) || c == '_')
                .ToArray());

            // اگر خالی شد یا فقط فارسی بود
            if (string.IsNullOrEmpty(nameEn) || nameEn.All(c => c > 127))
            {
                nameEn = "Role_" + DateTime.Now.Ticks;
            }

            return nameEn;
        }

        // GET: Role/ManagePermissions/5
        [HttpGet]
        public async Task<IActionResult> ManagePermissions(int id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                    return RedirectToAction("ErrorView", "Home");

                var selectedPermissionIds = await _roleService.GetRolePermissionIdsAsync(id);
                var permissionTree = await _permissionService.BuildPermissionTreeAsync(selectedPermissionIds);

                var viewModel = new ManageRolePermissionsViewModel
                {
                    RoleId = id,
                    RoleName = role.NameFa,
                    PermissionTree = permissionTree,
                    SelectedPermissionIds = selectedPermissionIds
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Role", "ManagePermissions", "خطا در نمایش صفحه مدیریت دسترسی‌ها", ex, recordId: id.ToString());
                return RedirectToAction("ErrorView", "Home");
            }
        }

        // POST: Role/ManagePermissions
        [HttpPost]
        [ValidateAntiForgeryToken]  
        public async Task<IActionResult> ManagePermissions(ManageRolePermissionsViewModel model)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var result = await _roleService.AssignPermissionsToRoleAsync(
                    model.RoleId, 
                    model.SelectedPermissionIds, 
                    currentUserId
                );

                if (result)
                {
                    await _activityLogger.LogActivityAsync(
                        ActivityTypeEnum.Edit,
                        "Role",
                        "ManagePermissions",
                        $"به‌روزرسانی دسترسی‌های نقش: {model.RoleName}",
                        recordId: model.RoleId.ToString(),
                        entityType: "Role",
                        recordTitle: model.RoleName
                    );

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "خطا در به‌روزرسانی دسترسی‌ها");
            }
            catch (Exception ex)
            {
                await _activityLogger.LogErrorAsync("Role", "ManagePermissions", "خطا در به‌روزرسانی دسترسی‌ها", ex, recordId: model.RoleId.ToString());
                ModelState.AddModelError("", "خطا در به‌روزرسانی دسترسی‌ها");
            }

            // بازگشت به فرم با خطا
            model.PermissionTree = await _permissionService.BuildPermissionTreeAsync(model.SelectedPermissionIds);
            return View(model);
        }
    }
}