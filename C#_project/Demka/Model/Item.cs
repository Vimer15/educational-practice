using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace Demka.Model;

public partial class Item
{
    public string ArticuleItem { get; set; } = null!;

    public int TypeItem { get; set; }

    public string MeasurementItem { get; set; } = null!;

    public string Measurement
    {
        get
        {
            return $"Единица измерения: {MeasurementItem}";
        }
    }

    public decimal PriceItem { get; set; }
    public string PriceColor
    {
        get
        {
            return (DiscountItem > 0) ? "Red" : "none";
        }
    }
    public string PriceStyle
    {
        get
        {
            return (DiscountItem > 0) ? "Strikethrough" : "none";
        }
    }
    public string Price
    {
        get
        {
            return $"{PriceItem * (100 - DiscountItem) / 100}";
        }
    }

    public int IdSupplierItem { get; set; }
    public string Supplier
    {
        get { return $"Поставщик: {IdSupplierItemNavigation.NameSupplier}"; }
    }

    public int IdManufactureItem { get; set; }
    public string Manufacture
    {
        get { return $"Производитель: {IdManufactureItemNavigation.NameManufacture}"; }
    }

    public int CategoryItem { get; set; }
    public string CategoryName { 
        get {
                return $"{CategoryItemNavigation.NameItemCategory} | {TypeItemNavigation.NameItemType}"; 
        } 
    
    }

    public int DiscountItem { get; set; }

    public string Discount
    {
        get
        {
            return $"Действующая скидка: {DiscountItem}%";
        }
    }

    public string DiscountColor
    {
        get
        {
            return (DiscountItem > 15) ? "#2E8B57" : "none";
        }
    }

    public int CountInStorageItem { get; set; }
    public string CountInStorage
    {
        get
        {
            return $"Количество на складе: {CountInStorageItem}";
        }
    }

    public string DescriptionItem { get; set; } = null!;
    public string Description
    {
        get { return $"Описание товара: {DescriptionItem}"; }
    }
    public string CountInStorageStyle
    {
        get
        {
            return (CountInStorageItem == 0) ? "Cyan" : "none";
        }
    }

    public byte[]? PhotoItem { get; set; }

    public BitmapImage Photo
    {
        get
        {
            BitmapImage image = new BitmapImage();
            Uri baseUrl = new Uri("../images/picture.png", UriKind.Relative);
            image.BeginInit();
            if (PhotoItem == null)
            {
                image.UriSource = baseUrl;
            }
            else
            {
                MemoryStream mem = new MemoryStream(PhotoItem);
                image.StreamSource = mem;
            }
            image.EndInit();
            return image;
        }
    }


    public virtual ItemCategory CategoryItemNavigation { get; set; } = null!;

    public virtual Manufacture IdManufactureItemNavigation { get; set; } = null!;

    public virtual Supplier IdSupplierItemNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ItemType TypeItemNavigation { get; set; } = null!;
}
