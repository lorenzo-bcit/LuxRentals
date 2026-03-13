using System.ComponentModel.DataAnnotations;
using LuxRentals.Models;

namespace LuxRentals.ViewModels.Cars;

public class CarUpsertVm
{
    public int? CarId { get; set; }

    [Display(Name = "Color")]
    [Required, StringLength(40)]
    public string Colour { get; set; } = "";

    [Display(Name = "Transmission")]
    [Required]
    public byte TransmissionType { get; set; }

    [Range(1886, 2100)]
    public int Year { get; set; }

    [Display(Name = "Thumbnail URL")]
    [StringLength(255)]
    public string? CarThumbnail { get; set; }

    [Display(Name = "VIN")]
    [Required, StringLength(17, MinimumLength = 17)]
    public string VinNumber { get; set; } = "";

    [Display(Name = "Licence Plate")]
    [Required, StringLength(10)]
    public string LicencePlate { get; set; } = "";

    [Display(Name = "Seats")]
    [Range(1, 20)]
    public int PersonCap { get; set; }

    [Display(Name = "Luggage")]
    [Range(0, 50)]
    public int LuggageCap { get; set; }

    [Display(Name = "Daily Rate")]
    [Range(typeof(decimal), "0.01", "9999999")]
    public decimal DailyRate { get; set; }

    [Display(Name = "Vehicle Class")]
    [Required]
    public int FkVehicleClassId { get; set; }

    [Display(Name = "Status")]
    [Required]
    public int FkCarStatusId { get; set; }

    [Display(Name = "Model")]
    [Required]
    public int FkModelId { get; set; }

    [Display(Name = "Fuel Type")]
    [Required]
    public int FkFuelTypeId { get; set; }

    public void ApplyToEntity(Car car)
    {
        car.Colour = Colour;
        car.TransmissionType = TransmissionType;
        car.Year = Year;
        car.CarThumbnail = CarThumbnail;
        car.VinNumber = VinNumber;
        car.LicencePlate = LicencePlate;
        car.PersonCap = PersonCap;
        car.LuggageCap = LuggageCap;
        car.DailyRate = DailyRate;

        car.FkVehicleClassId = FkVehicleClassId;
        car.FkCarStatusId = FkCarStatusId;
        car.FkModelId = FkModelId;
        car.FkFuelTypeId = FkFuelTypeId;
    }

    public static CarUpsertVm FromEntity(Car car) => new()
    {
        CarId = car.PkCarId,
        Colour = car.Colour,
        TransmissionType = car.TransmissionType,
        Year = car.Year,
        CarThumbnail = car.CarThumbnail,
        VinNumber = car.VinNumber,
        LicencePlate = car.LicencePlate,
        PersonCap = car.PersonCap,
        LuggageCap = car.LuggageCap,
        DailyRate = car.DailyRate,
        FkVehicleClassId = car.FkVehicleClassId,
        FkCarStatusId = car.FkCarStatusId,
        FkModelId = car.FkModelId,
        FkFuelTypeId = car.FkFuelTypeId
    };
}