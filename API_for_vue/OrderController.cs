using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIDemkaForVue.Model;
using APIDemkaForVue.DTO;

namespace APIDemkaForVue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DTO_Order_imports>>> GetOrders()
        {
            try
            {
                var orders = await ShoeContext.Context.OrderImports
                    .Include(o => o.IdClientOrderImportNavigation)
                    .Include(o => o.IdPickupPointOrderImportNavigation)
                    .OrderByDescending(o => o.DateOrderImport) // Сначала новые
                    .Select(o => new DTO_Order_imports
                    {
                        IdOrderImport = o.IdOrderImport,
                        CodeOrderImport = o.CodeOrderImport,
                        StatusOrderImport = o.StatusOrderImport,
                        DateOrderImport = o.DateOrderImport,
                        DateOfDeliveryOrderImport = o.DateOfDeliveryOrderImport,
                        IdPickupPointOrderImport = o.IdPickupPointOrderImport,
                        IdClientOrderImport = o.IdClientOrderImport
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DTO_Order_imports>> GetOrder(int id)
        {
            try
            {
                var order = await ShoeContext.Context.OrderImports
                    .Include(o => o.IdClientOrderImportNavigation)
                    .Include(o => o.IdPickupPointOrderImportNavigation)
                    .Where(o => o.IdOrderImport == id)
                    .Select(o => new DTO_Order_imports
                    {
                        IdOrderImport = o.IdOrderImport,
                        CodeOrderImport = o.CodeOrderImport,
                        StatusOrderImport = o.StatusOrderImport,
                        DateOrderImport = o.DateOrderImport,
                        DateOfDeliveryOrderImport = o.DateOfDeliveryOrderImport,
                        IdPickupPointOrderImport = o.IdPickupPointOrderImport,
                        IdClientOrderImport = o.IdClientOrderImport
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound($"Заказ с ID {id} не найден");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // GET: api/Order/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<DTO_Order_imports>> GetOrderByCode(string code)
        {
            try
            {
                var order = await ShoeContext.Context.OrderImports
                    .Include(o => o.IdClientOrderImportNavigation)
                    .Include(o => o.IdPickupPointOrderImportNavigation)
                    .Where(o => o.CodeOrderImport == code)
                    .Select(o => new DTO_Order_imports
                    {
                        IdOrderImport = o.IdOrderImport,
                        CodeOrderImport = o.CodeOrderImport,
                        StatusOrderImport = o.StatusOrderImport,
                        DateOrderImport = o.DateOrderImport,
                        DateOfDeliveryOrderImport = o.DateOfDeliveryOrderImport,
                        IdPickupPointOrderImport = o.IdPickupPointOrderImport,
                        IdClientOrderImport = o.IdClientOrderImport
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound($"Заказ с кодом {code} не найден");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // GET: api/Order/client/{clientId}
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<DTO_Order_imports>>> GetOrdersByClient(int clientId)
        {
            try
            {
                var orders = await ShoeContext.Context.OrderImports
                    .Include(o => o.IdClientOrderImportNavigation)
                    .Include(o => o.IdPickupPointOrderImportNavigation)
                    .Where(o => o.IdClientOrderImport == clientId)
                    .OrderByDescending(o => o.DateOrderImport)
                    .Select(o => new DTO_Order_imports
                    {
                        IdOrderImport = o.IdOrderImport,
                        CodeOrderImport = o.CodeOrderImport,
                        StatusOrderImport = o.StatusOrderImport,
                        DateOrderImport = o.DateOrderImport,
                        DateOfDeliveryOrderImport = o.DateOfDeliveryOrderImport,
                        IdPickupPointOrderImport = o.IdPickupPointOrderImport,
                        IdClientOrderImport = o.IdClientOrderImport
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // GET: api/Order/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<DTO_Order_imports>>> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await ShoeContext.Context.OrderImports
                    .Include(o => o.IdClientOrderImportNavigation)
                    .Include(o => o.IdPickupPointOrderImportNavigation)
                    .Where(o => o.StatusOrderImport == status)
                    .OrderByDescending(o => o.DateOrderImport)
                    .Select(o => new DTO_Order_imports
                    {
                        IdOrderImport = o.IdOrderImport,
                        CodeOrderImport = o.CodeOrderImport,
                        StatusOrderImport = o.StatusOrderImport,
                        DateOrderImport = o.DateOrderImport,
                        DateOfDeliveryOrderImport = o.DateOfDeliveryOrderImport,
                        IdPickupPointOrderImport = o.IdPickupPointOrderImport,
                        IdClientOrderImport = o.IdClientOrderImport
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // GET: api/Order/pickup/{pickupPointId}
        [HttpGet("pickup/{pickupPointId}")]
        public async Task<ActionResult<IEnumerable<DTO_Order_imports>>> GetOrdersByPickupPoint(int pickupPointId)
        {
            try
            {
                var orders = await ShoeContext.Context.OrderImports
                    .Include(o => o.IdClientOrderImportNavigation)
                    .Include(o => o.IdPickupPointOrderImportNavigation)
                    .Where(o => o.IdPickupPointOrderImport == pickupPointId)
                    .OrderByDescending(o => o.DateOrderImport)
                    .Select(o => new DTO_Order_imports
                    {
                        IdOrderImport = o.IdOrderImport,
                        CodeOrderImport = o.CodeOrderImport,
                        StatusOrderImport = o.StatusOrderImport,
                        DateOrderImport = o.DateOrderImport,
                        DateOfDeliveryOrderImport = o.DateOfDeliveryOrderImport,
                        IdPickupPointOrderImport = o.IdPickupPointOrderImport,
                        IdClientOrderImport = o.IdClientOrderImport
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // GET: api/Order/date-range
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<DTO_Order_imports>>> GetOrdersByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var start = DateOnly.FromDateTime(startDate);
                var end = DateOnly.FromDateTime(endDate);

                var orders = await ShoeContext.Context.OrderImports
                    .Include(o => o.IdClientOrderImportNavigation)
                    .Include(o => o.IdPickupPointOrderImportNavigation)
                    .Where(o => o.DateOrderImport >= start && o.DateOrderImport <= end)
                    .OrderByDescending(o => o.DateOrderImport)
                    .Select(o => new DTO_Order_imports
                    {
                        IdOrderImport = o.IdOrderImport,
                        CodeOrderImport = o.CodeOrderImport,
                        StatusOrderImport = o.StatusOrderImport,
                        DateOrderImport = o.DateOrderImport,
                        DateOfDeliveryOrderImport = o.DateOfDeliveryOrderImport,
                        IdPickupPointOrderImport = o.IdPickupPointOrderImport,
                        IdClientOrderImport = o.IdClientOrderImport
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // POST: api/Order
        [HttpPost]
        public async Task<ActionResult<DTO_Order_imports>> CreateOrder(CreateOrderDto createOrderDto)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(createOrderDto.CodeOrderImport))
                {
                    return BadRequest("Введите код заказа!");
                }

                // Проверяем, что код заказа - это число
                if (!createOrderDto.CodeOrderImport.All(char.IsDigit))
                {
                    return BadRequest("Код заказа должен содержать только цифры!");
                }

                // Проверяем длину кода
                if (createOrderDto.CodeOrderImport.Length > 5)
                {
                    return BadRequest("Код заказа слишком длинный! Максимум 5 цифр.");
                }

                // Проверяем статус
                if (createOrderDto.StatusOrderImport != "Новый" && createOrderDto.StatusOrderImport != "Завершен")
                {
                    return BadRequest("Статус должен быть 'Новый' или 'Завершен'");
                }

                // Проверяем существование пункта выдачи
                var pickupPoint = await ShoeContext.Context.PickupPoints
                    .FirstOrDefaultAsync(p => p.IdPickupPoint == createOrderDto.IdPickupPointOrderImport);

                if (pickupPoint == null)
                {
                    return BadRequest($"Пункт выдачи с ID {createOrderDto.IdPickupPointOrderImport} не найден");
                }

                // Проверяем существование клиента
                var client = await ShoeContext.Context.UserImports
                    .FirstOrDefaultAsync(u => u.IdUserImport == createOrderDto.IdClientOrderImport);

                if (client == null)
                {
                    return BadRequest($"Клиент с ID {createOrderDto.IdClientOrderImport} не найден");
                }

                // Проверяем, не существует ли уже такой код заказа
                bool codeExists = await ShoeContext.Context.OrderImports
                    .AnyAsync(o => o.CodeOrderImport == createOrderDto.CodeOrderImport);

                if (codeExists)
                {
                    return BadRequest($"Заказ с кодом {createOrderDto.CodeOrderImport} уже существует! Введите другой код.");
                }

                var newOrder = new OrderImport
                {
                    CodeOrderImport = createOrderDto.CodeOrderImport.Trim(),
                    StatusOrderImport = createOrderDto.StatusOrderImport,
                    IdPickupPointOrderImport = createOrderDto.IdPickupPointOrderImport,
                    DateOrderImport = DateOnly.FromDateTime(createOrderDto.DateOrderImport),
                    DateOfDeliveryOrderImport = DateOnly.FromDateTime(createOrderDto.DateOfDeliveryOrderImport),
                    IdClientOrderImport = createOrderDto.IdClientOrderImport
                };

                ShoeContext.Context.OrderImports.Add(newOrder);
                await ShoeContext.Context.SaveChangesAsync();

                var orderDto = new DTO_Order_imports
                {
                    IdOrderImport = newOrder.IdOrderImport,
                    CodeOrderImport = newOrder.CodeOrderImport,
                    StatusOrderImport = newOrder.StatusOrderImport,
                    DateOrderImport = newOrder.DateOrderImport,
                    DateOfDeliveryOrderImport = newOrder.DateOfDeliveryOrderImport,
                    IdPickupPointOrderImport = newOrder.IdPickupPointOrderImport,
                    IdClientOrderImport = newOrder.IdClientOrderImport
                };

                return CreatedAtAction(nameof(GetOrder), new { id = orderDto.IdOrderImport }, orderDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // PUT: api/Order/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDto updateOrderDto)
        {
            try
            {
                var existingOrder = await ShoeContext.Context.OrderImports
                    .FirstOrDefaultAsync(o => o.IdOrderImport == id);

                if (existingOrder == null)
                {
                    return NotFound($"Заказ с ID {id} не найден");
                }

                // Не даем редактировать завершенные заказы
                if (existingOrder.StatusOrderImport == "Завершен")
                {
                    return BadRequest("Нельзя редактировать завершенный заказ");
                }

                // Валидация
                if (updateOrderDto.CodeOrderImport != null)
                {
                    if (string.IsNullOrWhiteSpace(updateOrderDto.CodeOrderImport))
                    {
                        return BadRequest("Введите код заказа!");
                    }

                    if (!updateOrderDto.CodeOrderImport.All(char.IsDigit))
                    {
                        return BadRequest("Код заказа должен содержать только цифры!");
                    }

                    if (updateOrderDto.CodeOrderImport.Length > 5)
                    {
                        return BadRequest("Код заказа слишком длинный! Максимум 5 цифр.");
                    }

                    // Проверяем, не занят ли код другим заказом
                    bool codeExists = await ShoeContext.Context.OrderImports
                        .AnyAsync(o => o.CodeOrderImport == updateOrderDto.CodeOrderImport && o.IdOrderImport != id);

                    if (codeExists)
                    {
                        return BadRequest($"Заказ с кодом {updateOrderDto.CodeOrderImport} уже существует!");
                    }

                    existingOrder.CodeOrderImport = updateOrderDto.CodeOrderImport.Trim();
                }

                if (updateOrderDto.StatusOrderImport != null)
                {
                    if (updateOrderDto.StatusOrderImport != "Новый" && updateOrderDto.StatusOrderImport != "Завершен")
                    {
                        return BadRequest("Статус должен быть 'Новый' или 'Завершен'");
                    }
                    existingOrder.StatusOrderImport = updateOrderDto.StatusOrderImport;
                }

                if (updateOrderDto.IdPickupPointOrderImport.HasValue)
                {
                    var pickupPoint = await ShoeContext.Context.PickupPoints
                        .FirstOrDefaultAsync(p => p.IdPickupPoint == updateOrderDto.IdPickupPointOrderImport.Value);

                    if (pickupPoint == null)
                    {
                        return BadRequest($"Пункт выдачи с ID {updateOrderDto.IdPickupPointOrderImport.Value} не найден");
                    }
                    existingOrder.IdPickupPointOrderImport = updateOrderDto.IdPickupPointOrderImport.Value;
                }

                if (updateOrderDto.IdClientOrderImport.HasValue)
                {
                    var client = await ShoeContext.Context.UserImports
                        .FirstOrDefaultAsync(u => u.IdUserImport == updateOrderDto.IdClientOrderImport.Value);

                    if (client == null)
                    {
                        return BadRequest($"Клиент с ID {updateOrderDto.IdClientOrderImport.Value} не найден");
                    }
                    existingOrder.IdClientOrderImport = updateOrderDto.IdClientOrderImport.Value;
                }

                if (updateOrderDto.DateOrderImport.HasValue)
                {
                    existingOrder.DateOrderImport = DateOnly.FromDateTime(updateOrderDto.DateOrderImport.Value);
                }

                if (updateOrderDto.DateOfDeliveryOrderImport.HasValue)
                {
                    existingOrder.DateOfDeliveryOrderImport = DateOnly.FromDateTime(updateOrderDto.DateOfDeliveryOrderImport.Value);
                }

                await ShoeContext.Context.SaveChangesAsync();

                return Ok(new { message = "Заказ успешно обновлен" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await ShoeContext.Context.OrderImports
                    .FirstOrDefaultAsync(o => o.IdOrderImport == id);

                if (order == null)
                {
                    return NotFound($"Заказ с ID {id} не найден");
                }

                // Не даем удалять завершенные заказы
                if (order.StatusOrderImport == "Завершен")
                {
                    return BadRequest("Нельзя удалить завершенный заказ");
                }

                ShoeContext.Context.OrderImports.Remove(order);
                await ShoeContext.Context.SaveChangesAsync();

                return Ok(new { message = $"Заказ с ID {id} успешно удален" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
        [HttpGet("GetPickupPoints")]
        public async Task<ActionResult<IEnumerable<PickupPointDto>>> GetPickupPoints()
        {
            var pickupPoints = await ShoeContext.Context.PickupPoints
                .Select(p => new PickupPointDto
                {
                    IdPickupPoint = p.IdPickupPoint,
                    FullAddress = $"{p.CityPickupPoint}, {p.DistrictPickupPoint}, индекс: {p.IndexPickupPoint}"
                })
                .ToListAsync();

            return Ok(pickupPoints);
        }

        // GET: api/Order/next-code
        [HttpGet("next-code")]
        public async Task<ActionResult<string>> GetNextOrderCode()
        {
            try
            {
                // Находим максимальный код заказа и предлагаем следующий
                var maxCode = await ShoeContext.Context.OrderImports
                    .Select(o => o.CodeOrderImport)
                    .Where(c => !string.IsNullOrEmpty(c) && c.All(char.IsDigit))
                    .Select(c => int.Parse(c))
                    .DefaultIfEmpty(900)
                    .MaxAsync();

                return Ok((maxCode + 1).ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // GET: api/Order/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetOrderStats()
        {
            try
            {
                var totalOrders = await ShoeContext.Context.OrderImports.CountAsync();
                var newOrders = await ShoeContext.Context.OrderImports
                    .CountAsync(o => o.StatusOrderImport == "Новый");
                var completedOrders = await ShoeContext.Context.OrderImports
                    .CountAsync(o => o.StatusOrderImport == "Завершен");

                return Ok(new
                {
                    totalOrders,
                    newOrders,
                    completedOrders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
}