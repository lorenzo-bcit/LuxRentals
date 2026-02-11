namespace LuxRentals.ViewModels.Profile;

public class CustomerProfileViewModel : BaseProfileViewModel
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string DriverLicenceNo { get; set; } = "";
    public bool? LicenceVerified { get; set; }
}
