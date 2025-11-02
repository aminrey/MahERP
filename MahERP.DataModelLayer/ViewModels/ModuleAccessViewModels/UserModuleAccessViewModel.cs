using System;
using MahERP.DataModelLayer.Enums;

namespace MahERP.DataModelLayer.ViewModels.ModuleAccessViewModels
{
    /// <summary>
    /// ViewModel دسترسی مستقیم کاربر به ماژول
    /// </summary>
    public class UserModuleAccessViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserName { get; set; }
        public byte ModuleType { get; set; }
        public string ModuleName { get; set; }
        public string ModuleIcon { get; set; }
        public string ModuleColor { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime GrantedDate { get; set; }
        public string GrantedByUserId { get; set; }
        public string GrantedByUserName { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// ViewModel دسترسی تیم به ماژول
    /// </summary>
    public class TeamModuleAccessViewModel
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string TeamTitle { get; set; }
        public byte ModuleType { get; set; }
        public string ModuleName { get; set; }
        public string ModuleIcon { get; set; }
        public string ModuleColor { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime GrantedDate { get; set; }
        public string GrantedByUserId { get; set; }
        public string GrantedByUserName { get; set; }
        public string Notes { get; set; }
        public int MembersCount { get; set; }
    }

    /// <summary>
    /// ViewModel دسترسی شعبه به ماژول
    /// </summary>
    public class BranchModuleAccessViewModel
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public byte ModuleType { get; set; }
        public string ModuleName { get; set; }
        public string ModuleIcon { get; set; }
        public string ModuleColor { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime GrantedDate { get; set; }
        public string GrantedByUserId { get; set; }
        public string GrantedByUserName { get; set; }
        public string Notes { get; set; }
        public int UsersCount { get; set; }
    }

    /// <summary>
    /// ViewModel تنظیمات ماژول کاربر
    /// </summary>
    public class UserModulePreferenceViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public byte LastUsedModule { get; set; }
        public string LastUsedModuleName { get; set; }
        public DateTime LastAccessDate { get; set; }
        public byte? DefaultModule { get; set; }
        public string DefaultModuleName { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش در لیست اصلی
    /// </summary>
    public class ModuleAccessListItemViewModel
    {
        public int Id { get; set; }
        public string TargetType { get; set; } // "User", "Team", "Branch"
        public string TargetName { get; set; }
        public string TargetIdentifier { get; set; } // UserId, TeamId, BranchId
        public byte ModuleType { get; set; }
        public string ModuleName { get; set; }
        public string ModuleIcon { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime GrantedDate { get; set; }
        public string GrantedByUserName { get; set; }
    }
}