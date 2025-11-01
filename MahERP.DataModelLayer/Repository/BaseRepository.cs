using MahERP.DataModelLayer.Entities;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
   

    public class BaseRepository : IBaseRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private const string SETTINGS_CACHE_KEY = "system_settings";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

        public BaseRepository(AppDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public Settings GetSystemSettings()
        {
            // بررسی کش
            if (!_memoryCache.TryGetValue(SETTINGS_CACHE_KEY, out Settings settings))
            {
                // دریافت از دیتابیس
                settings = _context.Settings_Tbl.FirstOrDefault();

                // اگر تنظیمات وجود نداشت، یک تنظیمات پیش‌فرض ایجاد کن
                if (settings == null)
                {
                    settings = new Settings
                    {
                        IsTaskingModuleEnabled = true,
                        IsCrmModuleEnabled = true,
                        LastModified = DateTime.Now
                    };

                    _context.Settings_Tbl.Add(settings);
                    _context.SaveChanges();
                }

                // ذخیره در کش
                _memoryCache.Set(SETTINGS_CACHE_KEY, settings, CACHE_DURATION);
            }

            return settings;
        }

        public bool IsTaskingModuleEnabled()
        {
            var settings = GetSystemSettings();
            return settings?.IsTaskingModuleEnabled ?? true;
        }

        public bool IsCrmModuleEnabled()
        {
            var settings = GetSystemSettings();
            return settings?.IsCrmModuleEnabled ?? true;
        }

        

        public void ClearSettingsCache()
        {
            _memoryCache.Remove(SETTINGS_CACHE_KEY);
        }
    }
}