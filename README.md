# Quick start

### Prereqs
- .NET SDK (net10.0)
- SQL Server instance you can connect to

### Environment
This app reads the connection string from the environment key `ConnectionStrings__DefaultConnection`.
You should provide it via a `.env` file or your shell environment.

#### Example .env
```
ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=ssdp2600;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True
```