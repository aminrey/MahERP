using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.ViewModels.TaskViewModels
{
    public class TaskViewModelFull : TaskViewModel
    {
        /// <summary>
        /// هر جا initial هست یک لیست  جهت انتخاب هستد
        /// </summary>
        public List<BranchViewModel> branchListInitial {  get; set; }
        public List<UserViewModelFull> UsersInitial {  get; set; }
        public List<StakeholderViewModel> StakeholdersInitial { get; set; }






    }
}
