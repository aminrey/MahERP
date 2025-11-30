/**
 * Notification Templates Management
 * مدیریت الگوهای پیام
 */

$(document).ready(function () {
    // صفحه لیست
    if ($('.template-card').length > 0) {
        initializeTemplateList();
    }
});

/**
 * مقداردهی اولیه لیست الگوها
 */
function initializeTemplateList() {
    // Toggle وضعیت الگو
    $('.toggle-template-status').on('change', function () {
        const templateId = $(this).data('template-id');
        const isActive = $(this).is(':checked');
        toggleTemplateStatus(templateId, isActive, $(this));
    });

    // حذف الگو
    $('.delete-template').on('click', async function () {
        const templateId = $(this).data('template-id');
        const templateName = $(this).data('template-name');

        const confirmed = await showConfirmationSwal({
            title: 'حذف الگو',
            text: `آیا از حذف الگوی "${templateName}" اطمینان دارید؟`,
            icon: 'warning',
            confirmButtonText: 'بله، حذف شود',
            confirmButtonColor: '#dc3545'
        });

        if (confirmed) {
            deleteTemplate(templateId);
        }
    });
}

/**
 * فعال/غیرفعال کردن الگو
 */
function toggleTemplateStatus(templateId, isActive, $toggle) {
    $.ajax({
        url: '/AppCoreArea/NotificationTemplates/ToggleStatus',
        type: 'POST',
        data: {
            id: templateId,
            __RequestVerificationToken: getAntiForgeryToken()
        },
        success: function (response) {
            if (response.success) {
                // ⭐⭐⭐ FIX: استفاده امن
                if (typeof NotificationHelper !== 'undefined' && NotificationHelper.success) {
                    NotificationHelper.success(response.message);
                }
                
                // بروزرسانی UI
                const $card = $(`.template-card[data-template-id="${templateId}"]`);
                if (response.isActive) {
                    $card.removeClass('opacity-50');
                } else {
                    $card.addClass('opacity-50');
                }
            } else {
                // ⭐⭐⭐ FIX: استفاده امن
                if (typeof NotificationHelper !== 'undefined' && NotificationHelper.error) {
                    NotificationHelper.error(response.message);
                }
                // برگرداندن به حالت قبل
                $toggle.prop('checked', !isActive);
            }
        },
        error: function () {
            // ⭐⭐⭐ FIX: استفاده امن
            if (typeof NotificationHelper !== 'undefined' && NotificationHelper.error) {
                NotificationHelper.error('خطا در تغییر وضعیت');
            }
            $toggle.prop('checked', !isActive);
        }
    });
}

/**
 * حذف الگو
 */
function deleteTemplate(templateId) {
    $.ajax({
        url: '/AppCoreArea/NotificationTemplates/Delete',
        type: 'POST',
        data: {
            id: templateId,
            __RequestVerificationToken: getAntiForgeryToken()
        },
        success: function (response) {
            if (response.status === 'success') {
                SendResposeMessage(response.message);
                
                // حذف کارت از DOM با انیمیشن
                const $card = $(`.template-card[data-template-id="${templateId}"]`).closest('.col-md-6');
                $card.fadeOut(300, function () {
                    $(this).remove();
                    
                    // بررسی خالی بودن لیست
                    if ($('.template-card').length === 0) {
                        location.reload();
                    }
                });
            } else {
                SendResposeMessage(response.message);
            }
        },
        error: function () {
            // ⭐⭐⭐ FIX: استفاده امن
            if (typeof NotificationHelper !== 'undefined' && NotificationHelper.error) {
                NotificationHelper.error('خطا در حذف الگو');
            }
        }
    });
}

/**
 * ========================================
 * فرم ایجاد/ویرایش الگو
 * ========================================
 */

function initializeTemplateForm(isEditMode = false) {
    console.log('🔧 initializeTemplateForm called. EditMode:', isEditMode);

    // ⭐⭐⭐ تغییر کانال ارسال (Channel)
    $('#channelTypeSelect, #ChannelSelect').on('change', function () {
        handleChannelTypeChange($(this).val());
    });

    // اگر مقدار اولیه دارد
    const initialChannel = $('#channelTypeSelect').val() || $('#ChannelSelect').val();
    if (initialChannel) {
        handleChannelTypeChange(initialChannel);
    }

    // ⭐⭐⭐ Event Listener برای تغییر نوع اعلان (NotificationEventType)
    $('#notificationTypeSelect').off('change').on('change', function () {
        const eventType = parseInt($(this).val());
        const selectedOption = $(this).find('option:selected');
        const isSchedulable = selectedOption.data('schedulable') === true;

        console.log('🎯 NotificationEventType changed:', eventType, 'Schedulable:', isSchedulable);
        
        // ⭐ فیلتر متغیرها بر اساس EventType
        if (typeof window.filterAndDisplayVariables === 'function') {
            window.filterAndDisplayVariables(eventType);
        } else {
            console.warn('⚠️ filterAndDisplayVariables function not found!');
        }

        // نمایش/مخفی کردن بخش زمان‌بندی
        if (isSchedulable) {
            $('#schedulingBlock').slideDown();
        } else {
            $('#schedulingBlock').slideUp();
            $('#isScheduledSwitch').prop('checked', false).trigger('change');
        }
    });

    // درج متغیر
    $('.variable-item').on('click', function () {
        const variableName = $(this).data('variable-name');
        insertVariable(variableName);
    });

    // Submit فرم
    $('#templateForm').on('submit', function (e) {
        e.preventDefault();
        saveTemplate(isEditMode);
    });
}

/**
 * ⭐⭐⭐ RENAME: تغییر نام از handleTemplateTypeChange به handleChannelTypeChange
 * مدیریت تغییر کانال ارسال (Email/SMS/Telegram)
 */
function handleChannelTypeChange(channelType) {
    console.log('📡 Channel Type Changed:', channelType);
    
    const $subjectField = $('#subjectField');
    const $htmlEditorField = $('#htmlEditorField');
    
    if (channelType === '1') { // Email
        $subjectField.slideDown();
        $htmlEditorField.slideDown();
        
        // راه‌اندازی TinyMCE برای ایمیل
        if (typeof initEmailTemplateEditor !== 'undefined') {
            setTimeout(() => {
                initEmailTemplateEditor('htmlBodyEditor', 'محتوای HTML ایمیل را اینجا بنویسید...');
            }, 100);
        }
    } else {
        $subjectField.slideUp();
        $htmlEditorField.slideUp();
        
        // حذف TinyMCE
        if (typeof tinymce !== 'undefined') {
            tinymce.remove('#htmlBodyEditor');
        }
    }
}

/**
 * درج متغیر در textarea
 */
function insertVariable(variableName) {
    const $textarea = $('#bodyTextarea');
    const cursorPos = $textarea[0].selectionStart;
    const textBefore = $textarea.val().substring(0, cursorPos);
    const textAfter = $textarea.val().substring(cursorPos);
    
    const variableText = `{{${variableName}}}`;
    $textarea.val(textBefore + variableText + textAfter);
    
    // جابجایی cursor
    const newPos = cursorPos + variableText.length;
    $textarea[0].setSelectionRange(newPos, newPos);
    $textarea.focus();
    
    // ⭐⭐⭐ FIX: استفاده امن از NotificationHelper
    try {
        if (typeof NotificationHelper !== 'undefined' && NotificationHelper.success) {
            NotificationHelper.success(`متغیر {{${variableName}}} درج شد`);
        } else {
            console.log(`✅ Variable {{${variableName}}} inserted`);
        }
    } catch (e) {
        console.log(`✅ Variable {{${variableName}}} inserted`);
    }
}

/**
 * ذخیره الگو
 */
function saveTemplate(isEditMode) {
    const $btn = $('#saveTemplateBtn');
    const originalText = $btn.html();
    
    $btn.prop('disabled', true)
        .html('<i class="fa fa-spinner fa-spin me-1"></i>در حال ذخیره...');

    // TinyMCE save
    if (typeof tinymce !== 'undefined') {
        tinymce.triggerSave();
    }

    const formData = $('#templateForm').serialize();

    $.ajax({
        url: $('#templateForm').attr('action'),
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.status === 'redirect') {
                SendResposeMessage(response.message);
                setTimeout(() => {
                    window.location.href = response.redirectUrl;
                }, 500);
            } else if (response.status === 'validation-error') {
                SendResposeMessage(response.message);
            } else {
                SendResposeMessage(response.message);
            }
        },
        error: function (xhr) {
            // ⭐⭐⭐ FIX: استفاده امن
            if (typeof NotificationHelper !== 'undefined' && NotificationHelper.error) {
                NotificationHelper.error('خطا در ذخیره الگو');
            }
        },
        complete: function () {
            $btn.prop('disabled', false).html(originalText);
        }
    });
}

/**
 * Expose globally
 */
window.initializeTemplateForm = initializeTemplateForm;