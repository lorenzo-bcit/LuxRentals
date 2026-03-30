namespace LuxRentals.Data.Seeding;

public class SeedDataOptions
{
    public bool EnableDemoData { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
