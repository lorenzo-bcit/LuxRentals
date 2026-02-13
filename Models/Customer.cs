namespace LuxRentals.Models;
public class Customer
{
    public int PkCustomerId { get; set; }

    public string UserId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string DriverLicenceNo { get; set; } = null!;

    public bool? LicenceVerified { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
