using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using MahERP.Areas.AdminArea.Controllers.BaseControllers;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class StakeholderController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IStakeholderRepository _stakeholderRepository;
        private readonly UserManager<AppUsers> _userManager;
        private readonly IMapper _mapper;

        public StakeholderController(
            IUnitOfWork uow,
            IStakeholderRepository stakeholderRepository,
            UserManager<AppUsers> userManager,
            IMapper mapper,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache) : base(uow, userManager, persianDateHelper, memoryCache)
        {
            _uow = uow;
            _stakeholderRepository = stakeholderRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        // لیست طرف حساب‌ها
        public IActionResult Index(int? type = null)
        {
            var stakeholders = _stakeholderRepository.GetStakeholders(false, type);
            return View(stakeholders);
        }

        // جزئیات طرف حساب
        public IActionResult Details(int id)
        {
            var stakeholder = _stakeholderRepository.GetStakeholderById(id, true, true, true, true);
            if (stakeholder == null)
                return RedirectToAction("ErrorView", "Home");

            var stakeholderCRM = _stakeholderRepository.GetStakeholderCRMById(id);
            
            var viewModel = _mapper.Map<StakeholderViewModel>(stakeholder);
            if (stakeholderCRM != null)
            {
                viewModel.CRMInfo = _mapper.Map<StakeholderCRMViewModel>(stakeholderCRM);
            }

            return View(viewModel);
        }

        // افزودن طرف حساب جدید - نمایش فرم
        [HttpGet]
        public IActionResult AddStakeholder()
        {
            return View(new StakeholderViewModel { IsActive = true });
        }

        // افزودن طرف حساب جدید - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStakeholder(StakeholderViewModel model)
        {
            if (ModelState.IsValid)
            {
                // بررسی یکتا بودن کد ملی و ایمیل
                if (!string.IsNullOrEmpty(model.NationalCode) && !_stakeholderRepository.IsNationalCodeUnique(model.NationalCode))
                {
                    ModelState.AddModelError("NationalCode", "کد ملی وارد شده قبلاً ثبت شده است");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.Email) && !_stakeholderRepository.IsEmailUnique(model.Email))
                {
                    ModelState.AddModelError("Email", "ایمیل وارد شده قبلاً ثبت شده است");
                    return View(model);
                }

                // ایجاد طرف حساب جدید
                var stakeholder = _mapper.Map<Stakeholder>(model);
                stakeholder.CreateDate = DateTime.Now;
                stakeholder.CreatorUserId = _userManager.GetUserId(User);
                stakeholder.IsActive = true;
                stakeholder.IsDeleted = false;

                // ذخیره در دیتابیس
                _uow.StakeholderUW.Create(stakeholder);
                _uow.Save();

                // اگر اطلاعات CRM وجود داشته باشد، آنها را هم ذخیره می‌کنیم
                if (model.CRMInfo != null)
                {
                    var stakeholderCRM = _mapper.Map<StakeholderCRM>(model.CRMInfo);
                    stakeholderCRM.StakeholderId = stakeholder.Id;
                    stakeholderCRM.CreateDate = DateTime.Now;

                    _uow.StakeholderCRMUW.Create(stakeholderCRM);
                    _uow.Save();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ویرایش طرف حساب - نمایش فرم
        [HttpGet]
        public IActionResult EditStakeholder(int id)
        {
            var stakeholder = _stakeholderRepository.GetStakeholderById(id);
            if (stakeholder == null)
                return RedirectToAction("ErrorView", "Home");

            var viewModel = _mapper.Map<StakeholderViewModel>(stakeholder);
            
            // دریافت اطلاعات CRM اگر وجود داشته باشد
            var stakeholderCRM = _stakeholderRepository.GetStakeholderCRMById(id);
            if (stakeholderCRM != null)
            {
                viewModel.CRMInfo = _mapper.Map<StakeholderCRMViewModel>(stakeholderCRM);
            }

            return View(viewModel);
        }

        // ویرایش طرف حساب - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStakeholder(StakeholderViewModel model)
        {
            if (ModelState.IsValid)
            {
                // بررسی یکتا بودن کد ملی و ایمیل
                if (!string.IsNullOrEmpty(model.NationalCode) && 
                    !_stakeholderRepository.IsNationalCodeUnique(model.NationalCode, model.Id))
                {
                    ModelState.AddModelError("NationalCode", "کد ملی وارد شده قبلاً ثبت شده است");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.Email) && 
                    !_stakeholderRepository.IsEmailUnique(model.Email, model.Id))
                {
                    ModelState.AddModelError("Email", "ایمیل وارد شده قبلاً ثبت شده است");
                    return View(model);
                }

                // دریافت طرف حساب از دیتابیس
                var stakeholder = _uow.StakeholderUW.GetById(model.Id);
                if (stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");

                // به‌روزرسانی اطلاعات
                _mapper.Map(model, stakeholder);
                _uow.StakeholderUW.Update(stakeholder);
                _uow.Save();

                // به‌روزرسانی اطلاعات CRM
                if (model.CRMInfo != null)
                {
                    var stakeholderCRM = _uow.StakeholderCRMUW.Get(c => c.StakeholderId == model.Id).FirstOrDefault();
                    
                    if (stakeholderCRM == null)
                    {
                        // ایجاد رکورد جدید اگر وجود نداشته باشد
                        stakeholderCRM = new StakeholderCRM
                        {
                            StakeholderId = model.Id,
                            CreateDate = DateTime.Now
                        };
                        _mapper.Map(model.CRMInfo, stakeholderCRM);
                        _uow.StakeholderCRMUW.Create(stakeholderCRM);
                    }
                    else
                    {
                        // بروزرسانی رکورد موجود
                        _mapper.Map(model.CRMInfo, stakeholderCRM);
                        stakeholderCRM.LastUpdateDate = DateTime.Now;
                        _uow.StakeholderCRMUW.Update(stakeholderCRM);
                    }
                    
                    _uow.Save();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // فعال/غیرفعال کردن طرف حساب - نمایش مودال تأیید
        [HttpGet]
        public IActionResult ActiveOrDeactiveStakeholder(int id)
        {
            var stakeholder = _uow.StakeholderUW.GetById(id);
            if (stakeholder == null)
                return RedirectToAction("ErrorView", "Home");

            if (stakeholder.IsActive)
            {
                // غیرفعال کردن
                ViewBag.themeclass = "bg-gd-fruit";
                ViewBag.ModalTitle = "غیرفعال کردن طرف حساب";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            }
            else
            {
                // فعال کردن
                ViewBag.themeclass = "bg-gd-lake";
                ViewBag.ModalTitle = "فعال کردن طرف حساب";
                ViewBag.ButonClass = "btn rounded-0 btn-hero btn-success";
            }

            return PartialView("_ActiveOrDeactiveStakeholder", stakeholder);
        }

        // فعال/غیرفعال کردن طرف حساب - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActiveOrDeactiveStakeholderPost(int id, bool isActive)
        {
            var stakeholder = _uow.StakeholderUW.GetById(id);
            if (stakeholder == null)
                return RedirectToAction("ErrorView", "Home");

            stakeholder.IsActive = !isActive;
            _uow.StakeholderUW.Update(stakeholder);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        // حذف طرف حساب - نمایش مودال تأیید
        [HttpGet]
        public IActionResult DeleteStakeholder(int id)
        {
            var stakeholder = _uow.StakeholderUW.GetById(id);
            if (stakeholder == null)
                return RedirectToAction("ErrorView", "Home");

            ViewBag.ButonClass = "btn rounded-0 btn-hero btn-danger";
            ViewBag.themeclass = "bg-gd-fruit";
            ViewBag.ViewTitle = "حذف طرف حساب";

            return PartialView("_DeleteStakeholder", stakeholder);
        }

        // حذف طرف حساب - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStakeholderPost(int id)
        {
            var stakeholder = _uow.StakeholderUW.GetById(id);
            if (stakeholder == null)
                return RedirectToAction("ErrorView", "Home");

            stakeholder.IsDeleted = true;
            _uow.StakeholderUW.Update(stakeholder);
            _uow.Save();

            return RedirectToAction(nameof(Index));
        }
    }
}
