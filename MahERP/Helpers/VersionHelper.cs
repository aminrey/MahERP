using MahERP.CommonLayer.PublicClasses;
using System.Reflection;

namespace MahERP.Helpers
{
    public static class VersionHelper
    {
        /// <summary>
        /// دریافت شماره ورژن برنامه
        /// </summary>
        public static string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version?.Major}.{version?.Minor}.{version?.Build}";
        }

        /// <summary>
        /// دریافت تاریخ build
        /// </summary>
        public static string GetBuildDate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var buildDate = GetLinkerTime(assembly);
            
            // تبدیل به تاریخ شمسی
            return ConvertDateTime.ConvertMiladiToShamsi(buildDate, "yyyy/MM/dd HH:mm");
        }

        /// <summary>
        /// دریافت زمان Linker (زمان واقعی build)
        /// </summary>
        private static DateTime GetLinkerTime(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", null, 
                        System.Globalization.DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }
            }

            // روش جایگزین: از تاریخ فایل exe استفاده کنیم
            return new FileInfo(assembly.Location).LastWriteTime;
        }

        /// <summary>
        /// دریافت اطلاعات کامل برنامه
        /// </summary>
        public static string GetFullVersionInfo()
        {
            return $"نسخه {GetVersion()} - {GetBuildDate()}";
        }
    }
}