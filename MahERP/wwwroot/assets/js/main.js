const getAntiForgeryToken = function () {
    // روش 1: از input مخفی
    let token = $('input[name="__RequestVerificationToken"]').val();

    // روش 2: از فرم
    if (!token) {
        token = $('form').find('input[name="__RequestVerificationToken"]').val();
    }

    // روش 3: از meta tag
    if (!token) {
        token = $('meta[name="__RequestVerificationToken"]').attr('content');
    }

    console.log('🔑 AntiForgery Token:', token ? 'Found' : '❌ NOT FOUND');
    return token;
};
/**
 * Bootstrap5Modal.js
 * Enhanced modal functionality for Bootstrap 5 
 */

// Utility functions for URL 
const ModalUtils = {

    /**
     * Process all URLs in a container to 
     * @param {HTMLElement|jQuery} container - Container element to process
     */
    processUrlsInContainer: function (container) {
        const $container = $(container);

        // Process buttons with formaction
        $container.find("button[formaction]").each((i, button) => {
            const formaction = $(button).attr("formaction");
            if (formaction) {
                $(button).attr("formaction", formaction);
            }
        });

        // Process links with href
        $container.find("a[href]").each((i, link) => {
            const href = $(link).attr("href");
            if (href && href !== '#' && !href.startsWith('javascript:')) {
                $(link).attr("href", href);
            }
        });

      
    },

    /**
     * Get URL from various possible sources on an element
     * @param {jQuery} element - Element to get URL from
     * @returns {string} - The URL
     */
    getUrlFromElement: function ($element) {
        const targetUrl = $element.data('url');
        const href = $element.attr('href');
        const formaction = $element.attr('formaction');
        const formUrl = $element.closest('form').attr('action');

        return targetUrl || href || formaction || formUrl;
    }
};

/**
 * Dynamic Select2 Handler
 * سیستم پویا برای مدیریت Select2 ها با قابلیت بروزرسانی خودکار
 */
const DynamicSelect2Manager = {
    // تنظیمات پیش‌فرض
    defaultSettings: {
        ajaxMethod: 'POST',
        loadingText: '<div class="text-center p-3"><i class="fa fa-spinner fa-spin"></i> در حال بارگذاری...</div>',
        errorText: '<div class="alert alert-danger">خطا در بارگذاری اطلاعات</div>',
        emptyText: 'ابتدا گزینه مناسب را انتخاب کنید',
        select2Config: {
            allowClear: true,
            width: '100%'
        }
    },

    // متغیرهای سیستم
    customConfigs: {},
    isInitialized: false,

    /**
     * راه‌اندازی سیستم
     */
    init: function() {
        if (this.isInitialized) return;
        
        this.initializeSelect2Elements();
        this.setupEventListeners();
        this.isInitialized = true;
        
        console.log('Dynamic Select2 Manager initialized');
    },

    /**
     * راه‌اندازی اولیه Select2 برای تمام عناصر
     */
    initializeSelect2Elements: function() {
        const self = this;
        
        $('.js-select2').each(function() {
            const $element = $(this);
            
            if (!$element.hasClass('select2-hidden-accessible')) {
                const container = $element.data('container') || 'body';
                const config = $.extend({}, self.defaultSettings.select2Config);
                
                if (container !== 'body') {
                    config.dropdownParent = $(container);
                }
                
                $element.select2(config);
            }
        });
    },

    /**
     * نصب event listener ها
     */
    setupEventListeners: function() {
        const self = this;
        
        // Event delegation برای عناصر select با data-toggle="multiplepartialview"
        $(document).off('change.dynamicSelect2').on('change.dynamicSelect2', 'select[data-toggle="multiplepartialview"]', function(e) {
            self.handleSelectChange($(this));
        });
    },

    /**
     * مدیریت تغییر select
     */
    handleSelectChange: function($select) {
        const self = this;
        const url = $select.data('url');
        const targetDivs = $select.data('target-divs');
        const selectedValue = $select.val();
        
        if (!url) {
            console.warn('No URL specified for dynamic select:', $select);
            return;
        }

        // تعیین target divs
        let targets = [];
        if (targetDivs) {
            targets = targetDivs.split(',').map(function(div) {
                return div.trim();
            });
        } else {
            targets = this.detectTargetDivs($select);
        }

        console.log('Select changed:', {
            value: selectedValue,
            url: url,
            targets: targets
        });

        // اگر مقدار خالی است، target ها را خالی کن
        if (!selectedValue || selectedValue === '') {
            this.clearTargetDivs(targets);
            return;
        }

        // ارسال درخواست AJAX
        this.sendAjaxRequest($select, url, selectedValue, targets);
    },

    /**
     * تشخیص خودکار target div ها
     */
    detectTargetDivs: function($select) {
        const commonTargets = ['UsersDiv', 'CarbonCopyUsersDiv', 'CustomersDiv', 'StakeholdersDiv'];
        const detectedTargets = [];
        
        commonTargets.forEach(function(target) {
            if ($('#' + target).length > 0) {
                detectedTargets.push(target);
            }
        });
        
        return detectedTargets;
    },

    /**
     * خالی کردن target div ها
     */
    clearTargetDivs: function(targets) {
        const self = this;
        
        targets.forEach(function(targetId) {
            const $target = $('#' + targetId);
            if ($target.length) {
                const emptySelect = self.createEmptySelect(targetId);
                $target.html(emptySelect);
                self.reinitializeSelect2InDiv($target);
            }
        });
    },

    /**
     * ایجاد select خالی
     */
    createEmptySelect: function(targetId) {
        const selectConfig = this.getSelectConfigByTargetId(targetId);
        
        return `<select class="js-select2 form-select" 
                        id="${selectConfig.id}" 
                        name="${selectConfig.name}" 
                        data-placeholder="${this.defaultSettings.emptyText}" 
                        ${selectConfig.multiple ? 'multiple="multiple"' : ''}
                        style="width: 100%;">
                </select>`;
    },

    /**
     * دریافت کانفیگ select بر اساس target id
     */
    getSelectConfigByTargetId: function(targetId) {
        const configs = {
            'UsersDiv': {
                id: 'UserId',
                name: 'AppointmentUsers',
                multiple: true
            },
            'CarbonCopyUsersDiv': {
                id: 'CarbonCopyUsersIdForcc',
                name: 'CarbonCopyUsers',
                multiple: true
            },
            'CustomersDiv': {
                id: 'StakeholderId',
                name: 'StakeholderId',
                multiple: false
            },
            'StakeholdersDiv': {
                id: 'StakeholderId',
                name: 'StakeholderId',
                multiple: false
            }
        };
        
        // بررسی کانفیگ‌های سفارشی
        if (this.customConfigs[targetId]) {
            return this.customConfigs[targetId];
        }
        
        return configs[targetId] || {
            id: targetId.replace('Div', ''),
            name: targetId.replace('Div', ''),
            multiple: false
        };
    },

    /**
     * ارسال درخواست AJAX
     */
    sendAjaxRequest: function($select, url, selectedValue, targets) {
        const self = this;
        
        // نمایش loading
        this.showLoadingInTargets(targets);
        
        // تعیین نام پارامتر اصلی
        const paramName = this.getParamNameFromSelect($select);
        const ajaxData = {};
        ajaxData[paramName] = selectedValue;
        
        // اضافه کردن پارامتر اضافی (مثل BranchIdSelected)
        const branchParam = $select.data('branch-param');
        if (branchParam) {
            const branchValue = $('#' + branchParam).val();
            if (branchValue) {
                ajaxData[branchParam] = branchValue;
            }
        }
        
        // اضافه کردن CSRF token
        const token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            ajaxData.__RequestVerificationToken = token;
        }

 

        $.ajax({
            url: url,
            type: this.defaultSettings.ajaxMethod,
            data: ajaxData,
            beforeSend: function() {
                console.log('Sending AJAX request to:', url, 'with data:', ajaxData);
            },
            success: function(response) {
                console.log('AJAX response received:', response);
                self.handleAjaxSuccess(response, targets);
            },
            error: function(xhr, status, error) {
                console.error('AJAX request failed:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    error: error,
                    responseText: xhr.responseText
                });
                self.handleAjaxError(error, targets);
            }
        });
    },

    /**
     * تشخیص نام پارامتر از روی select
     */
    getParamNameFromSelect: function($select) {
        // بررسی data attribute مخصوص
        const customParam = $select.data('param-name');
        if (customParam) {
            return customParam;
        }

        // بر اساس کلاس‌های select
        if ($select.hasClass('branch')) {
            return 'branchId';
        }
        if ($select.hasClass('category')) {
            return 'categoryId';
        }
        if ($select.hasClass('stakeholder')) {
            return 'stakeholderId';
        }
        
        // پیش‌فرض
        return 'id';
    },

    /**
     * نمایش loading در target ها
     */
    showLoadingInTargets: function(targets) {
        const self = this;
        
        targets.forEach(function(targetId) {
            const $target = $('#' + targetId);
            if ($target.length) {
                $target.html(self.defaultSettings.loadingText);
            }
        });
    },

    /**
     * مدیریت پاسخ موفق AJAX
     */
    handleAjaxSuccess: function(response, targets) {
        const self = this;
        
        if (response.status === "update-view" && response.viewList) {
            response.viewList.forEach(function(item) {
                if (targets.includes(item.elementId)) {
                    const $target = $('#' + item.elementId);
                    if ($target.length && item.view && item.view.result) {
                        $target.html(item.view.result);
                        self.reinitializeSelect2InDiv($target);
                        
                        // Process URLs in updated content
                        ModalUtils.processUrlsInContainer($target);
                    }
                }
            });
        } else {
            console.warn('Unexpected response format:', response);
            this.handleAjaxError('فرمت پاسخ نامعتبر', targets);
        }
    },

    /**
     * مدیریت خطای AJAX
     */
    handleAjaxError: function(error, targets) {
        const self = this;
        
        targets.forEach(function(targetId) {
            const $target = $('#' + targetId);
            if ($target.length) {
                $target.html(self.defaultSettings.errorText + ': ' + error);
            }
        });
    },

    /**
     * راه‌اندازی مجدد Select2 در container
     */
    reinitializeSelect2InDiv: function($container) {
        const self = this;
        
        setTimeout(function() {
            $container.find('.js-select2').each(function() {
                const $element = $(this);
                
                // destroy قبلی اگر وجود دارد
                if ($element.hasClass('select2-hidden-accessible')) {
                    $element.select2('destroy');
                }
                
                // راه‌اندازی مجدد
                const containerAttr = $element.data('container') || 'body';
                const config = $.extend({}, self.defaultSettings.select2Config);
                
                if (containerAttr !== 'body') {
                    config.dropdownParent = $(containerAttr);
                }
                
                $element.select2(config);
            });
            
            console.log('Select2 reinitialized in container');
        }, 100);
    },

    /**
     * اضافه کردن کانفیگ سفارشی
     */
    addSelectConfig: function(targetId, config) {
        this.customConfigs[targetId] = config;
    },

    /**
     * بروزرسانی تنظیمات
     */
    updateSettings: function(newSettings) {
        $.extend(this.defaultSettings, newSettings);
    },

    /**
     * راه‌اندازی مجدد کل سیستم
     */
    reinitialize: function() {
        this.initializeSelect2Elements();
    }
};

// Modal handling functions
/**
 * Create and show a modal without removing it on close
 * @param {string} url - URL to load content from
 */
function createAndShowModalWithoutRemove(url) {
    const uniqueModalId = 'modal-' + Date.now();
    const modalHtml = `<div aria-labelledby="${uniqueModalId}" class="modal fade" data-bs-focus="false" role="dialog" id="${uniqueModalId}" tabindex="-1" aria-hidden="true"></div>`;

    $('body').append(modalHtml);
    const $modal = $('#' + uniqueModalId);


    // Load content into the modal via AJAX
    $.get(urlWithLanguage).done(function (data) {
        $modal.html(data);
        $modal.find('.js-select2').attr('data-container', '#' + uniqueModalId);

        // Process URLs in loaded modal content
        ModalUtils.processUrlsInContainer($modal);

        // راه‌اندازی Dynamic Select2 در modal
        DynamicSelect2Manager.reinitializeSelect2InDiv($modal);

        $modal.modal('show');
    }).fail(function (jqXHR) {
        console.error("Failed to load modal content:", jqXHR.statusText);
        handleAjaxError(jqXHR);
    });
}

/**
 * ⭐⭐⭐ Create and show a modal - Compatible با ساختار Partial View موجود
 * @param {string|Object} urlOrOptions - URL or config object
 * @param {Object} options - Optional config
 */
function createAndShowModal(urlOrOptions, options = {}) {
    // ⭐ Parse parameters
    let config = {};

    if (typeof urlOrOptions === 'string') {
        config = { url: urlOrOptions, ...options };
    } else if (typeof urlOrOptions === 'object') {
        config = { ...urlOrOptions };
    } else {
        console.error('Invalid parameter type for createAndShowModal');
        return Promise.reject(new Error('Invalid parameter'));
    }

    // ⭐ Default config
    const defaults = {
        url: null,
        modalId: 'modal-' + Date.now(),
        backdrop: true, // ⭐⭐⭐ تغییر از 'static' به true
        keyboard: true,
        removeOnHide: true,
        onShown: null,
        onHidden: null,
        onSubmitSuccess: null,
        onLoadError: null
    };

    config = { ...defaults, ...config };

    // ⭐⭐⭐ بررسی اینکه مودالی در حال load است یا نه
    if (window.isModalLoading) {
        console.warn('⚠️ Modal is already loading, please wait...');
        return Promise.reject(new Error('Modal is already loading'));
    }

    // ⭐⭐⭐ بررسی اینکه مودالی قبلاً باز است یا نه
    if ($('.modal.show').length > 0) {
        console.warn('⚠️ Another modal is already open');
        return Promise.reject(new Error('Another modal is already open'));
    }

    // ⭐ Set loading flag
    window.isModalLoading = true;

    // ⭐⭐⭐ ساختار ساده مودال (فقط wrapper)
    const modalHtml = `<div class="modal fade" id="${config.modalId}" tabindex="-1" role="dialog" aria-hidden="true" data-bs-focus="false"></div>`;

    // ⭐ Add to DOM
    $('body').append(modalHtml);
    const $modal = $('#' + config.modalId);

    return new Promise((resolve, reject) => {

        $.ajax({
            url: config.url,
            type: 'GET',
            dataType: 'html',
            beforeSend: function () {
                console.log('📤 Loading modal from:', config.url);
            },
            success: function (data) {
                try {
                    // ⭐ Check for JSON error
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

                    // ⭐ Validate content
                    if (!data || data.trim() === '' || data === '{}') {
                        throw new Error('محتوای مودال خالی است');
                    }

                    console.log('✅ Modal content loaded');

                    // ⭐⭐⭐ محتوای Partial View شامل modal-dialog است
                    $modal.html(data);

                    // ⭐ Process URLs
                    if (typeof ModalUtils !== 'undefined' && ModalUtils.processUrlsInContainer) {
                        try {
                            ModalUtils.processUrlsInContainer($modal);
                        } catch (err) {
                            console.warn('⚠️ ModalUtils error:', err);
                        }
                    }

                    // ⭐ Initialize Select2
                    if (typeof DynamicSelect2Manager !== 'undefined') {
                        try {
                            $modal.find('.js-select2').attr('data-container', '#' + config.modalId);
                            DynamicSelect2Manager.reinitializeSelect2InDiv($modal);
                        } catch (err) {
                            console.warn('⚠️ Select2 error:', err);
                        }
                    }

                    // ⭐ Initialize Persian Datepicker
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

                    // ⭐ Show modal
                    const modalInstance = new bootstrap.Modal($modal[0], {
                        backdrop: config.backdrop,
                        keyboard: config.keyboard
                    });

                    modalInstance.show();

                    // ⭐ Setup form handler
                    setupModalFormHandler($modal, config, modalInstance);

                    // ⭐ Event: shown
                    $modal.on('shown.bs.modal', function () {
                        console.log('✅ Modal shown:', config.modalId);

                        // ⭐⭐⭐ Reset loading flag after modal is shown
                        window.isModalLoading = false;

                        if (typeof config.onShown === 'function') {
                            config.onShown(modalInstance, $modal);
                        }

                        resolve({ modal: modalInstance, element: $modal[0] });
                    });

                    // ⭐ Event: hidden
                    $modal.on('hidden.bs.modal', function () {
                        console.log('🔄 Modal hidden:', config.modalId);

                        if (typeof config.onHidden === 'function') {
                            config.onHidden(modalInstance, $modal);
                        }

                        if (config.removeOnHide) {
                            $modal.remove();
                        }
                    });

                } catch (processError) {
                    console.error('❌ Error processing modal:', processError);
                    $modal.remove();

                    // ⭐ Reset loading flag on error
                    window.isModalLoading = false;

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
                console.error('❌ AJAX Error:', {
                    status: jqXHR.status,
                    statusText: jqXHR.statusText,
                    error: errorThrown
                });

                $modal.remove();

                // ⭐ Reset loading flag on error
                window.isModalLoading = false;

                const errorMessage = getAjaxErrorMessage(jqXHR);

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

/**
 * ⭐ Setup form submission handler for modal
 */
function setupModalFormHandler($modal, config, modalInstance) {
    const $form = $modal.find('form');

    if (!$form.length) {
        console.log('ℹ️ No form found in modal');
        return;
    }

    console.log('📝 Setting up form handler');

    // ⭐ Handle submit button with data-save="modal-ajax-save"
    $modal.find('[data-save="modal-ajax-save"]').off('click').on('click', function (e) {
        e.preventDefault();

        const $submitBtn = $(this);
        const originalBtnText = $submitBtn.html();

        // Validation
        if (!$form[0].checkValidity()) {
            $form[0].reportValidity();
            return;
        }

        // Disable button
        $submitBtn.prop('disabled', true)
            .html('<i class="fa fa-spinner fa-spin"></i> در حال پردازش...');

        // TinyMCE support
        if (typeof tinymce !== 'undefined') {
            tinymce.triggerSave();
        }

        const formData = new FormData($form[0]);

        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            dataType: 'json',
            success: function (response) {
                console.log('✅ Form submitted:', response);

                if (response.status === 'redirect') {
                    if (response.message) {
                        SendResposeMessage(response.message);
                    }
                    setTimeout(() => {
                        if (response.redirectUrl) {
                            window.location.href = response.redirectUrl;
                        } else {
                            location.reload();
                        }
                    }, 500);
                }
                else if (response.status === 'update-view') {
                    modalInstance.hide();

                    if (response.viewList && response.viewList.length > 0) {
                        updateMultipleViews(response.viewList);
                    }

                    if (response.message) {
                        SendResposeMessage(response.message);
                    }

                    if (typeof config.onSubmitSuccess === 'function') {
                        config.onSubmitSuccess(response, modalInstance);
                    }
                }
                else if (response.status === 'success-from-list' || response.status === 'success') {
                    if (typeof config.onSubmitSuccess === 'function') {
                        config.onSubmitSuccess(response, modalInstance);
                    } else {
                        modalInstance.hide();

                        if (response.message) {
                            SendResposeMessage(response.message);
                        }

                        setTimeout(() => location.reload(), 500);
                    }
                }
                else if (response.status === 'validation-error' || response.status === 'error') {
                    $submitBtn.prop('disabled', false).html(originalBtnText);

                    if (response.message) {
                        SendResposeMessage(response.message);
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('❌ Form error:', error);
                $submitBtn.prop('disabled', false).html(originalBtnText);

                if (typeof NotificationHelper !== 'undefined') {
                    NotificationHelper.error('خطا در ارسال فرم');
                } else {
                    alert('خطا در ارسال فرم');
                }
            }
        });
    });
}

/**
 * ⭐ Get error message from AJAX response
 */
function getAjaxErrorMessage(jqXHR) {
    if (jqXHR.status === 404) return 'صفحه مورد نظر یافت نشد (404)';
    if (jqXHR.status === 500) return 'خطای داخلی سرور (500)';
    if (jqXHR.status === 403) return 'دسترسی مجاز نیست (403)';
    if (jqXHR.status === 401) return 'نیاز به احراز هویت (401)';
    if (jqXHR.status === 0) return 'خطا در ارتباط با سرور';
    if (jqXHR.responseJSON?.message) return jqXHR.responseJSON.message;
    return 'خطا در بارگذاری محتوا';
}

// ========================================
// ⭐⭐⭐ Event Handler for data-toggle="modal-ajax"
// ========================================
$(document).on('click', '[data-toggle="modal-ajax"]', function (event) {
    event.preventDefault();

    const $trigger = $(this);
    const url = $trigger.attr("href") || $trigger.attr("formaction");

    console.log('🎯 [data-toggle="modal-ajax"] clicked:', url);

    // ⭐ استفاده از createAndShowModal
    createAndShowModal(url);
});

// ⭐ Expose globally
window.createAndShowModal = createAndShowModal;
window.loadScheduledTemplates = loadScheduledTemplates;
window.selectTemplate = selectTemplate;

/**
 * Helper function to trigger a modal
 * @param {HTMLElement} input - Element with href attribute
 */
function ModalTrigger(input) {
    event.preventDefault();
    const url = $(input).attr('href');
    createAndShowModal(url);
}

/**
 * Format prices with commas
 */
function convertprices() {
    const prices = document.getElementsByClassName("cprices");

    for (let i = 0; i < prices.length; i++) {
        prices[i].textContent = prices[i].textContent.replace(/,/g, '');
        const price = parseInt(prices[i].textContent).toLocaleString('en');
        prices[i].textContent = price;
    }
}

/**
 * Display response messages in views
 * @param {Object} response - Response object with viewList containing messages
 */
function SendResposeMessageInView(response) {
    $.each(response.viewList, function (index, viewdata) {
        $.each(viewdata.messages, function (index, messagerow) {
            const icons = {
                "success": 'fa fa-check',
                "warning": 'fa fa-exclamation-triangle',
                "info": 'fa fa-info',
                "error": 'fa fa-times'
            };

            const notificationTypes = {
                "success": 'success',
                "warning": 'warning',
                "info": 'info',
                "error": 'error'
            };

            // Determine notification type based on status or default to 'info'
            const notificationType = notificationTypes[messagerow.status] || 'info';

            // Determine icon based on status
            const icon = icons[messagerow.status] || 'fa fa-info';

            Dashmix.helpers('jq-notify', {
                type: notificationType,
                icon: icon + ' me-1', // Add space to separate icon classes
                message: messagerow.text
            });
        });
    });
}

/**
 * Helper function to convert single message to array format
 * @param {string|Object|Array} message - Message in various formats
 * @param {string} status - Default status if not provided
 * @returns {Array} - Array of message objects
 */
function normalizeMessages(message, status = 'info') {
    if (!message) return [];
    
    // اگر قبلاً آرایه است
    if (Array.isArray(message)) {
        return message;
    }
    
    // اگر object با text و status است
    if (typeof message === 'object' && message.text) {
        return [message];
    }
    
    // اگر string است
    if (typeof message === 'string') {
        return [{ status: status, text: message }];
    }
    
    return [];
}

// بروزرسانی تابع موجود
function SendResposeMessage(messages, defaultStatus = 'info') {
    const normalizedMessages = normalizeMessages(messages, defaultStatus);
    
    $.each(normalizedMessages, function (index, messagerow) {
        const icons = {
            "success": 'fa fa-check',
            "warning": 'fa fa-exclamation-triangle',
            "info": 'fa fa-info',
            "error": 'fa fa-times'
        };

        const notificationTypes = {
            "success": 'success',
            "warning": 'warning',
            "info": 'info',
            "error": 'error'
        };

        const notificationType = notificationTypes[messagerow.status] || 'info';
        const icon = icons[messagerow.status] || 'fa fa-info';

        Dashmix.helpers('jq-notify', {
            type: notificationType,
            icon: icon + ' me-1',
            message: messagerow.text
        });
    });
}

/**
 * Copy text to clipboard
 * @param {HTMLElement} Data - Element with url data attribute
 * @returns {Promise} - Promise for clipboard operation
 */
function copyToClipboard(Data) {
    event.preventDefault();
    const $target = $(Data);
    const textToCopy = $target.data('url');

    // navigator clipboard api needs a secure context (https)
    if (navigator.clipboard && window.isSecureContext) {
        // navigator clipboard api method
        return navigator.clipboard.writeText(textToCopy).then(() => {
            Dashmix.helpers('jq-notify', {
                type: 'success',
                icon: 'fa fa-check me-1',
                message: 'متن کپی شد'
            });
        });
    } else {
        // text area method for older browsers
        let textArea = document.createElement("textarea");
        textArea.value = textToCopy;
        // make the textarea out of viewport
        textArea.style.position = "fixed";
        textArea.style.left = "-999999px";
        textArea.style.top = "-999999px";
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        return new Promise((res, rej) => {
            // here the magic happens
            const successful = document.execCommand('copy');
            textArea.remove();

            if (successful) {
                Dashmix.helpers('jq-notify', {
                    type: 'success',
                    icon: 'fa fa-check me-1',
                    message: 'متن کپی شد'
                });
                res();
            } else {
                rej();
            }
        });
    }
}

/**
 * Handle AJAX errors
 * @param {Object} xhr - XHR object
 */
function handleAjaxError(xhr) {
    if (xhr.responseJSON) {
        const errors = xhr.responseJSON;
        for (let key in errors) {
            Dashmix.helpers('jq-notify', {
                type: 'danger',
                icon: 'fa fa-times me-1',
                message: errors[key]
            });
        }
    } else if (xhr.statusText) {
        Dashmix.helpers('jq-notify', {
            type: 'danger',
            icon: 'fa fa-times me-1',
            message: 'Error: ' + xhr.statusText
        });
    }
}

// Event Handlers

// Legacy modal handler
$(document).on('click', '[data-toggle="modal-old"]', function (event) {
    event.preventDefault();

    const $trigger = $(this);
    const targetModalId = $trigger.data('bs-target');

    const modalHtml = `<div aria-labelledby="modal-action" class="modal fade" data-bs-focus="false" role="dialog" id="modal-action" tabindex="-1" aria-hidden="true"></div>`;
    $('body').append(modalHtml);

    const $modal = $(targetModalId);
    let url = $trigger.attr("formaction");

    $.get(url).done(function (data) {
        $modal.html(data);
        // Process URLs in loaded modal content
        ModalUtils.processUrlsInContainer($modal);
        
        // راه‌اندازی Dynamic Select2 در modal
        DynamicSelect2Manager.reinitializeSelect2InDiv($modal);
        
        $modal.modal("show");
    }).fail(function (jqXHR) {
        handleAjaxError(jqXHR);
    });
});

// Modal without auto-remove handler
$(document).on('click', '[data-toggle="show-modal"]', function (event) {
    event.preventDefault();

    const $trigger = $(this);
    const url = $trigger.attr("href") || $trigger.attr("formaction");

    createAndShowModalWithoutRemove(url);
});

// AJAX modal handler
$(document).on('click', '[data-toggle="modal-ajax"]', function (event) {
    event.preventDefault();

    const $trigger = $(this);
    const url = $trigger.attr("href") || $trigger.attr("formaction");

    createAndShowModal(url);
});

// Clean up modals on hide
$(document).on('hidden.bs.modal', '.modal', function () {
    $(this).remove();
});

// ⭐⭐⭐ اصلاح modal-ajax-save برای trigger کردن refresh
$(document).on('click', '[data-save="modal-ajax-save"]', function (event) {
    event.preventDefault();

    const $button = $(this);
    $button.prop('disabled', true);
    const $modal = $button.closest('.modal');

    if (typeof tinymce !== 'undefined') {
        tinymce.triggerSave();
    }

    const $form = $modal.find('form');
    const formData = new FormData($form[0]);

    $('input[type="checkbox"]').each(function () {
        const $checkbox = $(this);
        formData.append($checkbox.attr('id'), $checkbox.prop('checked'));
    });

    $.ajax({
        url: $form.attr("action"),
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            console.log('AJAX Response:', response);

            if (response.status === "redirect") {
                $(location).prop('href', response.redirectUrl);
            } else if (response.status === "update-view") {
                $modal.find('[data-bs-dismiss="modal"]').click();

                if (response.viewList && response.viewList.length > 0) {
                    response.viewList.forEach(function (item) {
                        if (item.elementId && item.view && item.view.result) {
                            const $target = $('#' + item.elementId);
                            if ($target.length > 0) {
                                if (item.appendMode === true) {
                                    console.log('Appending content to:', item.elementId);
                                    $target.append(item.view.result);
                                } else {
                                    console.log('Replacing content in:', item.elementId);
                                    $target.html(item.view.result);
                                }

                                ModalUtils.processUrlsInContainer($target);
                                DynamicSelect2Manager.reinitializeSelect2InDiv($target);
                            }
                        }
                    });

                    // ⭐⭐⭐ بروزرسانی کل صفحه تسک
                    if (typeof TaskDetailsRefreshManager !== 'undefined' && TaskDetailsRefreshManager.isEnabled) {
                        TaskDetailsRefreshManager.triggerRefresh();
                    }

                    $button.trigger('ajaxSuccess', [response]);
                }

            } else if (response.status === "validation-error" || response.status === "error") {
                $button.prop('disabled', false);
            }

            if (response.message) {
                SendResposeMessage(response.message);
            }
        },
        error: function (xhr) {
            $button.prop('disabled', false);
            console.error('AJAX Error:', xhr);
            handleAjaxError(xhr);
        }
    });
});

// Submit a form inside a modal
$(document).on('click', '[data-save="modal"]', function (event) {
    const $button = $(this);
    $button.prop('disabled', true);

    const $form = $button.closest('.modal').find('form');
    $form.submit();
});

// Modal form submission without page reload
$("#modal-action").on('click', '[data-save="modal-noreload"]', function (event) {
    const $button = $(this);
    $button.prop('disabled', true);

    const $form = $button.closest('.modal').find('form');
    const url = $button.data('url');
    const dataform = new FormData($form[0]);


    const $partialElement = $("#partialid");

    $.ajax({
        type: "post",
        url: url,
        data: dataform,
        contentType: false,
        processData: false
    }).done(function (data) {
        $partialElement.empty().html(data);

        // Process URLs in updated content
        ModalUtils.processUrlsInContainer($partialElement);
        
        // راه‌اندازی مجدد Dynamic Select2
        DynamicSelect2Manager.reinitializeSelect2InDiv($partialElement);

        convertprices();
        $button.closest('.modal').find('button[data-bs-dismiss="modal"]').click();
    }).fail(function (xhr) {
        $button.prop('disabled', false);
        handleAjaxError(xhr);
    });
});

// Load partial view via AJAX
$(document).on('click', '[data-toggle="partialview"]', function (event) {
    event.preventDefault();

    const $target = $(this);
    const viewId = $target.data('target');

    const url = ModalUtils.getUrlFromElement($target);


    $.get(urlWithLanguage).done(function (data) {
        const $container = $(viewId);
        $container.html(data);

        // Process URLs in updated content
        ModalUtils.processUrlsInContainer($container);
        
        // راه‌اندازی مجدد Dynamic Select2
        DynamicSelect2Manager.reinitializeSelect2InDiv($container);

        convertprices();
    }).fail(function (xhr) {
        handleAjaxError(xhr);
    });
});

// Submit form to multiple targets
$(document).on('click', '[data-toggle="submitformmultiple"]', function (event) {
    event.preventDefault();

    const $target = $(this);
    const viewClass = $target.data('targetc');
    const $form = $target.closest('form');

    $("." + viewClass).addClass("block-mode-loading");

    const dataform = new FormData($form[0]);


    if (typeof tinymce !== 'undefined') {
        tinymce.triggerSave();
    }

    // Add checkbox states to form data
    $('input[type="checkbox"]').each(function () {
        const $checkbox = $(this);
        dataform.append($checkbox.attr('id'), $checkbox.prop('checked'));
    });

    // Add select values to form data
    $form.find('select').each(function () {
        dataform.append($(this).attr('name'), $(this).val());
    });

    const url = ModalUtils.getUrlFromElement($target);

    $.ajax({
        type: "post",
        url: url,
        data: dataform,
        contentType: false,
        processData: false
    }).done(function (response) {
        $.each(response.viewList, function (index, item) {
            const $container = $("#" + item.elementId);
            $container.html(item.view.result);

            // Process URLs in updated content
            ModalUtils.processUrlsInContainer($container);
            
            // راه‌اندازی مجدد Dynamic Select2
            DynamicSelect2Manager.reinitializeSelect2InDiv($container);
        });

        convertprices();
        $("." + viewClass).removeClass("block-mode-loading");
        SendResposeMessage(response);
    }).fail(function (xhr) {
        $("." + viewClass).removeClass("block-mode-loading");
        handleAjaxError(xhr);
    });
});

// Live text input with partial view update
$(document).on('keyup', 'input[type="text"][data-toggle="partialview"]', function (event) {
    const $target = $(this);
    const viewId = $target.data('target');

    Dashmix.block('state_loading', viewId);

    const formData = new FormData();


    // Get related select elements
    const selects = $target.data('sclass');
    if (typeof selects !== 'undefined') {
        document.querySelectorAll('.' + selects).forEach(select => {
            formData.append(select.name, select.value);
        });
    }

    // Add the input's own value
    formData.append($target.attr('name'), $target.val());

    const url = $target.data('href') || $target.attr("formaction") || $target.closest('form').attr("action");

    $.ajax({
        url: url,
        type: 'post',
        data: formData,
        processData: false,
        contentType: false
    }).done(function (response) {
        const $container = $(viewId);
        $container.html(response);

        // Process URLs in updated content
        ModalUtils.processUrlsInContainer($container);
        
        // راه‌اندازی مجدد Dynamic Select2
        DynamicSelect2Manager.reinitializeSelect2InDiv($container);

        Dashmix.block('state_normal', viewId);
    }).fail(function (xhr) {
        Dashmix.block('state_normal', viewId);
        handleAjaxError(xhr);
    });
});

// Radio button change with partial view update
$(document).on('click', 'input[type="radio"][data-toggle="partialview"]', function (event) {
    const $target = $(this);
    const viewId = $target.data('target');
    const value = $target.val();


    const url = $target.data('href') || $target.attr("formaction") || $target.closest('form').attr("action");

    $.get(url, data).done(function (data) {
        const $container = $(viewId);
        $container.html(data);

        // Process URLs in updated content
        ModalUtils.processUrlsInContainer($container);
        
        // راه‌اندازی مجدد Dynamic Select2
        DynamicSelect2Manager.reinitializeSelect2InDiv($container);
    }).fail(function (xhr) {
        handleAjaxError(xhr);
    });
});

// Submit form to update a specific target
$(document).on('click', '[data-toggle="submitform"]', function (event) {
    event.preventDefault();

    const $target = $(this);
    const viewId = $target.data('target');
    const $form = $target.closest('form');

    Dashmix.block('state_loading', viewId);

    if (typeof tinymce !== 'undefined') {
        tinymce.triggerSave();
    }

    const dataform = new FormData($form[0]);


    // Add checkbox states to form data
    $('input[type="checkbox"]').each(function () {
        const $checkbox = $(this);
        dataform.append($checkbox.attr('id'), $checkbox.prop('checked'));
    });

    const url = ModalUtils.getUrlFromElement($target);

    $.ajax({
        type: "post",
        url: url,
        data: dataform,
        contentType: false,
        processData: false
    }).done(function (data) {
        const $container = $(viewId);
        $container.html(data);

        // Process URLs in updated content
        ModalUtils.processUrlsInContainer($container);
        
        // راه‌اندازی مجدد Dynamic Select2
        DynamicSelect2Manager.reinitializeSelect2InDiv($container);

        convertprices();
        Dashmix.block('state_normal', viewId);
    }).fail(function (xhr) {
        Dashmix.block('state_normal', viewId);
        handleAjaxError(xhr);
    });
});

// SweetAlert confirmation
$(document).on('click', '[data-toggle="swal-asp"]', function (event) {
    event.preventDefault();

    const $target = $(this);
    const viewId = $target.data('target');
    const title = $target.data('title');
    const description = $target.data('desc');
    let href = $target.attr('href');

    Dashmix.block('state_loading', viewId);


    const swalWithBootstrapButtons = Swal.mixin({
        customClass: {
            confirmButton: "btn btn-success",
            cancelButton: "btn btn-danger"
        },
        buttonsStyling: false
    });

    swalWithBootstrapButtons.fire({
        title: title,
        text: description,
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "تایید",
        cancelButtonText: "لغو",
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $(location).prop('href', href);
        } else {
            Dashmix.block('state_normal', viewId);
        }
    });
});

// از کد قدیمی برای سازگاری با کدهای موجود باقی می‌ماند
// Multiple partial view update handler - قابلیت بروزرسانی چندین partial view با یک تغییر
$(document).on('change', 'select[data-toggle="multiplepartialview"]', function (event) {
    // این event handler اکنون توسط DynamicSelect2Manager مدیریت می‌شود
    // اما برای سازگاری با کدهای موجود باقی می‌ماند
    console.log('Legacy multiplepartialview handler called - redirecting to DynamicSelect2Manager');
});

// Initialize on page load
document.addEventListener("DOMContentLoaded", function () {
    // Process all URLs in the document on load
    ModalUtils.processUrlsInContainer(document);
    
    // راه‌اندازی Dynamic Select2 Manager
    DynamicSelect2Manager.init();
});

// jQuery ready handler برای سازگاری
$(document).ready(function() {
    // راه‌اندازی مجدد Dynamic Select2 Manager در صورت نیاز
    if (!DynamicSelect2Manager.isInitialized) {
        DynamicSelect2Manager.init();
    }
});

// Expose DynamicSelect2Manager globally for external access
window.DynamicSelect2Manager = DynamicSelect2Manager;


/**
* نمایش مودال تایید SweetAlert با قابلیت سفارشی‌سازی کامل
* @param {Object} options - تنظیمات مودال تایید
* @returns {Promise<boolean>} - true اگر کاربر تایید کند، false در غیر این صورت
*/
async function showConfirmationSwal(options = {}) {
    const defaultOptions = {
        title: 'آیا اطمینان دارید؟',
        text: 'این عملیات قابل بازگشت نیست',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'بله، ادامه بده',
        cancelButtonText: 'خیر، انصراف',
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        reverseButtons: true,
        focusCancel: true,
        customClass: {
            confirmButton: 'btn btn-success',
            cancelButton: 'btn btn-danger'
        },
        buttonsStyling: false
    };

    const mergedOptions = { ...defaultOptions, ...options };

    try {
        const result = await Swal.fire(mergedOptions);
        return result.isConfirmed;
    } catch (error) {
        console.error('خطا در نمایش SweetAlert:', error);
        return false;
    }
}

/**
 * نمایش مودال تایید ساده برای حذف
 * @param {string} itemName - نام آیتم برای حذف
 * @returns {Promise<boolean>}
 */
async function showDeleteConfirmation(itemName = 'این آیتم') {
    return await showConfirmationSwal({
        title: 'حذف ' + itemName,
        text: 'آیا از حذف این مورد اطمینان دارید؟',
        icon: 'warning',
        confirmButtonText: 'بله، حذف شود',
        cancelButtonText: 'خیر، انصراف',
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d'
    });
}

/**
 * نمایش پیام موفقیت SweetAlert
 * @param {string} title - عنوان پیام
 * @param {string} text - متن پیام
 */
function showSuccessSwal(title = 'موفق!', text = 'عملیات با موفقیت انجام شد') {
    Swal.fire({
        title: title,
        text: text,
        icon: 'success',
        confirmButtonText: 'باشه',
        customClass: {
            confirmButton: 'btn btn-success'
        },
        buttonsStyling: false
    });
}

/**
 * نمایش پیام خطا SweetAlert
 * @param {string} title - عنوان خطا
 * @param {string} text - متن خطا
 */
function showErrorSwal(title = 'خطا!', text = 'عملیات با خطا مواجه شد') {
    Swal.fire({
        title: title,
        text: text,
        icon: 'error',
        confirmButtonText: 'باشه',
        customClass: {
            confirmButton: 'btn btn-danger'
        },
        buttonsStyling: false
    });
}

/**
 * نمایش مودال تایید با input برای یادداشت
 * @param {Object} options - تنظیمات مودال
 * @returns {Promise<{confirmed: boolean, value: string|null}>}
 */
async function showConfirmationWithInput(options = {}) {
    const defaultOptions = {
        title: 'تایید عملیات',
        text: 'لطفاً یادداشت خود را وارد کنید',
        icon: 'question',
        input: 'textarea',
        inputPlaceholder: 'یادداشت (اختیاری)',
        showCancelButton: true,
        confirmButtonText: 'تایید',
        cancelButtonText: 'انصراف',
        customClass: {
            confirmButton: 'btn btn-success',
            cancelButton: 'btn btn-danger'
        },
        buttonsStyling: false,
        inputValidator: (value) => {
            // اعتبارسنجی اختیاری
            return null;
        }
    };

    const mergedOptions = { ...defaultOptions, ...options };

    try {
        const result = await Swal.fire(mergedOptions);
        return {
            confirmed: result.isConfirmed,
            value: result.value || null
        };
    } catch (error) {
        console.error('خطا در نمایش SweetAlert با input:', error);
        return { confirmed: false, value: null };
    }
}

// ✅ Expose به window برای استفاده global
window.showConfirmationSwal = showConfirmationSwal;
window.showDeleteConfirmation = showDeleteConfirmation;
window.showSuccessSwal = showSuccessSwal;
window.showErrorSwal = showErrorSwal;
window.showConfirmationWithInput = showConfirmationWithInput;

// ========================================
// ⭐ Notification Helper
// ========================================
const NotificationHelper = {
    success: function (message) {
        Dashmix.helpers('jq-notify', {
            type: 'success',
            icon: 'fa fa-check me-1',
            message: message
        });
    },
    error: function (message) {
        Dashmix.helpers('jq-notify', {
            type: 'danger',
            icon: 'fa fa-times me-1',
            message: message
        });
    },
    warning: function (message) {
        Dashmix.helpers('jq-notify', {
            type: 'warning',
            icon: 'fa fa-exclamation-triangle me-1',
            message: message
        });
    },
    info: function (message) {
        Dashmix.helpers('jq-notify', {
            type: 'info',
            icon: 'fa fa-info me-1',
            message: message
        });
    }
};

// ========================================
// ⭐ Coming Soon Feature Notification
// ========================================
/**
 * نمایش پیغام "به‌زودی" برای قابلیت‌های در حال توسعه
 * @param {Event} event - رویداد کلیک (برای جلوگیری از رفتار پیش‌فرض)
 * @param {string} featureName - نام قابلیت (برای نمایش در پیام)
 */
function showComingSoonNotification(event, featureName) {
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }

    const message = featureName
        ? `قابلیت "${featureName}" به‌زودی اضافه خواهد شد`
        : 'این قابلیت به‌زودی اضافه خواهد شد';

    NotificationHelper.info(message);
}
function updateMultipleViews(viewList) {
    if (!Array.isArray(viewList) || viewList.length === 0) {
        console.warn('updateMultipleViews: viewList is empty or invalid');
        return;
    }

    viewList.forEach(function (item) {
        if (!item || !item.elementId) {
            console.warn('updateMultipleViews: Invalid item', item);
            return;
        }

        const $target = $('#' + item.elementId);

        if ($target.length === 0) {
            console.warn(`updateMultipleViews: Element #${item.elementId} not found`);
            return;
        }

        if (item.view && item.view.result) {
            // بروزرسانی محتوا
            if (item.appendMode === true) {
                console.log('Appending content to:', item.elementId);
                $target.append(item.view.result);
            } else {
                console.log('Replacing content in:', item.elementId);
                $target.html(item.view.result);
            }

            // پردازش URL ها
            ModalUtils.processUrlsInContainer($target);

            // راه‌اندازی مجدد Select2
            DynamicSelect2Manager.reinitializeSelect2InDiv($target);
        } else {
            console.warn(`updateMultipleViews: No view.result for element #${item.elementId}`);
        }
    });
}

