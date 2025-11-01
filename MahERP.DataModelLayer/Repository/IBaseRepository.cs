using MahERP.DataModelLayer.Entities;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// Repository برای دریافت تنظیمات عمومی سیستم
    /// </summary>
    public interface IBaseRepository
    {
        /// <summary>
        /// دریافت تنظیمات سیستم با Cache
        /// </summary>
        Settings GetSystemSettings();

        /// <summary>
        /// بررسی فعال بودن ماژول Tasking
        /// </summary>
        bool IsTaskingModuleEnabled();

        /// <summary>
        /// بررسی فعال بودن ماژول CRM
        /// </summary>
        bool IsCrmModuleEnabled();

     
        /// <summary>
        /// پاک کردن کش تنظیمات
        /// </summary>
        void ClearSettingsCache();
    }
}