using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CRMViewModels
{
    public class CRMCommentViewModel
    {
        public int Id { get; set; }
        public int CRMInteractionId { get; set; }

        [Required(ErrorMessage = "متن نظر الزامی است")]
        [Display(Name = "متن نظر")]
        public string CommentText { get; set; }

        public DateTime CreateDate { get; set; }
        public string CreatorUserId { get; set; }
        public string? CreatorName { get; set; }
        public int? ParentCommentId { get; set; }

        public List<CRMCommentViewModel>? Replies { get; set; }
    }
}