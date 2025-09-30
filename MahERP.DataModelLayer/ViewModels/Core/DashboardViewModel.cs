using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.ViewModels.Core
{

    public class DashboardViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime LastLoginDate { get; set; }
        public TasksStatsViewModel TasksStats { get; set; } = new();
        public ContractsStatsViewModel ContractsStats { get; set; } = new();
        public StakeholdersStatsViewModel StakeholdersStats { get; set; } = new();
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    public class DashboardStatsViewModel
    {
        public TasksStatsViewModel TasksStats { get; set; } = new();
        public ContractsStatsViewModel ContractsStats { get; set; } = new();
        public StakeholdersStatsViewModel StakeholdersStats { get; set; } = new();
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    public class TasksStatsViewModel
    {
        public int MyTasksCount { get; set; }
        public int AssignedByMeCount { get; set; }
        public int SupervisedTasksCount { get; set; }
        public int TodayTasksCount { get; set; }
        public int OverdueTasksCount { get; set; }
        public int RemindersCount { get; set; }
    }

    public class ContractsStatsViewModel
    {
        public int ActiveContracts { get; set; }
        public int ExpiringContracts { get; set; }
    }

    public class StakeholdersStatsViewModel
    {
        public int TotalStakeholders { get; set; }
        public int NewThisMonth { get; set; }
    }


}