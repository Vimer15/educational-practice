using System;
using System.Collections.Generic;

namespace APIDemkaForVue.Model;

public partial class Item
{
    public string ArticuleItem { get; set; } = null!;

    public int TypeItem { get; set; }

    public string MeasurementItem { get; set; } = null!;

    public decimal PriceItem { get; set; }

    public int IdSupplierItem { get; set; }

    public int IdManufactureItem { get; set; }

    public int CategoryItem { get; set; }

    public int DiscountItem { get; set; }

    public int CountInStorageItem { get; set; }

    public string DescriptionItem { get; set; } = null!;

    public byte[]? PhotoItem { get; set; }

    public virtual ItemCategory CategoryItemNavigation { get; set; } = null!;

    public virtual Manufacture IdManufactureItemNavigation { get; set; } = null!;

    public virtual Supplier IdSupplierItemNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ItemType TypeItemNavigation { get; set; } = null!;
}
