using System.Collections.Generic;

namespace LuxRentals.ViewModels.Profile;

public class AdminProfileViewModel
{
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public List<string> Roles { get; set; } = new();
}
