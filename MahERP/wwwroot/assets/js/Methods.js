/**
 * افزودن سریع تسک به "روز من" با SweetAlert
 * @param {number} taskId - شناسه تسک
 * @param {string} taskTitle - عنوان تسک (اختیاری)
 */
async function quickAddToMyDay(taskId, taskTitle = null) {
    try {
        // ⭐ نمایش SweetAlert تایید
        const confirmed = await showConfirmationSwal({
            title: 'افزودن به روز من',
            text: taskTitle
                ? `آیا می‌خواهید تسک "${taskTitle}" را به روز من اضافه کنید؟`
                : 'آیا می‌خواهید این تسک را به روز من اضافه کنید؟',
            icon: 'question',
            confirmButtonText: '<i class="fa fa-calendar-day me-1"></i> بله، اضافه کن',
            cancelButtonText: '<i class="fa fa-times me-1"></i> خیر',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d'
        });

        if (!confirmed) {
            return; // کاربر انصراف داد
        }

        // نمایش لودینگ
        Swal.fire({
            title: 'در حال افزودن...',
            text: 'لطفاً صبر کنید',
            icon: 'info',
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        // ارسال درخواست AJAX
        const token = $('input[name="__RequestVerificationToken"]').val();

        const response = await $.ajax({
            url: '/AdminArea/Tasks/QuickAddToMyDay',
            type: 'POST',
            data: {
                taskId: taskId,
                __RequestVerificationToken: token
            }
        });

        console.log('QuickAddToMyDay Response:', response);

        if (response.success) {
            // ✅ نمایش پیام موفقیت
            await Swal.fire({
                title: 'موفق!',
                html: response.message || 'تسک با موفقیت به روز من اضافه شد',
                icon: 'success',
                confirmButtonText: 'باشه',
                timer: 3000,
                timerProgressBar: true,
                customClass: {
                    confirmButton: 'btn btn-success'
                },
                buttonsStyling: false
            });
            // ⭐⭐⭐ تغییر دکمه در صفحه Details
            const currentPath = window.location.pathname.toLowerCase();
            if (currentPath.includes('/tasks/details/')) {
                // پیدا کردن دکمه در dropdown
                const $dropdownItem = $(`.dropdown-item[onclick*="quickAddToMyDay(${taskId}"]`);

                if ($dropdownItem.length) {
                    // تبدیل به حالت غیرفعال
                    $dropdownItem
                        .addClass('disabled')
                        .css({
                            'cursor': 'not-allowed',
                            'opacity': '0.6',
                            'background-color': 'rgba(25, 135, 84, 0.05)',
                            'border-left': '3px solid #198754'
                        })
                        .attr('onclick', '')
                        .attr('title', 'این تسک در روز من شما قرار دارد')
                        .html(`
                            <i class="fa fa-check-circle text-success me-2"></i>
                            <span class="text-success fw-bold">در روز من قرار دارد</span>
                        `);
                }
            }

        } else {
            // ❌ نمایش پیام خطا
            let errorMessage = response.message || 'خطا در افزودن تسک';

            // بررسی اینکه آیا تسک قبلاً اضافه شده
            if (response.alreadyExists) {
                await Swal.fire({
                    title: 'توجه',
                    text: errorMessage,
                    icon: 'info',
                    confirmButtonText: 'باشه',
                    customClass: {
                        confirmButton: 'btn btn-info'
                    },
                    buttonsStyling: false
                });
            } else {
                showErrorSwal('خطا', errorMessage);
            }
        }
    } catch (error) {
        console.error('Error in quickAddToMyDay:', error);

        let errorMessage = 'خطا در ارتباط با سرور';
        if (error.responseJSON && error.responseJSON.message) {
            errorMessage = error.responseJSON.message;
        } else if (error.statusText) {
            errorMessage = error.statusText;
        }

        showErrorSwal('خطا', errorMessage);
    }
}

// ⭐ Expose به window برای استفاده global
window.quickAddToMyDay = quickAddToMyDay;

// ========================================
// ⭐⭐⭐ تابع جامع برای بروزرسانی View (قابل استفاده مجدد)
// ========================================
/**
 * بروزرسانی محتوای المان‌ها بر اساس پاسخ سرور
 * @param {Object} response - پاسخ سرور با فرمت { status: "update-view", viewList: [...] }
 * @returns {boolean} - موفقیت یا عدم موفقیت
 */
function updateViewFromResponse(response) {
    try {
        // بررسی ساختار پاسخ
        if (!response || response.status !== 'update-view') {
            console.warn('⚠️ Response is not in update-view format:', response);
            return false;
        }

        if (!response.viewList || !Array.isArray(response.viewList)) {
            console.warn('⚠️ viewList is missing or invalid:', response);
            return false;
        }

        console.log('🔄 Updating views from response:', response);

        // پردازش هر یک از viewList
        response.viewList.forEach((viewItem, index) => {
            try {
                const elementId = viewItem.elementId;
                const html = viewItem.view?.result;
                const appendMode = viewItem.appendMode || false;

                console.log(`📝 Processing view ${index + 1}:`, {
                    elementId,
                    appendMode,
                    hasHtml: !!html
                });

                // بررسی وجود HTML
                if (!html || html.trim() === '') {
                    console.warn(`⚠️ Empty HTML for element: ${elementId}`);
                    return;
                }

                // پیدا کردن المان هدف
                const $targetElement = $(`#${elementId}`);

                if (!$targetElement.length) {
                    console.warn(`⚠️ Element not found: #${elementId}`);
                    return;
                }

                // بروزرسانی محتوا
                if (appendMode) {
                    // ⭐ حالت Append
                    console.log(`➕ Appending to: #${elementId}`);
                    $targetElement.append(html);
                } else {
                    // ⭐ حالت Replace
                    console.log(`🔄 Replacing: #${elementId}`);
                    
                    // اضافه کردن انیمیشن fade
                    $targetElement.fadeOut(150, function() {
                        $(this).html(html).fadeIn(150);
                    });
                }

                // ✅ موفقیت
                console.log(`✅ Successfully updated: #${elementId}`);

            } catch (itemError) {
                console.error(`❌ Error processing view item ${index + 1}:`, itemError);
            }
        });

        // بازگشت موفقیت
        return true;

    } catch (error) {
        console.error('❌ Error in updateViewFromResponse:', error);
        return false;
    }
}

// ⭐ Expose به window برای استفاده global
window.updateViewFromResponse = updateViewFromResponse;

// ========================================
// ⭐ تنظیم فوکوس - اصلاح شده با استفاده از updateViewFromResponse
// ========================================
function setTaskFocus(taskId, fromList = false) {
    Swal.fire({
        title: 'تنظیم فوکوس',
        text: 'آیا میخواهید این تسک را به عنوان فوکوس اصلی خود انتخاب کنید؟',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'بله، تنظیم کن',
        cancelButtonText: 'انصراف',
        customClass: {
            confirmButton: 'btn btn-success',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/TaskingArea/Tasks/SetTaskFocus',
                type: 'POST',
                data: {
                    taskId: taskId,
                    fromList: fromList,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    console.log('SetTaskFocus Response:', response);

                    // ⭐⭐⭐ استفاده از تابع جدید برای بروزرسانی View
                    if (response.status === 'update-view' && fromList) {
                        const updated = updateViewFromResponse(response);
                        
                        if (updated) {
                            // نمایش پیام موفقیت
                            showSuccessMessage(response.message);
                        } else {
                            console.warn('⚠️ View update failed, reloading page...');
                            setTimeout(() => location.reload(), 500);
                        }
                    } 
                    // ⭐ پاسخ معمولی (از صفحه Details)
                    else if (response.success) {
                        showSuccessMessage(response.message || 'تسک با موفقیت به عنوان فوکوس تنظیم شد');
                        setTimeout(() => location.reload(), 1000);
                    } else {
                        showErrorMessage(response.message || 'خطا در تنظیم فوکوس');
                    }
                },
                error: function (xhr) {
                    console.error('Error in setTaskFocus:', xhr);
                    showErrorMessage('خطا در ارتباط با سرور');
                }
            });
        }
    });
}

// ========================================
// ⭐ حذف فوکوس - اصلاح شده با استفاده از updateViewFromResponse
// ========================================
function removeTaskFocus(taskId, fromList = false) {
    $.ajax({
        url: '/TaskingArea/Tasks/RemoveTaskFocus',
        type: 'POST',
        data: {
            taskId: taskId,
            fromList: fromList,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            console.log('RemoveTaskFocus Response:', response);

            // ⭐⭐⭐ استفاده از تابع جدید برای بروزرسانی View
            if (response.status === 'update-view' && fromList) {
                const updated = updateViewFromResponse(response);
                
                if (updated) {
                    // نمایش پیام موفقیت
                    showSuccessMessage(response.message);
                } else {
                    console.warn('⚠️ View update failed, reloading page...');
                    setTimeout(() => location.reload(), 500);
                }
            }
            // ⭐ پاسخ معمولی (از صفحه Details)
            else if (response.success) {
                showSuccessMessage(response.message || 'فوکوس با موفقیت حذف شد');
                setTimeout(() => location.reload(), 1000);
            } else {
                showErrorMessage(response.message || 'خطا در حذف فوکوس');
            }
        },
        error: function (xhr) {
            console.error('Error in removeTaskFocus:', xhr);
            showErrorMessage('خطا در ارتباط با سرور');
        }
    });
}

// ========================================
// ⭐⭐⭐ توابع کمکی برای نمایش پیام
// ========================================
/**
 * نمایش پیام موفقیت
 * @param {string|Array} message - پیام یا آرایه‌ای از پیام‌ها
 */
function showSuccessMessage(message) {
    let messageText = '';
    
    if (Array.isArray(message)) {
        messageText = message.map(m => m.text || m).join('<br>');
    } else if (typeof message === 'object' && message.text) {
        messageText = message.text;
    } else {
        messageText = message || 'عملیات با موفقیت انجام شد';
    }

    // استفاده از سیستم‌های مختلف notification
    if (typeof toastr !== 'undefined') {
        toastr.success(messageText);
    } else if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'موفق!',
            html: messageText,
            icon: 'success',
            confirmButtonText: 'باشه',
            timer: 2000,
            timerProgressBar: true
        });
    } else {
        alert(messageText);
    }
}

/**
 * نمایش پیام خطا
 * @param {string|Array} message - پیام یا آرایه‌ای از پیام‌ها
 */
function showErrorMessage(message) {
    let messageText = '';
    
    if (Array.isArray(message)) {
        messageText = message.map(m => m.text || m).join('<br>');
    } else if (typeof message === 'object' && message.text) {
        messageText = message.text;
    } else {
        messageText = message || 'خطایی رخ داده است';
    }

    // استفاده از سیستم‌های مختلف notification
    if (typeof toastr !== 'undefined') {
        toastr.error(messageText);
    } else if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'خطا',
            html: messageText,
            icon: 'error',
            confirmButtonText: 'باشه'
        });
    } else {
        alert(messageText);
    }
}

// ⭐ Expose به window برای استفاده global
window.setTaskFocus = setTaskFocus;
window.removeTaskFocus = removeTaskFocus;
window.showSuccessMessage = showSuccessMessage;
window.showErrorMessage = showErrorMessage;

// ========================================
// ⭐⭐⭐ NESTED MODAL SUPPORT
// ========================================

/**
 * ⭐⭐⭐ ایجاد و نمایش مودال تو در تو (Nested Modal)
 * برای استفاده در QuickAdd و سایر مواردی که نیاز به Modal داخل Modal دارند
 * 
 * @param {string|object} urlOrOptions - URL یا شیء تنظیمات
 * @param {object} options - تنظیمات اضافی
 * @returns {Promise} Promise که با نمایش modal resolve می‌شود
 */
function createAndShowNestedModal(urlOrOptions, options = {}) {
    // ⭐ Parse parameters
    let config = {};

    if (typeof urlOrOptions === 'string') {
        config = { url: urlOrOptions, ...options };
    } else if (typeof urlOrOptions === 'object') {
        config = { ...urlOrOptions };
    } else {
        console.error('❌ Invalid parameter type for createAndShowNestedModal');
        return Promise.reject(new Error('Invalid parameter'));
    }

    // ⭐ Default config for nested modals
    const defaults = {
        url: null,
        modalId: 'nested-modal-' + Date.now(),
        backdrop: false, // ⭐ مهم: بدون backdrop برای modal های تو در تو
        keyboard: true,
        removeOnHide: true,
        onShown: null,
        onHidden: null,
        onSubmitSuccess: null,
        onLoadError: null
    };

    config = { ...defaults, ...config };

    console.log('🎯 Opening nested modal:', config.modalId);

    // ⭐ پیدا کردن parent modal
    const $parentModal = $('.modal.show').last();
    const parentZIndex = parseInt($parentModal.css('z-index') || 1050);
    
    // ⭐⭐⭐ ساختار مودال تو در تو
    const modalHtml = `<div class="modal fade" id="${config.modalId}" tabindex="-1" role="dialog" aria-hidden="true" data-bs-focus="false"></div>`;

    // ⭐ اضافه کردن به DOM
    $('body').append(modalHtml);
    const $modal = $('#' + config.modalId);

    return new Promise((resolve, reject) => {

        $.ajax({
            url: config.url,
            type: 'GET',
            dataType: 'html',
            beforeSend: function () {
                console.log('📤 Loading nested modal from:', config.url);
            },
            success: function (data) {
                try {
                    // ⭐ بررسی خطای JSON
                    if (typeof data === 'string' && data.trim().startsWith('{')) {
                        try {
                            const errorResponse = JSON.parse(data);
                            if (errorResponse.status === 'error') {
                                throw new Error(errorResponse.message?.[0]?.text || 'خطا در بارگذاری محتوا');
                            }
                        } catch (parseError) {
                            // Not JSON, continue
                        }
                    }

                    // ⭐ اعتبارسنجی محتوا
                    if (!data || data.trim() === '' || data === '{}') {
                        throw new Error('محتوای مودال خالی است');
                    }

                    console.log('✅ Nested modal content loaded');

                    // ⭐ درج محتوا
                    $modal.html(data);

                    // ⭐ پردازش URL ها
                    if (typeof ModalUtils !== 'undefined' && ModalUtils.processUrlsInContainer) {
                        try {
                            ModalUtils.processUrlsInContainer($modal);
                        } catch (err) {
                            console.warn('⚠️ ModalUtils error:', err);
                        }
                    }

                    // ⭐ راه‌اندازی Select2
                    if (typeof DynamicSelect2Manager !== 'undefined') {
                        try {
                            $modal.find('.js-select2').attr('data-container', '#' + config.modalId);
                            DynamicSelect2Manager.reinitializeSelect2InDiv($modal);
                        } catch (err) {
                            console.warn('⚠️ Select2 error:', err);
                        }
                    }

                    // ⭐ راه‌اندازی Persian Datepicker
                    if (typeof $.fn.persianDatepicker !== 'undefined') {
                        try {
                            $modal.find('.js-flatpickr').persianDatepicker({
                                initialValue: true,
                                format: 'YYYY/MM/DD',
                                autoClose: true
                            });
                        } catch (err) {
                            console.warn('⚠️ PersianDatepicker error:', err);
                        }
                    }

                    // ⭐⭐⭐ نمایش nested modal (بدون backdrop)
                    const modalInstance = new bootstrap.Modal($modal[0], {
                        backdrop: config.backdrop, // false برای nested
                        keyboard: config.keyboard
                    });

                    modalInstance.show();

                    // ⭐⭐⭐ حذف setupModalFormHandler - از document-level handler در main.js استفاده می‌شود
                    // setupModalFormHandler فقط تداخل ایجاد می‌کند

                    // ⭐ Event: shown
                    $modal.on('shown.bs.modal', function () {
                        console.log('✅ Nested modal shown:', config.modalId);

                        // ⭐ تنظیم z-index برای modal تو در تو
                        $modal.css('z-index', parentZIndex + 10);
                        
                        // ⭐ اضافه کردن backdrop شفاف اگر نیاز باشد
                        if (config.backdrop === 'dim') {
                            $('<div class="modal-backdrop fade show nested-backdrop"></div>')
                                .css({
                                    'z-index': parentZIndex + 9,
                                    'background-color': 'rgba(0, 0, 0, 0.3)'
                                })
                                .attr('data-modal-id', config.modalId)
                                .appendTo('body');
                        }

                        if (typeof config.onShown === 'function') {
                            config.onShown(modalInstance, $modal);
                        }

                        resolve({ modal: modalInstance, element: $modal[0] });
                    });

                    // ⭐ Event: hidden
                    $modal.on('hidden.bs.modal', function () {
                        console.log('🔄 Nested modal hidden:', config.modalId);

                        // ⭐ حذف backdrop شفاف
                        $(`.nested-backdrop[data-modal-id="${config.modalId}"]`).remove();

                        if (typeof config.onHidden === 'function') {
                            config.onHidden(modalInstance, $modal);
                        }

                        if (config.removeOnHide) {
                            $modal.remove();
                        }
                    });

                } catch (processError) {
                    console.error('❌ Error processing nested modal:', processError);
                    $modal.remove();

                    if (typeof config.onLoadError === 'function') {
                        config.onLoadError(processError);
                    } else if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            title: 'خطا در بارگذاری محتوا',
                            text: processError.message,
                            icon: 'error',
                            confirmButtonText: 'باشه'
                        });
                    } else {
                        alert('خطا: ' + processError.message);
                    }

                    reject(processError);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error('❌ AJAX Error (nested modal):', {
                    status: jqXHR.status,
                    statusText: jqXHR.statusText,
                    error: errorThrown
                });

                $modal.remove();

                const errorMessage = (typeof getAjaxErrorMessage === 'function') 
                    ? getAjaxErrorMessage(jqXHR)
                    : `خطا در بارگذاری: ${jqXHR.status} - ${jqXHR.statusText}`;

                if (typeof config.onLoadError === 'function') {
                    config.onLoadError(new Error(errorMessage));
                } else if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        title: 'خطا در بارگذاری',
                        text: errorMessage,
                        icon: 'error',
                        confirmButtonText: 'باشه',
                        footer: `کد خطا: ${jqXHR.status || 'نامشخص'}`
                    });
                } else {
                    alert(errorMessage);
                }

                reject(new Error(errorMessage));
            }
        });
    });
}

// ⭐ Expose به window برای استفاده global
window.createAndShowNestedModal = createAndShowNestedModal;