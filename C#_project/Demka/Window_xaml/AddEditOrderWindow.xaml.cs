using Demka.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Demka.window_xaml
{
    public partial class AddEditOrderWindow : Window
    {
        private OrderImport _currentOrder;
        private ShoeContext _context = new ShoeContext();
        private bool _isEditMode = false;

        public class PickupPointDisplay
        {
            public int IdPickupPoint { get; set; }
            public string FullAddress { get; set; }
        }

        public AddEditOrderWindow(OrderImport order = null)
        {
            InitializeComponent();
            _currentOrder = order;
            _isEditMode = order != null;

            if (_isEditMode)
            {
                Title = "Редактирование заказа";
                LoadOrderData();
                if (StatusComboBox.Text == "Завершен") {
                    StatusEnd.IsEnabled = false;
                    CodeTextBox.IsEnabled = false;
                    StatusComboBox.IsEnabled = false;
                    PickupPointComboBox.IsEnabled = false;
                    OrderDatePicker.IsEnabled = false;
                    DeliveryDatePicker.IsEnabled = false;
                }
            }
            else
            {
                Title = "Добавление заказа";
                OrderDatePicker.SelectedDate = DateTime.Today;
                DeliveryDatePicker.SelectedDate = DateTime.Today.AddDays(3);
                StatusEnd.IsEnabled = false;
                // Предлагаем следующий код заказа
                SuggestNextOrderCode();
            }

            LoadPickupPoints();
        }

        private void SuggestNextOrderCode()
        {
            try
            {
                // Находим максимальный код заказа и предлагаем следующий
                var maxCode = _context.OrderImports
                    .Select(o => o.CodeOrderImport)
                    .Where(c => !string.IsNullOrEmpty(c) && c.All(char.IsDigit))
                    .Select(c => int.Parse(c))
                    .DefaultIfEmpty(900)
                    .Max();

                CodeTextBox.Text = (maxCode + 1).ToString();
            }
            catch
            {
                CodeTextBox.Text = "909"; // Значение по умолчанию
            }
        }

        private void LoadPickupPoints()
        {
            try
            {
                var pickupPoints = _context.PickupPoints
                    .ToList()
                    .Select(pp => new PickupPointDisplay
                    {
                        IdPickupPoint = pp.IdPickupPoint,
                        FullAddress = $"{pp.CityPickupPoint}, {pp.DistrictPickupPoint}, индекс: {pp.IndexPickupPoint}"
                    })
                    .ToList();

                PickupPointComboBox.ItemsSource = pickupPoints;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пунктов выдачи: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrderData()
        {
            if (_currentOrder == null) return;

            CodeTextBox.Text = _currentOrder.CodeOrderImport;

            // Статус уже хранится в БД на русском: 'Завершен' или 'Новый'
            string displayStatus = _currentOrder.StatusOrderImport;

            bool found = false;
            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == displayStatus)
                {
                    StatusComboBox.SelectedItem = item;
                    found = true;
                    break;
                }
            }

            if (!found && StatusComboBox.Items.Count > 0)
            {
                StatusComboBox.SelectedIndex = 0;
            }

            if (_currentOrder.IdPickupPointOrderImport > 0)
            {
                PickupPointComboBox.SelectedValue = _currentOrder.IdPickupPointOrderImport;
            }

            if (_currentOrder.DateOrderImport != null)
            {
                OrderDatePicker.SelectedDate = _currentOrder.DateOrderImport.ToDateTime(TimeOnly.MinValue);
            }

            if (_currentOrder.DateOfDeliveryOrderImport != null)
            {
                DeliveryDatePicker.SelectedDate = _currentOrder.DateOfDeliveryOrderImport.ToDateTime(TimeOnly.MinValue);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(CodeTextBox.Text))
                {
                    MessageBox.Show("Введите код заказа!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, что код заказа - это число
                if (!CodeTextBox.Text.All(char.IsDigit))
                {
                    MessageBox.Show("Код заказа должен содержать только цифры!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем длину кода (в таблице видно, что коды 3-значные)
                if (CodeTextBox.Text.Length > 5) // Даем небольшой запас
                {
                    MessageBox.Show("Код заказа слишком длинный! Максимум 5 цифр.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (StatusComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус заказа!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PickupPointComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пункт выдачи!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (OrderDatePicker.SelectedDate == null || DeliveryDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите даты!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ComboBoxItem selectedStatus = (ComboBoxItem)StatusComboBox.SelectedItem;

                // Используем русские значения, так как в БД ENUM с русскими значениями
                string statusForDb = selectedStatus.Content.ToString(); // "Новый" или "Завершен"

                int pickupPointId = (int)PickupPointComboBox.SelectedValue;
                DateOnly orderDate = DateOnly.FromDateTime(OrderDatePicker.SelectedDate.Value);
                DateOnly deliveryDate = DateOnly.FromDateTime(DeliveryDatePicker.SelectedDate.Value);

                if (_isEditMode)
                {
                    // Для редактирования используем отдельный контекст
                    using (var editContext = new ShoeContext())
                    {
                        var orderToUpdate = editContext.OrderImports
                            .FirstOrDefault(o => o.IdOrderImport == _currentOrder.IdOrderImport);

                        if (orderToUpdate != null)
                        {
                            orderToUpdate.CodeOrderImport = CodeTextBox.Text.Trim();
                            orderToUpdate.StatusOrderImport = statusForDb;
                            orderToUpdate.IdPickupPointOrderImport = pickupPointId;
                            orderToUpdate.DateOrderImport = orderDate;
                            orderToUpdate.DateOfDeliveryOrderImport = deliveryDate;

                            editContext.SaveChanges();
                        }
                    }
                }
                else
                {
                    // Проверяем, не существует ли уже такой код заказа
                    bool codeExists = _context.OrderImports
                        .Any(o => o.CodeOrderImport == CodeTextBox.Text.Trim());

                    if (codeExists)
                    {
                        MessageBox.Show($"Заказ с кодом {CodeTextBox.Text} уже существует! Введите другой код.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Для добавления используем основной контекст
                    var newOrder = new OrderImport
                    {
                        CodeOrderImport = CodeTextBox.Text.Trim(),
                        StatusOrderImport = statusForDb,
                        IdPickupPointOrderImport = pickupPointId,
                        DateOrderImport = orderDate,
                        DateOfDeliveryOrderImport = deliveryDate,
                        IdClientOrderImport = GetCurrentUserId()
                    };
                    _context.OrderImports.Add(newOrder);
                    _context.SaveChanges();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при сохранении: {ex.Message}";

                // Добавляем внутреннее исключение если есть
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                }

                MessageBox.Show(errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetCurrentUserId()
        {
            try
            {
                var userIdEnv = Environment.GetEnvironmentVariable("USERID");
                if (userIdEnv != "none" && !string.IsNullOrEmpty(userIdEnv))
                {
                    return Convert.ToInt32(userIdEnv);
                }
            }
            catch { }
            return 1;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}