using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.StakeholderViewModels
{
    public class StakeholderOrganizationChartViewModel
    {
        public int StakeholderId { get; set; }
        public string StakeholderName { get; set; }
        public List<StakeholderOrganizationViewModel> RootOrganizations { get; set; } = new List<StakeholderOrganizationViewModel>();
    }
}