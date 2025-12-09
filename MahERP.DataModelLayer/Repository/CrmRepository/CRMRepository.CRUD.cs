using AutoMapper;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.CRMViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// CRM Repository - CRUD Operations
    /// </summary>
    public partial class CRMRepository
    {
        #region Prepare Models

        /// <summary>
        /// آماده‌سازی مدل برای ایجاد تعامل جدید
        /// </summary>
        public async Task<CRMInteractionViewModel> PrepareCreateInteractionModelAsync(string userId)
        {
            var model = new CRMInteractionViewModel
            {
                IsActive = true,
                CreateDate = DateTime.Now
            };

            // تولید کد CRM
            Random random = new Random();
            string crmCode;
            do
            {
                crmCode = "CRM" + random.Next(10000, 99999).ToString();
            } while (!IsCRMCodeUnique(crmCode));

            model.CRMCode = crmCode;

            // دریافت لیست شعبه‌های کاربر
            var userBranches = await _context.BranchUser_Tbl
                .Where(bu => bu.UserId == userId && bu.IsActive)
                .Include(bu => bu.Branch)
                .Where(bu => bu.Branch.IsActive)
                .Select(bu => new BranchViewModel
                {
                    Id = bu.BranchId,
                    Name = bu.Branch.Name
                })
                .OrderBy(b => b.Name)
                .ToListAsync();

            model.BranchesInitial = userBranches;

            // اگر فقط یک شعبه باشد، خودکار انتخاب کن
            if (userBranches.Count == 1)
            {
                var singleBranch = userBranches.First();
                model.BranchId = singleBranch.Id;

                // بارگذاری Contacts و Organizations شعبه
                model.ContactsInitial = await GetBranchContactsAsync(singleBranch.Id);
                model.OrganizationsInitial = await GetBranchOrganizationsAsync(singleBranch.Id);
            }
            else
            {
                model.ContactsInitial = new List<ContactViewModel>();
                model.OrganizationsInitial = new List<OrganizationViewModel>();
            }

            model.ContactOrganizations = new List<OrganizationViewModel>();

            return model;
        }

        /// <summary>
        /// آماده‌سازی مدل برای ویرایش تعامل
        /// </summary>
        public async Task<CRMInteractionViewModel> PrepareEditInteractionModelAsync(int interactionId, string userId)
        {
            var interaction = GetCRMInteractionById(interactionId);
            if (interaction == null)
                return null;

            var model = _mapper.Map<CRMInteractionViewModel>(interaction);

            // دریافت لیست شعبه‌های کاربر
            var userBranches = await _context.BranchUser_Tbl
                .Where(bu => bu.UserId == userId && bu.IsActive)
                .Include(bu => bu.Branch)
                .Where(bu => bu.Branch.IsActive)
                .Select(bu => new BranchViewModel
                {
                    Id = bu.BranchId,
                    Name = bu.Branch.Name
                })
                .OrderBy(b => b.Name)
                .ToListAsync();

            model.BranchesInitial = userBranches;

            // بارگذاری Contacts و Organizations شعبه فعلی
            if (model.BranchId > 0)
            {
                model.ContactsInitial = await GetBranchContactsAsync(model.BranchId);
                model.OrganizationsInitial = await GetBranchOrganizationsAsync(model.BranchId);
            }

            // بارگذاری سازمان‌های Contact انتخاب شده
            if (model.ContactId.HasValue)
            {
                model.ContactOrganizations = await GetContactOrganizationsAsync(model.ContactId.Value);
            }

            return model;
        }

        /// <summary>
        /// بازیابی لیست‌ها در صورت خطا
        /// </summary>
        public async Task<CRMInteractionViewModel> RepopulateCreateModelAsync(CRMInteractionViewModel model, string userId)
        {
            // دریافت لیست شعبه‌های کاربر
            var userBranches = await _context.BranchUser_Tbl
                .Where(bu => bu.UserId == userId && bu.IsActive)
                .Include(bu => bu.Branch)
                .Where(bu => bu.Branch.IsActive)
                .Select(bu => new BranchViewModel
                {
                    Id = bu.BranchId,
                    Name = bu.Branch.Name
                })
                .OrderBy(b => b.Name)
                .ToListAsync();

            model.BranchesInitial = userBranches;

            // بارگذاری Contacts و Organizations شعبه
            if (model.BranchId > 0)
            {
                model.ContactsInitial = await GetBranchContactsAsync(model.BranchId);
                model.OrganizationsInitial = await GetBranchOrganizationsAsync(model.BranchId);
            }

            return model;
        }

        #endregion

        #region Create Interaction

        /// <summary>
        /// ایجاد تعامل جدید
        /// </summary>
        public async Task<int> CreateInteractionAsync(CRMInteractionViewModel model, string userId, string webRootPath)
        {
            // بررسی یکتا بودن کد
            if (!IsCRMCodeUnique(model.CRMCode))
                throw new InvalidOperationException("کد تعامل تکراری است");

            // ایجاد Entity
            var interaction = _mapper.Map<CRMInteraction>(model);
            interaction.CreateDate = DateTime.Now;
            interaction.CreatorUserId = userId;
            interaction.IsActive = true;
            interaction.IsDeleted = false;

            _context.CRMInteraction_Tbl.Add(interaction);
            await _context.SaveChangesAsync();

            // ذخیره فایل‌های پیوست
            if (model.UploadFiles != null && model.UploadFiles.Count > 0)
            {
                await SaveAttachmentsAsync(interaction.Id, model.UploadFiles, userId, webRootPath);
            }

            return interaction.Id;
        }

        /// <summary>
        /// ذخیره فایل‌های پیوست
        /// </summary>
        private async Task SaveAttachmentsAsync(int crmInteractionId, List<IFormFile> files, string userId, string webRootPath)
        {
            string uploadsFolder = Path.Combine(webRootPath, "uploads", "crm", crmInteractionId.ToString());

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
                        await file.CopyToAsync(stream);
                    }

                    var attachment = new CRMAttachment
                    {
                        CRMInteractionId = crmInteractionId,
                        FileName = file.FileName,
                        FileSize = file.Length,
                        FilePath = $"/uploads/crm/{crmInteractionId}/{uniqueFileName}",
                        FileType = file.ContentType,
                        UploadDate = DateTime.Now,
                        UploaderUserId = userId
                    };

                    _context.CRMAttachment_Tbl.Add(attachment);
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Update Interaction

        /// <summary>
        /// به‌روزرسانی تعامل
        /// </summary>
        public async Task<bool> UpdateInteractionAsync(CRMInteractionViewModel model, string userId, string webRootPath)
        {
            var interaction = await _context.CRMInteraction_Tbl.FindAsync(model.Id);
            if (interaction == null)
                return false;

            // بررسی یکتا بودن کد
            if (!IsCRMCodeUnique(model.CRMCode, model.Id))
                throw new InvalidOperationException("کد تعامل تکراری است");

            // به‌روزرسانی فیلدها
            _mapper.Map(model, interaction);
            interaction.LastUpdateDate = DateTime.Now;
            interaction.LastUpdaterUserId = userId;

            _context.CRMInteraction_Tbl.Update(interaction);
            await _context.SaveChangesAsync();

            // ذخیره فایل‌های پیوست جدید
            if (model.UploadFiles != null && model.UploadFiles.Count > 0)
            {
                await SaveAttachmentsAsync(interaction.Id, model.UploadFiles, userId, webRootPath);
            }

            return true;
        }

        #endregion

        #region Delete & Toggle Status

        /// <summary>
        /// حذف نرم تعامل
        /// </summary>
        public async Task<(bool success, string title)> SoftDeleteInteractionAsync(int interactionId, string userId)
        {
            var interaction = await _context.CRMInteraction_Tbl.FindAsync(interactionId);
            if (interaction == null)
                return (false, "");

            interaction.IsDeleted = true;
            interaction.LastUpdateDate = DateTime.Now;
            interaction.LastUpdaterUserId = userId;

            _context.CRMInteraction_Tbl.Update(interaction);
            await _context.SaveChangesAsync();

            return (true, interaction.Title);
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال تعامل
        /// </summary>
        public async Task<(bool success, bool isActive, string title)> ToggleInteractionActiveStatusAsync(int interactionId, string userId)
        {
            var interaction = await _context.CRMInteraction_Tbl.FindAsync(interactionId);
            if (interaction == null)
                return (false, false, "");

            interaction.IsActive = !interaction.IsActive;
            interaction.LastUpdateDate = DateTime.Now;
            interaction.LastUpdaterUserId = userId;

            _context.CRMInteraction_Tbl.Update(interaction);
            await _context.SaveChangesAsync();

            return (true, interaction.IsActive, interaction.Title);
        }

        #endregion
    }
}
