using Demka.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Demka.window_xaml
{
    /// <summary>
    /// Логика взаимодействия для ZakaziWindow.xaml
    /// </summary>
    public partial class ZakaziWindow : Window
    {
        private int _currentUserId;
        private string _userRole;
        private bool _isAdmin = false;
        private ShoeContext _context = new ShoeContext();

        public ZakaziWindow()
        {
            InitializeComponent();
            SetData();
        }

        private void SetData() // Установка данных в момент открытия окна
        {
            try
            {
                var userIdEnv = Environment.GetEnvironmentVariable("USERID");

                if (userIdEnv != "none" && !string.IsNullOrEmpty(userIdEnv))
                {
                    _currentUserId = Convert.ToInt32(userIdEnv);
                    var user = _context.UserImports.FirstOrDefault(u => u.IdUserImport == _currentUserId);

                    if (user != null)
                    {
                        FIOUserTBL.Text = $"{user.SurnameUserImport} {user.NameUserImport} {user.LastnameUserImport}";
                        RoleUserTBL.Text = user.userRole;
                        _userRole = user.userRole;

                        if (user.userRole == "Ваша роль: Администратор")
                        {
                            _isAdmin = true;
                            addOrderBTN.Visibility = Visibility.Visible;
                            EditOrderBTN.Visibility = Visibility.Visible;
                            DeleteOrderBTN.Visibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    FIOUserTBL.Text = "Гость";
                    RoleUserTBL.Text = "Ваша роль: Гость";
                    _currentUserId = -1;
                    addOrderBTN.Visibility = Visibility.Collapsed;
                    EditOrderBTN.Visibility = Visibility.Collapsed;
                    DeleteOrderBTN.Visibility = Visibility.Collapsed;
                }

                UpdateDataListView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDataListView()
        {
            try
            {
                IQueryable<OrderImport> query = _context.OrderImports
                    .Include(o => o.IdPickupPointOrderImportNavigation)
                    .Include(o => o.IdClientOrderImportNavigation)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ArticulItemNavigation)

                    .OrderByDescending(o => o.DateOrderImport);

                // Если не админ, показываем только свои заказы
                if (!_isAdmin && _currentUserId != -1)
                {
                    query = query.Where(o => o.IdClientOrderImport == _currentUserId);
                }

                var orders = query.ToList();

                // Преобразуем для отображения
                var displayOrders = orders.Select(o => new OrderDisplayModel
                {
                    Id = o.IdOrderImport,
                    Status = GetStatusDisplay(o.StatusOrderImport),
                    RawStatus = o.StatusOrderImport,
                    IdPickupPointNavigation = o.IdPickupPointOrderImportNavigation,
                    DateOrderingString = o.DateOrderImport.ToString("dd.MM.yyyy"),
                    DateDeliveryString = o.DateOfDeliveryOrderImport.ToString("dd.MM.yyyy"),
                    ClientName = $"{o.IdClientOrderImportNavigation.SurnameUserImport} {o.IdClientOrderImportNavigation.NameUserImport}",
                    Code = o.CodeOrderImport,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDisplayModel
                    {
                        Articul = oi.ArticulItem,
                        ItemName = oi.ArticulItemNavigation?.DescriptionItem ?? "Неизвестный товар",
                        Price = oi.ArticulItemNavigation?.PriceItem ?? 0,
                        Quantity = oi.ArticulItemNavigation?.CountInStorageItem ?? 0,
                        Photo = oi.ArticulItemNavigation?.Photo
                    }).ToList()
                }).ToList();

                OrderBoxLV.ItemsSource = displayOrders;

                if (!displayOrders.Any() && _currentUserId != -1)
                {
                    MessageBox.Show("У вас нет заказов", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetStatusDisplay(string status)
        {
            return status switch
            {
                "New" => "Новый",
                "Completed" => "Завершен",
                _ => status
            };
        }

        private string GetFullAddress(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                return "Адрес не указан";

            return $"{pickupPoint.CityPickupPoint}, {pickupPoint.DistrictPickupPoint}, индекс: {pickupPoint.IndexPickupPoint}";
        }

        private void ReturnBTN_Click(object sender, RoutedEventArgs e)
        {
            new UserWindow().Show();
            this.Close();
        }

        private void addOrderBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin)
            {
                MessageBox.Show("Только администратор может добавлять заказы", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddEditOrderWindow addWindow = new AddEditOrderWindow(null);
            if (addWindow.ShowDialog() == true)
            {
                _context = new ShoeContext();
                UpdateDataListView();
            }
        }

        private void EditOrderBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin)
            {
                MessageBox.Show("Только администратор может редактировать заказы", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (OrderBoxLV.SelectedItem is OrderDisplayModel selectedOrder)
            {
                // Создаем новый контекст для получения заказа
                using (var context = new ShoeContext())
                {
                    var originalOrder = context.OrderImports
                        .Include(o => o.IdPickupPointOrderImportNavigation)
                        .Include(o => o.IdClientOrderImportNavigation)
                        .FirstOrDefault(o => o.IdOrderImport == selectedOrder.Id);

                    if (originalOrder != null)
                    {
                        AddEditOrderWindow editWindow = new AddEditOrderWindow(originalOrder);
                        if (editWindow.ShowDialog() == true)
                        {
                            // Обновляем основной контекст и список
                            _context = new ShoeContext();
                            UpdateDataListView();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteOrderBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin)
            {
                MessageBox.Show("Только администратор может удалять заказы", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (OrderBoxLV.SelectedItem is OrderDisplayModel selectedOrder)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Вы действительно хотите удалить заказ №{selectedOrder.Id}?\n\nЭто действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteOrderFromDatabase(selectedOrder.Id);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteOrderFromDatabase(int orderId)
        {
            try
            {
                using (var deleteContext = new ShoeContext())
                {
                    var orderItems = deleteContext.OrderItems
                        .Where(oi => oi.IdOrderItem == orderId);
                    deleteContext.OrderItems.RemoveRange(orderItems);

                    var orderToDelete = deleteContext.OrderImports
                        .FirstOrDefault(o => o.IdOrderImport == orderId);

                    if (orderToDelete != null)
                    {
                        deleteContext.OrderImports.Remove(orderToDelete);
                        deleteContext.SaveChanges();

                        MessageBox.Show("Заказ успешно удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        _context = new ShoeContext();
                        UpdateDataListView();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заказа из базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrderBoxLV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!_isAdmin)
                {
                    return;
                }

                if (OrderBoxLV.SelectedItem is OrderDisplayModel selectedOrder)
                {
                    string address = GetFullAddress(selectedOrder.IdPickupPointNavigation);

                    string orderDetails = $"ЗАКАЗ №{selectedOrder.Id}\n" +
                                         $"Код: {selectedOrder.Code}\n" +
                                         $"Статус: {selectedOrder.Status}\n" +
                                         $"Клиент: {selectedOrder.ClientName}\n" +
                                         $"Дата заказа: {selectedOrder.DateOrderingString}\n" +
                                         $"Дата доставки: {selectedOrder.DateDeliveryString}\n" +
                                         $"{address}\n\n" +
                                         $"ТОВАРЫ В ЗАКАЗЕ:\n" +
                                         $"{new string('-', 50)}\n";

                    decimal totalOrderSum = 0;
                    foreach (var item in selectedOrder.OrderItems)
                    {
                        orderDetails += $"📦 {item.ItemName}\n" +
                                      $"   Артикул: {item.Articul}\n" +
                                      $"   Количество в заказе: {item.Quantity} шт.\n" +
                                      $"   Цена: {item.Price:N2} ₽\n" +
                                      $"   Фото: {(item.Photo != null ? "есть" : "нет")}\n" +
                                      $"{new string('-', 50)}\n";
                        totalOrderSum += item.Price * item.Quantity;
                    }

                    orderDetails += $"\n💰 ИТОГО ПО ЗАКАЗУ: {totalOrderSum:N2} ₽";

                    MessageBox.Show(orderDetails, "Детали заказа",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии деталей заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }

    public class OrderDisplayModel
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string RawStatus { get; set; }
        public PickupPoint IdPickupPointNavigation { get; set; }
        public string DateOrderingString { get; set; }
        public string DateDeliveryString { get; set; }
        public string ClientName { get; set; }
        public string Code { get; set; }
        public List<OrderItemDisplayModel> OrderItems { get; set; }

        public string FullAddress
        {
            get
            {
                if (IdPickupPointNavigation == null)
                    return "Адрес не указан";

                return $"{IdPickupPointNavigation.CityPickupPoint}, {IdPickupPointNavigation.DistrictPickupPoint}, индекс: {IdPickupPointNavigation.IndexPickupPoint}";
            }
        }
    }

    public class OrderItemDisplayModel
    {
        public string Articul { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public BitmapImage Photo { get; set; }
    }
}