using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// CRM Repository - Quick Add Operations
    /// </summary>
    public partial class CRMRepository
    {
        #region Contact Quick Add

        /// <summary>
        /// دریافت Contact بر اساس کد ملی
        /// </summary>
        public async Task<Contact?> GetContactByNationalCodeAsync(string nationalCode)
        {
            if (string.IsNullOrEmpty(nationalCode))
                return null;

            return await _context.Contact_Tbl
                .FirstOrDefaultAsync(c => c.NationalCode == nationalCode && c.IsActive);
        }

        /// <summary>
        /// ایجاد Contact همراه با شماره تلفن
        /// </summary>
        public async Task<int> CreateContactWithPhoneAsync(Contact contact, string phoneNumber, string userId)
        {
            // ایجاد Contact
            _context.Contact_Tbl.Add(contact);
            await _context.SaveChangesAsync();

            // ایجاد شماره تلفن
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                var phone = new ContactPhone
                {
                    ContactId = contact.Id,
                    PhoneNumber = phoneNumber,
                    PhoneType = 0, // موبایل
                    IsDefault = true,
                    IsSmsDefault = true,
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = userId
                };

                _context.ContactPhone_Tbl.Add(phone);
                await _context.SaveChangesAsync();
            }

            return contact.Id;
        }

        /// <summary>
        /// اتصال Contact به شعبه
        /// </summary>
        public async Task AssignContactToBranchAsync(int contactId, int branchId, string userId)
        {
            if (branchId <= 0 || contactId <= 0)
                return;

            // بررسی اینکه قبلاً اتصال وجود ندارد
            var existingAssignment = await _context.BranchContact_Tbl
                .FirstOrDefaultAsync(bc => bc.BranchId == branchId && bc.ContactId == contactId);

            if (existingAssignment != null)
            {
                // اگر غیرفعال است، فعال کن
                if (!existingAssignment.IsActive)
                {
                    existingAssignment.IsActive = true;
                    existingAssignment.AssignDate = DateTime.Now;
                    existingAssignment.AssignedByUserId = userId;
                    await _context.SaveChangesAsync();
                }
                return;
            }

            // ایجاد اتصال جدید
            var branchContact = new BranchContact
            {
                BranchId = branchId,
                ContactId = contactId,
                IsActive = true,
                AssignDate = DateTime.Now,
                AssignedByUserId = userId
            };

            _context.BranchContact_Tbl.Add(branchContact);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Organization Quick Add

        /// <summary>
        /// دریافت Organization بر اساس شماره ثبت
        /// </summary>
        public async Task<Organization?> GetOrganizationByRegistrationNumberAsync(string registrationNumber)
        {
            if (string.IsNullOrEmpty(registrationNumber))
                return null;

            return await _context.Organization_Tbl
                .FirstOrDefaultAsync(o => o.RegistrationNumber == registrationNumber && o.IsActive);
        }

        /// <summary>
        /// ایجاد Organization همراه با شماره تلفن
        /// </summary>
        public async Task<int> CreateOrganizationWithPhoneAsync(Organization organization, string phoneNumber, string userId)
        {
            // ایجاد Organization
            _context.Organization_Tbl.Add(organization);
            await _context.SaveChangesAsync();

            // ایجاد شماره تلفن
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                var phone = new OrganizationPhone
                {
                    OrganizationId = organization.Id,
                    PhoneNumber = phoneNumber,
                    PhoneType = 1, // ثابت
                    IsDefault = true,
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTime.Now,
                    CreatorUserId = userId
                };

                _context.OrganizationPhone_Tbl.Add(phone);
                await _context.SaveChangesAsync();
            }

            return organization.Id;
        }

        /// <summary>
        /// اتصال Organization به شعبه
        /// </summary>
        public async Task AssignOrganizationToBranchAsync(int organizationId, int branchId, string userId)
        {
            if (branchId <= 0 || organizationId <= 0)
                return;

            // بررسی اینکه قبلاً اتصال وجود ندارد
            var existingAssignment = await _context.BranchOrganization_Tbl
                .FirstOrDefaultAsync(bo => bo.BranchId == branchId && bo.OrganizationId == organizationId);

            if (existingAssignment != null)
            {
                // اگر غیرفعال است، فعال کن
                if (!existingAssignment.IsActive)
                {
                    existingAssignment.IsActive = true;
                    existingAssignment.AssignDate = DateTime.Now;
                    existingAssignment.AssignedByUserId = userId;
                    await _context.SaveChangesAsync();
                }
                return;
            }

            // ایجاد اتصال جدید
            var branchOrg = new BranchOrganization
            {
                BranchId = branchId,
                OrganizationId = organizationId,
                IsActive = true,
                AssignDate = DateTime.Now,
                AssignedByUserId = userId
            };

            _context.BranchOrganization_Tbl.Add(branchOrg);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// ⭐⭐⭐ دریافت یا ایجاد بخش پیش‌فرض "بدون بخش" برای سازمان
        /// </summary>
        public async Task<OrganizationDepartment?> GetOrCreateDefaultDepartmentAsync(int organizationId, string userId)
        {
            if (organizationId <= 0)
                return null;

            // بررسی وجود بخش پیش‌فرض
            var defaultDepartment = await _context.OrganizationDepartment_Tbl
                .FirstOrDefaultAsync(d => 
                    d.OrganizationId == organizationId && 
                    d.Code == "DEFAULT" && 
                    d.IsActive);

            if (defaultDepartment != null)
            {
                return defaultDepartment;
            }

            // ایجاد بخش پیش‌فرض
            defaultDepartment = new OrganizationDepartment
            {
                OrganizationId = organizationId,
                Title = "بدون بخش",
                Code = "DEFAULT",
                Description = "بخش پیش‌فرض برای اعضای بدون بخش مشخص",
                IsActive = true,
                DisplayOrder = 0,
                CreatedDate = DateTime.Now,
                CreatorUserId = userId
            };

            _context.OrganizationDepartment_Tbl.Add(defaultDepartment);
            await _context.SaveChangesAsync();

            return defaultDepartment;
        }

        #endregion

        #region Get Organization Members

        /// <summary>
        /// دریافت اعضای سازمان با جزئیات (سمت، بخش، شماره تلفن)
        /// </summary>
        public async Task<List<OrganizationMemberViewModel>> GetOrganizationMembersWithDetailsAsync(int organizationId)
        {
            if (organizationId <= 0)
                return new List<OrganizationMemberViewModel>();

            // ⭐⭐⭐ دریافت اعضای سازمان از DepartmentMember (ساختار صحیح)
            var members = await _context.DepartmentMember_Tbl
                .Where(dm => dm.Department.OrganizationId == organizationId && dm.IsActive)
                .Include(dm => dm.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsDefault && p.IsActive))
                .Include(dm => dm.Department)
                .Include(dm => dm.Position) // ⭐ ساده‌سازی شده
                .Where(dm => dm.Contact != null && dm.Contact.IsActive)
                .Select(dm => new OrganizationMemberViewModel
                {
                    Id = dm.Id,
                    ContactId = dm.ContactId,
                    ContactFullName = dm.Contact.FirstName + " " + dm.Contact.LastName,
                    ContactNationalCode = dm.Contact.NationalCode,
                    ContactPhone = dm.Contact.Phones
                        .Where(p => p.IsDefault && p.IsActive)
                        .Select(p => p.PhoneNumber)
                        .FirstOrDefault(),
                    OrganizationId = dm.Department.OrganizationId,
                    OrganizationName = dm.Department.Organization.Name,
                    DepartmentId = dm.DepartmentId,
                    DepartmentName = dm.Department.Title,
                    PositionId = dm.PositionId,
                    // ⭐⭐⭐ استفاده مستقیم از Title سمت
                    PositionTitle = dm.Position != null ? dm.Position.Title : null,
                    RelationType = dm.IsSupervisor ? (byte)1 : (byte)0, // 1=مدیر, 0=کارمند
                    IsPrimary = dm.IsSupervisor,
                    IsActive = dm.IsActive
                })
                .OrderByDescending(m => m.IsPrimary)
                .ThenBy(m => m.ContactFullName)
                .ToListAsync();

            // ⭐ اگر هیچ عضوی از DepartmentMember پیدا نشد، از OrganizationContact استفاده کن
            if (!members.Any())
            {
                members = await _context.OrganizationContact_Tbl
                    .Where(oc => oc.OrganizationId == organizationId && oc.IsActive)
                    .Include(oc => oc.Contact)
                        .ThenInclude(c => c.Phones.Where(p => p.IsDefault && p.IsActive))
                    .Include(oc => oc.Organization)
                    .Where(oc => oc.Contact != null && oc.Contact.IsActive)
                    .Select(oc => new OrganizationMemberViewModel
                    {
                        Id = oc.Id,
                        ContactId = oc.ContactId,
                        ContactFullName = oc.Contact.FirstName + " " + oc.Contact.LastName,
                        ContactNationalCode = oc.Contact.NationalCode,
                        ContactPhone = oc.Contact.Phones
                            .Where(p => p.IsDefault && p.IsActive)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault(),
                        OrganizationId = oc.OrganizationId,
                        OrganizationName = oc.Organization.Name,
                        DepartmentId = null,
                        DepartmentName = oc.Department, // فیلد متنی در OrganizationContact
                        PositionId = null,
                        PositionTitle = oc.JobTitle, // ⭐ JobTitle از OrganizationContact
                        RelationType = oc.RelationType,
                        IsPrimary = oc.IsPrimary,
                        IsActive = oc.IsActive
                    })
                    .OrderByDescending(m => m.IsPrimary)
                    .ThenBy(m => m.ContactFullName)
                    .ToListAsync();
            }

            return members;
        }

        #endregion
    }
}
