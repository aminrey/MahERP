using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.ViewModels.ContactViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.ContactRepository
{
    public interface IContactRepository
    {
        // ==================== CONTACT CRUD ====================
        
        /// <summary>
        /// دریافت لیست تمام افراد
        /// </summary>
        List<Contact> GetAllContacts(bool includeInactive = false);

        /// <summary>
        /// دریافت اطلاعات یک فرد با Include های مختلف
        /// </summary>
        Contact GetContactById(int id, bool includePhones = false, bool includeDepartments = false, bool includeOrganizations = false);

        /// <summary>
        /// دریافت اطلاعات یک فرد به صورت Async
        /// </summary>
        Task<Contact> GetContactByIdAsync(int id, bool includePhones = false, bool includeDepartments = false, bool includeOrganizations = false);

        /// <summary>
        /// جستجوی افراد
        /// </summary>
        List<Contact> SearchContacts(string searchTerm, byte? gender = null, bool includeInactive = false);

        /// <summary>
        /// بررسی یکتا بودن کد ملی
        /// </summary>
        bool IsNationalCodeUnique(string nationalCode, int? excludeId = null);

        /// <summary>
        /// بررسی یکتا بودن ایمیل اصلی
        /// </summary>
        bool IsPrimaryEmailUnique(string email, int? excludeId = null);

        /// <summary>
        /// دریافت فرد با کد ملی
        /// </summary>
        Contact GetContactByNationalCode(string nationalCode);

        // ==================== CONTACT PHONE ====================
        
        /// <summary>
        /// دریافت لیست شماره‌های یک فرد
        /// </summary>
        List<ContactPhone> GetContactPhones(int contactId, bool includeInactive = false);

        /// <summary>
        /// دریافت شماره پیش‌فرض یک فرد
        /// </summary>
        ContactPhone GetDefaultPhone(int contactId);

        /// <summary>
        /// تنظیم شماره به عنوان پیش‌فرض
        /// </summary>
        Task<bool> SetDefaultPhoneAsync(int phoneId, int contactId);

        /// <summary>
        /// بررسی وجود شماره تلفن
        /// </summary>
        bool IsPhoneNumberExists(string phoneNumber, int? excludeId = null);

        /// <summary>
        /// حذف شماره تلفن
        /// </summary>
        Task<bool> DeletePhoneAsync(int phoneId);

        // ==================== STATISTICS ====================
        
        /// <summary>
        /// دریافت آمار افراد
        /// </summary>
        Task<ContactStatisticsViewModel> GetContactStatisticsAsync();

        /// <summary>
        /// دریافت افرادی که تولد نزدیکی دارند
        /// </summary>
        List<Contact> GetUpcomingBirthdays(int daysAhead = 30);

        // ==================== DEPARTMENT MEMBERSHIPS ====================
        
        /// <summary>
        /// دریافت عضویت‌های سازمانی یک فرد
        /// </summary>
        List<DepartmentMember> GetContactDepartmentMemberships(int contactId, bool includeInactive = false);

        /// <summary>
        /// بررسی اینکه آیا فرد در بخشی عضو است
        /// </summary>
        bool IsContactMemberOfDepartment(int contactId, int departmentId);

        // ==================== ORGANIZATION RELATIONS ====================
        
        /// <summary>
        /// دریافت ارتباطات سازمانی یک فرد
        /// </summary>
        List<OrganizationContact> GetContactOrganizationRelations(int contactId, bool includeInactive = false);
    }
}