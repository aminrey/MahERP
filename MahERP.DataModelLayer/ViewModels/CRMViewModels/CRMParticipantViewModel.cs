using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CRMViewModels
{
    public class CRMParticipantViewModel
    {
        public int Id { get; set; }
        public int CRMInteractionId { get; set; }

        [Display(Name = "نوع شرکت کننده")]
        public byte ParticipantType { get; set; }

        [Display(Name = "کاربر")]
        public string? UserId { get; set; }

        [Display(Name = "شخص مرتبط")]
        public int? StakeholderContactId { get; set; }

        [Display(Name = "نام")]
        public string? Name { get; set; }

        [Display(Name = "سمت")]
        public string? Title { get; set; }

        [Display(Name = "اطلاعات تماس")]
        public string? ContactInfo { get; set; }

        [Display(Name = "یادداشت")]
        public string? Notes { get; set; }

        // Read-only properties
        public string? UserName { get; set; }
        public string? StakeholderContactName { get; set; }

        public string ParticipantTypeText => ParticipantType switch
        {
            0 => "کارمند شرکت",
            1 => "مشتری/نماینده",
            2 => "سایر",
            _ => "نامشخص"
        };

        public string DisplayName => ParticipantType switch
        {
            0 => UserName,
            1 => StakeholderContactName,
            2 => Name,
            _ => "نامشخص"
        };
    }
}