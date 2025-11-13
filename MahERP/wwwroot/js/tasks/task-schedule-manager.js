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
        
        console.log('📆 Selected days:', selectedDays);
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
    $('#oneTimeOptions, #weeklyOptions, #monthlyOptions').hide();
    
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
    const typeText = ['یکبار', 'روزانه', 'هفتگی', 'ماهانه'][scheduleType];
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
            const selectedDays = [];
            const dayNames = ['یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه'];
            $('input[name="dayOfWeek"]:checked').each(function() {
                selectedDays.push(dayNames[parseInt($(this).val())]);
            });
            if (selectedDays.length > 0 && scheduledTime) {
                previewHtml += ` هر ${selectedDays.join('، ')} ساعت ${scheduledTime}`;
            }
            break;
            
        case 3: // ماهانه
            const dayOfMonth = $('input[name="TaskSchedule.ScheduledDayOfMonth"]').val();
            if (dayOfMonth && scheduledTime) {
                previewHtml += ` روز ${dayOfMonth} هر ماه ساعت ${scheduledTime}`;
            }
            break;
    }
    previewHtml += '</li>';

    // بازه زمانی
    if (startDate) {
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
