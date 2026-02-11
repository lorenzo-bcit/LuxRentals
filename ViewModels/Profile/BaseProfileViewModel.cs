namespace LuxRentals.ViewModels.Profile;

public class BaseProfileViewModel
{
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public List<string> Roles { get; set; } = new();
}
