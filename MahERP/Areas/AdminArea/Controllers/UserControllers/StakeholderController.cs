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
using ClosedXML.Excel;

namespace MahERP.Areas.AdminArea.Controllers.UserControllers
{
    [Area("AdminArea")]
    [Authorize]
    public class StakeholderController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IStakeholderRepository _stakeholderRepository;
        private new readonly UserManager<AppUsers> _userManager;
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

        // خروجی اکسل از طرف حساب‌ها
        public IActionResult ExportToExcel(StakeholderSearchViewModel model)
        {
            // اجرای دوباره جستجو برای بدست آوردن نتایج
            var query = _uow.StakeholderUW.Get().AsQueryable();
            
            // فیلتر وضعیت حذف شده
            if (!model.IncludeDeleted)
            {
                query = query.Where(s => !s.IsDeleted);
            }
            
            // فیلتر وضعیت فعال
            if (model.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == model.IsActive.Value);
            }
            
            // سایر فیلترها مشابه متد Search
            // ...
            
            var stakeholders = query.OrderByDescending(s => s.CreateDate).ToList();
            
            // ایجاد فایل اکسل
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("طرف حساب‌ها");
                
                // سرستون‌ها
                worksheet.Cell(1, 1).Value = "ردیف";
                worksheet.Cell(1, 2).Value = "نام";
                worksheet.Cell(1, 3).Value = "نام خانوادگی";
                worksheet.Cell(1, 4).Value = "نام شرکت";
                worksheet.Cell(1, 5).Value = "تلفن ثابت";
                worksheet.Cell(1, 6).Value = "تلفن همراه";
                worksheet.Cell(1, 7).Value = "ایمیل";
                worksheet.Cell(1, 8).Value = "کد ملی";
                worksheet.Cell(1, 9).Value = "نوع طرف حساب";
                worksheet.Cell(1, 10).Value = "وضعیت";
                worksheet.Cell(1, 11).Value = "آدرس";
                worksheet.Cell(1, 12).Value = "تاریخ ثبت";
                
                // استایل هدر
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                
                // ترتیب ردیف
                int row = 2;
                foreach (var item in stakeholders)
                {
                    worksheet.Cell(row, 1).Value = (row - 1);
                    worksheet.Cell(row, 2).Value = item.FirstName;
                    worksheet.Cell(row, 3).Value = item.LastName;
                    worksheet.Cell(row, 4).Value = item.CompanyName;
                    worksheet.Cell(row, 5).Value = item.Phone;
                    worksheet.Cell(row, 6).Value = item.Mobile;
                    worksheet.Cell(row, 7).Value = item.Email;
                    worksheet.Cell(row, 8).Value = item.NationalCode;
                    
                    // نوع طرف حساب
                    string stakeholderType = item.StakeholderType switch
                    {
                        0 => "مشتری",
                        1 => "تامین کننده",
                        2 => "همکار",
                        3 => "سایر",
                        _ => "نامشخص"
                    };
                    worksheet.Cell(row, 9).Value = stakeholderType;
                    
                    // وضعیت
                    string status = item.IsDeleted ? "حذف شده" : (item.IsActive ? "فعال" : "غیرفعال");
                    worksheet.Cell(row, 10).Value = status;
                    
                    worksheet.Cell(row, 11).Value = item.Address;
                    worksheet.Cell(row, 12).Value = _persianDateHelper.GetPersianDate(item.CreateDate);
                    
                    row++;
                }
                
                // تنظیم عرض ستون‌ها
                worksheet.Columns().AdjustToContents();
                
                // ذخیره به حافظه
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();
                    
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"Stakeholders_{DateTime.Now:yyyy_MM_dd}.xlsx"
                    };
                }
            }
        }

        // جستجوی پیشرفته - نمایش فرم
        [HttpGet]
        public IActionResult AdvancedSearch()
        {
            // دریافت لیست کارشناسان فروش برای dropdown
            ViewBag.SalesReps = _userManager.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new { Id = u.Id, FullName = u.FirstName + " " + u.LastName })
                .ToList();
            
            return PartialView("_AdvancedSearch", new StakeholderSearchViewModel());
        }

        // جستجوی پیشرفته - پردازش جستجو
        [HttpPost]
        public IActionResult Search(StakeholderSearchViewModel model)
        {
            var query = _uow.StakeholderUW.Get().AsQueryable();
            
            // فیلتر وضعیت حذف شده
            if (!model.IncludeDeleted)
            {
                query = query.Where(s => !s.IsDeleted);
            }
            
            // فیلتر وضعیت فعال
            if (model.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == model.IsActive.Value);
            }
            
            // فیلتر نوع طرف حساب
            if (model.StakeholderType.HasValue)
            {
                query = query.Where(s => s.StakeholderType == model.StakeholderType.Value);
            }
            
            // جستجو در نام و نام خانوادگی
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                query = query.Where(s => s.FirstName.Contains(model.Name) || s.LastName.Contains(model.Name));
            }
            
            // جستجو در نام شرکت
            if (!string.IsNullOrWhiteSpace(model.CompanyName))
            {
                query = query.Where(s => s.CompanyName.Contains(model.CompanyName));
            }
            
            // جستجو در تلفن
            if (!string.IsNullOrWhiteSpace(model.Phone))
            {
                query = query.Where(s => s.Phone.Contains(model.Phone) || s.Mobile.Contains(model.Phone));
            }
            
            // جستجو در ایمیل
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                query = query.Where(s => s.Email.Contains(model.Email));
            }
            
            // جستجو در کد ملی
            if (!string.IsNullOrWhiteSpace(model.NationalCode))
            {
                query = query.Where(s => s.NationalCode.Contains(model.NationalCode));
            }
            
            // فیلتر تاریخ ایجاد
            if (!string.IsNullOrWhiteSpace(model.FromDate))
            {
                DateTime fromDate = PersianDateHelper.ConvertToGregorianDate(model.FromDate);
                query = query.Where(s => s.CreateDate >= fromDate);
            }
            
            if (!string.IsNullOrWhiteSpace(model.ToDate))
            {
                DateTime toDate = PersianDateHelper.ConvertToGregorianDate(model.ToDate).AddDays(1);
                query = query.Where(s => s.CreateDate <= toDate);
            }
            
            //// فیلترهای CRM
            //if (model.SalesStage.HasValue || model.LeadSource.HasValue || !string.IsNullOrWhiteSpace(model.Industry) ||
            //    !string.IsNullOrWhiteSpace(model.CreditRating) || model.MinPotentialValue.HasValue || model.MaxPotentialValue.HasValue ||
            //    !string.IsNullOrWhiteSpace(model.SalesRepUserId))
            //{
            //    // در صورتی که فیلترهای CRM وجود داشته باشند، باید ارتباط با جدول CRM را اضافه کنیم
            //    query = query.Join(_uow.StakeholderCRMUW.Get(),
            //        stakeholder => stakeholder.Id,
            //        crm => crm.StakeholderId,
            //        (stakeholder, crm) => new { Stakeholder = stakeholder, CRM = crm })
            //        .AsQueryable();
                
            //    if (model.SalesStage.HasValue)
            //    {
            //        query = query.Where(x => x.CRM.SalesStage == model.SalesStage.Value);
            //    }
                
            //    if (model.LeadSource.HasValue)
            //    {
            //        query = query.Where(x => x.CRM.LeadSource == model.LeadSource.Value);
            //    }
                
            //    if (!string.IsNullOrWhiteSpace(model.Industry))
            //    {
            //        query = query.Where(x => x.CRM.Industry.Contains(model.Industry));
            //    }
                
            //    if (!string.IsNullOrWhiteSpace(model.CreditRating))
            //    {
            //        query = query.Where(x => x.CRM.CreditRating == model.CreditRating);
            //    }
                
            //    if (model.MinPotentialValue.HasValue)
            //    {
            //        query = query.Where(x => x.CRM.PotentialValue >= model.MinPotentialValue.Value);
            //    }
                
            //    if (model.MaxPotentialValue.HasValue)
            //    {
            //        query = query.Where(x => x.CRM.PotentialValue <= model.MaxPotentialValue.Value);
            //    }
                
            //    if (!string.IsNullOrWhiteSpace(model.SalesRepUserId))
            //    {
            //        query = query.Where(x => x.CRM.SalesRepUserId == model.SalesRepUserId);
            //    }
                
            //    // استخراج دوباره فقط اطلاعات Stakeholder
            //    query = query.Select(x => x.Stakeholder);
            //}
            
            // مرتب‌سازی بر اساس تاریخ ایجاد (نزولی)
            var stakeholders = query.OrderByDescending(s => s.CreateDate).ToList();
            
            // ذخیره پارامترهای جستجو در ViewBag برای استفاده در صفحه نتایج
            ViewBag.SearchModel = model;
            
            return View("SearchResults", stakeholders);
        }

        // افزودن تماس مرتبط - نمایش فرم
        [HttpGet]
        public IActionResult AddContact(int stakeholderId)
        {
            var stakeholder = _uow.StakeholderUW.GetById(stakeholderId);
            if (stakeholder == null)
                return RedirectToAction("ErrorView", "Home");
            
            ViewBag.StakeholderId = stakeholderId;
            ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";
            
            return View(new StakeholderContactViewModel 
            { 
                StakeholderId = stakeholderId,
                IsActive = true 
            });
        }

        // افزودن تماس مرتبط - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContact(StakeholderContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                var Stakeholder = _uow.StakeholderUW.GetById(model.StakeholderId);
                if (Stakeholder == null)
                    return RedirectToAction("ErrorView", "Home");
                
                // ایجاد تماس جدید
                var contact = _mapper.Map<StakeholderContact>(model);
                contact.CreateDate = DateTime.Now;
                contact.CreatorUserId = _userManager.GetUserId(User);
                
                // اگر این تماس به عنوان اصلی انتخاب شده، سایر تماس‌ها را از حالت اصلی خارج کنیم
                if (model.IsPrimary)
                {
                    var primaryContacts = _uow.StakeholderContactUW.Get(c => c.StakeholderId == model.StakeholderId && c.IsPrimary);
                    foreach (var primaryContact in primaryContacts)
                    {
                        primaryContact.IsPrimary = false;
                        _uow.StakeholderContactUW.Update(primaryContact);
                    }
                }
                
                // ذخیره در دیتابیس
                _uow.StakeholderContactUW.Create(contact);
                _uow.Save();
                
                return RedirectToAction("Details", new { id = model.StakeholderId });
            }
            
            // در صورت وجود خطا، اطلاعات را دوباره به ویو برگردانیم
            var stakeholder = _uow.StakeholderUW.GetById(model.StakeholderId);
            ViewBag.StakeholderId = model.StakeholderId;
            ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";
            
            return View(model);
        }

        // ویرایش تماس مرتبط - نمایش فرم
        [HttpGet]
        public IActionResult EditContact(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");
            
            var stakeholder = _uow.StakeholderUW.GetById(contact.StakeholderId);
            ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";
            
            var viewModel = _mapper.Map<StakeholderContactViewModel>(contact);
            
            return View(viewModel);
        }

        // ویرایش تماس مرتبط - پردازش فرم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditContact(StakeholderContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contact = _uow.StakeholderContactUW.GetById(model.Id);
                if (contact == null)
                    return RedirectToAction("ErrorView", "Home");
                
                // به‌روزرسانی اطلاعات
                _mapper.Map(model, contact);
                
                // اگر این تماس به عنوان اصلی انتخاب شده، سایر تماس‌ها را از حالت اصلی خارج کنیم
                if (model.IsPrimary)
                {
                    var primaryContacts = _uow.StakeholderContactUW.Get(c => c.StakeholderId == model.StakeholderId && c.IsPrimary && c.Id != model.Id);
                    foreach (var primaryContact in primaryContacts)
                    {
                        primaryContact.IsPrimary = false;
                        _uow.StakeholderContactUW.Update(primaryContact);
                    }
                }
                
                _uow.StakeholderContactUW.Update(contact);
                _uow.Save();
                
                return RedirectToAction("Details", new { id = model.StakeholderId });
            }
            
            // در صورت وجود خطا، اطلاعات را دوباره به ویو برگردانیم
            var stakeholder = _uow.StakeholderUW.GetById(model.StakeholderId);
            ViewBag.StakeholderName = $"{stakeholder.FirstName} {stakeholder.LastName}";
            
            return View(model);
        }

        // حذف تماس مرتبط - نمایش مودال تأیید
        [HttpGet]
        public IActionResult DeleteContact(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");
            
            return PartialView("_DeleteContact", contact);
        }

        // حذف تماس مرتبط - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteContactPost(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");
            
            int stakeholderId = contact.StakeholderId;
            
            _uow.StakeholderContactUW.Delete(contact);
            _uow.Save();
            
            return RedirectToAction("Details", new { id = stakeholderId });
        }

        // فعال/غیرفعال کردن تماس مرتبط - نمایش مودال تأیید
        [HttpGet]
        public IActionResult ToggleContactStatus(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");
            
            ViewBag.ModalTitle = contact.IsActive ? "غیرفعال کردن فرد مرتبط" : "فعال کردن فرد مرتبط";
            ViewBag.ButtonClass = contact.IsActive ? "btn btn-danger" : "btn btn-success";
            ViewBag.ActionText = contact.IsActive ? "غیرفعال کردن" : "فعال کردن";
            
            return PartialView("_ToggleContactStatus", contact);
        }

        // فعال/غیرفعال کردن تماس مرتبط - پردازش درخواست
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleContactStatusPost(int id)
        {
            var contact = _uow.StakeholderContactUW.GetById(id);
            if (contact == null)
                return RedirectToAction("ErrorView", "Home");
            
            contact.IsActive = !contact.IsActive;
            _uow.StakeholderContactUW.Update(contact);
            _uow.Save();
            
            return RedirectToAction("Details", new { id = contact.StakeholderId });
        }
    }

}
