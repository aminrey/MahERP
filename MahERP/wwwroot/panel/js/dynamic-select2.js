/**
 * Dynamic Select2 Handler
 * سیستم پویا برای مدیریت Select2 ها با قابلیت بروزرسانی خودکار
 */
(function($) {
    'use strict';

    // تنظیمات پیش‌فرض
    var defaultSettings = {
        ajaxMethod: 'POST',
        loadingText: '<div class="text-center p-3"><i class="fa fa-spinner fa-spin"></i> در حال بارگذاری...</div>',
        errorText: '<div class="alert alert-danger">خطا در بارگذاری اطلاعات</div>',
        emptyText: 'ابتدا گزینه مناسب را انتخاب کنید',
        select2Config: {
            allowClear: true,
            width: '100%'
        }
    };

    // کلاس اصلی DynamicSelect2
    var DynamicSelect2 = function() {
        this.init();
    };

    DynamicSelect2.prototype = {
        
        init: function() {
            var self = this;
            
            // اجرای اولیه Select2 برای تمام عناصر
            this.initializeSelect2Elements();
            
            // نصب event listener برای تغییرات
            this.setupEventListeners();
            
            console.log('Dynamic Select2 System initialized');
        },

        initializeSelect2Elements: function() {
            var self = this;
            
            $('.js-select2').each(function() {
                var $element = $(this);
                
                // بررسی که آیا قبلاً init شده یا نه
                if (!$element.hasClass('select2-hidden-accessible')) {
                    var container = $element.data('container') || 'body';
                    var config = $.extend({}, defaultSettings.select2Config);
                    
                    if (container !== 'body') {
                        config.dropdownParent = $(container);
                    }
                    
                    $element.select2(config);
                }
            });
        },

        setupEventListeners: function() {
            var self = this;
            
            // Event delegation برای عناصر select با data-toggle="multiplepartialview"
            $(document).on('change', 'select[data-toggle="multiplepartialview"]', function(e) {
                self.handleSelectChange($(this));
            });
        },

        handleSelectChange: function($select) {
            var self = this;
            var url = $select.data('url');
            var targetDivs = $select.data('target-divs');
            var selectedValue = $select.val();
            
            if (!url) {
                console.warn('No URL specified for dynamic select:', $select);
                return;
            }

            // تعیین target divs
            var targets = [];
            if (targetDivs) {
                targets = targetDivs.split(',').map(function(div) {
                    return div.trim();
                });
            } else {
                // اگر target-divs مشخص نشده، از نام کلاس تشخیص بده
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

        detectTargetDivs: function($select) {
            // بر اساس کلاس‌های رایج target div ها را تشخیص بده
            var commonTargets = ['UsersDiv', 'CarbonCopyUsersDiv', 'CustomersDiv'];
            var detectedTargets = [];
            
            commonTargets.forEach(function(target) {
                if ($('#' + target).length > 0) {
                    detectedTargets.push(target);
                }
            });
            
            return detectedTargets;
        },

        clearTargetDivs: function(targets) {
            var self = this;
            
            targets.forEach(function(targetId) {
                var $target = $('#' + targetId);
                if ($target.length) {
                    // ایجاد select خالی با placeholder مناسب
                    var emptySelect = self.createEmptySelect(targetId);
                    $target.html(emptySelect);
                    
                    // راه‌اندازی مجدد Select2
                    self.reinitializeSelect2InDiv($target);
                }
            });
        },

        createEmptySelect: function(targetId) {
            // تعیین نام و id بر اساس targetId
            var selectConfig = this.getSelectConfigByTargetId(targetId);
            
            return `<select class="js-select2 form-select" 
                            id="${selectConfig.id}" 
                            name="${selectConfig.name}" 
                            data-placeholder="${defaultSettings.emptyText}" 
                            ${selectConfig.multiple ? 'multiple="multiple"' : ''}
                            style="width: 100%;">
                    </select>`;
        },

        getSelectConfigByTargetId: function(targetId) {
            var configs = {
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
                }
            };
            
            return configs[targetId] || {
                id: targetId.replace('Div', ''),
                name: targetId.replace('Div', ''),
                multiple: false
            };
        },

        sendAjaxRequest: function($select, url, selectedValue, targets) {
            var self = this;
            
            // نمایش loading در target div ها
            this.showLoadingInTargets(targets);
            
            // تعیین نام پارامتر بر اساس کلاس select
            var paramName = this.getParamNameFromSelect($select);
            var ajaxData = {};
            ajaxData[paramName] = selectedValue;
            
            // اضافه کردن CSRF token
            var token = $('input[name="__RequestVerificationToken"]').val();
            if (token) {
                ajaxData.__RequestVerificationToken = token;
            }

            $.ajax({
                url: url,
                type: defaultSettings.ajaxMethod,
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

        getParamNameFromSelect: function($select) {
            // بر اساس کلاس‌های select تشخیص بده که چه پارامتری باید ارسال شود
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

        showLoadingInTargets: function(targets) {
            var self = this;
            
            targets.forEach(function(targetId) {
                var $target = $('#' + targetId);
                if ($target.length) {
                    $target.html(defaultSettings.loadingText);
                }
            });
        },

        handleAjaxSuccess: function(response, targets) {
            var self = this;
            
            if (response.status === "update-view" && response.viewList) {
                response.viewList.forEach(function(item) {
                    if (targets.includes(item.elementId)) {
                        var $target = $('#' + item.elementId);
                        if ($target.length && item.view && item.view.result) {
                            $target.html(item.view.result);
                            self.reinitializeSelect2InDiv($target);
                        }
                    }
                });
            } else {
                console.warn('Unexpected response format:', response);
                this.handleAjaxError('نامشخص', targets);
            }
        },

        handleAjaxError: function(error, targets) {
            var self = this;
            
            targets.forEach(function(targetId) {
                var $target = $('#' + targetId);
                if ($target.length) {
                    $target.html(defaultSettings.errorText + ': ' + error);
                }
            });
        },

        reinitializeSelect2InDiv: function($container) {
            var self = this;
            
            // کمی صبر کن تا DOM آپدیت شود
            setTimeout(function() {
                $container.find('.js-select2').each(function() {
                    var $element = $(this);
                    
                    // اگر قبلاً Select2 بوده، destroy کن
                    if ($element.hasClass('select2-hidden-accessible')) {
                        $element.select2('destroy');
                    }
                    
                    // راه‌اندازی مجدد
                    var containerAttr = $element.data('container') || 'body';
                    var config = $.extend({}, defaultSettings.select2Config);
                    
                    if (containerAttr !== 'body') {
                        config.dropdownParent = $(containerAttr);
                    }
                    
                    $element.select2(config);
                });
                
                console.log('Select2 reinitialized in container');
            }, 100);
        },

        // متد عمومی برای add کردن کانفیگ جدید
        addSelectConfig: function(targetId, config) {
            // امکان اضافه کردن کانفیگ جدید در runtime
            if (typeof this.customConfigs === 'undefined') {
                this.customConfigs = {};
            }
            this.customConfigs[targetId] = config;
        },

        // متد برای تغییر تنظیمات پیش‌فرض
        updateSettings: function(newSettings) {
            $.extend(defaultSettings, newSettings);
        }
    };

    // ایجاد instance و اتصال به jQuery
    var dynamicSelect2Instance = new DynamicSelect2();
    
    // اتصال به window برای دسترسی global
    window.DynamicSelect2 = dynamicSelect2Instance;
    
    // متد jQuery برای راحتی
    $.fn.dynamicSelect2 = function(method, options) {
        if (method === 'reinit') {
            dynamicSelect2Instance.initializeSelect2Elements();
        } else if (method === 'config') {
            dynamicSelect2Instance.updateSettings(options);
        }
        return this;
    };

})(jQuery);