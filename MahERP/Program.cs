using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using MahERP.DataModelLayer;
using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;
using MahERP.DataModelLayer.Services;
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

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IStakeholderRepository, StakeholderRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
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

builder.Services.AddHttpClient(); // اگر قبلاً ثبت نشده است
builder.Services.AddMemoryCache(); // اگر قبلاً ثبت نشده است
builder.Services.AddScoped<PersianDateHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    //app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        //Re-execute the request so the user gets the error page
        string originalPath = ctx.Request.Path.Value;
        ctx.Items["originalPath"] = originalPath;
        ctx.Request.Path = "/Error404";
        await next();
    }
});
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseWebOptimizer();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    //AdminArea
    endpoints.MapAreaControllerRoute(
        "AdminArea",
        "AdminArea",
        "AdminArea/{controller=Dashboard}/{action=Index}/{id?}",
        defaults: new { Controller = "Dashboard", Action = "Index" });

    //Default
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "/{controller=Account}/{action=Login}/{id?}");

    endpoints.MapControllerRoute(
        name: "Login",
        pattern: "/{controller=Account}/{action=Login}/{ReturnUrl?}");

});

app.Run();
