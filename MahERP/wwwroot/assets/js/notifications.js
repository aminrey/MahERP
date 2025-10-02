class NotificationManager {
    constructor() {
        this.connection = null;
        this.audioEnabled = true;
        this.lastPlayTime = 0;
        this.audioInterval = 5 * 60 * 1000; // 5 دقیقه
        this.unreadNotifications = [];
        this.init();
    }

    async init() {
        await this.setupSignalR();
        this.setupAudioPermission();
        this.loadInitialNotifications();
        this.startPeriodicCheck();
    }

    async setupSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/notificationHub")
                .withAutomaticReconnect()
                .build();

            this.connection.on("ReceiveNotification", (notification) => {
                this.handleNewNotification(notification);
            });

            await this.connection.start();
            console.log("SignalR متصل شد");
        } catch (err) {
            console.error("خطا در اتصال SignalR:", err);
        }
    }

    setupAudioPermission() {
        // درخواست اجازه پخش صدا از کاربر
        document.addEventListener('click', () => {
            const audio = document.getElementById('notificationSound');
            if (audio) {
                audio.play().then(() => {
                    audio.pause();
                    audio.currentTime = 0;
                }).catch(() => {
                    console.log("Audio permission denied");
                });
            }
        }, { once: true });
    }

    handleNewNotification(notification) {
        this.unreadNotifications.push(notification);
        this.updateUI();
        this.showBrowserNotification(notification);
        this.playNotificationSound();
        this.highlightRelatedRows(notification);
    }

    playNotificationSound() {
        const now = Date.now();
        if (this.audioEnabled && (now - this.lastPlayTime) >= this.audioInterval) {
            const audio = document.getElementById('notificationSound');
            if (audio) {
                audio.play().catch(() => {
                    console.log("خطا در پخش صدا");
                });
                this.lastPlayTime = now;
            }
        }
    }

    showBrowserNotification(notification) {
        if (Notification.permission === "granted") {
            new Notification(notification.title, {
                body: notification.message,
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

    highlightRelatedRows(notification) {
        // چشمک زدن ردیف‌های مرتبط در داشبورد
        if (notification.type === 'NewTask') {
            this.blinkTableRow(`#receivedTasksTable tr[data-task-id="${notification.relatedId}"]`);
        } else if (notification.type === 'Reminder') {
            this.blinkTableRow(`#remindersTable tr[data-reminder-id="${notification.relatedId}"]`);
        }
    }

    blinkTableRow(selector) {
        const row = document.querySelector(selector);
        if (row) {
            row.classList.add('notification-blink');
            setTimeout(() => {
                row.classList.remove('notification-blink');
            }, 10000); // 10 ثانیه چشمک بزند
        }
    }

    updateUI() {
        const unreadCount = this.unreadNotifications.filter(n => !n.isRead).length;
        
        // بروزرسانی badge در header
        const headerBadge = document.getElementById('headerNotificationBadge');
        if (headerBadge) {
            if (unreadCount > 0) {
                headerBadge.textContent = unreadCount;
                headerBadge.style.display = 'block';
            } else {
                headerBadge.style.display = 'none';
            }
        }

        // بروزرسانی لیست نوتیفیکیشن‌ها
        this.updateNotificationsList();
    }

    updateNotificationsList() {
        const container = document.getElementById('headerNotificationsList');
        if (!container) return;

        if (this.unreadNotifications.length === 0) {
            container.innerHTML = `
                <div class="text-center p-3 text-muted">
                    <i class="fa fa-bell-slash fa-2x mb-2"></i>
                    <div>هیچ نوتیفیکیشنی وجود ندارد</div>
                </div>
            `;
            return;
        }

        let html = '';
        this.unreadNotifications.slice(0, 10).forEach(notification => {
            const isReadClass = notification.isRead ? 'notification-read' : 'notification-unread';
            const priorityClass = this.getPriorityClass(notification.priority);

            html += `
                <div class="notification-item ${isReadClass} ${priorityClass}" data-notification-id="${notification.id}">
                    <a class="d-flex text-dark py-2" href="javascript:void(0)" onclick="goToNotification(${notification.id}, '${notification.actionUrl}')">
                        <div class="flex-shrink-0 mx-3">
                            <i class="fa fa-fw ${notification.icon} text-primary"></i>
                        </div>
                        <div class="flex-grow-1 fs-sm pe-2">
                            <div class="fw-semibold">${notification.title}</div>
                            <div class="text-muted">${notification.message}</div>
                            <div class="text-muted small">${this.formatTime(notification.createTime)}</div>
                        </div>
                        ${!notification.isRead ? '<div class="badge bg-danger">جدید</div>' : ''}
                    </a>
                </div>
            `;
        });

        container.innerHTML = html;
    }

    getPriorityClass(priority) {
        switch (priority) {
            case 4: return 'priority-urgent';
            case 3: return 'priority-high';
            default: return '';
        }
    }

    formatTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        
        if (diffMins < 1) return 'اکنون';
        if (diffMins < 60) return `${diffMins} دقیقه پیش`;
        if (diffMins < 1440) return `${Math.floor(diffMins / 60)} ساعت پیش`;
        return date.toLocaleDateString('fa-IR');
    }

    async loadInitialNotifications() {
        try {
            const response = await fetch('/AdminArea/Notification/GetLatestNotifications?count=10');
            const data = await response.json();
            
            if (data.success) {
                this.unreadNotifications = data.notifications;
                this.updateUI();
            }
        } catch (error) {
            console.error('خطا در بارگذاری نوتیفیکیشن‌ها:', error);
        }
    }

    startPeriodicCheck() {
        // چک هر 30 ثانیه برای بروزرسانی
        setInterval(() => {
            this.loadInitialNotifications();
        }, 30000);
    }

    async markAsRead(notificationId) {
        try {
            const response = await fetch('/AdminArea/Notification/MarkAsRead', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ id: notificationId })
            });

            if (response.ok) {
                const notification = this.unreadNotifications.find(n => n.id === notificationId);
                if (notification) {
                    notification.isRead = true;
                    this.updateUI();
                }
            }
        } catch (error) {
            console.error('خطا در علامت‌گذاری نوتیفیکیشن:', error);
        }
    }

    async markAllAsRead() {
        try {
            const response = await fetch('/AdminArea/Notification/MarkAllAsRead', {
                method: 'POST'
            });

            if (response.ok) {
                this.unreadNotifications.forEach(n => n.isRead = true);
                this.updateUI();
            }
        } catch (error) {
            console.error('خطا در علامت‌گذاری همه نوتیفیکیشن‌ها:', error);
        }
    }
}

// مقداردهی اولیه
const notificationManager = new NotificationManager();

// توابع Global
function goToNotification(notificationId, actionUrl) {
    notificationManager.markAsRead(notificationId);
    if (actionUrl) {
        window.location.href = actionUrl;
    }
}

function markAllHeaderNotificationsAsRead() {
    notificationManager.markAllAsRead();
}