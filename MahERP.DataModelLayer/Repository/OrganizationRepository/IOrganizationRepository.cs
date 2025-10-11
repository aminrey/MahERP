using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.ViewModels.OrganizationViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.OrganizationRepository
{
    /// <summary>
    /// Interface برای عملیات مربوط به سازمان‌ها، بخش‌ها، سمت‌ها و اعضا
    /// </summary>
    public interface IOrganizationRepository
    {
        #region Organization CRUD

        /// <summary>
        /// دریافت لیست تمام سازمان‌ها
        /// </summary>
        /// <param name="includeInactive">شامل سازمان‌های غیرفعال</param>
        /// <returns>لیست سازمان‌ها</returns>
        List<Organization> GetOrganizations(bool includeInactive = false);

        /// <summary>
        /// دریافت لیست تمام سازمان‌ها با فیلتر نوع
        /// </summary>
        /// <param name="includeInactive">شامل سازمان‌های غیرفعال</param>
        /// <param name="organizationType">نوع سازمان</param>
        /// <returns>لیست سازمان‌ها</returns>
        List<Organization> GetAllOrganizations(bool includeInactive = false, byte? organizationType = null);

        /// <summary>
        /// دریافت سازمان با شناسه
        /// </summary>
        /// <param name="id">شناسه سازمان</param>
        /// <returns>اطلاعات سازمان</returns>
        Organization GetOrganizationById(int id);

        /// <summary>
        /// دریافت سازمان با شناسه به صورت Async
        /// </summary>
        Task<Organization> GetOrganizationByIdAsync(int id, bool includeDepartments = false, bool includeContacts = false);

        /// <summary>
        /// جستجوی سازمان‌ها
        /// </summary>
        List<Organization> SearchOrganizations(string searchTerm, byte? organizationType = null, bool includeInactive = false);

        /// <summary>
        /// ایجاد سازمان جدید
        /// </summary>
        /// <param name="organization">اطلاعات سازمان</param>
        /// <returns>سازمان ایجاد شده</returns>
        Task<Organization> CreateOrganizationAsync(Organization organization);

        /// <summary>
        /// بروزرسانی سازمان
        /// </summary>
        /// <param name="organization">اطلاعات بروز شده</param>
        /// <returns>سازمان بروز شده</returns>
        Task<Organization> UpdateOrganizationAsync(Organization organization);

        /// <summary>
        /// حذف سازمان (Soft Delete)
        /// </summary>
        /// <param name="id">شناسه سازمان</param>
        /// <returns>موفقیت عملیات</returns>
        Task<bool> DeleteOrganizationAsync(int id);

        /// <summary>
        /// بررسی یکتایی شماره ثبت
        /// </summary>
        /// <param name="registrationNumber">شماره ثبت</param>
        /// <param name="excludeId">شناسه سازمان که از بررسی مستثنی است</param>
        /// <returns>true اگر یکتا باشد</returns>
        bool IsRegistrationNumberUnique(string registrationNumber, int? excludeId = null);

        /// <summary>
        /// بررسی یکتایی کد اقتصادی
        /// </summary>
        /// <param name="economicCode">کد اقتصادی</param>
        /// <param name="excludeId">شناسه سازمان که از بررسی مستثنی است</param>
        /// <returns>true اگر یکتا باشد</returns>
        bool IsEconomicCodeUnique(string economicCode, int? excludeId = null);

        #endregion

        #region Organization Departments

        /// <summary>
        /// دریافت بخش‌های یک سازمان
        /// </summary>
        /// <param name="organizationId">شناسه سازمان</param>
        /// <param name="includeInactive">شامل بخش‌های غیرفعال</param>
        /// <returns>لیست بخش‌ها</returns>
        List<OrganizationDepartment> GetOrganizationDepartments(int organizationId, bool includeInactive = false);

        /// <summary>
        /// دریافت بخش با شناسه (Simple - بدون Include)
        /// </summary>
        /// <param name="id">شناسه بخش</param>
        /// <returns>اطلاعات بخش</returns>
        OrganizationDepartment GetDepartmentById(int id);

        /// <summary>
        /// دریافت بخش با شناسه (با کنترل بارگذاری اطلاعات مرتبط)
        /// </summary>
        /// <param name="id">شناسه بخش</param>
        /// <param name="includePositions">شامل سمت‌ها</param>
        /// <param name="includeMembers">شامل اعضا</param>
        /// <returns>اطلاعات بخش</returns>
        OrganizationDepartment GetDepartmentById(int id, bool includePositions = false, bool includeMembers = false);

        /// <summary>
        /// دریافت بخش‌های ریشه سازمان
        /// </summary>
        List<OrganizationDepartment> GetRootDepartments(int organizationId);

        /// <summary>
        /// دریافت زیربخش‌های یک بخش
        /// </summary>
        List<OrganizationDepartment> GetChildDepartments(int parentDepartmentId);

        /// <summary>
        /// ایجاد بخش جدید
        /// </summary>
        /// <param name="department">اطلاعات بخش</param>
        /// <returns>بخش ایجاد شده</returns>
        Task<OrganizationDepartment> CreateDepartmentAsync(OrganizationDepartment department);

        /// <summary>
        /// بروزرسانی بخش
        /// </summary>
        /// <param name="department">اطلاعات بروز شده</param>
        /// <returns>بخش بروز شده</returns>
        Task<OrganizationDepartment> UpdateDepartmentAsync(OrganizationDepartment department);

        /// <summary>
        /// حذف بخش
        /// </summary>
        /// <param name="id">شناسه بخش</param>
        /// <returns>موفقیت عملیات</returns>
        Task<bool> DeleteDepartmentAsync(int id);

        /// <summary>
        /// بررسی وجود زیربخش
        /// </summary>
        bool HasChildDepartments(int departmentId);

        /// <summary>
        /// دریافت ساختار سلسله مراتبی بخش‌ها
        /// </summary>
        /// <param name="organizationId">شناسه سازمان</param>
        /// <returns>لیست بخش‌ها به صورت سلسله مراتبی</returns>
        Task<List<OrganizationDepartment>> GetDepartmentHierarchyAsync(int organizationId);

        #endregion

        #region Department Positions

        /// <summary>
        /// دریافت سمت‌های یک بخش
        /// </summary>
        /// <param name="departmentId">شناسه بخش</param>
        /// <param name="includeInactive">شامل سمت‌های غیرفعال</param>
        /// <returns>لیست سمت‌ها</returns>
        List<DepartmentPosition> GetDepartmentPositions(int departmentId, bool includeInactive = false);

        /// <summary>
        /// دریافت سمت پیش‌فرض
        /// </summary>
        DepartmentPosition GetDefaultPosition(int departmentId);

        /// <summary>
        /// دریافت سمت با شناسه
        /// </summary>
        /// <param name="id">شناسه سمت</param>
        /// <returns>اطلاعات سمت</returns>
        DepartmentPosition GetPositionById(int id);

        /// <summary>
        /// ایجاد سمت جدید
        /// </summary>
        /// <param name="position">اطلاعات سمت</param>
        /// <returns>سمت ایجاد شده</returns>
        Task<DepartmentPosition> CreatePositionAsync(DepartmentPosition position);

        /// <summary>
        /// بروزرسانی سمت
        /// </summary>
        /// <param name="position">اطلاعات بروز شده</param>
        /// <returns>سمت بروز شده</returns>
        Task<DepartmentPosition> UpdatePositionAsync(DepartmentPosition position);

        /// <summary>
        /// حذف سمت
        /// </summary>
        /// <param name="id">شناسه سمت</param>
        /// <returns>موفقیت عملیات</returns>
        Task<bool> DeletePositionAsync(int id);

        #endregion

        #region Department Members

        /// <summary>
        /// دریافت اعضای یک بخش
        /// </summary>
        /// <param name="departmentId">شناسه بخش</param>
        /// <param name="includeInactive">شامل اعضای غیرفعال</param>
        /// <returns>لیست اعضا</returns>
        List<DepartmentMember> GetDepartmentMembers(int departmentId, bool includeInactive = false);

        /// <summary>
        /// دریافت عضو با شناسه
        /// </summary>
        /// <param name="id">شناسه عضو</param>
        /// <returns>اطلاعات عضو</returns>
        DepartmentMember GetMemberById(int id);

        /// <summary>
        /// بررسی عضویت قبلی فرد در بخش
        /// </summary>
        bool IsContactMemberOfDepartment(int contactId, int departmentId);

        /// <summary>
        /// افزودن عضو به بخش
        /// </summary>
        Task<int> AddMemberToDepartmentAsync(DepartmentMember member);

        /// <summary>
        /// افزودن عضو به بخش (Alias)
        /// </summary>
        /// <param name="member">اطلاعات عضو</param>
        /// <returns>عضو اضافه شده</returns>
        Task<DepartmentMember> AddMemberAsync(DepartmentMember member);

        /// <summary>
        /// بروزرسانی عضو
        /// </summary>
        /// <param name="member">اطلاعات بروز شده</param>
        /// <returns>عضو بروز شده</returns>
        Task<DepartmentMember> UpdateMemberAsync(DepartmentMember member);

        /// <summary>
        /// حذف عضو از بخش
        /// </summary>
        Task<bool> RemoveMemberFromDepartmentAsync(int memberId);

        /// <summary>
        /// حذف عضو از بخش (Alias)
        /// </summary>
        /// <param name="id">شناسه عضو</param>
        /// <returns>موفقیت عملیات</returns>
        Task<bool> RemoveMemberAsync(int id);

        /// <summary>
        /// بررسی اینکه آیا فرد قبلاً به بخش اضافه شده
        /// </summary>
        /// <param name="departmentId">شناسه بخش</param>
        /// <param name="contactId">شناسه فرد</param>
        /// <returns>true اگر قبلاً اضافه شده</returns>
        bool IsMemberAlreadyInDepartment(int departmentId, int contactId);

        #endregion

        #region Organization Contacts

        /// <summary>
        /// دریافت روابط افراد با سازمان
        /// </summary>
        /// <param name="organizationId">شناسه سازمان</param>
        /// <param name="includeInactive">شامل روابط غیرفعال</param>
        /// <returns>لیست روابط</returns>
        List<OrganizationContact> GetOrganizationContacts(int organizationId, bool includeInactive = false);

        /// <summary>
        /// دریافت روابط افراد با سازمان (با فیلتر نوع رابطه)
        /// </summary>
        List<OrganizationContact> GetOrganizationContacts(int organizationId, byte? relationType = null, bool includeInactive = false);

        /// <summary>
        /// دریافت رابطه با شناسه
        /// </summary>
        /// <param name="id">شناسه رابطه</param>
        /// <returns>اطلاعات رابطه</returns>
        OrganizationContact GetOrganizationContactById(int id);

        /// <summary>
        /// افزودن رابطه
        /// </summary>
        Task<int> AddContactToOrganizationAsync(OrganizationContact organizationContact);

        /// <summary>
        /// ایجاد رابطه بین فرد و سازمان (Alias)
        /// </summary>
        /// <param name="organizationContact">اطلاعات رابطه</param>
        /// <returns>رابطه ایجاد شده</returns>
        Task<OrganizationContact> CreateOrganizationContactAsync(OrganizationContact organizationContact);

        /// <summary>
        /// حذف رابطه
        /// </summary>
        Task<bool> RemoveContactFromOrganizationAsync(int organizationContactId);

        /// <summary>
        /// حذف رابطه (Alias)
        /// </summary>
        /// <param name="id">شناسه رابطه</param>
        /// <returns>موفقیت عملیات</returns>
        Task<bool> DeleteOrganizationContactAsync(int id);

        #endregion

        #region Statistics & Reports

        /// <summary>
        /// دریافت آمار کلی یک سازمان
        /// </summary>
        /// <param name="organizationId">شناسه سازمان</param>
        /// <returns>آمار سازمان</returns>
        Task<OrganizationStatisticsViewModel> GetOrganizationStatisticsAsync(int organizationId);

        /// <summary>
        /// دریافت چارت سازمانی
        /// </summary>
        Task<OrganizationChartViewModel> GetOrganizationChartAsync(int organizationId);

        /// <summary>
        /// دریافت تعداد اعضای یک سازمان
        /// </summary>
        /// <param name="organizationId">شناسه سازمان</param>
        /// <param name="activeOnly">فقط اعضای فعال</param>
        /// <returns>تعداد اعضا</returns>
        int GetOrganizationMembersCount(int organizationId, bool activeOnly = true);

        /// <summary>
        /// دریافت تعداد بخش‌های یک سازمان
        /// </summary>
        /// <param name="organizationId">شناسه سازمان</param>
        /// <param name="activeOnly">فقط بخش‌های فعال</param>
        /// <returns>تعداد بخش‌ها</returns>
        int GetOrganizationDepartmentsCount(int organizationId, bool activeOnly = true);

        #endregion

        #region Search & Filter

        /// <summary>
        /// جستجوی سازمان‌ها (Async)
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="includeInactive">شامل سازمان‌های غیرفعال</param>
        /// <returns>لیست سازمان‌های مطابق</returns>
        Task<List<Organization>> SearchOrganizationsAsync(string searchTerm, bool includeInactive = false);

        /// <summary>
        /// جستجوی اعضا در سازمان
        /// </summary>
        /// <param name="organizationId">شناسه سازمان</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست اعضای مطابق</returns>
        Task<List<DepartmentMember>> SearchMembersInOrganizationAsync(int organizationId, string searchTerm);

        #endregion

        #region ViewModels

        /// <summary>
        /// دریافت ViewModel کامل برای جزئیات سازمان
        /// </summary>
        /// <param name="organizationId">شناسه سازمان</param>
        /// <returns>ViewModel جزئیات</returns>
        Task<OrganizationDetailsViewModel> GetOrganizationDetailsViewModelAsync(int organizationId);

        /// <summary>
        /// دریافت لیست سازمان‌ها به صورت ViewModel
        /// </summary>
        /// <param name="includeInactive">شامل غیرفعال‌ها</param>
        /// <returns>لیست ViewModel ها</returns>
        Task<List<OrganizationViewModel>> GetOrganizationsAsViewModelAsync(bool includeInactive = false);

        #endregion
    }
}