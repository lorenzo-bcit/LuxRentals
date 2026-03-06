using DotNetEnv.Configuration;
using LuxRentals.Data;
using LuxRentals.Data.Seeders;
using LuxRentals.Repositories.Cars;
using LuxRentals.Models;
using LuxRentals.Repositories.Bookings;
using LuxRentals.Services;
using LuxRentals.Services.Cars;
using LuxRentals.Services.Payment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using LuxRentals.Repositories.Roles;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDotNetEnv();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<LuxRentalsDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<LuxRentalsDbContext>();

builder.Services.Configure<PaypalOptions>(builder.Configuration.GetSection("Paypal"));

builder.Services.AddHttpClient<IPaymentService, PayPalPaymentService>(client =>
{
    var paypalOptions = builder.Configuration.GetSection("Paypal").Get<PaypalOptions>()
        ?? throw new InvalidOperationException("PayPal configuration missing");

    client.BaseAddress = new Uri(paypalOptions.BaseUrl);
});

builder.Services.AddControllersWithViews();

// Repositories
builder.Services.AddScoped<ICarReadRepository, CarRepository>();
builder.Services.AddScoped<ICarWriteRepository, CarRepository>();
builder.Services.AddScoped<ICarLookupRepository, CarLookupRepository>();
builder.Services.AddScoped<RoleRepo>();
builder.Services.AddScoped<UserRepo>();
builder.Services.AddScoped<UserRoleRepo>();

// Services
builder.Services.AddScoped<ICarInventoryService, CarInventoryService>();

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

// Register repositories
builder.Services.AddScoped<BookingRepo>();

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


// TODO: Remove this for merging
static void SeedData(LuxRentalsDbContext context)
{
    // 1. Seed BookingStatus if empty
    if (!context.BookingStatuses.Any())
    {
        context.BookingStatuses.AddRange(
            new BookingStatus { BookingStatus1 = "unbooked" },
            new BookingStatus { BookingStatus1 = "booked" },
            new BookingStatus { BookingStatus1 = "cancelled" }
        );
        context.SaveChanges();
        Console.WriteLine("✓ BookingStatuses seeded");
    }

    // 2. Seed CarStatus if empty
    if (!context.CarStatuses.Any())
    {
        context.CarStatuses.AddRange(
            new CarStatus { },
            new CarStatus { },
            new CarStatus { }
        );
        context.SaveChanges();
        Console.WriteLine("✓ CarStatuses seeded");
    }

    // 3. Seed VehicleClass if empty
    if (!context.VehicleClasses.Any())
    {
        context.VehicleClasses.AddRange(
            new VehicleClass { VehicleClass1 = "Sedan" },
            new VehicleClass { VehicleClass1 = "SUV" },
            new VehicleClass { VehicleClass1 = "Truck" },
            new VehicleClass { VehicleClass1 = "Van" }
        );
        context.SaveChanges();
        Console.WriteLine("✓ VehicleClasses seeded");
    }

    // 4. Seed FuelType if empty
    if (!context.FuelTypes.Any())
    {
        context.FuelTypes.AddRange(
            new FuelType { FuelType1 = "Gasoline" },
            new FuelType { FuelType1 = "Diesel" },
            new FuelType { FuelType1 = "Electric" },
            new FuelType { FuelType1 = "Hybrid" }
        );
        context.SaveChanges();
        Console.WriteLine("✓ FuelTypes seeded");
    }

    // 5. Seed Makes if empty
    if (!context.Makes.Any())
    {
        context.Makes.AddRange(
            new Make { MakeName = "Honda" },
            new Make { MakeName = "Toyota" },
            new Make { MakeName = "Ford" }
        );
        context.SaveChanges();
        Console.WriteLine("✓ Makes seeded");
    }

    // 6. Seed Models if empty
    if (!context.Models.Any())
    {
        var honda = context.Makes.First(m => m.MakeName == "Honda");
        var toyota = context.Makes.First(m => m.MakeName == "Toyota");
        var ford = context.Makes.First(m => m.MakeName == "Ford");

        context.Models.AddRange(
            new Model { ModelName = "Accord", FkMakeId = honda.PkMakeId },
            new Model { ModelName = "Camry", FkMakeId = toyota.PkMakeId },
            new Model { ModelName = "F-150", FkMakeId = ford.PkMakeId }
        );
        context.SaveChanges();
        Console.WriteLine("✓ Models seeded");
    }

    // 7. Seed Cars if empty
    if (!context.Cars.Any())
    {
        var accordModel = context.Models.First(m => m.ModelName == "Accord");
        var camryModel = context.Models.First(m => m.ModelName == "Camry");
        var f150Model = context.Models.First(m => m.ModelName == "F-150");

        var availableStatus = context.CarStatuses.First();

        var sedanClass = context.VehicleClasses.First(vc => vc.VehicleClass1 == "Sedan");
        var truckClass = context.VehicleClasses.First(vc => vc.VehicleClass1 == "Truck");

        var gasoline = context.FuelTypes.First(ft => ft.FuelType1 == "Gasoline");
        var hybrid = context.FuelTypes.First(ft => ft.FuelType1 == "Hybrid");

        context.Cars.AddRange(
            new Car
            {
                Colour = "Silver",
                TransmissionType = 1,
                Year = 2023,
                CarThumbnail = null,
                VinNumber = "1HGCM82633A123456",
                LicencePlate = "ABC-1234",
                PersonCap = 5,
                LuggageCap = 3,
                DailyRate = 59.99m,
                FkVehicleClassId = sedanClass.PkVehicleClassId,
                FkCarStatusId = availableStatus.PkCarStatusId,
                FkModelId = accordModel.PkModelId,
                FkFuelTypeId = gasoline.PkFuelTypeId
            },
            new Car
            {
                Colour = "White",
                TransmissionType = 1,
                Year = 2024,
                CarThumbnail = null,
                VinNumber = "4T1BF1FK5CU123456",
                LicencePlate = "XYZ-5678",
                PersonCap = 5,
                LuggageCap = 4,
                DailyRate = 69.99m,
                FkVehicleClassId = sedanClass.PkVehicleClassId,
                FkCarStatusId = availableStatus.PkCarStatusId,
                FkModelId = camryModel.PkModelId,
                FkFuelTypeId = hybrid.PkFuelTypeId
            },
            new Car
            {
                Colour = "Blue",
                TransmissionType = 1,
                Year = 2023,
                CarThumbnail = null,
                VinNumber = "1FTFW1E84NFA12345",
                LicencePlate = "TRK-9999",
                PersonCap = 5,
                LuggageCap = 2,
                DailyRate = 89.99m,
                FkVehicleClassId = truckClass.PkVehicleClassId,
                FkCarStatusId = availableStatus.PkCarStatusId,
                FkModelId = f150Model.PkModelId,
                FkFuelTypeId = gasoline.PkFuelTypeId
            }
        );
        context.SaveChanges();
        Console.WriteLine("✓ Cars seeded");
    }

    Console.WriteLine("✓ Database seeding completed!");
}