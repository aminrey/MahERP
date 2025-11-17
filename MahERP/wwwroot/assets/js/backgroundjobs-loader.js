/**
 * ========================================
 * Load Background Jobs for Header Dropdown
 * ========================================
 */

function loadBackgroundJobs() {
    $.ajax({
        url: '/CrmArea/BackgroundJob/GetActiveJobs',
        type: 'GET',
        success: function (response) {
            if (response.success) {
                updateJobsDropdown(response.jobs);
                updateJobsBadge(response.jobs.length);
            } else {
                console.error('Failed to load jobs:', response.message);
                updateJobsDropdown([]);
                updateJobsBadge(0);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading background jobs:', error);
            updateJobsDropdown([]);
            updateJobsBadge(0);
        }
    });
}

function updateJobsDropdown(jobs) {
    const $jobsList = $('#background-jobs-list');
    const $dropdown = $('#page-header-jobs-dropdown');
    
    if (!jobs || jobs.length === 0) {
        $jobsList.html('<div class="text-center text-muted py-3">کاری در حال اجرا نیست</div>');
        $dropdown.addClass('d-none'); // ⭐ مخفی کردن دکمه
        return;
    }

    // ⭐ نمایش دکمه اگر Job فعال داشتیم
    $dropdown.removeClass('d-none');

    let html = '';
    jobs.forEach(job => {
        html += `
            <div class="job-item mb-2 p-2 border-bottom" data-job-id="${job.id}">
                <div class="d-flex justify-content-between align-items-center mb-1">
                    <strong class="fs-sm">${job.title}</strong>
                    <span class="badge ${job.statusBadgeClass}">${job.statusText}</span>
                </div>
                <div class="progress mb-1" style="height: 15px;">
                    <div class="progress-bar" role="progressbar" 
                         style="width: ${job.progress}%" 
                         data-job-id="${job.id}">
                        <span class="progress-text">${job.progress}%</span>
                    </div>
                </div>
                <div class="d-flex justify-content-between fs-xs text-muted">
                    <span>پردازش شده: <span class="processed-text">${job.processedItems}</span>/${job.totalItems}</span>
                    <span>موفق: <span class="success-text">${job.successCount}</span> | ناموفق: <span class="failed-text">${job.failedCount}</span></span>
                </div>
                ${job.errorMessage ? `<div class="text-danger fs-xs mt-1">${job.errorMessage}</div>` : ''}
            </div>
        `;
    });

    $jobsList.html(html);
}

function updateJobsBadge(count) {
    const $badge = $('#background-jobs-count');
    const $dropdown = $('#page-header-jobs-dropdown');
    
    if (count > 0) {
        $badge.text(count).removeClass('d-none');
        $dropdown.removeClass('d-none'); // ⭐ نمایش دکمه
    } else {
        $badge.addClass('d-none');
        $dropdown.addClass('d-none'); // ⭐ مخفی کردن دکمه
    }
}

// ⭐ بارگذاری اولیه و تنظیم Interval
$(document).ready(function () {
    const currentArea = $('body').data('area');
    
    if (currentArea === 'CrmArea' || window.location.pathname.includes('/CrmArea/')) {
        console.log('🔄 Loading Background Jobs...');
        
        // بارگذاری اولیه
        loadBackgroundJobs();
        
        // بروزرسانی هر 5 ثانیه (برای Realtime بهتر)
        setInterval(loadBackgroundJobs, 5000);
    }
});

// ⭐ بارگذاری مجدد بعد از اتصال SignalR
window.reloadBackgroundJobs = function() {
    console.log('🔄 Reloading Background Jobs after SignalR connect...');
    loadBackgroundJobs();
};
