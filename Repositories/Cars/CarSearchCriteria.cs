namespace LuxRentals.Repositories.Cars;

public class CarSearchCriteria
{
    public bool AvailableOnly { get; set; }

    public int? FuelTypeId { get; set; }
    public int? VehicleClassId { get; set; }
    public byte? TransmissionType { get; set; }
    public int? MinSeats { get; set; }
    public int? MinLuggage { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}