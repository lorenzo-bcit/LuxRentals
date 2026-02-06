namespace LuxRentals.Models;

public class Make
{
    public int PkMakeId { get; set; }

    public string MakeName { get; set; } = null!;

    public virtual ICollection<Model> Models { get; set; } = new List<Model>();
}
