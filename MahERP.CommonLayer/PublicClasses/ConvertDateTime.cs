using System;
using MD.PersianDateTime.Standard;

namespace MahERP.CommonLayer.PublicClasses
{
    public static class ConvertDateTime
    {
        /// <summary>
        /// تبدیل تاریخ شمسی (string) به میلادی
        /// </summary>
        public static DateTime ConvertShamsiToMiladi(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                throw new ArgumentNullException(nameof(date), "تاریخ نمی‌تواند خالی باشد");
            }

            PersianDateTime persianDateTime = PersianDateTime.Parse(date);
            return persianDateTime.ToDateTime();
        }

        /// <summary>
        /// ⭐⭐⭐ تبدیل تاریخ شمسی (nullable string) به میلادی (nullable DateTime)
        /// </summary>
        public static DateTime? ConvertShamsiToMiladiNullable(string? date)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                return null;
            }

            try
            {
                PersianDateTime persianDateTime = PersianDateTime.Parse(date);
                return persianDateTime.ToDateTime();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// تبدیل تاریخ میلادی به شمسی با فرمت دلخواه
        /// </summary>
        public static string ConvertMiladiToShamsi(this DateTime? date, string format)
        {
            if (!date.HasValue)
            {
                return string.Empty;
            }
            
            PersianDateTime persianDateTime = new PersianDateTime(date);
            return persianDateTime.ToString(format);
        }

        /// <summary>
        /// تبدیل تاریخ میلادی (non-nullable) به شمسی
        /// </summary>
        public static string ConvertMiladiToShamsi(DateTime date, string format)
        {
            PersianDateTime persianDateTime = new PersianDateTime(date);
            return persianDateTime.ToString(format);
        }
    }
}
