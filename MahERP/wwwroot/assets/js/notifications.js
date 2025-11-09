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
            });

            this.connection.onclose(() => {
                console.error("❌ SignalR connection closed");
            });

            await this.connection.start();
            console.log("✅ SignalR Connected");
        } catch (err) {
            console.error("❌ SignalR Error:", err);
            setTimeout(() => this.setupSignalR(), 5000); // تلاش مجدد بعد از 5 ثانیه
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
        this.loadInitialNotifications(); // بروزرسانی لیست
    }

    // ⭐ پخش صدا
    playNotificationSound() {
        const now = Date.now();
        if (this.audioEnabled && (now - this.lastPlayTime) >= this.audioInterval) {
            const audio = document.getElementById('notificationSound');
            if (audio) {
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
            }, 10000); // 10 ثانیه
        }
    }

    // ⭐ بروزرسانی Badge
    updateBadge() {
        $.get('/TaskingArea/Notification/GetUnreadCount').done((data) => {
            if (data.success) {
                const badge = $('#headerNotificationBadge');
                if (data.count > 0) {
                    badge.text(data.count > 99 ? '99+' : data.count).show();
                    badge.addClass('animate__animated animate__heartBeat');
                    setTimeout(() => badge.removeClass('animate__animated animate__heartBeat'), 1000);
                } else {
                    badge.hide();
                }
            }
        });
    }

    // ⭐ بارگذاری لیست اولیه
    loadInitialNotifications() {
        $.get('/TaskingArea/Notification/GetHeaderNotifications').done((html) => {
            $('#headerNotificationsList').html(html);
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
            const response = await fetch('/TaskingArea/Notification/MarkAsRead', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
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
            const response = await fetch('/TaskingArea/Notification/MarkAllAsRead', {
                method: 'POST'
            });

            if (response.ok) {
                toastr.success('همه اعلان‌ها خوانده شدند');
                this.updateBadge();
                this.loadInitialNotifications();
            }
        } catch (error) {
            console.error('❌ Error marking all as read:', error);
        }
    }
}

// ⭐ مقداردهی اولیه
const notificationManager = new NotificationManager();

// ⭐ توابع Global
function goToNotification(notificationId, actionUrl) {
    notificationManager.markAsRead(notificationId);
    if (actionUrl) {
        window.location.href = actionUrl;
    }
}

function markAllHeaderNotificationsAsRead() {
    notificationManager.markAllAsRead();
}