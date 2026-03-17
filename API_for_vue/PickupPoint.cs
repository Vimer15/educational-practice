using System;
using System.Collections.Generic;

namespace APIDemkaForVue.Model;

public partial class PickupPoint
{
    public int IdPickupPoint { get; set; }

    public string IndexPickupPoint { get; set; } = null!;

    public string CityPickupPoint { get; set; } = null!;

    public string DistrictPickupPoint { get; set; } = null!;

    public virtual ICollection<OrderImport> OrderImports { get; set; } = new List<OrderImport>();
}
