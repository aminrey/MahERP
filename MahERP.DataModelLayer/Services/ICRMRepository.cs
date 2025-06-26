using MahERP.DataModelLayer.Entities.Crm;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    public interface ICRMRepository
    {
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
    }
}