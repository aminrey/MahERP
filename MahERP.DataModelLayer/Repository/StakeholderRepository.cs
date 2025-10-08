using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
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

        // ========== Stakeholder Methods ==========
        public List<Stakeholder> GetStakeholders(bool includeDeleted = false, int? stakeholderType = null, byte? personType = null)
        {
            var query = _context.Stakeholder_Tbl.AsQueryable();

            if (!includeDeleted)
                query = query.Where(s => !s.IsDeleted);

            if (stakeholderType.HasValue)
                query = query.Where(s => s.StakeholderType == stakeholderType.Value);

            if (personType.HasValue)
                query = query.Where(s => s.PersonType == personType.Value);

            return query.OrderByDescending(s => s.CreateDate).ToList();
        }

        public Stakeholder GetStakeholderById(int id, bool includeCRM = false, bool includeContacts = false, 
            bool includeContracts = false, bool includeTasks = false, bool includeOrganizations = false)
        {
            var query = _context.Stakeholder_Tbl.AsQueryable();

            if (includeContacts)
                query = query.Include(s => s.StakeholderContacts.Where(c => c.IsActive));

            if (includeContracts)
                query = query.Include(s => s.Contracts);

            if (includeTasks)
                query = query.Include(s => s.TaskList);

            if (includeOrganizations)
                query = query.Include(s => s.StakeholderOrganizations)
                    .ThenInclude(o => o.Positions)
                    .Include(s => s.StakeholderOrganizations)
                    .ThenInclude(o => o.Members);

            return query.FirstOrDefault(s => s.Id == id);
        }

        public List<Stakeholder> SearchStakeholders(string searchTerm, int? stakeholderType = null, byte? personType = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetStakeholders(false, stakeholderType, personType);

            var query = _context.Stakeholder_Tbl
                .Where(s => !s.IsDeleted &&
                           (s.FirstName.Contains(searchTerm) ||
                            s.LastName.Contains(searchTerm) ||
                            s.CompanyName.Contains(searchTerm) ||
                            s.Mobile.Contains(searchTerm) ||
                            s.Phone.Contains(searchTerm) ||
                            s.Email.Contains(searchTerm) ||
                            s.NationalCode.Contains(searchTerm) ||
                            s.RegistrationNumber.Contains(searchTerm) ||
                            s.EconomicCode.Contains(searchTerm)));

            if (stakeholderType.HasValue)
                query = query.Where(s => s.StakeholderType == stakeholderType.Value);

            if (personType.HasValue)
                query = query.Where(s => s.PersonType == personType.Value);

            return query.OrderByDescending(s => s.CreateDate).ToList();
        }

        public bool IsNationalCodeUnique(string nationalCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(nationalCode))
                return true;

            var query = _context.Stakeholder_Tbl.Where(s => s.NationalCode == nationalCode && !s.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return !query.Any();
        }

        public bool IsEmailUnique(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            var query = _context.Stakeholder_Tbl.Where(s => s.Email == email && !s.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return !query.Any();
        }

        public bool IsRegistrationNumberUnique(string registrationNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
                return true;

            var query = _context.Stakeholder_Tbl.Where(s => s.RegistrationNumber == registrationNumber && !s.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return !query.Any();
        }

        public bool IsEconomicCodeUnique(string economicCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(economicCode))
                return true;

            var query = _context.Stakeholder_Tbl.Where(s => s.EconomicCode == economicCode && !s.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return !query.Any();
        }

        public List<StakeholderViewModel> GetStakeholdersByBranchId(int branchId, byte? personType = null)
        {
            var query = from stakeholder in _context.Stakeholder_Tbl
                        join stakeholderBranch in _context.StakeholderBranch_Tbl
                        on stakeholder.Id equals stakeholderBranch.StakeholderId
                        where stakeholderBranch.BranchId == branchId &&
                              !stakeholder.IsDeleted &&
                              stakeholder.IsActive
                        select stakeholder;

            if (personType.HasValue)
                query = query.Where(s => s.PersonType == personType.Value);

            var stakeholders = query.Select(s => new StakeholderViewModel
            {
                Id = s.Id,
                PersonType = s.PersonType,
                FirstName = s.FirstName,
                LastName = s.LastName,
                CompanyName = s.CompanyName,
                Mobile = s.Mobile,
                Phone = s.Phone,
                Email = s.Email
            }).ToList();

            return stakeholders;
        }

        

        // ========== StakeholderContact Methods ==========
        public List<StakeholderContact> GetStakeholderContacts(int stakeholderId, bool includeInactive = false)
        {
            var query = _context.StakeholderContact_Tbl
                .Where(c => c.StakeholderId == stakeholderId);

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            return query.OrderByDescending(c => c.IsPrimary)
                       .ThenByDescending(c => c.IsDecisionMaker)
                       .ThenBy(c => c.FirstName)
                       .ToList();
        }

        public StakeholderContact GetStakeholderContactById(int id)
        {
            return _context.StakeholderContact_Tbl.FirstOrDefault(c => c.Id == id);
        }

        public List<StakeholderContact> GetAvailableContactsForOrganization(int stakeholderId, int? excludeOrganizationId = null)
        {
            var query = _context.StakeholderContact_Tbl
                .Where(c => c.StakeholderId == stakeholderId && c.IsActive);

            if (excludeOrganizationId.HasValue)
            {
                var existingMemberContactIds = _context.StakeholderOrganizationMember_Tbl
                    .Where(m => m.OrganizationId == excludeOrganizationId.Value && m.IsActive)
                    .Select(m => m.ContactId)
                    .ToList();

                query = query.Where(c => !existingMemberContactIds.Contains(c.Id));
            }

            return query.OrderBy(c => c.FirstName).ToList();
        }

        // ========== StakeholderOrganization Methods ==========
        public List<StakeholderOrganization> GetStakeholderOrganizations(int stakeholderId, bool includeInactive = false)
        {
            var query = _context.StakeholderOrganization_Tbl
                .Include(o => o.ManagerContact)
                .Include(o => o.Members)
                .Where(o => o.StakeholderId == stakeholderId);

            if (!includeInactive)
                query = query.Where(o => o.IsActive);

            return query.OrderBy(o => o.Level)
                       .ThenBy(o => o.DisplayOrder)
                       .ToList();
        }

        public StakeholderOrganization GetStakeholderOrganizationById(int id, bool includePositions = false, bool includeMembers = false)
        {
            var query = _context.StakeholderOrganization_Tbl
                .Include(o => o.ManagerContact)
                .Include(o => o.ParentOrganization)
                .AsQueryable();

            if (includePositions)
                query = query.Include(o => o.Positions);

            if (includeMembers)
                query = query.Include(o => o.Members)
                    .ThenInclude(m => m.Contact)
                    .Include(o => o.Members)
                    .ThenInclude(m => m.Position);

            return query.FirstOrDefault(o => o.Id == id);
        }

        public List<StakeholderOrganization> GetRootOrganizations(int stakeholderId)
        {
            return _context.StakeholderOrganization_Tbl
                .Where(o => o.StakeholderId == stakeholderId && o.ParentOrganizationId == null && o.IsActive)
                .OrderBy(o => o.DisplayOrder)
                .ToList();
        }

        public List<StakeholderOrganization> GetChildOrganizations(int parentOrganizationId)
        {
            return _context.StakeholderOrganization_Tbl
                .Where(o => o.ParentOrganizationId == parentOrganizationId && o.IsActive)
                .OrderBy(o => o.DisplayOrder)
                .ToList();
        }

        public StakeholderOrganizationViewModel GetOrganizationChartData(int stakeholderId)
        {
            // این متد برای نمایش چارت سازمانی کامل استفاده می‌شود
            // می‌توانید بعداً پیاده‌سازی کنید
            throw new NotImplementedException();
        }

        // ========== StakeholderOrganizationPosition Methods ==========
        public List<StakeholderOrganizationPosition> GetOrganizationPositions(int organizationId, bool includeInactive = false)
        {
            var query = _context.StakeholderOrganizationPosition_Tbl
                .Where(p => p.OrganizationId == organizationId);

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            return query.OrderBy(p => p.PowerLevel)
                       .ThenBy(p => p.DisplayOrder)
                       .ToList();
        }

        public StakeholderOrganizationPosition GetOrganizationPositionById(int id)
        {
            return _context.StakeholderOrganizationPosition_Tbl.FirstOrDefault(p => p.Id == id);
        }

        public StakeholderOrganizationPosition GetDefaultPosition(int organizationId)
        {
            return _context.StakeholderOrganizationPosition_Tbl
                .FirstOrDefault(p => p.OrganizationId == organizationId && p.IsDefault && p.IsActive);
        }

        // ========== StakeholderOrganizationMember Methods ==========
        public List<StakeholderOrganizationMember> GetOrganizationMembers(int organizationId, bool includeInactive = false)
        {
            var query = _context.StakeholderOrganizationMember_Tbl
                .Include(m => m.Contact)
                .Include(m => m.Position)
                .Where(m => m.OrganizationId == organizationId);

            if (!includeInactive)
                query = query.Where(m => m.IsActive);

            return query.OrderBy(m => m.Position != null ? m.Position.PowerLevel : 999)
                       .ThenBy(m => m.Contact.FirstName)
                       .ToList();
        }

        public StakeholderOrganizationMember GetOrganizationMemberById(int id)
        {
            return _context.StakeholderOrganizationMember_Tbl
                .Include(m => m.Contact)
                .Include(m => m.Position)
                .Include(m => m.Organization)
                .FirstOrDefault(m => m.Id == id);
        }

        public List<StakeholderOrganizationMember> GetContactMemberships(int contactId)
        {
            return _context.StakeholderOrganizationMember_Tbl
                .Include(m => m.Organization)
                .Include(m => m.Position)
                .Where(m => m.ContactId == contactId && m.IsActive)
                .ToList();
        }

        public bool IsContactAlreadyMember(int organizationId, int contactId)
        {
            return _context.StakeholderOrganizationMember_Tbl
                .Any(m => m.OrganizationId == organizationId && m.ContactId == contactId && m.IsActive);
        }

        /// <summary>
        /// ایجاد واحد سازمانی جدید
        /// </summary>
        public int CreateOrganization(StakeholderOrganization organization)
        {
            // بررسی null بودن Title
            if (string.IsNullOrWhiteSpace(organization.Title))
            {
                throw new ArgumentException("عنوان واحد سازمانی الزامی است", nameof(organization.Title));
            }

            // بررسی CreatorUserId
            if (string.IsNullOrWhiteSpace(organization.CreatorUserId))
            {
                throw new ArgumentException("شناسه کاربر سازنده الزامی است", nameof(organization.CreatorUserId));
            }

            // بررسی StakeholderId
            if (organization.StakeholderId <= 0)
            {
                throw new ArgumentException("شناسه طرف حساب نامعتبر است", nameof(organization.StakeholderId));
            }

            // بررسی وجود Stakeholder در دیتابیس
            var stakeholderExists = _context.Stakeholder_Tbl
                .Any(s => s.Id == organization.StakeholderId && !s.IsDeleted);

            if (!stakeholderExists)
            {
                throw new InvalidOperationException($"طرف حساب با شناسه {organization.StakeholderId} یافت نشد یا حذف شده است");
            }

            // بررسی اینکه طرف حساب باید شخص حقوقی باشد
            var stakeholder = _context.Stakeholder_Tbl
                .FirstOrDefault(s => s.Id == organization.StakeholderId);

            if (stakeholder?.PersonType != 1)
            {
                throw new InvalidOperationException("چارت سازمانی فقط برای اشخاص حقوقی قابل ایجاد است");
            }

            // محاسبه سطح سازمانی
            if (organization.ParentOrganizationId.HasValue)
            {
                var parent = GetStakeholderOrganizationById(organization.ParentOrganizationId.Value);
                if (parent == null)
                {
                    throw new InvalidOperationException("واحد سازمانی والد یافت نشد");
                }
                organization.Level = parent.Level + 1;
            }
            else
            {
                organization.Level = 0;
            }

            // تنظیم تاریخ ایجاد
            organization.CreateDate = DateTime.Now;

            // افزودن به دیتابیس
            _context.StakeholderOrganization_Tbl.Add(organization);
            _context.SaveChanges();

            return organization.Id;
        }

        /// <summary>
        /// به‌روزرسانی واحد سازمانی
        /// </summary>
        public void UpdateOrganization(StakeholderOrganization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            var existing = _context.StakeholderOrganization_Tbl
                .FirstOrDefault(o => o.Id == organization.Id);

            if (existing == null)
            {
                throw new InvalidOperationException("واحد سازمانی یافت نشد");
            }

            // به‌روزرسانی فقط فیلدهای مجاز
            existing.Title = organization.Title;
            existing.Description = organization.Description;
            existing.ManagerContactId = organization.ManagerContactId;
            existing.DisplayOrder = organization.DisplayOrder;
            existing.IsActive = organization.IsActive;
            existing.LastUpdateDate = DateTime.Now;
            existing.LastUpdaterUserId = organization.LastUpdaterUserId;

            _context.SaveChanges();
        }
    }
}