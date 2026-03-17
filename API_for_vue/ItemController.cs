using APIDemkaForVue.DTO;
using APIDemkaForVue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static APIDemkaForVue.DTO.ItemApiDTO;

namespace APIDemkaForVue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems()
        {
            var items = await ShoeContext.Context.Items
                .Include(i => i.CategoryItemNavigation)
                .Include(i => i.IdManufactureItemNavigation)
                .Include(i => i.IdSupplierItemNavigation)
                .Include(i => i.TypeItemNavigation)
                .Select(i => new ItemDTO
                {
                    ArticuleItem = i.ArticuleItem,
                    TypeItem = i.TypeItem,
                    MeasurementItem = i.MeasurementItem,
                    Measurement = $"Единица измерения: {i.MeasurementItem}",
                    PriceItem = i.PriceItem,
                    PriceColor = i.DiscountItem > 0 ? "Red" : "none",
                    PriceStyle = i.DiscountItem > 0 ? "Strikethrough" : "none",
                    Price = Math.Round(i.PriceItem * (100 - i.DiscountItem) / 100, 2),
                    PriceWithDiscount = Math.Round(i.PriceItem * (100 - i.DiscountItem) / 100, 2),
                    OriginalPrice = i.PriceItem,
                    IdSupplierItem = i.IdSupplierItem,
                    Supplier = $"Поставщик: {i.IdSupplierItemNavigation.NameSupplier}",
                    IdManufactureItem = i.IdManufactureItem,
                    Manufacture = $"Производитель: {i.IdManufactureItemNavigation.NameManufacture}",
                    CategoryItem = i.CategoryItem,
                    CategoryName = $"{i.CategoryItemNavigation.NameItemCategory} | {i.TypeItemNavigation.NameItemType}",
                    DiscountItem = i.DiscountItem,
                    Discount = $"Действующая скидка: {i.DiscountItem}%",
                    DiscountColor = i.DiscountItem > 15 ? "#2E8B57" : "none",
                    CountInStorageItem = i.CountInStorageItem,
                    CountInStorage = $"Количество на складе: {i.CountInStorageItem}",
                    DescriptionItem = i.DescriptionItem,
                    Description = $"Описание товара: {i.DescriptionItem}",
                    CountInStorageStyle = i.CountInStorageItem == 0 ? "Cyan" : "none",
                    PhotoItem = i.PhotoItem,
                    Category = new CategoryDto
                    {
                        Id = i.CategoryItemNavigation.IdItemCategory,
                        Name = i.CategoryItemNavigation.NameItemCategory
                    },
                    Manufacturer = new ManufacturerDto
                    {
                        Id = i.IdManufactureItemNavigation.IdManufacture,
                        Name = i.IdManufactureItemNavigation.NameManufacture
                    },
                    SupplierInfo = new SupplierDto
                    {
                        Id = i.IdSupplierItemNavigation.IdSupplier,
                        Name = i.IdSupplierItemNavigation.NameSupplier
                    },
                    Type = new ItemTypeDto
                    {
                        Id = i.TypeItemNavigation.IdItemType,
                        Name = i.TypeItemNavigation.NameItemType
                    },
                    IsInStock = i.CountInStorageItem > 0,
                    HasDiscount = i.DiscountItem > 0,
                    DiscountAmount = Math.Round(i.PriceItem * i.DiscountItem / 100, 2)
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("GetItem/{articule}")]
        public async Task<ActionResult<ItemDTO>> GetItem(string articule)
        {
            var item = await ShoeContext.Context.Items
                .Include(i => i.CategoryItemNavigation)
                .Include(i => i.IdManufactureItemNavigation)
                .Include(i => i.IdSupplierItemNavigation)
                .Include(i => i.TypeItemNavigation)
                .Where(i => i.ArticuleItem == articule)
                .Select(i => new ItemDTO
                {
                    ArticuleItem = i.ArticuleItem,
                    TypeItem = i.TypeItem,
                    MeasurementItem = i.MeasurementItem,
                    Measurement = $"Единица измерения: {i.MeasurementItem}",
                    PriceItem = i.PriceItem,
                    PriceColor = i.DiscountItem > 0 ? "Red" : "none",
                    PriceStyle = i.DiscountItem > 0 ? "Strikethrough" : "none",
                    Price = Math.Round(i.PriceItem * (100 - i.DiscountItem) / 100, 2),
                    PriceWithDiscount = Math.Round(i.PriceItem * (100 - i.DiscountItem) / 100, 2),
                    OriginalPrice = i.PriceItem,
                    IdSupplierItem = i.IdSupplierItem,
                    Supplier = $"Поставщик: {i.IdSupplierItemNavigation.NameSupplier}",
                    IdManufactureItem = i.IdManufactureItem,
                    Manufacture = $"Производитель: {i.IdManufactureItemNavigation.NameManufacture}",
                    CategoryItem = i.CategoryItem,
                    CategoryName = $"{i.CategoryItemNavigation.NameItemCategory} | {i.TypeItemNavigation.NameItemType}",
                    DiscountItem = i.DiscountItem,
                    Discount = $"Действующая скидка: {i.DiscountItem}%",
                    DiscountColor = i.DiscountItem > 15 ? "#2E8B57" : "none",
                    CountInStorageItem = i.CountInStorageItem,
                    CountInStorage = $"Количество на складе: {i.CountInStorageItem}",
                    DescriptionItem = i.DescriptionItem,
                    Description = $"Описание товара: {i.DescriptionItem}",
                    CountInStorageStyle = i.CountInStorageItem == 0 ? "Cyan" : "none",
                    PhotoItem = i.PhotoItem,
                    Category = new CategoryDto
                    {
                        Id = i.CategoryItemNavigation.IdItemCategory,
                        Name = i.CategoryItemNavigation.NameItemCategory
                    },
                    Manufacturer = new ManufacturerDto
                    {
                        Id = i.IdManufactureItemNavigation.IdManufacture,
                        Name = i.IdManufactureItemNavigation.NameManufacture
                    },
                    SupplierInfo = new SupplierDto
                    {
                        Id = i.IdSupplierItemNavigation.IdSupplier,
                        Name = i.IdSupplierItemNavigation.NameSupplier
                    },
                    Type = new ItemTypeDto
                    {
                        Id = i.TypeItemNavigation.IdItemType,
                        Name = i.TypeItemNavigation.NameItemType
                    },
                    IsInStock = i.CountInStorageItem > 0,
                    HasDiscount = i.DiscountItem > 0,
                    DiscountAmount = Math.Round(i.PriceItem * i.DiscountItem / 100, 2)
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound($"Товар с артикулом {articule} не найден");
            }

            return Ok(item);
        }

        [HttpPost("AddItem")]
        public async Task<ActionResult<ItemDTO>> CreateItem(CreateItemDto createItemDto)
        {
            // Проверяем обязательные поля
            if (string.IsNullOrEmpty(createItemDto.ArticuleItem))
            {
                return BadRequest("Артикул товара обязателен для заполнения");
            }

            var category = await ShoeContext.Context.ItemCategories
                .FirstOrDefaultAsync(c => c.IdItemCategory == createItemDto.CategoryId);

            var manufacturer = await ShoeContext.Context.Manufactures
                .FirstOrDefaultAsync(m => m.IdManufacture == createItemDto.ManufacturerId);

            var supplier = await ShoeContext.Context.Suppliers
                .FirstOrDefaultAsync(s => s.IdSupplier == createItemDto.SupplierId);

            var itemType = await ShoeContext.Context.ItemTypes
                .FirstOrDefaultAsync(t => t.IdItemType == createItemDto.TypeId);

            if (category == null || manufacturer == null || supplier == null || itemType == null)
            {
                return BadRequest("Один или несколько связанных элементов не найдены");
            }

            // Проверяем, существует ли товар с таким артикулом
            var existingItem = await ShoeContext.Context.Items
                .FirstOrDefaultAsync(i => i.ArticuleItem == createItemDto.ArticuleItem);

            if (existingItem != null)
            {
                return BadRequest($"Товар с артикулом {createItemDto.ArticuleItem} уже существует");
            }

            // Создаем новый товар
            var newItem = new Item
            {
                ArticuleItem = createItemDto.ArticuleItem,
                TypeItem = createItemDto.TypeId,
                MeasurementItem = createItemDto.MeasurementItem ?? "шт",
                PriceItem = createItemDto.PriceItem,
                IdSupplierItem = createItemDto.SupplierId,
                IdManufactureItem = createItemDto.ManufacturerId,
                CategoryItem = createItemDto.CategoryId,
                DiscountItem = createItemDto.DiscountItem ?? 0,
                CountInStorageItem = createItemDto.CountInStorageItem ?? 0,
                DescriptionItem = createItemDto.DescriptionItem,
                PhotoItem = !string.IsNullOrEmpty(createItemDto.PhotoItem)
                    ? Convert.FromBase64String(createItemDto.PhotoItem)
                    : null
            };

            // Добавляем товар в базу данных
            ShoeContext.Context.Items.Add(newItem);
            await ShoeContext.Context.SaveChangesAsync();

            // Загружаем связанные данные для ответа
            var createdItem = await ShoeContext.Context.Items
                .Include(i => i.CategoryItemNavigation)
                .Include(i => i.IdManufactureItemNavigation)
                .Include(i => i.IdSupplierItemNavigation)
                .Include(i => i.TypeItemNavigation)
                .FirstOrDefaultAsync(i => i.ArticuleItem == newItem.ArticuleItem);

            // Маппим в DTO для ответа
            var itemDto = MapToDTO(createdItem);

            return CreatedAtAction(nameof(GetItems), new { id = itemDto.ArticuleItem }, itemDto);
        }

        [HttpPut("UpdateItem/{articule}")]
        public async Task<ActionResult<ItemDTO>> UpdateItem(string articule, UpdateItemDto updateItemDto)
        {
            var existingItem = await ShoeContext.Context.Items
                .Include(i => i.CategoryItemNavigation)
                .Include(i => i.IdManufactureItemNavigation)
                .Include(i => i.IdSupplierItemNavigation)
                .Include(i => i.TypeItemNavigation)
                .FirstOrDefaultAsync(i => i.ArticuleItem == articule);

            if (existingItem == null)
            {
                return NotFound($"Товар с артикулом {articule} не найден");
            }

            // Проверяем связанные сущности, если они были изменены
            if (updateItemDto.CategoryId.HasValue)
            {
                var category = await ShoeContext.Context.ItemCategories
                    .FirstOrDefaultAsync(c => c.IdItemCategory == updateItemDto.CategoryId);
                if (category == null)
                {
                    return BadRequest($"Категория с ID {updateItemDto.CategoryId} не найдена");
                }
                existingItem.CategoryItem = updateItemDto.CategoryId.Value;
            }

            if (updateItemDto.ManufacturerId.HasValue)
            {
                var manufacturer = await ShoeContext.Context.Manufactures
                    .FirstOrDefaultAsync(m => m.IdManufacture == updateItemDto.ManufacturerId);
                if (manufacturer == null)
                {
                    return BadRequest($"Производитель с ID {updateItemDto.ManufacturerId} не найден");
                }
                existingItem.IdManufactureItem = updateItemDto.ManufacturerId.Value;
            }

            if (updateItemDto.SupplierId.HasValue)
            {
                var supplier = await ShoeContext.Context.Suppliers
                    .FirstOrDefaultAsync(s => s.IdSupplier == updateItemDto.SupplierId);
                if (supplier == null)
                {
                    return BadRequest($"Поставщик с ID {updateItemDto.SupplierId} не найден");
                }
                existingItem.IdSupplierItem = updateItemDto.SupplierId.Value;
            }

            if (updateItemDto.TypeId.HasValue)
            {
                var itemType = await ShoeContext.Context.ItemTypes
                    .FirstOrDefaultAsync(t => t.IdItemType == updateItemDto.TypeId);
                if (itemType == null)
                {
                    return BadRequest($"Тип товара с ID {updateItemDto.TypeId} не найден");
                }
                existingItem.TypeItem = updateItemDto.TypeId.Value;
            }

            // Обновляем остальные поля
            if (!string.IsNullOrEmpty(updateItemDto.MeasurementItem))
                existingItem.MeasurementItem = updateItemDto.MeasurementItem;

            if (updateItemDto.PriceItem.HasValue)
                existingItem.PriceItem = updateItemDto.PriceItem.Value;

            if (updateItemDto.DiscountItem.HasValue)
                existingItem.DiscountItem = updateItemDto.DiscountItem.Value;

            if (updateItemDto.CountInStorageItem.HasValue)
                existingItem.CountInStorageItem = updateItemDto.CountInStorageItem.Value;

            if (!string.IsNullOrEmpty(updateItemDto.DescriptionItem))
                existingItem.DescriptionItem = updateItemDto.DescriptionItem;

            if (!string.IsNullOrEmpty(updateItemDto.PhotoItem))
                existingItem.PhotoItem = Convert.FromBase64String(updateItemDto.PhotoItem);

            await ShoeContext.Context.SaveChangesAsync();

            // Загружаем обновленный товар со всеми связями
            var updatedItem = await ShoeContext.Context.Items
                .Include(i => i.CategoryItemNavigation)
                .Include(i => i.IdManufactureItemNavigation)
                .Include(i => i.IdSupplierItemNavigation)
                .Include(i => i.TypeItemNavigation)
                .FirstOrDefaultAsync(i => i.ArticuleItem == articule);

            var itemDto = MapToDTO(updatedItem);

            return Ok(itemDto);
        }

        [HttpDelete("DeleteItem/{articule}")]
        public async Task<IActionResult> DeleteItem(string articule)
        {
            var item = await ShoeContext.Context.Items
                .FirstOrDefaultAsync(i => i.ArticuleItem == articule);

            if (item == null)
            {
                return NotFound($"Товар с артикулом {articule} не найден");
            }

            ShoeContext.Context.Items.Remove(item);
            await ShoeContext.Context.SaveChangesAsync();

            return Ok(new { message = $"Товар с артикулом {articule} успешно удален" });
        }

        [HttpGet("GetCategories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await ShoeContext.Context.ItemCategories
                .Select(c => new CategoryDto
                {
                    Id = c.IdItemCategory,
                    Name = c.NameItemCategory
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("GetManufacturers")]
        public async Task<ActionResult<IEnumerable<ManufacturerDto>>> GetManufacturers()
        {
            var manufacturers = await ShoeContext.Context.Manufactures
                .Select(m => new ManufacturerDto
                {
                    Id = m.IdManufacture,
                    Name = m.NameManufacture
                })
                .ToListAsync();

            return Ok(manufacturers);
        }

        [HttpGet("GetSuppliers")]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
        {
            var suppliers = await ShoeContext.Context.Suppliers
                .Select(s => new SupplierDto
                {
                    Id = s.IdSupplier,
                    Name = s.NameSupplier
                })
                .ToListAsync();

            return Ok(suppliers);
        }

        [HttpGet("GetItemTypes")]
        public async Task<ActionResult<IEnumerable<ItemTypeDto>>> GetItemTypes()
        {
            var itemTypes = await ShoeContext.Context.ItemTypes
                .Select(t => new ItemTypeDto
                {
                    Id = t.IdItemType,
                    Name = t.NameItemType
                })
                .ToListAsync();

            return Ok(itemTypes);
        }

        private ItemDTO MapToDTO(Item item)
        {
            return new ItemDTO
            {
                ArticuleItem = item.ArticuleItem,
                TypeItem = item.TypeItem,
                MeasurementItem = item.MeasurementItem,
                Measurement = $"Единица измерения: {item.MeasurementItem}",
                PriceItem = item.PriceItem,
                PriceColor = item.DiscountItem > 0 ? "Red" : "none",
                PriceStyle = item.DiscountItem > 0 ? "Strikethrough" : "none",
                Price = Math.Round(item.PriceItem * (100 - item.DiscountItem) / 100, 2),
                PriceWithDiscount = Math.Round(item.PriceItem * (100 - item.DiscountItem) / 100, 2),
                OriginalPrice = item.PriceItem,
                IdSupplierItem = item.IdSupplierItem,
                Supplier = $"Поставщик: {item.IdSupplierItemNavigation?.NameSupplier}",
                IdManufactureItem = item.IdManufactureItem,
                Manufacture = $"Производитель: {item.IdManufactureItemNavigation?.NameManufacture}",
                CategoryItem = item.CategoryItem,
                CategoryName = $"{item.CategoryItemNavigation?.NameItemCategory} | {item.TypeItemNavigation?.NameItemType}",
                DiscountItem = item.DiscountItem,
                Discount = $"Действующая скидка: {item.DiscountItem}%",
                DiscountColor = item.DiscountItem > 15 ? "#2E8B57" : "none",
                CountInStorageItem = item.CountInStorageItem,
                CountInStorage = $"Количество на складе: {item.CountInStorageItem}",
                DescriptionItem = item.DescriptionItem,
                Description = $"Описание товара: {item.DescriptionItem}",
                CountInStorageStyle = item.CountInStorageItem == 0 ? "Cyan" : "none",
                PhotoItem = item.PhotoItem,
                Category = item.CategoryItemNavigation != null ? new CategoryDto
                {
                    Id = item.CategoryItemNavigation.IdItemCategory,
                    Name = item.CategoryItemNavigation.NameItemCategory
                } : null,
                Manufacturer = item.IdManufactureItemNavigation != null ? new ManufacturerDto
                {
                    Id = item.IdManufactureItemNavigation.IdManufacture,
                    Name = item.IdManufactureItemNavigation.NameManufacture
                } : null,
                SupplierInfo = item.IdSupplierItemNavigation != null ? new SupplierDto
                {
                    Id = item.IdSupplierItemNavigation.IdSupplier,
                    Name = item.IdSupplierItemNavigation.NameSupplier
                } : null,
                Type = item.TypeItemNavigation != null ? new ItemTypeDto
                {
                    Id = item.TypeItemNavigation.IdItemType,
                    Name = item.TypeItemNavigation.NameItemType
                } : null,
                IsInStock = item.CountInStorageItem > 0,
                HasDiscount = item.DiscountItem > 0,
                DiscountAmount = Math.Round(item.PriceItem * item.DiscountItem / 100, 2)
            };
        }
    }
    public class ItemDTO
    {
        public string ArticuleItem { get; set; }
        public int? TypeItem { get; set; }
        public string MeasurementItem { get; set; }
        public string Measurement { get; set; }
        public decimal PriceItem { get; set; }
        public string PriceColor { get; set; }
        public string PriceStyle { get; set; }
        public decimal Price { get; set; }
        public decimal PriceWithDiscount { get; set; }
        public decimal OriginalPrice { get; set; }
        public int? IdSupplierItem { get; set; }
        public string Supplier { get; set; }
        public int? IdManufactureItem { get; set; }
        public string Manufacture { get; set; }
        public int? CategoryItem { get; set; }
        public string CategoryName { get; set; }
        public int DiscountItem { get; set; }
        public string Discount { get; set; }
        public string DiscountColor { get; set; }
        public int CountInStorageItem { get; set; }
        public string CountInStorage { get; set; }
        public string DescriptionItem { get; set; }
        public string Description { get; set; }
        public string CountInStorageStyle { get; set; }
        public byte[] PhotoItem { get; set; }
        public CategoryDto Category { get; set; }
        public ManufacturerDto Manufacturer { get; set; }
        public SupplierDto SupplierInfo { get; set; }
        public ItemTypeDto Type { get; set; }
        public bool IsInStock { get; set; }
        public bool HasDiscount { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class CreateItemDto
    {
        public string ArticuleItem { get; set; }
        public int TypeId { get; set; }
        public string MeasurementItem { get; set; }
        public decimal PriceItem { get; set; }
        public int SupplierId { get; set; }
        public int ManufacturerId { get; set; }
        public int CategoryId { get; set; }
        public int? DiscountItem { get; set; }
        public int? CountInStorageItem { get; set; }
        public string DescriptionItem { get; set; }
        public string PhotoItem { get; set; }
    }

    public class UpdateItemDto
    {
        public int? TypeId { get; set; }
        public string MeasurementItem { get; set; }
        public decimal? PriceItem { get; set; }
        public int? SupplierId { get; set; }
        public int? ManufacturerId { get; set; }
        public int? CategoryId { get; set; }
        public int? DiscountItem { get; set; }
        public int? CountInStorageItem { get; set; }
        public string DescriptionItem { get; set; }
        public string PhotoItem { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ManufacturerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ItemTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}