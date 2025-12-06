using AutoMapper; // اضافه کردن فضای نام AutoMapper
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.TaskManagement; // اضافه شده
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.AcControl; // اضافه شده
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels; // اضافه شده برای TaskCalendarViewModel
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository
{
    public class BranchRepository : IBranchRepository
    {
        private readonly AppDbContext _context;
        private readonly IUserManagerRepository _userManagerRepository;
        private readonly IMapper _mapper; // اضافه کردن AutoMapper

        public BranchRepository(AppDbContext context, IUserManagerRepository userManagerRepository, IMapper mapper)
        {
            _context = context;
            _userManagerRepository = userManagerRepository;
            _mapper = mapper; // تزریق AutoMapper
        }

        public List<Branch> GetBranches(bool includeInactive = false)
        {
            var query = _context.Branch_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(b => b.IsActive);

            return query.OrderBy(b => b.Name).ToList();
        }

      


        public List<BranchUser> GetBranchUsers(int branchId, bool includeInactive = false)
        {
            var query = _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Where(bu => bu.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(bu => bu.IsActive);

            return query.OrderBy(bu => bu.User.LastName).ThenBy(bu => bu.User.FirstName).ToList();
        }

        public BranchUser GetBranchUserById(int id)
        {
            return _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Include(bu => bu.Branch)
                .FirstOrDefault(bu => bu.Id == id);
        }

        public bool IsBranchNameUnique(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true;

            var query = _context.Branch_Tbl.Where(b => b.Name == name);

            if (excludeId.HasValue)
                query = query.Where(b => b.Id != excludeId.Value);

            return !query.Any();
        }

      


        /// <summary>
        /// لیستی از شعبه‌هایی که آن کاربر در آن تعریف شده است 
        /// </summary>
        /// <param name="UserLoginingid">نام کاربر لاگین شده را می‌گیرد و در خروجی شعبه‌هایی که مجوز اتصال دارد را می‌دهد</param>
        /// <returns></returns>
        public List<BranchViewModel> GetBrnachListByUserId(string UserLoginingid)
        {
            //این باید درست بشه 
            List<BranchViewModel> branchList = new List<BranchViewModel>();
            bool IsAdmin = _userManagerRepository.GetUserInfoData(UserLoginingid) != null;
            IsAdmin = true;
            if (IsAdmin)
            {
                branchList = (from branchUser in _context.BranchUser_Tbl
                              join bu in _context.Branch_Tbl on branchUser.BranchId equals bu.Id
                              select new BranchViewModel
                              {
                                  Id = bu.Id,
                                  Name = bu.Name,
                                  IsMainBranch = bu.IsMainBranch,
                                  IsActive = branchUser.IsActive
                              }).ToList();
            }
            else
            {
            
            }

               branchList = (from branchUser in _context.BranchUser_Tbl
                                                    join bu in _context.Branch_Tbl on branchUser.BranchId equals bu.Id
                                                    where branchUser.UserId == UserLoginingid && branchUser.IsActive
                                                    select new BranchViewModel
                                                    {
                                                        Id = bu.Id,
                                                        Name = bu.Name,
                                                        IsMainBranch = bu.IsMainBranch,
                                                        IsActive = branchUser.IsActive
                                                    }).ToList();
            return branchList;
        }

        /// <summary>
        /// بررسی اینکه آیا کاربر مشخص قبلاً به شعبه اختصاص داده شده است
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>true اگر کاربر قبلاً اختصاص داده شده باشد</returns>
        public bool IsUserAssignedToBranch(string userId, int branchId)
        {
            return _context.BranchUser_Tbl
                .Any(bu => bu.UserId == userId && bu.BranchId == branchId && bu.IsActive);
        }

        /// <summary>
        /// دریافت تعداد کاربران فعال یک شعبه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>تعداد کاربران active</returns>
        public int GetActiveUsersCountByBranch(int branchId)
        {
            return _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Count(bu => bu.BranchId == branchId && 
                            bu.IsActive && 
                            bu.User.IsActive && 
                            !bu.User.IsRemoveUser);
        }

        /// <summary>
        /// دریافت جزئیات کامل یک شعبه شامل کاربران، طرف حساب‌ها و شعبه‌های زیرمجموعه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="userId">شناسه کاربر جهت بررسی دسترسی</param>
        /// <param name="includeInactiveUsers">شامل کاربران غیرفعال</param>
        /// <param name="includeInactiveStakeholders">شامل طرف حساب‌های غیرفعال</param>
        /// <param name="includeInactiveChildBranches">شامل شعبه‌های زیرمجموعه غیرفعال</param>
        /// <returns>جزئیات کامل شعبه</returns>
        public BranchDetailsViewModel GetBranchDetailsById(int branchId, string userId = null,
            bool includeInactiveUsers = false, bool includeInactiveStakeholders = false,
            bool includeInactiveChildBranches = false)
        {
            // دریافت شعبه اصلی با شعبه مادر
            var branch = _context.Branch_Tbl
                .Include(b => b.ParentBranch)
                .FirstOrDefault(b => b.Id == branchId);

            if (branch == null)
                return null;

            // بررسی دسترسی کاربر به شعبه (در صورت ارسال userId)
            if (!string.IsNullOrEmpty(userId))
            {
                var userHasAccess = _context.BranchUser_Tbl
                    .Any(bu => bu.BranchId == branchId && bu.UserId == userId && bu.IsActive);
                
                bool isAdmin = _userManagerRepository.GetUserInfoData(userId) != null;
                if (!isAdmin && !userHasAccess)
                    return null;
            }

            var result = new BranchDetailsViewModel
            {
                Id = branch.Id,
                Name = branch.Name,
                Description = branch.Description,
                Address = branch.Address,
                Phone = branch.Phone,
                Email = branch.Email,
                ManagerName = branch.ManagerName,
                IsActive = branch.IsActive,
                IsMainBranch = branch.IsMainBranch,
                ParentId = branch.ParentId,
                CreateDate = branch.CreateDate,
                LastUpdateDate = branch.LastUpdateDate
            };

            // اطلاعات شعبه مادر
            if (branch.ParentBranch != null)
            {
                result.ParentBranch = new BranchViewModel
                {
                    Id = branch.ParentBranch.Id,
                    Name = branch.ParentBranch.Name,
                    IsMainBranch = branch.ParentBranch.IsMainBranch,
                    IsActive = branch.ParentBranch.IsActive
                };
            }

            // دریافت کاربران شعبه
            var usersQuery = _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Include(bu => bu.AssignedByUser)
                .Where(bu => bu.BranchId == branchId);

            if (!includeInactiveUsers)
                usersQuery = usersQuery.Where(bu => bu.IsActive && bu.User.IsActive && !bu.User.IsRemoveUser);

            result.BranchUsers = usersQuery
                .OrderBy(bu => bu.User.LastName)
                .ThenBy(bu => bu.User.FirstName)
                .ToList();

            // ✅ NEW: دریافت افراد شعبه
            result.BranchContacts = GetBranchContacts(branchId, includeInactive: includeInactiveStakeholders);

            // ✅ NEW: دریافت سازمان‌های شعبه
            result.BranchOrganizations = GetBranchOrganizations(branchId, includeInactive: includeInactiveStakeholders);

            // ❌ DEPRECATED: نگه می‌داریم برای سازگاری
            var stakeholderBranchesQuery = _context.StakeholderBranch_Tbl
                .Include(sb => sb.Stakeholder)
                .Include(sb => sb.AssignedBy)
                .Where(sb => sb.BranchId == branchId && sb.IsActive && !sb.Stakeholder.IsDeleted);

            if (!includeInactiveStakeholders)
                stakeholderBranchesQuery = stakeholderBranchesQuery.Where(sb => sb.Stakeholder.IsActive);

            result.BranchStakeholders = stakeholderBranchesQuery
                .OrderBy(sb => sb.Stakeholder.LastName)
                .ThenBy(sb => sb.Stakeholder.FirstName)
                .ToList();

            result.Stakeholders = result.BranchStakeholders
                .Select(sb => sb.Stakeholder)
                .ToList();

            // محاسبه آمار جدید
            var stats = GetBranchContactStatistics(branchId);
            result.DirectContactsCount = stats.TotalContacts;
            result.OrganizationsCount = stats.TotalOrganizations;
            result.TotalVisibleMembersCount = stats.TotalVisibleMembers;

            // آمار قدیمی
            result.ActiveStakeholdersCount = result.Stakeholders.Count(s => s.IsActive);

            // دریافت شعبه‌های زیرمجموعه
            var childBranchesQuery = _context.Branch_Tbl
                .Where(b => b.ParentId == branchId);

            if (!includeInactiveChildBranches)
                childBranchesQuery = childBranchesQuery.Where(b => b.IsActive);

            result.ChildBranches = childBranchesQuery
                .OrderBy(b => b.Name)
                .ToList();

            // محاسبه آمار
            result.ActiveUsersCount = result.BranchUsers.Count(bu => bu.IsActive && bu.User.IsActive && !bu.User.IsRemoveUser);
            result.ActiveStakeholdersCount = result.Stakeholders.Count(s => s.IsActive);
            result.ChildBranchesCount = result.ChildBranches.Count;
            result.ActiveChildBranchesCount = result.ChildBranches.Count(cb => cb.IsActive);

            return result;
        }

        /// <summary>
        /// دریافت اطلاعات کامل برای فرم افزودن کاربر به شعبه
        /// شامل اطلاعات شعبه و لیست کاربران قابل انتساب
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>ViewModel کامل برای فرم افزودن کاربر</returns>
        public BranchUserViewModel GetAddUserToBranchViewModel(int branchId)
        {
            // دریافت اطلاعات شعبه
            var branch = _context.Branch_Tbl.FirstOrDefault(b => b.Id == branchId);
            if (branch == null)
                return null;

            // دریافت شناسه‌های کاربرانی که قبلاً به این شعبه اختصاص داده شده‌اند
            var existingUserIds = _context.BranchUser_Tbl
                .Where(bu => bu.BranchId == branchId && bu.IsActive)
                .Select(bu => bu.UserId)
                .ToList();

            // دریافت لیست کاربران فعال که در این شعبه نیستند
            var availableUsers = (from user in _context.Users
                                where user.IsActive && 
                                      !user.IsRemoveUser && 
                                      !existingUserIds.Contains(user.Id)
                                select new UserViewModelFull
                                {
                                    Id = user.Id,
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    FullNamesString = user.FirstName + " " + user.LastName,
                                    UserName = user.UserName,
                                    Email = user.Email,
                                    PhoneNumber = user.PhoneNumber,
                                    IsActive = user.IsActive
                                }).OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();

            // ایجاد ViewModel کامل
            var viewModel = new BranchUserViewModel
            {
                BranchId = branchId,
                BranchName = branch.Name,
                IsActive = true,
                Role = 0, // کارشناس به‌عنوان پیش‌فرض
                AssignDate = DateTime.Now,
                UsersIntial = availableUsers,
                UsersSelected = new List<string>()
            };

            return viewModel;
        }
        public List<BranchUserViewModel> GetBranchUsersByBranchId(int branchId, bool includeInactive = false)
        {
            var query = from bu in _context.BranchUser_Tbl
                        join b in _context.Branch_Tbl on bu.BranchId equals b.Id
                        join u in _context.Users on bu.UserId equals u.Id
                        where bu.BranchId == branchId
                        select new { BranchUser = bu, Branch = b, User = u };

            if (!includeInactive)
                query = query.Where(x => x.BranchUser.IsActive && x.Branch.IsActive && x.User.IsActive && !x.User.IsRemoveUser);

            return query.Select(x => new BranchUserViewModel
            {
                Id = x.BranchUser.Id,
                BranchId = x.BranchUser.BranchId,
                UserId = x.BranchUser.UserId,
                Role = x.BranchUser.Role,
                IsActive = x.BranchUser.IsActive,
                AssignDate = x.BranchUser.AssignDate,
                AssignedByUserId = x.BranchUser.AssignedByUserId,
                BranchName = x.Branch.Name,
                UserFullName = x.User.FirstName + " " + x.User.LastName
            }).OrderBy(x => x.UserFullName).ToList();
        }

        #region متدهای مدیریت دسته‌بندی تسک شعبه با طرف حساب

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های تسک متصل به شعبه و طرف حساب مشخص
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="stakeholderId">شناسه طرف حساب</param>
        /// <param name="activeOnly">فقط موارد فعال</param>
        /// <returns>لیست دسته‌بندی‌های تسک شعبه و طرف حساب</returns>
        public List<BranchTaskCategoryStakeholder> GetTaskCategoriesByBranchAndStakeholder(int branchId, int? stakeholderId = null, bool activeOnly = true)
        {
            var query = _context.BranchTaskCategoryStakeholder_Tbl
                .Include(btcs => btcs.Branch)
                .Include(btcs => btcs.TaskCategory)
                .Include(btcs => btcs.Stakeholder)
                .Include(btcs => btcs.AssignedByUser)
                .Where(btcs => btcs.BranchId == branchId);

            if (stakeholderId.HasValue)
                query = query.Where(btcs => btcs.StakeholderId == stakeholderId.Value);

            if (activeOnly)
                query = query.Where(btcs => btcs.IsActive);

            return query.OrderBy(btcs => btcs.TaskCategory.Title).ToList();
        }

        /// <summary>
        /// دریافت اطلاعات کامل انتساب دسته‌بندی به شعبه و طرف حساب
        /// </summary>
        /// <param name="id">شناسه انتساب</param>
        /// <returns>اطلاعات انتساب</returns>
        public BranchTaskCategoryStakeholder GetBranchTaskCategoryStakeholderById(int id)
        {
            return _context.BranchTaskCategoryStakeholder_Tbl
                .Include(btcs => btcs.Branch)
                .Include(btcs => btcs.TaskCategory)
                .Include(btcs => btcs.Stakeholder)
                .Include(btcs => btcs.AssignedByUser)
                .FirstOrDefault(btcs => btcs.Id == id);
        }

        /// <summary>
        /// دریافت ViewModel برای افزودن دسته‌بندی به شعبه با طرف حساب
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="stakeholderId">شناسه طرف حساب</param>
        /// <returns>ViewModel شامل اطلاعات لازم</returns>
        public BranchTaskCategoryStakeholderViewModel GetAddTaskCategoryToBranchStakeholderViewModel(int branchId, int? stakeholderId = null)
        {
            try
            {
                // دریافت اطلاعات شعبه
                var branch = _context.Branch_Tbl.FirstOrDefault(b => b.Id == branchId);
                if (branch == null)
                    return null;

                // دریافت شناسه‌های دسته‌بندی‌هایی که قبلاً به این شعبه و طرف حساب اختصاص داده شده‌اند
                var existingCategoryIds = _context.BranchTaskCategoryStakeholder_Tbl
                    .Where(btcs => btcs.BranchId == branchId && 
                                  (!stakeholderId.HasValue || btcs.StakeholderId == stakeholderId.Value) && 
                                  btcs.IsActive)
                    .Select(btcs => btcs.TaskCategoryId)
                    .ToList();

                // دریافت لیست دسته‌بندی‌های فعال که هنوز اختصاص داده نشده‌اند
                var availableCategories = _context.TaskCategory_Tbl
                    .Where(tc => tc.IsActive && !existingCategoryIds.Contains(tc.Id))
                    .Select(tc => new TaskCategoryItemViewModel
                    {
                        Id = tc.Id,
                        Title = tc.Title,
                        Description = tc.Description,
                        IsActive = tc.IsActive
                    })
                    .OrderBy(tc => tc.Title)
                    .ToList();

                // دریافت لیست طرف حساب‌های مرتبط با شعبه
                var stakeholderIds = _context.StakeholderBranch_Tbl
                    .Where(sb => sb.BranchId == branchId && sb.IsActive)
                    .Select(sb => sb.StakeholderId);

                var availableStakeholders = _context.Stakeholder_Tbl
                    .Where(s => stakeholderIds.Contains(s.Id) && s.IsActive && !s.IsDeleted)
                    .Select(s => new StakeholderItemViewModel
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        CompanyName = s.CompanyName,
                        StakeholderType = s.StakeholderType,
                        IsActive = s.IsActive
                    })
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .ToList();

                // ایجاد ViewModel
                var viewModel = new BranchTaskCategoryStakeholderViewModel
                {
                    BranchId = branchId,
                    BranchName = branch.Name,
                    StakeholderId = stakeholderId ?? 0,
                    IsActive = true,
                    AssignDate = DateTime.Now,
                    TaskCategoryInitial = availableCategories,
                    StakeholdersInitial = availableStakeholders,
                    TaskCategoriesSelected = new List<int>()
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                // لاگ خطا
                throw new Exception($"خطا در دریافت اطلاعات فرم افزودن دسته‌بندی به شعبه {branchId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// بررسی اینکه آیا دسته‌بندی قبلاً به شعبه و طرف حساب اضافه شده یا نه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="taskCategoryId">شناسه دسته‌بندی</param>
        /// <param name="stakeholderId">شناسه طرف حساب</param>
        /// <returns>true اگر قبلاً اضافه شده باشد</returns>
        public bool IsTaskCategoryAssignedToBranchStakeholder(int branchId, int taskCategoryId, int stakeholderId)
        {
            return _context.BranchTaskCategoryStakeholder_Tbl
                .Any(btcs => btcs.BranchId == branchId && 
                            btcs.TaskCategoryId == taskCategoryId && 
                            btcs.StakeholderId == stakeholderId && 
                            btcs.IsActive);
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های تسک برای شعبه مشخص (برای استفاده در فرم‌های دیگر)
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="stakeholderId">شناسه طرف حساب (اختیاری)</param>
        /// <returns>لیست دسته‌بندی‌های تسک شعبه</returns>
        public List<TaskCategoryItemViewModel> GetTaskCategoriesForBranchStakeholder(int branchId, int? stakeholderId = null)
        {
            try
            {
                var query = from btcs in _context.BranchTaskCategoryStakeholder_Tbl
                           join tc in _context.TaskCategory_Tbl on btcs.TaskCategoryId equals tc.Id
                           where btcs.BranchId == branchId && btcs.IsActive && tc.IsActive
                           select new { btcs, tc };

                if (stakeholderId.HasValue)
                    query = query.Where(x => x.btcs.StakeholderId == stakeholderId.Value);

                var taskCategories = query.Select(x => x.tc)
                       .Distinct()
                       .OrderBy(tc => tc.Title)
                       .ToList();
            
                // استفاده از AutoMapper برای تبدیل به TaskCategoryItemViewModel
                return _mapper.Map<List<TaskCategoryItemViewModel>>(taskCategories);
            }
            catch (Exception ex)
            {
                // لاگ خطا
                throw new Exception($"خطا در دریافت دسته‌بندی‌های شعبه {branchId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت دسته‌بندی‌های تسک بر اساس شعبه و طرف حساب انتخاب شده (برای cascade)
        /// این متد زمانی فراخوانی می‌شود که طرف حساب تغییر کند
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <param name="stakeholderId">شناسه طرف حساب</param>
        /// <returns>لیست دسته‌بندی‌های تسک قابل انتخاب</returns>
        public List<TaskCategoryItemViewModel> GetTaskCategoriesForStakeholderChange(int branchId, int stakeholderId)
        {
            try
            {
                // دریافت دسته‌بندی‌هایی که برای این شعبه و طرف حساب تعریف شده‌اند
                var assignedCategoryIds = _context.BranchTaskCategoryStakeholder_Tbl
                    .Where(btcs => btcs.BranchId == branchId && 
                                  btcs.StakeholderId == stakeholderId && 
                                  btcs.IsActive)
                    .Select(btcs => btcs.TaskCategoryId)
                    .ToList();

                // دریافت اطلاعات کامل دسته‌بندی‌ها
                var taskCategories = _context.TaskCategory_Tbl
                    .Where(tc => assignedCategoryIds.Contains(tc.Id) && tc.IsActive)
                    .OrderBy(tc => tc.Title)
                    .ToList();

                // استفاده از AutoMapper برای تبدیل به TaskCategoryItemViewModel
                return _mapper.Map<List<TaskCategoryItemViewModel>>(taskCategories);
            }
            catch (Exception ex)
            {
                // لاگ خطا
                throw new Exception($"خطا در دریافت دسته‌بندی‌های طرف حساب {stakeholderId} در شعبه {branchId}: {ex.Message}", ex);
            }
        }

        #endregion

        public List<StakeholderBranch> GetBranchStakeholders(int branchId)
        {
            return _context.StakeholderBranch_Tbl
                .Include(sb => sb.Stakeholder)
                .Where(sb => sb.BranchId == branchId && sb.IsActive)
                .OrderByDescending(sb => sb.AssignDate)
                .ToList();
        }

        /// <summary>
        /// دریافت شعبه بر اساس شناسه
        /// </summary>
        /// <param name="branchId">شناسه شعبه</param>
        /// <returns>اطلاعات شعبه</returns>
        public Branch GetBranchById(int branchId)
        {
            return _context.Branch_Tbl
                .AsNoTracking()
                .FirstOrDefault(b => b.Id == branchId);
        }

        // در کلاس BranchRepository اضافه کنید:

        /// <summary>
        /// دریافت افراد قابل افزودن به شعبه
        /// (افرادی که هنوز به این شعبه اختصاص نیافته‌اند)
        /// </summary>
        public List<Contact> GetAvailableContactsForBranch(int branchId)
        {
            // دریافت IDs افرادی که قبلاً به این شعبه اختصاص یافته‌اند
            var assignedContactIds = _context.BranchContact_Tbl
                .Where(bc => bc.BranchId == branchId && bc.IsActive)
                .Select(bc => bc.ContactId)
                .ToList();

            // دریافت افراد فعال که به این شعبه اختصاص نیافته‌اند
            var availableContacts = _context.Contact_Tbl
                .Where(c => c.IsActive && !assignedContactIds.Contains(c.Id))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToList();

            return availableContacts;
        }

        /// <summary>
        /// دریافت سازمان‌های قابل افزودن به شعبه
        /// (سازمان‌هایی که هنوز به این شعبه اختصاص نیافته‌اند)
        /// </summary>
        public List<Organization> GetAvailableOrganizationsForBranch(int branchId)
        {
            // دریافت IDs سازمان‌هایی که قبلاً به این شعبه اختصاص یافته‌اند
            var assignedOrganizationIds = _context.BranchOrganization_Tbl
                .Where(bo => bo.BranchId == branchId && bo.IsActive)
                .Select(bo => bo.OrganizationId)
                .ToList();

            // دریافت سازمان‌های فعال که به این شعبه اختصاص نیافته‌اند
            var availableOrganizations = _context.Organization_Tbl
                .Where(o => o.IsActive && !assignedOrganizationIds.Contains(o.Id))
                .OrderBy(o => o.Name)
                .ToList();

            return availableOrganizations;
        }

        /// <summary>
        /// بررسی اینکه آیا فرد قبلاً به شعبه اختصاص یافته است
        /// </summary>
        public bool IsContactAssignedToBranch(int branchId, int contactId)
        {
            return _context.BranchContact_Tbl
                .Any(bc => bc.BranchId == branchId &&
                           bc.ContactId == contactId &&
                           bc.IsActive);
        }

        /// <summary>
        /// بررسی اینکه آیا سازمان قبلاً به شعبه اختصاص یافته است
        /// </summary>
        public bool IsOrganizationAssignedToBranch(int branchId, int organizationId)
        {
            return _context.BranchOrganization_Tbl
                .Any(bo => bo.BranchId == branchId &&
                           bo.OrganizationId == organizationId &&
                           bo.IsActive);
        }

        /// <summary>
        /// دریافت BranchContact با شناسه
        /// </summary>
        public BranchContact GetBranchContactById(int id)
        {
            return _context.BranchContact_Tbl
                .Include(bc => bc.Branch)
                .Include(bc => bc.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                .FirstOrDefault(bc => bc.Id == id);
        }

        /// <summary>
        /// دریافت BranchOrganization با شناسه
        /// </summary>
        public BranchOrganization GetBranchOrganizationById(int id)
        {
            return _context.BranchOrganization_Tbl
                .Include(bo => bo.Branch)
                .Include(bo => bo.Organization)
                .FirstOrDefault(bo => bo.Id == id);
        }

        /// <summary>
        /// دریافت لیست BranchContacts یک شعبه
        /// </summary>
        public List<BranchContact> GetBranchContacts(int branchId)
        {
            return _context.BranchContact_Tbl
                .Include(bc => bc.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                .Where(bc => bc.BranchId == branchId && bc.IsActive)
                .OrderBy(bc => bc.Contact.LastName)
                .ThenBy(bc => bc.Contact.FirstName)
                .ToList();
        }

        /// <summary>
        /// دریافت لیست BranchOrganizations یک شعبه
        /// </summary>
        public List<BranchOrganization> GetBranchOrganizations(int branchId)
        {
            return _context.BranchOrganization_Tbl
                .Include(bo => bo.Organization)
                .Where(bo => bo.BranchId == branchId && bo.IsActive)
                .OrderBy(bo => bo.Organization.Name)
                .ToList();
        }


        #region BranchContact Methods

        /// <summary>
        /// دریافت لیست افراد مرتبط با شعبه
        /// </summary>
        public List<BranchContact> GetBranchContacts(int branchId, bool includeInactive = false)
        {
            var query = _context.BranchContact_Tbl
                .Include(bc => bc.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                .Include(bc => bc.AssignedBy)
                .Where(bc => bc.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(bc => bc.IsActive && bc.Contact.IsActive);

            return query
                .OrderBy(bc => bc.Contact.FirstName)
                .ThenBy(bc => bc.Contact.LastName)
                .ToList();
        }

      

        #endregion

        #region BranchOrganization Methods

        /// <summary>
        /// دریافت لیست سازمان‌های مرتبط با شعبه
        /// </summary>
        public List<BranchOrganization> GetBranchOrganizations(int branchId, bool includeInactive = false)
        {
            var query = _context.BranchOrganization_Tbl
                .Include(bo => bo.Organization)
                    .ThenInclude(o => o.Departments)
                .Include(bo => bo.AssignedBy)
                .Where(bo => bo.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(bo => bo.IsActive && bo.Organization.IsActive);

            return query
                .OrderBy(bo => bo.Organization.Name)
                .ToList();
        }

        /// <summary>
        /// دریافت تمام افراد نمایان در شعبه (افراد مستقیم + اعضای سازمان‌ها)
        /// </summary>
        public List<MahERP.DataModelLayer.Entities.Contacts.Contact> GetAllVisibleContactsInBranch(int branchId)
        {
            var contacts = new List<MahERP.DataModelLayer.Entities.Contacts.Contact>();

            // 1. افراد مستقیم اضافه شده به شعبه
            var directContacts = _context.BranchContact_Tbl
                .Include(bc => bc.Contact)
                .Where(bc => bc.BranchId == branchId && bc.IsActive && bc.Contact.IsActive)
                .Select(bc => bc.Contact)
                .ToList();

            contacts.AddRange(directContacts);

            // 2. اعضای سازمان‌هایی که IncludeAllMembers = true است
            var orgIds = _context.BranchOrganization_Tbl
                .Where(bo => bo.BranchId == branchId && bo.IsActive && bo.IncludeAllMembers)
                .Select(bo => bo.OrganizationId)
                .ToList();

            var orgMemberContacts = _context.DepartmentMember_Tbl
                .Include(dm => dm.Contact)
                .Include(dm => dm.Department)
                .Where(dm => orgIds.Contains(dm.Department.OrganizationId) && 
                             dm.IsActive && 
                             dm.Contact.IsActive)
                .Select(dm => dm.Contact)
                .ToList();

            contacts.AddRange(orgMemberContacts);

            // حذف تکراری‌ها و مرتب‌سازی
            return contacts
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToList();
        }

        /// <summary>
        /// دریافت آمار افراد و سازمان‌های شعبه
        /// </summary>
        public (int TotalContacts, int TotalOrganizations, int TotalVisibleMembers) GetBranchContactStatistics(int branchId)
        {
            var totalContacts = _context.BranchContact_Tbl
                .Count(bc => bc.BranchId == branchId && bc.IsActive);

            var totalOrganizations = _context.BranchOrganization_Tbl
                .Count(bo => bo.BranchId == branchId && bo.IsActive);

            var totalVisibleMembers = GetAllVisibleContactsInBranch(branchId).Count;

            return (totalContacts, totalOrganizations, totalVisibleMembers);
        }

        /// <summary>
        /// دریافت لیست شناسه شعبه‌های کاربر (Async)
        /// </summary>
        public async Task<List<int>> GetUserBranchIdsAsync(string userId)
        {
            return await _context.BranchUser_Tbl
                .Where(bu => bu.UserId == userId && bu.IsActive)
                .Select(bu => bu.BranchId)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// دریافت لیست شناسه شعبه‌هایی که دسته‌بندی در آن‌ها تعریف شده است
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>لیست شناسه شعبه‌ها</returns>
        public async Task<List<int>> GetCategoryBranchIdsAsync(int categoryId)
        {
            // دریافت شعبه‌هایی که این دسته‌بندی در آن‌ها استفاده شده
            return await _context.BranchTaskCategoryStakeholder_Tbl
                .Where(btcs => btcs.TaskCategoryId == categoryId && btcs.IsActive)
                .Select(btcs => btcs.BranchId)
                .Distinct()
                .ToListAsync();
        }

        #endregion
    }
}