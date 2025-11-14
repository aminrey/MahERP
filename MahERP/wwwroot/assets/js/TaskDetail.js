
// ⭐⭐⭐ دریافت توکن با روش بهتر



// ========================================
// ⭐⭐⭐ Task Details Auto-Refresh Manager
// ========================================
const TaskDetailsRefreshManager = {
    taskId: null,
    isEnabled: false,
    config: null,

    init: function (taskId, config = null) {
        this.taskId = taskId;
        // ⭐⭐⭐ اصلاح: اطمینان از وجود config
        this.config = config || window.TaskDetailConfig;

        if (!this.config) {
            console.error('❌ TaskDetailConfig not found!');
            return;
        }

        this.isEnabled = true;
        console.log('✅ Task Details Refresh Manager initialized for task:', taskId);
    },

    refreshAll: function () {
        if (!this.isEnabled || !this.taskId || !this.config) {
            console.warn('⚠️ RefreshManager not properly initialized');
            return;
        }

        console.log('🔄 Refreshing all task details sections...');

        this.refreshHeroStats();
        this.refreshProgressBar();
        this.refreshSidebarStats();
        this.refreshOperationsCount();

        if ($('#operations-tab').hasClass('active') && typeof DynamicOperationsManager !== 'undefined') {
            DynamicOperationsManager.updateStats();
        }
    },

    refreshHeroStats: function () {
        const self = this;

        // ⭐⭐⭐ بررسی وجود config
        if (!self.config || !self.config.urls) {
            console.error('❌ Config or URLs not available');
            return;
        }

        $.ajax({
            url: self.config.urls.getTaskHeroStats,
            type: 'GET',
            data: { taskId: self.taskId },
            success: function (data) {
                if (data.success) {
                    $('.stat-value').filter(function () {
                        return $(this).closest('.stat-card').find('.stat-label').text().includes('پیشرفت');
                    }).html(data.progressPercentage + '<span class="stat-unit">%</span>');

                    $('.stat-value').filter(function () {
                        return $(this).closest('.stat-card').find('.stat-label').text().includes('عملیات');
                    }).text(`${data.completedOperations}/${data.totalOperations}`);

                    console.log('✅ Hero stats refreshed');
                }
            },
            error: function (xhr) {
                console.error('❌ Error refreshing hero stats:', xhr);
            }
        });
    },

    refreshProgressBar: function () {
        const self = this;

        if (!self.config || !self.config.urls) {
            console.error('❌ Config or URLs not available');
            return;
        }

        $.ajax({
            url: self.config.urls.getTaskProgress,
            type: 'GET',
            data: { taskId: self.taskId },
            success: function (data) {
                if (data.success) {
                    const $progressBar = $('.progress-bar');
                    $progressBar.css('width', data.percentage + '%');
                    $progressBar.attr('aria-valuenow', data.percentage);
                    $progressBar.text(data.percentage + '%');

                    if (data.percentage === 100) {
                        $progressBar.removeClass('bg-primary').addClass('bg-success');
                    }

                    console.log('✅ Progress bar refreshed:', data.percentage + '%');
                }
            },
            error: function (xhr) {
                console.error('❌ Error refreshing progress bar:', xhr);
            }
        });
    },

    refreshSidebarStats: function () {
        const self = this;

        if (!self.config || !self.config.urls) {
            console.error('❌ Config or URLs not available');
            return;
        }

        $.ajax({
            url: self.config.urls.getTaskSidebarStats,
            type: 'GET',
            data: { taskId: self.taskId },
            success: function (data) {
                if (data.success) {
                    $('#sidebar-completed-ops').text(data.completedOps);
                    $('#sidebar-pending-ops').text(data.pendingOps);
                    $('#sidebar-team-members').text(data.teamMembers);
                    $('#sidebar-progress').text(data.progress + '%');

                    console.log('✅ Sidebar stats refreshed');
                }
            },
            error: function (xhr) {
                console.error('❌ Error refreshing sidebar stats:', xhr);
            }
        });
    },

    refreshOperationsCount: function () {
        const totalOps = $('#totalOpsCount').text();
        $('#operations-tab .badge').text(totalOps);
    },

    triggerRefresh: function () {
        setTimeout(() => {
            this.refreshAll();
        }, 500);
    }
};



// ⭐ تابع نمایش مودال View WorkLogs
async function showViewWorkLogsModal(operationId) {
    if (!operationId) return;

    console.log('📋 Opening View WorkLogs modal for operation:', operationId);

    try {
        const response = await fetch(`/TaskingArea/TaskOperations/ViewWorkLogsModal?operationId=${operationId}`);
        const html = await response.text();

        // فرض می‌کنیم تابع showModal وجود داره
        if (typeof showModal === 'function') {
            showModal('مشاهده گزارش کارها', html);
        } else {
            // Fallback: استفاده از Bootstrap Modal
            const modalHtml = `
                <div class="modal fade" id="workLogsModal" tabindex="-1">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content">
                            ${html}
                        </div>
                    </div>
                </div>
            `;
            $('body').append(modalHtml);
            const modal = new bootstrap.Modal(document.getElementById('workLogsModal'));
            modal.show();
            $('#workLogsModal').on('hidden.bs.modal', function () {
                $(this).remove();
            });
        }
    } catch (error) {
        console.error('❌ Error loading modal:', error);
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.error('خطا در بارگذاری مودال');
        }
    }
}
/**
 * ثبت WorkLog جدید
 */
async function submitWorkLog(formElement) {
    if (!formElement) return;

    const formData = new FormData(formElement);
    formData.append('__RequestVerificationToken', getAntiForgeryToken());

    try {
        const response = await fetch('/TaskingArea/TaskOperations/AddWorkLog', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.status === 'update-view' && result.viewList) {
            await updateMultipleViews(result.viewList);

            // ⭐ بستن مودال
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalContainer'));
            modal?.hide();

            showToastReal('گزارش کار ثبت شد', 'success');
        } else if (result.status === 'validation-error') {
            showValidationErrors(result.message);
        } else {
            showToastReal(result.message?.[0]?.text || 'خطا در ثبت', 'error');
        }
    } catch (error) {
        console.error('❌ Error submitting worklog:', error);
        showToastReal('خطا در ارتباط با سرور', 'error');
    }
}

// ==========================================
// 🔹 Helper Functions
// ==========================================




/**
 * نمایش خطاهای Validation
 */
function showValidationErrors(errors) {
    if (!Array.isArray(errors)) return;

    errors.forEach(err => {
        showToast(err.text, err.status || 'error');
    });
}


// ========================================
// ⭐⭐⭐ Chat Manager - نسخه اصلاح شده
// ========================================
const TaskChatManager = {
    taskId: null,
    config: null,
    selectedFiles: [],
    isInitialized: false, // ⭐⭐⭐ فلگ جلوگیری از Initialize مکرر
    refreshInterval: null, // ⭐⭐⭐ ذخیره interval برای clear کردن

    init: function (config) {
        // ⭐⭐⭐ جلوگیری از Initialize چندباره
        if (this.isInitialized) {
            console.log('ℹ️ Chat Manager already initialized');
            return;
        }

        this.config = config || window.TaskDetailConfig;

        if (!this.config) {
            console.error('❌ TaskDetailConfig not found for ChatManager!');
            return;
        }

        this.taskId = this.config.taskId;
        this.scrollToBottom();
        this.autoRefresh();
        this.isInitialized = true; // ⭐⭐⭐ علامت‌گذاری به عنوان Initialize شده

        console.log('✅ Chat Manager initialized for task:', this.taskId);
    },

    scrollToBottom: function () {
        const container = $('#chat-messages-container');
        if (container.length) {
            setTimeout(() => {
                container.scrollTop(container[0].scrollHeight);
            }, 100);
        }
    },

    autoRefresh: function () {
        const self = this;

        // ⭐⭐⭐ پاک کردن interval قبلی
        if (self.refreshInterval) {
            clearInterval(self.refreshInterval);
            console.log('🔄 Cleared previous refresh interval');
        }

        // ⭐⭐⭐ ایجاد interval جدید
        self.refreshInterval = setInterval(() => {
            self.loadNewMessages();
        }, 30000); // هر 30 ثانیه

        console.log('✅ Chat auto-refresh enabled');
    },

    loadNewMessages: function () {
        const self = this;

        // ⭐⭐⭐ جلوگیری از load اگر در تب Chat نیستیم
        if (!$('#tab-chat').hasClass('active')) {
            console.log('ℹ️ Chat tab not active, skipping refresh');
            return;
        }

        console.log('🔄 Loading new messages...');

        $.ajax({
            url: self.config.urls.getTaskComments,
            type: 'GET',
            data: { taskId: self.taskId },
            success: function (response) {
                if (response.success && response.html) {
                    // ⭐⭐⭐ بررسی تغییرات قبل از replace
                    const currentHtml = $('#chat-messages-container').html();
                    if (currentHtml !== response.html) {
                        $('#chat-messages-container').html(response.html);
                        self.scrollToBottom();
                        self.updateCommentCount();
                        console.log('✅ Messages updated');
                    } else {
                        console.log('ℹ️ No new messages');
                    }
                }
            },
            error: function (xhr) {
                console.error('❌ Error loading messages:', xhr);
            }
        });
    },
    sendMessage: function (messageText, isImportant) {
        const self = this;
        const formData = new FormData();

        formData.append('TaskId', self.taskId);
        formData.append('CommentText', messageText || '');
        formData.append('IsImportant', isImportant);
        formData.append('__RequestVerificationToken', $('[name="__RequestVerificationToken"]').val());

        // ⭐⭐⭐ افزودن فایل‌ها
        if (self.selectedFiles && self.selectedFiles.length > 0) {
            console.log(`📤 Adding ${self.selectedFiles.length} files to FormData`);
            self.selectedFiles.forEach((file, index) => {
                formData.append('Attachments', file);
                console.log(`  ✅ File ${index}: ${file.name} (${file.size} bytes)`);
            });
        } else {
            console.log('ℹ️ No files to upload');
        }

        // نمایش لودینگ
        const $sendBtn = $('#chat-form button[type="submit"]');
        const originalHtml = $sendBtn.html();
        $sendBtn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin me-1"></i>در حال ارسال...');

        $.ajax({
            url: self.config.urls.addTaskComment,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                console.log('✅ Response:', response);

                if (response.success) {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.success('پیام با موفقیت ارسال شد');
                    }

                    // پاک کردن فرم
                    $('#chat-message-input').val('');
                    $('#chat-important-check').prop('checked', false);
                    $('#chat-file-input').val('');
                    self.selectedFiles = [];
                    $('#chat-selected-files').hide().empty();

                    // بارگذاری پیام‌های جدید
                    setTimeout(() => {
                        self.loadNewMessages();
                    }, 500);
                } else {
                    console.error('❌ Server error:', response.message);
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error(response.message || 'خطا در ارسال پیام');
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('❌ AJAX Error:', { xhr, status, error });
                console.error('Response Text:', xhr.responseText);

                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error('خطا در ارسال پیام به سرور');
                }
            },
            complete: function () {
                $sendBtn.prop('disabled', false).html(originalHtml);
            }
        });
    },
    updateCommentCount: function () {
        const currentCount = $('.chat-message').length;
        const $badge = $('#chat-badge-count');

        if (currentCount > 0) {
            $badge.text(currentCount).show();
        } else {
            $badge.hide();
        }

        console.log(`✅ Comment badge updated: ${currentCount}`);
    },

    // ⭐⭐⭐ متد جدید: پاکسازی منابع
    destroy: function () {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
        this.isInitialized = false;
        console.log('🧹 Chat Manager destroyed');
    }
};

// ========================================
// ⭐⭐⭐ Global Functions
// ========================================


function sendChatMessage(event) {
    event.preventDefault();

    const message = $('#chat-message-input').val().trim();
    const isImportant = $('#chat-important-check').is(':checked');

    // بررسی: حداقل پیام یا فایل
    if (!message && TaskChatManager.selectedFiles.length === 0) {
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.warning('لطفاً متن پیام یا فایل را انتخاب کنید');
        }
        return false;
    }

    // ⭐⭐⭐ Debug: نمایش فایل‌های انتخاب شده
    console.log(`📎 Files to send: ${TaskChatManager.selectedFiles.length}`);
    TaskChatManager.selectedFiles.forEach((file, i) => {
        console.log(`  File ${i}: ${file.name} (${file.size} bytes)`);
    });

    TaskChatManager.sendMessage(message, isImportant);

    return false;
}
function handleChatKeyPress(event) {
    if (event.ctrlKey && event.keyCode === 13) {
        sendChatMessage(event);
    }
}
// ⭐ تابع مدیریت انتخاب فایل در چت - نسخه کامل شده
function handleChatFileSelect(input) {
    const container = $('#chat-selected-files');
    const allowedExtensions = [
        // تصاویر
        'jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp',
        // اسناد
        'txt', 'pdf', 'doc', 'docx', 'rtf',
        // صفحات گسترده
        'xls', 'xlsx', 'csv',
        // ارائه‌ها
        'ppt', 'pptx',
        // فشرده‌سازی
        'zip', 'rar', '7z',
        // سایر
        'odt', 'ods', 'odp'
    ];

    const maxFileSize = 10 * 1024 * 1024; // 10MB

    container.empty();

    // ⭐⭐⭐ پاک کردن لیست فایل‌های قبلی
    TaskChatManager.selectedFiles = [];

    if (input.files && input.files.length > 0) {
        let hasValidFiles = false;

        Array.from(input.files).forEach((file, index) => {
            const extension = file.name.split('.').pop().toLowerCase();

            // بررسی پسوند فایل
            if (!allowedExtensions.includes(extension)) {
                Swal.fire({
                    icon: 'error',
                    title: 'فرمت نامعتبر',
                    text: `فایل "${file.name}" فرمت مجاز نیست.`,
                    confirmButtonText: 'باشه'
                });
                return;
            }

            // بررسی حجم فایل
            if (file.size > maxFileSize) {
                Swal.fire({
                    icon: 'error',
                    title: 'حجم زیاد',
                    text: `فایل "${file.name}" بیش از 10MB حجم دارد.`,
                    confirmButtonText: 'باشه'
                });
                return;
            }

            hasValidFiles = true;

            // ⭐⭐⭐ افزودن فایل معتبر به لیست
            TaskChatManager.selectedFiles.push(file);

            // آیکون بر اساس نوع فایل
            let iconClass = 'fa-file';
            let badgeClass = 'bg-secondary';

            if (['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp'].includes(extension)) {
                iconClass = 'fa-file-image';
                badgeClass = 'bg-success';
            } else if (['pdf'].includes(extension)) {
                iconClass = 'fa-file-pdf';
                badgeClass = 'bg-danger';
            } else if (['doc', 'docx'].includes(extension)) {
                iconClass = 'fa-file-word';
                badgeClass = 'bg-primary';
            } else if (['xls', 'xlsx', 'csv'].includes(extension)) {
                iconClass = 'fa-file-excel';
                badgeClass = 'bg-success';
            } else if (['ppt', 'pptx'].includes(extension)) {
                iconClass = 'fa-file-powerpoint';
                badgeClass = 'bg-warning';
            } else if (['txt'].includes(extension)) {
                iconClass = 'fa-file-alt';
                badgeClass = 'bg-info';
            } else if (['zip', 'rar', '7z'].includes(extension)) {
                iconClass = 'fa-file-archive';
                badgeClass = 'bg-dark';
            }

            const fileSize = (file.size / 1024).toFixed(2);
            const badge = $(`
                <span class="badge ${badgeClass} me-1 mb-1" data-file-index="${TaskChatManager.selectedFiles.length - 1}">
                    <i class="fa ${iconClass} me-1"></i>
                    ${file.name}
                    <span class="ms-1">(${fileSize} KB)</span>
                    <button type="button" class="btn-close btn-close-white ms-2" 
                            onclick="removeChatFile(${TaskChatManager.selectedFiles.length - 1})" 
                            style="font-size: 0.6rem; vertical-align: middle;"></button>
                </span>
            `);

            container.append(badge);
        });

        if (hasValidFiles) {
            container.show();
            console.log(`✅ ${TaskChatManager.selectedFiles.length} فایل انتخاب شد`);
        } else {
            container.hide();
            input.value = '';
            TaskChatManager.selectedFiles = [];
        }
    } else {
        container.hide();
    }
}

// ⭐ تابع حذف فایل از لیست
function removeChatFile(fileIndex) {
    console.log(`🗑️ Removing file at index: ${fileIndex}`);

    // حذف از آرایه
    TaskChatManager.selectedFiles.splice(fileIndex, 1);

    // حذف badge مربوطه
    $(`#chat-selected-files .badge[data-file-index="${fileIndex}"]`).remove();

    // بروزرسانی index‌های باقی‌مانده
    $('#chat-selected-files .badge').each(function (newIndex) {
        $(this).attr('data-file-index', newIndex);
        $(this).find('.btn-close').attr('onclick', `removeChatFile(${newIndex})`);
    });

    // پاک کردن input اگر هیچ فایلی نماند
    if (TaskChatManager.selectedFiles.length === 0) {
        $('#chat-file-input').val('');
        $('#chat-selected-files').hide();
    }

    console.log(`✅ Files remaining: ${TaskChatManager.selectedFiles.length}`);
}
function deleteComment(commentId) {
    const config = window.TaskDetailConfig;
    const token = getAntiForgeryToken();

    if (typeof showDeleteConfirmation === 'function') {
        showDeleteConfirmation('این پیام').then(confirmed => {
            if (confirmed) {
                $.ajax({
                    url: config.urls.deleteTaskComment,
                    type: 'POST',
                    data: {
                        id: commentId,
                        __RequestVerificationToken: token
                    },
                    success: function (response) {
                        if (response.success) {
                            if (typeof NotificationHelper !== 'undefined') {
                                NotificationHelper.success('پیام حذف شد');
                            }
                            TaskChatManager.loadNewMessages();

                            // ⭐⭐⭐ بروزرسانی Badge
                            TaskChatManager.updateCommentCount();
                        } else {
                            if (typeof NotificationHelper !== 'undefined') {
                                NotificationHelper.error(response.message);
                            }
                        }
                    }
                });
            }
        });
    } else {
        // Fallback confirm
        if (confirm('آیا از حذف این پیام اطمینان دارید؟')) {
            $.ajax({
                url: config.urls.deleteTaskComment,
                type: 'POST',
                data: {
                    id: commentId,
                    __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        alert('پیام حذف شد');
                        TaskChatManager.loadNewMessages();

                        // ⭐⭐⭐ بروزرسانی Badge
                        TaskChatManager.updateCommentCount();
                    }
                }
            });
        }
    }
}
function handleChatKeyPress(event) {
    if (event.ctrlKey && event.keyCode === 13) {
        sendChatMessage(event);
    }
}
function editComment(commentId) {
    if (typeof NotificationHelper !== 'undefined') {
        NotificationHelper.info('قابلیت ویرایش به‌زودی اضافه می‌شود');
    }
}

function loadTaskReminders(config) {
    const container = $('#reminders-list-container');

    container.html(`
        <div class="text-center py-4">
            <i class="fa fa-spinner fa-spin fa-2x text-muted"></i>
            <p class="text-muted mt-2">در حال بارگذاری...</p>
        </div>
    `);

    container.load(config.urls.getTaskReminders, function () {
        if (config.isTaskCompleted) {
            console.log('🔒 Disabling reminder actions after load');
            disableReminders();
        }
    });
}

function loadTaskHistory() {
    const config = window.TaskDetailConfig;

    $.ajax({
        url: config.urls.getTaskHistory,
        type: 'GET',
        data: { taskId: config.taskId },
        success: function (result) {
            $('#task-timeline-container').html(result);
        },
        error: function () {
            if (typeof Dashmix !== 'undefined' && Dashmix.helpers) {
                Dashmix.helpers('jq-notify', {
                    type: 'danger',
                    icon: 'fa fa-times me-1',
                    message: 'خطا در بارگذاری تاریخچه'
                });
            }
        }
    });
}

function disableTaskEditing() {
    console.log('🔒 Task is locked - disabling edit features');



    $('.progress-bar').removeClass('bg-primary').addClass('bg-success');
    $('.progress-bar').css('width', '100%').attr('aria-valuenow', 100).text('100%');

    disableReminders();
}

function disableReminders() {
    setTimeout(function () {
        $('.delete-reminder-btn, [data-reminder-delete]')
            .prop('disabled', true)
            .addClass('disabled')
            .css('cursor', 'not-allowed')
            .attr('onclick', 'return false;')
            .hide();

        $('.edit-reminder-btn, [data-reminder-edit], .toggle-reminder-btn, [data-reminder-toggle]')
            .prop('disabled', true)
            .addClass('disabled')
            .css('cursor', 'not-allowed')
            .attr('onclick', 'return false;');

        $('.reminder-item').each(function () {
            if (!$(this).find('.lock-badge').length) {
                $(this).css('opacity', '0.7')
                    .prepend('<span class="badge bg-secondary lock-badge position-absolute top-0 end-0 m-2"><i class="fa fa-lock"></i></span>');
            }
        });

        console.log('✅ Reminders disabled successfully');
    }, 500);
}

// ========================================
// ⭐⭐⭐ Document Ready
// ========================================
$(document).ready(function () {
    console.log('🔄 TaskDetail.js: Document Ready');

    // ⭐⭐⭐ بررسی وجود Config
    if (typeof window.TaskDetailConfig === 'undefined') {
        console.error('❌ TaskDetailConfig not found! Make sure it is defined in the page.');
        console.error('Checking for config in 500ms...');

        // ⭐ تلاش مجدد بعد از 500ms (برای اطمینان از load شدن)
        setTimeout(function () {
            if (typeof window.TaskDetailConfig !== 'undefined') {
                console.log('✅ Config found on retry');
                initializeTaskDetails();
            } else {
                console.error('❌ Config still not found after retry');
            }
        }, 500);

        return;
    }

    initializeTaskDetails();
});
function initializeTaskDetails() {
    const config = window.TaskDetailConfig;

    console.log('✅ Initializing Task Details with config:', config);

    // Initialize Managers
    TaskDetailsRefreshManager.init(config.taskId, config);
    DynamicOperationsManager.init(config);

    // ⭐⭐⭐ فراخوانی Event Handlers
    initializeOperationHandlers();
 
    // Operations input handlers
    $('#newOperationTitleInput').on('keypress', function (e) {
        // ⭐⭐⭐ برای textarea: Ctrl + Enter
        if (e.ctrlKey && e.which === 13) {
            e.preventDefault();
            DynamicOperationsManager.addOperation();
            return false;
        }

        // ⭐⭐⭐ برای input معمولی: فقط Enter
        if (!e.ctrlKey && e.which === 13 && $(this).is('input')) {
            e.preventDefault();
            DynamicOperationsManager.addOperation();
            return false;
        }
    });

    $('#addNewOperationBtn').on('click', function () {
        DynamicOperationsManager.addOperation();
    });

    // Update stats on load
    if (typeof DynamicOperationsManager !== 'undefined' && DynamicOperationsManager.config) {
        DynamicOperationsManager.updateStats();
    }

    // ⭐⭐⭐ اصلاح: Initialize فقط یک بار با استفاده از .one()
    $('#chat-tab').one('shown.bs.tab', function () {
        console.log('🗨️ Chat tab shown - initializing Chat Manager');
        if (window.TaskDetailConfig && !TaskChatManager.isInitialized) {
            TaskChatManager.init(config);
        }
    });

    // Reminders tab click handler
    $('#reminders-tab').on('click', function () {
        if (window.TaskDetailConfig) {
            loadTaskReminders(config);
        }
    });

    // Disable editing if task completed
    if (config.isTaskCompleted) {
        disableTaskEditing();
    }

    console.log('✅ TaskDetail.js initialized successfully');
}

// ========================================
// ⭐⭐⭐ Task Operations Management
// ========================================

/**
 * تغییر وضعیت ستاره عملیات
 */
function toggleOperationStar(operationId) {
    if (!operationId) {
        console.error('❌ Operation ID is required');
        return;
    }

    // ⭐⭐⭐ پیدا کردن دکمه و جلوگیری از کلیک مجدد
    const $button = $(`.btn-toggle-star[data-operation-id="${operationId}"]`);

    if ($button.prop('disabled')) {
        console.log('⚠️ Button already processing...');
        return; // جلوگیری از کلیک مجدد
    }

    // ⭐⭐⭐ ذخیره آیکون اصلی و نمایش اسپینر
    const $icon = $button.find('i');
    const originalIconClass = $icon.attr('class');

    $button.prop('disabled', true).addClass('disabled');
    $icon.attr('class', 'fa fa-spinner fa-spin');

    const token = getAntiForgeryToken();

    $.ajax({
        url: '/TaskingArea/TaskOperations/ToggleOperationStar',
        type: 'POST',
        data: {
            id: operationId,
            __RequestVerificationToken: token
        },
        success: function (response) {
            if (response.success) {
                // بروزرسانی UI از طریق سیستم update-view
                if (response.status === 'update-view' && response.viewList) {
                    updateMultipleViews(response.viewList);
                }

                // ⭐⭐⭐ بروزرسانی کل صفحه تسک
                if (typeof TaskDetailsRefreshManager !== 'undefined' && TaskDetailsRefreshManager.isEnabled) {
                    setTimeout(() => {
                        TaskDetailsRefreshManager.refreshAll();
                    }, 500);
                }

                // نمایش پیام موفقیت
                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.success(response.message);
                } else {
                    showToastReal(response.message, 'success');
                }

                console.log('✅ Operation star toggled successfully');
            } else {
                // ⭐⭐⭐ در صورت خطا، بازگرداندن دکمه به حالت اولیه
                $button.prop('disabled', false).removeClass('disabled');
                $icon.attr('class', originalIconClass);

                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error(response.message || 'خطا در تغییر وضعیت ستاره');
                } else {
                    showToastReal(response.message || 'خطا در تغییر وضعیت ستاره', 'error');
                }
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ Error toggling star:', { xhr, status, error });

            // ⭐⭐⭐ بازگرداندن دکمه به حالت اولیه در صورت خطا
            $button.prop('disabled', false).removeClass('disabled');
            $icon.attr('class', originalIconClass);

            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.error('خطا در ارتباط با سرور');
            } else {
                showToastReal('خطا در ارتباط با سرور', 'error');
            }
        }
    });
}

/**
 * تغییر وضعیت تکمیل عملیات (Checkbox)
 */
function toggleOperationComplete(operationId) {
    if (!operationId) {
        console.error('❌ Operation ID is required');
        return;
    }

    // ⭐⭐⭐ پیدا کردن چک‌باکس و جلوگیری از کلیک مجدد
    const $checkbox = $(`.operation-checkbox[data-operation-id="${operationId}"]`);

    if ($checkbox.prop('disabled')) {
        console.log('⚠️ Checkbox already processing...');
        return; // جلوگیری از کلیک مجدد
    }

    // ⭐⭐⭐ Disable کردن چک‌باکس و نمایش اسپینر
    $checkbox.prop('disabled', true);

    // ⭐⭐⭐ اضافه کردن اسپینر کنار چک‌باکس
    const $operationItem = $checkbox.closest('.operation-item-card');
    let $spinner = $operationItem.find('.operation-processing-spinner');

    if ($spinner.length === 0) {
        $spinner = $('<i class="fa fa-spinner fa-spin ms-2 operation-processing-spinner text-primary"></i>');
        $checkbox.after($spinner);
    } else {
        $spinner.show();
    }

    const token = getAntiForgeryToken();

    $.ajax({
        url: '/TaskingArea/TaskOperations/ToggleOperationComplete',
        type: 'POST',
        data: {
            id: operationId,
            __RequestVerificationToken: token
        },
        success: function (response) {
            if (response.success) {
                // بروزرسانی آمارها و Partial Views
                if (response.status === 'update-view' && response.viewList) {
                    updateMultipleViews(response.viewList);
                }

                // ⭐⭐⭐ بروزرسانی کل صفحه تسک
                if (typeof TaskDetailsRefreshManager !== 'undefined' && TaskDetailsRefreshManager.isEnabled) {
                    setTimeout(() => {
                        TaskDetailsRefreshManager.refreshAll();
                    }, 500);
                }

                // نمایش پیام
                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.success(response.message);
                } else {
                    showToastReal(response.message, 'success');
                }

                // بروزرسانی Badge در تب Operations
                if (typeof DynamicOperationsManager !== 'undefined') {
                    DynamicOperationsManager.updateStats();
                }

                console.log('✅ Operation completion toggled successfully');
            } else {
                // ⭐⭐⭐ در صورت خطا، بازگرداندن چک‌باکس
                $checkbox.prop('disabled', false);
                $spinner.hide();

                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error(response.message || 'خطا در تغییر وضعیت تکمیل');
                } else {
                    showToastReal(response.message || 'خطا در تغییر وضعیت تکمیل', 'error');
                }
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ Error toggling completion:', { xhr, status, error });

            // ⭐⭐⭐ بازگرداندن چک‌باکس به حالت اولیه
            $checkbox.prop('disabled', false);
            $spinner.hide();

            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.error('خطا در ارتباط با سرور');
            } else {
                showToastReal('خطا در ارتباط با سرور', 'error');
            }
        }
    });
}

/**
 * حذف عملیات (با تأیید)
 */
function deleteOperation(operationId) {
    if (!operationId) {
        console.error('❌ Operation ID is required');
        return;
    }

    // تأیید حذف
    Swal.fire({
        title: 'حذف عملیات',
        text: 'آیا از حذف این عملیات اطمینان دارید؟',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'بله، حذف شود',
        cancelButtonText: 'انصراف',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            performDeleteOperation(operationId);
        }
    });
}
/**
 * انجام عملیات حذف (پس از تأیید)
 */
function performDeleteOperation(operationId) {
    // ⭐⭐⭐ پیدا کردن دکمه حذف و جلوگیری از کلیک مجدد
    const $deleteButton = $(`.btn-delete-operation[data-operation-id="${operationId}"]`);

    if ($deleteButton.prop('disabled')) {
        console.log('⚠️ Delete already in progress...');
        return;
    }

    // ⭐⭐⭐ Disable کردن دکمه و نمایش اسپینر
    const $icon = $deleteButton.find('i');
    const originalIconClass = $icon.attr('class');

    $deleteButton.prop('disabled', true).addClass('disabled');
    $icon.attr('class', 'fa fa-spinner fa-spin');

    const token = getAntiForgeryToken();

    $.ajax({
        url: '/TaskingArea/TaskOperations/DeleteOperation',
        type: 'POST',
        data: {
            id: operationId,
            __RequestVerificationToken: token
        },
        success: function (response) {
            if (response.success) {
                // ⭐⭐⭐ بروزرسانی UI از طریق viewList
                if (response.status === 'update-view' && response.viewList) {
                    updateMultipleViews(response.viewList);
                }

                // ⭐⭐⭐ بروزرسانی Badge و آمار عملیات
                if (typeof DynamicOperationsManager !== 'undefined') {
                    DynamicOperationsManager.updateStats();
                }

                // ⭐⭐⭐ بروزرسانی کل صفحه تسک (Hero Stats, Progress Bar, Sidebar)
                if (typeof TaskDetailsRefreshManager !== 'undefined' && TaskDetailsRefreshManager.isEnabled) {
                    setTimeout(() => {
                        TaskDetailsRefreshManager.refreshAll();
                    }, 500);
                }

                // نمایش پیام موفقیت
                Swal.fire({
                    icon: 'success',
                    title: 'حذف شد',
                    text: response.message || 'عملیات با موفقیت حذف شد',
                    timer: 2000,
                    showConfirmButton: false
                });

                console.log('✅ Operation deleted successfully');
            } else {
                // ⭐⭐⭐ در صورت خطا، بازگرداندن دکمه
                $deleteButton.prop('disabled', false).removeClass('disabled');
                $icon.attr('class', originalIconClass);

                Swal.fire({
                    icon: 'error',
                    title: 'خطا',
                    text: response.message || 'خطا در حذف عملیات'
                });
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ Error deleting operation:', { xhr, status, error });

            // ⭐⭐⭐ بازگرداندن دکمه در صورت خطا
            $deleteButton.prop('disabled', false).removeClass('disabled');
            $icon.attr('class', originalIconClass);

            Swal.fire({
                icon: 'error',
                title: 'خطا',
                text: 'خطا در ارتباط با سرور'
            });
        }
    });
}
// ========================================
// ⭐⭐⭐ Event Handlers - باید در Document Ready فراخوانی شود
// ========================================
/**
 * راه‌اندازی Event Handlers برای دکمه‌های عملیات
 */
function initializeOperationHandlers() {
    console.log('🔧 Initializing operation handlers...');

    // ⭐⭐⭐ تغییر از $(document).on به استفاده مستقیم از document root
    // Handler برای دکمه ستاره
    $(document).off('click', '.btn-toggle-star').on('click', '.btn-toggle-star', function (e) {
        e.preventDefault();
        console.log('⭐ Star button clicked');
        const operationId = $(this).data('operation-id');
        console.log('Operation ID:', operationId);
        if (operationId) {
            toggleOperationStar(operationId);
        } else {
            console.error('❌ No operation ID found');
        }
    });

    // Handler برای Checkbox تکمیل
    $(document).off('change', '.operation-checkbox').on('change', '.operation-checkbox', function () {
        console.log('☑️ Checkbox clicked');
        const operationId = $(this).data('operation-id');
        console.log('Operation ID:', operationId);
        if (operationId) {
            toggleOperationComplete(operationId);
        } else {
            console.error('❌ No operation ID found');
        }
    });

    // Handler برای دکمه حذف
    $(document).off('click', '.btn-delete-operation').on('click', '.btn-delete-operation', function (e) {
        e.preventDefault();
        console.log('🗑️ Delete button clicked');
        const operationId = $(this).data('operation-id');
        console.log('Operation ID:', operationId);
        if (operationId) {
            deleteOperation(operationId);
        } else {
            console.error('❌ No operation ID found');
        }
    });

    // Handler برای دکمه افزودن WorkLog
    $(document).off('click', '.btn-add-worklog').on('click', '.btn-add-worklog', function (e) {
        e.preventDefault();
        console.log('📝 Add WorkLog button clicked');
        const operationId = $(this).data('operation-id');
        console.log('Operation ID:', operationId);
        if (operationId) {
            openAddWorkLogModal(operationId);
        } else {
            console.error('❌ No operation ID found');
        }
    });

    // Handler برای دکمه مشاهده WorkLogs
    $(document).off('click', '.btn-view-worklogs').on('click', '.btn-view-worklogs', function (e) {
        e.preventDefault();
        console.log('👁️ View WorkLogs button clicked');
        const operationId = $(this).data('operation-id');
        console.log('Operation ID:', operationId);
        if (operationId) {
            showViewWorkLogsModal(operationId);
        } else {
            console.error('❌ No operation ID found');
        }
    });

    console.log('✅ Operation handlers initialized');
}

/**
 * باز کردن مودال افزودن WorkLog
 */
async function openAddWorkLogModal(operationId) {
    if (!operationId) return;

    console.log('📝 Opening Add WorkLog modal for operation:', operationId);

    try {
        const response = await fetch(`/TaskingArea/TaskOperations/AddWorkLogModal?operationId=${operationId}`);
        const html = await response.text();

        // استفاده از showModal اگر موجود است
        if (typeof showModal === 'function') {
            showModal('ثبت گزارش کار', html);
        } else {
            // Fallback: استفاده از Bootstrap Modal
            const modalHtml = `
                <div class="modal fade" id="workLogModal" tabindex="-1">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content">
                            ${html}
                        </div>
                    </div>
                </div>
            `;
            $('body').append(modalHtml);
            const modal = new bootstrap.Modal(document.getElementById('workLogModal'));
            modal.show();
            $('#workLogModal').on('hidden.bs.modal', function () {
                $(this).remove();
            });
        }
    } catch (error) {
        console.error('❌ Error loading modal:', error);
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.error('خطا در بارگذاری مودال');
        }
    }
}
// ========================================
// ⭐⭐⭐ Dynamic Operations Manager
// ========================================
const DynamicOperationsManager = {
    config: null,
    taskId: null,

    init: function (config) {
        this.config = config || window.TaskDetailConfig;

        if (!this.config) {
            console.error('❌ TaskDetailConfig not found for DynamicOperationsManager!');
            return;
        }

        this.taskId = this.config.taskId;
        console.log('✅ Dynamic Operations Manager initialized for task:', this.taskId);
    },
    /**
 * افزودن عملیات جدید
 */
    addOperation: function () {
        const $input = $('#newOperationTitleInput');
        const title = $input.val().trim();

        if (!title) {
            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.warning('لطفاً عنوان عملیات را وارد کنید');
            } else {
                showToastReal('لطفاً عنوان عملیات را وارد کنید', 'warning');
            }
            $input.focus();
            return;
        }

        // ⭐⭐⭐ جلوگیری از ارسال چندباره
        const $addButton = $('#addNewOperationBtn');

        if ($addButton.prop('disabled')) {
            console.log('⚠️ Add operation already in progress...');
            return;
        }

        // ⭐⭐⭐ Disable کردن input و button + نمایش اسپینر
        $input.prop('disabled', true);
        $addButton.prop('disabled', true);

        const originalButtonHtml = $addButton.html();
        $addButton.html('<i class="fa fa-spinner fa-spin me-1"></i>در حال افزودن...');

        const token = getAntiForgeryToken();

        $.ajax({
            url: '/TaskingArea/TaskOperations/AddOperation',
            type: 'POST',
            data: {
                taskId: this.taskId,
                title: title,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    // ⭐⭐⭐ پاک کردن input/textarea
                    $input.val('');

                    // بروزرسانی UI
                    if (response.status === 'update-view' && response.viewList) {
                        updateMultipleViews(response.viewList);
                    }

                    // ⭐⭐⭐ بروزرسانی کل صفحه تسک
                    if (typeof TaskDetailsRefreshManager !== 'undefined' && TaskDetailsRefreshManager.isEnabled) {
                        setTimeout(() => {
                            TaskDetailsRefreshManager.refreshAll();
                        }, 500);
                    }

                    // نمایش پیام
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.success(response.message || 'عملیات با موفقیت اضافه شد');
                    } else {
                        showToastReal(response.message || 'عملیات با موفقیت اضافه شد', 'success');
                    }

                    // بروزرسانی آمار
                    DynamicOperationsManager.updateStats();
                    

                    console.log('✅ Operation added successfully');
                } else {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error(response.message || 'خطا در افزودن عملیات');
                    } else {
                        showToastReal(response.message || 'خطا در افزودن عملیات', 'error');
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('❌ Error adding operation:', { xhr, status, error });

                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error('خطا در ارتباط با سرور');
                } else {
                    showToastReal('خطا در ارتباط با سرور', 'error');
                }
            },
            complete: function () {
                // ⭐⭐⭐ بازگرداندن button و input به حالت عادی
                $input.prop('disabled', false).focus();
                $addButton.prop('disabled', false).html(originalButtonHtml);
            }
        });
    },
    /**
     * بروزرسانی آمار عملیات
     */
    updateStats: function () {
        if (!this.config || !this.taskId) {
            console.warn('⚠️ DynamicOperationsManager not properly initialized');
            return;
        }

        // شمارش عملیات‌ها از DOM
        const totalOps = $('.operation-item-card').length;
        const completedOps = $('.operation-item-card[data-completed="true"]').length;
        const pendingOps = totalOps - completedOps;
        const starredOps = $('.operation-item-card[data-starred="true"]').length;
        const $operationsBadge = $('#operations-badge-count');
        if (totalOps > 0) {
            $operationsBadge.text(totalOps).show();
        } else {
            $operationsBadge.text('0').hide();
        }


        // بروزرسانی آمار در صفحه
        $('#totalOpsCount').text(totalOps);
        $('#completedOpsCount').text(completedOps);
        $('#pendingOpsCount').text(pendingOps);
        $('#starredOpsCount').text(starredOps);

        console.log(`✅ Stats updated: Total=${totalOps}, Completed=${completedOps}, Pending=${pendingOps}, Starred=${starredOps}`);
    },

    /**
     * بارگذاری مجدد لیست عملیات
     */
    refreshOperationsList: function () {
        if (!this.config || !this.taskId) {
            console.warn('⚠️ DynamicOperationsManager not properly initialized');
            return;
        }

        const token = getAntiForgeryToken();

        $.ajax({
            url: '/TaskingArea/TaskOperations/GetAllOperations',
            type: 'GET',
            data: {
                taskId: this.taskId,
                __RequestVerificationToken: token
            },
            success: function (html) {
                $('#pending-operations-container').html(html);
                DynamicOperationsManager.updateStats();
                console.log('✅ Operations list refreshed');
            },
            error: function (xhr, status, error) {
                console.error('❌ Error refreshing operations:', { xhr, status, error });
            }
        });
    }
};

/**
* بروزرسانی چندین view به صورت همزمان
* @param {Array} viewList - آرایه‌ای از اشیاء {elementId, view}
*/

    