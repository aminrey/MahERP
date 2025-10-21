using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;

namespace MahERP.DataModelLayer.Repository.ContactGroupRepository
{
    public interface IContactGroupRepository
    {
        // ========== ContactGroup (System Level) ==========
        
        /// <summary>
        /// دریافت تمام گروه‌های فعال کل سیستم
        /// </summary>
        List<ContactGroup> GetAllGroups(bool includeInactive = false);
        
        /// <summary>
        /// دریافت گروه با شناسه
        /// </summary>
        ContactGroup GetGroupById(int id, bool includeMembers = false);
        
        /// <summary>
        /// ایجاد گروه جدید
        /// </summary>
        Task<ContactGroup> CreateGroupAsync(ContactGroup group);
        
        /// <summary>
        /// بروزرسانی گروه
        /// </summary>
        Task<ContactGroup> UpdateGroupAsync(ContactGroup group);
        
        /// <summary>
        /// حذف گروه
        /// </summary>
        Task<bool> DeleteGroupAsync(int id);

        // ========== ContactGroupMember ==========
        
        /// <summary>
        /// افزودن فرد به گروه
        /// </summary>
        Task<ContactGroupMember> AddContactToGroupAsync(int groupId, int contactId, string addedByUserId, string notes = null);
        
        /// <summary>
        /// حذف فرد از گروه
        /// </summary>
        Task<bool> RemoveContactFromGroupAsync(int groupId, int contactId);
        
        /// <summary>
        /// دریافت اعضای یک گروه
        /// </summary>
        List<ContactGroupMember> GetGroupMembers(int groupId, bool includeInactive = false);
        
        /// <summary>
        /// دریافت افراد یک گروه (فقط Contact ها)
        /// </summary>
        List<Contact> GetGroupContacts(int groupId, bool includeInactive = false);

        /// <summary>
        /// دریافت گروه‌های یک فرد
        /// </summary>
        List<ContactGroup> GetContactGroups(int contactId);

        // ========== BranchContactGroup (Branch Level) ==========
        
        /// <summary>
        /// دریافت گروه‌های یک شعبه
        /// </summary>
        List<BranchContactGroup> GetBranchGroups(int branchId, bool includeInactive = false);
        
        /// <summary>
        /// ⭐ دریافت گروه شعبه با شناسه
        /// </summary>
        BranchContactGroup GetBranchGroupById(int id, bool includeMembers = false);
        
        /// <summary>
        /// ایجاد گروه برای شعبه
        /// </summary>
        Task<BranchContactGroup> CreateBranchGroupAsync(BranchContactGroup branchGroup);
        
        /// <summary>
        /// ⭐ بروزرسانی گروه شعبه
        /// </summary>
        Task<BranchContactGroup> UpdateBranchGroupAsync(BranchContactGroup branchGroup);
        
        /// <summary>
        /// ⭐ حذف گروه شعبه
        /// </summary>
        Task<bool> DeleteBranchGroupAsync(int id);
        
        /// <summary>
        /// افزودن BranchContact به گروه شعبه
        /// </summary>
        Task<BranchContactGroupMember> AddBranchContactToGroupAsync(
            int branchGroupId, 
            int branchContactId, 
            string addedByUserId, 
            string notes = null);
        
        /// <summary>
        /// ⭐ حذف فرد از گروه شعبه
        /// </summary>
        Task<bool> RemoveBranchContactFromGroupAsync(int branchGroupId, int branchContactId);
        
        /// <summary>
        /// دریافت افراد یک گروه شعبه
        /// </summary>
        List<BranchContact> GetBranchGroupContacts(int branchGroupId, bool includeInactive = false);

        /// <summary>
        /// دریافت گروه‌های یک BranchContact
        /// </summary>
        List<BranchContactGroup> GetBranchContactGroups(int branchContactId);

        // ========== Statistics ==========
        
        /// <summary>
        /// آمار گروه‌های کل سیستم
        /// </summary>
        Task<Dictionary<string, int>> GetGroupStatisticsAsync();

        /// <summary>
        /// ⭐ NEW: دریافت افراد قابل افزودن به گروه شعبه
        /// (افرادی که در شعبه هستند اما در این گروه نیستند)
        /// </summary>
        /// <param name="branchGroupId">شناسه گروه شعبه</param>
        /// <returns>لیست BranchContact های قابل افزودن</returns>
        List<BranchContact> GetAvailableBranchContactsForGroup(int branchGroupId);
    }
}