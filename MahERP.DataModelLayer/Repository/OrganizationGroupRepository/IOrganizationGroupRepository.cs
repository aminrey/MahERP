using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;

namespace MahERP.DataModelLayer.Repository.OrganizationGroupRepository
{
    public interface IOrganizationGroupRepository
    {
        // ========== OrganizationGroup (System Level) ==========
        
        /// <summary>
        /// دریافت تمام گروه‌های فعال کل سیستم
        /// </summary>
        List<OrganizationGroup> GetAllGroups(bool includeInactive = false);
        
        /// <summary>
        /// دریافت گروه با شناسه
        /// </summary>
        OrganizationGroup GetGroupById(int id, bool includeMembers = false);
        
        /// <summary>
        /// ایجاد گروه جدید
        /// </summary>
        Task<OrganizationGroup> CreateGroupAsync(OrganizationGroup group);
        
        /// <summary>
        /// بروزرسانی گروه
        /// </summary>
        Task<OrganizationGroup> UpdateGroupAsync(OrganizationGroup group);
        
        /// <summary>
        /// حذف گروه
        /// </summary>
        Task<bool> DeleteGroupAsync(int id);

        // ========== OrganizationGroupMember ==========
        
        /// <summary>
        /// افزودن سازمان به گروه
        /// </summary>
        Task<OrganizationGroupMember> AddOrganizationToGroupAsync(int groupId, int organizationId, string addedByUserId, string notes = null);
        
        /// <summary>
        /// حذف سازمان از گروه
        /// </summary>
        Task<bool> RemoveOrganizationFromGroupAsync(int groupId, int organizationId);
        
        /// <summary>
        /// دریافت اعضای یک گروه
        /// </summary>
        List<OrganizationGroupMember> GetGroupMembers(int groupId, bool includeInactive = false);
        
        /// <summary>
        /// دریافت سازمان‌های یک گروه (فقط Organization ها)
        /// </summary>
        List<Organization> GetGroupOrganizations(int groupId, bool includeInactive = false);

        /// <summary>
        /// دریافت گروه‌های یک سازمان
        /// </summary>
        List<OrganizationGroup> GetOrganizationGroups(int organizationId);

        // ========== BranchOrganizationGroup (Branch Level) ==========
        
        /// <summary>
        /// دریافت گروه‌های یک شعبه
        /// </summary>
        List<BranchOrganizationGroup> GetBranchGroups(int branchId, bool includeInactive = false);
        
        /// <summary>
        /// دریافت گروه شعبه با شناسه
        /// </summary>
        BranchOrganizationGroup GetBranchGroupById(int id, bool includeMembers = false);
        
        /// <summary>
        /// ایجاد گروه برای شعبه
        /// </summary>
        Task<BranchOrganizationGroup> CreateBranchGroupAsync(BranchOrganizationGroup branchGroup);
        
        /// <summary>
        /// بروزرسانی گروه شعبه
        /// </summary>
        Task<BranchOrganizationGroup> UpdateBranchGroupAsync(BranchOrganizationGroup branchGroup);
        
        /// <summary>
        /// حذف گروه شعبه
        /// </summary>
        Task<bool> DeleteBranchGroupAsync(int id);
        
        /// <summary>
        /// افزودن BranchOrganization به گروه شعبه
        /// </summary>
        Task<BranchOrganizationGroupMember> AddBranchOrganizationToGroupAsync(
            int branchGroupId, 
            int branchOrganizationId, 
            string addedByUserId, 
            string notes = null);
        
        /// <summary>
        /// حذف سازمان از گروه شعبه
        /// </summary>
        Task<bool> RemoveBranchOrganizationFromGroupAsync(int branchGroupId, int branchOrganizationId);
        
        /// <summary>
        /// دریافت سازمان‌های یک گروه شعبه
        /// </summary>
        List<BranchOrganization> GetBranchGroupOrganizations(int branchGroupId, bool includeInactive = false);

        /// <summary>
        /// دریافت گروه‌های یک BranchOrganization
        /// </summary>
        List<BranchOrganizationGroup> GetBranchOrganizationGroups(int branchOrganizationId);

        /// <summary>
        /// دریافت سازمان‌های قابل افزودن به گروه شعبه
        /// </summary>
        List<BranchOrganization> GetAvailableBranchOrganizationsForGroup(int branchGroupId);

        // ========== Statistics ==========
        
        /// <summary>
        /// آمار گروه‌های کل سیستم
        /// </summary>
        Task<Dictionary<string, int>> GetGroupStatisticsAsync();
    }
}