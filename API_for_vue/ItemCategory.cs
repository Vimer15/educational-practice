using System;
using System.Collections.Generic;

namespace APIDemkaForVue.Model;

public partial class ItemCategory
{
    public int IdItemCategory { get; set; }

    public string NameItemCategory { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
