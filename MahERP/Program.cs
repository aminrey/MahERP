using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Extensions;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Repository.ContactRepository;
using MahERP.DataModelLayer.Repository.MyDayTaskRepository;
using MahERP.DataModelLayer.Repository.OrganizationRepository;
using MahERP.DataModelLayer.Repository.Tasking;
using MahERP.DataModelLayer.Repository.TaskRepository;
using MahERP.DataModelLayer.Repository.TaskRepository.Tasking;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.Services.BackgroundServices;
using MahERP.Hubs;
using MahERP.Services;
using MahERP.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(
    option => option.UseSqlServer(builder.Configuration.GetConnectionString("MahERPConnectionString"),
    datamodel => datamodel.MigrationsAssembly("MahERP.DataModelLayer")));

//Identity Service
builder.Services.AddIdentity<AppUsers, AppRoles>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

// Repository Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IStakeholderRepository, StakeholderRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ICRMRepository, CRMRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>(); // اضافه شده
builder.Services.AddScoped<IUserManagerRepository,UserManagerRepository>(); 
builder.Services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>(); // اضافه شده
builder.Services.AddScoped<ICoreNotificationRepository, CoreNotificationRepository>(); // سیستم نوتیفیشن کلی
builder.Services.AddScoped<TaskCodeGenerator>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IMainDashboardRepository, MainDashboardRepository>();
builder.Services.AddScoped<ITaskVisibilityRepository, TaskVisibilityRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStakeholderRepository, StakeholderRepository>();
builder.Services.AddScoped<ITaskFilterRepository, TaskFilterRepository>();
builder.Services.AddScoped<ITaskOperationsRepository, TaskOperationsRepository>(); // ⭐ جدید
builder.Services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
builder.Services.AddScoped<IMyDayTaskRepository, MyDayTaskRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddHostedService<ExpiredRoleCleanupService>();

// اضافه کردن SmsProviderRepository
builder.Services.AddScoped<SmsProviderRepository>();


builder.Services.AddHostedService<SmsBackgroundService>();
builder.Services.AddHostedService<SmsDeliveryCheckService>();
builder.Services.AddHostedService<EmailBackgroundService>();


builder.Services.AddScoped<ITaskVisibilityRepository, TaskVisibilityRepository>();


// ⭐ اضافه کنید - SMS Services:
builder.Services.AddScoped<ISmsProviderRepository, SmsProviderRepository>();
builder.Services.AddScoped<ISmsQueueRepository, SmsQueueRepository>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<ISmsTemplateRepository, SmsTemplateRepository>();

// ⭐ اضافه کنید - Email Services:
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
builder.Services.AddScoped<IEmailQueueRepository, EmailQueueRepository>();
builder.Services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();


// 1️⃣ EmailService (وابستگی اصلی EmailRepository)
builder.Services.AddScoped<EmailService>();

// 2️⃣ Logger برای EmailRepository (در صورت نیاز)
builder.Services.AddLogging();

// 3️⃣ اگر از EmailSettings استفاده می‌کنید:

// ⭐ Services - Scoped
builder.Services.AddScoped<TaskNotificationService>();
builder.Services.AddScoped<TaskCodeGenerator>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<NotificationBackgroundService>();

// ⭐ Background Service - Singleton + Hosted
builder.Services.AddSingleton<NotificationBackgroundService>();
builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<NotificationBackgroundService>());

// اضافه کردن سرویس background برای نوتیفیکیشن‌ها

// اضافه کردن Scoped Service برای TransactionManager
builder.Services.AddScoped<TransactionManager>();
builder.Services.AddSignalR();
// Activity Logger Service
builder.Services.AddScoped<ActivityLoggerService>(); // اضافه شده
builder.Services.AddScoped<TaskNotificationService>(); // سرویس نوتیفیکیشن تسک‌ها
builder.Services.AddHttpContextAccessor(); // اضافه شده


builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 5;
});

builder.Services.AddMvc();
builder.Services.AddWebOptimizer();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
    options.MaxRequestBodySize = int.MaxValue;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/";
    //configure your other properties
});

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<PersianDateHelper>();

// ✅ Register NEW Permission Services
builder.Services.AddScoped<IPermissionService, PermissionRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionRepository>();


var app = builder.Build();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseWebOptimizer();

app.UseRouting();
app.UseSession();
app.MapHub<NotificationHub>("/notificationHub");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
