using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
    public class BranchRepository : IBranchRepository
    {
        private readonly AppDbContext _context;
        private readonly IUserManagerRepository _userManagerRepository;

        public BranchRepository(AppDbContext context, IUserManagerRepository userManagerRepository)
        {
            _context = context;
            _userManagerRepository = userManagerRepository;
        }

        public List<Branch> GetBranches(bool includeInactive = false)
        {
            var query = _context.Branch_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(b => b.IsActive);

            return query.OrderBy(b => b.Name).ToList();
        }

        public BranchViewModel GetBranchById(int id, bool includeUsers = false, bool includeStakeholders = false, bool includeTasks = false, bool includeChildBranches = false)
        {
            IQueryable<Branch> query = _context.Branch_Tbl.AsQueryable();

            if (includeUsers)
                query = query.Include(b => b.BranchUsers)
                             .ThenInclude(bu => bu.User);

            if (includeStakeholders)
                query = query.Include(b => b.Stakeholders);

            if (includeTasks)
                query = query.Include(b => b.TaskList);

            if (includeChildBranches)
                query = query.Include(b => b.ChildBranches);

            // Include parent branch information
            query = query.Include(b => b.ParentBranch);

            var branch = query.FirstOrDefault(b => b.Id == id);

            if (branch == null)
                return null;

            // Map Branch entity to BranchViewModel
            BranchViewModel? branchViewModel = new BranchViewModel
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
                LastUpdateDate = branch.LastUpdateDate,
                ParentBranch = branch.ParentBranch != null ? new BranchViewModel
                {
                    Id = branch.ParentBranch.Id,
                    Name = branch.ParentBranch.Name,
                    IsMainBranch = branch.ParentBranch.IsMainBranch,
                    IsActive = branch.ParentBranch.IsActive
                } : null
            };

            return branchViewModel;
        }

        public List<Branch> GetMainBranches(bool includeInactive = false)
        {
            var query = _context.Branch_Tbl.Where(b => b.IsMainBranch);

            if (!includeInactive)
                query = query.Where(b => b.IsActive);

            return query.OrderBy(b => b.Name).ToList();
        }

        public List<Branch> GetChildBranches(int parentId, bool includeInactive = false)
        {
            var query = _context.Branch_Tbl.Where(b => b.ParentId == parentId);

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

        public List<Branch> SearchBranches(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetBranches();

            var query = _context.Branch_Tbl
                .Where(b => b.IsActive &&
                           (b.Name.Contains(searchTerm) ||
                            b.ManagerName.Contains(searchTerm) ||
                            b.Phone.Contains(searchTerm) ||
                            b.Email.Contains(searchTerm)));

            return query.OrderBy(b => b.Name).ToList();
        }

        public List<Stakeholder> GetBranchStakeholders(int branchId, bool includeInactive = false)
        {
            var stakeholderIds = _context.StakeholderBranch_Tbl
                .Where(sb => sb.BranchId == branchId && sb.IsActive)
                .Select(sb => sb.StakeholderId);

            var query = _context.Stakeholder_Tbl
                .Where(s => stakeholderIds.Contains(s.Id) && !s.IsDeleted);

            if (!includeInactive)
                query = query.Where(s => s.IsActive);

            return query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();
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
                
                // اگر کاربر ادمین نیست و دسترسی به شعبه ندارد
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

            // دریافت طرف حساب‌های شعبه
            var stakeholderIds = _context.StakeholderBranch_Tbl
                .Where(sb => sb.BranchId == branchId && sb.IsActive)
                .Select(sb => sb.StakeholderId);

            var stakeholdersQuery = _context.Stakeholder_Tbl
                .Where(s => stakeholderIds.Contains(s.Id) && !s.IsDeleted);

            if (!includeInactiveStakeholders)
                stakeholdersQuery = stakeholdersQuery.Where(s => s.IsActive);

            result.Stakeholders = stakeholdersQuery
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();

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
    }
}