/**
 * ⭐⭐⭐ سیستم Drag & Drop برای گروه‌بندی تسک‌های "روز من"
 * استفاده از SortableJS برای Drag & Drop
 */

(function ($) {
    'use strict';

    // ========================================
    // 1️⃣ تنظیمات اولیه
    // ========================================
    const MyDayDragDrop = {
        sortableInstances: [],
        isDirty: false, // آیا تغییری ایجاد شده؟
        
        /**
         * ⭐ راه‌اندازی اولیه
         */
        init: function () {
            console.log('🎯 MyDayDragDrop: Initializing...');
            
            // ✅ بررسی وجود Sortable
            if (typeof Sortable === 'undefined') {
                console.error('❌ Sortable.js not loaded!');
                return;
            }

            this.initializeSortable();
            this.initializeSaveButton();
            this.setupEventListeners();
            
            console.log('✅ MyDayDragDrop: Initialized successfully');
        },

        /**
         * ⭐⭐⭐ راه‌اندازی Sortable برای گروه‌ها
         */
        initializeSortable: function () {
            const groupsContainer = document.getElementById('my-day-groups-container');
            
            if (!groupsContainer) {
                console.warn('⚠️ Groups container not found');
                return;
            }

            // ✅ ایجاد Sortable برای کانتینر اصلی
            const sortable = Sortable.create(groupsContainer, {
                animation: 200,
                handle: '.group-drag-handle', // ⭐ فقط با گرفتن handle قابل جابجایی
                ghostClass: 'sortable-ghost',
                chosenClass: 'sortable-chosen',
                dragClass: 'sortable-drag',
                forceFallback: true,
                fallbackClass: 'sortable-fallback',
                fallbackOnBody: true,
                
                onStart: (evt) => {
                    console.log('🎯 Drag started:', evt.item);
                    evt.item.classList.add('is-dragging');
                },
                
                onEnd: (evt) => {
                    console.log('🎯 Drag ended:', evt.item);
                    evt.item.classList.remove('is-dragging');
                    this.isDirty = true;
                    this.showSaveButton();
                },
                
                onUpdate: (evt) => {
                    console.log('📝 Order changed');
                    this.updateGroupOrderIndicators();
                }
            });

            this.sortableInstances.push(sortable);
            console.log('✅ Sortable initialized for groups container');
        },

        /**
         * ⭐ نمایش شماره ترتیب گروه‌ها
         */
        updateGroupOrderIndicators: function () {
            const groups = document.querySelectorAll('.my-day-group');
            
            groups.forEach((group, index) => {
                const indicator = group.querySelector('.group-order-indicator');
                if (indicator) {
                    indicator.textContent = `#${index + 1}`;
                }
            });
        },

        /**
         * ⭐⭐⭐ نمایش دکمه ذخیره
         */
        showSaveButton: function () {
            const saveBtn = document.getElementById('save-group-order-btn');
            if (saveBtn) {
                saveBtn.classList.remove('d-none');
                saveBtn.classList.add('animate__animated', 'animate__bounceIn');
            }
        },

        /**
         * ⭐⭐⭐ مخفی کردن دکمه ذخیره
         */
        hideSaveButton: function () {
            const saveBtn = document.getElementById('save-group-order-btn');
            if (saveBtn) {
                saveBtn.classList.add('d-none');
                saveBtn.classList.remove('animate__animated', 'animate__bounceIn');
            }
            this.isDirty = false;
        },

        /**
         * ⭐⭐⭐ راه‌اندازی دکمه ذخیره
         */
        initializeSaveButton: function () {
            const saveBtn = document.getElementById('save-group-order-btn');
            
            if (saveBtn) {
                saveBtn.addEventListener('click', () => {
                    this.saveGroupOrder();
                });
            }
        },

        /**
         * ⭐⭐⭐ ذخیره ترتیب گروه‌ها
         */
        saveGroupOrder: async function () {
            try {
                const groups = document.querySelectorAll('.my-day-group');
                const groupPriorities = [];

                groups.forEach((group, index) => {
                    const groupTitle = group.getAttribute('data-group-title');
                    groupPriorities.push({
                        groupTitle: groupTitle,
                        priority: index + 1
                    });
                });

                console.log('💾 Saving group order:', groupPriorities);

                // ✅ ارسال به سرور
                const response = await fetch(window.TaskListConfig.urls.updateGroupPriorities || '@Url.Action("UpdateGroupPriorities", "MyDayTask")', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({
                        groupPriorities: groupPriorities
                    })
                });

                const result = await response.json();

                if (result.success) {
                    Swal.fire({
                        title: 'موفق!',
                        text: result.message || 'ترتیب گروه‌ها با موفقیت ذخیره شد',
                        icon: 'success',
                        confirmButtonText: 'باشه',
                        timer: 2000,
                        timerProgressBar: true
                    });

                    this.hideSaveButton();
                } else {
                    throw new Error(result.message || 'خطا در ذخیره‌سازی');
                }

            } catch (error) {
                console.error('❌ Error saving group order:', error);
                Swal.fire({
                    title: 'خطا!',
                    text: error.message || 'خطا در ذخیره‌سازی',
                    icon: 'error',
                    confirmButtonText: 'باشه'
                });
            }
        },

        /**
         * ⭐ راه‌اندازی Event Listeners
         */
        setupEventListeners: function () {
            // ✅ هشدار قبل از خروج از صفحه (اگر تغییری ذخیره نشده)
            window.addEventListener('beforeunload', (e) => {
                if (this.isDirty) {
                    e.preventDefault();
                    e.returnValue = 'تغییرات ذخیره نشده‌اند. آیا مطمئن هستید؟';
                    return e.returnValue;
                }
            });
        },

        /**
         * ⭐ Cleanup
         */
        destroy: function () {
            this.sortableInstances.forEach(instance => {
                instance.destroy();
            });
            this.sortableInstances = [];
        }
    };

    // ========================================
    // 2️⃣ راه‌اندازی خودکار
    // ========================================
    $(document).ready(function () {
        MyDayDragDrop.init();
    });

    // ✅ Expose به window
    window.MyDayDragDrop = MyDayDragDrop;

})(jQuery);
