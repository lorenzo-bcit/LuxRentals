# LuxRentals

ASP.NET Core MVC car-rental application with:

- ASP.NET Core Identity authentication and roles
- Entity Framework Core with SQL Server
- Google reCAPTCHA on registration
- PayPal checkout flow
- Email confirmation for new accounts

## Prerequisites

- .NET 10 SDK
- SQL Server
- A `.env` file with valid secrets

## Repository

GitHub remote configured in this repo:

- `git@github.com:lorenzo-bcit/LuxRentals.git`

## Secret Configuration

The app loads secrets from a local `.env` file.

Copy `.env.example` into `.env` in the project root and define proper variables.

## Database Setup

Development startup applies pending EF Core migrations automatically.

If you want to create or update the database manually:

```bash
dotnet ef database update
```

## Run The App

```bash
dotnet restore
dotnet build
dotnet run
```

Then open the local URL shown in the terminal.

## Identity And Roles

- Registration requires email confirmation before sign-in.
- New registrations are assigned the `Customer` role automatically.
- Admin-only role management is available in the app.

### Default Admin Account

In `Development`, the app seeds one admin account on startup if no admin exists:

- Email: `admin@example.com`
- Password: `Admin123!`

This comes from `Data/Seeders/AdminSeeder.cs`.

## Core User Flows

- Public users can browse the landing page and vehicle inventory.
- Customers can register, confirm email, log in, create bookings, pay with PayPal, and view/cancel bookings.
- Admins can manage cars, roles, and user-role assignments.

## Project Structure

- `Program.cs`: service registration and app startup
- `Data/`: EF Core context, migrations, and seeders
- `Areas/Identity/`: login and registration pages
- `Areas/Admin/`: admin car-management area
- `Controllers/`: public MVC controllers
- `Repositories/`: data-access logic
- `Services/`: payment, email, captcha, and other services
- `ViewModels/`: view models used by MVC/Razor pages
