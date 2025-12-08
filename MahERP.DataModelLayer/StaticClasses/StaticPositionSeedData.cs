using MahERP.DataModelLayer.Entities.Organizations;
using System;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.StaticClasses
{
    /// <summary>
    /// داده‌های پایه (Seed Data) برای سمت‌های استاندارد سازمانی
    /// ⭐⭐⭐ این کلاس توسط SystemSeedDataBackgroundService استفاده می‌شود
    /// </summary>
    public static class StaticPositionSeedData
    {
        /// <summary>
        /// ⭐⭐⭐ سمت‌های استاندارد سازمانی (32 سمت)
        /// </summary>
        public static List<OrganizationPosition> CommonPositions => new()
        {
            // ========== 1️⃣ مدیریت عالی (Level 0) ==========
            new OrganizationPosition
            {
                Id = 1,
                Title = "مدیرعامل",
                TitleEnglish = "CEO (Chief Executive Officer)",
                Category = "مدیریت",
                Description = "مدیر ارشد اجرایی و بالاترین مقام سازمان",
                Level = 0,
                DefaultPowerLevel = 0,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2, // کارشناسی
                MinimumExperienceYears = 10,
                CanHireSubordinates = true,
                DisplayOrder = 1,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 2,
                Title = "مدیر مالی",
                TitleEnglish = "CFO (Chief Financial Officer)",
                Category = "مدیریت",
                Description = "مدیر ارشد امور مالی و حسابداری",
                Level = 0,
                DefaultPowerLevel = 5,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                MinimumExperienceYears = 8,
                CanHireSubordinates = true,
                DisplayOrder = 2,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 3,
                Title = "مدیر فناوری اطلاعات",
                TitleEnglish = "CTO (Chief Technology Officer)",
                Category = "مدیریت",
                Description = "مدیر ارشد فناوری و سیستم‌های اطلاعاتی",
                Level = 0,
                DefaultPowerLevel = 5,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                MinimumExperienceYears = 8,
                CanHireSubordinates = true,
                DisplayOrder = 3,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 4,
                Title = "مدیر منابع انسانی",
                TitleEnglish = "HR Manager",
                Category = "مدیریت",
                Description = "مدیر امور کارکنان و منابع انسانی",
                Level = 0,
                DefaultPowerLevel = 10,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                MinimumExperienceYears = 5,
                CanHireSubordinates = true,
                DisplayOrder = 4,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },

            // ========== 2️⃣ مدیریت میانی (Level 1) ==========
            new OrganizationPosition
            {
                Id = 5,
                Title = "مدیر بخش",
                TitleEnglish = "Department Manager",
                Category = "مدیریت",
                Description = "مدیر یک بخش یا واحد سازمانی",
                Level = 1,
                DefaultPowerLevel = 20,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                MinimumExperienceYears = 5,
                CanHireSubordinates = true,
                DisplayOrder = 5,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 6,
                Title = "سرپرست",
                TitleEnglish = "Supervisor",
                Category = "مدیریت",
                Description = "سرپرست تیم یا گروه کاری",
                Level = 1,
                DefaultPowerLevel = 25,
                IsCommon = true,
                RequiresDegree = false,
                MinimumExperienceYears = 3,
                CanHireSubordinates = false,
                DisplayOrder = 6,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 7,
                Title = "معاون",
                TitleEnglish = "Deputy Manager",
                Category = "مدیریت",
                Description = "معاون و دستیار مدیر",
                Level = 1,
                DefaultPowerLevel = 15,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                MinimumExperienceYears = 4,
                CanHireSubordinates = false,
                DisplayOrder = 7,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },

            // ========== 3️⃣ مالی و حسابداری ==========
            new OrganizationPosition
            {
                Id = 8,
                Title = "حسابدار ارشد",
                TitleEnglish = "Senior Accountant",
                Category = "مالی",
                Description = "حسابدار با سابقه و تجربه",
                Level = 2,
                DefaultPowerLevel = 30,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                MinimumExperienceYears = 5,
                DisplayOrder = 8,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 9,
                Title = "حسابدار",
                TitleEnglish = "Accountant",
                Category = "مالی",
                Description = "حسابدار عمومی",
                Level = 3,
                DefaultPowerLevel = 40,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 1, // کاردانی
                MinimumExperienceYears = 2,
                DisplayOrder = 9,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 10,
                Title = "کارشناس مالی",
                TitleEnglish = "Financial Analyst",
                Category = "مالی",
                Description = "تحلیلگر و کارشناس امور مالی",
                Level = 3,
                DefaultPowerLevel = 40,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                MinimumExperienceYears = 2,
                DisplayOrder = 10,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 11,
                Title = "خزانه دار",
                TitleEnglish = "Treasurer",
                Category = "مالی",
                Description = "مسئول مدیریت نقدینگی و خزانه",
                Level = 2,
                DefaultPowerLevel = 30,
                IsCommon = true,
                DisplayOrder = 11,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },

            // ========== 4️⃣ فروش و بازاریابی ==========
            new OrganizationPosition
            {
                Id = 12,
                Title = "مدیر فروش",
                TitleEnglish = "Sales Manager",
                Category = "فروش",
                Description = "مدیر واحد فروش",
                Level = 1,
                DefaultPowerLevel = 20,
                IsCommon = true,
                MinimumExperienceYears = 5,
                CanHireSubordinates = true,
                DisplayOrder = 12,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 13,
                Title = "کارشناس فروش",
                TitleEnglish = "Sales Specialist",
                Category = "فروش",
                Description = "کارشناس و مسئول فروش",
                Level = 3,
                DefaultPowerLevel = 50,
                IsCommon = true,
                DisplayOrder = 13,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 14,
                Title = "مدیر بازاریابی",
                TitleEnglish = "Marketing Manager",
                Category = "بازاریابی",
                Description = "مدیر واحد بازاریابی و تبلیغات",
                Level = 1,
                DefaultPowerLevel = 20,
                IsCommon = true,
                MinimumExperienceYears = 5,
                DisplayOrder = 14,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 15,
                Title = "کارشناس بازاریابی",
                TitleEnglish = "Marketing Specialist",
                Category = "بازاریابی",
                Description = "کارشناس تحقیقات بازار و تبلیغات",
                Level = 3,
                DefaultPowerLevel = 50,
                IsCommon = true,
                DisplayOrder = 15,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },

            // ========== 5️⃣ فناوری اطلاعات ==========
            new OrganizationPosition
            {
                Id = 16,
                Title = "برنامه‌نویس ارشد",
                TitleEnglish = "Senior Developer",
                Category = "فناوری اطلاعات",
                Description = "توسعه‌دهنده نرم‌افزار با سابقه",
                Level = 2,
                DefaultPowerLevel = 35,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                MinimumExperienceYears = 5,
                DisplayOrder = 16,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 17,
                Title = "برنامه‌نویس",
                TitleEnglish = "Developer",
                Category = "فناوری اطلاعات",
                Description = "توسعه‌دهنده نرم‌افزار",
                Level = 3,
                DefaultPowerLevel = 50,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                DisplayOrder = 17,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 18,
                Title = "مهندس شبکه",
                TitleEnglish = "Network Engineer",
                Category = "فناوری اطلاعات",
                Description = "متخصص شبکه و زیرساخت",
                Level = 3,
                DefaultPowerLevel = 45,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                DisplayOrder = 18,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 19,
                Title = "پشتیبان فنی",
                TitleEnglish = "IT Support",
                Category = "فناوری اطلاعات",
                Description = "پشتیبانی فنی کاربران",
                Level = 4,
                DefaultPowerLevel = 60,
                IsCommon = true,
                DisplayOrder = 19,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },

            // ========== 6️⃣ منابع انسانی ==========
            new OrganizationPosition
            {
                Id = 20,
                Title = "کارشناس منابع انسانی",
                TitleEnglish = "HR Specialist",
                Category = "منابع انسانی",
                Description = "کارشناس امور کارکنان و استخدام",
                Level = 3,
                DefaultPowerLevel = 40,
                IsCommon = true,
                RequiresDegree = true,
                MinimumDegree = 2,
                DisplayOrder = 20,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 21,
                Title = "کارشناس آموزش",
                TitleEnglish = "Training Specialist",
                Category = "منابع انسانی",
                Description = "مسئول آموزش و توسعه کارکنان",
                Level = 3,
                DefaultPowerLevel = 45,
                IsCommon = true,
                DisplayOrder = 21,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },

            // ========== 7️⃣ روابط عمومی و ارتباطات ==========
            new OrganizationPosition
            {
                Id = 22,
                Title = "مسئول روابط عمومی",
                TitleEnglish = "Public Relations Manager",
                Category = "ارتباطات",
                Description = "مدیر روابط عمومی و ارتباطات خارجی",
                Level = 1,
                DefaultPowerLevel = 25,
                IsCommon = true,
                MinimumExperienceYears = 3,
                DisplayOrder = 22,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 23,
                Title = "کارشناس ارتباطات",
                TitleEnglish = "Communications Specialist",
                Category = "ارتباطات",
                Description = "کارشناس ارتباطات و روابط عمومی",
                Level = 3,
                DefaultPowerLevel = 50,
                IsCommon = true,
                DisplayOrder = 23,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },

            // ========== 8️⃣ اداری و پشتیبانی ==========
            new OrganizationPosition
            {
                Id = 24,
                Title = "مسئول اداری",
                TitleEnglish = "Administrative Manager",
                Category = "اداری",
                Description = "مدیر امور اداری و پشتیبانی",
                Level = 1,
                DefaultPowerLevel = 30,
                IsCommon = true,
                DisplayOrder = 24,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 25,
                Title = "کارشناس اداری",
                TitleEnglish = "Administrative Specialist",
                Category = "اداری",
                Description = "کارشناس امور اداری",
                Level = 3,
                DefaultPowerLevel = 55,
                IsCommon = true,
                DisplayOrder = 25,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 26,
                Title = "منشی",
                TitleEnglish = "Secretary",
                Category = "اداری",
                Description = "منشی و دبیر دفتر",
                Level = 4,
                DefaultPowerLevel = 70,
                IsCommon = true,
                DisplayOrder = 26,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },

            // ========== 9️⃣ تولید و عملیات ==========
            new OrganizationPosition
            {
                Id = 27,
                Title = "مدیر تولید",
                TitleEnglish = "Production Manager",
                Category = "تولید",
                Description = "مدیر واحد تولید و عملیات",
                Level = 1,
                DefaultPowerLevel = 20,
                IsCommon = true,
                MinimumExperienceYears = 5,
                CanHireSubordinates = true,
                DisplayOrder = 27,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 28,
                Title = "سرپرست تولید",
                TitleEnglish = "Production Supervisor",
                Category = "تولید",
                Description = "سرپرست خط تولید",
                Level = 2,
                DefaultPowerLevel = 35,
                IsCommon = true,
                MinimumExperienceYears = 3,
                DisplayOrder = 28,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 29,
                Title = "اپراتور",
                TitleEnglish = "Operator",
                Category = "تولید",
                Description = "اپراتور دستگاه و خط تولید",
                Level = 4,
                DefaultPowerLevel = 65,
                IsCommon = true,
                DisplayOrder = 29,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },

            // ========== 🔟 انبار و لجستیک ==========
            new OrganizationPosition
            {
                Id = 30,
                Title = "مدیر انبار",
                TitleEnglish = "Warehouse Manager",
                Category = "لجستیک",
                Description = "مدیر انبار و موجودی",
                Level = 1,
                DefaultPowerLevel = 25,
                IsCommon = true,
                DisplayOrder = 30,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 31,
                Title = "انباردار",
                TitleEnglish = "Storekeeper",
                Category = "لجستیک",
                Description = "مسئول ثبت و کنترل انبار",
                Level = 3,
                DefaultPowerLevel = 50,
                IsCommon = true,
                DisplayOrder = 31,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            },
            new OrganizationPosition
            {
                Id = 32,
                Title = "راننده",
                TitleEnglish = "Driver",
                Category = "لجستیک",
                Description = "راننده و مسئول حمل و نقل",
                Level = 4,
                DefaultPowerLevel = 70,
                IsCommon = true,
                DisplayOrder = 32,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                CreatorUserId = "system"
            }
        };
    }
}
