namespace LuxRentals.Models;

public class VehicleClass
{
    public int PkVehicleClassId { get; set; }

    public string VehicleClass1 { get; set; } = null!;

    public ICollection<Car> Cars { get; set; } = new List<Car>();
}
