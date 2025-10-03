using AutoMapper;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.CRMViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.AdminArea.Controllers.CRMControllers
{
    [Area("AdminArea")]
    [Authorize]
    [PermissionRequired("CRM")]

    public class CRMController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly ICRMRepository _crmRepository;
        private readonly IStakeholderRepository _stakeholderRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        protected readonly IUserManagerRepository _userRepository;

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
            IUserManagerRepository userRepository) : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository)
        {
            _uow = uow;
            _crmRepository = crmRepository;
            _stakeholderRepository = stakeholderRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
        }

        // لیست تعاملات CRM
        [Permission("CRM", "Index", 0)] // Read
        public IActionResult Index()
        {
            var interactions = _crmRepository.GetCRMInteractions();
            var viewModels = _mapper.Map<List<CRMInteractionViewModel>>(interactions);
            
            ViewBag.TotalCount = _crmRepository.GetTotalCRMInteractionsCount();
            ViewBag.TodayCount = _crmRepository.GetTodayCRMInteractionsCount();
            ViewBag.PendingFollowUps = _crmRepository.GetPendingFollowUpsCount();
            
            return View(viewModels);
        }

        // نمایش تعاملات من
        public IActionResult MyInteractions()
        {
            var userId = _userManager.GetUserId(User);
            var interactions = _crmRepository.GetCRMInteractionsByUser(userId);
            var viewModels = _mapper.Map<List<CRMInteractionViewModel>>(interactions);
            
            ViewBag.Title = "تعاملات من";
            ViewBag.IsMyInteractions = true;
            
            return View("Index", viewModels);
        }

        // جزئیات تعامل CRM
        public IActionResult Details(int id)
        {
            var interaction = _crmRepository.GetCRMInteractionById(id, includeAttachments: true, includeComments: true, includeParticipants: true);
            if (interaction == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<CRMInteractionViewModel>(interaction);
            
            return View(viewModel);
        }

        // افزودن تعامل CRM جدید - نمایش فرم
        [HttpGet]
        [Permission("CRM", "Create", 1)] // Create  
        public IActionResult Create()
        {
            PopulateDropdowns();
            
            // ایجاد کد CRM جدید - یک عدد 8 رقمی تصادفی
            Random random = new Random();
            string crmCode;
            do
            {
                crmCode = "CRM" + random.Next(10000, 99999).ToString();
            } while (!_crmRepository.IsCRMCodeUnique(crmCode));
            
            return View(new CRMInteractionViewModel 
            { 
                IsActive = true,
                CRMCode = crmCode,
                CreateDate = DateTime.Now,
                BranchId = GetCurrentUserBranchId()
            });
        }

        // افزودن تعامل CRM جدید - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("CRM", "Create", 1)] // Create  
        public IActionResult Create(CRMInteractionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // بررسی یکتا بودن کد CRM
                if (!_crmRepository.IsCRMCodeUnique(model.CRMCode))
                {
                    ModelState.AddModelError("CRMCode", "کد تعامل وارد شده قبلاً ثبت شده است");
                    PopulateDropdowns();
                    return View(model);
                }

                // ایجاد تعامل CRM جدید
                var interaction = _mapper.Map<CRMInteraction>(model);
                interaction.CreateDate = DateTime.Now;
                interaction.CreatorUserId = _userManager.GetUserId(User);
                interaction.IsActive = true;
                interaction.IsDeleted = false;

                // ذخیره در دیتابیس
                _uow.CRMInteractionUW.Create(interaction);
                _uow.Save();
                
                // ذخیره فایل‌های پیوست
                if (model.UploadFiles != null && model.UploadFiles.Count > 0)
                {
                    SaveCRMAttachments(interaction.Id, model.UploadFiles);
                }

                return RedirectToAction(nameof(Index));
            }
            
            PopulateDropdowns();
            return View(model);
        }

        // ویرایش تعامل CRM - نمایش فرم
        [HttpGet]
        [Permission("CRM", "Edit", 2)] // Edit
        public IActionResult Edit(int id)
        {
            var interaction = _crmRepository.GetCRMInteractionById(id);
            if (interaction == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<CRMInteractionViewModel>(interaction);
            
            PopulateDropdowns();
            
            return View(viewModel);
        }

        // ویرایش تعامل CRM - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("CRM", "Edit", 2)] // Edit
        public IActionResult Edit(CRMInteractionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // بررسی یکتا بودن کد CRM
                if (!_crmRepository.IsCRMCodeUnique(model.CRMCode, model.Id))
                {
                    ModelState.AddModelError("CRMCode", "کد تعامل وارد شده قبلاً ثبت شده است");
                    PopulateDropdowns();
                    return View(model);
                }

                // دریافت تعامل از دیتابیس
                var interaction = _uow.CRMInteractionUW.GetById(model.Id);
                if (interaction == null)
                    return RedirectToAction("ErrorView", "Home");

                // به‌روزرسانی اطلاعات
                _mapper.Map(model, interaction);
                interaction.LastUpdateDate = DateTime.Now;
                interaction.LastUpdaterUserId = _userManager.GetUserId(User);
                
                _uow.CRMInteractionUW.Update(interaction);
                _uow.Save();
                
                // ذخیره فایل‌های پیوست جدید
                if (model.UploadFiles != null && model.UploadFiles.Count > 0)
                {
                    SaveCRMAttachments(interaction.Id, model.UploadFiles);
                }

                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            
            PopulateDropdowns();
            return View(model);
        }

        // فعال/غیرفعال کردن تعامل - نمایش مودال تأیید
        [HttpGet]
        public IActionResult ToggleActiveStatus(int id)
        {
            var interaction = _uow.CRMInteractionUW.GetById(id);
            if (interaction == null)
                return RedirectToAction("ErrorView", "Home");

            if (interaction.IsActive)
            {
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ModalTitle = "غیرفعال کردن تعامل";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            }
            else
            {
                ViewBag.themeclass = "bg-gd-lake";
                ViewBag.ModalTitle = "فعال کردن تعامل";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
            }

            return PartialView("_ToggleActiveStatus", _mapper.Map<CRMInteractionViewModel>(interaction));
        }

        // فعال/غیرفعال کردن تعامل - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActiveStatusPost(int id)
        {
            var interaction = _uow.CRMInteractionUW.GetById(id);
            if (interaction == null)
                return RedirectToAction("ErrorView", "Home");

            interaction.IsActive = !interaction.IsActive;
            interaction.LastUpdateDate = DateTime.Now;
            interaction.LastUpdaterUserId = _userManager.GetUserId(User);
            
            _uow.CRMInteractionUW.Update(interaction);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        // حذف تعامل - نمایش مودال تأیید
        [HttpGet]
        [Permission("CRM", "Delete", 3)] // Delete
        public IActionResult Delete(int id)
        {
            var interaction = _uow.CRMInteractionUW.GetById(id);
            if (interaction == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";
            ViewBag.ViewTitle = "حذف تعامل CRM";

            return PartialView("_DeleteCRMInteraction", _mapper.Map<CRMInteractionViewModel>(interaction));
        }

        // حذف تعامل - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("CRM", "Delete", 3)] // Delete
        public IActionResult DeletePost(int id)
        {
            var interaction = _uow.CRMInteractionUW.GetById(id);
            if (interaction == null)
                return RedirectToAction("ErrorView", "Home");

            interaction.IsDeleted = true;
            interaction.LastUpdateDate = DateTime.Now;
            interaction.LastUpdaterUserId = _userManager.GetUserId(User);
            
            _uow.CRMInteractionUW.Update(interaction);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        // افزودن نظر - نمایش مودال
        [HttpGet]
        public IActionResult AddComment(int crmInteractionId)
        {
            var interaction = _uow.CRMInteractionUW.GetById(crmInteractionId);
            if (interaction == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.CRMInteractionId = crmInteractionId;
            ViewBag.CRMTitle = interaction.Title;

            return PartialView("_AddComment", new CRMCommentViewModel
            {
                CRMInteractionId = crmInteractionId
            });
        }

        // افزودن نظر - پردازش مودال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(CRMCommentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var comment = _mapper.Map<CRMComment>(model);
                comment.CreateDate = DateTime.Now;
                comment.CreatorUserId = _userManager.GetUserId(User);

                _uow.CRMCommentUW.Create(comment);
                _uow.Save();

                return RedirectToAction(nameof(Details), new { id = model.CRMInteractionId });
            }

            var interaction = _uow.CRMInteractionUW.GetById(model.CRMInteractionId);
            ViewBag.CRMInteractionId = model.CRMInteractionId;
            ViewBag.CRMTitle = interaction.Title;

            return PartialView("_AddComment", model);
        }

        // افزودن شرکت کننده - نمایش مودال
        [HttpGet]
        public IActionResult AddParticipant(int crmInteractionId)
        {
            var interaction = _uow.CRMInteractionUW.GetById(crmInteractionId);
            if (interaction == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.CRMInteractionId = crmInteractionId;
            ViewBag.CRMTitle = interaction.Title;
            
            ViewBag.Users = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");

            if (interaction.StakeholderId.HasValue)
            {
                ViewBag.StakeholderContacts = new SelectList(_stakeholderRepository.GetStakeholderContacts(interaction.StakeholderId.Value)
                    .Select(c => new { Id = c.Id, FullName = c.FirstName + " " + c.LastName }),
                    "Id", "FullName");
            }

            return PartialView("_AddParticipant", new CRMParticipantViewModel
            {
                CRMInteractionId = crmInteractionId
            });
        }

        // افزودن شرکت کننده - پردازش مودال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddParticipant(CRMParticipantViewModel model)
        {
            if (ModelState.IsValid)
            {
                var participant = _mapper.Map<CRMParticipant>(model);

                _uow.CRMParticipantUW.Create(participant);
                _uow.Save();

                return RedirectToAction(nameof(Details), new { id = model.CRMInteractionId });
            }

            var interaction = _uow.CRMInteractionUW.GetById(model.CRMInteractionId);
            ViewBag.CRMInteractionId = model.CRMInteractionId;
            ViewBag.CRMTitle = interaction.Title;
            
            ViewBag.Users = new SelectList(_userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName }),
                "Id", "FullName");

            if (interaction.StakeholderId.HasValue)
            {
                ViewBag.StakeholderContacts = new SelectList(_stakeholderRepository.GetStakeholderContacts(interaction.StakeholderId.Value)
                    .Select(c => new { Id = c.Id, FullName = c.FirstName + " " + c.LastName }),
                    "Id", "FullName");
            }

            return PartialView("_AddParticipant", model);
        }

        // جستجوی پیشرفته - نمایش فرم
        [HttpGet]
        public IActionResult AdvancedSearch()
        {
            ViewBag.Stakeholders = new SelectList(_stakeholderRepository.GetStakeholders()
                .Select(s => new { Id = s.Id, FullName = $"{s.FirstName} {s.LastName}" }),
                "Id", "FullName");
                
            ViewBag.Branches = new SelectList(_uow.BranchUW.Get()
                .Select(b => new { Id = b.Id, Name = b.Name }),
                "Id", "Name");

            return PartialView("_AdvancedSearch", new CRMSearchViewModel());
        }

        // جستجوی پیشرفته - پردازش جستجو
        [HttpPost]
        public IActionResult Search(CRMSearchViewModel model)
        {
            var interactions = _crmRepository.SearchCRMInteractions(
                model.SearchTerm, 
                model.CRMType, 
                model.Direction,
                model.Result,
                model.StakeholderId);
                
            var viewModels = _mapper.Map<List<CRMInteractionViewModel>>(interactions);

            ViewBag.SearchModel = model;
            ViewBag.Title = "نتایج جستجو";

            return View("SearchResults", viewModels);
        }

        // دانلود پیوست
        public IActionResult DownloadAttachment(int id)
        {
            var attachment = _crmRepository.GetCRMAttachmentById(id);
            if (attachment == null)
                return NotFound();

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, attachment.FilePath.TrimStart('/'));
            
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", attachment.FileName);
        }

        // گزارش آماری
        public IActionResult Statistics()
        {
            ViewBag.TotalCount = _crmRepository.GetTotalCRMInteractionsCount();
            ViewBag.TodayCount = _crmRepository.GetTodayCRMInteractionsCount();
            ViewBag.PendingFollowUps = _crmRepository.GetPendingFollowUpsCount();
            ViewBag.InteractionsByType = _crmRepository.GetCRMInteractionsByType();
            ViewBag.InteractionsByResult = _crmRepository.GetCRMInteractionsByResult();

            return View();
        }

        // توابع کمکی
        
        private void PopulateDropdowns()
        {
            ViewBag.Stakeholders = new SelectList(_stakeholderRepository.GetStakeholders()
                .Select(s => new { Id = s.Id, FullName = $"{s.FirstName} {s.LastName}" }),
                "Id", "FullName");
                
            ViewBag.Branches = new SelectList(_uow.BranchUW.Get()
                .Select(b => new { Id = b.Id, Name = b.Name }),
                "Id", "Name");
                
            ViewBag.Contracts = new SelectList(_uow.ContractUW.Get()
                .Where(c => c.IsActive)
                .Select(c => new { Id = c.Id, Title = c.Title }),
                "Id", "Title");
        }
        
        private void SaveCRMAttachments(int crmInteractionId, List<IFormFile> files)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "crm", crmInteractionId.ToString());
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
                
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    
                    var attachment = new CRMAttachment
                    {
                        CRMInteractionId = crmInteractionId,
                        FileName = file.FileName,
                        FileSize = file.Length,
                        FilePath = $"/uploads/crm/{crmInteractionId}/{uniqueFileName}",
                        FileType = file.ContentType,
                        UploadDate = DateTime.Now,
                        UploaderUserId = _userManager.GetUserId(User)
                    };
                    
                    _uow.CRMAttachmentUW.Create(attachment);
                }
            }
            
            _uow.Save();
        }
        
        private int GetCurrentUserBranchId()
        {
            // پیاده‌سازی منطق دریافت شعبه کاربر جاری
            // فعلاً شعبه پیش‌فرض برگردانده می‌شود
            return _uow.BranchUW.Get().FirstOrDefault()?.Id ?? 1;
        }
    }
}