namespace LuxRentals.Models;

public class Model
{
    public int PkModelId { get; set; }

    public string ModelName { get; set; } = null!;

    public int FkMakeId { get; set; }

    public ICollection<Car> Cars { get; set; } = new List<Car>();

    public Make FkMake { get; set; } = null!;
}
