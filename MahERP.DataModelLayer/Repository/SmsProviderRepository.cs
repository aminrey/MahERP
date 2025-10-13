using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Services.SmsProviders;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    public class SmsProviderRepository : ISmsProviderRepository
    {
        private readonly AppDbContext _context;

        public SmsProviderRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========== مدیریت Providers ==========

        /// <summary>
        /// دریافت تمام Providerها
        /// </summary>
        public List<SmsProvider> GetAllProviders()
        {
            return _context.SmsProvider_Tbl
                .Include(p => p.Creator)
                .OrderBy(p => p.ProviderName)
                .ToList();
        }

        /// <summary>
        /// دریافت Providerهای فعال
        /// </summary>
        public List<SmsProvider> GetActiveProviders()
        {
            return _context.SmsProvider_Tbl
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProviderName)
                .ToList();
        }

        /// <summary>
        /// دریافت Provider پیش‌فرض
        /// </summary>
        public SmsProvider GetDefaultProvider()
        {
            return _context.SmsProvider_Tbl
                .FirstOrDefault(p => p.IsDefault && p.IsActive);
        }

        /// <summary>
        /// دریافت Provider بر اساس کد
        /// </summary>
        public SmsProvider GetProviderByCode(string providerCode)
        {
            return _context.SmsProvider_Tbl
                .FirstOrDefault(p => p.ProviderCode == providerCode);
        }

        /// <summary>
        /// ایجاد Provider جدید
        /// </summary>
        public int CreateProvider(SmsProvider provider)
        {
            _context.SmsProvider_Tbl.Add(provider);
            _context.SaveChanges();
            return provider.Id;
        }

        /// <summary>
        /// بروزرسانی Provider
        /// </summary>
        public void UpdateProvider(SmsProvider provider)
        {
            _context.SmsProvider_Tbl.Update(provider);
            _context.SaveChanges();
        }

        /// <summary>
        /// حذف Provider
        /// </summary>
        public void DeleteProvider(int providerId)
        {
            var provider = _context.SmsProvider_Tbl.Find(providerId);
            if (provider != null)
            {
                _context.SmsProvider_Tbl.Remove(provider);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// تنظیم Provider به عنوان پیش‌فرض
        /// </summary>
        public void SetAsDefaultProvider(int providerId)
        {
            // حذف پیش‌فرض قبلی
            var existingDefaults = _context.SmsProvider_Tbl.Where(p => p.IsDefault).ToList();
            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }

            // تنظیم پیش‌فرض جدید
            var provider = _context.SmsProvider_Tbl.Find(providerId);
            if (provider != null)
            {
                provider.IsDefault = true;
            }

            _context.SaveChanges();
        }

        // ========== کار با Instance واقعی ==========

        /// <summary>
        /// ایجاد Instance از Provider
        /// </summary>
        public ISmsProvider CreateProviderInstance(SmsProvider providerEntity)
        {
            switch (providerEntity.ProviderCode.ToUpper())
            {
                case "SUNWAY":
                    return new SunWaySmsProvider(
                        providerEntity.Username,
                        providerEntity.Password,
                        providerEntity.SenderNumber
                    );

                // در آینده Providerهای دیگر اضافه می‌شوند
                // case "PAYAMITO":
                //     return new PayamitoSmsProvider(...);

                default:
                    throw new NotSupportedException($"Provider {providerEntity.ProviderCode} پشتیبانی نمی‌شود");
            }
        }

        /// <summary>
        /// دریافت Instance پیش‌فرض
        /// </summary>
        public ISmsProvider GetDefaultProviderInstance()
        {
            var provider = GetDefaultProvider();
            if (provider == null)
                throw new InvalidOperationException("هیچ Provider پیش‌فرضی تنظیم نشده است");

            return CreateProviderInstance(provider);
        }

        // ========== بروزرسانی اعتبار ==========

        /// <summary>
        /// بروزرسانی اعتبار باقیمانده
        /// </summary>
        public async Task UpdateProviderCredit(int providerId)
        {
            var provider = _context.SmsProvider_Tbl.Find(providerId);
            if (provider == null) return;

            try
            {
                var instance = CreateProviderInstance(provider);
                long credit = await instance.GetCreditAsync();

                provider.RemainingCredit = credit;
                provider.LastCreditCheckDate = DateTime.Now;
                _context.SaveChanges();
            }
            catch
            {
                // در صورت خطا، اعتبار را null می‌گذاریم
                provider.RemainingCredit = null;
                provider.LastCreditCheckDate = DateTime.Now;
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// بروزرسانی اعتبار تمام Providerهای فعال
        /// </summary>
        public async Task UpdateAllActiveProvidersCredit()
        {
            var activeProviders = GetActiveProviders();
            foreach (var provider in activeProviders)
            {
                await UpdateProviderCredit(provider.Id);
                await Task.Delay(500); // تاخیر کوتاه
            }
        }
    }
}