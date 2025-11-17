using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.ContactRepository
{
    public class ContactRepository : IContactRepository
    {
        private readonly AppDbContext _context;

        public ContactRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==================== CONTACT CRUD ====================

        public List<Contact> GetAllContacts(bool includeInactive = false)
        {
            var query = _context.Contact_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            return query
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToList();
        }

        public Contact GetContactById(int id, bool includePhones = false, bool includeDepartments = false, bool includeOrganizations = false)
        {
            var query = _context.Contact_Tbl.AsQueryable();

            if (includePhones)
                query = query.Include(c => c.Phones.Where(p => p.IsActive));

            if (includeDepartments)
                query = query.Include(c => c.DepartmentMemberships.Where(dm => dm.IsActive))
                            .ThenInclude(dm => dm.Department)
                            .Include(c => c.DepartmentMemberships)
                            .ThenInclude(dm => dm.Position);

            if (includeOrganizations)
                query = query.Include(c => c.OrganizationRelations.Where(or => or.IsActive))
                            .ThenInclude(or => or.Organization);

            return query.FirstOrDefault(c => c.Id == id);
        }

        public async Task<Contact> GetContactByIdAsync(int id, bool includePhones = false, bool includeDepartments = false, bool includeOrganizations = false)
        {
            var query = _context.Contact_Tbl.AsQueryable();

            if (includePhones)
                query = query.Include(c => c.Phones.Where(p => p.IsActive));

            if (includeDepartments)
                query = query.Include(c => c.DepartmentMemberships.Where(dm => dm.IsActive))
                            .ThenInclude(dm => dm.Department)
                            .Include(c => c.DepartmentMemberships)
                            .ThenInclude(dm => dm.Position);

            if (includeOrganizations)
                query = query.Include(c => c.OrganizationRelations.Where(or => or.IsActive))
                            .ThenInclude(or => or.Organization);

            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public List<Contact> SearchContacts(string searchTerm, byte? gender = null, bool includeInactive = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllContacts(includeInactive);

            var query = _context.Contact_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            query = query.Where(c =>
                c.FirstName.Contains(searchTerm) ||
                c.LastName.Contains(searchTerm) ||
                (c.NationalCode != null && c.NationalCode.Contains(searchTerm)) ||
                (c.PrimaryEmail != null && c.PrimaryEmail.Contains(searchTerm)) ||
                c.Phones.Any(p => p.PhoneNumber.Contains(searchTerm))
            );

            if (gender.HasValue)
                query = query.Where(c => c.Gender == gender.Value);

            return query
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToList();
        }

        public bool IsNationalCodeUnique(string nationalCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(nationalCode))
                return true;

            var query = _context.Contact_Tbl.Where(c => c.NationalCode == nationalCode && c.IsActive);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return !query.Any();
        }

        public bool IsPrimaryEmailUnique(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            var query = _context.Contact_Tbl.Where(c => c.PrimaryEmail == email && c.IsActive);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return !query.Any();
        }

        public Contact GetContactByNationalCode(string nationalCode)
        {
            return _context.Contact_Tbl
                .FirstOrDefault(c => c.NationalCode == nationalCode && c.IsActive);
        }

        // ==================== CONTACT PHONE ====================

        public List<ContactPhone> GetContactPhones(int contactId, bool includeInactive = false)
        {
            var query = _context.ContactPhone_Tbl
                .Where(cp => cp.ContactId == contactId);

            if (!includeInactive)
                query = query.Where(cp => cp.IsActive);

            return query
                .OrderByDescending(cp => cp.IsDefault)
                .ThenBy(cp => cp.DisplayOrder)
                .ToList();
        }

        public ContactPhone GetDefaultPhone(int contactId)
        {
            return _context.ContactPhone_Tbl
                .FirstOrDefault(cp => cp.ContactId == contactId && cp.IsDefault && cp.IsActive);
        }

        /// <summary>
        /// دریافت شماره پیش‌فرض پیامک یک فرد
        /// </summary>
        public ContactPhone GetSmsDefaultPhone(int contactId)
        {
            return _context.ContactPhone_Tbl
                .FirstOrDefault(cp => cp.ContactId == contactId && cp.IsSmsDefault && cp.IsActive);
        }

        public async Task<bool> SetDefaultPhoneAsync(int phoneId, int contactId)
        {
            try
            {
                // حذف پیش‌فرض قبلی
                var previousDefault = _context.ContactPhone_Tbl
                    .Where(cp => cp.ContactId == contactId && cp.IsDefault)
                    .ToList();

                foreach (var phone in previousDefault)
                {
                    phone.IsDefault = false;
                }

                // تنظیم پیش‌فرض جدید
                var newDefault = await _context.ContactPhone_Tbl.FindAsync(phoneId);
                if (newDefault != null && newDefault.ContactId == contactId)
                {
                    newDefault.IsDefault = true;
                     _context.ContactPhone_Tbl.Update(newDefault);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// تنظیم شماره به عنوان پیش‌فرض پیامک
        /// </summary>
        public async Task<bool> SetSmsDefaultPhoneAsync(int phoneId, int contactId)
        {
            try
            {
                // حذف پیش‌فرض پیامک قبلی
                var previousSmsDefault = _context.ContactPhone_Tbl
                    .Where(cp => cp.ContactId == contactId && cp.IsSmsDefault)
                    .ToList();

                foreach (var phone in previousSmsDefault)
                {
                    phone.IsSmsDefault = false;
                }

                // تنظیم پیش‌فرض جدید
                var newSmsDefault = await _context.ContactPhone_Tbl.FindAsync(phoneId);
                if (newSmsDefault != null && newSmsDefault.ContactId == contactId)
                {
                    newSmsDefault.IsSmsDefault = true;
                    _context.ContactPhone_Tbl.Update(newSmsDefault);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsPhoneNumberExists(string phoneNumber, int? excludeId = null)
        {
            var query = _context.ContactPhone_Tbl.Where(cp => cp.PhoneNumber == phoneNumber && cp.IsActive);

            if (excludeId.HasValue)
                query = query.Where(cp => cp.Id != excludeId.Value);

            return query.Any();
        }

        public async Task<bool> DeletePhoneAsync(int phoneId)
        {
            try
            {
                var phone = await _context.ContactPhone_Tbl.FindAsync(phoneId);
                if (phone == null)
                    return false;

                // اگر پیش‌فرض بود، اولین شماره دیگر را پیش‌فرض کن
                if (phone.IsDefault)
                {
                    var nextPhone = _context.ContactPhone_Tbl
                        .Where(cp => cp.ContactId == phone.ContactId && cp.Id != phoneId && cp.IsActive)
                        .OrderBy(cp => cp.DisplayOrder)
                        .FirstOrDefault();

                    if (nextPhone != null)
                        nextPhone.IsDefault = true;
                }

                _context.ContactPhone_Tbl.Remove(phone);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== STATISTICS ====================

        public async Task<ContactStatisticsViewModel> GetContactStatisticsAsync()
        {
            var stats = new ContactStatisticsViewModel
            {
                TotalContacts = await _context.Contact_Tbl.CountAsync(c => c.IsActive),
                TotalMale = await _context.Contact_Tbl.CountAsync(c => c.IsActive && c.Gender == 0),
                TotalFemale = await _context.Contact_Tbl.CountAsync(c => c.IsActive && c.Gender == 1),
                TotalWithEmail = await _context.Contact_Tbl.CountAsync(c => c.IsActive && !string.IsNullOrEmpty(c.PrimaryEmail)),
                TotalWithPhone = await _context.Contact_Tbl.CountAsync(c => c.IsActive && c.Phones.Any(p => p.IsActive)),
                TotalInDepartments = await _context.DepartmentMember_Tbl.CountAsync(dm => dm.IsActive),
                TotalInOrganizations = await _context.OrganizationContact_Tbl.CountAsync(oc => oc.IsActive)
            };

            return stats;
        }

        public List<Contact> GetUpcomingBirthdays(int daysAhead = 30)
        {
            var today = DateTime.Today;
            var endDate = today.AddDays(daysAhead);

            return _context.Contact_Tbl
                .Where(c => c.IsActive && c.BirthDate.HasValue)
                .AsEnumerable()
                .Where(c =>
                {
                    var birthDate = c.BirthDate.Value;
                    var nextBirthday = new DateTime(today.Year, birthDate.Month, birthDate.Day);
                    
                    if (nextBirthday < today)
                        nextBirthday = nextBirthday.AddYears(1);

                    return nextBirthday >= today && nextBirthday <= endDate;
                })
                .OrderBy(c =>
                {
                    var birthDate = c.BirthDate.Value;
                    var nextBirthday = new DateTime(today.Year, birthDate.Month, birthDate.Day);
                    if (nextBirthday < today)
                        nextBirthday = nextBirthday.AddYears(1);
                    return nextBirthday;
                })
                .ToList();
        }

        // ==================== DEPARTMENT MEMBERSHIPS ====================

        public List<DepartmentMember> GetContactDepartmentMemberships(int contactId, bool includeInactive = false)
        {
            var query = _context.DepartmentMember_Tbl
                .Include(dm => dm.Department)
                    .ThenInclude(d => d.Organization)
                .Include(dm => dm.Position)
                .Where(dm => dm.ContactId == contactId);

            if (!includeInactive)
                query = query.Where(dm => dm.IsActive);

            return query
                .OrderByDescending(dm => dm.JoinDate)
                .ToList();
        }

        public bool IsContactMemberOfDepartment(int contactId, int departmentId)
        {
            return _context.DepartmentMember_Tbl
                .Any(dm => dm.ContactId == contactId && dm.DepartmentId == departmentId && dm.IsActive);
        }

        // ==================== ORGANIZATION RELATIONS ====================

        public List<OrganizationContact> GetContactOrganizationRelations(int contactId, bool includeInactive = false)
        {
            var query = _context.OrganizationContact_Tbl
                .Include(oc => oc.Organization)
                .Where(oc => oc.ContactId == contactId);

            if (!includeInactive)
                query = query.Where(oc => oc.IsActive);

            return query
                .OrderByDescending(oc => oc.IsPrimary)
                .ThenBy(oc => oc.Organization.Name)
                .ToList();
        }
    }
}