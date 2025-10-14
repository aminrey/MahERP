using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.OrganizationRepository
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AppDbContext _context;

        public OrganizationRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==================== ORGANIZATION CRUD ====================



        /// <summary>
        /// دریافت بخش با شناسه (Simple - موجود در کد فعلی)
        /// </summary>
        public OrganizationDepartment GetDepartmentById(int id)
        {
            return _context.OrganizationDepartment_Tbl
                .Include(d => d.ManagerContact)
                .Include(d => d.ParentDepartment)
                .Include(d => d.Organization)
                .Include(d => d.Positions.Where(p => p.IsActive))
                .Include(d => d.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.Contact)
                .Include(d => d.Members)
                    .ThenInclude(m => m.Position)
                .FirstOrDefault(d => d.Id == id);
        }

        /// <summary>
        /// ⭐ NEW: دریافت بخش با کنترل بارگذاری (Overload)
        /// </summary>
        public OrganizationDepartment GetDepartmentById(int id, bool includePositions = false, bool includeMembers = false)
        {
            var query = _context.OrganizationDepartment_Tbl
                .Include(d => d.ManagerContact)
                .Include(d => d.ParentDepartment)
                .Include(d => d.Organization)
                .AsQueryable();

            if (includePositions)
                query = query.Include(d => d.Positions.Where(p => p.IsActive));

            if (includeMembers)
                query = query.Include(d => d.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.Contact)
                    .Include(d => d.Members)
                    .ThenInclude(m => m.Position);

            return query.FirstOrDefault(d => d.Id == id);
        }
        public List<Organization> GetAllOrganizations(bool includeInactive = false, byte? organizationType = null)
        {
            var query = _context.Organization_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(o => o.IsActive);

            if (organizationType.HasValue)
                query = query.Where(o => o.OrganizationType == organizationType.Value);

            return query
                .OrderBy(o => o.Name)
                .ToList();
        }

        /// <summary>
        /// ⭐ اصلاح شده - حذف پارامترهای اضافی
        /// </summary>
        public Organization GetOrganizationById(int id)
        {
            return _context.Organization_Tbl
                .Include(o => o.Departments.Where(d => d.IsActive))
                .Include(o => o.Contacts.Where(c => c.IsActive))
                .FirstOrDefault(o => o.Id == id);
        }

        public async Task<Organization> GetOrganizationByIdAsync(int id, bool includeDepartments = false, bool includeContacts = false)
        {
            var query = _context.Organization_Tbl.AsQueryable();

            if (includeDepartments)
                query = query.Include(o => o.Departments.Where(d => d.IsActive))
                            .ThenInclude(d => d.Positions)
                            .Include(o => o.Departments)
                            .ThenInclude(d => d.Members);

            if (includeContacts)
                query = query.Include(o => o.Contacts.Where(c => c.IsActive))
                            .ThenInclude(c => c.Contact);

            return await query.FirstOrDefaultAsync(o => o.Id == id);
        }

        public List<Organization> SearchOrganizations(string searchTerm, byte? organizationType = null, bool includeInactive = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllOrganizations(includeInactive, organizationType);

            var query = _context.Organization_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(o => o.IsActive);

            query = query.Where(o =>
                o.Name.Contains(searchTerm) ||
                (o.Brand != null && o.Brand.Contains(searchTerm)) ||
                (o.RegistrationNumber != null && o.RegistrationNumber.Contains(searchTerm)) ||
                (o.EconomicCode != null && o.EconomicCode.Contains(searchTerm)) ||
                (o.Email != null && o.Email.Contains(searchTerm))
            );

            if (organizationType.HasValue)
                query = query.Where(o => o.OrganizationType == organizationType.Value);

            return query
                .OrderBy(o => o.Name)
                .ToList();
        }

        public bool IsRegistrationNumberUnique(string registrationNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
                return true;

            var query = _context.Organization_Tbl
                .Where(o => o.RegistrationNumber == registrationNumber && o.IsActive);

            if (excludeId.HasValue)
                query = query.Where(o => o.Id != excludeId.Value);

            return !query.Any();
        }

        public bool IsEconomicCodeUnique(string economicCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(economicCode))
                return true;

            var query = _context.Organization_Tbl
                .Where(o => o.EconomicCode == economicCode && o.IsActive);

            if (excludeId.HasValue)
                query = query.Where(o => o.Id != excludeId.Value);

            return !query.Any();
        }

        // ==================== DEPARTMENT ====================

        public List<OrganizationDepartment> GetOrganizationDepartments(int organizationId, bool includeInactive = false)
        {
            var query = _context.OrganizationDepartment_Tbl
                .Include(d => d.ManagerContact)
                .Include(d => d.Positions)
                .Include(d => d.Members)
                .Where(d => d.OrganizationId == organizationId);

            if (!includeInactive)
                query = query.Where(d => d.IsActive);

            return query
                .OrderBy(d => d.Level)
                .ThenBy(d => d.DisplayOrder)
                .ToList();
        }

        public List<OrganizationDepartment> GetRootDepartments(int organizationId)
        {
            return _context.OrganizationDepartment_Tbl
                .Include(d => d.ManagerContact)
                .Where(d => d.OrganizationId == organizationId && d.ParentDepartmentId == null && d.IsActive)
                .OrderBy(d => d.DisplayOrder)
                .ToList();
        }

        public List<OrganizationDepartment> GetChildDepartments(int parentDepartmentId)
        {
            return _context.OrganizationDepartment_Tbl
                .Include(d => d.ManagerContact)
                .Where(d => d.ParentDepartmentId == parentDepartmentId && d.IsActive)
                .OrderBy(d => d.DisplayOrder)
                .ToList();
        }

        
        /// <summary>
        /// ⭐ اصلاح شده - تغییر return type
        /// </summary>
        public async Task<OrganizationDepartment> CreateDepartmentAsync(OrganizationDepartment department)
        {
            // محاسبه سطح
            if (department.ParentDepartmentId.HasValue)
            {
                var parent = await _context.OrganizationDepartment_Tbl
                    .FindAsync(department.ParentDepartmentId.Value);
                
                if (parent != null)
                    department.Level = parent.Level + 1;
            }
            else
            {
                department.Level = 0;
            }

            department.CreatedDate = DateTime.Now;
            _context.OrganizationDepartment_Tbl.Add(department);
            await _context.SaveChangesAsync();

            return department; // ⭐ برگرداندن entity
        }

        /// <summary>
        /// ⭐ اصلاح شده - تغییر return type
        /// </summary>
        public async Task<OrganizationDepartment> UpdateDepartmentAsync(OrganizationDepartment department)
        {
            department.LastUpdateDate = DateTime.Now;
            _context.OrganizationDepartment_Tbl.Update(department);
            await _context.SaveChangesAsync();
            
            return department; // ⭐ برگرداندن entity
        }

        public async Task<bool> DeleteDepartmentAsync(int departmentId)
        {
            try
            {
                var department =  GetDepartmentById(departmentId);
                if (department == null)
                    return false;

                // بررسی وجود زیربخش
                if (HasChildDepartments(departmentId))
                    return false;

                // حذف اعضا
                if (department.Members?.Any() == true)
                {
                    _context.DepartmentMember_Tbl.RemoveRange(department.Members);
                }

                // حذف سمت‌ها
                if (department.Positions?.Any() == true)
                {
                    _context.DepartmentPosition_Tbl.RemoveRange(department.Positions);
                }

                // حذف بخش
                _context.OrganizationDepartment_Tbl.Remove(department);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool HasChildDepartments(int departmentId)
        {
            return _context.OrganizationDepartment_Tbl
                .Any(d => d.ParentDepartmentId == departmentId && d.IsActive);
        }

        // ==================== POSITION ====================

        public List<DepartmentPosition> GetDepartmentPositions(int departmentId, bool includeInactive = false)
        {
            var query = _context.DepartmentPosition_Tbl
                .Include(p => p.Members)
                .Where(p => p.DepartmentId == departmentId);

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            return query
                .OrderBy(p => p.PowerLevel)
                .ThenBy(p => p.DisplayOrder)
                .ToList();
        }

        public DepartmentPosition GetDefaultPosition(int departmentId)
        {
            return _context.DepartmentPosition_Tbl
                .FirstOrDefault(p => p.DepartmentId == departmentId && p.IsDefault && p.IsActive);
        }

        public DepartmentPosition GetPositionById(int id)
        {
            return _context.DepartmentPosition_Tbl
                .Include(p => p.Department)
                .Include(p => p.Members)
                .FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// ⭐ اصلاح شده - تغییر return type
        /// </summary>
        public async Task<DepartmentPosition> CreatePositionAsync(DepartmentPosition position)
        {
            position.CreatedDate = DateTime.Now;
            _context.DepartmentPosition_Tbl.Add(position);
            await _context.SaveChangesAsync();

            return position; // ⭐ برگرداندن entity
        }

        /// <summary>
        /// ⭐ اصلاح شده - تغییر return type
        /// </summary>
        public async Task<DepartmentPosition> UpdatePositionAsync(DepartmentPosition position)
        {
            _context.DepartmentPosition_Tbl.Update(position);
            await _context.SaveChangesAsync();
            
            return position; // ⭐ برگرداندن entity
        }

        public async Task<bool> DeletePositionAsync(int positionId)
        {
            try
            {
                var position = await _context.DepartmentPosition_Tbl.FindAsync(positionId);
                if (position == null)
                    return false;

                // بررسی وجود اعضا با این سمت
                var hasMembers = _context.DepartmentMember_Tbl
                    .Any(m => m.PositionId == positionId && m.IsActive);

                if (hasMembers)
                    return false;

                _context.DepartmentPosition_Tbl.Remove(position);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== MEMBER ====================

        public List<DepartmentMember> GetDepartmentMembers(int departmentId, bool includeInactive = false)
        {
            var query = _context.DepartmentMember_Tbl
                .Include(m => m.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                .Include(m => m.Position)
                .Where(m => m.DepartmentId == departmentId);

            if (!includeInactive)
                query = query.Where(m => m.IsActive);

            return query
                .OrderBy(m => m.Position.PowerLevel)
                .ThenBy(m => m.Contact.FirstName)
                .ToList();
        }

        public bool IsContactMemberOfDepartment(int contactId, int departmentId)
        {
            return _context.DepartmentMember_Tbl
                .Any(m => m.ContactId == contactId && m.DepartmentId == departmentId && m.IsActive);
        }

        /// <summary>
        /// افزودن عضو به بخش بدون نیاز به سمت
        /// </summary>
        public async Task<int> AddMemberToDepartmentAsync(DepartmentMember member)
        {
            // ⭐ اگر سمت مشخص نشده، سمت پیش‌فرض را انتخاب کن
            if ( member.PositionId == 0)
            {
                var defaultPosition = GetDefaultPosition(member.DepartmentId);

                // اگر سمت پیش‌فرض وجود نداشت، یک سمت عمومی ایجاد کن
                if (defaultPosition == null)
                {
                    defaultPosition = new DepartmentPosition
                    {
                        DepartmentId = member.DepartmentId,
                        Title = "عضو",
                        Description = "سمت عمومی",
                        IsDefault = true,
                        IsActive = true,
                        DisplayOrder = 999,
                        PowerLevel = 50,
                        CreatedDate = DateTime.Now,
                        CreatorUserId = member.CreatorUserId
                    };

                    _context.DepartmentPosition_Tbl.Add(defaultPosition);
                    await _context.SaveChangesAsync();
                }

                member.PositionId = defaultPosition.Id;
            }

            member.CreatedDate = DateTime.Now;
            _context.DepartmentMember_Tbl.Add(member);
            await _context.SaveChangesAsync();

            return member.Id;
        }

        public async Task<bool> RemoveMemberFromDepartmentAsync(int memberId)
        {
            try
            {
                var member = await _context.DepartmentMember_Tbl.FindAsync(memberId);
                if (member == null)
                    return false;

                _context.DepartmentMember_Tbl.Remove(member);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public DepartmentMember GetMemberById(int id)
        {
            return _context.DepartmentMember_Tbl
                .Include(m => m.Contact)
                .Include(m => m.Department)
                .Include(m => m.Position)
                .FirstOrDefault(m => m.Id == id);
        }

        // ==================== ORGANIZATION CONTACT ====================

        public List<OrganizationContact> GetOrganizationContacts(int organizationId, byte? relationType = null, bool includeInactive = false)
        {
            var query = _context.OrganizationContact_Tbl
                .Include(oc => oc.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                .Where(oc => oc.OrganizationId == organizationId);

            if (relationType.HasValue)
                query = query.Where(oc => oc.RelationType == relationType.Value);

            if (!includeInactive)
                query = query.Where(oc => oc.IsActive);

            return query
                .OrderByDescending(oc => oc.IsPrimary)
                .ThenByDescending(oc => oc.ImportanceLevel)
                .ThenBy(oc => oc.Contact.FirstName)
                .ToList();
        }

        public async Task<int> AddContactToOrganizationAsync(OrganizationContact organizationContact)
        {
            organizationContact.CreatedDate = DateTime.Now;
            _context.OrganizationContact_Tbl.Add(organizationContact);
            await _context.SaveChangesAsync();

            return organizationContact.Id;
        }

        public async Task<bool> RemoveContactFromOrganizationAsync(int organizationContactId)
        {
            try
            {
                var organizationContact = await _context.OrganizationContact_Tbl.FindAsync(organizationContactId);
                if (organizationContact == null)
                    return false;

                _context.OrganizationContact_Tbl.Remove(organizationContact);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== STATISTICS ====================

        public async Task<OrganizationStatisticsViewModel> GetOrganizationStatisticsAsync(int organizationId)
        {
            var stats = new OrganizationStatisticsViewModel
            {
                TotalDepartments = await _context.OrganizationDepartment_Tbl
                    .CountAsync(d => d.OrganizationId == organizationId && d.IsActive),
                
                TotalPositions = await _context.DepartmentPosition_Tbl
                    .CountAsync(p => p.Department.OrganizationId == organizationId && p.IsActive),
                
                TotalMembers = await _context.DepartmentMember_Tbl
                    .CountAsync(m => m.Department.OrganizationId == organizationId && m.IsActive),
                
                TotalContacts = await _context.OrganizationContact_Tbl
                    .CountAsync(oc => oc.OrganizationId == organizationId && oc.IsActive),
                
                TotalEmployees = await _context.DepartmentMember_Tbl
                    .CountAsync(m => m.Department.OrganizationId == organizationId && 
                                     m.IsActive && 
                                     m.EmploymentType == 0), // تمام‌وقت
                
                TotalCustomers = await _context.OrganizationContact_Tbl
                    .CountAsync(oc => oc.OrganizationId == organizationId && 
                                      oc.IsActive && 
                                      oc.RelationType == 1) // مشتری
            };

            return stats;
        }

        public async Task<OrganizationChartViewModel> GetOrganizationChartAsync(int organizationId)
        {
            var organization = await GetOrganizationByIdAsync(organizationId, includeDepartments: true);
            
            if (organization == null)
                return null;

            var chartViewModel = new OrganizationChartViewModel
            {
                OrganizationId = organizationId,
                OrganizationName = organization.DisplayName,
                RootDepartments = GetRootDepartments(organizationId)
            };

            return chartViewModel;
        }

        // ==================== MISSING METHODS FROM INTERFACE ====================

        /// <summary>
        /// دریافت لیست تمام سازمان‌ها (پیاده‌سازی Interface)
        /// </summary>
        public List<Organization> GetOrganizations(bool includeInactive = false)
        {
            return GetAllOrganizations(includeInactive);
        }

        /// <summary>
        /// ایجاد سازمان جدید
        /// </summary>
        public async Task<Organization> CreateOrganizationAsync(Organization organization)
        {
            try
            {
                organization.CreatedDate = DateTime.Now; // ⭐ تغییر از CreateDate به CreatedDate
                organization.IsActive = true;
                
                _context.Organization_Tbl.Add(organization);
                await _context.SaveChangesAsync();
                
                return organization;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ایجاد سازمان: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// بروزرسانی سازمان
        /// </summary>
        public async Task<Organization> UpdateOrganizationAsync(Organization organization)
        {
            try
            {
                organization.LastUpdateDate = DateTime.Now;
                
                _context.Organization_Tbl.Update(organization);
                await _context.SaveChangesAsync();
                
                return organization;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در بروزرسانی سازمان: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// حذف سازمان (Soft Delete)
        /// </summary>
        public async Task<bool> DeleteOrganizationAsync(int id)
        {
            try
            {
                var organization = await _context.Organization_Tbl.FindAsync(id);
                if (organization == null)
                    return false;

                // بررسی وجود بخش‌ها
                var hasDepartments = await _context.OrganizationDepartment_Tbl
                    .AnyAsync(d => d.OrganizationId == id && d.IsActive);

                if (hasDepartments)
                    return false; // نمی‌توان سازمان با بخش فعال را حذف کرد

                organization.IsActive = false;
                organization.LastUpdateDate = DateTime.Now;
                
                _context.Organization_Tbl.Update(organization);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// دریافت ساختار سلسله مراتبی بخش‌ها
        /// </summary>
        public async Task<List<OrganizationDepartment>> GetDepartmentHierarchyAsync(int organizationId)
        {
            var allDepartments = await _context.OrganizationDepartment_Tbl
                .Include(d => d.ManagerContact)
                .Include(d => d.ChildDepartments)
                .Where(d => d.OrganizationId == organizationId && d.IsActive)
                .ToListAsync();

            return allDepartments;
        }

        /// <summary>
        /// افزودن عضو به بخش (Alias)
        /// </summary>
        public async Task<DepartmentMember> AddMemberAsync(DepartmentMember member)
        {
            await AddMemberToDepartmentAsync(member);
            return member;
        }

        /// <summary>
        /// بروزرسانی عضو
        /// </summary>
        public async Task<DepartmentMember> UpdateMemberAsync(DepartmentMember member)
        {
            try
            {
                _context.DepartmentMember_Tbl.Update(member);
                await _context.SaveChangesAsync();
                
                return member;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در بروزرسانی عضو: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// حذف عضو از بخش (Alias)
        /// </summary>
        public async Task<bool> RemoveMemberAsync(int id)
        {
            return await RemoveMemberFromDepartmentAsync(id);
        }

        /// <summary>
        /// بررسی اینکه آیا فرد قبلاً به بخش اضافه شده
        /// </summary>
        public bool IsMemberAlreadyInDepartment(int departmentId, int contactId)
        {
            return IsContactMemberOfDepartment(contactId, departmentId);
        }

        /// <summary>
        /// دریافت روابط افراد با سازمان (Overload)
        /// </summary>
        public List<OrganizationContact> GetOrganizationContacts(int organizationId, bool includeInactive = false)
        {
            return GetOrganizationContacts(organizationId, null, includeInactive);
        }

        /// <summary>
        /// دریافت رابطه با شناسه
        /// </summary>
        public OrganizationContact GetOrganizationContactById(int id)
        {
            return _context.OrganizationContact_Tbl
                .Include(oc => oc.Contact)
                .Include(oc => oc.Organization)
                .FirstOrDefault(oc => oc.Id == id);
        }

        /// <summary>
        /// ایجاد رابطه بین فرد و سازمان (Alias)
        /// </summary>
        public async Task<OrganizationContact> CreateOrganizationContactAsync(OrganizationContact organizationContact)
        {
            await AddContactToOrganizationAsync(organizationContact);
            return organizationContact;
        }

        /// <summary>
        /// حذف رابطه (Alias)
        /// </summary>
        public async Task<bool> DeleteOrganizationContactAsync(int id)
        {
            return await RemoveContactFromOrganizationAsync(id);
        }

        /// <summary>
        /// دریافت تعداد اعضای یک سازمان
        /// </summary>
        public int GetOrganizationMembersCount(int organizationId, bool activeOnly = true)
        {
            var query = _context.DepartmentMember_Tbl
                .Where(m => m.Department.OrganizationId == organizationId);

            if (activeOnly)
                query = query.Where(m => m.IsActive);

            return query.Count();
        }

        /// <summary>
        /// دریافت تعداد بخش‌های یک سازمان
        /// </summary>
        public int GetOrganizationDepartmentsCount(int organizationId, bool activeOnly = true)
        {
            var query = _context.OrganizationDepartment_Tbl
                .Where(d => d.OrganizationId == organizationId);

            if (activeOnly)
                query = query.Where(d => d.IsActive);

            return query.Count();
        }

        /// <summary>
        /// جستجوی سازمان‌ها (Async)
        /// </summary>
        public async Task<List<Organization>> SearchOrganizationsAsync(string searchTerm, bool includeInactive = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await _context.Organization_Tbl
                    .Where(o => includeInactive || o.IsActive)
                    .OrderBy(o => o.Name)
                    .ToListAsync();

            var query = _context.Organization_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(o => o.IsActive);

            query = query.Where(o =>
                o.Name.Contains(searchTerm) ||
                (o.DisplayName != null && o.DisplayName.Contains(searchTerm)) ||
                (o.RegistrationNumber != null && o.RegistrationNumber.Contains(searchTerm)) ||
                (o.EconomicCode != null && o.EconomicCode.Contains(searchTerm)) ||
                (o.Email != null && o.Email.Contains(searchTerm))
            );

            return await query
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        /// <summary>
        /// جستجوی اعضا در سازمان
        /// </summary>
        public async Task<List<DepartmentMember>> SearchMembersInOrganizationAsync(int organizationId, string searchTerm)
        {
            var query = _context.DepartmentMember_Tbl
                .Include(m => m.Contact)
                    .ThenInclude(c => c.Phones)
                .Include(m => m.Department)
                .Include(m => m.Position)
                .Where(m => m.Department.OrganizationId == organizationId && m.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m =>
                    m.Contact.FirstName.Contains(searchTerm) ||
                    m.Contact.LastName.Contains(searchTerm) ||
                    (m.Contact.NationalCode != null && m.Contact.NationalCode.Contains(searchTerm)) ||
                    (m.Position != null && m.Position.Title.Contains(searchTerm))
                );
            }

            return await query
                .OrderBy(m => m.Contact.FirstName)
                .ThenBy(m => m.Contact.LastName)
                .ToListAsync();
        }
        /// <summary>
        /// دریافت ViewModel کامل برای جزئیات سازمان
        /// </summary>
        public async Task<OrganizationDetailsViewModel> GetOrganizationDetailsViewModelAsync(int organizationId)
        {
            var organization = await GetOrganizationByIdAsync(organizationId, includeDepartments: true, includeContacts: true);

            if (organization == null)
                return null;

            var stats = await GetOrganizationStatisticsAsync(organizationId);

            var viewModel = new OrganizationDetailsViewModel
            {
                Id = organization.Id,

                // ⭐⭐⭐ اصلاح شده: استفاده از property های صحیح
                Name = organization.Name,
                DisplayName = organization.DisplayName, // Computed property
                Brand = organization.Brand,
                LegalName = organization.Name, // برای backward compatibility

                RegistrationNumber = organization.RegistrationNumber,
                EconomicCode = organization.EconomicCode,
                RegistrationDate = organization.RegistrationDate,
                NationalId = null, // این property در Entity وجود ندارد

                PrimaryPhone = organization.PrimaryPhone,
                SecondaryPhone = organization.SecondaryPhone,
                Phone = organization.PrimaryPhone, // برای backward compatibility

                Email = organization.Email,
                Website = organization.Website,
                Address = organization.Address,
                PostalCode = organization.PostalCode,

                OrganizationType = organization.OrganizationType,
                Description = organization.Description,
                Notes = organization.Description, // برای backward compatibility

                IsActive = organization.IsActive,
                CreateDate = organization.CreatedDate, // ⭐ توجه: در Entity CreatedDate است
                LastUpdateDate = organization.LastUpdateDate,

                Departments = organization.Departments?.ToList() ?? new List<OrganizationDepartment>(),
                RelatedContacts = organization.Contacts?.ToList() ?? new List<OrganizationContact>(),

                Statistics = stats
            };

            return viewModel;
        }

        /// <summary>
        /// دریافت لیست سازمان‌ها به صورت ViewModel
        /// </summary>
        public async Task<List<OrganizationViewModel>> GetOrganizationsAsViewModelAsync(bool includeInactive = false)
        {
            var organizations = await _context.Organization_Tbl
                .Include(o => o.Departments.Where(d => d.IsActive))
                .Where(o => includeInactive || o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync();

            var viewModels = new List<OrganizationViewModel>();

            foreach (var org in organizations)
            {
                var stats = await GetOrganizationStatisticsAsync(org.Id);

                viewModels.Add(new OrganizationViewModel
                {
                    Id = org.Id,
                    Name = org.Name, // ⭐ اضافه شده
                    DisplayName = org.DisplayName, // ⭐ اکنون قابل نوشتن است
                    RegistrationNumber = org.RegistrationNumber,
                    EconomicCode = org.EconomicCode,
                    Email = org.Email,
                    IsActive = org.IsActive,
                    OrganizationType = org.OrganizationType,
                    TotalDepartments = stats.TotalDepartments,
                    TotalMembers = stats.TotalMembers
                });
            }

            return viewModels;
        }
    }
}