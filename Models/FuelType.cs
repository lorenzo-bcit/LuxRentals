using System;
using System.Collections.Generic;

namespace LuxRentals.Models;

public partial class FuelType
{
    public int PkFuelTypeId { get; set; }

    public string FuelType1 { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
