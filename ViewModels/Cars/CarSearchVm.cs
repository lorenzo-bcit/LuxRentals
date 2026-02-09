namespace LuxRentals.ViewModels.Cars;

public class CarSearchVm
{
    public int? FuelTypeId { get; set; }
    public int? VehicleClassId { get; set; }
    public int? TransmissionType { get; set; }
    public int? MinSeats { get; set; }
    public int? MinLuggage { get; set; }
    public bool AvailableOnly { get; set; } = true;

}
