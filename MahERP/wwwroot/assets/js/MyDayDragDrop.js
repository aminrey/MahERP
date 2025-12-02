/**
 * ⭐⭐⭐ سیستم Drag & Drop حرفه‌ای برای "روز من"
 * نسخه بهینه شده با عملکرد روان
 */

(function ($) {
    'use strict';

    // ========================================
    // 🎯 تنظیمات اولیه
    // ========================================
    const MyDayDragDrop = {
        // متغیرهای اصلی
        sortableInstance: null,
        isDirty: false,
        originalOrder: [],
        currentOrder: [],
        
        // تنظیمات Sortable
        sortableOptions: {
            animation: 200,
            easing: 'cubic-bezier(0.25, 0.8, 0.25, 1)',
            handle: '.group-drag-handle',
            ghostClass: 'sortable-ghost',
            chosenClass: 'sortable-chosen',
            dragClass: 'sortable-drag',
            
            // ⭐ تنظیمات مهم برای عملکرد روان
            forceFallback: true,
            fallbackClass: 'sortable-fallback',
            fallbackOnBody: true,
            swapThreshold: 0.65,
            invertSwap: false,
            direction: 'vertical',
            
            // ⭐ غیرفعال کردن scroll خودکار
            scrollSensitivity: 60,
            scrollSpeed: 10,
            
            // Events
            onStart: null,
            onEnd: null,
            onUpdate: null,
            onChange: null
        },

        // ========================================
        // 🚀 راه‌اندازی اولیه
        // ========================================
        init: function () {
            console.log('🎯 MyDayDragDrop: شروع راه‌اندازی...');
            
            // بررسی وجود Sortable.js
            if (typeof Sortable === 'undefined') {
                console.error('❌ Sortable.js بارگذاری نشده است!');
                this.showError('کتابخانه Drag & Drop بارگذاری نشد. لطفاً صفحه را Refresh کنید.');
                return false;
            }

            // بررسی وجود container
            const container = document.getElementById('my-day-groups-container');
            if (!container) {
                console.warn('⚠️ Container گروه‌ها یافت نشد');
                return false;
            }

            // بررسی وجود گروه‌ها
            const groups = container.querySelectorAll('.my-day-group');
            if (groups.length === 0) {
                console.warn('⚠️ هیچ گروهی برای Drag & Drop یافت نشد');
                return false;
            }

            // ذخیره ترتیب اولیه
            this.saveOriginalOrder();

            // راه‌اندازی Sortable
            this.initializeSortable(container);

            // راه‌اندازی Event Listeners
            this.setupEventListeners();

            // راه‌اندازی دکمه ذخیره
            this.initializeSaveButton();

            console.log(`✅ MyDayDragDrop راه‌اندازی شد - تعداد گروه‌ها: ${groups.length}`);
            return true;
        },

        // ========================================
        // 📦 ذخیره ترتیب اولیه
        // ========================================
        saveOriginalOrder: function () {
            const container = document.getElementById('my-day-groups-container');
            const groups = container.querySelectorAll('.my-day-group');
            
            this.originalOrder = Array.from(groups).map(group => ({
                element: group,
                groupTitle: group.getAttribute('data-group-title'),
                order: Array.from(groups).indexOf(group)
            }));

            this.currentOrder = [...this.originalOrder];
            
            console.log('📦 ترتیب اولیه ذخیره شد:', this.originalOrder.map(g => g.groupTitle));
        },

        // ========================================
        // 🎨 راه‌اندازی Sortable
        // ========================================
        initializeSortable: function (container) {
            // تنظیم Event Handlers
            this.sortableOptions.onStart = (evt) => this.onDragStart(evt);
            this.sortableOptions.onEnd = (evt) => this.onDragEnd(evt);
            this.sortableOptions.onUpdate = (evt) => this.onDragUpdate(evt);
            this.sortableOptions.onChange = (evt) => this.onDragChange(evt);

            // ایجاد Sortable Instance
            try {
                this.sortableInstance = Sortable.create(container, this.sortableOptions);
                console.log('✅ Sortable Instance ایجاد شد');
            } catch (error) {
                console.error('❌ خطا در ایجاد Sortable:', error);
                this.showError('خطا در راه‌اندازی Drag & Drop');
            }
        },

        // ========================================
        // 📍 Event: شروع Drag
        // ========================================
        onDragStart: function (evt) {
            console.log('🎯 Drag شروع شد:', evt.item.getAttribute('data-group-title'));
            
            // اضافه کردن کلاس به body
            document.body.classList.add('is-dragging');
            
            // مخفی کردن tooltip‌ها
            $('[data-bs-toggle="tooltip"]').tooltip('hide');
        },

        // ========================================
        // 🏁 Event: پایان Drag
        // ========================================
        onDragEnd: function (evt) {
            console.log('🏁 Drag تمام شد');
            
            // حذف کلاس از body
            document.body.classList.remove('is-dragging');
            
            // اضافه کردن انیمیشن
            evt.item.classList.add('drop-animation');
            setTimeout(() => {
                evt.item.classList.remove('drop-animation');
            }, 500);
        },

        // ========================================
        // 🔄 Event: تغییر ترتیب
        // ========================================
        onDragUpdate: function (evt) {
            console.log('🔄 ترتیب تغییر کرد - از', evt.oldIndex, 'به', evt.newIndex);
            
            // علامت‌گذاری تغییر
            this.isDirty = true;
            
            // بروزرسانی شماره‌های ترتیب
            this.updateOrderIndicators();
            
            // نمایش دکمه ذخیره
            this.showSaveButton();
            
            // بروزرسانی ترتیب فعلی
            this.updateCurrentOrder();
        },

        // ========================================
        // 🎭 Event: تغییر موقعیت (هنگام حرکت)
        // ========================================
        onDragChange: function (evt) {
            // انیمیشن محو برای آیتم‌ها
            if (evt.from !== evt.to) {
                $(evt.from).addClass('highlight');
                setTimeout(() => $(evt.from).removeClass('highlight'), 300);
            }
        },

        // ========================================
        // 🔢 بروزرسانی شماره‌های ترتیب
        // ========================================
        updateOrderIndicators: function () {
            const container = document.getElementById('my-day-groups-container');
            const groups = container.querySelectorAll('.my-day-group');
            
            groups.forEach((group, index) => {
                const indicator = group.querySelector('.group-order-indicator');
                if (indicator) {
                    indicator.textContent = `#${index + 1}`;
                    
                    // انیمیشن تغییر
                    indicator.classList.add('order-changed');
                    setTimeout(() => {
                        indicator.classList.remove('order-changed');
                    }, 300);
                }
            });
        },

        // ========================================
        // 📝 بروزرسانی ترتیب فعلی
        // ========================================
        updateCurrentOrder: function () {
            const container = document.getElementById('my-day-groups-container');
            const groups = container.querySelectorAll('.my-day-group');
            
            this.currentOrder = Array.from(groups).map((group, index) => ({
                element: group,
                groupTitle: group.getAttribute('data-group-title'),
                order: index
            }));
        },

        // ========================================
        // 💾 ذخیره‌سازی
        // ========================================
        saveGroupOrder: async function () {
            console.log('💾 شروع ذخیره‌سازی...');

            // غیرفعال کردن دکمه
            const $btn = $('#save-group-order-btn');
            $btn.prop('disabled', true).addClass('saving');
            $btn.html('<i class="fa fa-spinner fa-spin me-2"></i>در حال ذخیره...');

            try {
                // آماده‌سازی داده‌ها
                const groupPriorities = this.currentOrder.map((item, index) => ({
                    groupTitle: item.groupTitle,
                    priority: index + 1
                }));

                console.log('📤 ارسال داده:', groupPriorities);

                // ارسال به سرور
                const response = await fetch(window.TaskListConfig?.urls?.updateGroupPriorities || '/TaskingArea/MyDayTask/UpdateGroupPriorities', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    body: JSON.stringify({
                        groupPriorities: groupPriorities
                    })
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const result = await response.json();

                if (result.success) {
                    console.log('✅ ذخیره‌سازی موفق');
                    
                    // نمایش پیام موفقیت
                    this.showSuccess(result.message || 'ترتیب گروه‌ها با موفقیت ذخیره شد');
                    
                    // مخفی کردن دکمه
                    this.hideSaveButton();
                    
                    // بروزرسانی ترتیب اولیه
                    this.saveOriginalOrder();
                    this.isDirty = false;

                } else {
                    throw new Error(result.message || 'خطا در ذخیره‌سازی');
                }

            } catch (error) {
                console.error('❌ خطا در ذخیره‌سازی:', error);
                this.showError(error.message || 'خطا در ارتباط با سرور');
                
                // فعال کردن مجدد دکمه
                $btn.prop('disabled', false).removeClass('saving');
                $btn.html('<i class="fa fa-save me-2"></i>ذخیره ترتیب گروه‌ها');
            }
        },

        // ========================================
        // 👁️ نمایش/مخفی کردن دکمه ذخیره
        // ========================================
        showSaveButton: function () {
            const $container = $('#save-group-order-container');
            if ($container.hasClass('d-none')) {
                $container.removeClass('d-none');
            }
        },

        hideSaveButton: function () {
            const $container = $('#save-group-order-container');
            $container.addClass('d-none');
        },

        // ========================================
        // 🎛️ راه‌اندازی دکمه ذخیره
        // ========================================
        initializeSaveButton: function () {
            const $btn = $('#save-group-order-btn');
            
            if ($btn.length) {
                $btn.off('click').on('click', () => {
                    if (this.isDirty) {
                        this.saveGroupOrder();
                    }
                });
            }
        },

        // ========================================
        // 🔔 Event Listeners
        // ========================================
        setupEventListeners: function () {
            // هشدار قبل از خروج
            $(window).on('beforeunload', (e) => {
                if (this.isDirty) {
                    const message = 'تغییرات ذخیره نشده‌اند. آیا مطمئن هستید؟';
                    e.returnValue = message;
                    return message;
                }
            });

            // Keyboard Shortcuts
            $(document).on('keydown', (e) => {
                // Ctrl+S یا Cmd+S برای ذخیره
                if ((e.ctrlKey || e.metaKey) && e.key === 's') {
                    e.preventDefault();
                    if (this.isDirty) {
                        this.saveGroupOrder();
                    }
                }
                
                // Escape برای انصراف
                if (e.key === 'Escape' && this.isDirty) {
                    this.confirmReset();
                }
            });
        },

        // ========================================
        // 🔄 بازنشانی (Reset)
        // ========================================
        confirmReset: function () {
            Swal.fire({
                title: 'بازنشانی تغییرات؟',
                text: 'آیا می‌خواهید تغییرات را لغو کنید؟',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'بله، لغو کن',
                cancelButtonText: 'خیر',
                customClass: {
                    confirmButton: 'btn btn-danger',
                    cancelButton: 'btn btn-secondary'
                },
                buttonsStyling: false
            }).then((result) => {
                if (result.isConfirmed) {
                    location.reload();
                }
            });
        },

        // ========================================
        // 💬 نمایش پیام‌ها
        // ========================================
        showSuccess: function (message) {
            Swal.fire({
                title: 'موفق!',
                text: message,
                icon: 'success',
                confirmButtonText: 'باشه',
                timer: 3000,
                timerProgressBar: true,
                customClass: {
                    confirmButton: 'btn btn-success'
                },
                buttonsStyling: false
            });
        },

        showError: function (message) {
            Swal.fire({
                title: 'خطا!',
                text: message,
                icon: 'error',
                confirmButtonText: 'باشه',
                customClass: {
                    confirmButton: 'btn btn-danger'
                },
                buttonsStyling: false
            });
        },

        // ========================================
        // 🧹 پاکسازی
        // ========================================
        destroy: function () {
            if (this.sortableInstance) {
                this.sortableInstance.destroy();
                this.sortableInstance = null;
            }
            
            $(window).off('beforeunload');
            $(document).off('keydown');
            
            console.log('🧹 MyDayDragDrop پاکسازی شد');
        }
    };

    // ========================================
    // 🚀 راه‌اندازی خودکار
    // ========================================
    $(document).ready(function () {
        // تاخیر کوچک برای اطمینان از بارگذاری کامل DOM
        setTimeout(() => {
            MyDayDragDrop.init();
        }, 100);
    });

    // ✅ Export به window
    window.MyDayDragDrop = MyDayDragDrop;

})(jQuery);
