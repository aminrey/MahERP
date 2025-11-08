/**
 * Notification Settings Management
 * مدیریت تنظیمات سیستم اعلان‌رسانی
 */

$(document).ready(function () {
    initializeNotificationSettings();
});

function initializeNotificationSettings() {
    // ذخیره تنظیمات کلی
    $('#saveGlobalSettings').on('click', function () {
        saveGlobalSettings();
    });

    // Toggle نوع اعلان
    $('.toggle-notification-type').on('change', function () {
        const typeId = $(this).data('type-id');
        const isActive = $(this).is(':checked');
        toggleNotificationType(typeId, isActive);
    });

    // Toggle کانال
    $('.toggle-channel').on('click', function () {
        const $btn = $(this);
        const typeId = $btn.data('type-id');
        const channel = $btn.data('channel');
        const isActive = !$btn.hasClass('active');

        toggleChannel(typeId, channel, isActive, $btn);
    });
}

/**
 * ذخیره تنظیمات کلی
 */
function saveGlobalSettings() {
    const data = {
        IsEmailEnabled: $('#emailEnabled').is(':checked'),
        IsSmsEnabled: $('#smsEnabled').is(':checked'),
        IsTelegramEnabled: $('#telegramEnabled').is(':checked'),
        MaxEmailPerUserPerDay: parseInt($('#maxEmailPerDay').val()),
        MaxSmsPerUserPerDay: parseInt($('#maxSmsPerDay').val()),
        MaxTelegramPerUserPerDay: parseInt($('#maxTelegramPerDay').val()),
        __RequestVerificationToken: getAntiForgeryToken()
    };

    $.ajax({
        url: '/AppCoreArea/NotificationSettings/SaveGlobalSettings',
        type: 'POST',
        data: data,
        success: function (response) {
            if (response.status === 'success') {
                SendResposeMessage(response.message);
            } else {
                SendResposeMessage(response.message);
            }
        },
        error: function (xhr) {
            NotificationHelper.error('خطا در ذخیره تنظیمات');
        }
    });
}

/**
 * فعال/غیرفعال کردن نوع اعلان
 */
function toggleNotificationType(typeId, isActive) {
    $.ajax({
        url: '/AppCoreArea/NotificationSettings/ToggleNotificationType',
        type: 'POST',
        data: {
            typeId: typeId,
            isActive: isActive,
            __RequestVerificationToken: getAntiForgeryToken()
        },
        success: function (response) {
            if (response.status === 'success') {
                SendResposeMessage(response.message);
                
                // بروزرسانی UI
                const $row = $(`tr[data-type-id="${typeId}"]`);
                if (isActive) {
                    $row.removeClass('opacity-50');
                } else {
                    $row.addClass('opacity-50');
                }
            } else {
                SendResposeMessage(response.message);
                // برگرداندن به حالت قبل
                $(`.toggle-notification-type[data-type-id="${typeId}"]`)
                    .prop('checked', !isActive);
            }
        },
        error: function (xhr) {
            NotificationHelper.error('خطا در تغییر وضعیت');
            // برگرداندن به حالت قبل
            $(`.toggle-notification-type[data-type-id="${typeId}"]`)
                .prop('checked', !isActive);
        }
    });
}

/**
 * فعال/غیرفعال کردن کانال
 */
function toggleChannel(typeId, channel, isActive, $btn) {
    $.ajax({
        url: '/AppCoreArea/NotificationSettings/ToggleChannel',
        type: 'POST',
        data: {
            typeId: typeId,
            channel: channel,
            isActive: isActive,
            __RequestVerificationToken: getAntiForgeryToken()
        },
        success: function (response) {
            if (response.status === 'success') {
                SendResposeMessage(response.message);
                
                // بروزرسانی UI
                if (isActive) {
                    $btn.addClass('active');
                } else {
                    $btn.removeClass('active');
                }
            } else {
                SendResposeMessage(response.message);
            }
        },
        error: function (xhr) {
            NotificationHelper.error('خطا در تغییر وضعیت کانال');
        }
    });
}