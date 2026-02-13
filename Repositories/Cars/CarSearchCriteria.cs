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

    public int Page { get; set; }
    public int PageSize { get; set; }
}