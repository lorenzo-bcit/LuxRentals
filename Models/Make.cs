namespace LuxRentals.Models;

public class Make
{
    public int PkMakeId { get; set; }

    public string MakeName { get; set; } = null!;

    public ICollection<Model> Models { get; set; } = new List<Model>();
}
