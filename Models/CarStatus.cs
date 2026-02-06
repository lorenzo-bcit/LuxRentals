using System;
using System.Collections.Generic;

namespace LuxRentals.Models;

public partial class CarStatus
{
    public int PkCarStatusId { get; set; }

    public string StatusFlag { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
