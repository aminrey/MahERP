using AutoMapper;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// مدیریت مخاطبین و سازمان‌ها
    /// </summary>
    public partial class TaskRepository
    {
        #region Contacts and Organizations Management

        /// <summary>
        /// دریافت Contactهای شعبه (افراد مرتبط با شعبه)
        /// </summary>
        public async Task<List<ContactViewModel>> GetBranchContactsAsync(int branchId)
        {
            try
            {
                var contacts = await _context.BranchContact_Tbl
                    .Include(bc => bc.Contact)
                        .ThenInclude(c => c.Phones)
                    .Where(bc => bc.BranchId == branchId && bc.IsActive)
                    .Select(bc => new ContactViewModel
                    {
                        Id = bc.ContactId,
                        FirstName = bc.Contact.FirstName,
                        LastName = bc.Contact.LastName,
                        FullName = $"{bc.Contact.FirstName} {bc.Contact.LastName}",
                        NationalCode = bc.Contact.NationalCode,
                        PrimaryPhone = bc.Contact.Phones
                            .Where(p => p.IsDefault && p.IsActive)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault(),
                        RelationType = bc.RelationType,
                        IsActive = bc.Contact.IsActive
                    })
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToListAsync();

                return contacts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetBranchContactsAsync: {ex.Message}");
                return new List<ContactViewModel>();
            }
        }

        /// <summary>
        /// دریافت Organizationهای شعبه (سازمان‌های مرتبط با شعبه) - اصلاح شده
        /// </summary>
        public async Task<List<OrganizationViewModel>> GetBranchOrganizationsAsync(int branchId)
        {
            try
            {
                // ⭐⭐⭐ اصلاح شده: استفاده از Name به جای DisplayName
                var organizations = await _context.BranchOrganization_Tbl
                    .Include(bo => bo.Organization)
                        .ThenInclude(o => o.Departments)
                    .Where(bo => bo.BranchId == branchId && bo.IsActive)
                    .Select(bo => new
                    {
                        bo.OrganizationId,
                        bo.Organization.Name,
                        bo.Organization.Brand,
                        bo.Organization.RegistrationNumber,
                        bo.Organization.EconomicCode,
                        bo.Organization.IsActive,
                        Departments = bo.Organization.Departments
                            .Where(d => d.IsActive)
                            .Select(d => new
                            {
                                d.Id,
                                Members = d.Members.Where(m => m.IsActive).Select(m => m.Id)
                            })
                    })
                    .OrderBy(o => o.Name)
                    .ToListAsync();

                // ⭐⭐⭐ محاسبه DisplayName و mapping در Client-Side
                return organizations.Select(o => new OrganizationViewModel
                {
                    Id = o.OrganizationId,
                    DisplayName = !string.IsNullOrEmpty(o.Brand) ? o.Brand : o.Name,
                    Name = o.Name,
                    Brand = o.Brand,
                    RegistrationNumber = o.RegistrationNumber,
                    EconomicCode = o.EconomicCode,
                    TotalDepartments = o.Departments.Count(),
                    TotalMembers = o.Departments.SelectMany(d => d.Members).Count(),
                    IsActive = o.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetBranchOrganizationsAsync: {ex.Message}");
                return new List<OrganizationViewModel>();
            }
        }

        /// <summary>
        /// دریافت سازمان‌هایی که یک Contact در آن‌ها عضو است
        /// </summary>
        public async Task<List<OrganizationViewModel>> GetContactOrganizationsAsync(int contactId)
        {
            try
            {
                // دریافت سازمان‌هایی که Contact از طریق OrganizationContact و DepartmentMember عضو آن‌هاست
                var organizations = await _context.OrganizationContact_Tbl
                    .Include(oc => oc.Organization)
                    .Where(oc => oc.ContactId == contactId && oc.IsActive)
                    .Select(oc => new OrganizationViewModel
                    {
                        Id = oc.OrganizationId,
                        DisplayName = oc.Organization.DisplayName,
                        RegistrationNumber = oc.Organization.RegistrationNumber,
                        IsActive = oc.Organization.IsActive
                    })
                    .Distinct()
                    .OrderBy(o => o.DisplayName)
                    .ToListAsync();

                return organizations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetContactOrganizationsAsync: {ex.Message}");
                return new List<OrganizationViewModel>();
            }
        }

        /// <summary>
        /// دریافت افراد عضو یک سازمان
        /// </summary>
        public async Task<List<ContactViewModel>> GetOrganizationContactsAsync(int organizationId)
        {
            try
            {
                var contacts = await _context.OrganizationContact_Tbl
                    .Include(oc => oc.Contact)
                        .ThenInclude(c => c.Phones)
                    .Where(oc => oc.OrganizationId == organizationId && oc.IsActive)
                    .Select(oc => new ContactViewModel
                    {
                        Id = oc.ContactId,
                        FirstName = oc.Contact.FirstName,
                        LastName = oc.Contact.LastName,
                        FullName = $"{oc.Contact.FirstName} {oc.Contact.LastName}",
                        NationalCode = oc.Contact.NationalCode,
                        PrimaryPhone = oc.Contact.Phones
                            .Where(p => p.IsDefault && p.IsActive)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault(),
                        IsActive = oc.Contact.IsActive
                    })
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToListAsync();

                return contacts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetOrganizationContactsAsync: {ex.Message}");
                return new List<ContactViewModel>();
            }
        }

        /// <summary>
        /// دریافت کامنت‌های تسک
        /// </summary>
        public async Task<List<TaskCommentViewModel>> GetTaskCommentsAsync(int taskId)
        {
            try
            {
                var comments = await _context.TaskComment_Tbl
                    .Where(c => c.TaskId == taskId && c.ParentCommentId == null) // فقط کامنت‌های اصلی
                    .Include(c => c.Creator)
                    .Include(c => c.Attachments)
                    .OrderBy(c => c.CreateDate)
                    .ToListAsync();

                return _mapper.Map<List<TaskCommentViewModel>>(comments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTaskCommentsAsync: {ex.Message}");
                return new List<TaskCommentViewModel>();
            }
        }

        /// <summary>
        /// دریافت اطلاعات فایل پیوست شده به کامنت تسک برای دانلود
        /// </summary>
        public async Task<TaskCommentAttachment?> GetCommentAttachmentByIdAsync(int attachmentId)
        {
            try
            {
                return await _context.TaskCommentAttachment_Tbl
                    .Include(a => a.Comment)
                        .ThenInclude(c => c.Task)
                    .FirstOrDefaultAsync(a => a.Id == attachmentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetCommentAttachmentByIdAsync: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
