using DotNetEnv.Configuration;
using LuxRentals.Data;
using LuxRentals.Data.Seeders;
using LuxRentals.Repositories.Cars;
using LuxRentals.Services;
using LuxRentals.Services.Cars;
using LuxRentals.Services.Payment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using LuxRentals.Repositories.Roles;
using LuxRentals.Services.ServiceSettings;
using LuxRentals.Repositories.Bookings;
using System.Net.Mail;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDotNetEnv();
builder.Services.AddSession();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<LuxRentalsDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<LuxRentalsDbContext>();

// Load .env into builder.Configuration
builder.Configuration.AddDotNetEnv();

// Bind PaypalOptions from configuration (.env variables)
builder.Services.Configure<PaypalOptions>(
    builder.Configuration.GetSection("PAYPAL")
);

// Register PayPal service
builder.Services.AddHttpClient<IPaymentService, PayPalPaymentService>();

builder.Services.AddControllersWithViews();

// Repositories
builder.Services.AddScoped<RoleRepo>();
builder.Services.AddScoped<UserRepo>();
builder.Services.AddScoped<UserRoleRepo>();
builder.Services.AddScoped<BookingRepo>();
builder.Services.AddScoped<BookingStatusRepo>();
builder.Services.AddScoped<ICarRepository, CarRepository>();

// Services
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<ICarImageStorage, CarImageStorage>();

builder.Services.Configure<ReCaptchaOptions>(
    builder.Configuration.GetSection("ReCaptcha"));

builder.Services.AddHttpClient<IReCaptchaService, ReCaptchaService>(client =>
{
    client.BaseAddress = new Uri("https://www.google.com");
});
builder.Services.AddHostedService<BookingCleanupService>();

// Configure email
var emailOptions = builder.Configuration
    .GetSection("Email")
    .Get<EmailOptions>() ?? throw new InvalidOperationException("Email configuration missing");

builder.Services
    .AddFluentEmail(emailOptions.From, emailOptions.Name)
    .AddSmtpSender(() => new SmtpClient(emailOptions.Host)
    {
        Port = emailOptions.Port,
        EnableSsl = true,
        Credentials = new NetworkCredential(
            emailOptions.Username,
            emailOptions.Password)
    });

builder.Services.AddTransient<IEmailSender, IdentityEmailSender>();

var app = builder.Build();

// Apply any pending migrations and seeding in dev mode
if (app.Environment.IsDevelopment())
{
    await app.ApplyPendingMigrationsAsync();
    await app.EnsureAdminSeededAsync();
    await app.EnsureCarCatalogSeededAsync();
}

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
