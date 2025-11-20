using AutoMapper;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    public partial class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;
        private readonly IBranchRepository _BranchRipository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStakeholderRepository _StakeholderRepo;
        private readonly IUserManagerRepository _userManagerRepository;
        private readonly TaskCodeGenerator _taskCodeGenerator;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IMapper _mapper;
        private readonly ITaskGroupingRepository _groupingRepository;

        public TaskRepository(
            AppDbContext context,
            IBranchRepository branchRipository,
            IUnitOfWork unitOfWork,
            IUserManagerRepository userManagerRepository,
            IStakeholderRepository stakeholderRepo,
            TaskCodeGenerator taskCodeGenerator,
            ITaskHistoryRepository taskHistoryRepository,
            IMapper mapper,
            ITaskGroupingRepository groupingRepository)
        {
            _context = context;
            _BranchRipository = branchRipository;
            _unitOfWork = unitOfWork;
            _userManagerRepository = userManagerRepository;
            _StakeholderRepo = stakeholderRepo;
            _taskCodeGenerator = taskCodeGenerator;
            _taskHistoryRepository = taskHistoryRepository;
            _mapper = mapper;
            _groupingRepository = groupingRepository;
        }


        /// <summary>
        /// دریافت همه انواع تسک‌های کاربر به تفکیک نوع - نسخه انتخابی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="includeCreatedTasks">شامل تسک‌های ایجاد شده توسط کاربر</param>
        /// <param name="includeAssignedTasks">شامل تسک‌های منتصب شده به کاربر</param>
        /// <param name="includeSupervisedTasks">شامل تسک‌های تحت نظارت کاربر</param>
        /// <param name="includeDeletedTasks">شامل تسک‌های حذف شده</param>
        /// <returns>ViewModel جامع حاوی انواع تسک‌های انتخاب شده</returns>
        public async Task<UserTasksComprehensiveViewModel> GetUserTasksComprehensiveAsync(
            string userId,
            bool includeCreatedTasks = true,
            bool includeAssignedTasks = true,
            bool includeSupervisedTasks = false,
            bool includeDeletedTasks = false)
        {
            try
            {
                var result = new UserTasksComprehensiveViewModel();

                // 1. دریافت تسک‌های ایجاد شده توسط کاربر (اختیاری)
                if (includeCreatedTasks)
                {
                    var createdTasks = await GetTasksByUserWithPermissionsAsync(userId,
                        includeAssigned: false, includeCreated: true, includeDeleted: includeDeletedTasks);
                    result.CreatedTasks = createdTasks.Select(MapToTaskViewModel).ToList();
                }

                // 2. دریافت تسک‌های منتصب شده به کاربر (اختیاری)
                if (includeAssignedTasks)
                {
                    var assignedTasks = await GetTasksByUserWithPermissionsAsync(userId,
                        includeAssigned: true, includeCreated: false, includeDeleted: includeDeletedTasks);
                    var filteredAssignedTasks = assignedTasks.Where(t => t.CreatorUserId != userId).ToList();
                    result.AssignedTasks = filteredAssignedTasks.Select(MapToTaskViewModel).ToList();
                }

                // 3. دریافت تسک‌های تحت نظارت (اختیاری)
                if (includeSupervisedTasks)
                {
                    var supervisedTasks = await GetTasksByUserWithPermissionsAsync(userId,
                        includeAssigned: false, includeCreated: false, includeDeleted: includeDeletedTasks, includeSupervisedTasks: true);
                    result.SupervisedTasks = supervisedTasks.Select(MapToTaskViewModel).ToList();
                }

                // 4. دریافت تسک‌های حذف شده (اختیاری)
                if (includeDeletedTasks)
                {
                    var deletedTasks = await GetTasksByUserWithPermissionsAsync(userId,
                        includeAssigned: true, includeCreated: true, includeDeleted: true, includeSupervisedTasks: includeSupervisedTasks);
                    result.DeletedTasks = deletedTasks.Where(t => t.IsDeleted).Select(MapToTaskViewModel).ToList();
                }

                // 5. محاسبه آمار
                result.Stats = CalculateUserTasksStats(result);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserTasksComprehensiveAsync: {ex.Message}");
                return new UserTasksComprehensiveViewModel();
            }
        }
    }
}