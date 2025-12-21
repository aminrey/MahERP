using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// بخش مدیریت تسک‌های زمان‌بندی شده (Scheduled Tasks)
    /// </summary>
    public partial class TaskRepository
    {
        private static readonly TimeZoneInfo IranTimeZone = 
            TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");

        #region Scheduled Tasks CRUD

        /// <summary>
        /// ایجاد زمان‌بندی تسک (Schedule) و بررسی CreateImmediately
        /// </summary>
        public async Task<(int ScheduleId, Tasks? ImmediateTask)> CreateScheduledTaskAsync(
            TaskViewModel model, 
            string userId)
        {
            // ⭐ ساخت JSON Template
            var taskTemplateJson = CreateTaskTemplateJson(model);

            // ⭐ محاسبه NextExecutionDate
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            var nextExecution = CalculateNextExecutionDate(model.TaskSchedule, nowIran);

            // ⭐⭐⭐ تعیین IsRecurring بر اساس ScheduleType
            bool isRecurring = model.TaskSchedule?.ScheduleType switch
            {
                0 => false, // یکبار
                1 => true,  // روزانه
                2 => true,  // هفتگی
                3 => true,  // ماهانه
                _ => false
            };

            // ⭐ ذخیره در ScheduledTaskCreation_Tbl
            var schedule = new ScheduledTaskCreation
            {
                ScheduleTitle = model.TaskSchedule?.ScheduleTitle ?? model.Title,
                ScheduleDescription = model.TaskSchedule?.ScheduleDescription,
                TaskDataJson = taskTemplateJson,
                ScheduleType = model.TaskSchedule?.ScheduleType ?? 0,
                ScheduledTime = model.TaskSchedule?.ScheduledTime,
                ScheduledDaysOfWeek = model.TaskSchedule?.ScheduledDaysOfWeek,
                ScheduledDaysOfMonth = model.TaskSchedule?.ScheduledDaysOfMonth, // ⭐⭐⭐ جدید
                ScheduledDayOfMonth = model.TaskSchedule?.ScheduledDayOfMonth, // Backward compatibility
                // ⭐⭐⭐ استفاده از متد nullable-safe
                StartDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(
                    model.TaskSchedule?.StartDatePersian),
                EndDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(
                    model.TaskSchedule?.EndDatePersian),
                MaxOccurrences = model.TaskSchedule?.MaxOccurrences,
                IsRecurring = isRecurring, // ⭐⭐⭐ اضافه شد
                IsScheduleEnabled = true,
                IsActive = true,
                ExecutionCount = 0,
                IsExecuted = false, // ⭐⭐⭐ اضافه شد
                CreatedByUserId = userId,
                CreatedDate = DateTime.UtcNow,
                BranchId = model.BranchIdSelected,
                NextExecutionDate = nextExecution
            };

            _context.ScheduledTaskCreation_Tbl.Add(schedule);
            await _context.SaveChangesAsync();

            // ⭐⭐⭐ بررسی CreateImmediately - فقط اگر true باشد
            Tasks? immediateTask = null;
            if (model.TaskSchedule?.CreateImmediately == true)
            {
                Console.WriteLine($"✅ CreateImmediately is TRUE - Creating immediate task for schedule {schedule.Id}");
                
                // ساخت تسک فوری (ViewModel از قبل آماده است)
                immediateTask = await CreateTaskEntityAsync(model, userId, _mapper);
                immediateTask.ScheduleId = schedule.Id;
                immediateTask.CreationMode = 1; // ⭐ خودکار (از Schedule)
                
                // ⭐⭐⭐ استفاده مستقیم از _context به جای _unitOfWork
                _context.Tasks_Tbl.Update(immediateTask);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ Immediate task created with ID: {immediateTask.Id}");
            }
            else
            {
                Console.WriteLine($"ℹ️ CreateImmediately is FALSE - No immediate task created for schedule {schedule.Id}");
            }

            return (schedule.Id, immediateTask);
        }

        /// <summary>
        /// ⭐⭐⭐ محاسبه NextExecutionDate بر اساس نوع زمان‌بندی
        /// کپی شده از NotificationTemplateRepository - تست شده و کار می‌کند
        /// </summary>
        private DateTime? CalculateNextExecutionDate(
            TaskScheduleViewModel? schedule, 
            DateTime nowIran)
        {
            if (schedule == null || string.IsNullOrEmpty(schedule.ScheduledTime))
            {
                Console.WriteLine("⚠️ CalculateNextExecutionDate: schedule یا ScheduledTime خالی است");
                return null;
            }

            if (!TimeSpan.TryParse(schedule.ScheduledTime, out var timeOfDay))
            {
                Console.WriteLine($"⚠️ CalculateNextExecutionDate: نمی‌توان ScheduledTime را parse کرد: {schedule.ScheduledTime}");
                return null;
            }

            int hour = timeOfDay.Hours;
            int minute = timeOfDay.Minutes;

            // ⭐⭐⭐ اعتبارسنجی: ساعت باید بین 0-23 باشد
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
            {
                Console.WriteLine($"⚠️ CalculateNextExecutionDate: ساعت یا دقیقه نامعتبر: {hour}:{minute}");
                return null;
            }

            DateTime nextExecutionIran;

            Console.WriteLine($"🔍 CalculateNextExecutionDate: ScheduleType={schedule.ScheduleType}, Time={hour}:{minute}, NowIran={nowIran:yyyy-MM-dd HH:mm}");

            switch (schedule.ScheduleType)
            {
                case 0: // OneTime
                    // ⭐⭐⭐ FIX: استفاده از OneTimeExecutionDatePersian یا StartDatePersian
                    var dateString = !string.IsNullOrEmpty(schedule.OneTimeExecutionDatePersian)
                        ? schedule.OneTimeExecutionDatePersian
                        : schedule.StartDatePersian;

                    if (string.IsNullOrEmpty(dateString))
                    {
                        Console.WriteLine("⚠️ OneTime: تاریخ اجرا مشخص نشده");
                        return null;
                    }

                    var oneTimeDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(dateString);
                    if (!oneTimeDate.HasValue)
                    {
                        Console.WriteLine($"⚠️ OneTime: نمی‌توان تاریخ را تبدیل کرد: {dateString}");
                        return null;
                    }

                    // ⭐ ساخت DateTime در Iran TimeZone
                    nextExecutionIran = new DateTime(
                        oneTimeDate.Value.Year, oneTimeDate.Value.Month, oneTimeDate.Value.Day,
                        hour, minute, 0, DateTimeKind.Unspecified);
                    
                    Console.WriteLine($"✅ OneTime: NextExecution={nextExecutionIran:yyyy-MM-dd HH:mm}");
                    break;

                case 1: // روزانه
                    // ⭐ ساخت DateTime در Iran TimeZone
                    nextExecutionIran = new DateTime(
                        nowIran.Year, nowIran.Month, nowIran.Day, 
                        hour, minute, 0, DateTimeKind.Unspecified);
                    
                    // ⭐⭐⭐ FIX: اگر زمان امروز گذشته، حتماً یک روز اضافه کن
                    if (nextExecutionIran <= nowIran)
                    {
                        nextExecutionIran = nextExecutionIran.AddDays(1);
                        Console.WriteLine($"✅ Daily: امروز گذشت، فردا: {nextExecutionIran:yyyy-MM-dd HH:mm}");
                    }
                    else
                    {
                        Console.WriteLine($"✅ Daily: امروز: {nextExecutionIran:yyyy-MM-dd HH:mm}");
                    }
                    break;

                case 2: // هفتگی
                    if (string.IsNullOrEmpty(schedule.ScheduledDaysOfWeek))
                    {
                        Console.WriteLine("⚠️ Weekly: روزهای هفته مشخص نشده");
                        return null;
                    }

                    var daysOfWeek = schedule.ScheduledDaysOfWeek
                        .Split(',')
                        .Select(d => int.TryParse(d.Trim(), out var day) ? day : -1)
                        .Where(d => d >= 0 && d <= 6)
                        .OrderBy(d => d)
                        .ToList();

                    if (!daysOfWeek.Any())
                    {
                        Console.WriteLine("⚠️ Weekly: هیچ روز معتبری انتخاب نشده");
                        return null;
                    }

                    Console.WriteLine($"🔍 Weekly: روزهای انتخابی: {string.Join(", ", daysOfWeek)}");
                    nextExecutionIran = FindNextWeeklyExecution(nowIran, hour, minute, daysOfWeek);
                    Console.WriteLine($"✅ Weekly: NextExecution={nextExecutionIran:yyyy-MM-dd HH:mm}");
                    break;

                case 3: // ماهانه
                    // ⭐⭐⭐ اگر چند روز انتخاب شده باشد (ScheduledDaysOfMonth)
                    if (!string.IsNullOrEmpty(schedule.ScheduledDaysOfMonth))
                    {
                        Console.WriteLine($"🔍 Monthly (Multiple): ScheduledDaysOfMonth={schedule.ScheduledDaysOfMonth}");
                        
                        var daysOfMonth = schedule.ScheduledDaysOfMonth
                            .Split(',')
                            .Select(d => int.TryParse(d.Trim(), out var day) ? day : (int?)null)
                            .Where(d => d.HasValue && d.Value >= 1 && d.Value <= 31)
                            .Select(d => d.Value)
                            .OrderBy(d => d)
                            .ToList();

                        if (daysOfMonth.Any())
                        {
                            Console.WriteLine($"🔍 Monthly (Multiple): روزهای انتخابی: {string.Join(", ", daysOfMonth)}");
                            nextExecutionIran = FindNextMonthlyMultipleDaysExecution(nowIran, hour, minute, daysOfMonth);
                            Console.WriteLine($"✅ Monthly (Multiple): NextExecution={nextExecutionIran:yyyy-MM-dd HH:mm}");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Monthly (Multiple): هیچ روزی معتبری parse نشد");
                        }
                    }
                    else
                    {
                        Console.WriteLine("🔍 Monthly: ScheduledDaysOfMonth خالی است");
                    }

                    // ⭐ در غیر این صورت از ScheduledDayOfMonth استفاده کن
#pragma warning disable CS0618
                    if (!schedule.ScheduledDayOfMonth.HasValue)
#pragma warning restore CS0618
                    {
                        Console.WriteLine("⚠️ Monthly: هیچ روزی مشخص نشده (نه ScheduledDaysOfMonth و نه ScheduledDayOfMonth)");
                        return null;
                    }

#pragma warning disable CS0618
                    Console.WriteLine($"🔍 Monthly (Single): ScheduledDayOfMonth={schedule.ScheduledDayOfMonth.Value}");
                    nextExecutionIran = FindNextMonthlyExecution(nowIran, hour, minute, schedule.ScheduledDayOfMonth.Value);
                    Console.WriteLine($"✅ Monthly (Single): NextExecution={nextExecutionIran:yyyy-MM-dd HH:mm}");
#pragma warning restore CS0618
                    break;

                case 4: // ⭐⭐⭐ NEW: Interval (با فاصله)
                    if (!schedule.IntervalDays.HasValue || schedule.IntervalDays.Value <= 0)
                    {
                        Console.WriteLine("⚠️ Interval: IntervalDays مشخص نشده یا نامعتبر است");
                        return null;
                    }

                    Console.WriteLine($"🔍 Interval: هر {schedule.IntervalDays.Value} روز یکبار");
                    
                    // بررسی تاریخ شروع
                    if (!string.IsNullOrEmpty(schedule.StartDatePersian))
                    {
                        var startDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(schedule.StartDatePersian);
                        if (startDate.HasValue)
                        {
                            // تبدیل به Iran TimeZone
                            var startDateIran = new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, hour, minute, 0);
                            Console.WriteLine($"🔍 Interval: تاریخ شروع={startDateIran:yyyy-MM-dd HH:mm}");
                            
                            nextExecutionIran = FindNextIntervalExecution(nowIran, startDateIran, schedule.IntervalDays.Value, schedule.IntervalDayOfWeek, hour, minute);
                            Console.WriteLine($"✅ Interval: NextExecution={nextExecutionIran:yyyy-MM-dd HH:mm}");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Interval: نمی‌توان StartDatePersian را تبدیل کرد");
                            return null;
                        }
                    }
                    else
                    {
                        // اگر تاریخ شروع نداریم، از امروز شروع کن
                        Console.WriteLine("🔍 Interval: تاریخ شروع نداریم، از امروز شروع می‌کنیم");
                        var todayStart = new DateTime(nowIran.Year, nowIran.Month, nowIran.Day, hour, minute, 0);
                        nextExecutionIran = FindNextIntervalExecution(nowIran, todayStart, schedule.IntervalDays.Value, schedule.IntervalDayOfWeek, hour, minute);
                        Console.WriteLine($"✅ Interval: NextExecution={nextExecutionIran:yyyy-MM-dd HH:mm}");
                    }
                    break;

                default:
                    Console.WriteLine($"⚠️ ScheduleType نامعتبر: {schedule.ScheduleType}");
                    return null;
            }

            // ⭐⭐⭐ تبدیل Iran Time به UTC برای ذخیره در دیتابیس
            var utcResult = TimeZoneInfo.ConvertTimeToUtc(nextExecutionIran, IranTimeZone);
            Console.WriteLine($"✅ تبدیل نهایی: Iran={nextExecutionIran:yyyy-MM-dd HH:mm} → UTC={utcResult:yyyy-MM-dd HH:mm}");
            return utcResult;
        }

        /// <summary>
        /// ⭐⭐⭐ پیدا کردن زمان بعدی برای زمان‌بندی هفتگی
        /// کپی شده از NotificationTemplateRepository
        /// </summary>
        private DateTime FindNextWeeklyExecution(DateTime nowIran, int hour, int minute, List<int> daysOfWeek)
        {
            var currentDayOfWeek = (int)nowIran.DayOfWeek;

            // چک کردن امروز
            var todayExecution = new DateTime(
                nowIran.Year, nowIran.Month, nowIran.Day, 
                hour, minute, 0, DateTimeKind.Unspecified);
            
            if (daysOfWeek.Contains(currentDayOfWeek) && todayExecution > nowIran)
            {
                return todayExecution;
            }

            // پیدا کردن روز بعدی
            for (int i = 1; i <= 7; i++)
            {
                var nextDate = nowIran.AddDays(i);
                var nextDayOfWeek = (int)nextDate.DayOfWeek;

                if (daysOfWeek.Contains(nextDayOfWeek))
                {
                    return new DateTime(
                        nextDate.Year, nextDate.Month, nextDate.Day, 
                        hour, minute, 0, DateTimeKind.Unspecified);
                }
            }

            return nowIran.AddDays(7);
        }

        /// <summary>
        /// ⭐⭐⭐ پیدا کردن زمان بعدی برای زمان‌بندی ماهانه (یک روز)
        /// کپی شده از NotificationTemplateRepository
        /// </summary>
        private DateTime FindNextMonthlyExecution(DateTime nowIran, int hour, int minute, int dayOfMonth)
        {
            // چک کردن این ماه
            var daysInMonth = DateTime.DaysInMonth(nowIran.Year, nowIran.Month);
            var targetDay = Math.Min(dayOfMonth, daysInMonth);

            var thisMonthExecution = new DateTime(
                nowIran.Year, nowIran.Month, targetDay, 
                hour, minute, 0, DateTimeKind.Unspecified);
            
            if (thisMonthExecution > nowIran)
            {
                return thisMonthExecution;
            }

            // ماه بعد
            var nextMonth = nowIran.AddMonths(1);
            daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            targetDay = Math.Min(dayOfMonth, daysInMonth);

            return new DateTime(
                nextMonth.Year, nextMonth.Month, targetDay, 
                hour, minute, 0, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// ⭐⭐⭐ پیدا کردن زمان بعدی برای زمان‌بندی ماهانه (چند روز)
        /// کپی شده از NotificationTemplateRepository
        /// </summary>
        private DateTime FindNextMonthlyMultipleDaysExecution(DateTime nowIran, int hour, int minute, List<int> daysOfMonth)
        {
            var currentDay = nowIran.Day;
            var currentMonth = nowIran.Month;
            var currentYear = nowIran.Year;

            // ⭐ مرحله 1: بررسی ماه جاری
            var daysInCurrentMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            var todayExecution = new DateTime(currentYear, currentMonth, Math.Min(currentDay, daysInCurrentMonth))
                .Add(new TimeSpan(hour, minute, 0));
            
            // آیا امروز در لیست است و ساعت نگذشته؟
            if (daysOfMonth.Contains(currentDay) && nowIran < todayExecution)
            {
                return todayExecution;
            }

            // پیدا کردن اولین روز بعد از امروز در ماه جاری
            foreach (var day in daysOfMonth.Where(d => d > currentDay))
            {
                if (day <= daysInCurrentMonth)
                {
                    return new DateTime(currentYear, currentMonth, day).Add(new TimeSpan(hour, minute, 0));
                }
            }

            // ⭐ مرحله 2: اگر در ماه جاری روزی نماند، ماه بعد
            var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
            var nextYear = currentMonth == 12 ? currentYear + 1 : currentYear;
            var daysInNextMonth = DateTime.DaysInMonth(nextYear, nextMonth);

            // اولین روز موجود در ماه بعد
            var firstAvailableDay = daysOfMonth.FirstOrDefault(d => d <= daysInNextMonth);
            if (firstAvailableDay > 0)
            {
                return new DateTime(nextYear, nextMonth, firstAvailableDay).Add(new TimeSpan(hour, minute, 0));
            }

            // اگر هیچ روزی در ماه بعد موجود نیست (مثلاً روز 31 در فوریه)
            // برو 2 ماه بعد
            var nextNextMonth = nextMonth == 12 ? 1 : nextMonth + 1;
            var nextNextYear = nextMonth == 12 ? nextYear + 1 : nextYear;
            var daysInNextNextMonth = DateTime.DaysInMonth(nextNextYear, nextNextMonth);

            firstAvailableDay = daysOfMonth.FirstOrDefault(d => d <= daysInNextNextMonth);
            if (firstAvailableDay > 0)
            {
                return new DateTime(nextNextYear, nextNextMonth, firstAvailableDay).Add(new TimeSpan(hour, minute, 0));
            }

            // در صورت مشکل، ماه بعد اولین روز از لیست
            return new DateTime(nextYear, nextMonth, daysOfMonth.First()).Add(new TimeSpan(hour, minute, 0));
        }

        /// <summary>
        /// ⭐⭐⭐ NEW: پیدا کردن زمان بعدی برای زمان‌بندی با فاصله (Interval)
        /// </summary>
        /// <param name="nowIran">زمان فعلی در Iran TimeZone</param>
        /// <param name="startDateIran">تاریخ شروع در Iran TimeZone</param>
        /// <param name="intervalDays">فاصله به روز (مثلاً 14 روز)</param>
        /// <param name="dayOfWeek">روز هفته (اختیاری) - 0=Sunday, 6=Saturday, null=هر روز</param>
        /// <param name="hour">ساعت اجرا</param>
        /// <param name="minute">دقیقه اجرا</param>
        /// <returns>زمان بعدی اجرا در Iran TimeZone</returns>
        private DateTime FindNextIntervalExecution(
            DateTime nowIran, 
            DateTime startDateIran, 
            int intervalDays, 
            int? dayOfWeek, 
            int hour, 
            int minute)
        {
            Console.WriteLine($"🔍 FindNextIntervalExecution: Start={startDateIran:yyyy-MM-dd HH:mm}, Interval={intervalDays}, DayOfWeek={dayOfWeek?.ToString() ?? "null"}");

            // اگر تاریخ شروع هنوز نرسیده
            if (startDateIran > nowIran)
            {
                // اگر روز هفته مشخص شده، باید منتظر آن روز بمانیم
                if (dayOfWeek.HasValue)
                {
                    var nextDate = FindNextDayOfWeek(startDateIran, dayOfWeek.Value, hour, minute);
                    Console.WriteLine($"✅ Start date in future + specific day: {nextDate:yyyy-MM-dd HH:mm}");
                    return nextDate;
                }
                
                Console.WriteLine($"✅ Start date in future: {startDateIran:yyyy-MM-dd HH:mm}");
                return startDateIran;
            }

            // محاسبه تعداد دوره‌های گذشته از تاریخ شروع
            var daysPassed = (nowIran.Date - startDateIran.Date).Days;
            var cyclesPassed = daysPassed / intervalDays;
            var nextCycle = cyclesPassed + 1;

            // محاسبه تاریخ دوره بعدی
            var nextExecutionDate = startDateIran.AddDays(nextCycle * intervalDays);

            // ⭐⭐⭐ اگر ساعت امروز هنوز نرسیده و امروز روز دوره است
            var todayWithTime = new DateTime(nowIran.Year, nowIran.Month, nowIran.Day, hour, minute, 0);
            if (daysPassed % intervalDays == 0 && todayWithTime > nowIran)
            {
                nextExecutionDate = todayWithTime;
                Console.WriteLine($"🔍 امروز روز دوره است و ساعت نرسیده: {nextExecutionDate:yyyy-MM-dd HH:mm}");
            }

            Console.WriteLine($"🔍 Calculated next date (without day restriction): {nextExecutionDate:yyyy-MM-dd HH:mm}");

            // اگر روز هفته خاصی مشخص شده
            if (dayOfWeek.HasValue)
            {
                // اگر تاریخ محاسبه شده روز هفته مورد نظر نیست
                if ((int)nextExecutionDate.DayOfWeek != dayOfWeek.Value)
                {
                    // پیدا کردن نزدیک‌ترین روز هفته مورد نظر بعد از این تاریخ
                    nextExecutionDate = FindNextDayOfWeek(nextExecutionDate, dayOfWeek.Value, hour, minute);
                    Console.WriteLine($"🔍 Adjusted for day of week: {nextExecutionDate:yyyy-MM-dd HH:mm}");
                }

                // ⭐⭐⭐ بررسی: آیا این تاریخ از تاریخ شروع فاصله مضربی از intervalDays دارد؟
                // اگر نه، باید به دوره بعدی برویم
                var daysFromStart = (nextExecutionDate.Date - startDateIran.Date).Days;
                if (daysFromStart % intervalDays != 0)
                {
                    // محاسبه نزدیک‌ترین دوره بعدی که در روز هفته مورد نظر باشد
                    nextExecutionDate = FindNextIntervalWithDayOfWeek(startDateIran, intervalDays, dayOfWeek.Value, nowIran, hour, minute);
                    Console.WriteLine($"🔍 Adjusted to next valid interval: {nextExecutionDate:yyyy-MM-dd HH:mm}");
                }
            }

            Console.WriteLine($"✅ Final next execution: {nextExecutionDate:yyyy-MM-dd HH:mm}");
            return nextExecutionDate;
        }

        /// <summary>
        /// ⭐⭐⭐ پیدا کردن نزدیک‌ترین روز هفته مورد نظر بعد از یک تاریخ
        /// </summary>
        private DateTime FindNextDayOfWeek(DateTime fromDate, int targetDayOfWeek, int hour, int minute)
        {
            var currentDayOfWeek = (int)fromDate.DayOfWeek;
            int daysToAdd;

            if (currentDayOfWeek == targetDayOfWeek)
            {
                // همان روز است
                daysToAdd = 0;
            }
            else if (currentDayOfWeek < targetDayOfWeek)
            {
                // روز هفته جلوتر در همین هفته
                daysToAdd = targetDayOfWeek - currentDayOfWeek;
            }
            else
            {
                // روز هفته در هفته بعد
                daysToAdd = 7 - currentDayOfWeek + targetDayOfWeek;
            }

            var resultDate = fromDate.AddDays(daysToAdd);
            return new DateTime(resultDate.Year, resultDate.Month, resultDate.Day, hour, minute, 0);
        }

        /// <summary>
        /// ⭐⭐⭐ پیدا کردن نزدیک‌ترین دوره که در روز هفته مورد نظر باشد
        /// </summary>
        private DateTime FindNextIntervalWithDayOfWeek(
            DateTime startDate, 
            int intervalDays, 
            int targetDayOfWeek, 
            DateTime afterDate,
            int hour,
            int minute)
        {
            // از afterDate شروع کن و تا 365 روز جستجو کن
            var searchDate = afterDate.Date;
            var maxSearchDays = 365; // حداکثر یک سال جستجو
            var searchCount = 0;

            while (searchCount < maxSearchDays)
            {
                // آیا این روز روز هفته مورد نظر است؟
                if ((int)searchDate.DayOfWeek == targetDayOfWeek)
                {
                    // آیا این روز از تاریخ شروع فاصله مضربی از intervalDays دارد؟
                    var daysFromStart = (searchDate - startDate.Date).Days;
                    if (daysFromStart >= 0 && daysFromStart % intervalDays == 0)
                    {
                        var resultWithTime = new DateTime(searchDate.Year, searchDate.Month, searchDate.Day, hour, minute, 0);
                        
                        // اگر در آینده است، برگردان
                        if (resultWithTime > afterDate)
                        {
                            return resultWithTime;
                        }
                    }
                }

                searchDate = searchDate.AddDays(1);
                searchCount++;
            }

            // اگر نتیجه‌ای پیدا نشد، یک دوره بعدی را برگردان (fallback)
            var fallback = startDate.AddDays(((afterDate.Date - startDate.Date).Days / intervalDays + 1) * intervalDays);
            return FindNextDayOfWeek(fallback, targetDayOfWeek, hour, minute);
        }

        /// <summary>
        /// ساخت JSON Template از TaskViewModel
        /// </summary>
        private string CreateTaskTemplateJson(TaskViewModel model)
        {
            // ⭐⭐⭐ فقط اطلاعات مورد نیاز ذخیره می‌شود
            var template = new
            {
                model.Title,
                model.Description,
                model.BranchIdSelected,
                model.Priority,
                model.Important,
                model.TaskType,
                model.EstimatedHours,
                model.TaskCategoryIdSelected,
                model.SelectedOrganizationId,
                model.SelectedContactId,
                model.DueDatePersian,
                model.SuggestedStartDatePersian,
                model.IsHardDeadline,
                model.TimeNote,
                Operations = model.Operations?.Select(o => new
                {
                    o.Title,
                    o.Description,
                    o.OperationOrder,
                    o.EstimatedHours
                }).ToList(),
                Assignments = model.UserTeamAssignmentsJson, // ⭐ JSON پیش‌ساخته
                Reminders = model.TaskRemindersJson // ⭐ JSON پیش‌ساخته
            };

            return JsonSerializer.Serialize(template, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = false
            });
        }

        /// <summary>
        /// دریافت لیست زمان‌بندی‌های کاربر (تفکیک شده: من + تیمی)
        /// </summary>
        public async Task<ScheduledTaskListViewModel> GetUserScheduledTasksAsync(
            string userId, 
            bool isAdmin = false)
        {
            Console.WriteLine($"🔍 GetUserScheduledTasksAsync called for userId: {userId}, isAdmin: {isAdmin}");
            
            // ⭐⭐⭐ بخش 1: تسک‌های زمان‌بندی شده که من سازنده‌شان هستم
            var mySchedulesQuery = _context.ScheduledTaskCreation_Tbl
                .Include(s => s.CreatedByUser)
                .Include(s => s.Branch)
                .Where(s => s.IsActive && s.CreatedByUserId == userId)
                .AsQueryable();

            var mySchedules = await mySchedulesQuery
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            Console.WriteLine($"✅ Found {mySchedules.Count} schedules created by user");

            // ⭐⭐⭐ بخش 2: تسک‌های زمان‌بندی شده که من عضو تیم هستم
            var teamSchedules = new List<ScheduledTaskCreation>();

            if (!isAdmin) // Admin نیازی به دیدن تسک‌های تیمی ندارد
            {
                // پیدا کردن Schedule هایی که:
                // 1. من سازنده‌شان نیستم
                // 2. در TaskDataJson، من جزو Assignments هستم
                var allSchedules = await _context.ScheduledTaskCreation_Tbl
                    .Include(s => s.CreatedByUser)
                    .Include(s => s.Branch)
                    .Where(s => s.IsActive && s.CreatedByUserId != userId)
                    .ToListAsync();

                foreach (var schedule in allSchedules)
                {
                    // بررسی JSON برای یافتن Assignments
                    if (IsUserInScheduleAssignments(schedule.TaskDataJson, userId))
                    {
                        teamSchedules.Add(schedule);
                    }
                }

                Console.WriteLine($"✅ Found {teamSchedules.Count} team schedules where user is assigned");
            }

            // ⭐⭐⭐ تبدیل به ViewModel
            var myCards = mySchedules.Select(s => 
            {
                var card = MapToScheduledTaskCard(s);
                card.IsCreatedByMe = true; // ⭐ من سازنده هستم
                return card;
            }).ToList();
            
            var teamCards = teamSchedules.Select(s => 
            {
                var card = MapToScheduledTaskCard(s);
                card.IsCreatedByMe = false; // ⭐ من عضو تیم هستم
                return card;
            }).ToList();
            
            // محاسبه آمار (فقط برای تسک‌های خودم)
            var stats = CalculateScheduledTaskStats(mySchedules);

            return new ScheduledTaskListViewModel
            {
                MyScheduledTasks = myCards,
                TeamScheduledTasks = teamCards,
#pragma warning disable CS0618
                ScheduledTasks = myCards, // Backward compatibility
#pragma warning restore CS0618
                Stats = stats
            };
        }

        /// <summary>
        /// بررسی اینکه آیا کاربر در Assignments این Schedule هست یا نه
        /// </summary>
        private bool IsUserInScheduleAssignments(string taskDataJson, string userId)
        {
            try
            {
                var taskModel = DeserializeTaskTemplate(taskDataJson);
                if (taskModel == null) return false;

                // بررسی UserTeamAssignmentsJson
                if (!string.IsNullOrEmpty(taskModel.UserTeamAssignmentsJson))
                {
                    // ساده: بررسی می‌کنیم که userId در JSON وجود دارد یا نه
                    if (taskModel.UserTeamAssignmentsJson.Contains($"\"{userId}\""))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ خطا در بررسی Assignments: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// دریافت جزئیات یک زمان‌بندی
        /// </summary>
        public async Task<ScheduledTaskCreation?> GetScheduleByIdAsync(int scheduleId)
        {
            return await _context.ScheduledTaskCreation_Tbl
                .Include(s => s.CreatedByUser)
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);
        }

        /// <summary>
        /// فعال/غیرفعال کردن زمان‌بندی
        /// </summary>
        public async Task ToggleScheduleAsync(int scheduleId, bool isEnabled)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.IsScheduleEnabled = isEnabled;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// حذف زمان‌بندی (نرم)
        /// </summary>
        public async Task DeleteScheduleAsync(int scheduleId)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return;

            schedule.IsActive = false;
            schedule.IsScheduleEnabled = false;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// دریافت تسک زمان‌بندی شده برای ویرایش
        /// </summary>
        public async Task<TaskViewModel?> GetScheduledTaskForEditAsync(int scheduleId)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return null;

            var taskModel = DeserializeTaskTemplate(schedule.TaskDataJson);
            if (taskModel == null) return null;

            // بازیابی اطلاعات زمان‌بندی
            taskModel.TaskSchedule = new TaskScheduleViewModel
            {
                IsScheduled = true,
                ScheduleTitle = schedule.ScheduleTitle,
                ScheduleDescription = schedule.ScheduleDescription,
                ScheduleType = schedule.ScheduleType,
                ScheduledTime = schedule.ScheduledTime,
                ScheduledDaysOfWeek = schedule.ScheduledDaysOfWeek,
#pragma warning disable CS0618
                ScheduledDayOfMonth = schedule.ScheduledDayOfMonth,
#pragma warning restore CS0618
                ScheduledDaysOfMonth = schedule.ScheduledDaysOfMonth, // ⭐⭐⭐ جدید
                IntervalDays = schedule.IntervalDays, // ⭐⭐⭐ جدید
                IntervalDayOfWeek = schedule.IntervalDayOfWeek, // ⭐⭐⭐ جدید
                StartDatePersian = schedule.StartDate.HasValue
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                        TimeZoneInfo.ConvertTimeFromUtc(schedule.StartDate.Value, IranTimeZone),
                        "yyyy/MM/dd HH:mm")
                    : null,
                EndDatePersian = schedule.EndDate.HasValue
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                        TimeZoneInfo.ConvertTimeFromUtc(schedule.EndDate.Value, IranTimeZone),
                        "yyyy/MM/dd HH:mm")
                    : null,
                MaxOccurrences = schedule.MaxOccurrences
            };

            return taskModel;
        }

        /// <summary>
        /// ⭐⭐⭐ بروزرسانی زمان‌بندی موجود - با الگوریتم جدید
        /// </summary>
        public async Task<bool> UpdateScheduledTaskAsync(
            int scheduleId, 
            TaskViewModel taskModel, 
            string userId)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return false;

            // بروزرسانی JSON
            var taskDataJson = CreateTaskTemplateJson(taskModel);

            schedule.ScheduleTitle = taskModel.TaskSchedule?.ScheduleTitle ?? taskModel.Title;
            schedule.ScheduleDescription = taskModel.TaskSchedule?.ScheduleDescription;
            schedule.TaskDataJson = taskDataJson;
            schedule.ScheduleType = taskModel.TaskSchedule?.ScheduleType ?? 0;
            schedule.ScheduledTime = taskModel.TaskSchedule?.ScheduledTime;
            schedule.ScheduledDaysOfWeek = taskModel.TaskSchedule?.ScheduledDaysOfWeek;
            schedule.ScheduledDaysOfMonth = taskModel.TaskSchedule?.ScheduledDaysOfMonth; // ⭐⭐⭐ جدید
#pragma warning disable CS0618
            schedule.ScheduledDayOfMonth = taskModel.TaskSchedule?.ScheduledDayOfMonth; // Backward compatibility
#pragma warning restore CS0618
            schedule.IntervalDays = taskModel.TaskSchedule?.IntervalDays; // ⭐⭐⭐ جدید
            schedule.IntervalDayOfWeek = taskModel.TaskSchedule?.IntervalDayOfWeek; // ⭐⭐⭐ جدید
            // ⭐⭐⭐ استفاده از متد nullable-safe
            schedule.StartDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(
                taskModel.TaskSchedule?.StartDatePersian);
            schedule.EndDate = CommonLayer.PublicClasses.ConvertDateTime.ConvertShamsiToMiladiNullable(
                taskModel.TaskSchedule?.EndDatePersian);
            schedule.MaxOccurrences = taskModel.TaskSchedule?.MaxOccurrences;
            schedule.BranchId = taskModel.BranchIdSelected;

            // ⭐⭐⭐ محاسبه مجدد NextExecutionDate با الگوریتم جدید
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IranTimeZone);
            schedule.NextExecutionDate = CalculateNextExecutionDate(taskModel.TaskSchedule, nowIran);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// دریافت زمان‌بندی‌های آماده برای اجرا توسط Background Service
        /// </summary>
        public async Task<List<ScheduledTaskCreation>> GetDueScheduledTasksAsync()
        {
            var nowUtc = DateTime.UtcNow;

            return await _context.ScheduledTaskCreation_Tbl
                .Where(s =>
                    s.IsActive &&
                    s.IsScheduleEnabled &&
                    s.NextExecutionDate.HasValue &&
                    s.NextExecutionDate.Value <= nowUtc &&
                    // ⭐ جلوگیری از اجرای مکرر: حداقل 1 دقیقه فاصله
                    (!s.LastExecutionDate.HasValue ||
                     EF.Functions.DateDiffMinute(s.LastExecutionDate.Value, nowUtc) >= 1))
                .ToListAsync();
        }

        /// <summary>
        /// ⭐⭐⭐ بروزرسانی وضعیت اجرا پس از ساخت تسک - با الگوریتم جدید
        /// </summary>
        public async Task UpdateExecutionStatusAsync(
            int scheduleId, 
            bool success, 
            string? notes = null)
        {
            var schedule = await _context.ScheduledTaskCreation_Tbl.FindAsync(scheduleId);
            if (schedule == null) return;

            var nowUtc = DateTime.UtcNow;
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, IranTimeZone);

            schedule.LastExecutionDate = nowUtc;
            schedule.ExecutionCount++;
            schedule.Notes = notes;

            // بررسی اتمام زمان‌بندی
            if (schedule.ScheduleType == 0) // OneTime
            {
                schedule.IsScheduleEnabled = false;
                schedule.NextExecutionDate = null;
            }
            else if (schedule.MaxOccurrences.HasValue && 
                     schedule.ExecutionCount >= schedule.MaxOccurrences.Value)
            {
                // تعداد دفعات به حد نصاب رسیده
                schedule.IsScheduleEnabled = false;
                schedule.NextExecutionDate = null;
            }
            else if (schedule.EndDate.HasValue && nowUtc >= schedule.EndDate.Value)
            {
                // تاریخ پایان رسیده
                schedule.IsScheduleEnabled = false;
                schedule.NextExecutionDate = null;
            }
            else
            {
                // ⭐⭐⭐ محاسبه اجرای بعدی با الگوریتم جدید
                var taskSchedule = new TaskScheduleViewModel
                {
                    ScheduleType = schedule.ScheduleType,
                    ScheduledTime = schedule.ScheduledTime,
                    ScheduledDaysOfWeek = schedule.ScheduledDaysOfWeek,
                    ScheduledDaysOfMonth = schedule.ScheduledDaysOfMonth,
#pragma warning disable CS0618
                    ScheduledDayOfMonth = schedule.ScheduledDayOfMonth,
#pragma warning restore CS0618
                    IntervalDays = schedule.IntervalDays, // ⭐⭐⭐ جدید
                    IntervalDayOfWeek = schedule.IntervalDayOfWeek, // ⭐⭐⭐ جدید
                    StartDatePersian = schedule.StartDate.HasValue
                        ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                            schedule.StartDate.Value, "yyyy/MM/dd HH:mm")
                        : null
                };

                schedule.NextExecutionDate = CalculateNextExecutionDate(taskSchedule, nowIran);
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        /// <summary>
        /// Map ScheduledTaskCreation به CardViewModel
        /// </summary>
        private ScheduledTaskCardViewModel MapToScheduledTaskCard(ScheduledTaskCreation schedule)
        {
            var taskModel = DeserializeTaskTemplate(schedule.TaskDataJson);

            return new ScheduledTaskCardViewModel
            {
                Id = schedule.Id,
                ScheduleTitle = schedule.ScheduleTitle,
                ScheduleDescription = schedule.ScheduleDescription,
                TaskTitle = taskModel?.Title ?? "نامشخص",
                ScheduleType = schedule.ScheduleType,
                ScheduledTime = schedule.ScheduledTime,
                ScheduledDaysOfWeek = schedule.ScheduledDaysOfWeek,
                ScheduledDayOfMonth = schedule.ScheduledDayOfMonth,
                StartDate = schedule.StartDate,
                StartDatePersian = schedule.StartDate.HasValue
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                        TimeZoneInfo.ConvertTimeFromUtc(schedule.StartDate.Value, IranTimeZone),
                        "yyyy/MM/dd HH:mm")
                    : null,
                EndDate = schedule.EndDate,
                EndDatePersian = schedule.EndDate.HasValue
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                        TimeZoneInfo.ConvertTimeFromUtc(schedule.EndDate.Value, IranTimeZone),
                        "yyyy/MM/dd HH:mm")
                    : null,
                NextExecutionDate = schedule.NextExecutionDate,
                NextExecutionDatePersian = schedule.NextExecutionDate.HasValue
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                        TimeZoneInfo.ConvertTimeFromUtc(schedule.NextExecutionDate.Value, IranTimeZone),
                        "yyyy/MM/dd HH:mm")
                    : null,
                LastExecutionDate = schedule.LastExecutionDate,
                LastExecutionDatePersian = schedule.LastExecutionDate.HasValue
                    ? CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                        TimeZoneInfo.ConvertTimeFromUtc(schedule.LastExecutionDate.Value, IranTimeZone),
                        "yyyy/MM/dd HH:mm")
                    : null,
                ExecutionCount = schedule.ExecutionCount,
                MaxOccurrences = schedule.MaxOccurrences,
                IsRecurring = schedule.IsRecurring,
                IsActive = schedule.IsActive,
                IsScheduleEnabled = schedule.IsScheduleEnabled,
                IsExecuted = schedule.IsExecuted,
                BranchId = schedule.BranchId,
                BranchName = schedule.Branch?.Name,
                CreatedByUserName = schedule.CreatedByUser != null
                    ? $"{schedule.CreatedByUser.FirstName} {schedule.CreatedByUser.LastName}"
                    : "نامشخص",
                CreatedDate = schedule.CreatedDate,
                CreatedDatePersian = CommonLayer.PublicClasses.ConvertDateTime.ConvertMiladiToShamsi(
                    TimeZoneInfo.ConvertTimeFromUtc(schedule.CreatedDate, IranTimeZone),
                    "yyyy/MM/dd HH:mm"),
                Priority = taskModel?.Priority ?? 0,
                Important = taskModel?.Important ?? false,
                TaskType = taskModel?.TaskType ?? 0,
                IsCreatedByMe = false // ⭐⭐⭐ پیش‌فرض false، Controller تنظیم می‌کند
            };
        }

        /// <summary>
        /// محاسبه آمار زمان‌بندی‌ها
        /// </summary>
        private ScheduledTaskStatsViewModel CalculateScheduledTaskStats(
            List<ScheduledTaskCreation> schedules)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            return new ScheduledTaskStatsViewModel
            {
                TotalScheduled = schedules.Count,
                ActiveCount = schedules.Count(s => s.IsScheduleEnabled),
                InactiveCount = schedules.Count(s => !s.IsScheduleEnabled),
                CompletedCount = schedules.Count(s => 
                    s.MaxOccurrences.HasValue && 
                    s.ExecutionCount >= s.MaxOccurrences.Value),
                PendingCount = schedules.Count(s => 
                    s.IsScheduleEnabled && 
                    (!s.MaxOccurrences.HasValue || 
                     s.ExecutionCount < s.MaxOccurrences.Value)),
                TodayCount = schedules.Count(s =>
                    s.NextExecutionDate.HasValue &&
                    s.NextExecutionDate.Value.Date == today),
                ThisWeekCount = schedules.Count(s =>
                    s.NextExecutionDate.HasValue &&
                    s.NextExecutionDate.Value >= today &&
                    s.NextExecutionDate.Value < today.AddDays(7))
            };
        }

        /// <summary>
        /// Deserialize کردن TaskTemplateJson
        /// </summary>
        private TaskViewModel? DeserializeTaskTemplate(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<TaskViewModel>(json, 
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// ⭐ متد public برای Deserialize (برای استفاده در Controller)
        /// </summary>
        public TaskViewModel? DeserializeTaskData(string taskDataJson)
        {
            return DeserializeTaskTemplate(taskDataJson);
        }
    }
}
