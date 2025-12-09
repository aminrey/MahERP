using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Services;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// CRM Repository - Main Class (Existing Methods)
    /// متدهای CRUD، Data، Comments و Search در فایل‌های Partial جداگانه قرار دارند
    /// </summary>
    public partial class CRMRepository : ICRMRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CRMRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Existing Methods - CRM Interactions

        public IEnumerable<CRMInteraction> GetCRMInteractions(bool includeDeleted = false)
        {
            var query = _context.CRMInteraction_Tbl
                .Include(c => c.Stakeholder)
                .Include(c => c.StakeholderContact)
                .Include(c => c.Creator)
                .Include(c => c.Branch)
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return query.OrderByDescending(c => c.CreateDate).ToList();
        }

        public CRMInteraction GetCRMInteractionById(int id, bool includeAttachments = false, bool includeComments = false, bool includeParticipants = false)
        {
            var query = _context.CRMInteraction_Tbl
                .Include(c => c.Stakeholder)
                .Include(c => c.StakeholderContact)
                .Include(c => c.Creator)
                .Include(c => c.Branch)
                .Include(c => c.Contract)
                .AsQueryable();

            if (includeAttachments)
            {
                query = query.Include(c => c.CRMAttachments)
                    .ThenInclude(a => a.Uploader);
            }

            if (includeComments)
            {
                query = query.Include(c => c.CRMComments)
                    .ThenInclude(com => com.Creator);
            }

            if (includeParticipants)
            {
                query = query.Include(c => c.CRMParticipants)
                    .ThenInclude(p => p.User)
                    .Include(c => c.CRMParticipants)
                    .ThenInclude(p => p.StakeholderContact);
            }

            return query.FirstOrDefault(c => c.Id == id);
        }

        public IEnumerable<CRMInteraction> GetCRMInteractionsByStakeholder(int stakeholderId)
        {
            return _context.CRMInteraction_Tbl
                .Include(c => c.Creator)
                .Include(c => c.StakeholderContact)
                .Where(c => c.StakeholderId == stakeholderId && !c.IsDeleted)
                .OrderByDescending(c => c.CreateDate)
                .ToList();
        }

        public IEnumerable<CRMInteraction> GetCRMInteractionsByUser(string userId)
        {
            return _context.CRMInteraction_Tbl
                .Include(c => c.Stakeholder)
                .Include(c => c.StakeholderContact)
                .Where(c => c.CreatorUserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreateDate)
                .ToList();
        }

        public IEnumerable<CRMInteraction> SearchCRMInteractions(string searchTerm, byte? crmType = null, byte? direction = null, byte? result = null, int? stakeholderId = null)
        {
            var query = _context.CRMInteraction_Tbl
                .Include(c => c.Stakeholder)
                .Include(c => c.StakeholderContact)
                .Include(c => c.Creator)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Title.Contains(searchTerm) ||
                                        c.Description.Contains(searchTerm) ||
                                        c.CRMCode.Contains(searchTerm));
            }

            if (crmType.HasValue)
            {
                query = query.Where(c => c.CRMType == crmType.Value);
            }

            if (direction.HasValue)
            {
                query = query.Where(c => c.Direction == direction.Value);
            }

            if (result.HasValue)
            {
                query = query.Where(c => c.Result == result.Value);
            }

            if (stakeholderId.HasValue)
            {
                query = query.Where(c => c.StakeholderId == stakeholderId.Value);
            }

            return query.OrderByDescending(c => c.CreateDate).ToList();
        }

        public bool IsCRMCodeUnique(string crmCode, int? exceptId = null)
        {
            var query = _context.CRMInteraction_Tbl.Where(c => c.CRMCode == crmCode);

            if (exceptId.HasValue)
            {
                query = query.Where(c => c.Id != exceptId.Value);
            }

            return !query.Any();
        }

        #endregion

        #region Existing Methods - Comments

        public IEnumerable<CRMComment> GetCRMComments(int crmInteractionId)
        {
            return _context.CRMComment_Tbl
                .Include(c => c.Creator)
                .Include(c => c.Replies)
                .Where(c => c.CRMInteractionId == crmInteractionId && c.ParentCommentId == null)
                .OrderBy(c => c.CreateDate)
                .ToList();
        }

        public CRMComment GetCRMCommentById(int id)
        {
            return _context.CRMComment_Tbl
                .Include(c => c.Creator)
                .Include(c => c.Replies)
                .FirstOrDefault(c => c.Id == id);
        }

        #endregion

        #region Existing Methods - Attachments

        public IEnumerable<CRMAttachment> GetCRMAttachments(int crmInteractionId)
        {
            return _context.CRMAttachment_Tbl
                .Include(a => a.Uploader)
                .Where(a => a.CRMInteractionId == crmInteractionId)
                .OrderByDescending(a => a.UploadDate)
                .ToList();
        }

        public CRMAttachment GetCRMAttachmentById(int id)
        {
            return _context.CRMAttachment_Tbl
                .Include(a => a.Uploader)
                .FirstOrDefault(a => a.Id == id);
        }

        #endregion

        #region Existing Methods - Participants

        public IEnumerable<CRMParticipant> GetCRMParticipants(int crmInteractionId)
        {
            return _context.CRMParticipant_Tbl
                .Include(p => p.User)
                .Include(p => p.StakeholderContact)
                .Where(p => p.CRMInteractionId == crmInteractionId)
                .ToList();
        }

        public CRMParticipant GetCRMParticipantById(int id)
        {
            return _context.CRMParticipant_Tbl
                .Include(p => p.User)
                .Include(p => p.StakeholderContact)
                .FirstOrDefault(p => p.Id == id);
        }

        #endregion

        #region Existing Methods - Teams

        public IEnumerable<CRMTeam> GetCRMTeams(int crmInteractionId)
        {
            return _context.CRMTeam_Tbl
                .Include(t => t.Team)
                .Include(t => t.Creator)
                .Where(t => t.CRMInteractionId == crmInteractionId)
                .ToList();
        }

        public CRMTeam GetCRMTeamById(int id)
        {
            return _context.CRMTeam_Tbl
                .Include(t => t.Team)
                .Include(t => t.Creator)
                .FirstOrDefault(t => t.Id == id);
        }

        #endregion

        #region Existing Methods - Statistics

        public int GetTotalCRMInteractionsCount()
        {
            return _context.CRMInteraction_Tbl.Count(c => !c.IsDeleted);
        }

        public int GetTodayCRMInteractionsCount()
        {
            var today = DateTime.Today;
            return _context.CRMInteraction_Tbl.Count(c => !c.IsDeleted && c.CreateDate.Date == today);
        }

        public int GetPendingFollowUpsCount()
        {
            var today = DateTime.Today;
            return _context.CRMInteraction_Tbl.Count(c => !c.IsDeleted &&
                                                       c.NextFollowUpDate.HasValue &&
                                                       c.NextFollowUpDate.Value.Date <= today);
        }

        public Dictionary<string, int> GetCRMInteractionsByType()
        {
            var types = new Dictionary<byte, string>
            {
                { 0, "تماس تلفنی" },
                { 1, "جلسه حضوری" },
                { 2, "ایمیل" },
                { 3, "پیامک" },
                { 4, "سایر" }
            };

            var result = new Dictionary<string, int>();

            foreach (var type in types)
            {
                var count = _context.CRMInteraction_Tbl.Count(c => !c.IsDeleted && c.CRMType == type.Key);
                result.Add(type.Value, count);
            }

            return result;
        }

        public Dictionary<string, int> GetCRMInteractionsByResult()
        {
            var results = new Dictionary<byte, string>
            {
                { 0, "بی نتیجه" },
                { 1, "موفق" },
                { 2, "نیاز به پیگیری" }
            };

            var result = new Dictionary<string, int>();

            foreach (var res in results)
            {
                var count = _context.CRMInteraction_Tbl.Count(c => !c.IsDeleted && c.Result == res.Key);
                result.Add(res.Value, count);
            }

            return result;
        }

        #endregion
    }
}