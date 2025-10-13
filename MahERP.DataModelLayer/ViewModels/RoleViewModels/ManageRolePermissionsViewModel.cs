using System.Collections.Generic;
using MahERP.DataModelLayer.ViewModels.PermissionViewModels;

namespace MahERP.DataModelLayer.ViewModels.RoleViewModels
{
    public class ManageRolePermissionsViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<PermissionTreeViewModel> PermissionTree { get; set; } = new List<PermissionTreeViewModel>();
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();
    }
}