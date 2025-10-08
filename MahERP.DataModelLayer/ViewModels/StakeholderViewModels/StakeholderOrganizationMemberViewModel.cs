using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    public class StakeholderOrganizationMemberViewModel
    {
        public int Id { get; set; }

        public int OrganizationId { get; set; }

        [Display(Name = "عضو")]
        [Required(ErrorMessage = "انتخاب عضو الزامی است")]
        public int ContactId { get; set; }

        [Display(Name = "سمت")]
        public int? PositionId { get; set; }

        [Display(Name = "ناظر")]
        public bool IsSupervisor { get; set; }

        [Display(Name = "تاریخ پیوستن")]
        [Required(ErrorMessage = "تاریخ پیوستن الزامی است")]
        public string JoinDate { get; set; }

        [Display(Name = "تاریخ خروج")]
        public string? LeaveDate { get; set; }

        [Display(Name = "یادداشت")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public string? ContactName { get; set; }
        public string? PositionTitle { get; set; }
    }
}