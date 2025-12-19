using MahERP.Areas.CrmArea.Controllers.BaseControllers;
using MahERP.Attributes;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Enums;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.CrmRepository;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MahERP.Areas.CrmArea.Controllers
{
    /// <summary>
    /// کنترلر مدیریت مراحل (Lead Stages و Post-Purchase Stages)
    /// </summary>
    [Area("CrmArea")]
    [Authorize]
    [PermissionRequired("CRM")]
    public class StageController : BaseController
    {
        private readonly ILeadStageStatusRepository _leadStageRepo;
        private readonly IPostPurchaseStageRepository _postPurchaseRepo;

        public StageController(
            IUnitOfWork uow,
            UserManager<AppUsers> userManager,
            PersianDateHelper persianDateHelper,
            IMemoryCache memoryCache,
            ActivityLoggerService activityLogger,
            IUserManagerRepository userRepository,
            IBaseRepository baseRepository,
            IModuleTrackingService moduleTracking,
            IModuleAccessService moduleAccessService,
            ILeadStageStatusRepository leadStageRepo,
            IPostPurchaseStageRepository postPurchaseRepo)
            : base(uow, userManager, persianDateHelper, memoryCache, activityLogger, userRepository, baseRepository, moduleTracking, moduleAccessService)
        {
            _leadStageRepo = leadStageRepo;
            _postPurchaseRepo = postPurchaseRepo;
        }

        /// <summary>
        /// صفحه اصلی مدیریت مراحل
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        #region Lead Stage Status

        /// <summary>
        /// دریافت لیست مراحل قیف فروش (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLeadStages()
        {
            var stages = await _leadStageRepo.GetAllAsync(activeOnly: true);
            
            var result = stages.Select(s => new
            {
                id = s.Id,
                title = s.Title,
                description = s.Description,
                color = s.ColorCode,
                displayOrder = s.DisplayOrder,
                stageType = s.StageType,
                stageTypeName = GetLeadStageTypeName(s.StageType)
            }).ToList();

            return Json(result);
        }

        /// <summary>
        /// نمایش مودال افزودن مرحله قیف فروش
        /// </summary>
        [HttpGet]
        public IActionResult AddLeadStageModal()
        {
            ViewBag.ThemeClass = "bg-primary";
            ViewBag.ViewTitle = "افزودن مرحله قیف فروش";
            return PartialView("_AddLeadStageModal");
        }

        /// <summary>
        /// ایجاد مرحله قیف فروش جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLeadStage(string title, string? description, string? color, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = "عنوان الزامی است" } } 
                });
            }

            try
            {
                var stage = new LeadStageStatus
                {
                    Title = title,
                    Description = description,
                    ColorCode = color ?? "#667eea",
                    DisplayOrder = displayOrder,
                    StageType = LeadStageType.Awareness
                };

                await _leadStageRepo.CreateAsync(stage);

                return Json(new { 
                    status = "success", 
                    message = new[] { new { status = "success", text = "مرحله با موفقیت ایجاد شد" } },
                    id = stage.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = ex.Message } } 
                });
            }
        }

        /// <summary>
        /// نمایش مودال ویرایش مرحله قیف فروش
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditLeadStageModal(int id)
        {
            var stage = await _leadStageRepo.GetByIdAsync(id);
            if (stage == null)
            {
                return NotFound();
            }

            ViewBag.ThemeClass = "bg-primary";
            ViewBag.ViewTitle = "ویرایش مرحله قیف فروش";
            return PartialView("_EditLeadStageModal", stage);
        }

        /// <summary>
        /// ویرایش مرحله قیف فروش
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLeadStage(int id, string title, string? description, string? color, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = "عنوان الزامی است" } } 
                });
            }

            try
            {
                var stage = await _leadStageRepo.GetByIdAsync(id);
                if (stage == null)
                {
                    return Json(new { 
                        status = "error", 
                        message = new[] { new { status = "error", text = "مرحله یافت نشد" } } 
                    });
                }

                stage.Title = title;
                stage.Description = description;
                stage.ColorCode = color ?? stage.ColorCode;
                stage.DisplayOrder = displayOrder;

                var success = await _leadStageRepo.UpdateAsync(stage);

                return Json(new { 
                    status = success ? "success" : "error", 
                    message = new[] { new { status = success ? "success" : "error", text = success ? "تغییرات ذخیره شد" : "خطا در ذخیره" } }
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = ex.Message } } 
                });
            }
        }

        /// <summary>
        /// نمایش مودال حذف مرحله قیف فروش
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteLeadStageModal(int id)
        {
            var stage = await _leadStageRepo.GetByIdAsync(id);
            if (stage == null)
            {
                return NotFound();
            }

            ViewBag.ThemeClass = "bg-danger";
            ViewBag.ViewTitle = "حذف مرحله قیف فروش";
            ViewBag.StageType = "lead";
            return PartialView("_DeleteStageModal", stage);
        }

        /// <summary>
        /// حذف مرحله قیف فروش
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLeadStage(int id)
        {
            try
            {
                var success = await _leadStageRepo.DeleteAsync(id);
                return Json(new { 
                    status = success ? "success" : "error", 
                    message = new[] { new { status = success ? "success" : "error", text = success ? "مرحله حذف شد" : "خطا در حذف" } }
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = ex.Message } } 
                });
            }
        }

        #endregion

        #region Post-Purchase Stages

        /// <summary>
        /// دریافت لیست مراحل بعد از خرید (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPostPurchaseStages()
        {
            var stages = await _postPurchaseRepo.GetAllAsync(activeOnly: true);
            
            var result = stages.Select(s => new
            {
                id = s.Id,
                title = s.Title,
                description = s.Description,
                color = s.ColorCode,
                displayOrder = s.DisplayOrder,
                stageType = s.StageType,
                stageTypeName = GetPostPurchaseStageTypeName(s.StageType)
            }).ToList();

            return Json(result);
        }

        /// <summary>
        /// نمایش مودال افزودن مرحله بعد از خرید
        /// </summary>
        [HttpGet]
        public IActionResult AddPostPurchaseStageModal()
        {
            ViewBag.ThemeClass = "bg-success";
            ViewBag.ViewTitle = "افزودن مرحله بعد از خرید";
            return PartialView("_AddPostPurchaseStageModal");
        }

        /// <summary>
        /// ایجاد مرحله بعد از خرید جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePostPurchaseStage(string title, string? description, string? color, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = "عنوان الزامی است" } } 
                });
            }

            try
            {
                var stage = new PostPurchaseStage
                {
                    Title = title,
                    Description = description,
                    ColorCode = color ?? "#28a745",
                    DisplayOrder = displayOrder,
                    StageType = PostPurchaseStageType.Retention
                };

                await _postPurchaseRepo.CreateAsync(stage);

                return Json(new { 
                    status = "success", 
                    message = new[] { new { status = "success", text = "مرحله با موفقیت ایجاد شد" } },
                    id = stage.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = ex.Message } } 
                });
            }
        }

        /// <summary>
        /// نمایش مودال ویرایش مرحله بعد از خرید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditPostPurchaseStageModal(int id)
        {
            var stage = await _postPurchaseRepo.GetByIdAsync(id);
            if (stage == null)
            {
                return NotFound();
            }

            ViewBag.ThemeClass = "bg-success";
            ViewBag.ViewTitle = "ویرایش مرحله بعد از خرید";
            return PartialView("_EditPostPurchaseStageModal", stage);
        }

        /// <summary>
        /// ویرایش مرحله بعد از خرید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePostPurchaseStage(int id, string title, string? description, string? color, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = "عنوان الزامی است" } } 
                });
            }

            try
            {
                var stage = await _postPurchaseRepo.GetByIdAsync(id);
                if (stage == null)
                {
                    return Json(new { 
                        status = "error", 
                        message = new[] { new { status = "error", text = "مرحله یافت نشد" } } 
                    });
                }

                stage.Title = title;
                stage.Description = description;
                stage.ColorCode = color ?? stage.ColorCode;
                stage.DisplayOrder = displayOrder;

                var success = await _postPurchaseRepo.UpdateAsync(stage);

                return Json(new { 
                    status = success ? "success" : "error", 
                    message = new[] { new { status = success ? "success" : "error", text = success ? "تغییرات ذخیره شد" : "خطا در ذخیره" } }
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = ex.Message } } 
                });
            }
        }

        /// <summary>
        /// نمایش مودال حذف مرحله بعد از خرید
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeletePostPurchaseStageModal(int id)
        {
            var stage = await _postPurchaseRepo.GetByIdAsync(id);
            if (stage == null)
            {
                return NotFound();
            }

            ViewBag.ThemeClass = "bg-danger";
            ViewBag.ViewTitle = "حذف مرحله بعد از خرید";
            ViewBag.StageType = "postpurchase";
            return PartialView("_DeletePostPurchaseStageModal", stage);
        }

        /// <summary>
        /// حذف مرحله بعد از خرید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePostPurchaseStage(int id)
        {
            try
            {
                var success = await _postPurchaseRepo.DeleteAsync(id);
                return Json(new { 
                    status = success ? "success" : "error", 
                    message = new[] { new { status = success ? "success" : "error", text = success ? "مرحله حذف شد" : "خطا در حذف" } }
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    status = "error", 
                    message = new[] { new { status = "error", text = ex.Message } } 
                });
            }
        }

        #endregion

        #region Private Methods

        private string GetLeadStageTypeName(LeadStageType stageType)
        {
            return stageType switch
            {
                LeadStageType.Awareness => "آگاهی",
                LeadStageType.Interest => "علاقه‌مندی",
                LeadStageType.Evaluation => "ارزیابی",
                LeadStageType.Decision => "تصمیم‌گیری",
                LeadStageType.Purchase => "خرید",
                _ => "نامشخص"
            };
        }

        private string GetPostPurchaseStageTypeName(PostPurchaseStageType stageType)
        {
            return stageType switch
            {
                PostPurchaseStageType.Retention => "حفظ مشتری",
                PostPurchaseStageType.Referral => "ارجاع/توصیه",
                PostPurchaseStageType.Loyalty => "وفادارسازی",
                PostPurchaseStageType.VIP => "VIP",
                _ => "نامشخص"
            };
        }

        #endregion
    }
}
