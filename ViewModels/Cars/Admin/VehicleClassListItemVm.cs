namespace LuxRentals.ViewModels.Cars.Admin;

public class VehicleClassListItemVm
{
    public int VehicleClassId { get; set; }

    public string VehicleClassName { get; set; } = string.Empty;

    public bool CanDelete { get; set; }
}
