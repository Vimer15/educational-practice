using Demka.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Demka.window_xaml
{
    public partial class UserWindow : Window
    {
        private List<Item> allItems;
        private ShoeContext context;
        private bool _isAdmin = false;
        private Item _selectedItemForContextMenu; // Для хранения выбранного элемента в контекстном меню

        // Объявляем поля для MenuItem (для 2-го варианта)
        private MenuItem EditMenuItem;
        private MenuItem DeleteMenuItem;

        public UserWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadData();
        }

        public void ExitAccounts()
        {
            Window window = new Authorization();
            window.Close();
            this.Close();
        }

        private void ExitAccount_Click(object sender, RoutedEventArgs e)
        {
            ExitAccounts();
        }

        public void LoadData()
        {
            try
            {
                context = new ShoeContext();
                var UserId = Environment.GetEnvironmentVariable("USERID");
                if (UserId != "none")
                {
                    UserImport? user = context.UserImports.FirstOrDefault(u => u.IdUserImport == Convert.ToInt32(UserId));
                    if (user != null)
                    {
                        UserNameTB.Text = $"{user.SurnameUserImport} {user.NameUserImport} {user.LastnameUserImport}";
                        UserRoleTB.Text = $"{user.userRole}";

                        if (user.userRole == "Ваша роль: Администратор")
                        {
                            _isAdmin = true;
                        }

                        if (UserRoleTB.Text == "Ваша роль: Авторизированный клиент")
                        {
                            SearchTB.Visibility = Visibility.Hidden;
                            SupplierFilterCB.Visibility = Visibility.Hidden;
                            SortCB.Visibility = Visibility.Hidden;
                            AddItemWindow.Visibility = Visibility.Hidden;
                        }
                        if (UserRoleTB.Text == "Ваша роль: Менеджер")
                        {
                            AddItemWindow.Visibility = Visibility.Hidden;
                        }
                    }
                }
                else
                {
                    UserNameTB.Text = $"Гость";
                    SearchTB.Visibility = Visibility.Hidden;
                    SupplierFilterCB.Visibility = Visibility.Hidden;
                    SortCB.Visibility = Visibility.Hidden;
                    AddItemWindow.Visibility = Visibility.Hidden;
                    Zakazi.Visibility = Visibility.Hidden;
                }

                allItems = context.Items
                    .Include(i => i.CategoryItemNavigation)
                    .Include(i => i.IdManufactureItemNavigation)
                    .Include(i => i.IdSupplierItemNavigation)
                    .Include(i => i.TypeItemNavigation)
                    .ToList();

                LoadFilters();
                ApplyFiltersAndSort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFilters()
        {
            try
            {

                while (SupplierFilterCB.Items.Count > 1)
                {
                    SupplierFilterCB.Items.RemoveAt(1);
                }

                if (allItems != null)
                {
                    // Загрузка поставщиков
                    var suppliers = allItems
                        .Where(i => i.IdSupplierItemNavigation != null)
                        .Select(i => i.IdSupplierItemNavigation.NameSupplier)
                        .Distinct()
                        .OrderBy(s => s)
                        .ToList();

                    foreach (var supplier in suppliers)
                    {
                        SupplierFilterCB.Items.Add(new ComboBoxItem { Content = supplier });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFiltersAndSort()
        {
            try
            {
                if (allItems == null) return;

                var filteredItems = allItems.AsEnumerable();

                // Поиск
                string searchText = SearchTB.Text?.ToLower() ?? "";
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredItems = filteredItems.Where(item =>
                        (item.CategoryItemNavigation?.NameItemCategory?.ToLower().Contains(searchText) ?? false) ||
                        (item.DescriptionItem?.ToLower().Contains(searchText) ?? false) ||
                        (item.IdManufactureItemNavigation?.NameManufacture?.ToLower().Contains(searchText) ?? false) ||
                        (item.IdSupplierItemNavigation?.NameSupplier?.ToLower().Contains(searchText) ?? false)
                    );
                }

                // Фильтр по поставщику
                if (SupplierFilterCB.SelectedItem is ComboBoxItem supplierItem &&
                    supplierItem.Content.ToString() != "Все поставщики")
                {
                    string selectedSupplier = supplierItem.Content.ToString();
                    filteredItems = filteredItems.Where(item =>
                        item.IdSupplierItemNavigation?.NameSupplier == selectedSupplier);
                }

                // Сортировка
                if (SortCB.SelectedItem is ComboBoxItem sortItem)
                {
                    string sortType = sortItem.Content.ToString();

                    switch (sortType)
                    {
                        case "По цене (возрастание)":
                            filteredItems = filteredItems.OrderBy(item => item.PriceItem);
                            break;
                        case "По цене (убывание)":
                            filteredItems = filteredItems.OrderByDescending(item => item.PriceItem);
                            break;
                        case "По количеству (возрастание)":
                            filteredItems = filteredItems
                                .OrderBy(item => item.CountInStorageItem)
                                .ThenBy(item => item.ArticuleItem);
                            break;
                        case "По количеству (убывание)":
                            filteredItems = filteredItems
                                .OrderByDescending(item => item.CountInStorageItem)
                                .ThenBy(item => item.ArticuleItem);
                            break;
                    }
                }

                var resultList = filteredItems.ToList();
                ItemsLB.ItemsSource = resultList;

                ResultCountTB.Text = $"Найдено товаров: {resultList.Count} из {allItems.Count}";

                // Изменение цвета в зависимости от количества
                if (resultList.Count == 0)
                {
                    ResultCountTB.Foreground = Brushes.Red;
                    MessageBox.Show("Товары не найдены");
                }
                else if (resultList.Count < 5)
                {
                    ResultCountTB.Foreground = Brushes.Orange;
                }
                else
                {
                    ResultCountTB.Foreground = Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка применения фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void Sort_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }


        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (contextMenu != null)
            {
                foreach (var item in contextMenu.Items)
                {
                    if (item is MenuItem menuItem)
                    {
                        if (menuItem.Header.ToString() == "Редактировать")
                            EditMenuItem = menuItem;
                        else if (menuItem.Header.ToString() == "Удалить")
                            DeleteMenuItem = menuItem;
                    }
                }
            }

            if (!_isAdmin)
            {
                // Если не админ
                if (EditMenuItem != null) EditMenuItem.Visibility = Visibility.Collapsed;
                if (DeleteMenuItem != null) DeleteMenuItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Если админ - показываем оба пункта
                if (EditMenuItem != null) EditMenuItem.Visibility = Visibility.Visible;
                if (DeleteMenuItem != null) DeleteMenuItem.Visibility = Visibility.Visible;
            }
        }


        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var menuItem = sender as MenuItem;
                var contextMenu = menuItem?.Parent as ContextMenu;
                var border = contextMenu?.PlacementTarget as Border;
                var item = border?.DataContext as Item;

                if (item != null)
                {
                    EditItemWindow editWindow = new EditItemWindow(item);
                    editWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии редактирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, является ли пользователь администратором
                if (!_isAdmin)
                {
                    MessageBox.Show("Только администратор может удалять товары",
                        "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var menuItem = sender as MenuItem;
                var contextMenu = menuItem?.Parent as ContextMenu;
                var border = contextMenu?.PlacementTarget as Border;
                var item = border?.DataContext as Item;

                if (item != null)
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"Вы действительно хотите удалить товар '{item.ArticuleItem} - {item.DescriptionItem}'?\n\nЭто действие нельзя отменить!",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        DeleteItemFromDatabase(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteItemFromDatabase(Item item)
        {
            try
            {
                using (var deleteContext = new ShoeContext())
                {
                    var hasOrders = deleteContext.OrderItems.Any(oi => oi.ArticulItem == item.ArticuleItem);

                    if (hasOrders)
                    {
                        MessageBox.Show(
                            "Невозможно удалить товар, так как он присутствует в заказах. Сначала удалите связанные заказы.",
                            "Ошибка удаления",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var itemToDelete = deleteContext.Items
                        .FirstOrDefault(i => i.ArticuleItem == item.ArticuleItem);

                    if (itemToDelete != null)
                    {
                        deleteContext.Items.Remove(itemToDelete);
                        deleteContext.SaveChanges();

                        MessageBox.Show("Товар успешно удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Товар не найден в базе данных.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении из базы данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ItemsLB_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!_isAdmin)
                {
                    MessageBox.Show("Только администратор может редактировать товары",
                        "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ItemsLB.SelectedItem is Item selectedItem)
                {
                    EditItemWindow editWindow = new EditItemWindow(selectedItem);
                    editWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии редактирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы точно хотите выйти ?", "Уведомление");
            Exit();
        }

        public void Exit()
        {
            new Authorization().Show();
            this.Close();
        }

        private void AddItemWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isAdmin)
                {
                    MessageBox.Show("Только администратор может добавлять товары",
                        "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                EditItemWindow editWindow = new EditItemWindow(null);
                editWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна добавления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Zakazi_Click(object sender, RoutedEventArgs e)
        {
            Window window = new ZakaziWindow();
            window.Show();
            this.Close();
        }
    }
}