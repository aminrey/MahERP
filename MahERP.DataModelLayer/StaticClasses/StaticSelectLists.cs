using Microsoft.AspNetCore.Mvc.Rendering;

namespace MahERP.DataModelLayer.StaticClasses
{
    public static class StaticSelectLists
    {
        public static List<SelectListItem> ContactTypes => new()
            {
                new() { Value = "0", Text = "اصلی" },
                new() { Value = "1", Text = "مدیر" },
                new() { Value = "2", Text = "کارمند" },
                new() { Value = "3", Text = "نماینده" },
                new() { Value = "4", Text = "سایر" }
            };

        public static List<SelectListItem> StakeholderTypes => new()
            {
                new() { Value = "0", Text = "مشتری" },
                new() { Value = "1", Text = "تامین کننده" },
                new() { Value = "2", Text = "همکار" },
                new() { Value = "3", Text = "سایر" }
            };

        public static List<SelectListItem> LeadSources => new()
            {
                new() { Value = "0", Text = "وب سایت" },
                new() { Value = "1", Text = "تبلیغات" },
                new() { Value = "2", Text = "معرفی" },
                new() { Value = "3", Text = "نمایشگاه" },
                new() { Value = "4", Text = "تماس مستقیم" },
                new() { Value = "5", Text = "سایر" }
            };

        public static List<SelectListItem> SalesStages => new()
            {
                new() { Value = "0", Text = "سرنخ اولیه" },
                new() { Value = "1", Text = "مذاکره" },
                new() { Value = "2", Text = "پیشنهاد قیمت" },
                new() { Value = "3", Text = "در حال قرارداد" },
                new() { Value = "4", Text = "مشتری فعال" },
                new() { Value = "5", Text = "مشتری غیرفعال" }
            };

        public static List<SelectListItem> CreditRatings => new()
            {
                new() { Value = "", Text = "انتخاب کنید" },
                new() { Value = "A", Text = "A" },
                new() { Value = "B", Text = "B" },
                new() { Value = "C", Text = "C" },
                new() { Value = "D", Text = "D" }
            };

        public static List<SelectListItem> ActiveStatusOptions => new()
            {
                new() { Value = "", Text = "همه" },
                new() { Value = "true", Text = "فعال" },
                new() { Value = "false", Text = "غیرفعال" }
            };

        public static List<SelectListItem> DeletedStatusOptions => new()
            {
                new() { Value = "false", Text = "خیر" },
                new() { Value = "true", Text = "بله" }
            };
    }
}