using AutoMapper;
using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.CrmArea.Controllers.CRMControllers
{
    /// <summary>
    /// کنترلر اصلی CRM - فقط Constructor و Fields
    /// متدهای CRUD، Ajax و Helpers در فایل‌های Partial جداگانه قرار دارند
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM.VIEW")]
    public partial class CRMController : BaseController
    {
        #region Fields

        protected readonly IUnitOfWork _uow;
        protected readonly ICRMRepository _crmRepository;
        protected readonly IStakeholderRepository _stakeholderRepository;
        protected new readonly UserManager<AppUsers> _userManager;
        protected readonly IMapper _mapper;
        protected readonly IWebHostEnvironment _webHostEnvironment;
        protected readonly IUserManagerRepository _userRepository;

        #endregion

        #region Constructor

        public CRMController(
            IUnitOfWork uow,
            ICRMRepository crmRepository,
            IStakeholderRepository stakeholderRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            IWebHostEnvironment webHostEnvironment,
            ActivityLoggerService activityLogger,
            IBaseRepository BaseRepository,
            IUserManagerRepository userRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, BaseRepository, moduleTracking, moduleAccessService)
        {
            _uow = uow;
            _crmRepository = crmRepository;
            _stakeholderRepository = stakeholderRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
        }

        #endregion
    }
}