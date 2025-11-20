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