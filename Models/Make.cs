using System;
using System.Collections.Generic;

namespace LuxRentals.Models;

public partial class Make
{
    public int PkMakeId { get; set; }

    public string MakeName { get; set; } = null!;

    public virtual ICollection<Model> Models { get; set; } = new List<Model>();
}
