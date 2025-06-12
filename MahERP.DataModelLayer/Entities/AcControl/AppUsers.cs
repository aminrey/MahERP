using MahERP.DataModelLayer.Entities.Organization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class AppUsers : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? TellPhone { get; set; }
        public string? InternalTellPhone { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public string? MelliCode { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PersonalCode { get; set; }
        public string? PositionName { get; set; }
        
        /// <summary>
        /// کاربر بالادست (مدیر مستقیم)
        /// </summary>
        public string? DirectManagerUserId { get; set; }
        [ForeignKey("DirectManagerUserId")]
        public virtual AppUsers DirectManager { get; set; }

        public byte Gender { get; set; }
        /// <summary>
        /// سطح در سلسله مراتب سازمانی
        /// 0- کارشناس
        /// 1- سرپرست
        /// 2- مدیر
        /// 3- مدیر ارشد
        /// </summary>
        public byte OrganizationalLevel { get; set; }
        
        public string? ParentUser { get; set; }
        public long? ChatidUserTelegram { get; set; }
        public string? TelegramRobatId { get; set; }
        
        /// <summary>
        /// در صورت حذف مشتری این مقدار به 0 تغییر پیدا میکند 
        /// </summary>
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        
        /// <summary>
        /// مشتری حذف شده؟
        /// </summary>
        public bool IsRemoveUser { get; set; }

        public DateTime BirthDay { get; set; }
        public DateTime RegisterDate { get; set; }//تاریخ عضویت
        
        // Essential navigation properties for organizational structure
        [InverseProperty("DirectManager")]
        public virtual ICollection<AppUsers> ManagedUsers { get; set; }
        
        public virtual ICollection<TeamMember> TeamMemberships { get; set; }
    }
}
