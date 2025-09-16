using MahERP.DataModelLayer.Entities.Core;
using MahERP.DataModelLayer.ViewModels.Core.NotificationViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// رابط مخزن سیستم نوتیفیکیشن کلی ERP
    /// برای مدیریت نوتیفیکیشن‌های تمام سیستم‌های اصلی
    /// </summary>
    public interface ICoreNotificationRepository
    {
        #region عملیات پایه - Basic Operations

        /// <summary>
        /// دریافت نوتیفیکیشن‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="systemId">شناسه سیستم (اختیاری - برای فیلتر)</param>
        /// <param name="unreadOnly">فقط خوانده نشده‌ها</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لیست نوتیفیکیشن‌های کاربر</returns>
        Task<CoreNotificationListViewModel> GetUserNotificationsAsync(
            string userId, 
            byte? systemId = null, 
            bool unreadOnly = false, 
            int pageNumber = 1, 
            int pageSize = 20);

        /// <summary>
        /// دریافت تعداد نوتیفیکیشن‌های خوانده نشده کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="systemId">شناسه سیستم (اختیاری)</param>
        /// <returns>تعداد نوتیفیکیشن‌های خوانده نشده</returns>
        Task<int> GetUnreadNotificationCountAsync(string userId, byte? systemId = null);

        /// <summary>
        /// دریافت جزئیات یک نوتیفیکیشن
        /// </summary>
        /// <param name="notificationId">شناسه نوتیفیکیشن</param>
        /// <returns>جزئیات نوتیفیکیشن</returns>
        Task<CoreNotificationViewModel> GetNotificationByIdAsync(int notificationId);

        #endregion

        #region ایجاد نوتیفیکیشن - Create Notifications

        /// <summary>
        /// ایجاد نوتیفیکیشن جدید
        /// </summary>
        /// <param name="notification">اطلاعات نوتیفیکیشن</param>
        /// <returns>شناسه نوتیفیکیشن ایجاد شده</returns>
        Task<int> CreateNotificationAsync(CoreNotification notification);

        /// <summary>
        /// ایجاد نوتیفیکیشن برای چندین کاربر
        /// </summary>
        /// <param name="userIds">لیست شناسه کاربران</param>
        /// <param name="systemId">شناسه سیستم</param>
        /// <param name="notificationType">نوع نوتیفیکیشن</param>
        /// <param name="title">عنوان</param>
        /// <param name="message">پیام</param>
        /// <param name="senderUserId">شناسه فرستنده</param>
        /// <param name="actionUrl">لینک عمل</param>
        /// <param name="relatedRecordId">شناسه رکورد مرتبط</param>
        /// <param name="relatedRecordType">نوع رکورد مرتبط</param>
        /// <param name="relatedRecordTitle">عنوان رکورد مرتبط</param>
        /// <param name="priority">اولویت</param>
        /// <returns>لیست شناسه نوتیفیکیشن‌های ایجاد شده</returns>
        Task<List<int>> CreateBulkNotificationAsync(
            List<string> userIds,
            byte systemId,
            byte notificationType,
            string title,
            string message,
            string senderUserId,
            string actionUrl = null,
            string relatedRecordId = null,
            string relatedRecordType = null,
            string relatedRecordTitle = null,
            byte priority = 0);

        /// <summary>
        /// ایجاد نوتیفیکیشن تغییر رکورد
        /// </summary>
        /// <param name="userIds">لیست کاربران دریافت کننده</param>
        /// <param name="systemId">شناسه سیستم</param>
        /// <param name="recordId">شناسه رکورد تغییر یافته</param>
        /// <param name="recordType">نوع رکورد</param>
        /// <param name="recordTitle">عنوان رکورد</param>
        /// <param name="changedByUserId">کاربر تغییر دهنده</param>
        /// <param name="changeDetails">جزئیات تغییرات</param>
        /// <returns>لیست شناسه نوتیفیکیشن‌های ایجاد شده</returns>
        Task<List<int>> CreateRecordChangeNotificationAsync(
            List<string> userIds,
            byte systemId,
            string recordId,
            string recordType,
            string recordTitle,
            string changedByUserId,
            List<CoreNotificationDetailViewModel> changeDetails);

        #endregion

        #region مدیریت وضعیت - Status Management

        /// <summary>
        /// علامت‌گذاری نوتیفیکیشن به عنوان خوانده شده
        /// </summary>
        /// <param name="notificationId">شناسه نوتیفیکیشن</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>موفقیت عملیات</returns>
        Task<bool> MarkAsReadAsync(int notificationId, string userId);

        /// <summary>
        /// علامت‌گذاری نوتیفیکیشن به عنوان کلیک شده
        /// </summary>
        /// <param name="notificationId">شناسه نوتیفیکیشن</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>موفقیت عملیات</returns>
        Task<bool> MarkAsClickedAsync(int notificationId, string userId);

        /// <summary>
        /// علامت‌گذاری همه نوتیفیکیشن‌های مرتبط با یک رکورد به عنوان خوانده شده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="systemId">شناسه سیستم</param>
        /// <param name="relatedRecordId">شناسه رکورد مرتبط</param>
        /// <returns>تعداد نوتیفیکیشن‌های به‌روزرسانی شده</returns>
        Task<int> MarkRelatedNotificationsAsReadAsync(string userId, byte systemId, string relatedRecordId);

        /// <summary>
        /// علامت‌گذاری همه نوتیفیکیشن‌های کاربر به عنوان خوانده شده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="systemId">شناسه سیستم (اختیاری)</param>
        /// <returns>تعداد نوتیفیکیشن‌های به‌روزرسانی شده</returns>
        Task<int> MarkAllAsReadAsync(string userId, byte? systemId = null);

        #endregion

        #region مدیریت ارسال - Delivery Management

        /// <summary>
        /// ایجاد رکورد ارسال برای نوتیفیکیشن
        /// </summary>
        /// <param name="notificationId">شناسه نوتیفیکیشن</param>
        /// <param name="deliveryMethod">روش ارسال</param>
        /// <param name="deliveryAddress">آدرس مقصد</param>
        /// <returns>شناسه رکورد ارسال</returns>
        Task<int> CreateNotificationDeliveryAsync(int notificationId, byte deliveryMethod, string deliveryAddress);

        /// <summary>
        /// به‌روزرسانی وضعیت ارسال
        /// </summary>
        /// <param name="deliveryId">شناسه رکورد ارسال</param>
        /// <param name="status">وضعیت جدید</param>
        /// <param name="errorMessage">پیام خطا (در صورت وجود)</param>
        /// <param name="externalId">شناسه خارجی</param>
        /// <returns>موفقیت عملیات</returns>
        Task<bool> UpdateDeliveryStatusAsync(int deliveryId, byte status, string errorMessage = null, string externalId = null);

        /// <summary>
        /// دریافت نوتیفیکیشن‌هایی که باید ارسال شوند
        /// </summary>
        /// <param name="deliveryMethod">روش ارسال</param>
        /// <param name="maxAttempts">حداکثر تعداد تلاش</param>
        /// <param name="limit">حداکثر تعداد رکورد</param>
        /// <returns>لیست نوتیفیکیشن‌های آماده ارسال</returns>
        Task<List<CoreNotificationDeliveryViewModel>> GetPendingDeliveriesAsync(
            byte deliveryMethod, 
            int maxAttempts = 3, 
            int limit = 100);

        #endregion

        #region تنظیمات کاربر - User Settings

        /// <summary>
        /// دریافت تنظیمات نوتیفیکیشن کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="systemId">شناسه سیستم</param>
        /// <returns>تنظیمات نوتیفیکیشن</returns>
        Task<CoreNotificationSettingViewModel> GetUserNotificationSettingsAsync(string userId, byte systemId);

        /// <summary>
        /// به‌روزرسانی تنظیمات نوتیفیکیشن کاربر
        /// </summary>
        /// <param name="settings">تنظیمات جدید</param>
        /// <returns>موفقیت عملیات</returns>
        Task<bool> UpdateUserNotificationSettingsAsync(CoreNotificationSetting settings);

        /// <summary>
        /// بررسی اینکه آیا کاربر برای دریافت نوتیفیکیشن تنظیم کرده است
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="systemId">شناسه سیستم</param>
        /// <param name="notificationType">نوع نوتیفیکیشن</param>
        /// <param name="deliveryMethod">روش ارسال</param>
        /// <returns>تنظیمات فعال است یا خیر</returns>
        Task<bool> IsUserNotificationEnabledAsync(string userId, byte systemId, byte notificationType, byte deliveryMethod = 0);

        #endregion

        #region آمار و گزارش - Statistics & Reports

        /// <summary>
        /// دریافت آمار نوتیفیکیشن‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>آمار نوتیفیکیشن‌ها</returns>
        Task<CoreNotificationStatsViewModel> GetUserNotificationStatsAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// دریافت آمار کلی سیستم نوتیفیکیشن
        /// </summary>
        /// <param name="systemId">شناسه سیستم (اختیاری)</param>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>آمار کلی</returns>
        Task<CoreNotificationSystemStatsViewModel> GetSystemNotificationStatsAsync(byte? systemId = null, DateTime? fromDate = null, DateTime? toDate = null);

        #endregion

        #region پاکسازی - Cleanup

        /// <summary>
        /// حذف نوتیفیکیشن‌های قدیمی
        /// </summary>
        /// <param name="beforeDate">تاریخ مبنا</param>
        /// <param name="keepImportant">حفظ نوتیفیکیشن‌های مهم</param>
        /// <returns>تعداد نوتیفیکیشن‌های حذف شده</returns>
        Task<int> DeleteOldNotificationsAsync(DateTime beforeDate, bool keepImportant = true);

        /// <summary>
        /// آرشیو کردن نوتیفیکیشن‌های قدیمی
        /// </summary>
        /// <param name="beforeDate">تاریخ مبنا</param>
        /// <returns>تعداد نوتیفیکیشن‌های آرشیو شده</returns>
        Task<int> ArchiveOldNotificationsAsync(DateTime beforeDate);

        #endregion
    }
}