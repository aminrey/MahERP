using MahERP.DataModelLayer.Entities.Contacts;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای نمایش چارت سازمانی
    /// </summary>
    public class OrganizationChartViewModel
    {
        public int OrganizationId { get; set; }
        
        public string OrganizationName { get; set; }
        
        public List<OrganizationDepartment> RootDepartments { get; set; } = new List<OrganizationDepartment>();
    }
}