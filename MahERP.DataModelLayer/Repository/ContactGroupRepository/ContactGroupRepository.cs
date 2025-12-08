using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.ContactGroupRepository
{
    public class ContactGroupRepository : IContactGroupRepository
    {
        private readonly AppDbContext _context;

        public ContactGroupRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==================== CONTACT GROUP (SYSTEM LEVEL) ====================

        /// <summary>
        /// دریافت تمام گروه‌های کل سیستم
        /// </summary>
        public List<ContactGroup> GetAllGroups(bool includeInactive = false)
        {
            var query = _context.ContactGroup_Tbl
                .Include(g => g.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.Contact)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(g => g.IsActive);

            return query
                .OrderBy(g => g.DisplayOrder)
                .ThenBy(g => g.Title)
                .ToList();
        }

        /// <summary>
        /// دریافت گروه با شناسه
        /// </summary>
        public ContactGroup GetGroupById(int id, bool includeMembers = false)
        {
            var query = _context.ContactGroup_Tbl.AsQueryable();

            if (includeMembers)
            {
                query = query
                    .Include(g => g.Members.Where(m => m.IsActive))
                        .ThenInclude(m => m.Contact)
                            .ThenInclude(c => c.Phones.Where(p => p.IsDefault));
            }

            return query.FirstOrDefault(g => g.Id == id);
        }

        /// <summary>
        /// ایجاد گروه جدید
        /// </summary>
        public async Task<ContactGroup> CreateGroupAsync(ContactGroup group)
        {
            // بررسی یکتا بودن کد
            if (_context.ContactGroup_Tbl.Any(g => g.Code == group.Code))
                throw new InvalidOperationException($"کد '{group.Code}' قبلاً استفاده شده است");

            group.CreatedDate = DateTime.Now;
            _context.ContactGroup_Tbl.Add(group);
            await _context.SaveChangesAsync();

            return group;
        }

        /// <summary>
        /// بروزرسانی گروه
        /// </summary>
        public async Task<ContactGroup> UpdateGroupAsync(ContactGroup group)
        {
            var existing = await _context.ContactGroup_Tbl.FindAsync(group.Id);
            if (existing == null)
                throw new ArgumentException("گروه یافت نشد");

            // بررسی یکتا بودن کد (برای گروه‌های دیگر)
            if (_context.ContactGroup_Tbl.Any(g => g.Code == group.Code && g.Id != group.Id))
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

        /// <summary>
        /// حذف گروه (فقط اگر سیستمی نباشد)
        /// </summary>
        public async Task<bool> DeleteGroupAsync(int id)
        {
            var group = await _context.ContactGroup_Tbl.FindAsync(id);
            if (group == null)
                return false;

            if (group.IsSystemGroup)
                throw new InvalidOperationException("گروه‌های سیستمی قابل حذف نیستند");

            // حذف اعضا (Cascade)
            _context.ContactGroup_Tbl.Remove(group);
            await _context.SaveChangesAsync();

            return true;
        }

        // ==================== CONTACT GROUP MEMBERS ====================

        /// <summary>
        /// افزودن فرد به گروه
        /// </summary>
        public async Task<ContactGroupMember> AddContactToGroupAsync(
            int groupId, 
            int contactId, 
            string addedByUserId, 
            string notes = null)
        {
            // بررسی وجود قبلی
            var existing = _context.ContactGroupMember_Tbl
                .FirstOrDefault(m => m.GroupId == groupId && m.ContactId == contactId);

            if (existing != null)
            {
                if (existing.IsActive)
                    throw new InvalidOperationException("این فرد قبلاً در گروه موجود است");

                // فعال‌سازی مجدد
                existing.IsActive = true;
                existing.AddedDate = DateTime.Now;
                existing.Notes = notes;
                await _context.SaveChangesAsync();
                return existing;
            }

            // ایجاد عضو جدید
            var member = new ContactGroupMember
            {
                GroupId = groupId,
                ContactId = contactId,
                AddedDate = DateTime.Now,
                IsActive = true,
                Notes = notes,
                AddedByUserId = addedByUserId
            };

            _context.ContactGroupMember_Tbl.Add(member);
            await _context.SaveChangesAsync();

            return member;
        }

        /// <summary>
        /// حذف فرد از گروه
        /// </summary>
        public async Task<bool> RemoveContactFromGroupAsync(int groupId, int contactId)
        {
            var member = _context.ContactGroupMember_Tbl
                .FirstOrDefault(m => m.GroupId == groupId && m.ContactId == contactId && m.IsActive);

            if (member == null)
                return false;

            // حذف فیزیکی
            _context.ContactGroupMember_Tbl.Remove(member);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// دریافت اعضای یک گروه (با اطلاعات کامل)
        /// </summary>
        public List<ContactGroupMember> GetGroupMembers(int groupId, bool includeInactive = false)
        {
            var query = _context.ContactGroupMember_Tbl
                .Include(m => m.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                .Include(m => m.AddedByUser)
                .Where(m => m.GroupId == groupId);

            if (!includeInactive)
                query = query.Where(m => m.IsActive);

            return query
                .OrderByDescending(m => m.AddedDate)
                .ToList();
        }

        /// <summary>
        /// دریافت افراد یک گروه (فقط Contact ها)
        /// </summary>
        public List<Contact> GetGroupContacts(int groupId, bool includeInactive = false)
        {
            var query = _context.ContactGroupMember_Tbl
                .Where(m => m.GroupId == groupId && m.IsActive);

            if (!includeInactive)
                query = query.Where(m => m.Contact.IsActive); // ⭐ فیلتر روی Contact

            return query
                .Select(m => m.Contact)
                .Include(c => c.Phones.Where(p => p.IsDefault))
                .ToList();
        }

        /// <summary>
        /// دریافت گروه‌های یک فرد
        /// </summary>
        public List<ContactGroup> GetContactGroups(int contactId)
        {
            return _context.ContactGroupMember_Tbl
                .Where(m => m.ContactId == contactId && m.IsActive)
                .Select(m => m.Group)
                .Where(g => g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .ToList();
        }

        // ==================== BRANCH CONTACT GROUP ====================

        /// <summary>
        /// دریافت گروه‌های یک شعبه
        /// </summary>
        public List<BranchContactGroup> GetBranchGroups(int branchId, bool includeInactive = false)
        {
            var query = _context.BranchContactGroup_Tbl
                .Include(g => g.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.BranchContact)
                        .ThenInclude(bc => bc.Contact)
                .Where(g => g.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(g => g.IsActive);

            return query
                .OrderBy(g => g.DisplayOrder)
                .ThenBy(g => g.Title)
                .ToList();
        }

        /// <summary>
        /// دریافت گروه شعبه با شناسه
        /// </summary>
        public BranchContactGroup GetBranchGroupById(int id, bool includeMembers = false)
        {
            var query = _context.BranchContactGroup_Tbl.AsQueryable();

            if (includeMembers)
            {
                query = query
                    .Include(g => g.Members.Where(m => m.IsActive))
                        .ThenInclude(m => m.BranchContact)
                            .ThenInclude(bc => bc.Contact)
                                .ThenInclude(c => c.Phones.Where(p => p.IsDefault));
            }

            return query.FirstOrDefault(g => g.Id == id);
        }

        /// <summary>
        /// ایجاد گروه برای شعبه
        /// </summary>
        public async Task<BranchContactGroup> CreateBranchGroupAsync(BranchContactGroup branchGroup)
        {
            // بررسی یکتا بودن کد در سطح شعبه
            if (_context.BranchContactGroup_Tbl.Any(g => 
                g.BranchId == branchGroup.BranchId && 
                g.Code == branchGroup.Code))
            {
                throw new InvalidOperationException($"کد '{branchGroup.Code}' قبلاً در این شعبه استفاده شده است");
            }

            branchGroup.CreatedDate = DateTime.Now;
            _context.BranchContactGroup_Tbl.Add(branchGroup);
            await _context.SaveChangesAsync();

            return branchGroup;
        }

        /// <summary>
        /// بروزرسانی گروه شعبه
        /// </summary>
        public async Task<BranchContactGroup> UpdateBranchGroupAsync(BranchContactGroup branchGroup)
        {
            var existing = await _context.BranchContactGroup_Tbl.FindAsync(branchGroup.Id);
            if (existing == null)
                throw new ArgumentException("گروه یافت نشد");

            // بررسی یکتا بودن کد
            if (_context.BranchContactGroup_Tbl.Any(g => 
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

        /// <summary>
        /// حذف گروه شعبه
        /// </summary>
        public async Task<bool> DeleteBranchGroupAsync(int id)
        {
            var group = await _context.BranchContactGroup_Tbl.FindAsync(id);
            if (group == null)
                return false;

            _context.BranchContactGroup_Tbl.Remove(group);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// افزودن BranchContact به گروه شعبه
        /// </summary>
        public async Task<BranchContactGroupMember> AddBranchContactToGroupAsync(
            int branchGroupId, 
            int branchContactId, 
            string addedByUserId, 
            string notes = null)
        {
            // بررسی وجود قبلی
            var existing = _context.BranchContactGroupMember_Tbl
                .FirstOrDefault(m => m.BranchGroupId == branchGroupId && m.BranchContactId == branchContactId);

            if (existing != null)
            {
                if (existing.IsActive)
                    throw new InvalidOperationException("این فرد قبلاً در گروه موجود است");

                existing.IsActive = true;
                existing.AddedDate = DateTime.Now;
                existing.Notes = notes;
                await _context.SaveChangesAsync();
                return existing;
            }

            var member = new BranchContactGroupMember
            {
                BranchGroupId = branchGroupId,
                BranchContactId = branchContactId,
                AddedDate = DateTime.Now,
                IsActive = true,
                Notes = notes,
                AddedByUserId = addedByUserId
            };

            _context.BranchContactGroupMember_Tbl.Add(member);
            await _context.SaveChangesAsync();

            return member;
        }

        /// <summary>
        /// حذف فرد از گروه شعبه
        /// </summary>
        public async Task<bool> RemoveBranchContactFromGroupAsync(int branchGroupId, int branchContactId)
        {
            var member = _context.BranchContactGroupMember_Tbl
                .FirstOrDefault(m => m.BranchGroupId == branchGroupId && 
                                     m.BranchContactId == branchContactId && 
                                     m.IsActive);

            if (member == null)
                return false;

            _context.BranchContactGroupMember_Tbl.Remove(member);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// دریافت افراد یک گروه شعبه (BranchContact ها)
        /// </summary>
        public List<BranchContact> GetBranchGroupContacts(int branchGroupId, bool includeInactive = false)
        {
            var query = _context.BranchContactGroupMember_Tbl
                .Where(m => m.BranchGroupId == branchGroupId)
                .Select(m => m.BranchContact);

            if (!includeInactive)
                query = query.Where(bc => bc.IsActive);

            return query.ToList();
        }

        /// <summary>
        /// دریافت گروه‌های یک BranchContact
        /// </summary>
        public List<BranchContactGroup> GetBranchContactGroups(int branchContactId)
        {
            return _context.BranchContactGroupMember_Tbl
                .Where(m => m.BranchContactId == branchContactId && m.IsActive)
                .Select(m => m.BranchGroup)
                .Where(g => g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .ToList();
        }

        // ==================== STATISTICS ====================

        /// <summary>
        /// آمار گروه‌های کل سیستم
        /// </summary>
        public async Task<Dictionary<string, int>> GetGroupStatisticsAsync()
        {
            var stats = new Dictionary<string, int>
            {
                ["TotalGroups"] = await _context.ContactGroup_Tbl.CountAsync(g => g.IsActive),
                ["TotalMembers"] = await _context.ContactGroupMember_Tbl.CountAsync(m => m.IsActive),
                ["SystemGroups"] = await _context.ContactGroup_Tbl.CountAsync(g => g.IsSystemGroup && g.IsActive),
                ["CustomGroups"] = await _context.ContactGroup_Tbl.CountAsync(g => !g.IsSystemGroup && g.IsActive)
            };

            return stats;
        }

        /// <summary>
        /// دریافت افراد قابل افزودن به گروه شعبه
        /// (افرادی که در شعبه هستند اما در این گروه نیستند)
        /// </summary>
        public List<BranchContact> GetAvailableBranchContactsForGroup(int branchGroupId)
        {
            // دریافت گروه شعبه
            var group = _context.BranchContactGroup_Tbl
                .Include(g => g.Members.Where(m => m.IsActive))
                .FirstOrDefault(g => g.Id == branchGroupId);

            if (group == null)
                return new List<BranchContact>();

            // دریافت IDs افرادی که قبلاً در این گروه هستند
            var existingBranchContactIds = group.Members
                .Where(m => m.IsActive)
                .Select(m => m.BranchContactId)
                .ToList();

            // دریافت تمام افراد فعال این شعبه که در گروه نیستند
            var availableContacts = _context.BranchContact_Tbl
                .Include(bc => bc.Contact)
                    .ThenInclude(c => c.Phones.Where(p => p.IsDefault))
                .Where(bc => bc.BranchId == group.BranchId &&
                             bc.IsActive &&
                             !existingBranchContactIds.Contains(bc.Id))
                .OrderBy(bc => bc.Contact.LastName)
                .ThenBy(bc => bc.Contact.FirstName)
                .ToList();

            return availableContacts;
        }
    }
}