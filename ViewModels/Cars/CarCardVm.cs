using LuxRentals.Models;

namespace LuxRentals.ViewModels.Cars;

public class CarCardVm
{
    public int CarId { get; set; }
    public string Title { get; set; } = "";
    public decimal DailyRate { get; set; }
    public int Seats { get; set; }
    public int Luggage { get; set; }
    public string? Thumbnail { get; set; }

    public static CarCardVm FromEntity(Car c) => new()
    {
        CarId = c.PkCarId,
        Title = $"{c.Year} {c.FkModel.FkMake.MakeName} {c.FkModel.ModelName}",
        DailyRate = c.DailyRate,
        Seats = c.PersonCap,
        Luggage = c.LuggageCap,
        Thumbnail = c.CarThumbnail
    };
}
