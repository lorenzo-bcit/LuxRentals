using LuxRentals.Models;

namespace LuxRentals.ViewModels.Cars.Admin;

public class AdminCarListItemVm
{
    public int CarId { get; set; }
    public string DisplayName { get; set; } = "";
    public string VehicleClass { get; set; } = "";
    public string Status { get; set; } = "";
    public string LicencePlate { get; set; } = "";
    public string VinNumber { get; set; } = "";
    public string Transmission { get; set; } = "";
    public string FuelType { get; set; } = "";
    public decimal DailyRate { get; set; }

    public static AdminCarListItemVm FromEntity(Car car) => new()
    {
        CarId = car.PkCarId,
        DisplayName = $"{car.Year} {car.FkModel.FkMake.MakeName} {car.FkModel.ModelName}",
        VehicleClass = car.FkVehicleClass.VehicleClass1,
        Status = car.FkCarStatus.StatusFlag,
        LicencePlate = car.LicencePlate,
        VinNumber = car.VinNumber,
        Transmission = car.TransmissionType == 1 ? "Automatic" : "Manual",
        FuelType = car.FkFuelType.FuelType1,
        DailyRate = car.DailyRate
    };
}
