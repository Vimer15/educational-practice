using System;
using System.Collections.Generic;

namespace Demka.Model;

public partial class ItemType
{
    public int IdItemType { get; set; }

    public string NameItemType { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
