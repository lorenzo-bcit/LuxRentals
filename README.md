# Quick start

### Prereqs
- .NET SDK (net10.0)
- SQL Server instance you can connect to

### Environment
Configure environment variables via a `.env` file or your shell environment.

Setup .env
1) Copy `.env.example` to `.env`.
2) Replace the placeholder values with real secrets and settings.

### Setup

## Setup on Windows (without Docker)
1) Grab server name from SQL Server Management Studio
2) Paste server name in here:
`'Server=PASTE_SERVER_NAME_HERE;Database=LuxRentals;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;'`
3) Run the following: `dotnet ef database update`
4) Database should now appear in SQL Server Management Studio!


