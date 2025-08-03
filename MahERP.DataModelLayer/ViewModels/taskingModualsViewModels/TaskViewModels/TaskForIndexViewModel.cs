using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.StakeholderViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
    public class TaskForIndexViewModel
    {
        public List<TaskViewModel> Tasks { get; set; } = new List<TaskViewModel>();
        public int TotalCount { get; set; }

        /// <summary>
        /// هر جا initial هست یک لیست  جهت انتخاب هستد
        /// intial یعنی همه لیست و لود میکنه و برای نمایش تمام کاربر ها هست . 
        /// </summary>
        public List<BranchViewModel>? branchListInitial { get; set; }
        public List<UserViewModelFull>? UsersInitial { get; set; }
        public List<StakeholderViewModel>? StakeholdersInitial { get; set; }
        public List<TaskCategory>? TaskCategoryInitial { get; set; }







       
         



    }
}
