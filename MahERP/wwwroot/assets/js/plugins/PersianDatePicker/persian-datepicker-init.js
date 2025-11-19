/**
 * Persian DatePicker Initializer
 * این فایل مسئول initialize کردن تمام Persian DatePicker ها در پروژه است
 * ⭐⭐⭐ با پشتیبانی کامل موبایل و دسکتاپ
 */

(function() {
    'use strict';

    // ⭐⭐⭐ تشخیص دستگاه موبایل
    function isMobileDevice() {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ||
               (window.innerWidth <= 768);
    }

    // ⭐⭐⭐ تابع سراسری برای Initialize کردن Persian DatePicker
    window.initPersianDatePicker = function(container) {
        var $container = container ? $(container) : $(document);

        // بررسی وجود کتابخانه mds.MdsPersianDateTimePicker
        if (typeof mds === 'undefined' || typeof mds.MdsPersianDateTimePicker === 'undefined') {
            console.warn('⚠️ Persian DateTimePicker (mds) not loaded');
            return;
        }

        // پیدا کردن تمام المان‌هایی که باید Persian DatePicker داشته باشند
        $container.find('.js-flatpickr:not(.persian-picker-initialized), input[data-persian-picker]:not(.persian-picker-initialized)').each(function() {
            var $elem = $(this);

            try {
                // ⭐⭐⭐ غیرفعال کردن native date picker در موبایل
                $elem.attr('type', 'text');
                $elem.attr('inputmode', 'none'); // جلوگیری از باز شدن کیبورد در موبایل
                $elem.attr('readonly', 'readonly'); // جلوگیری از ویرایش دستی

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
                    
                    // ⭐⭐⭐ تنظیمات موبایل
                    showTodayButton: true,
                    modalMode: isMobileDevice(), // در موبایل به صورت modal
                    placement: isMobileDevice() ? 'center' : 'bottom', // موبایل: وسط، دسکتاپ: پایین
                    
                    // تنظیمات Calendar
                    calendar: {
                        persian: {
                            locale: 'fa',
                            showHint: true,
                            leapYearMode: 'algorithmic'
                        }
                    }
                };

                // اگر زمان فعال باشد، فرمت را تغییر بده
                if (config.enableTimePicker) {
                    config.textFormat = 'yyyy/MM/dd HH:mm';
                }

                // ⭐⭐⭐ Initialize Persian DateTimePicker
                new mds.MdsPersianDateTimePicker($elem[0], config);

                // علامت‌گذاری به عنوان initialized
                $elem.addClass('persian-picker-initialized');

                console.log('✅ Persian DatePicker initialized:', $elem.attr('name') || $elem.attr('id'));
            } catch (error) {
                console.error('❌ Error initializing Persian DatePicker:', error);
            }
        });
    };

    // Initialize در زمان بارگذاری صفحه
    $(document).ready(function() {
        console.log('📅 Initializing Persian DatePickers...');
        console.log('📱 Device type:', isMobileDevice() ? 'Mobile' : 'Desktop');
        initPersianDatePicker();
    });

    // ⭐⭐⭐ Global event handler برای محتوای AJAX loaded (مودال‌ها)
    $(document).on('shown.bs.modal', '.modal', function() {
        console.log('📅 Modal opened - Re-initializing Persian DatePickers...');
        // کمی تاخیر برای اطمینان از بارگذاری کامل محتوا
        setTimeout(function() {
            initPersianDatePicker();
        }, 200);
    });

    // ⭐⭐⭐ برای مودال‌های AJAX که با modal-ajax-helper باز می‌شوند
    $(document).on('modal-ajax-loaded', function(event, modalId) {
        console.log('📅 AJAX Modal loaded - Re-initializing Persian DatePickers...');
        setTimeout(function() {
            var $modal = $('#' + modalId);
            if ($modal.length) {
                initPersianDatePicker($modal);
            } else {
                initPersianDatePicker();
            }
        }, 100);
    });

    // Global event برای DOMContentLoaded و ajaxComplete
    $(document).on('DOMContentLoaded ajaxComplete', function() {
        setTimeout(function() {
            initPersianDatePicker();
        }, 100);
    });

})();
