using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
    public class StakeholderRepository : IStakeholderRepository
    {
        private readonly AppDbContext _context;

        public StakeholderRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Stakeholder> GetStakeholders(bool includeDeleted = false, int? stakeholderType = null)
        {
            var query = _context.Stakeholder_Tbl.AsQueryable();

            if (!includeDeleted)
                query = query.Where(s => !s.IsDeleted);

            if (stakeholderType.HasValue)
                query = query.Where(s => s.StakeholderType == stakeholderType.Value);

            return query.OrderByDescending(s => s.CreateDate).ToList();
        }
        public List<StakeholderViewModel> GetStakeholdersByBranchId(int BranchId)
        {
            // دریافت طرف حساب‌های مرتبط با شعبه از طریق جدول StakeholderBranch
            var stakeholdersInBranch = (from stakeholder in _context.Stakeholder_Tbl
                                       join stakeholderBranch in _context.StakeholderBranch_Tbl
                                       on stakeholder.Id equals stakeholderBranch.StakeholderId
                                       where stakeholderBranch.BranchId == BranchId && 
                                             !stakeholder.IsDeleted && 
                                             stakeholder.IsActive
                                       select new StakeholderViewModel
                                       {
                                           Id = stakeholder.Id,
                                           FirstName = stakeholder.FirstName,
                                           LastName = stakeholder.LastName,
                                           CompanyName = stakeholder.CompanyName,
                                           Mobile = stakeholder.Mobile,
                                           Phone = stakeholder.Phone,
                                           Email = stakeholder.Email
                                       }).ToList();

            return stakeholdersInBranch;
        }
        

        public Stakeholder GetStakeholderById(int id, bool includeCRM = false, bool includeContacts = false, bool includeContracts = false, bool includeTasks = false)
        {
            var query = _context.Stakeholder_Tbl.AsQueryable();

            if (includeCRM)
            {
                var crmData = _context.StakeholderCRM_Tbl.FirstOrDefault(c => c.StakeholderId == id);
                // StakeholderCRM را در اینجا پیوست نمی‌کنیم چون در مدل ما ارتباط یک طرفه است
            }

            if (includeContacts)
                query = query.Include(s => s.StakeholderContacts);

            if (includeContracts)
                query = query.Include(s => s.Contracts);

            if (includeTasks)
                query = query.Include(s => s.TaskList);

            return query.FirstOrDefault(s => s.Id == id);
        }

        public StakeholderCRM GetStakeholderCRMById(int stakeholderId)
        {
            return _context.StakeholderCRM_Tbl.FirstOrDefault(c => c.StakeholderId == stakeholderId);
        }

        public List<Stakeholder> SearchStakeholders(string searchTerm, int? stakeholderType = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetStakeholders(false, stakeholderType);

            var query = _context.Stakeholder_Tbl
                .Where(s => !s.IsDeleted &&
                           (s.FirstName.Contains(searchTerm) ||
                            s.LastName.Contains(searchTerm) ||
                            s.CompanyName.Contains(searchTerm) ||
                            s.Mobile.Contains(searchTerm) ||
                            s.Phone.Contains(searchTerm) ||
                            s.Email.Contains(searchTerm) ||
                            s.NationalCode.Contains(searchTerm)));

            if (stakeholderType.HasValue)
                query = query.Where(s => s.StakeholderType == stakeholderType.Value);

            return query.OrderByDescending(s => s.CreateDate).ToList();
        }

        public bool IsNationalCodeUnique(string nationalCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(nationalCode))
                return true;

            var query = _context.Stakeholder_Tbl.Where(s => s.NationalCode == nationalCode);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return !query.Any();
        }

        public bool IsEmailUnique(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            var query = _context.Stakeholder_Tbl.Where(s => s.Email == email);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return !query.Any();
        }

        public List<StakeholderContact> GetStakeholderContacts(int stakeholderId, bool includeInactive = false)
        {
            var query = _context.StakeholderContact_Tbl
                .Where(c => c.StakeholderId == stakeholderId);
            
            if (!includeInactive)
                query = query.Where(c => c.IsActive);
            
            return query.OrderByDescending(c => c.IsPrimary).ThenBy(c => c.FirstName).ToList();
        }

        public StakeholderContact GetStakeholderContactById(int id)
        {
            return _context.StakeholderContact_Tbl.FirstOrDefault(c => c.Id == id);
        }

        public List<Stakeholder> SearchAdvanced(StakeholderSearchViewModel model)
        {
            // پیاده‌سازی جستجوی پیشرفته
            var query = _context.Stakeholder_Tbl.AsQueryable();
            
            // اضافه کردن فیلترها مشابه متد Search در کنترلر...
            
            return query.OrderByDescending(s => s.CreateDate).ToList();
        }
    }
}