using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels.OrganizationViewModels
{
    /// <summary>
    /// ViewModel برای نمایش ناظران خودکار یک تیم
    /// </summary>
    public class TeamSupervisorsViewModel
    {
        public int TeamId { get; set; }
        public string TeamTitle { get; set; }
        public List<SupervisorInfoViewModel> Supervisors { get; set; } = new List<SupervisorInfoViewModel>();
        
        /// <summary>
        /// تعداد کل ناظران
        /// </summary>
        public int TotalSupervisors => Supervisors.Count;
        
        /// <summary>
        /// آیا این تیم ناظر دارد؟
        /// </summary>
        public bool HasSupervisors => Supervisors.Count > 0;
    }
}
