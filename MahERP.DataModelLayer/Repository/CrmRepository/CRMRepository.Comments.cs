using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.ViewModels.CRMViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// CRM Repository - Comments & Participants
    /// </summary>
    public partial class CRMRepository
    {
        #region Comments

        /// <summary>
        /// ایجاد نظر جدید
        /// </summary>
        public async Task<int> CreateCommentAsync(CRMCommentViewModel model, string userId)
        {
            var comment = _mapper.Map<CRMComment>(model);
            comment.CreateDate = DateTime.Now;
            comment.CreatorUserId = userId;

            _context.CRMComment_Tbl.Add(comment);
            await _context.SaveChangesAsync();

            return comment.Id;
        }

        #endregion

        #region Participants

        /// <summary>
        /// ایجاد شرکت کننده جدید
        /// </summary>
        public async Task<int> CreateParticipantAsync(CRMParticipantViewModel model)
        {
            var participant = _mapper.Map<CRMParticipant>(model);

            _context.CRMParticipant_Tbl.Add(participant);
            await _context.SaveChangesAsync();

            return participant.Id;
        }

        /// <summary>
        /// دریافت لیست‌های Dropdown برای افزودن شرکت کننده
        /// </summary>
        public async Task<(SelectList users, SelectList contacts)> GetParticipantDropdownsAsync(int crmInteractionId)
        {
            // دریافت کاربران فعال
            var users = await _context.Users
                .Where(u => u.IsActive && !u.IsRemoveUser)
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName
                })
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var usersList = new SelectList(users, "Id", "FullName");

            // ⭐⭐⭐ دریافت Contacts از تعامل (سیستم جدید)
            var interaction = await _context.CRMInteraction_Tbl
                .Include(i => i.Organization)
                    .ThenInclude(o => o.Contacts)
                        .ThenInclude(oc => oc.Contact)
                .FirstOrDefaultAsync(i => i.Id == crmInteractionId);

            SelectList contactsList = null;

            // اگر سازمان انتخاب شده، اعضای آن را نمایش بده
            if (interaction?.OrganizationId != null)
            {
                var contacts = await _context.OrganizationContact_Tbl
                    .Where(oc => oc.OrganizationId == interaction.OrganizationId && oc.IsActive)
                    .Include(oc => oc.Contact)
                    .Where(oc => oc.Contact != null && oc.Contact.IsActive)
                    .Select(oc => new
                    {
                        Id = oc.ContactId,
                        FullName = oc.Contact.FirstName + " " + oc.Contact.LastName
                    })
                    .OrderBy(c => c.FullName)
                    .ToListAsync();

                contactsList = new SelectList(contacts, "Id", "FullName");
            }
            // اگر فقط Contact انتخاب شده، خودش را نمایش بده
            else if (interaction?.ContactId != null)
            {
                var contact = await _context.Contact_Tbl.FindAsync(interaction.ContactId);
                if (contact != null)
                {
                    var contacts = new List<object>
                    {
                        new
                        {
                            Id = contact.Id,
                            FullName = contact.FullName
                        }
                    };

                    contactsList = new SelectList(contacts, "Id", "FullName");
                }
            }

            return (usersList, contactsList);
        }

        #endregion
    }
}
