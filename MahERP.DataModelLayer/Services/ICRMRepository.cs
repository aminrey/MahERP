using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using MahERP.DataModelLayer.ViewModels.CRMViewModels;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    public interface ICRMRepository
    {
        #region Existing Methods

        // CRM Interactions
        IEnumerable<CRMInteraction> GetCRMInteractions(bool includeDeleted = false);
        CRMInteraction GetCRMInteractionById(int id, bool includeAttachments = false, bool includeComments = false, bool includeParticipants = false);
        IEnumerable<CRMInteraction> GetCRMInteractionsByStakeholder(int stakeholderId);
        IEnumerable<CRMInteraction> GetCRMInteractionsByUser(string userId);
        IEnumerable<CRMInteraction> SearchCRMInteractions(string searchTerm, byte? crmType = null, byte? direction = null, byte? result = null, int? stakeholderId = null);
        bool IsCRMCodeUnique(string crmCode, int? exceptId = null);

        // CRM Comments
        IEnumerable<CRMComment> GetCRMComments(int crmInteractionId);
        CRMComment GetCRMCommentById(int id);

        // CRM Attachments
        IEnumerable<CRMAttachment> GetCRMAttachments(int crmInteractionId);
        CRMAttachment GetCRMAttachmentById(int id);

        // CRM Participants
        IEnumerable<CRMParticipant> GetCRMParticipants(int crmInteractionId);
        CRMParticipant GetCRMParticipantById(int id);

        // CRM Teams
        IEnumerable<CRMTeam> GetCRMTeams(int crmInteractionId);
        CRMTeam GetCRMTeamById(int id);

        // Statistics
        int GetTotalCRMInteractionsCount();
        int GetTodayCRMInteractionsCount();
        int GetPendingFollowUpsCount();
        Dictionary<string, int> GetCRMInteractionsByType();
        Dictionary<string, int> GetCRMInteractionsByResult();

        #endregion

        #region ⭐⭐⭐ New Methods for Partial Controllers

        // ⭐ Create & Edit Model Preparation
        Task<CRMInteractionViewModel> PrepareCreateInteractionModelAsync(string userId);
        Task<CRMInteractionViewModel> PrepareEditInteractionModelAsync(int interactionId, string userId);
        Task<CRMInteractionViewModel> RepopulateCreateModelAsync(CRMInteractionViewModel model, string userId);

        // ⭐ CRUD Operations
        Task<int> CreateInteractionAsync(CRMInteractionViewModel model, string userId, string webRootPath);
        Task<bool> UpdateInteractionAsync(CRMInteractionViewModel model, string userId, string webRootPath);
        Task<(bool success, string title)> SoftDeleteInteractionAsync(int interactionId, string userId);
        Task<(bool success, bool isActive, string title)> ToggleInteractionActiveStatusAsync(int interactionId, string userId);

        // ⭐ Comments & Participants
        Task<int> CreateCommentAsync(CRMCommentViewModel model, string userId);
        Task<int> CreateParticipantAsync(CRMParticipantViewModel model);
        Task<(SelectList users, SelectList contacts)> GetParticipantDropdownsAsync(int crmInteractionId);

        // ⭐ Branch & Stakeholder Data
        Task<(List<ContactViewModel> contacts, List<OrganizationViewModel> organizations)> GetBranchDataAsync(int branchId);
        Task<List<ContactViewModel>> GetBranchContactsAsync(int branchId);
        Task<List<OrganizationViewModel>> GetBranchOrganizationsAsync(int branchId);
        Task<List<OrganizationViewModel>> GetContactOrganizationsAsync(int contactId);
        Task<List<ContactViewModel>> GetOrganizationContactsAsync(int organizationId);

        // ⭐ Search Dropdowns
        Task<(SelectList stakeholders, SelectList branches)> GetSearchDropdownsAsync();

        #endregion
    }
}