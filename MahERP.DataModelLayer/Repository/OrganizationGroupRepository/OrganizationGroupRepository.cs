using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.OrganizationGroupRepository
{
    public class OrganizationGroupRepository : IOrganizationGroupRepository
    {
        private readonly AppDbContext _context;

        public OrganizationGroupRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==================== ORGANIZATION GROUP (SYSTEM LEVEL) ====================

        public List<OrganizationGroup> GetAllGroups(bool includeInactive = false)
        {
            var query = _context.OrganizationGroup_Tbl
                .Include(g => g.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.Organization)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(g => g.IsActive);

            return query
                .OrderBy(g => g.DisplayOrder)
                .ThenBy(g => g.Title)
                .ToList();
        }

        public OrganizationGroup GetGroupById(int id, bool includeMembers = false)
        {
            var query = _context.OrganizationGroup_Tbl.AsQueryable();

            if (includeMembers)
            {
                query = query
                    .Include(g => g.Members.Where(m => m.IsActive))
                        .ThenInclude(m => m.Organization);
            }

            return query.FirstOrDefault(g => g.Id == id);
        }

        public async Task<OrganizationGroup> CreateGroupAsync(OrganizationGroup group)
        {
            if (_context.OrganizationGroup_Tbl.Any(g => g.Code == group.Code))
                throw new InvalidOperationException($"کد '{group.Code}' قبلاً استفاده شده است");

            group.CreatedDate = DateTime.Now;
            _context.OrganizationGroup_Tbl.Add(group);
            await _context.SaveChangesAsync();

            return group;
        }

        public async Task<OrganizationGroup> UpdateGroupAsync(OrganizationGroup group)
        {
            var existing = await _context.OrganizationGroup_Tbl.FindAsync(group.Id);
            if (existing == null)
                throw new ArgumentException("گروه یافت نشد");

            if (_context.OrganizationGroup_Tbl.Any(g => g.Code == group.Code && g.Id != group.Id))
                throw new InvalidOperationException($"کد '{group.Code}' قبلاً استفاده شده است");

            existing.Code = group.Code;
            existing.Title = group.Title;
            existing.Description = group.Description;
            existing.ColorHex = group.ColorHex;
            existing.IconClass = group.IconClass;
            existing.DisplayOrder = group.DisplayOrder;
            existing.IsActive = group.IsActive;
            existing.LastUpdateDate = DateTime.Now;
            existing.LastUpdaterUserId = group.LastUpdaterUserId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteGroupAsync(int id)
        {
            var group = await _context.OrganizationGroup_Tbl.FindAsync(id);
            if (group == null)
                return false;

            if (group.IsSystemGroup)
                throw new InvalidOperationException("گروه‌های سیستمی قابل حذف نیستند");

            _context.OrganizationGroup_Tbl.Remove(group);
            await _context.SaveChangesAsync();

            return true;
        }

        // ==================== ORGANIZATION GROUP MEMBERS ====================

        public async Task<OrganizationGroupMember> AddOrganizationToGroupAsync(
            int groupId, 
            int organizationId, 
            string addedByUserId, 
            string notes = null)
        {
            var existing = _context.OrganizationGroupMember_Tbl
                .FirstOrDefault(m => m.GroupId == groupId && m.OrganizationId == organizationId);

            if (existing != null)
            {
                if (existing.IsActive)
                    throw new InvalidOperationException("این سازمان قبلاً در گروه موجود است");

                existing.IsActive = true;
                existing.AddedDate = DateTime.Now;
                existing.Notes = notes;
                await _context.SaveChangesAsync();
                return existing;
            }

            var member = new OrganizationGroupMember
            {
                GroupId = groupId,
                OrganizationId = organizationId,
                AddedDate = DateTime.Now,
                IsActive = true,
                Notes = notes,
                AddedByUserId = addedByUserId
            };

            _context.OrganizationGroupMember_Tbl.Add(member);
            await _context.SaveChangesAsync();

            return member;
        }

        public async Task<bool> RemoveOrganizationFromGroupAsync(int groupId, int organizationId)
        {
            var member = _context.OrganizationGroupMember_Tbl
                .FirstOrDefault(m => m.GroupId == groupId && m.OrganizationId == organizationId && m.IsActive);

            if (member == null)
                return false;

            _context.OrganizationGroupMember_Tbl.Remove(member);
            await _context.SaveChangesAsync();

            return true;
        }

        public List<OrganizationGroupMember> GetGroupMembers(int groupId, bool includeInactive = false)
        {
            var query = _context.OrganizationGroupMember_Tbl
                .Include(m => m.Organization)
                .Include(m => m.AddedByUser)
                .Where(m => m.GroupId == groupId);

            if (!includeInactive)
                query = query.Where(m => m.IsActive);

            return query
                .OrderByDescending(m => m.AddedDate)
                .ToList();
        }

        public List<Organization> GetGroupOrganizations(int groupId, bool includeInactive = false)
        {
            var query = _context.OrganizationGroupMember_Tbl
                .Where(m => m.GroupId == groupId);

            if (!includeInactive)
                query = query.Where(m => m.Organization.IsActive); // ⭐ فیلتر روی Member

            return query
                .Select(m => m.Organization)
                .Include(o => o.Phones.Where(p => p.IsActive))
                .ToList();
        }

        public List<OrganizationGroup> GetOrganizationGroups(int organizationId)
        {
            return _context.OrganizationGroupMember_Tbl
                .Where(m => m.OrganizationId == organizationId && m.IsActive)
                .Select(m => m.Group)
                .Where(g => g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .ToList();
        }

        // ==================== BRANCH ORGANIZATION GROUP ====================

        public List<BranchOrganizationGroup> GetBranchGroups(int branchId, bool includeInactive = false)
        {
            var query = _context.BranchOrganizationGroup_Tbl
                .Include(g => g.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.BranchOrganization)
                        .ThenInclude(bo => bo.Organization)
                .Where(g => g.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(g => g.IsActive);

            return query
                .OrderBy(g => g.DisplayOrder)
                .ThenBy(g => g.Title)
                .ToList();
        }

        public BranchOrganizationGroup GetBranchGroupById(int id, bool includeMembers = false)
        {
            var query = _context.BranchOrganizationGroup_Tbl.AsQueryable();

            if (includeMembers)
            {
                query = query
                    .Include(g => g.Members.Where(m => m.IsActive))
                        .ThenInclude(m => m.BranchOrganization)
                            .ThenInclude(bo => bo.Organization);
            }

            return query.FirstOrDefault(g => g.Id == id);
        }

        public async Task<BranchOrganizationGroup> CreateBranchGroupAsync(BranchOrganizationGroup branchGroup)
        {
            if (_context.BranchOrganizationGroup_Tbl.Any(g => 
                g.BranchId == branchGroup.BranchId && 
                g.Code == branchGroup.Code))
            {
                throw new InvalidOperationException($"کد '{branchGroup.Code}' قبلاً در این شعبه استفاده شده است");
            }

            branchGroup.CreatedDate = DateTime.Now;
            _context.BranchOrganizationGroup_Tbl.Add(branchGroup);
            await _context.SaveChangesAsync();

            return branchGroup;
        }

        public async Task<BranchOrganizationGroup> UpdateBranchGroupAsync(BranchOrganizationGroup branchGroup)
        {
            var existing = await _context.BranchOrganizationGroup_Tbl.FindAsync(branchGroup.Id);
            if (existing == null)
                throw new ArgumentException("گروه یافت نشد");

            if (_context.BranchOrganizationGroup_Tbl.Any(g => 
                g.BranchId == branchGroup.BranchId && 
                g.Code == branchGroup.Code && 
                g.Id != branchGroup.Id))
            {
                throw new InvalidOperationException($"کد '{branchGroup.Code}' قبلاً استفاده شده است");
            }

            existing.Code = branchGroup.Code;
            existing.Title = branchGroup.Title;
            existing.Description = branchGroup.Description;
            existing.ColorHex = branchGroup.ColorHex;
            existing.IconClass = branchGroup.IconClass;
            existing.DisplayOrder = branchGroup.DisplayOrder;
            existing.IsActive = branchGroup.IsActive;
            existing.LastUpdateDate = DateTime.Now;
            existing.LastUpdaterUserId = branchGroup.LastUpdaterUserId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteBranchGroupAsync(int id)
        {
            var group = await _context.BranchOrganizationGroup_Tbl.FindAsync(id);
            if (group == null)
                return false;

            _context.BranchOrganizationGroup_Tbl.Remove(group);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<BranchOrganizationGroupMember> AddBranchOrganizationToGroupAsync(
            int branchGroupId, 
            int branchOrganizationId, 
            string addedByUserId, 
            string notes = null)
        {
            var existing = _context.BranchOrganizationGroupMember_Tbl
                .FirstOrDefault(m => m.BranchGroupId == branchGroupId && m.BranchOrganizationId == branchOrganizationId);

            if (existing != null)
            {
                if (existing.IsActive)
                    throw new InvalidOperationException("این سازمان قبلاً در گروه موجود است");

                existing.IsActive = true;
                existing.AddedDate = DateTime.Now;
                existing.Notes = notes;
                await _context.SaveChangesAsync();
                return existing;
            }

            var member = new BranchOrganizationGroupMember
            {
                BranchGroupId = branchGroupId,
                BranchOrganizationId = branchOrganizationId,
                AddedDate = DateTime.Now,
                IsActive = true,
                Notes = notes,
                AddedByUserId = addedByUserId
            };

            _context.BranchOrganizationGroupMember_Tbl.Add(member);
            await _context.SaveChangesAsync();

            return member;
        }

        public async Task<bool> RemoveBranchOrganizationFromGroupAsync(int branchGroupId, int branchOrganizationId)
        {
            var member = _context.BranchOrganizationGroupMember_Tbl
                .FirstOrDefault(m => m.BranchGroupId == branchGroupId && 
                                     m.BranchOrganizationId == branchOrganizationId && 
                                     m.IsActive);

            if (member == null)
                return false;

            _context.BranchOrganizationGroupMember_Tbl.Remove(member);
            await _context.SaveChangesAsync();

            return true;
        }

        public List<BranchOrganization> GetBranchGroupOrganizations(int branchGroupId, bool includeInactive = false)
        {
            var query = _context.BranchOrganizationGroupMember_Tbl
                .Where(m => m.BranchGroupId == branchGroupId)
                .Select(m => m.BranchOrganization);

            if (!includeInactive)
                query = query.Where(bo => bo.IsActive);

            return query.ToList();
        }

        public List<BranchOrganizationGroup> GetBranchOrganizationGroups(int branchOrganizationId)
        {
            return _context.BranchOrganizationGroupMember_Tbl
                .Where(m => m.BranchOrganizationId == branchOrganizationId && m.IsActive)
                .Select(m => m.BranchGroup)
                .Where(g => g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .ToList();
        }

        public List<BranchOrganization> GetAvailableBranchOrganizationsForGroup(int branchGroupId)
        {
            var group = _context.BranchOrganizationGroup_Tbl
                .Include(g => g.Members.Where(m => m.IsActive))
                .FirstOrDefault(g => g.Id == branchGroupId);

            if (group == null)
                return new List<BranchOrganization>();

            var existingBranchOrganizationIds = group.Members
                .Where(m => m.IsActive)
                .Select(m => m.BranchOrganizationId)
                .ToList();

            var availableOrganizations = _context.BranchOrganization_Tbl
                .Include(bo => bo.Organization)
                .Where(bo => bo.BranchId == group.BranchId && 
                             bo.IsActive && 
                             !existingBranchOrganizationIds.Contains(bo.Id))
                .OrderBy(bo => bo.Organization.Name)
                .ToList();

            return availableOrganizations;
        }

        // ==================== STATISTICS ====================

        public async Task<Dictionary<string, int>> GetGroupStatisticsAsync()
        {
            var stats = new Dictionary<string, int>
            {
                ["TotalGroups"] = await _context.OrganizationGroup_Tbl.CountAsync(g => g.IsActive),
                ["TotalMembers"] = await _context.OrganizationGroupMember_Tbl.CountAsync(m => m.IsActive),
                ["SystemGroups"] = await _context.OrganizationGroup_Tbl.CountAsync(g => g.IsSystemGroup && g.IsActive),
                ["CustomGroups"] = await _context.OrganizationGroup_Tbl.CountAsync(g => !g.IsSystemGroup && g.IsActive)
            };

            return stats;
        }
    }
}