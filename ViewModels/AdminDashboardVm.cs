namespace LuxRentals.ViewModels;

public class AdminDashboardVm
{
    public string AdminEmail { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int RoleCount { get; set; }
}
