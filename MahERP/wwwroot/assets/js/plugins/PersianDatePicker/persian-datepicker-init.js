/**
 * Persian DatePicker Initializer
 * این فایل مسئول initialize کردن تمام Persian DatePicker ها در پروژه است
 * ⭐⭐⭐ با پشتیبانی کامل موبایل و دسکتاپ
 * ⭐⭐⭐ با پشتیبانی صحیح از initialValueType
 * ⭐⭐⭐ با retry mechanism برای مودال‌های AJAX
 */

(function() {
    'use strict';

    // ⭐⭐⭐ تشخیص دستگاه موبایل
    function isMobileDevice() {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ||
               (window.innerWidth <= 768);
    }

    // ⭐⭐⭐ تابع کمکی برای بررسی بارگذاری کتابخانه با retry
    function waitForLibrary(callback, maxRetries, retryDelay) {
        if (maxRetries === void 0) { maxRetries = 10; }
        if (retryDelay === void 0) { retryDelay = 100; }
        
        var attempts = 0;
        
        function checkLibrary() {
            attempts++;
            
            if (typeof mds !== 'undefined' && typeof mds.MdsPersianDateTimePicker !== 'undefined') {
                console.log('✅ Persian DatePicker library loaded (attempt ' + attempts + ')');
                callback();
                return true;
            }
            
            if (attempts < maxRetries) {
                console.log('⏳ Waiting for Persian DatePicker library... (attempt ' + attempts + ')');
                setTimeout(checkLibrary, retryDelay);
                return false;
            }
            
            console.error('❌ Persian DatePicker library failed to load after ' + maxRetries + ' attempts');
            return false;
        }
        
        checkLibrary();
    }

    // ⭐⭐⭐ تابع سراسری برای Initialize کردن Persian DatePicker
    window.initPersianDatePicker = function(container, forceRetry) {
        if (forceRetry === void 0) { forceRetry = false; }
        
        // ⭐⭐⭐ اگر forceRetry فعال باشد، صبر کن تا کتابخانه لود شود
        if (forceRetry) {
            waitForLibrary(function() {
                performInitialization(container);
            });
            return;
        }
        
        // ⭐⭐⭐ بررسی فوری وجود کتابخانه
        if (typeof mds === 'undefined' || typeof mds.MdsPersianDateTimePicker === 'undefined') {
            console.warn('⚠️ Persian DateTimePicker (mds) not loaded, retrying...');
            // یک بار دیگر با retry
            waitForLibrary(function() {
                performInitialization(container);
            });
            return;
        }
        
        performInitialization(container);
    };

    // ⭐⭐⭐ تابع اصلی initialization
    function performInitialization(container) {
        var $container = container ? $(container) : $(document);

        // پیدا کردن تمام المان‌هایی که باید Persian DatePicker داشته باشند
        $container.find('.js-flatpickr:not(.persian-picker-initialized), input[data-persian-picker]:not(.persian-picker-initialized), .persian-datepicker:not(.persian-picker-initialized)').each(function() {
            var $elem = $(this);

            try {
                // ⭐⭐⭐ غیرفعال کردن native date picker در موبایل
                $elem.attr('type', 'text');
                $elem.attr('inputmode', 'none');
                $elem.attr('readonly', 'readonly');

                // ⭐⭐⭐ تشخیص نوع مقدار اولیه
                var currentValue = $elem.val();
                var initialValueType = 'persian'; // پیش‌فرض: شمسی
                
                if (currentValue) {
                    if (currentValue.includes('-')) {
                        var year = parseInt(currentValue.split('-')[0]);
                        if (year > 1500) {
                            initialValueType = 'gregorian';
                        } else {
                            initialValueType = 'persian';
                        }
                    } else if (currentValue.includes('/')) {
                        var yearSlash = parseInt(currentValue.split('/')[0]);
                        if (yearSlash > 1500) {
                            initialValueType = 'gregorian';
                        } else {
                            initialValueType = 'persian';
                        }
                    }
                }

                // ⭐⭐⭐ اگر data-initial-value-type تنظیم شده باشد، از آن استفاده کن
                if ($elem.data('initial-value-type')) {
                    initialValueType = $elem.data('initial-value-type');
                }

                // تنظیمات پیش‌فرض
                var config = {
                    targetTextSelector: $elem[0],
                    targetDateSelector: $elem.data('alt-field') || null,
                    englishNumber: false,
                    textFormat: $elem.data('format') || 'yyyy/MM/dd',
                    enableTimePicker: $elem.data('enable-time') === true,
                    disableBeforeToday: $elem.data('disable-before-today') === true,
                    disableAfterToday: $elem.data('disable-after-today') === true,
                    groupId: $elem.attr('id') || 'picker_' + Math.random().toString(36).substr(2, 9),
                    initialValueType: initialValueType,
                    initialValue: currentValue ? true : false,
                    showTodayButton: true,
                    modalMode: isMobileDevice(),
                    placement: isMobileDevice() ? 'center' : 'bottom',
                    calendar: {
                        persian: {
                            locale: 'fa',
                            showHint: true,
                            leapYearMode: 'algorithmic'
                        }
                    }
                };

                // ⭐⭐⭐ اگر targetDateSelector تنظیم شده باشد
                if (config.targetDateSelector) {
                    config.targetDateFormat = 'yyyy-MM-dd';
                }

                // اگر زمان فعال باشد
                if (config.enableTimePicker) {
                    config.textFormat = 'yyyy/MM/dd HH:mm';
                }

                // ⭐⭐⭐ Initialize Persian DateTimePicker
                new mds.MdsPersianDateTimePicker($elem[0], config);

                // علامت‌گذاری به عنوان initialized
                $elem.addClass('persian-picker-initialized');

                console.log('✅ Persian DatePicker initialized:', $elem.attr('name') || $elem.attr('id'), '- Type:', initialValueType);
            } catch (error) {
                console.error('❌ Error initializing Persian DatePicker:', error);
            }
        });
    }

    // Initialize در زمان بارگذاری صفحه
    $(document).ready(function() {
        console.log('📅 Initializing Persian DatePickers...');
        console.log('📱 Device type:', isMobileDevice() ? 'Mobile' : 'Desktop');
        
        // ⭐⭐⭐ استفاده از waitForLibrary برای صفحه اصلی
        waitForLibrary(function() {
            initPersianDatePicker();
        });
    });

    // ⭐⭐⭐ Global event handler برای محتوای AJAX loaded (مودال‌ها)
    $(document).on('shown.bs.modal', '.modal', function() {
        console.log('📅 Modal opened - Re-initializing Persian DatePickers...');
        setTimeout(function() {
            // ⭐⭐⭐ استفاده از forceRetry=true برای مودال‌ها
            initPersianDatePicker(null, true);
        }, 300); // ⭐⭐⭐ افزایش تاخیر به 300ms
    });

    // ⭐⭐⭐ برای مودال‌های AJAX که با modal-ajax-helper باز می‌شوند
    $(document).on('modal-ajax-loaded', function(event, modalId) {
        console.log('📅 AJAX Modal loaded - Re-initializing Persian DatePickers...');
        setTimeout(function() {
            var $modal = $('#' + modalId);
            if ($modal.length) {
                // ⭐⭐⭐ استفاده از forceRetry=true
                initPersianDatePicker($modal, true);
            } else {
                initPersianDatePicker(null, true);
            }
        }, 200); // ⭐⭐⭐ افزایش تاخیر
    });

    // Global event برای DOMContentLoaded و ajaxComplete
    $(document).on('DOMContentLoaded ajaxComplete', function() {
        setTimeout(function() {
            // ⭐⭐⭐ استفاده از forceRetry=true
            initPersianDatePicker(null, true);
        }, 150);
    });

})();
