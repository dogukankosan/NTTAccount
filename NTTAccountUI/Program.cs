using NTTAccountUI.Data;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Middleware;
using NTTAccountUI.Security;
using NTTAccountUI.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
// Dapper
builder.Services.AddSingleton<DapperContext>();
// Servisler
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAdminSessionRepository, AdminSessionRepository>();
builder.Services.AddScoped<ISiteSettingsRepository, SiteSettingsRepository>();
builder.Services.AddScoped<IBannerSlideRepository, BannerSlideRepository>();
builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
builder.Services.AddScoped<IMailSettingsRepository, MailSettingsRepository>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(30);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}
app.UseStatusCodePagesWithRedirects("/Error/{0}");
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseMiddleware<AdminAuthMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();