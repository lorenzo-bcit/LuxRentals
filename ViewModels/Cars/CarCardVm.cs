using LuxRentals.Models;

namespace LuxRentals.ViewModels.Cars;

public class CarCardVm
{
    public int CarId { get; set; }
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Transmission { get; set; } = "";
    public string FuelType { get; set; } = "";
    public decimal DailyRate { get; set; }
    public int Seats { get; set; }
    public int Luggage { get; set; }
    public string? Thumbnail { get; set; }

    public static CarCardVm FromEntity(Car c) => new()
    {
        CarId = c.PkCarId,
        Title = $"{c.Year} {c.FkModel.FkMake.MakeName} {c.FkModel.ModelName}",
        Subtitle = c.FkVehicleClass.VehicleClass1,
        Transmission = c.TransmissionType == 1 ? "Automatic" : "Manual",
        FuelType = c.FkFuelType.FuelType1,
        DailyRate = c.DailyRate,
        Seats = c.PersonCap,
        Luggage = c.LuggageCap,
        Thumbnail = c.CarThumbnail
    };
}
