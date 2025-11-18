/**
 * Persian DatePicker Initializer
 * این فایل مسئول initialize کردن تمام Persian DatePicker ها در پروژه است
 */

(function() {
    'use strict';

    // تابع سراسری برای Initialize کردن Persian DatePicker
    window.initPersianDatePicker = function(container) {
        var $container = container ? $(container) : $(document);

        // بررسی وجود کتابخانه Persian DatePicker
        if (typeof $.fn.persianDatepicker === 'undefined') {
            console.warn('⚠️ Persian DatePicker plugin not loaded');
            return;
        }

        // پیدا کردن تمام المان‌هایی که باید Persian DatePicker داشته باشند
        $container.find('.persian-datepicker:not(.hasDatepicker)').each(function() {
            var $elem = $(this);

            try {
                // تنظیمات پیش‌فرض
                var config = {
                    format: 'YYYY/MM/DD',
                    autoClose: true,
                    initialValue: false,
                    observer: true,
                    calendar: {
                        persian: {
                            locale: 'fa'
                        }
                    },
                    // تنظیمات اضافی از data attributes
                    minDate: $elem.data('min-date') || null,
                    maxDate: $elem.data('max-date') || null,
                    altField: $elem.data('alt-field') || null,
                    altFormat: $elem.data('alt-format') || 'YYYY/MM/DD'
                };

                // Initialize Persian DatePicker
                $elem.persianDatepicker(config);

                console.log('✅ Persian DatePicker initialized:', $elem.attr('name') || $elem.attr('id'));
            } catch (error) {
                console.error('❌ Error initializing Persian DatePicker:', error);
            }
        });
    };

    // Initialize در زمان بارگذاری صفحه
    $(document).ready(function() {
        initPersianDatePicker();
    });

    // Global event handler برای AJAX loaded content
    $(document).on('DOMContentLoaded ajaxComplete', function() {
        // کمی تاخیر برای اطمینان از بارگذاری کامل محتوا
        setTimeout(function() {
            initPersianDatePicker();
        }, 100);
    });

})();
