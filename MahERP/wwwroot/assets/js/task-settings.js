/**
 * ⭐⭐⭐ Task Settings Manager
 * مدیریت تنظیمات تسک با قابلیت‌های پیشرفته
 */

const TaskSettingsManager = {
    config: null,
    isDirty: false,

    /**
     * راه‌اندازی اولیه
     */
    init: function(config) {
        this.config = config;
        this.setupEventListeners();
        this.loadInitialState();
        console.log('✅ TaskSettingsManager initialized', config);
    },

    /**
     * نصب event listener ها
     */
    setupEventListeners: function() {
        const self = this;

        // تغییر در checkbox ها
        $(document).on('change', 'input[type="checkbox"][name*="Roles"]', function() {
            self.markAsDirty();
            self.validateChange($(this));
        });

        // هشدار قبل از خروج
        $(window).on('beforeunload', function(e) {
            if (self.isDirty) {
                e.preventDefault();
                return 'تغییرات ذخیره نشده‌اند. آیا مطمئن هستید؟';
            }
        });

        // ذخیره موفق
        $(document).on('ajaxSuccess', '[data-save="modal-ajax-save"]', function() {
            self.isDirty = false;
        });
    },

    /**
     * بارگذاری وضعیت اولیه
     */
    loadInitialState: function() {
        // ذخیره وضعیت اولیه در localStorage
        const initialState = this.getCurrentState();
        localStorage.setItem('taskSettings_initial_' + this.config.taskId, JSON.stringify(initialState));
    },

    /**
     * دریافت وضعیت فعلی
     */
    getCurrentState: function() {
        return {
            canComment: this.getCheckedRoles('CanCommentRoles'),
            canAddMembers: this.getCheckedRoles('CanAddMembersRoles'),
            canRemoveMembers: this.getCheckedRoles('CanRemoveMembersRoles'),
            canEditAfterCompletion: this.getCheckedRoles('CanEditAfterCompletionRoles'),
            creatorCanEditDelete: $('#CreatorCanEditDelete').is(':checked')
        };
    },

    /**
     * دریافت نقش‌های انتخاب شده
     */
    getCheckedRoles: function(fieldName) {
        const roles = [];
        $(`input[name="${fieldName}"]:checked`).each(function() {
            roles.push($(this).val());
        });
        return roles;
    },

    /**
     * علامت‌گذاری به عنوان تغییر یافته
     */
    markAsDirty: function() {
        this.isDirty = true;
        $('#task-settings-form .modal-footer .btn-primary')
            .addClass('btn-pulse')
            .html('<i class="fa fa-exclamation-circle me-1"></i>ذخیره تغییرات');
    },

    /**
     * اعتبارسنجی تغییر
     */
    validateChange: function($checkbox) {
        const userAuthority = this.getRoleAuthority(this.config.currentUserRole);
        const targetAuthority = parseInt($checkbox.data('authority'));

        // بررسی Authority Level
        if (targetAuthority < userAuthority) {
            $checkbox.prop('checked', true);
            this.showError('شما نمی‌توانید دسترسی افراد با سطح بالاتر را تغییر دهید');
            return false;
        }

        return true;
    },

    /**
     * دریافت سطح اعتبار نقش
     */
    getRoleAuthority: function(role) {
        const map = {
            'Manager': 1,
            'Creator': 2,
            'Member': 3,
            'Supervisor': 4,
            'CarbonCopy': 5
        };
        return map[role] || 5;
    },

    /**
     * نمایش خطا
     */
    showError: function(message) {
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.error(message);
        } else {
            alert(message);
        }
    },

    /**
     * کپی تنظیمات
     */
    copySettings: function() {
        const settings = this.getCurrentState();
        localStorage.setItem('taskSettingsClipboard', JSON.stringify(settings));
        
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.success('تنظیمات کپی شد!');
        }
    },

    /**
     * چسباندن تنظیمات
     */
    pasteSettings: function() {
        const clipboard = localStorage.getItem('taskSettingsClipboard');
        if (!clipboard) {
            this.showError('هیچ تنظیماتی کپی نشده است!');
            return;
        }

        try {
            const settings = JSON.parse(clipboard);
            
            // اعمال تنظیمات
            this.applySettings(settings);
            
            if (typeof NotificationHelper !== 'undefined') {
                NotificationHelper.success('تنظیمات اعمال شد!');
            }
        } catch (e) {
            this.showError('خطا در خواندن تنظیمات کپی شده');
        }
    },

    /**
     * اعمال تنظیمات
     */
    applySettings: function(settings) {
        // پاک کردن همه
        $('input[type="checkbox"][name*="Roles"]').prop('checked', false);

        // اعمال جدید
        settings.canComment.forEach(role => {
            $(`input[name="CanCommentRoles"][value="${role}"]`).prop('checked', true);
        });

        settings.canAddMembers.forEach(role => {
            $(`input[name="CanAddMembersRoles"][value="${role}"]`).prop('checked', true);
        });

        settings.canRemoveMembers.forEach(role => {
            $(`input[name="CanRemoveMembersRoles"][value="${role}"]`).prop('checked', true);
        });

        settings.canEditAfterCompletion.forEach(role => {
            $(`input[name="CanEditAfterCompletionRoles"][value="${role}"]`).prop('checked', true);
        });

        $('#CreatorCanEditDelete').prop('checked', settings.creatorCanEditDelete);

        // بروزرسانی UI
        if (typeof checkImpact === 'function') checkImpact();
        if (typeof updateLivePreview === 'function') updateLivePreview();

        this.markAsDirty();
    },

    /**
     * بازنشانی به حالت اولیه
     */
    reset: function() {
        const initial = localStorage.getItem('taskSettings_initial_' + this.config.taskId);
        if (initial) {
            this.applySettings(JSON.parse(initial));
            this.isDirty = false;
        }
    }
};

// Expose globally
window.TaskSettingsManager = TaskSettingsManager;

// ⭐⭐⭐ Helper Functions برای استفاده در View

/**
 * کپی کردن تنظیمات
 */
function copyTaskSettings() {
    TaskSettingsManager.copySettings();
}

/**
 * چسباندن تنظیمات
 */
function pasteTaskSettings() {
    TaskSettingsManager.pasteSettings();
}

/**
 * انیمیشن pulse برای دکمه
 */
$.fn.extend({
    addPulse: function() {
        return this.addClass('btn-pulse');
    },
    removePulse: function() {
        return this.removeClass('btn-pulse');
    }
});

// CSS برای انیمیشن
const style = document.createElement('style');
style.textContent = `
    @keyframes pulse {
        0% { box-shadow: 0 0 0 0 rgba(var(--bs-primary-rgb), 0.7); }
        70% { box-shadow: 0 0 0 10px rgba(var(--bs-primary-rgb), 0); }
        100% { box-shadow: 0 0 0 0 rgba(var(--bs-primary-rgb), 0); }
    }

    .btn-pulse {
        animation: pulse 2s infinite;
    }
`;
document.head.appendChild(style);
