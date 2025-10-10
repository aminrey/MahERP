using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels
{
    /// <summary>
    /// ViewModel برای نمایش تاریخچه تسک
    /// </summary>
    public class TaskHistoryViewModel
    {
        public int Id { get; set; }

        public byte ActionType { get; set; }

        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "کاربر")]
        public string UserName { get; set; }

        public DateTime ActionDate { get; set; }

        [Display(Name = "تاریخ")]
        public string ActionDatePersian { get; set; }

        public int? RelatedItemId { get; set; }

        public string RelatedItemType { get; set; }

        /// <summary>
        /// کلاس آیکون FontAwesome
        /// </summary>
        public string IconClass { get; set; }

        /// <summary>
        /// کلاس Badge
        /// </summary>
        public string BadgeClass { get; set; }
    }
}