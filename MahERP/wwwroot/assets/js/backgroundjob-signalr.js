/**
 * ========================================
 * MahERP Background Jobs SignalR Client
 * ========================================
 * بروزرسانی Realtime کارهای پس‌زمینه
 */

class BackgroundJobSignalR {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 3000;
    }

    /**
     * شروع اتصال SignalR
     */
    async start() {
        try {
            // ایجاد اتصال
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/backgroundjob")
                .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // تنظیم Event Handlers
            this.setupEventHandlers();

            // شروع اتصال
            await this.connection.start();
            this.isConnected = true;
            this.reconnectAttempts = 0;

            console.log("✅ BackgroundJob SignalR connected");
            
            // بروزرسانی اولیه
            this.requestJobsUpdate();

        } catch (error) {
            console.error("❌ BackgroundJob SignalR connection error:", error);
            this.isConnected = false;
            
            // تلاش مجدد برای اتصال
            this.scheduleReconnect();
        }
    }

    /**
     * تنظیم Event Handlers
     */
    setupEventHandlers() {
        // ========== رویداد شروع Job ==========
        this.connection.on("JobStarted", (data) => {
            console.log("🚀 Job Started:", data);
            
            // نمایش دکمه Background Jobs
            $('#page-header-jobs-dropdown').removeClass('d-none');
            
            // بروزرسانی لیست
            loadBackgroundJobs();
            
            // نمایش Toast Notification
            if (typeof toastr !== 'undefined') {
                toastr.info(`شروع: ${data.title}`, 'کار جدید', {
                    timeOut: 3000,
                    positionClass: 'toast-top-left'
                });
            }
        });

        // ========== رویداد پیشرفت Job ==========
        this.connection.on("JobProgressUpdated", (data) => {
            console.log("📊 Job Progress:", data);
            
            // بروزرسانی Progress Bar
            const progressBar = $(`.progress-bar[data-job-id="${data.jobId}"]`);
            if (progressBar.length > 0) {
                progressBar.css('width', `${data.progress}%`);
                progressBar.closest('.job-item').find('.progress-text').text(`${data.progress}%`);
                progressBar.closest('.job-item').find('.processed-text').text(`${data.processed}`);
                progressBar.closest('.job-item').find('.success-text').text(`${data.success}`);
                progressBar.closest('.job-item').find('.failed-text').text(`${data.failed}`);
            }
            
            // بروزرسانی Badge
            updateJobsBadge();
        });

        // ========== رویداد تکمیل Job ==========
        this.connection.on("JobCompleted", (data) => {
            console.log("✅ Job Completed:", data);
            
            // بروزرسانی لیست
            setTimeout(() => {
                loadBackgroundJobs();
            }, 1000);
            
            // نمایش Notification
            if (data.isSuccess) {
                if (typeof toastr !== 'undefined') {
                    toastr.success('عملیات با موفقیت تکمیل شد', 'اتمام کار', {
                        timeOut: 5000,
                        positionClass: 'toast-top-left'
                    });
                }
            } else {
                if (typeof toastr !== 'undefined') {
                    toastr.error(data.errorMessage || 'عملیات با خطا مواجه شد', 'خطا', {
                        timeOut: 5000,
                        positionClass: 'toast-top-left'
                    });
                }
            }
        });

        // ========== درخواست بروزرسانی لیست ==========
        this.connection.on("RefreshJobsList", () => {
            console.log("🔄 Refresh Jobs List");
            loadBackgroundJobs();
        });

        // ========== رویدادهای اتصال ==========
        this.connection.onreconnecting((error) => {
            console.warn("⚠️ SignalR Reconnecting...", error);
            this.isConnected = false;
        });

        this.connection.onreconnected((connectionId) => {
            console.log("✅ SignalR Reconnected:", connectionId);
            this.isConnected = true;
            this.reconnectAttempts = 0;
            
            // بروزرسانی لیست پس از اتصال مجدد
            this.requestJobsUpdate();
        });

        this.connection.onclose((error) => {
            console.error("❌ SignalR Connection Closed:", error);
            this.isConnected = false;
            
            // تلاش مجدد برای اتصال
            this.scheduleReconnect();
        });
    }

    /**
     * درخواست بروزرسانی Job ها
     */
    async requestJobsUpdate() {
        if (this.isConnected && this.connection) {
            try {
                await this.connection.invoke("RequestJobsUpdate");
            } catch (error) {
                console.error("Error requesting jobs update:", error);
            }
        }
    }

    /**
     * برنامه‌ریزی اتصال مجدد
     */
    scheduleReconnect() {
        if (this.reconnectAttempts >= this.maxReconnectAttempts) {
            console.error("Max reconnect attempts reached");
            return;
        }

        this.reconnectAttempts++;
        const delay = this.reconnectDelay * this.reconnectAttempts;

        console.log(`Reconnecting in ${delay}ms (Attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`);

        setTimeout(() => {
            this.start();
        }, delay);
    }

    /**
     * قطع اتصال
     */
    async stop() {
        if (this.connection) {
            try {
                await this.connection.stop();
                console.log("SignalR connection stopped");
            } catch (error) {
                console.error("Error stopping SignalR:", error);
            }
        }
        this.isConnected = false;
    }
}

// ========== متغیر Global ==========
let backgroundJobSignalR = null;

// ========== شروع خودکار در صفحات CRM ==========
$(document).ready(function() {
    // فقط در CrmArea اجرا شود
    const currentArea = $('body').data('area');
    
    if (currentArea === 'CrmArea' || window.location.pathname.includes('/CrmArea/')) {
        // ایجاد و شروع اتصال SignalR
        backgroundJobSignalR = new BackgroundJobSignalR();
        backgroundJobSignalR.start();
    }
});

// ========== Cleanup در هنگام خروج از صفحه ==========
$(window).on('beforeunload', function() {
    if (backgroundJobSignalR) {
        backgroundJobSignalR.stop();
    }
});
