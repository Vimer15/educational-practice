using System;
using System.Collections.Generic;

namespace Demka.Model;

public partial class OrderImport
{
    public int IdOrderImport { get; set; }

    public DateOnly DateOrderImport { get; set; }

    public DateOnly DateOfDeliveryOrderImport { get; set; }

    public int IdPickupPointOrderImport { get; set; }

    public int IdClientOrderImport { get; set; }

    public string CodeOrderImport { get; set; } = null!;

    public string StatusOrderImport { get; set; } = null!;

    public virtual UserImport IdClientOrderImportNavigation { get; set; } = null!;

    public virtual PickupPoint IdPickupPointOrderImportNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
