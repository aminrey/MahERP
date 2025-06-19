using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using System;
using System.Linq;
using MahERP.DataModelLayer.ViewModels.UserViewModels;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class ContractController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IContractRepository _contractRepository;
        private readonly IStakeholderRepository _stakeholderRepository;
        private new readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public ContractController(
            IUnitOfWork uow,
            IContractRepository contractRepository,
            IStakeholderRepository stakeholderRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache) : base(uow, userManager, persianDateHelper, memoryCache)
        {
            _uow = uow;
            _contractRepository = contractRepository;
            _stakeholderRepository = stakeholderRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        // لیست قراردادها
        public IActionResult Index()
        {
            var contracts = _contractRepository.GetContracts();
            return View(contracts);
        }

        // جزئیات قرارداد
        public IActionResult Details(int id)
        {
            var contract = _contractRepository.GetContractById(id, true);
            if (contract == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<ContractViewModel>(contract);
            viewModel.StartDatePersian = _persianDateHelper.GetPersianDate(contract.StartDate);
            
            if (contract.EndDate.HasValue)
                viewModel.EndDatePersian = _persianDateHelper.GetPersianDate(contract.EndDate.Value);

            viewModel.StakeholderFullName = $"{contract.Stakeholder.FirstName} {contract.Stakeholder.LastName}";
            
            if (!string.IsNullOrEmpty(contract.Stakeholder.CompanyName))
                viewModel.StakeholderFullName += $" ({contract.Stakeholder.CompanyName})";

            // دریافت تسک‌های مرتبط با قرارداد
            ViewBag.Tasks = contract.TaskList.Where(t => !t.IsDeleted).ToList();

            return View(viewModel);
        }

        // افزودن قرارداد جدید - نمایش فرم
        [HttpGet]
        public IActionResult AddContract(int? stakeholderId = null)
        {
            // دریافت لیست طرف حساب‌های فعال
            var stakeholders = _stakeholderRepository.GetStakeholders(false)
                .Select(s => new 
                { 
                    Id = s.Id, 
                    FullName = string.IsNullOrEmpty(s.CompanyName) ? $"{s.FirstName} {s.LastName}" : $"{s.FirstName} {s.LastName} ({s.CompanyName})" 
                })
                .ToList();

            ViewBag.Stakeholders = new SelectList(stakeholders, "Id", "FullName");

            var viewModel = new ContractViewModel
            {
                IsActive = true,
                Status = 1, // فعال به عنوان پیش‌فرض
                StartDatePersian = _persianDateHelper.GetPersianDate(DateTime.Now)
            };

            if (stakeholderId.HasValue)
            {
                viewModel.StakeholderId = stakeholderId.Value;
                var stakeholder = _stakeholderRepository.GetStakeholderById(stakeholderId.Value);
                if (stakeholder != null)
                {
                    viewModel.StakeholderFullName = $"{stakeholder.FirstName} {stakeholder.LastName}";
                    if (!string.IsNullOrEmpty(stakeholder.CompanyName))
                        viewModel.StakeholderFullName += $" ({stakeholder.CompanyName})";
                }
            }

            return View(viewModel);
        }

        // Fix for CS0136: Renaming the inner variable to avoid conflict with the outer variable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContract(ContractViewModel model)
        {
            if (ModelState.IsValid)
            {
                // بررسی یکتا بودن شماره قرارداد
                if (!_contractRepository.IsContractNumberUnique(model.ContractNumber))
                {
                    ModelState.AddModelError("ContractNumber", "شماره قرارداد تکراری است");

                    var stakeholders = _stakeholderRepository.GetStakeholders(false)
                        .Select(s => new
                        {
                            s.Id,
                            FullName = string.IsNullOrEmpty(s.CompanyName) ? $"{s.FirstName} {s.LastName}" : $"{s.FirstName} {s.LastName} ({s.CompanyName})"
                        })
                        .ToList();

                    ViewBag.Stakeholders = new SelectList(stakeholders, "Id", "FullName");

                    return View(model);
                }

                // تبدیل تاریخ‌های شمسی به میلادی
                DateTime startDate = PersianDateHelper.ConvertToGregorianDate(model.StartDatePersian);
                DateTime? endDate = null;

                if (!string.IsNullOrEmpty(model.EndDatePersian))
                    endDate = PersianDateHelper.ConvertToGregorianDate(model.EndDatePersian);

                // ایجاد قرارداد جدید
                var contract = _mapper.Map<Contract>(model);
                contract.StartDate = startDate;
                contract.EndDate = endDate;
                contract.CreateDate = DateTime.Now;
                contract.CreatorUserId = _userManager.GetUserId(User);

                // ذخیره در دیتابیس
                _uow.ContractUW.Create(contract);
                _uow.Save();

                return RedirectToAction(nameof(Index));
            }

            var activeStakeholders = _stakeholderRepository.GetStakeholders(false)
                .Select(s => new
                {
                    Id = s.Id,
                    FullName = string.IsNullOrEmpty(s.CompanyName) ? $"{s.FirstName} {s.LastName}" : $"{s.FirstName} {s.LastName} ({s.CompanyName})"
                })
                .ToList();

            ViewBag.Stakeholders = new SelectList(activeStakeholders, "Id", "FullName");

            return View(model);
        }

        // ویرایش قرارداد - نمایش فرم
        [HttpGet]
        public IActionResult EditContract(int id)
        {
            var contract = _contractRepository.GetContractById(id);
            if (contract == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<ContractViewModel>(contract);
            viewModel.StartDatePersian = _persianDateHelper.GetPersianDate(contract.StartDate);
            
            if (contract.EndDate.HasValue)
                viewModel.EndDatePersian = _persianDateHelper.GetPersianDate(contract.EndDate.Value);

            // دریافت لیست طرف حساب‌های فعال
            var stakeholders = _stakeholderRepository.GetStakeholders(false)
                .Select(s => new 
                { 
                    Id = s.Id, 
                    FullName = string.IsNullOrEmpty(s.CompanyName) ? $"{s.FirstName} {s.LastName}" : $"{s.FirstName} {s.LastName} ({s.CompanyName})" 
                })
                .ToList();

            ViewBag.Stakeholders = new SelectList(stakeholders, "Id", "FullName");

            return View(viewModel);
        }

        // Fix for CS0136: Renaming the inner variable to avoid conflict with the outer variable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditContract(ContractViewModel model)
        {
            if (ModelState.IsValid)
            {
                // بررسی یکتا بودن شماره قرارداد
                if (!_contractRepository.IsContractNumberUnique(model.ContractNumber, model.Id))
                {
                    ModelState.AddModelError("ContractNumber", "شماره قرارداد تکراری است");

                    var Stakeholders = _stakeholderRepository.GetStakeholders(false)
                        .Select(s => new
                        {
                            Id = s.Id,
                            FullName = string.IsNullOrEmpty(s.CompanyName) ? $"{s.FirstName} {s.LastName}" : $"{s.FirstName} {s.LastName} ({s.CompanyName})"
                        })
                        .ToList();

                    ViewBag.Stakeholders = new SelectList(Stakeholders, "Id", "FullName");

                    return View(model);
                }

                // دریافت قرارداد از دیتابیس
                var contract = _uow.ContractUW.GetById(model.Id);
                if (contract == null)
                    return RedirectToAction("ErrorView", "Home");

                // تبدیل تاریخ‌های شمسی به میلادی
                DateTime startDate = PersianDateHelper.ConvertToGregorianDate(model.StartDatePersian);
                DateTime? endDate = null;

                if (!string.IsNullOrEmpty(model.EndDatePersian))
                    endDate = PersianDateHelper.ConvertToGregorianDate(model.EndDatePersian);

                // به‌روزرسانی اطلاعات
                _mapper.Map(model, contract);
                contract.StartDate = startDate;
                contract.EndDate = endDate;
                contract.LastUpdateDate = DateTime.Now;
                contract.LastUpdaterUserId = _userManager.GetUserId(User);

                _uow.ContractUW.Update(contract);
                _uow.Save();

                return RedirectToAction(nameof(Index));
            }

            var activeStakeholders = _stakeholderRepository.GetStakeholders(false)
                .Select(s => new
                {
                    Id = s.Id,
                    FullName = string.IsNullOrEmpty(s.CompanyName) ? $"{s.FirstName} {s.LastName}" : $"{s.FirstName} {s.LastName} ({s.CompanyName})"
                })
                .ToList();

            ViewBag.Stakeholders = new SelectList(activeStakeholders, "Id", "FullName");

            return View(model);
        }

        // تغییر وضعیت قرارداد - نمایش مودال تأیید
        [HttpGet]
        public IActionResult ChangeStatus(int id)
        {
            var contract = _contractRepository.GetContractById(id);
            if (contract == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<ContractViewModel>(contract);
            viewModel.StakeholderFullName = $"{contract.Stakeholder.FirstName} {contract.Stakeholder.LastName}";
            
            if (!string.IsNullOrEmpty(contract.Stakeholder.CompanyName))
                viewModel.StakeholderFullName += $" ({contract.Stakeholder.CompanyName})";

            return PartialView("_ChangeStatus", viewModel);
        }

        // تغییر وضعیت قرارداد - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatusPost(int id, byte status)
        {
            var contract = _uow.ContractUW.GetById(id);
            if (contract == null)
                return RedirectToAction("ErrorView", "Home");

            contract.Status = status;
            contract.LastUpdateDate = DateTime.Now;
            contract.LastUpdaterUserId = _userManager.GetUserId(User);
            
            _uow.ContractUW.Update(contract);
            _uow.Save();

            return RedirectToAction("Details", new { id = id });
        }

        // فعال/غیرفعال کردن قرارداد - نمایش مودال تأیید
        [HttpGet]
        public IActionResult ToggleActivation(int id)
        {
            var contract = _contractRepository.GetContractById(id);
            if (contract == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<ContractViewModel>(contract);
            viewModel.StakeholderFullName = $"{contract.Stakeholder.FirstName} {contract.Stakeholder.LastName}";
            
            if (!string.IsNullOrEmpty(contract.Stakeholder.CompanyName))
                viewModel.StakeholderFullName += $" ({contract.Stakeholder.CompanyName})";

            if (contract.IsActive)
            {
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ModalTitle = "غیرفعال کردن قرارداد";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            }
            else
            {
                ViewBag.themeclass = "bg-gd-lake";
                ViewBag.ModalTitle = "فعال کردن قرارداد";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
            }

            return PartialView("_ToggleActivation", viewModel);
        }

        // فعال/غیرفعال کردن قرارداد - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActivationPost(int id)
        {
            var contract = _uow.ContractUW.GetById(id);
            if (contract == null)
                return RedirectToAction("ErrorView", "Home");

            contract.IsActive = !contract.IsActive;
            contract.LastUpdateDate = DateTime.Now;
            contract.LastUpdaterUserId = _userManager.GetUserId(User);
            
            _uow.ContractUW.Update(contract);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        // لیست قراردادهای مرتبط با یک طرف حساب
        public IActionResult StakeholderContracts(int stakeholderId)
        {
            var stakeholder = _stakeholderRepository.GetStakeholderById(stakeholderId);
            if (stakeholder == null)
                return RedirectToAction("ErrorView", "Home");

            var contracts = _contractRepository.GetStakeholderContracts(stakeholderId);
            
            ViewBag.StakeholderId = stakeholderId;
            ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";
            
            if (!string.IsNullOrEmpty(stakeholder.CompanyName))
                ViewBag.StakeholderName += $" ({stakeholder.CompanyName})";

            return View("Index", contracts);
        }

        // جستجوی قرارداد
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            var contracts = _contractRepository.SearchContracts(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", contracts);
        }

        // فیلتر قراردادها بر اساس وضعیت
        [HttpGet]
        public IActionResult FilterByStatus(byte status)
        {
            var contracts = _contractRepository.GetContractsByStatus(status);
            
            ViewBag.StatusFilter = status;
            ViewBag.StatusText = status switch
            {
                0 => "پیش‌نویس",
                1 => "فعال",
                2 => "تمام شده",
                3 => "لغو شده",
                _ => "همه"
            };
            
            return View("Index", contracts);
        }

        // لیست قراردادهای فعال
        [HttpGet]
        public IActionResult ActiveContracts()
        {
            var contracts = _contractRepository.GetActiveContracts();
            ViewBag.Title = "قراردادهای فعال";
            return View("Index", contracts);
        }

        // لیست قراردادهای منقضی شده
        [HttpGet]
        public IActionResult ExpiredContracts()
        {
            var contracts = _contractRepository.GetExpiredContracts();
            ViewBag.Title = "قراردادهای منقضی شده";
            return View("Index", contracts);
        }
    }
}