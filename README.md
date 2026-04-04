# LuxRentals

ASP.NET Core MVC luxury car-rental app with Identity, EF Core, SQL Server, reCAPTCHA, SMTP email confirmation, and PayPal checkout.

## Core Workflows

- Public users can browse a luxury fleet and filter by booking dates, make, vehicle class, fuel type, transmission, seats, luggage capacity, and daily rate.
- Registration uses reCAPTCHA, creates both an Identity account and a linked `Customer` record, and assigns the new user to the `Customer` role.
- Sign-in requires confirmed email.
- Customers can maintain their profile, create bookings, complete checkout through PayPal, view their bookings, and cancel eligible bookings.
- Admins can view the dashboard, manage fleet data, manage makes/models/vehicle classes, manage roles, and assign or remove user roles.
- Admins can also review customer booking activity and cancel bookings on behalf of customers.

## Important Rules

- Booking dates are day-based. The app normalizes them to midnight UTC.
- The booking calendar uses Vancouver local time via `Utils/BookingClock.cs`.
- Earliest allowed pickup is tomorrow in the booking timezone.
- Customers can only cancel more than 2 days before pickup; `Admin` bypasses this rule.
- Public inventory only shows cars that are both `Available` (operationally) and free for the selected booking window.
- Price is currently `dailyRate * numberOfDays`.
- Booking checkout is validated on the server before capture: session order, customer, dates, and recalculated price must still match.

## Configuration

The app expects to load settings from a local `.env` file. Use the `.env.example` file in the repo root as the template.

## Setup

Prerequisites:

- .NET 10 SDK
- SQL Server
- Proper configuration via .env for database, external services, etc. (see `.env.example`)

## Install

1. Get the project onto your machine by cloning the repository or downloading/copying the project folder.
2. Open a terminal in the `LuxRentals` folder.
3. Copy `.env.example` to `.env`.
4. Edit `.env` and replace placeholders with real values.
5. Make sure `ConnectionStrings__DefaultConnection` points to a reachable SQL Server database.
6. Run:

```bash
dotnet restore
dotnet build
dotnet run
```

On first startup in `Development`, the app applies pending migrations automatically and seeds the required lookup data. If you are not running in `Development`, either enable `Bootstrap__AutoApplyMigrations=true` or run:

```bash
dotnet ef database update
```

Default local URLs from `Properties/launchSettings.json`:

- `http://localhost:5007`
- `https://localhost:7025`

## Startup Behavior

- In `Development`, pending EF Core migrations and demo fleet seeding run automatically.
- Outside `Development`, those only run when `Bootstrap:AutoApplyMigrations` or `Bootstrap:EnableDemoData` are enabled.
- Booking statuses, fuel types, car statuses, and core lookup data are always seeded if missing.
- Demo fleet data is optional outside `Development`.
- The admin user is only seeded when both `Bootstrap:AdminEmail` and `Bootstrap:AdminPassword` are provided and no existing admin already exists.

## Initial Identity Role Setup

- New registrations are assigned the `Customer` role automatically.
- On startup, the app ensures the `Admin` role exists.
- On startup, the app also creates the first admin user if:
  - `Bootstrap__AdminEmail` and `Bootstrap__AdminPassword` are set in `.env`
  - there is not already a user in the `Admin` role
- The seeded admin account is marked `EmailConfirmed = true`, so it can sign in immediately.

Example first admin login:

- Email: value of `Bootstrap__AdminEmail`
- Password: value of `Bootstrap__AdminPassword`

## Repo Layout

- `Program.cs`: startup and configuration
- `Data/`: EF Core context, migrations, seeders
- `Areas/Identity/`: auth pages
- `Areas/Admin/`: admin controllers and views
- `Controllers/`: browse, booking, payment, profile flows
- `Repositories/`: data access
- `Services/`: payment, email, reCAPTCHA, cleanup, image storage
- `Views/`: public MVC views
- `ViewModels/`: view models for MVC and Razor Pages
- `Utils/`: shared helper utilities
- `Styles/`: source styles for Tailwind build input
- `wwwroot/`: static assets, compiled CSS, JS, demo images, uploaded files
- `Properties/`: local launch settings
