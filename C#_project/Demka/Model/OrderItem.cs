using System;
using System.Collections.Generic;

namespace Demka.Model;

public partial class OrderItem
{
    public int IdOrderItem { get; set; }

    public string ArticulItem { get; set; } = null!;

    public int CountItem { get; set; }

    public virtual Item ArticulItemNavigation { get; set; } = null!;

    public virtual OrderImport IdOrderItemNavigation { get; set; } = null!;
}
