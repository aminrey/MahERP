using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.EntityFrameworkCore;
using MahERP.DataModelLayer.Extensions;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// متدهای AJAX و داده‌های دینامیک
    /// </summary>
    public partial class TaskRepository
    {
        #region AJAX and Helper Methods

        /// <summary>
        /// بروزرسانی لیست کاربران و تیم‌ها بر اساس شعبه انتخاب شده
        /// </summary>
        public async Task<BranchChangeDataViewModel> GetBranchChangeDataAsync(int branchId)
        {
            var result = new BranchChangeDataViewModel();
            
            try
            {
                // دریافت کاربران شعبه انتخاب شده
                result.Users = _BranchRipository.GetBranchUsersByBranchId(branchId, includeInactive: false);

                // دریافت تیم‌های شعبه انتخاب شده
                result.Teams = await GetBranchTeamsByBranchId(branchId);

                // دریافت طرف حساب‌های شعبه انتخاب شده
                result.Stakeholders = _StakeholderRepo.GetStakeholdersByBranchId(branchId);

                return result;
            }
            catch (Exception)
            {
                // در صورت خطا، لیست‌های خالی برگردان
                result.Users = new List<BranchUserViewModel>();
                result.Teams = new List<TeamViewModel>();
                result.Stakeholders = new List<StakeholderViewModel>();
                return result;
            }
        }

        /// <summary>
        /// متد کمکی برای دریافت تیم‌های شعبه
        /// </summary>
        private async Task<List<TeamViewModel>> GetBranchTeamsByBranchId(int branchId)
        {
            try
            {
                // دریافت تیم‌ها از طریق DbContext
                var teams = _context.Team_Tbl
                    .Where(t => t.BranchId == branchId && t.IsActive)
                    .Select(t => new TeamViewModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        BranchId = t.BranchId,
                        IsActive = t.IsActive,
                        ManagerFullName = !string.IsNullOrEmpty(t.ManagerUserId) 
                            ? _context.Users.Where(u => u.Id == t.ManagerUserId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault()
                            : "ندارد"
                    })
                    .OrderBy(t => t.Title)
                    .ToList();

                return teams;
            }
            catch (Exception)
            {
                return new List<TeamViewModel>();
            }
        }


        /// <summary>
        /// متد کمکی برای بازیابی داده‌های فرم CreateTask
        /// </summary>
        public async Task<TaskViewModel> RepopulateCreateTaskModelAsync(TaskViewModel model, string userId)
        {
            try
            {
                // بازیابی لیست شعبه‌ها
                model.branchListInitial = _BranchRipository.GetBrnachListByUserId(userId);
                
                // بازیابی تنظیمات کد تسک
                if (model.TaskCodeSettings == null)
                {
                    model.TaskCodeSettings = _taskCodeGenerator.GetTaskCodeSettings();
                }
                
                // اگر کد تسک خالی است، کد جدید تولید کن
                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = _taskCodeGenerator.GenerateTaskCode();
                }

                // بازیابی دسته‌بندی‌ها
                model.TaskCategoryInitial ??= GetAllCategories();

                // بازیابی کاربران (محدود به شعبه کاربر)
                var userBranchId = GetUserBranchId(userId);
                if (model.UsersInitial == null)
                {
                    var branchUsers = _BranchRipository.GetBranchUsersByBranchId(userBranchId, includeInactive: false);
                    model.UsersInitial = branchUsers.Select(u => new UserViewModelFull
                    {
                        Id = u.UserId,
                        FullNamesString = u.UserFullName,
                        IsActive = u.IsActive
                    }).ToList();
                }

                // بازیابی تیم‌ها (محدود به شعبه کاربر)
                model.TeamsInitial ??= await GetUserRelatedTeamsAsync(userId);

                // بازیابی طرف حساب‌ها (محدود به شعبه کاربر)
                if (model.StakeholdersInitial == null)
                {
                    var stakeholders = _StakeholderRepo.GetStakeholdersByBranchId(userBranchId);
                    model.StakeholdersInitial = stakeholders.Select(s => new StakeholderViewModel
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        CompanyName = s.CompanyName,
                        NationalCode = s.NationalCode,
                        IsActive = s.IsActive
                    }).ToList();
                }

                return model;
            }
            catch (Exception)
            {
                // در صورت خطا، حداقل لیست‌های خالی ایجاد کن
                model.branchListInitial ??= new List<BranchViewModel>();
                model.TaskCategoryInitial ??= new List<TaskCategory>();
                model.UsersInitial ??= new List<UserViewModelFull>();
                model.TeamsInitial ??= new List<TeamViewModel>();
                model.StakeholdersInitial ??= new List<StakeholderViewModel>();
                
                model.TaskCodeSettings ??= new TaskCodeSettings
                {
                    AllowManualInput = false,
                    SystemPrefix = "TSK"
                };

                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = "TSK-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                return model;
            }
        }

        /// <summary>
        /// آماده‌سازی مدل برای ایجاد تسک جدید (نسخه Async جدید) - بروزرسانی شده
        /// </summary>
        public async Task<TaskViewModel> PrepareCreateTaskModelAsync(string userId)
        {
            var model = new TaskViewModel
            {
                CreateDate = DateTime.Now,
                IsActive = true,
                TaskCode = _taskCodeGenerator.GenerateTaskCode(),
                TaskCodeSettings = new TaskCodeSettings
                {
                    AllowManualInput = true,
                    SystemPrefix = "TSK"
                }
            };

            // دریافت شعبه‌های کاربر
            var userBranches = _BranchRipository.GetBrnachListByUserId(userId);
            model.branchListInitial = userBranches;

            // ⭐⭐⭐ اگر فقط یک شعبه باشد، خودکار پر کن
            if (userBranches?.Count() == 1)
            {
                var singleBranch = userBranches.First();
                model.BranchIdSelected = singleBranch.Id;

                // بارگذاری کاربران شعبه با "خودم" در صدر
                var branchUsers = await GetBranchUsersWithCurrentUserFirstAsync(singleBranch.Id, userId);
                model.UsersInitial = branchUsers;

                // بارگذاری تیم‌ها
                model.TeamsInitial = await GetBranchTeamsWithManagersAsync(singleBranch.Id);

                // بارگذاری Contacts و Organizations
                model.ContactsInitial = await GetBranchContactsAsync(singleBranch.Id);
                model.OrganizationsInitial = await GetBranchOrganizationsAsync(singleBranch.Id);
            }

            return model;
        }

        /// <summary>
        /// ⭐⭐⭐ متد جدید: دریافت کاربران با "خودم" در صدر
        /// </summary>
        private async Task<List<UserViewModelFull>> GetBranchUsersWithCurrentUserFirstAsync(int branchId, string currentUserId)
        {
            var allUsers = _context.BranchUser_Tbl.Where(
                bu => bu.BranchId == branchId && bu.IsActive
            ).Include(bu => bu.User)
            .Where(bu => bu.User != null && bu.User.IsActive)
            .Select(bu => new UserViewModelFull
            {
                Id = bu.UserId,
                FirstName = bu.User.FirstName,
                LastName = bu.User.LastName,
                UserName = bu.User.UserName,
                Email = bu.User.Email,
                // ⭐ افزودن فیلد برای تصویر پروفایل
                ProfileImagePath = bu.User.ProfileImagePath ?? "/images/default-avatar.png"
            })
            .ToList();

            // ⭐⭐⭐ جدا کردن کاربر جاری
            var currentUser = allUsers.FirstOrDefault(u => u.Id == currentUserId);
            var otherUsers = allUsers.Where(u => u.Id != currentUserId).OrderBy(u => u.FirstName).ToList();

            // ⭐⭐⭐ ساخت لیست نهایی با "خودم" در صدر
            var result = new List<UserViewModelFull>();

            if (currentUser != null)
            {
                // ⭐ تغییر نمایش به "خودم"
                currentUser.FullNamesString = $"خودم ({currentUser.FirstName} {currentUser.LastName})";
                result.Add(currentUser);
            }

            result.AddRange(otherUsers);
            return result;
        }

        /// <summary>
        /// دریافت داده‌های شعبه برای AJAX - بروزرسانی شده
        /// </summary>
        public async Task<BranchSelectResponseViewModel> GetBranchTriggeredDataAsync(int branchId)
        {
            try
            {
                var response = new BranchSelectResponseViewModel
                {
                    // دریافت کاربران شعبه
                    Users = _BranchRipository.GetBranchUsersByBranchId(branchId, includeInactive: false),

                    // دریافت تیم‌های شعبه
                    Teams = await GetBranchTeamsByBranchId(branchId),

                    // ⭐⭐⭐ OLD - حفظ برای backward compatibility
                    Stakeholders = _StakeholderRepo.GetStakeholdersByBranchId(branchId),

                    // ⭐⭐⭐ NEW - سیستم جدید
                    Contacts = await GetBranchContactsAsync(branchId),
                    Organizations = await GetBranchOrganizationsAsync(branchId)
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetBranchTriggeredDataAsync: {ex.Message}");

                return new BranchSelectResponseViewModel
                {
                    Users = new List<BranchUserViewModel>(),
                    Teams = new List<TeamViewModel>(),
                    Stakeholders = new List<StakeholderViewModel>(),
                    Contacts = new List<ContactViewModel>(),
                    Organizations = new List<OrganizationViewModel>()
                };
            }
        }

        /// <summary>
        /// دریافت آمار پروژه
        /// </summary>
        public async Task<ProjectStatsViewModel> GetProjectStatsAsync(int branchId, int? stakeholderId = null, int? categoryId = null)
        {
            try
            {
                var query = _context.Tasks_Tbl.Where(t => !t.IsDeleted && t.BranchId == branchId);

                var stakeholderTasksCount = stakeholderId.HasValue ? 
                    await query.CountAsync(t => t.StakeholderId == stakeholderId.Value) : 0;

                var categoryTasksCount = categoryId.HasValue ? 
                    await query.CountAsync(t => t.TaskCategoryId == categoryId.Value) : 0;

                return new ProjectStatsViewModel
                {
                    StakeholderTasksCount = stakeholderTasksCount,
                    CategoryTasksCount = categoryTasksCount
                };
            }
            catch (Exception ex)
            {
                return new ProjectStatsViewModel 
                { 
                    StakeholderTasksCount = 0, 
                    CategoryTasksCount = 0 
                };
            }
        }

        /// <summary>
        /// تکمیل داده‌های مدل ایجاد تسک - بروزرسانی شده
        /// </summary>
        private async Task PopulateCreateTaskDataAsync(TaskViewModel model, string userId)
        {
            try
            {
                // بازیابی لیست شعبه‌ها
                model.branchListInitial ??= _BranchRipository.GetBrnachListByUserId(userId);

                // تنظیمات کد تسک از appsettings.json
                model.TaskCodeSettings ??= _taskCodeGenerator.GetTaskCodeSettings();

                // تولید کد تسک اتوماتیک اگر خالی باشد
                if (string.IsNullOrEmpty(model.TaskCode))
                {
                    model.TaskCode = _taskCodeGenerator.GenerateTaskCode();
                }

                // مقداردهی پیش‌فرض لیست‌ها
                model.TaskCategoryInitial ??= GetAllCategories();
                model.UsersInitial ??= new List<UserViewModelFull>();
                model.TeamsInitial ??= await GetUserRelatedTeamsAsync(userId);

                // ⭐⭐⭐ OLD - نگهداری برای backward compatibility
                model.StakeholdersInitial ??= new List<StakeholderViewModel>();

                // ⭐⭐⭐ NEW - مقداردهی لیست‌های جدید
                model.ContactsInitial ??= new List<ContactViewModel>();
                model.OrganizationsInitial ??= new List<OrganizationViewModel>();
                model.ContactOrganizations ??= new List<OrganizationViewModel>();

                // اگر شعبه‌ای وجود دارد، داده‌های مربوطه را بارگذاری کن
                if (model.branchListInitial?.Any() == true)
                {
                    var firstBranchId = model.branchListInitial.First().Id;

                    // بارگذاری کاربران
                    var branchData = await GetBranchTriggeredDataAsync(firstBranchId);
                    model.UsersInitial = branchData.Users.Select(u => new UserViewModelFull
                    {
                        Id = u.UserId,
                        FullNamesString = u.UserFullName,
                        IsActive = u.IsActive
                    }).ToList();

                    // ⭐⭐⭐ OLD - بارگذاری Stakeholders
                    model.StakeholdersInitial = branchData.Stakeholders;

                    // ⭐⭐⭐ NEW - بارگذاری Contacts & Organizations
                    model.ContactsInitial = await GetBranchContactsAsync(firstBranchId);
                    model.OrganizationsInitial = await GetBranchOrganizationsAsync(firstBranchId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in PopulateCreateTaskDataAsync: {ex.Message}");

                // در صورت خطا لیست‌های خالی
                InitializeEmptyCreateTaskLists(model);
            }
        }

        /// <summary>
        /// مقداردهی اولیه لیست‌های خالی برای مدل ایجاد تسک - بروزرسانی شده
        /// </summary>
        private void InitializeEmptyCreateTaskLists(TaskViewModel model)
        {
            model.branchListInitial ??= new List<BranchViewModel>();
            model.TaskCategoryInitial ??= new List<TaskCategory>();
            model.UsersInitial ??= new List<UserViewModelFull>();
            model.TeamsInitial ??= new List<TeamViewModel>();

            // ⭐⭐⭐ OLD
            model.StakeholdersInitial ??= new List<StakeholderViewModel>();

            // ⭐⭐⭐ NEW
            model.ContactsInitial ??= new List<ContactViewModel>();
            model.OrganizationsInitial ??= new List<OrganizationViewModel>();
            model.ContactOrganizations ??= new List<OrganizationViewModel>();

            model.TaskCodeSettings ??= new TaskCodeSettings
            {
                AllowManualInput = false,
                SystemPrefix = "TSK"
            };

            if (string.IsNullOrEmpty(model.TaskCode))
            {
                model.TaskCode = "TSK-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        #endregion
    }
}
