using MahERP.DataModelLayer.Entities.Contacts;
using MahERP.DataModelLayer.Entities.Organizations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Repository.OrganizationRepository
{
    /// <summary>
    /// Interface برای مدیریت سمت‌های استاندارد سازمانی
    /// </summary>
    public interface IPositionRepository
    {
        // ========== CRUD Operations ==========
        
        /// <summary>
        /// دریافت تمام سمت‌ها
        /// </summary>
        List<OrganizationPosition> GetAllPositions(bool includeInactive = false);

        /// <summary>
        /// دریافت سمت‌های رایج (IsCommon = true)
        /// </summary>
        List<OrganizationPosition> GetCommonPositions();

        /// <summary>
        /// دریافت سمت‌ها بر اساس دسته‌بندی
        /// </summary>
        List<OrganizationPosition> GetPositionsByCategory(string category, bool includeInactive = false);

        /// <summary>
        /// دریافت سمت‌ها بر اساس سطح
        /// </summary>
        List<OrganizationPosition> GetPositionsByLevel(int level, bool includeInactive = false);

        /// <summary>
        /// دریافت سمت به شناسه
        /// </summary>
        OrganizationPosition? GetPositionById(int id);

        /// <summary>
        /// جستجوی سمت‌ها
        /// </summary>
        List<OrganizationPosition> SearchPositions(string searchTerm, string? category = null, int? level = null);

        /// <summary>
        /// ایجاد سمت جدید
        /// </summary>
        Task<int> CreatePositionAsync(OrganizationPosition position);

        /// <summary>
        /// بروزرسانی سمت
        /// </summary>
        Task<bool> UpdatePositionAsync(OrganizationPosition position);

        /// <summary>
        /// حذف سمت (Soft Delete)
        /// </summary>
        Task<bool> DeletePositionAsync(int id);

        /// <summary>
        /// بررسی استفاده از سمت در بخش‌ها
        /// </summary>
        bool IsPositionInUse(int positionId);

        /// <summary>
        /// دریافت تعداد بخش‌هایی که از این سمت استفاده می‌کنند
        /// </summary>
        int GetUsageCount(int positionId);

        // ========== دسته‌بندی‌ها ==========
        
        /// <summary>
        /// دریافت لیست دسته‌بندی‌های موجود
        /// </summary>
        List<string> GetAllCategories();

        /// <summary>
        /// دریافت آمار سمت‌ها بر اساس دسته‌بندی
        /// </summary>
        Dictionary<string, int> GetCategoryStatistics();

        /// <summary>
        /// دریافت آمار سمت‌ها بر اساس سطح
        /// </summary>
        Dictionary<int, int> GetLevelStatistics();
    }
}
