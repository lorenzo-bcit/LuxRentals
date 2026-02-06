using System;
using System.Collections.Generic;

namespace LuxRentals.Models;

public partial class VehicleClass
{
    public int PkVehicleClassId { get; set; }

    public string VehicleClass1 { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
