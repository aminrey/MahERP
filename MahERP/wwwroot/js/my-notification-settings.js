/**
 * My Notification Settings
 * تنظیمات شخصی اعلان‌های کاربر
 */

$(document).ready(function () {
    initializeMyNotificationSettings();
});

function initializeMyNotificationSettings() {
    // نمایش/مخفی کردن فیلد زمان ارسال تجمیعی
    $('input[name*="DeliveryMode"]').on('change', function () {
        const $container = $(this).closest('.col-md-6');
        const $timeContainer = $container.find('.delivery-time-container');
        
        if ($(this).val() === '1' && $(this).is(':checked')) {
            $timeContainer.slideDown();
        } else if ($(this).val() === '0' && $(this).is(':checked')) {
            $timeContainer.slideUp();
        }
    });

    // ذخیره تنظیمات
    $('#savePreferencesBtn').on('click', function () {
        saveUserPreferences();
    });

    // بازنشانی به پیش‌فرض
    $('#resetToDefaultBtn').on('click', async function () {
        const confirmed = await showConfirmationSwal({
            title: 'بازنشانی تنظیمات',
            text: 'آیا از بازنشانی تمام تنظیمات به حالت پیش‌فرض اطمینان دارید؟',
            icon: 'warning',
            confirmButtonText: 'بله، بازنشانی شود',
            cancelButtonText: 'خیر'
        });

        if (confirmed) {
            resetToDefault();
        }
    });

    // غیرفعال/فعال کردن فیلدها بر اساس checkbox اصلی
    $('input[id^="IsEnabled_"]').on('change', function () {
        const $prefItem = $(this).closest('.notification-preference-item');
        const isEnabled = $(this).is(':checked');
        
        $prefItem.find('input, select').not(this).prop('disabled', !isEnabled);
        
        if (isEnabled) {
            $prefItem.removeClass('opacity-50');
        } else {
            $prefItem.addClass('opacity-50');
        }
    });

    // اعمال وضعیت اولیه
    $('input[id^="IsEnabled_"]').each(function () {
        $(this).trigger('change');
    });
}

/**
 * ذخیره تنظیمات کاربر
 */
function saveUserPreferences() {
    const $btn = $('#savePreferencesBtn');
    const originalText = $btn.html();
    
    $btn.prop('disabled', true)
        .html('<i class="fa fa-spinner fa-spin me-2"></i>در حال ذخیره...');

    const formData = $('#userPreferencesForm').serialize();

    $.ajax({
        url: '/TaskingArea/MyNotificationSettings/SavePreferences',
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.status === 'success') {
                SendResposeMessage(response.message);
            } else {
                SendResposeMessage(response.message);
            }
        },
        error: function (xhr) {
            NotificationHelper.error('خطا در ذخیره تنظیمات');
        },
        complete: function () {
            $btn.prop('disabled', false).html(originalText);
        }
    });
}

/**
 * بازنشانی به تنظیمات پیش‌فرض
 */
function resetToDefault() {
    $.ajax({
        url: '/TaskingArea/MyNotificationSettings/ResetToDefault',
        type: 'POST',
        data: {
            __RequestVerificationToken: getAntiForgeryToken()
        },
        success: function (response) {
            if (response.status === 'success') {
                SendResposeMessage(response.message);
                
                if (response.reload) {
                    setTimeout(() => {
                        location.reload();
                    }, 1000);
                }
            } else {
                SendResposeMessage(response.message);
            }
        },
        error: function (xhr) {
            NotificationHelper.error('خطا در بازنشانی تنظیمات');
        }
    });
}