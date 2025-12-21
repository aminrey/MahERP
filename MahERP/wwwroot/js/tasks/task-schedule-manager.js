// ⭐⭐⭐ مدیریت زمان‌بندی تسک - TaskSchedule.js
$(document).ready(function() {
    console.log('✅ Task Schedule Manager initialized');

    // فعال/غیرفعال کردن زمان‌بندی
    $('#enableScheduleCheckbox').on('change', function() {
        const isEnabled = $(this).is(':checked');
        $('#scheduleContent').slideToggle(300);
        
        if (!isEnabled) {
            // پاک کردن تمام فیلدها
            $('#scheduleContent input[type="text"], #scheduleContent input[type="number"], #scheduleContent textarea').val('');
            $('#scheduleContent input[type="checkbox"]').prop('checked', false);
            $('#scheduleType').val('0');
            updateScheduleTypeOptions();
        }
        
        updateSchedulePreview();
        
        console.log('📅 Schedule enabled:', isEnabled);
    });

    // تغییر نوع زمان‌بندی
    $('#scheduleType').on('change', function() {
        updateScheduleTypeOptions();
        updateSchedulePreview();
    });

    // بروزرسانی چک‌باکس‌های روزهای هفته
    $('input[name="dayOfWeek"]').on('change', function() {
        const selectedDays = [];
        $('input[name="dayOfWeek"]:checked').each(function() {
            selectedDays.push($(this).val());
        });
        $('#scheduledDaysOfWeek').val(selectedDays.join(','));
        updateSchedulePreview();
        
        console.log('📆 Selected week days:', selectedDays);
    });

    // ⭐⭐⭐ بروزرسانی چک‌باکس‌های روزهای ماه
    $('input[name="dayOfMonth"]').on('change', function() {
        const selectedDays = [];
        $('input[name="dayOfMonth"]:checked').each(function() {
            selectedDays.push($(this).val());
        });
        $('#scheduledDaysOfMonth').val(selectedDays.join(','));
        updateSchedulePreview();
        
        console.log('📆 Selected month days:', selectedDays);
    });

    // ⭐⭐⭐ NEW: بروزرسانی رادیو باتن‌های روز هفته برای Interval
    $('input[name="intervalDayOfWeekRadio"]').on('change', function() {
        const selectedDay = $(this).val();
        $('#intervalDayOfWeekHidden').val(selectedDay);
        updateSchedulePreview();
        
        console.log('📆 Selected interval day of week:', selectedDay === '' ? 'Any day' : selectedDay);
    });

    // تغییر سایر فیلدها
    $('#scheduleContent input, #scheduleContent select, #scheduleContent textarea').on('change', updateSchedulePreview);

    // راه‌اندازی اولیه
    if ($('#enableScheduleCheckbox').is(':checked')) {
        $('#scheduleContent').show();
        updateScheduleTypeOptions();
        updateSchedulePreview();
    }
});

/**
 * بروزرسانی نمایش بخش‌های مختلف بر اساس نوع زمان‌بندی
 */
function updateScheduleTypeOptions() {
    const scheduleType = parseInt($('#scheduleType').val());
    
    // پنهان کردن همه
    $('#oneTimeOptions, #weeklyOptions, #monthlyOptions, #intervalOptions, #intervalDayOfWeekRow').hide();
    
    // نمایش بخش مربوطه
    switch(scheduleType) {
        case 0: // یکبار
            $('#oneTimeOptions').show();
            console.log('🕐 Schedule type: One-Time');
            break;
        case 1: // روزانه
            console.log('📅 Schedule type: Daily');
            break;
        case 2: // هفتگی
            $('#weeklyOptions').show();
            console.log('📆 Schedule type: Weekly');
            break;
        case 3: // ماهانه
            $('#monthlyOptions').show();
            console.log('📆 Schedule type: Monthly');
            break;
        case 4: // ⭐⭐⭐ Interval
            $('#intervalOptions').show();
            $('#intervalDayOfWeekRow').show();
            console.log('🔄 Schedule type: Interval');
            break;
    }
}

/**
 * پیش‌نمایش تنظیمات زمان‌بندی
 */
function updateSchedulePreview() {
    if (!$('#enableScheduleCheckbox').is(':checked')) {
        $('#schedulePreview').hide();
        return;
    }

    const scheduleType = parseInt($('#scheduleType').val());
    const scheduledTime = $('input[name="TaskSchedule.ScheduledTime"]').val();
    const startDate = $('input[name="TaskSchedule.StartDatePersian"]').val();
    const endDate = $('input[name="TaskSchedule.EndDatePersian"]').val();
    const maxOccurrences = $('input[name="TaskSchedule.MaxOccurrences"]').val();
    const createImmediately = $('input[name="TaskSchedule.CreateImmediately"]').is(':checked');

    let previewHtml = '<ul class="mb-0">';

    // نوع تکرار
    const typeText = ['یکبار', 'روزانه', 'هفتگی', 'ماهانه', 'تکرار با فاصله'][scheduleType];
    previewHtml += `<li><strong>نوع:</strong> ${typeText}`;

    // جزئیات بر اساس نوع
    switch(scheduleType) {
        case 0: // یکبار
            const oneTimeDate = $('input[name="TaskSchedule.OneTimeExecutionDatePersian"]').val();
            if (oneTimeDate && scheduledTime) {
                previewHtml += ` در تاریخ ${oneTimeDate} ساعت ${scheduledTime}`;
            }
            break;
            
        case 1: // روزانه
            if (scheduledTime) {
                previewHtml += ` هر روز ساعت ${scheduledTime}`;
            }
            break;
            
        case 2: // هفتگی
            const selectedWeekDays = [];
            const dayNames = ['یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه'];
            $('input[name="dayOfWeek"]:checked').each(function() {
                selectedWeekDays.push(dayNames[parseInt($(this).val())]);
            });
            if (selectedWeekDays.length > 0 && scheduledTime) {
                previewHtml += ` هر ${selectedWeekDays.join('، ')} ساعت ${scheduledTime}`;
            }
            break;
            
        case 3: // ماهانه
            // ⭐⭐⭐ FIX: بررسی چند روز یا یک روز
            const selectedMonthDays = [];
            $('input[name="dayOfMonth"]:checked').each(function() {
                selectedMonthDays.push(parseInt($(this).val()));
            });
            
            if (selectedMonthDays.length > 0 && scheduledTime) {
                previewHtml += ` روزهای ${selectedMonthDays.sort((a, b) => a - b).join('، ')} هر ماه ساعت ${scheduledTime}`;
            }
            break;
        
        case 4: // ⭐⭐⭐ Interval
            const intervalDays = $('input[name="TaskSchedule.IntervalDays"]').val();
            const intervalDayOfWeek = $('input[name="intervalDayOfWeekRadio"]:checked').val();
            
            if (intervalDays && scheduledTime) {
                previewHtml += ` هر ${intervalDays} روز ساعت ${scheduledTime}`;
                
                if (intervalDayOfWeek && intervalDayOfWeek !== '') {
                    const dayNames = ['یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه'];
                    const dayName = dayNames[parseInt(intervalDayOfWeek)];
                    previewHtml += ` (فقط ${dayName}ها)`;
                }
                
                if (startDate) {
                    previewHtml += ` از ${startDate}`;
                }
            }
            break;
    }
    previewHtml += '</li>';

    // بازه زمانی
    if (startDate && scheduleType !== 4) { // برای Interval قبلاً نمایش داده شد
        previewHtml += `<li><strong>شروع:</strong> ${startDate}</li>`;
    }
    if (endDate) {
        previewHtml += `<li><strong>پایان:</strong> ${endDate}</li>`;
    }

    // حداکثر تعداد
    if (maxOccurrences) {
        previewHtml += `<li><strong>حداکثر اجرا:</strong> ${maxOccurrences} بار</li>`;
    } else {
        previewHtml += `<li><strong>تکرار:</strong> نامحدود</li>`;
    }

    // ساخت فوری
    if (createImmediately) {
        previewHtml += `<li class="text-success"><strong><i class="fa fa-bolt me-1"></i>یک تسک همین الان ساخته می‌شود</strong></li>`;
    }

    previewHtml += '</ul>';

    $('#schedulePreviewContent').html(previewHtml);
    $('#schedulePreview').fadeIn();
}

// Export functions for external use
window.TaskScheduleManager = {
    updateScheduleTypeOptions: updateScheduleTypeOptions,
    updateSchedulePreview: updateSchedulePreview
};
