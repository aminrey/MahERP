
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

// ========================================
// ⭐⭐⭐ Dynamic Operations Manager
// ========================================
const DynamicOperationsManager = {
    taskId: null,
    config: null,

    init: function (config) {
        this.config = config || window.TaskDetailConfig;
        this.taskId = this.config.taskId;
        console.log('✅ Dynamic Operations Manager initialized');
    },

    refreshGroup: function (groupType) {
        if (!groupType || typeof groupType !== 'string' || groupType.trim() === '') {
            console.error('❌ Invalid GroupType:', groupType);
            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.error('خطا: نوع گروه نامعتبر است');
            }
            return;
        }

        const normalizedGroupType = groupType.trim().toLowerCase();

        if (!['starred', 'pending', 'completed'].includes(normalizedGroupType)) {
            console.error('❌ Unknown GroupType:', normalizedGroupType);
            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.error(`نوع گروه نامعتبر: ${normalizedGroupType}`);
            }
            return;
        }

        console.log(`🔄 Refreshing group: "${normalizedGroupType}"`);

        const self = this;
        $.ajax({
            url: self.config.urls.getOperationsGroup,
            type: 'GET',
            data: {
                taskId: self.taskId,
                groupType: normalizedGroupType
            },
            success: function (result) {
                console.log(`📦 Response for "${normalizedGroupType}":`, result);

                if (!result || typeof result !== 'object') {
                    console.error('❌ Invalid response format:', result);
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error('خطا در دریافت پاسخ سرور');
                    }
                    return;
                }

                if (result.success) {
                    const groupId = `${normalizedGroupType}-group`;
                    $(`#${groupId}`).remove();
                    $(`.operations-group[data-group="${normalizedGroupType}"]`).remove();

                    console.log(`🗑️ Removed old group: #${groupId}`);

                    const placeholderId = `${normalizedGroupType}-group-placeholder`;

                    if (result.html && typeof result.html === 'string' && result.html.trim() !== '') {
                        $(`#${placeholderId}`).html(result.html);
                        console.log(`✅ Added group "${normalizedGroupType}" to placeholder with ${result.count} operations`);
                    } else {
                        $(`#${placeholderId}`).empty();
                        console.log(`ℹ️ Group "${normalizedGroupType}" is empty - placeholder cleared`);
                    }

                    self.updateStats();
                } else {
                    console.error(`❌ Server error:`, result.message);
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error(result.message || 'خطا در سرور');
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error(`❌ AJAX error for "${normalizedGroupType}":`, xhr);
                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error('خطا در ارتباط با سرور');
                }
            }
        });
    },

    updateStats: function () {
        const total = $('.operation-item-card').length;
        const completed = $('.operation-item-card[data-completed="true"]').length;
        const starred = $('.operation-item-card[data-starred="true"]').length;
        const withWorkLog = $('.operation-item-card .btn-info:not(.btn-outline-info)').length;

        $('#totalOpsCount').text(total);
        $('#completedOpsCount').text(completed);
        $('#starredOpsCount').text(starred);
        $('#withWorkLogCount').text(withWorkLog);

        console.log(`📊 Stats - Total: ${total}, Completed: ${completed}, Starred: ${starred}`);
    },

    toggleComplete: function (operationId) {
        const $card = $(`#operation-card-${operationId}`);
        if (!$card.length) {
            console.error(`❌ Card not found: operation-card-${operationId}`);
            return;
        }

        const $spinner = $card.find('.operation-spinner');
        const wasCompleted = $card.attr('data-completed') === 'true';
        const isStarred = $card.attr('data-starred') === 'true';

        console.log(`🔄 Toggle operation ${operationId}:`, {
            wasCompleted,
            isStarred,
            willMoveTo: wasCompleted ? 'pending' : 'completed'
        });

        $card.css('opacity', '0.6');
        $spinner.show();

        const self = this;
        $.ajax({
            url: self.config.urls.toggleOperationComplete,
            type: 'POST',
            data: {
                id: operationId,
                __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.success(result.message || 'وضعیت تغییر کرد');
                    }

                    $card.fadeOut(200, function () {
                        $(this).remove();
                        console.log(`✅ Card ${operationId} removed`);

                        if (wasCompleted) {
                            setTimeout(() => self.refreshGroup('completed'), 100);
                            setTimeout(() => self.refreshGroup(isStarred ? 'starred' : 'pending'), 300);
                        } else {
                            setTimeout(() => self.refreshGroup(isStarred ? 'starred' : 'pending'), 100);
                            setTimeout(() => self.refreshGroup('completed'), 300);
                        }
                    });
                } else {
                    console.error('❌ Toggle failed:', result.message);
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error(result.message);
                    }
                    $card.css('opacity', '1');
                    $spinner.hide();
                }
            },
            error: function (xhr) {
                console.error('❌ Toggle error:', xhr);
                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error('خطا در ذخیره');
                }
                $card.css('opacity', '1');
                $spinner.hide();
            }
        });
    },

    toggleStar: function (operationId) {
        const $card = $(`#operation-card-${operationId}`);
        if (!$card.length) return;

        const $spinner = $card.find('.operation-spinner');
        console.log(`⭐ Toggling star for operation ${operationId}`);

        $card.css('opacity', '0.6');
        $spinner.show();

        const self = this;
        $.ajax({
            url: self.config.urls.toggleOperationStar,
            type: 'POST',
            data: {
                id: operationId,
                __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.success(result.message || 'ستاره تغییر کرد');
                    }

                    $card.fadeOut(200, function () {
                        $(this).remove();

                        setTimeout(() => {
                            self.refreshGroup('starred');
                        }, 100);

                        setTimeout(() => {
                            self.refreshGroup('pending');
                        }, 300);
                    });
                } else {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error(result.message);
                    }
                    $card.css('opacity', '1');
                    $spinner.hide();
                }
            },
            error: function () {
                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error('خطا در ذخیره');
                }
                $card.css('opacity', '1');
                $spinner.hide();
            }
        });
    },

    deleteOperation: function (operationId) {
        if (!confirm('آیا از حذف این عملیات اطمینان دارید؟')) return;

        const $card = $(`#operation-card-${operationId}`);
        const self = this;

        $.ajax({
            url: self.config.urls.deleteOperation,
            type: 'POST',
            data: {
                id: operationId,
                __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.success('عملیات حذف شد');
                    }

                    $card.fadeOut(300, function () {
                        $(this).remove();
                        self.updateStats();
                    });
                } else {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error(result.message);
                    }
                }
            },
            error: function () {
                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error('خطا در حذف');
                }
            }
        });
    },

    addOperation: function () {
        const title = $('#newOperationTitleInput').val().trim();

        if (!title) {
            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.warning('لطفاً عنوان عملیات را وارد کنید');
            }
            $('#newOperationTitleInput').focus();
            return;
        }

        const self = this;
        $.ajax({
            url: self.config.urls.addOperation,
            type: 'POST',
            data: {
                taskId: self.taskId,
                title: title,
                __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.success('عملیات اضافه شد');
                    }
                    $('#newOperationTitleInput').val('');

                    setTimeout(() => {
                        $.ajax({
                            url: self.config.urls.getAllOperations,
                            type: 'GET',
                            data: { taskId: self.taskId },
                            success: function (html) {
                                if (html && html.trim() !== '') {
                                    $('#operationsList').html(html);
                                    $('#emptyOperationsList').hide();
                                    self.updateStats();
                                    console.log('✅ Operations list refreshed');
                                } else {
                                    console.warn('⚠️ Empty HTML received');
                                }
                            },
                            error: function () {
                                self.refreshGroup('pending');
                            }
                        });
                    }, 200);
                } else {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error(result.message);
                    }
                }
            },
            error: function () {
                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error('خطا در افزودن عملیات');
                }
            }
        });
    }
};

// ========================================
// ⭐⭐⭐ Chat Manager
// ========================================
const TaskChatManager = {
    taskId: null,
    config: null,
    selectedFiles: [],

    init: function (config) {
        this.config = config || window.TaskDetailConfig;
        this.taskId = this.config.taskId;
        this.scrollToBottom();
        this.autoRefresh();

        console.log('✅ Chat Manager initialized');
    },

    scrollToBottom: function () {
        const container = $('#chat-messages-container');
        if (container.length) {
            container.scrollTop(container[0].scrollHeight);
        }
    },

    autoRefresh: function () {
        const self = this;
        setInterval(() => {
            self.loadNewMessages();
        }, 30000);
    },

    loadNewMessages: function () {
        const self = this;
        $.ajax({
            url: self.config.urls.getTaskComments,
            type: 'GET',
            data: { taskId: self.taskId },
            success: function (response) {
                if (response.success && response.html) {
                    $('#chat-messages-container').html(response.html);
                    self.scrollToBottom();
                }
            }
        });
    },
    Detail.js
sendMessage: function (messageText, isImportant) {
        const self = this;
        const formData = new FormData();
        formData.append('TaskId', self.taskId);
        formData.append('CommentText', messageText);
        formData.append('IsImportant', isImportant);
        formData.append('__RequestVerificationToken', $('[name="__RequestVerificationToken"]').val());

        self.selectedFiles.forEach((file, index) => {
            formData.append(`Attachments[${index}]`, file);
        });

        $.ajax({
            url: self.config.urls.addTaskComment,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.success('پیام ارسال شد');
                    }
                    $('#chat-message-input').val('');
                    $('#chat-important-check').prop('checked', false);
                    self.selectedFiles = [];
                    $('#chat-selected-files').hide().empty();
                    self.loadNewMessages();

                    // ⭐⭐⭐ بروزرسانی Badge
                    self.updateCommentCount();
                } else {
                    if (typeof NotificationHelper !== 'undefined') {
                        NotificationHelper.error(response.message || 'خطا در ارسال پیام');
                    }
                }
            },
            error: function () {
                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error('خطا در ارسال پیام');
                }
            }
        });
    },

    // ⭐⭐⭐ متد جدید: بروزرسانی تعداد کامنت‌ها
    updateCommentCount: function () {
        const currentCount = $('.chat-message').length;
        const $badge = $('#chat-badge-count');

        if (currentCount > 0) {
            $badge.text(currentCount).show();
        } else {
            $badge.hide();
        }

        console.log(`✅ Comment badge updated: ${currentCount}`);
    }
};

// ========================================
// ⭐⭐⭐ Global Functions
// ========================================

function sendChatMessage(event) {
    event.preventDefault();

    const message = $('#chat-message-input').val().trim();
    if (!message) {
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.warning('لطفاً پیام خود را بنویسید');
        }
        return false;
    }

    const isImportant = $('#chat-important-check').is(':checked');
    TaskChatManager.sendMessage(message, isImportant);

    return false;
}

function handleChatKeyPress(event) {
    if (event.ctrlKey && event.keyCode === 13) {
        sendChatMessage(event);
    }
}

function handleChatFileSelect(input) {
    TaskChatManager.selectedFiles = Array.from(input.files);

    if (TaskChatManager.selectedFiles.length > 0) {
        let filesHtml = '<div class="alert alert-info">';
        filesHtml += '<i class="fa fa-paperclip me-1"></i> فایل‌های انتخاب شده:';
        filesHtml += '<ul class="mb-0 mt-1">';
        TaskChatManager.selectedFiles.forEach(file => {
            filesHtml += `<li>${file.name} (${(file.size / 1024).toFixed(2)} KB)</li>`;
        });
        filesHtml += '</ul></div>';

        $('#chat-selected-files').html(filesHtml).show();
    }
}
function deleteComment(commentId) {
    const config = window.TaskDetailConfig;

    if (typeof showDeleteConfirmation === 'function') {
        showDeleteConfirmation('این پیام').then(confirmed => {
            if (confirmed) {
                $.ajax({
                    url: config.urls.deleteTaskComment,
                    type: 'POST',
                    data: {
                        id: commentId,
                        __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
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

    $('.operation-checkbox').prop('disabled', true).css('cursor', 'not-allowed');
    $('[onclick^="dynamicToggleStar"]').prop('disabled', true).addClass('disabled');
    $('[onclick^="dynamicDeleteOperation"]').hide();
    $('#newOperationTitleInput, #addNewOperationBtn').prop('disabled', true);

    $('.operation-item-card').each(function () {
        $(this).css('opacity', '0.7');
        if (!$(this).find('.lock-badge').length) {
            $(this).prepend('<span class="badge bg-secondary lock-badge position-absolute top-0 end-0 m-2"><i class="fa fa-lock"></i></span>');
        }
    });

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

// Global functions for operations
function dynamicToggleComplete(checkbox, operationId) {
    if (window.TaskDetailConfig && window.TaskDetailConfig.isTaskCompleted) {
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.warning('این تسک تکمیل شده و قابل ویرایش نیست');
        }
        if (checkbox) {
            checkbox.checked = !checkbox.checked;
        }
        return false;
    }

    DynamicOperationsManager.toggleComplete(operationId);
}

function dynamicToggleStar(operationId) {
    DynamicOperationsManager.toggleStar(operationId);
}

function dynamicDeleteOperation(operationId) {
    DynamicOperationsManager.deleteOperation(operationId);
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

// ⭐⭐⭐ تابع جداگانه برای Initialize
function initializeTaskDetails() {
    const config = window.TaskDetailConfig;

    console.log('✅ Initializing Task Details with config:', config);

    // Initialize Managers
    TaskDetailsRefreshManager.init(config.taskId, config);
    DynamicOperationsManager.init(config);

    // Operations input handlers
    $('#newOperationTitleInput').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            DynamicOperationsManager.addOperation();
        }
    });

    $('#addNewOperationBtn').on('click', function () {
        DynamicOperationsManager.addOperation();
    });

    // Update stats on load
    if (typeof DynamicOperationsManager !== 'undefined' && DynamicOperationsManager.config) {
        DynamicOperationsManager.updateStats();
    }

    // Chat tab click handler
    $(document).on('click', '#chat-tab', function () {
        setTimeout(() => {
            if (window.TaskDetailConfig) {
                TaskChatManager.init(config);
            }
        }, 300);
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

console.log('✅ TaskDetail.js loaded successfully');