// ========================================
// ⭐⭐⭐ Task List Manager - نسخه جدید
// ========================================

const TaskListManager = {
    config: null,
    currentViewType: 0,
    currentGrouping: 0,

    init: function(config) {
        this.config = config || window.TaskListConfig;

        if (!this.config) {
            console.error('❌ TaskListConfig not found!');
            return;
        }

        this.currentViewType = this.config.currentViewType;
        this.currentGrouping = this.config.currentGrouping;

        console.log('✅ TaskListManager initialized');
        console.log('View Type:', this.currentViewType, 'Grouping:', this.currentGrouping);
    },
    changeGrouping: async function (grouping) {
        if (this.currentGrouping === grouping) {
            console.log('ℹ️ Same grouping, skipping');
            return;
        }

        console.log('🔄 Changing grouping to:', grouping);

        // ⭐ ابتدا دکمه‌ها رو اکتیو/غیرفعال کن
        $('.btn-grouping').each(function () {
            const btnGrouping = parseInt($(this).attr('onclick').match(/changeGrouping\((\d+)\)/)[1]);
            if (btnGrouping === grouping) {
                $(this).removeClass('btn-alt-secondary').addClass('btn-primary');
            } else {
                $(this).removeClass('btn-primary').addClass('btn-alt-secondary');
            }
        });

        const $container = $('#task-groups-container');
        $container.html('<div class="text-center py-5"><i class="fa fa-spinner fa-spin fa-3x"></i></div>');

        const token = getAntiForgeryToken();

        try {
            const response = await fetch(this.config.urls.changeGrouping, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({
                    viewType: this.currentViewType,
                    grouping: grouping,
                    __RequestVerificationToken: token
                })
            });

            const result = await response.json();

            if (result.status === 'update-view' && result.viewList) {
                updateMultipleViews(result.viewList);

                this.currentGrouping = grouping;

                console.log('✅ Grouping changed successfully');
            }
        } catch (error) {
            console.error('❌ Error changing grouping:', error);
            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.error('خطا در تغییر گروه‌بندی');
            }

            // در صورت خطا، دکمه قبلی رو دوباره اکتیو کن
            $('.btn-grouping').each(function () {
                const btnGrouping = parseInt($(this).attr('onclick').match(/changeGrouping\((\d+)\)/)[1]);
                if (btnGrouping === this.currentGrouping) {
                    $(this).removeClass('btn-alt-secondary').addClass('btn-primary');
                } else {
                    $(this).removeClass('btn-primary').addClass('btn-alt-secondary');
                }
            }.bind(this));
        }
    },
    changeViewType: function(viewType) {
        if (this.currentViewType === viewType) {
            console.log('ℹ️ Same view type, skipping');
            return;
        }

        console.log('🔄 Changing view type to:', viewType);

        // ⭐ Redirect به صفحه Index با پارامترهای جدید
        window.location.href = `/TaskingArea/Tasks/Index?viewType=${viewType}&grouping=${this.currentGrouping}`;
    }
};

// ========================================
// ⭐⭐⭐ Global Functions
// ========================================

function changeGrouping(grouping) {
    TaskListManager.changeGrouping(grouping);
}

function changeViewType(viewType) {
    TaskListManager.changeViewType(viewType);
}

async function completeTask(taskId) {
    // استفاده از مودال موجود CompleteTask
    const modalUrl = `/TaskingArea/Tasks/CompleteTask?id=${taskId}`;
    
    try {
        const response = await fetch(modalUrl);
        const html = await response.text();
        
        if (typeof showModal === 'function') {
            showModal('تکمیل تسک', html);
        }
    } catch (error) {
        console.error('❌ Error loading complete modal:', error);
    }
}

function editTask(taskId) {
    window.location.href = `/TaskingArea/Tasks/Edit/${taskId}`;
}

async function deleteTask(taskId) {
    if (typeof showDeleteConfirmation !== 'function') {
        if (!confirm('آیا از حذف این تسک اطمینان دارید؟')) return;
    } else {
        const confirmed = await showDeleteConfirmation('این تسک');
        if (!confirmed) return;
    }

    const token = getAntiForgeryToken();

    try {
        const response = await fetch(`/TaskingArea/Tasks/Delete/${taskId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: new URLSearchParams({
                __RequestVerificationToken: token
            })
        });

        const result = await response.json();

        if (result.success) {
            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.success('تسک حذف شد');
            }

            // ⭐ Reload صفحه
            setTimeout(() => {
                window.location.reload();
            }, 1000);
        }
    } catch (error) {
        console.error('❌ Error deleting task:', error);
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.error('خطا در حذف تسک');
        }
    }
}

// ========================================
// ⭐⭐⭐ Document Ready
// ========================================

$(document).ready(function() {
    console.log('🔄 TaskList.js: Document Ready');

    if (typeof window.TaskListConfig === 'undefined') {
        console.error('❌ TaskListConfig not found!');
        return;
    }

    TaskListManager.init(window.TaskListConfig);

    // ⭐ Initialize tooltips
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    console.log('✅ TaskList initialized successfully');
});

$(document).ready(function () {
    // ⭐⭐⭐ مدیریت تب‌های اکتیو
    function setActiveTab(groupBy) {
        // حذف کلاس active از همه تب‌ها
        $('.group-filter-btn').removeClass('active');

        // اضافه کردن کلاس active به تب انتخاب شده
        $(`.group-filter-btn[data-group="${groupBy}"]`).addClass('active');
    }

    // تنظیم تب اکتیو اولیه
    const initialGroupBy = $('#group-by-filter').val() || 'status';
    setActiveTab(initialGroupBy);

    // ⭐ کلیک روی کارت برای نمایش جزئیات
    $(document).on('click', '.task-card', function (e) {
        if (!$(e.target).closest('button').length) {
            const taskId = $(this).data('task-id');
            loadTaskDetail(taskId);
        }
    });

    // ⭐ دکمه مشاهده
    $(document).on('click', '.view-task-btn', function (e) {
        e.stopPropagation();
        const taskId = $(this).data('task-id');
        loadTaskDetail(taskId);
    });

    // ⭐ دکمه تکمیل
    $(document).on('click', '.complete-task-btn', function (e) {
        e.stopPropagation();
        const taskId = $(this).data('task-id');
        completeTask(taskId);
    });

    // ⭐ دکمه بازگشایی
    $(document).on('click', '.reopen-task-btn', function (e) {
        e.stopPropagation();
        const taskId = $(this).data('task-id');
        reopenTask(taskId);
    });

    // ⭐⭐⭐ تغییر گروه‌بندی با تب‌ها
    $('.group-filter-btn').on('click', function () {
        const groupBy = $(this).data('group');
        $('#group-by-filter').val(groupBy);
        setActiveTab(groupBy);
        loadTasks();
    });

    // ⭐ تغییر فیلتر وضعیت
    $('#status-filter').on('change', function () {
        loadTasks();
    });

    // ⭐ تغییر فیلتر اولویت
    $('#priority-filter').on('change', function () {
        loadTasks();
    });

    // ⭐ تغییر فیلتر گروه‌بندی (dropdown)
    $('#group-by-filter').on('change', function () {
        const groupBy = $(this).val();
        setActiveTab(groupBy);
        loadTasks();
    });

    // ⭐ جستجو
    $('#search-input').on('keyup', debounce(function () {
        loadTasks();
    }, 500));

    // ⭐ بارگذاری لیست تسک‌ها
    function loadTasks() {
        const filters = {
            status: $('#status-filter').val(),
            priority: $('#priority-filter').val(),
            groupBy: $('#group-by-filter').val(),
            search: $('#search-input').val()
        };

        $.ajax({
            url: '/TaskingArea/Tasks/GetTaskList',
            type: 'GET',
            data: filters,
            beforeSend: function () {
                $('#task-list-container').html('<div class="text-center py-5"><i class="fa fa-spinner fa-spin fa-3x text-primary"></i></div>');
            },
            success: function (response) {
                $('#task-list-container').html(response);
            },
            error: function () {
                $('#task-list-container').html('<div class="alert alert-danger">خطا در بارگذاری تسک‌ها</div>');
            }
        });
    }

    // ⭐ نمایش جزئیات تسک
    function loadTaskDetail(taskId) {
        $.ajax({
            url: `/TaskingArea/Tasks/GetTaskDetail/${taskId}`,
            type: 'GET',
            success: function (response) {
                $('#task-detail-container').html(response);
                // اسکرول به بالای جزئیات
                $('html, body').animate({
                    scrollTop: $('#task-detail-container').offset().top - 100
                }, 500);
            },
            error: function () {
                toastr.error('خطا در بارگذاری جزئیات تسک');
            }
        });
    }

    // ⭐ تکمیل تسک
    function completeTask(taskId) {
        if (confirm('آیا از تکمیل این تسک اطمینان دارید؟')) {
            $.ajax({
                url: `/TaskingArea/Tasks/CompleteTask/${taskId}`,
                type: 'POST',
                success: function () {
                    toastr.success('تسک با موفقیت تکمیل شد');
                    loadTasks();
                    loadTaskDetail(taskId);
                },
                error: function () {
                    toastr.error('خطا در تکمیل تسک');
                }
            });
        }
    }

    // ⭐ بازگشایی تسک
    function reopenTask(taskId) {
        if (confirm('آیا از بازگشایی این تسک اطمینان دارید؟')) {
            $.ajax({
                url: `/TaskingArea/Tasks/ReopenTask/${taskId}`,
                type: 'POST',
                success: function () {
                    toastr.success('تسک با موفقیت بازگشایی شد');
                    loadTasks();
                    loadTaskDetail(taskId);
                },
                error: function () {
                    toastr.error('خطا در بازگشایی تسک');
                }
            });
        }
    }

    // ⭐ Debounce برای جستجو
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
});

// ========================================
// ⭐⭐⭐ تکمیل تسک از لیست - استفاده از createAndShowModal
// ========================================
// ⭐⭐⭐ تکمیل تسک از لیست - استفاده از createAndShowModal
$(document).on('click', '.complete-task-from-list-btn', function (e) {
    e.preventDefault();
    e.stopPropagation();

    const taskId = $(this).data('task-id');
    const $taskContainer = $(`[data-task-container="${taskId}"]`);

    console.log('🔄 Complete task from list:', taskId);

    // ⭐⭐⭐ استفاده از createAndShowModal
    createAndShowModal({
        url: `/TaskingArea/Tasks/CompleteTask?id=${taskId}&fromList=true`,
        // ⭐⭐⭐ حذف backdrop: 'static' - حالا با کلیک بسته می‌شه
        backdrop: true, // یا اصلاً نذار تا default استفاده بشه
        keyboard: true,
        onSubmitSuccess: function (response, modalInstance) {
            console.log('✅ Task completion submitted:', response);

            if (response.status === 'success-from-list') {
                // ⭐ بستن مودال
                modalInstance.hide();

                // ⭐ نمایش پیام موفقیت
                const successMsg = response.message?.[0]?.text || 'تسک با موفقیت تکمیل شد';
                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.success(successMsg);
                } else {
                    alert(successMsg);
                }

                // ⭐ انیمیشن حذف از pending
                $taskContainer.find('.block').addClass('task-removing');

                setTimeout(function () {
                    // حذف از DOM
                    $taskContainer.remove();

                    // ⭐ پیدا کردن بخش completed
                    const $completedSection = $('h4:contains("تکمیل شده")')
                        .closest('.mb-4, .mb-3, div')
                        .find('.row.g-3')
                        .first();

                    if ($completedSection.length && response.taskCard) {
                        console.log('✅ Adding card to completed section');

                        const $newCard = $(response.taskCard);
                        $newCard.find('.block').addClass('task-adding');

                        $completedSection.append($newCard);

                        // ⭐ بروزرسانی badge ها
                        updateTaskCounts();

                        // ⭐ Initialize tooltips برای کارت جدید
                        if (typeof bootstrap !== 'undefined') {
                            $newCard.find('[data-bs-toggle="tooltip"]').each(function () {
                                new bootstrap.Tooltip(this);
                            });
                        }

                        // ⭐ اسکرول به کارت جدید
                        setTimeout(function () {
                            $('html, body').animate({
                                scrollTop: $newCard.offset().top - 100
                            }, 500);
                        }, 100);
                    } else {
                        console.log('⚠️ Completed section not found, reloading...');
                        location.reload();
                    }

                }, 500);
            }
        },
        onLoadError: function (error) {
            console.error('❌ Error loading modal:', error);
            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.error('خطا در بارگذاری فرم تکمیل');
            } else {
                alert('خطا در بارگذاری فرم تکمیل');
            }ح
        }
    }).catch(error => {
        console.error('❌ Modal creation failed:', error);
    });
});
// ⭐⭐⭐ Handler Functions - اصلاح شده

function handleSuccessFromList(result, $taskContainer, modalInstance) {
    console.log('🎉 Task completed successfully from list');

    // ⭐ بستن مودال
    if (modalInstance) {
        modalInstance.hide();
    } else {
        $('#modal-dialog').modal('hide');
    }

    const successMsg = result.message && result.message.length > 0
        ? result.message[0].text
        : 'تسک با موفقیت تکمیل شد';

    if (typeof NotificationHelper !== 'undefined') {
        NotificationHelper.success(successMsg);
    } else {
        alert(successMsg);
    }

    $taskContainer.find('.block').addClass('task-removing');

    setTimeout(function () {
        $taskContainer.remove();

        const $completedSection = $('h4:contains("تکمیل شده")')
            .closest('.mb-4, .mb-3, div')
            .find('.row.g-3')
            .first();

        if ($completedSection.length && result.taskCard) {
            console.log('✅ Adding card to completed section');

            const $newCard = $(result.taskCard);
            $newCard.find('.block').addClass('task-adding');

            $completedSection.append($newCard);

            updateTaskCounts();

            if (typeof bootstrap !== 'undefined') {
                $newCard.find('[data-bs-toggle="tooltip"]').each(function () {
                    new bootstrap.Tooltip(this);
                });
            }

            setTimeout(function () {
                $('html, body').animate({
                    scrollTop: $newCard.offset().top - 100
                }, 500);
            }, 100);
        } else {
            console.log('⚠️ Completed section not found, reloading...');
            location.reload();
        }

    }, 500);
}

function handleValidationError(result, $submitBtn, originalBtnText) {
    console.log('⚠️ Validation errors:', result.message);

    $submitBtn.prop('disabled', false).html(originalBtnText);

    if (result.message && Array.isArray(result.message)) {
        result.message.forEach(function (msg) {
            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.error(msg.text);
            } else {
                alert(msg.text);
            }
        });
    }
}

function handleError(result, $submitBtn, originalBtnText) {
    console.log('❌ Error:', result.message);

    $submitBtn.prop('disabled', false).html(originalBtnText);

    const errorMsg = result.message && result.message.length > 0
        ? result.message[0].text
        : 'خطا در تکمیل تسک';

    if (typeof NotificationHelper !== 'undefined') {
        NotificationHelper.error(errorMsg);
    } else {
        alert(errorMsg);
    }
}

function handleRedirect(result, modalInstance) {
    console.log('🔄 Redirecting...');

    if (modalInstance) {
        modalInstance.hide();
    } else {
        $('#modal-dialog').modal('hide');
    }

    if (typeof NotificationHelper !== 'undefined') {
        NotificationHelper.success('تسک با موفقیت تکمیل شد');
    }

    setTimeout(function () {
        if (result.redirectUrl) {
            window.location.href = result.redirectUrl;
        } else {
            location.reload();
        }
    }, 500);
}

function updateTaskCounts() {
    console.log('🔢 Updating badge counts');

    const $pendingSection = $('h4:contains("در حال انجام")').closest('.d-flex');
    const $pendingBadge = $pendingSection.find('.badge.bg-primary, .badge.badge-primary');

    const $completedSection = $('h4:contains("تکمیل شده")').closest('.d-flex');
    const $completedBadge = $completedSection.find('.badge.bg-success, .badge.badge-success');

    if ($pendingBadge.length) {
        const currentCount = parseInt($pendingBadge.text()) || 0;
        if (currentCount > 0) {
            const newCount = currentCount - 1;
            $pendingBadge.text(newCount);
            console.log('📉 Pending count:', currentCount, '→', newCount);
        }
    }

    if ($completedBadge.length) {
        const currentCount = parseInt($completedBadge.text()) || 0;
        const newCount = currentCount + 1;
        $completedBadge.text(newCount);
        console.log('📈 Completed count:', currentCount, '→', newCount);
    }
}