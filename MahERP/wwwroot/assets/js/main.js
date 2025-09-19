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
 * Create and show a modal that removes itself on close
 * @param {string} url - URL to load content from
 */
function createAndShowModal(url) {
    const uniqueModalId = 'modal-' + Date.now();
    const modalHtml = `<div aria-labelledby="${uniqueModalId}" class="modal fade" data-bs-focus="false" role="dialog" id="${uniqueModalId}" tabindex="-1" aria-hidden="true"></div>`;

    $('body').append(modalHtml);
    const $modal = $('#' + uniqueModalId);

    // Load content into the modal via AJAX
    $.get(url).done(function (data) {
        try {
            // Check if data is valid
            if (!data) {
                throw new Error('محتوای مودال خالی است');
            }

            // Handle different response types
            let modalContent = '';
            if (typeof data === 'object' && data.status === 'error') {
                // Server returned JSON error
                throw new Error(data.message || 'خطا در بارگذاری محتوا');
            } else if (typeof data === 'string') {
                modalContent = data;
            } else if (typeof data === 'object') {
                // Check if it's JSON response with error
                if (data.status && data.status === 'error') {
                    throw new Error(data.message || 'خطا در سرور');
                }
                modalContent = JSON.stringify(data);
            } else {
                modalContent = String(data);
            }

            // Validate content is not empty
            if (!modalContent || modalContent.trim() === '' || modalContent === '{}') {
                throw new Error('محتوای مودال خالی یا نامعتبر است');
            }

            $modal.html(modalContent);
            $modal.find('.js-select2').attr('data-container', '#' + uniqueModalId);

            // Process URLs in loaded modal content
            ModalUtils.processUrlsInContainer($modal);

            // راه‌اندازی Dynamic Select2 در modal
            DynamicSelect2Manager.reinitializeSelect2InDiv($modal);

            // Show modal with error handling
            try {
                $modal.modal('show');
            } catch (modalError) {
                throw new Error('خطا در نمایش مودال: ' + modalError.message);
            }

        } catch (processError) {
            console.error("Error processing modal content:", processError);

            // Remove the modal element
            $modal.remove();

            // Show user-friendly error
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'خطا در بارگذاری محتوا',
                    text: processError.message || 'خطای نامشخص در بارگذاری محتوای مودال',
                    icon: 'error',
                    confirmButtonText: 'باشه'
                });
            } else {
                alert('خطا در بارگذاری محتوا: ' + (processError.message || 'خطای نامشخص'));
            }
        }

    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("Failed to load modal content:", {
            status: jqXHR.status,
            statusText: jqXHR.statusText,
            textStatus: textStatus,
            errorThrown: errorThrown,
            responseText: jqXHR.responseText
        });

        // Remove the modal element
        $modal.remove();

        // Determine error message
        let errorMessage = 'خطا در بارگذاری محتوای مودال';

        if (jqXHR.status === 404) {
            errorMessage = 'صفحه مورد نظر یافت نشد (404)';
        } else if (jqXHR.status === 500) {
            errorMessage = 'خطای داخلی سرور (500)';
        } else if (jqXHR.status === 403) {
            errorMessage = 'دسترسی مجاز نیست (403)';
        } else if (jqXHR.status === 401) {
            errorMessage = 'نیاز به احراز هویت (401)';
        } else if (jqXHR.status === 0) {
            errorMessage = 'خطا در ارتباط با سرور (شبکه قطع است)';
        } else if (jqXHR.responseJSON && jqXHR.responseJSON.message) {
            errorMessage = jqXHR.responseJSON.message;
        } else if (jqXHR.responseText) {
            // Try to extract meaningful error from HTML response
            const htmlError = $(jqXHR.responseText).find('title').text() ||
                $(jqXHR.responseText).find('h1').first().text() ||
                jqXHR.statusText;
            if (htmlError && htmlError.trim() !== '') {
                errorMessage = htmlError;
            }
        }

        // Show user-friendly error
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'خطا در بارگذاری',
                text: errorMessage,
                icon: 'error',
                confirmButtonText: 'باشه',
                footer: `کد خطا: ${jqXHR.status || 'نامشخص'}`
            });
        } else {
            alert(`خطا در بارگذاری: ${errorMessage}\nکد خطا: ${jqXHR.status || 'نامشخص'}`);
        }

        // Call the global error handler if it exists
        if (typeof handleAjaxError === 'function') {
            handleAjaxError(jqXHR);
        }
    });

    // Handle modal hidden event to remove it from the DOM
    $modal.on('hidden.bs.modal', function () {
        $(this).remove();
    });

    // Handle modal show errors
    $modal.on('show.bs.modal', function (e) {
        console.log('Modal about to show:', uniqueModalId);
    });

    $modal.on('shown.bs.modal', function (e) {
        console.log('Modal successfully shown:', uniqueModalId);
    });

    // Handle modal show failure
    $modal.on('show.bs.modal', function (e) {
        if (!$(this).find('.modal-dialog').length) {
            console.error('Modal content is not properly structured');
            e.preventDefault();
            $(this).remove();

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'خطا در ساختار مودال',
                    text: 'محتوای مودال به درستی بارگذاری نشده است',
                    icon: 'error',
                    confirmButtonText: 'باشه'
                });
            }
        }
    });
}
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
 * Display response messages
 * @param {Object} messages - Array of message objects
 */
function SendResposeMessage(messages) {
    if (!messages) return;

    $.each(messages, function (index, messagerow) {
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

// Handle AJAX form submission inside modals
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

    // Add checkbox values to form data
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
            if (response.status === "redirect") {
                $(location).prop('href', response.redirectUrl);
            } else if (response.status === "update-view") {
                $.each(response.viewList, function (index, item) {
                    const $container = $("#" + item.elementId);
                    $container.html(item.view.result);
                    // Process URLs in updated content
                    ModalUtils.processUrlsInContainer($container);
                    
                    // راه‌اندازی مجدد Dynamic Select2
                    DynamicSelect2Manager.reinitializeSelect2InDiv($container);
                });
                $modal.find('[data-bs-dismiss="modal"]').click();
            } else if (response.status === "temp-save") {
                $.each(response.viewList, function (index, item) {
                    const $container = $("#" + item.elementId);
                    $container.html(item.view.result);
                    // Process URLs in updated content
                    ModalUtils.processUrlsInContainer($container);
                    
                    // راه‌اندازی مجدد Dynamic Select2
                    DynamicSelect2Manager.reinitializeSelect2InDiv($container);
                });
                SendResposeMessageInView(response);
            }

            if (response.message) {
                SendResposeMessage(response.message);
            }
        },
        error: function (xhr) {
            $button.prop('disabled', false);
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
