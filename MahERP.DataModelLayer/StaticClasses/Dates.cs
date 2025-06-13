using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.StaticClasses
{
  public static class Dates
    {
        public static readonly List<string> Months = new List<string>
        {
            "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
        };

        public static readonly List<int> Years = Enumerable.Range(1300, 104).ToList();
    }
}
