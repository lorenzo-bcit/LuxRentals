namespace LuxRentals.Models;

public class CarStatus
{
    public int PkCarStatusId { get; set; }

    public string StatusFlag { get; set; } = null!;

    public ICollection<Car> Cars { get; set; } = new List<Car>();
}
