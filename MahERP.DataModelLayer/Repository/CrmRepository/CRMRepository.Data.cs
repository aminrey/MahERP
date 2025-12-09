using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// CRM Repository - Branch & Stakeholder Data
    /// </summary>
    public partial class CRMRepository
    {
        #region Branch Data

        /// <summary>
        /// دریافت Contacts و Organizations یک شعبه
        /// </summary>
        public async Task<(List<ContactViewModel> contacts, List<OrganizationViewModel> organizations)> GetBranchDataAsync(int branchId)
        {
            var contacts = await GetBranchContactsAsync(branchId);
            var organizations = await GetBranchOrganizationsAsync(branchId);

            return (contacts, organizations);
        }

        /// <summary>
        /// دریافت Contacts یک شعبه
        /// </summary>
        public async Task<List<ContactViewModel>> GetBranchContactsAsync(int branchId)
        {
            try
            {
                var branchContacts = await _context.BranchContact_Tbl
                    .Where(bc => bc.BranchId == branchId && bc.IsActive)
                    .Include(bc => bc.Contact)
                        .ThenInclude(c => c.Phones.Where(p => p.IsActive)) // ⭐ بارگذاری شماره‌ها
                    .Where(bc => bc.Contact != null && bc.Contact.IsActive)
                    .Select(bc => new ContactViewModel
                    {
                        Id = bc.ContactId,
                        FirstName = bc.Contact.FirstName ?? "",
                        LastName = bc.Contact.LastName,
                        NationalCode = bc.Contact.NationalCode ?? "",
                        // ⭐⭐⭐ دریافت شماره پیش‌فرض از Phones collection
                        PrimaryPhone = bc.Contact.Phones
                            .Where(p => p.IsDefault && p.IsActive)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault() ?? ""
                    })
                    .OrderBy(c => c.LastName)
                    .ToListAsync();

                return branchContacts;
            }
            catch
            {
                return new List<ContactViewModel>();
            }
        }

        /// <summary>
        /// دریافت Organizations یک شعبه
        /// </summary>
        public async Task<List<OrganizationViewModel>> GetBranchOrganizationsAsync(int branchId)
        {
            try
            {
                var branchOrganizations = await _context.BranchOrganization_Tbl
                    .Where(bo => bo.BranchId == branchId && bo.IsActive)
                    .Include(bo => bo.Organization)
                        .ThenInclude(o => o.Phones.Where(p => p.IsActive)) // ⭐ بارگذاری شماره‌ها
                    .Where(bo => bo.Organization != null && bo.Organization.IsActive)
                    .Select(bo => new OrganizationViewModel
                    {
                        Id = bo.OrganizationId,
                        Name = bo.Organization.Name,
                        RegistrationNumber = bo.Organization.RegistrationNumber ?? "",
                        // ⭐⭐⭐ دریافت شماره پیش‌فرض از Phones collection
                        PrimaryPhone = bo.Organization.Phones
                            .Where(p => p.IsDefault && p.IsActive)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault() ?? "",
                        Email = bo.Organization.Email ?? ""
                    })
                    .OrderBy(o => o.Name)
                    .ToListAsync();

                return branchOrganizations;
            }
            catch
            {
                return new List<OrganizationViewModel>();
            }
        }

        #endregion

        #region Contact & Organization Relations

        /// <summary>
        /// دریافت سازمان‌های مرتبط با یک Contact
        /// </summary>
        public async Task<List<OrganizationViewModel>> GetContactOrganizationsAsync(int contactId)
        {
            try
            {
                var organizations = await _context.OrganizationContact_Tbl
                    .Where(om => om.ContactId == contactId && om.IsActive)
                    .Include(om => om.Organization)
                        .ThenInclude(o => o.Phones.Where(p => p.IsActive)) // ⭐ بارگذاری شماره‌ها
                    .Where(om => om.Organization != null && om.Organization.IsActive)
                    .Select(om => new OrganizationViewModel
                    {
                        Id = om.OrganizationId,
                        Name = om.Organization.Name,
                        RegistrationNumber = om.Organization.RegistrationNumber ?? "",
                        // ⭐⭐⭐ دریافت شماره پیش‌فرض از Phones collection
                        PrimaryPhone = om.Organization.Phones
                            .Where(p => p.IsDefault && p.IsActive)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault() ?? "",
                        Email = om.Organization.Email ?? ""
                    })
                    .Distinct()
                    .OrderBy(o => o.Name)
                    .ToListAsync();

                return organizations;
            }
            catch
            {
                return new List<OrganizationViewModel>();
            }
        }

        /// <summary>
        /// دریافت اعضای یک Organization
        /// </summary>
        public async Task<List<ContactViewModel>> GetOrganizationContactsAsync(int organizationId)
        {
            try
            {
                var contacts = await _context.OrganizationContact_Tbl
                    .Where(om => om.OrganizationId == organizationId && om.IsActive)
                    .Include(om => om.Contact)
                        .ThenInclude(c => c.Phones.Where(p => p.IsActive)) // ⭐ بارگذاری شماره‌ها
                    .Where(om => om.Contact != null && om.Contact.IsActive)
                    .Select(om => new ContactViewModel
                    {
                        Id = om.ContactId,
                        FirstName = om.Contact.FirstName ?? "",
                        LastName = om.Contact.LastName,
                        NationalCode = om.Contact.NationalCode ?? "",
                        // ⭐⭐⭐ دریافت شماره پیش‌فرض از Phones collection
                        PrimaryPhone = om.Contact.Phones
                            .Where(p => p.IsDefault && p.IsActive)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault() ?? ""
                    })
                    .Distinct()
                    .OrderBy(c => c.LastName)
                    .ToListAsync();

                return contacts;
            }
            catch
            {
                return new List<ContactViewModel>();
            }
        }

        #endregion
    }
}
