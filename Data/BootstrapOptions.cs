namespace LuxRentals.Data;

public class BootstrapOptions
{
    public bool AutoApplyMigrations { get; set; }
    public bool EnableDemoData { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
