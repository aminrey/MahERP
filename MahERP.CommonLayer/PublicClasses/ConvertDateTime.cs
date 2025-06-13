using System;
using MD.PersianDateTime.Standard;

namespace MahERP.CommonLayer.PublicClasses
{
    public static class ConvertDateTime
    {

        public static DateTime ConvertShamsiToMiladi(string date)
        {
            PersianDateTime persianDateTime = PersianDateTime.Parse(date);
            return persianDateTime.ToDateTime();
        }

        public static string ConvertMiladiToShamsi(this DateTime? date, string format)
        {
            PersianDateTime persianDateTime = new PersianDateTime(date);
            return persianDateTime.ToString(format);
        }

        public static string ConvertShamsiToMiladi(DateTime? docDate)
        {
            throw new NotImplementedException();
        }
    }
}
