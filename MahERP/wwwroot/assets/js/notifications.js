// ==================== Notification Management with SignalR ====================

class NotificationManager {
    constructor() {
        this.connection = null;
        this.audioEnabled = true;
        this.lastPlayTime = 0;
        this.audioInterval = 5 * 60 * 1000; // 5 دقیقه
        this.init();
    }

    async init() {
        await this.setupSignalR();
        this.setupAudioPermission();
        this.loadInitialNotifications();
        this.updateBadge(); // ⭐ بارگذاری اولیه badge
        this.startPeriodicCheck();
    }

    // ⭐ اتصال SignalR
    async setupSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/notificationHub")
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // ⭐ رویداد دریافت اعلان جدید
            this.connection.on("ReceiveNotification", (notification) => {
                console.log("✅ New notification received:", notification);
                this.handleNewNotification(notification);
            });

            // ⭐ رویداد اتصال مجدد
            this.connection.onreconnecting(() => {
                console.warn("⚠️ SignalR reconnecting...");
            });

            this.connection.onreconnected(() => {
                console.log("✅ SignalR reconnected");
                this.loadInitialNotifications();
                this.updateBadge();
            });

            this.connection.onclose(() => {
                console.error("❌ SignalR connection closed");
            });

            await this.connection.start();
            console.log("✅ SignalR Connected");
        } catch (err) {
            console.error("❌ SignalR Error:", err);
            setTimeout(() => this.setupSignalR(), 5000);
        }
    }

    // ⭐ مدیریت اجازه پخش صدا
    setupAudioPermission() {
        document.addEventListener('click', () => {
            const audio = document.getElementById('notificationSound');
            if (audio) {
                audio.play().then(() => {
                    audio.pause();
                    audio.currentTime = 0;
                }).catch(() => {
                    console.log("❌ Audio permission denied");
                });
            }
        }, { once: true });
    }

    // ⭐ مدیریت اعلان جدید
    handleNewNotification(notification) {
        this.updateBadge();
        this.showBrowserNotification(notification);
        this.playNotificationSound();
        this.highlightRelatedRows(notification);
        this.loadInitialNotifications();
    }

    // ⭐ پخش صدا
    playNotificationSound() {
        const now = Date.now();
        if (this.audioEnabled && (now - this.lastPlayTime) >= this.audioInterval) {
            const audio = document.getElementById('notificationSound');
            if (audio) {
                audio.volume = 0.5;
                audio.play().catch(() => {
                    console.log("❌ Cannot play sound");
                });
                this.lastPlayTime = now;
            }
        }
    }

    // ⭐ اعلان مرورگر
    showBrowserNotification(notification) {
        if (Notification.permission === "granted") {
            new Notification(notification.title || "اعلان جدید", {
                body: notification.message || "شما یک اعلان جدید دریافت کردید",
                icon: "/assets/media/favicons/favicon-192x192.png",
                tag: `notification_${notification.id}`
            });
        } else if (Notification.permission !== "denied") {
            Notification.requestPermission().then(permission => {
                if (permission === "granted") {
                    this.showBrowserNotification(notification);
                }
            });
        }
    }

    // ⭐ هایلایت ردیف‌های مربوطه
    highlightRelatedRows(notification) {
        if (notification.type === 'NewTask' || notification.type === 'TaskAssigned') {
            this.blinkTableRow(`#receivedTasksTable tr[data-task-id="${notification.relatedId}"]`);
        } else if (notification.type === 'Reminder') {
            this.blinkTableRow(`#remindersTable tr[data-reminder-id="${notification.relatedId}"]`);
        }
    }

    blinkTableRow(selector) {
        const row = $(selector);
        if (row.length) {
            row.addClass('notification-blink');
            setTimeout(() => {
                row.removeClass('notification-blink');
            }, 10000);
        }
    }

    // ⭐⭐⭐ بروزرسانی Badge با انیمیشن
    updateBadge() {
        $.get('/TaskingArea/Notification/GetUnreadCount').done((data) => {
            if (data.success) {
                const count = data.count || 0;
                const badge = $('#headerNotificationBadge');
                const bell = $('.notification-bell');
                const oldCount = parseInt(badge.text()) || 0;
                
                console.log('🔔 Badge Update: old=' + oldCount + ', new=' + count);
                
                if (count > 0) {
                    badge.text(count > 99 ? '99+' : count).show();
                    
                    // اگر تعداد افزایش یافته (نوتیفیکیشن جدید)
                    if (count > oldCount && oldCount !== 0) {
                        console.log('✨ New notification animation!');
                        
                        // اضافه کردن کلاس برای انیمیشن
                        badge.removeClass('new-notification').addClass('new-notification');
                        bell.removeClass('shake has-new').addClass('shake has-new');
                        
                        // حذف کلاس بعد از اتمام انیمیشن
                        setTimeout(() => {
                            badge.removeClass('new-notification');
                            bell.removeClass('shake');
                        }, 600);
                        
                        // حذف ring effect بعد از 3 ثانیه
                        setTimeout(() => {
                            bell.removeClass('has-new');
                        }, 3000);
                    }
                } else {
                    console.log('✅ No notifications');
                    badge.hide();
                    bell.removeClass('has-new');
                }
            }
        }).fail(() => {
            console.error('❌ Failed to load notification count');
        });
    }

    // ⭐ بارگذاری لیست اولیه
    loadInitialNotifications() {
        $.get('/TaskingArea/Notification/GetHeaderNotifications').done((html) => {
            $('#headerNotificationsList').html(html);
        }).fail(() => {
            $('#headerNotificationsList').html(`
                <div class="text-center p-3 text-muted">
                    <i class="fa fa-exclamation-triangle mb-2"></i>
                    <p>خطا در بارگذاری اعلان‌ها</p>
                </div>
            `);
        });
    }

    // ⭐ چک دوره‌ای (backup برای SignalR)
    startPeriodicCheck() {
        setInterval(() => {
            this.updateBadge();
        }, 30000); // هر 30 ثانیه
    }

    // ⭐ علامت‌گذاری یک اعلان
    async markAsRead(notificationId) {
        try {
            const token = $('input[name="__RequestVerificationToken"]').val();
            const response = await fetch('/TaskingArea/Notification/MarkAsRead', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ id: notificationId })
            });

            if (response.ok) {
                this.updateBadge();
                this.loadInitialNotifications();
            }
        } catch (error) {
            console.error('❌ Error marking notification as read:', error);
        }
    }

    // ⭐ علامت‌گذاری همه
    async markAllAsRead() {
        try {
            const token = $('input[name="__RequestVerificationToken"]').val();
            const response = await fetch('/TaskingArea/Notification/MarkAllAsRead', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                }
            });

            if (response.ok) {
                if (typeof toastr !== 'undefined') {
                    toastr.success('همه اعلان‌ها خوانده شدند');
                }
                this.updateBadge();
                this.loadInitialNotifications();
            }
        } catch (error) {
            console.error('❌ Error marking all as read:', error);
        }
    }
}

// ⭐ مقداردهی اولیه
let notificationManager;

$(document).ready(function() {
    notificationManager = new NotificationManager();
    console.log('🔔 Notification Manager initialized');
});

// ⭐ توابع Global
function goToNotification(notificationId, actionUrl) {
    if (notificationManager) {
        notificationManager.markAsRead(notificationId);
    }
    if (actionUrl) {
        window.location.href = actionUrl;
    }
}

function markAllHeaderNotificationsAsRead() {
    if (notificationManager) {
        notificationManager.markAllAsRead();
    }
} 