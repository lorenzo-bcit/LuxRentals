using System;
using System.Collections.Generic;

namespace LuxRentals.Models;

public partial class Car
{
    public int PkCarId { get; set; }

    public string Colour { get; set; } = null!;

    public byte TransmissionType { get; set; }

    public int Year { get; set; }

    public string? CarThumbnail { get; set; }

    public string VinNumber { get; set; } = null!;

    public string LicencePlate { get; set; } = null!;

    public int PersonCap { get; set; }

    public int LuggageCap { get; set; }

    public decimal DailyRate { get; set; }

    public int FkVehicleClassId { get; set; }

    public int FkCarStatusId { get; set; }

    public int FkModelId { get; set; }

    public int FkFuelTypeId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual CarStatus FkCarStatus { get; set; } = null!;

    public virtual FuelType FkFuelType { get; set; } = null!;

    public virtual Model FkModel { get; set; } = null!;

    public virtual VehicleClass FkVehicleClass { get; set; } = null!;
}
