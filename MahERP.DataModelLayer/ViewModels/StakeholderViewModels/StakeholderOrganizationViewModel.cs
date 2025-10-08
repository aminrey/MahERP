using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    public class StakeholderOrganizationViewModel
    {
        public int Id { get; set; }

        public int StakeholderId { get; set; }

        [Display(Name = "عنوان واحد")]
        [Required(ErrorMessage = "عنوان واحد الزامی است")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "واحد والد")]
        public int? ParentOrganizationId { get; set; }

        [Display(Name = "مدیر واحد")]
        public int? ManagerContactId { get; set; }

        [Display(Name = "سطح")]
        public int Level { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public string? ParentOrganizationTitle { get; set; }
        public string? ManagerName { get; set; }
        
        public List<StakeholderOrganizationPositionViewModel>? Positions { get; set; }
        public List<StakeholderOrganizationMemberViewModel>? Members { get; set; }
        
        // برای نمایش چارت سازمانی
        public List<StakeholderOrganizationViewModel>? ChildOrganizations { get; set; }
    }
}