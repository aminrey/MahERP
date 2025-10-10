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
// ⭐ تنظیم فوکوس - اصلاح شده
// ========================================
function setTaskFocus(taskId) {
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
                url: '/AdminArea/Tasks/SetTaskFocus', // ⭐ URL استاتیک
                type: 'POST',
                data: {
                    taskId: taskId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        // نمایش پیام موفقیت
                        if (typeof SendResposeMessage === 'function') {
                            SendResposeMessage(response.message);
                        } else if (typeof NotificationHelper !== 'undefined') {
                            NotificationHelper.success(response.message || 'تسک با موفقیت به عنوان فوکوس تنظیم شد');
                        } else {
                            Swal.fire({
                                title: 'موفق!',
                                text: response.message || 'تسک با موفقیت به عنوان فوکوس تنظیم شد',
                                icon: 'success',
                                confirmButtonText: 'باشه',
                                timer: 2000
                            });
                        }
                        setTimeout(() => location.reload(), 1000);
                    } else {
                        // نمایش پیام خطا
                        if (typeof SendResposeMessage === 'function') {
                            SendResposeMessage(response.message);
                        } else if (typeof NotificationHelper !== 'undefined') {
                            NotificationHelper.error(response.message || 'خطا در تنظیم فوکوس');
                        } else {
                            Swal.fire({
                                title: 'خطا',
                                text: response.message || 'خطا در تنظیم فوکوس',
                                icon: 'error',
                                confirmButtonText: 'باشه'
                            });
                        }
                    }
                },
                error: function (xhr) {
                    console.error('Error in setTaskFocus:', xhr);

                    if (typeof handleAjaxError === 'function') {
                        handleAjaxError(xhr);
                    } else if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error('خطا در ارتباط با سرور');
                    } else {
                        Swal.fire({
                            title: 'خطا',
                            text: 'خطا در ارتباط با سرور',
                            icon: 'error',
                            confirmButtonText: 'باشه'
                        });
                    }
                }
            });
        }
    });
}

// ========================================
// ⭐ حذف فوکوس - اصلاح شده
// ========================================
function removeTaskFocus(taskId) {
    $.ajax({
        url: '/AdminArea/Tasks/RemoveTaskFocus', // ⭐ URL استاتیک
        type: 'POST',
        data: {
            taskId: taskId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                // نمایش پیام موفقیت
                if (typeof SendResposeMessage === 'function') {
                    SendResposeMessage(response.message);
                } else if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.success(response.message || 'فوکوس با موفقیت حذف شد');
                } else {
                    Swal.fire({
                        title: 'موفق!',
                        text: response.message || 'فوکوس با موفقیت حذف شد',
                        icon: 'success',
                        confirmButtonText: 'باشه',
                        timer: 2000
                    });
                }
                setTimeout(() => location.reload(), 1000);
            } else {
                // نمایش پیام خطا
                if (typeof SendResposeMessage === 'function') {
                    SendResposeMessage(response.message);
                } else if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error(response.message || 'خطا در حذف فوکوس');
                } else {
                    Swal.fire({
                        title: 'خطا',
                        text: response.message || 'خطا در حذف فوکوس',
                        icon: 'error',
                        confirmButtonText: 'باشه'
                    });
                }
            }
        },
        error: function (xhr) {
            console.error('Error in removeTaskFocus:', xhr);

            if (typeof handleAjaxError === 'function') {
                handleAjaxError(xhr);
            } else if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.error('خطا در ارتباط با سرور');
            } else {
                Swal.fire({
                    title: 'خطا',
                    text: 'خطا در ارتباط با سرور',
                    icon: 'error',
                    confirmButtonText: 'باشه'
                });
            }
        }
    });
}

// ⭐ Expose به window برای استفاده global
window.setTaskFocus = setTaskFocus;
window.removeTaskFocus = removeTaskFocus;