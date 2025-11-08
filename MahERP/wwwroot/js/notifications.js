// ==================== Notification Management ====================

let notificationCheckInterval;

$(document).ready(function() {
    loadHeaderNotifications();
    
    // بروزرسانی هر 30 ثانیه
    notificationCheckInterval = setInterval(loadHeaderNotifications, 30000);
});

function loadHeaderNotifications() {
    $.get('/TaskingArea/Notification/GetUnreadCount').done(function(data) {
        if (data.success) {
            updateNotificationBadge(data.count);
            
            if (data.count > 0) {
                loadNotificationList();
            }
        }
    });
}

function updateNotificationBadge(count) {
    const badge = $('#headerNotificationBadge');
    
    if (count > 0) {
        badge.text(count > 99 ? '99+' : count).show();
        
        // انیمیشن pulse
        badge.addClass('animate__animated animate__heartBeat');
        setTimeout(() => badge.removeClass('animate__animated animate__heartBeat'), 1000);
        
        // پخش صدا (اختیاری)
        playNotificationSound();
    } else {
        badge.hide();
    }
}

function loadNotificationList() {
    $.get('/TaskingArea/Notification/GetHeaderNotifications').done(function(html) {
        $('#headerNotificationsList').html(html);
    });
}

function markAllHeaderNotificationsAsRead() {
    $.post('/TaskingArea/Notification/MarkAllAsRead').done(function(data) {
        if (data.success) {
            toastr.success('همه اعلان‌ها خوانده شده علامت‌گذاری شدند');
            loadHeaderNotifications();
        }
    });
}

function playNotificationSound() {
    const audio = document.getElementById('notificationSound');
    if (audio) {
        audio.play().catch(() => {}); // ignore errors
    }
}