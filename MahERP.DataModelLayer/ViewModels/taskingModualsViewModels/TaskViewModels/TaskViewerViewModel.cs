using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels
{
   

    /// <summary>
    /// آمار مجوزهای مشاهده تسک
    /// </summary>
    public class TaskViewerStatsViewModel
    {
        public int TotalViewers { get; set; }
        public int SpecialPermissions { get; set; }
        public int TeamManagers { get; set; }
        public int TeamMembers { get; set; }
        public int ExpiredPermissions { get; set; }
        public int ActivePermissions { get; set; }
    }


    public class TaskViewerViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public byte AccessType { get; set; }
        public string AccessTypeText { get; set; }
        public string? Description { get; set; }
        public DateTime AddedDate { get; set; }
        public bool IsActive { get; set; }
        public string AddedDatePersian => ConvertDateTime.ConvertMiladiToShamsi(AddedDate, "yyyy/MM/dd");
    }
}