/**
 * QuickAdd Helper Functions
 * برای استفاده در Interaction Create و Goal Create
 * استفاده از Nested Modal برای باز کردن Modal داخل Modal
 */

// ==================== OPEN MODALS ====================

/**
 * باز کردن مودال انتخاب نوع (Contact یا Organization)
 * @param {number} branchId - شناسه شعبه
 * @param {number|null} organizationId - شناسه سازمان (اختیاری)
 */
function openQuickAddModal(branchId, organizationId = null) {
    // ⭐⭐⭐ Debug logging
    console.log('📞 openQuickAddModal called with:', { branchId, organizationId });
    console.log('   - branchId type:', typeof branchId);
    console.log('   - branchId value:', branchId);
    
    // ⭐ بررسی BranchId
    if (!branchId || branchId === 0 || branchId === '0') {
        console.error('❌ Invalid branchId:', branchId);
        
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'خطا',
                html: 'شناسه شعبه نامعتبر است!<br><br>' +
                      'مقدار دریافت شده: ' + branchId + '<br>' +
                      'لطفاً با مدیر سیستم تماس بگیرید.',
                icon: 'error',
                confirmButtonText: 'متوجه شدم'
            });
        } else if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.error('شناسه شعبه نامعتبر است: ' + branchId);
        } else if (typeof toastr !== 'undefined') {
            toastr.error('شناسه شعبه نامعتبر است: ' + branchId);
        } else {
            alert('شناسه شعبه نامعتبر است: ' + branchId);
        }
        return;
    }

    console.log('✅ BranchId is valid, opening modal...');

    // ⭐⭐⭐ استفاده از createAndShowNestedModal برای modal داخل modal
    createAndShowNestedModal({
        url: `/CrmArea/QuickAdd/SelectTypeModal?branchId=${branchId}` + (organizationId ? `&organizationId=${organizationId}` : ''),
        backdrop: 'dim', // backdrop شفاف
        onHidden: function () {
            console.log('✅ QuickAdd SelectType Modal closed');
        }
    }).catch(err => {
        console.error('❌ Error opening SelectType modal:', err);
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'خطا',
                text: 'خطا در باز کردن مودال: ' + err.message,
                icon: 'error'
            });
        }
    });
}

/**
 * باز کردن مودال افزودن سریع Contact
 * این تابع Modal قبلی رو می‌بنده و Modal جدید رو باز می‌کنه
 * @param {number} branchId - شناسه شعبه
 * @param {number|null} organizationId - شناسه سازمان (اختیاری)
 */
function openQuickAddContact(branchId, organizationId = null) {
    console.log('🎯 Opening QuickAdd Contact modal');

    // ⭐⭐⭐ بستن Modal SelectType
    const $selectTypeModal = $('.modal.show').last();

    if ($selectTypeModal.length) {
        const modalInstance = bootstrap.Modal.getInstance($selectTypeModal[0]);
        if (modalInstance) {
            console.log('🔄 Closing SelectType modal...');
            modalInstance.hide();
        }
    }

    // ⭐ کمی تاخیر برای اطمینان از بسته شدن Modal قبلی
    setTimeout(() => {
        createAndShowNestedModal({
            url: `/CrmArea/QuickAdd/QuickAddContactModal?branchId=${branchId}` + (organizationId ? `&organizationId=${organizationId}` : ''),
            backdrop: 'dim',
            onHidden: function () {
                console.log('✅ QuickAdd Contact Modal closed');
            }
        }).catch(err => {
            console.error('❌ Error opening Contact modal:', err);
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'خطا',
                    text: 'خطا در باز کردن فرم: ' + err.message,
                    icon: 'error'
                });
            }
        });
    }, 300); // 300ms delay
}

/**
 * باز کردن مودال افزودن سریع Organization
 * این تابع Modal قبلی رو می‌بنده و Modal جدید رو باز می‌کنه
 * @param {number} branchId - شناسه شعبه
 */
function openQuickAddOrganization(branchId) {
    console.log('🎯 Opening QuickAdd Organization modal');

    // ⭐⭐⭐ بستن Modal SelectType
    const $selectTypeModal = $('.modal.show').last();

    if ($selectTypeModal.length) {
        const modalInstance = bootstrap.Modal.getInstance($selectTypeModal[0]);
        if (modalInstance) {
            console.log('🔄 Closing SelectType modal...');
            modalInstance.hide();
        }
    }

    // ⭐ کمی تاخیر برای اطمینان از بسته شدن Modal قبلی
    setTimeout(() => {
        createAndShowNestedModal({
            url: `/CrmArea/QuickAdd/QuickAddOrganizationModal?branchId=${branchId}`,
            backdrop: 'dim',
            onHidden: function () {
                console.log('✅ QuickAdd Organization Modal closed');
            }
        }).catch(err => {
            console.error('❌ Error opening Organization modal:', err);
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'خطا',
                    text: 'خطا در باز کردن فرم: ' + err.message,
                    icon: 'error'
                });
            }
        });
    }, 300); // 300ms delay
}

// ==================== CALLBACK ====================

/**
 * Callback بعد از ساخت موفق
 * این تابع باید در صفحه اصلی override بشه
 * @param {string} type - نوع (contact یا organization)
 * @param {object} response - پاسخ سرور
 */
window.onQuickAddComplete = function (type, response) {
    console.log('✅ QuickAdd completed:', type, response);

    // پیش‌فرض: فقط لاگ
    // در صفحه اصلی این تابع override میشه

    if (type === 'contact') {
        console.log('📝 Contact created:', response.contactId, response.contactName);
        // باید در Select2 انتخاب بشه یا صفحه Reload بشه
    } else if (type === 'organization') {
        console.log('🏢 Organization created:', response.organizationId, response.organizationName);
        // باید در Select2 انتخاب بشه یا صفحه Reload بشه
    }
};

// ==================== SELECT2 HELPERS ====================

/**
 * انتخاب خودکار مقدار جدید در Select2 Contact
 * @param {number} contactId - شناسه Contact
 * @param {string} contactName - نام Contact
 */
function selectNewContact(contactId, contactName) {
    const $select = $('#contactSelector');

    if ($select.length === 0) {
        console.error('❌ Contact selector not found');
        return;
    }

    // اضافه کردن option جدید
    const newOption = new Option(contactName, contactId, true, true);
    $select.append(newOption).trigger('change');

    console.log('✅ Contact selected:', contactId, contactName);
}

/**
 * انتخاب خودکار مقدار جدید در Select2 Organization
 * @param {number} organizationId - شناسه Organization
 * @param {string} organizationName - نام Organization
 */
function selectNewOrganization(organizationId, organizationName) {
    const $select = $('#organizationSelector');

    if ($select.length === 0) {
        console.error('❌ Organization selector not found');
        return;
    }

    // اضافه کردن option جدید
    const newOption = new Option(organizationName, organizationId, true, true);
    $select.append(newOption).trigger('change');

    console.log('✅ Organization selected:', organizationId, organizationName);
}

// ==================== CONSOLE INFO ====================

console.log('📦 QuickAdd Helper loaded successfully');
console.log('✅ Available functions:');
console.log('  - openQuickAddModal(branchId, organizationId?)');
console.log('  - openQuickAddContact(branchId, organizationId?)');
console.log('  - openQuickAddOrganization(branchId)');
console.log('  - window.onQuickAddComplete(type, response)');
console.log('  - selectNewContact(contactId, contactName)');
console.log('  - selectNewOrganization(organizationId, organizationName)');
