using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.Notifications
{
    public class ManageRecipientsViewModel
    {
        public int NotificationTypeId { get; set; }
        public string? TypeName { get; set; }
        public string? Description { get; set; }
        
        /// <summary>
        /// 0=AllUsers, 1=SpecificUsers, 2=AllExceptUsers
        /// </summary>
        public byte SendMode { get; set; }
        
        public List<RecipientUser> CurrentRecipients { get; set; } = new();
        public List<AvailableUser> AllUsers { get; set; } = new();
    }

    public class RecipientUser
    {
        public int RecipientId { get; set; }
        public string UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? Reason { get; set; }
        public DateTime AddedDate { get; set; }
        public string? AddedByName { get; set; }
    }

    public class AvailableUser
    {
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
    }
}