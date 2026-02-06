namespace LuxRentals.Models;

public class Model
{
    public int PkModelId { get; set; }

    public string ModelName { get; set; } = null!;

    public int FkMakeId { get; set; }

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

    public virtual Make FkMake { get; set; } = null!;
}
