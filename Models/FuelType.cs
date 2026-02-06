namespace LuxRentals.Models;

public class FuelType
{
    public int PkFuelTypeId { get; set; }

    public string FuelType1 { get; set; } = null!;

    public ICollection<Car> Cars { get; set; } = new List<Car>();
}
