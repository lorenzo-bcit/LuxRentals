namespace LuxRentals.ViewModels.Cars.Admin;

public class MakeListItemVm
{
    public int MakeId { get; set; }

    public string MakeName { get; set; } = string.Empty;

    public bool CanDelete { get; set; }
}
