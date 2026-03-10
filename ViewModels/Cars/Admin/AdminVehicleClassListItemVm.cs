namespace LuxRentals.ViewModels.Cars.Admin;

public class AdminVehicleClassListItemVm
{
    public int VehicleClassId { get; set; }

    public string VehicleClassName { get; set; } = string.Empty;

    public bool CanDelete { get; set; }
}
