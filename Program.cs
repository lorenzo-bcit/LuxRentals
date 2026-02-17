using DotNetEnv.Configuration;
using LuxRentals.Data;
using LuxRentals.Extensions;
using LuxRentals.Models;
using LuxRentals.Services;
using LuxRentals.Services.PaymentService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDotNetEnv();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<LuxRentalsDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<LuxRentalsDbContext>();

builder.Services.Configure<PaypalOptions>(
builder.Configuration.GetSection("Paypal"));

builder.Services.AddHttpClient<IPaymentService, PayPalPaymentService>(client =>
{
    var paypalOptions = builder.Configuration.GetSection("Paypal").Get<PaypalOptions>() ?? throw new InvalidOperationException("PayPal configuration missing");
    client.BaseAddress = new Uri(paypalOptions.BaseUrl);
});

builder.Services.AddScoped<IPaymentService, PayPalPaymentService>();



builder.Services.AddControllersWithViews();

builder.Configuration.AddDotNetEnv();

builder.Services.Configure<ReCaptchaOptions>(
    builder.Configuration.GetSection("ReCaptcha"));

builder.Services.AddHttpClient<IReCaptchaService, ReCaptchaService>(client =>
{
    client.BaseAddress = new Uri("https://www.google.com");
});

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
}

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();