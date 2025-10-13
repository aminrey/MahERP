using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.RoleViewModels
{
    public class RoleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام انگلیسی الزامی است")]
        [Display(Name = "نام انگلیسی")]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "نام فارسی الزامی است")]
        [Display(Name = "نام فارسی")]
        public string NameFa { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "رنگ")]
        public string Color { get; set; }

        [Display(Name = "آیکون")]
        public string Icon { get; set; }

        [Display(Name = "اولویت")]
        public int Priority { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }
        public int PermissionsCount { get; set; }
        public int UsersCount { get; set; }
    }
}