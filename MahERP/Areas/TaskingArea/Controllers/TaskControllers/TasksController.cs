using AutoMapper;
using MahERP.Areas.AppCoreArea.Controllers.BaseControllers;
using MahERP.Attributes;

using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


namespace MahERP.Areas.TaskingArea.Controllers.TaskControllers
{
    /// <summary>
    /// کنترلر اصلی مدیریت تسک‌ها
    /// تقسیم شده به Partial Classes برای سازماندهی بهتر
    /// </summary>
    [Area("TaskingArea")]
    [Authorize]
    [PermissionRequired("TASK")]
    public partial class TasksController : BaseController
    {
        #region Fields

        private readonly ITaskRepository _taskRepository;
        private readonly IStakeholderRepository _stakeholderRepository;
        private readonly IBranchRepository _branchRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly TaskCodeGenerator _taskCodeGenerator;
        protected readonly IUserManagerRepository _userRepository;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory; // ⭐⭐⭐ اضافه شده

        #endregion

        #region Constructor

        public TasksController(
            IUnitOfWork uow,
            ITaskRepository taskRepository,
            IStakeholderRepository stakeholderRepository,
            IBranchRepository branchRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            IWebHostEnvironment webHostEnvironment,
            ActivityLoggerService activityLogger,
            TaskCodeGenerator taskCodeGenerator,
            IUserManagerRepository userRepository,
            IBaseRepository BaseRepository,
            ITaskHistoryRepository taskHistoryRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            IOrganizationRepository organizationRepository,
            ITeamRepository teamRepository,
            IServiceScopeFactory serviceScopeFactory) // ⭐⭐⭐ اضافه شده
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, 
                   userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _taskRepository = taskRepository;
            _stakeholderRepository = stakeholderRepository;
            _branchRepository = branchRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _taskCodeGenerator = taskCodeGenerator;
            _userRepository = userRepository;
            _taskHistoryRepository = taskHistoryRepository;
            _organizationRepository = organizationRepository;
            _teamRepository = teamRepository;
            _serviceScopeFactory = serviceScopeFactory; // ⭐⭐⭐ اضافه شده
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تعیین Content-Type بر اساس پسوند فایل
        /// </summary>
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// متد کمکی برای ذخیره موازی فایل‌ها
        /// </summary>
        private async Task<TaskCommentAttachment> SaveAttachmentAsync(
            IFormFile file,
            int commentId,
            string uploadsFolder,
            string currentUserId,
            int taskId)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return new TaskCommentAttachment
            {
                TaskCommentId = commentId,
                FileName = file.FileName,
                FilePath = $"/uploads/task-comments/{taskId}/{uniqueFileName}",
                FileExtension = Path.GetExtension(file.FileName),
                FileSize = file.Length.ToString(),
                FileUUID = uniqueFileName,
                UploadDate = DateTime.Now,
                UploaderUserId = currentUserId
            };
        }

        #endregion
    }
}