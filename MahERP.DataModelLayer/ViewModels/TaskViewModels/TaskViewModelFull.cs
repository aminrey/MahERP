using MahERP.DataModelLayer.ViewModels.UserViewModels;
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
        public BranchViewModel branchListInitial {  get; set; }



    }
}
