using System;
using System.Collections.Generic;

namespace APIDemkaForVue.Model;

public partial class Manufacture
{
    public int IdManufacture { get; set; }

    public string NameManufacture { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
