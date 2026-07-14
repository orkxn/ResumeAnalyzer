using ResumeAnalyzer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddMemoryCache();

// dependency-injection için
builder.Services.AddScoped<TextExtractorService>();
builder.Services.AddScoped<GoogleDriveService>();
builder.Services.AddHttpClient<AiAnalysisService>();
builder.Services.AddScoped<ResumeService>();
builder.Services.AddScoped<AuthService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("UploadPolicy", httpContext =>
    {
        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? httpContext.Connection.RemoteIpAddress?.ToString() 
            ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: userId,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 3,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.AddPolicy("GeneralPolicy", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: ip,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                QueueLimit = 0,
                SegmentsPerWindow = 6,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("Çok fazla istek gönderdiniz. Lütfen daha sonra tekrar deneyin.", token);
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler("/Home/Error");

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Uygulama başlarken bekleyen veritabanı migrasyonlarını otomatik uygular (Zaten güncelse işlem yapmaz)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
}

app.Run();
