namespace LuxRentals.ViewModels.Cars.Admin;

public class AdminModelListItemVm
{
    public int ModelId { get; set; }

    public int MakeId { get; set; }

    public string MakeName { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public bool CanDelete { get; set; }
}
