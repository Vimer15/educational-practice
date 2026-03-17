using System;
using System.Collections.Generic;

namespace APIDemkaForVue.Model;

public partial class Supplier
{
    public int IdSupplier { get; set; }

    public string NameSupplier { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
