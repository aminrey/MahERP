using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.PermissionViewModels
{
    public class PermissionTreeViewModel
    {
        public int Id { get; set; }
        public string NameEn { get; set; }
        public string NameFa { get; set; }
        public string Code { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int? ParentId { get; set; }
        public bool IsActive { get; set; }
        public bool IsSelected { get; set; } // برای انتخاب در UI
        public List<PermissionTreeViewModel> Children { get; set; } = new List<PermissionTreeViewModel>();
    }
}