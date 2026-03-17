using Demka.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Demka.window_xaml
{
    public partial class EditItemWindow : Window
    {
        private Item _currentItem;
        private ShoeContext _context;
        private byte[] _selectedPhotoBytes;
        private bool _isNewItem;

        private const int MAX_ARTICULE_LENGTH = 6;
        private const int MAX_DESCRIPTION_LENGTH = 500;
        private const int MAX_MEASUREMENT_LENGTH = 20;
        private const int DEFAULT_TYPE_ITEM = 1;

        // Требуемый размер изображения
        private const int REQUIRED_IMAGE_WIDTH = 300;
        private const int REQUIRED_IMAGE_HEIGHT = 200;

        public EditItemWindow(Item item)
        {
            InitializeComponent();
            MeasurementTB.Text = "шт.";
            
            MeasurementTB.IsEnabled = false;
            _context = new ShoeContext();
            _isNewItem = (item == null);

            if (_isNewItem)
            {
                _currentItem = new Item
                {
                    ArticuleItem = "",
                    DescriptionItem = "",
                    MeasurementItem = "",
                    PriceItem = 0,
                    DiscountItem = 0,
                    CountInStorageItem = 0,
                    PhotoItem = null,
                    TypeItem = DEFAULT_TYPE_ITEM
                };

                this.Title = "Добавление нового товара";
                TitleText.Text = "Добавление нового товара";

                ArticuleTB.IsReadOnly = false;
                ArticuleTB.Background = System.Windows.Media.Brushes.White;
                ArticuleTB.Text = "";
            }
            else
            {
                _context.Entry(item).State = EntityState.Detached;

                _currentItem = _context.Items
                    .Include(i => i.CategoryItemNavigation)
                    .Include(i => i.IdManufactureItemNavigation)
                    .Include(i => i.IdSupplierItemNavigation)
                    .FirstOrDefault(i => i.ArticuleItem == item.ArticuleItem);

                if (_currentItem == null)
                {
                    _currentItem = item;
                }

                this.Title = "Редактирование товара";
                TitleText.Text = "Редактирование товара";

                ArticuleTB.IsReadOnly = true;
                ArticuleTB.Background = System.Windows.Media.Brushes.LightGray;
            }

            LoadComboBoxes();
            LoadItemData();
        }

        private void LoadComboBoxes()
        {
            try
            {
                var categories = _context.ItemCategories.AsNoTracking().ToList();
                CategoryCB.ItemsSource = categories;
                System.Diagnostics.Debug.WriteLine($"Загружено категорий: {categories.Count}");

                var manufactures = _context.Manufactures.AsNoTracking().ToList();
                ManufactureCB.ItemsSource = manufactures;
                System.Diagnostics.Debug.WriteLine($"Загружено производителей: {manufactures.Count}");

                var suppliers = _context.Suppliers.AsNoTracking().ToList();
                SupplierCB.ItemsSource = suppliers;
                System.Diagnostics.Debug.WriteLine($"Загружено поставщиков: {suppliers.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadItemData()
        {
            try
            {
                ArticuleTB.Text = _currentItem.ArticuleItem ?? "";
                DescriptionTB.Text = _currentItem.DescriptionItem ?? "";
                MeasurementTB.Text = "шт." ?? "";
                //MeasurementTB.Text = _currentItem.MeasurementItem ?? "";
                PriceTB.Text = _currentItem.PriceItem.ToString();
                DiscountTB.Text = _currentItem.DiscountItem.ToString();
                CountTB.Text = _currentItem.CountInStorageItem.ToString();

                if (_currentItem.IdSupplierItem > 0)
                    SupplierCB.SelectedValue = _currentItem.IdSupplierItem;

                if (_currentItem.IdManufactureItem > 0)
                    ManufactureCB.SelectedValue = _currentItem.IdManufactureItem;

                if (_currentItem.CategoryItem > 0)
                    CategoryCB.SelectedValue = _currentItem.CategoryItem;

                if (_currentItem.PhotoItem != null && _currentItem.PhotoItem.Length > 0)
                {
                    PreviewImage.Source = _currentItem.Photo;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateFieldLengths()
        {
            if (_isNewItem && ArticuleTB.Text.Length > MAX_ARTICULE_LENGTH)
            {
                MessageBox.Show($"Артикул не может быть длиннее {MAX_ARTICULE_LENGTH} символов",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DescriptionTB.Text.Length > MAX_DESCRIPTION_LENGTH)
            {
                MessageBox.Show($"Описание не может быть длиннее {MAX_DESCRIPTION_LENGTH} символов",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (MeasurementTB.Text.Length > MAX_MEASUREMENT_LENGTH)
            {
                MessageBox.Show($"Единица измерения не может быть длиннее {MAX_MEASUREMENT_LENGTH} символов",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Метод для проверки размера изображения
        private bool ValidateImageSize(string filePath)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(filePath);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();

                if (image.PixelWidth != REQUIRED_IMAGE_WIDTH || image.PixelHeight != REQUIRED_IMAGE_HEIGHT)
                {
                    MessageBox.Show($"Изображение должно быть размером {REQUIRED_IMAGE_WIDTH}x{REQUIRED_IMAGE_HEIGHT} пикселей.\n" +
                                  $"Текущий размер: {image.PixelWidth}x{image.PixelHeight}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке изображения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void SelectPhotoBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
                openFileDialog.Title = "Выберите изображение для товара";

                if (openFileDialog.ShowDialog() == true)
                {
                    // Проверяем размер изображения
                    if (!ValidateImageSize(openFileDialog.FileName))
                    {
                        return; // Прерываем загрузку, если размер не подходит
                    }

                    _selectedPhotoBytes = File.ReadAllBytes(openFileDialog.FileName);

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = new MemoryStream(_selectedPhotoBytes);
                    image.EndInit();
                    PreviewImage.Source = image;

                    MessageBox.Show($"Изображение успешно загружено! Размер: {image.PixelWidth}x{image.PixelHeight}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ArticuleTB.Text) && _isNewItem)
                {
                    MessageBox.Show("Введите артикул товара", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(DescriptionTB.Text))
                {
                    MessageBox.Show("Введите описание товара", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(MeasurementTB.Text))
                {
                    MessageBox.Show("Введите единицу измерения", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateFieldLengths())
                {
                    return;
                }

                if (!decimal.TryParse(PriceTB.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Введите корректную цену (положительное число)", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(DiscountTB.Text, out int discount))
                {
                    discount = 0;
                }
                else if (discount > 99)
                {
                    MessageBox.Show("Не возможно добавить товар, скидка огромная");
                    return;
                }

                if (!int.TryParse(CountTB.Text, out int count))
                {
                    MessageBox.Show("Введите корректное количество", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else if (count == 0 || count < 0) {
                    MessageBox.Show("Добавте количество на складе");
                    return;
                }

                if (SupplierCB.SelectedValue == null)
                {
                    MessageBox.Show("Выберите поставщика", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ManufactureCB.SelectedValue == null)
                {
                    MessageBox.Show("Выберите производителя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CategoryCB.SelectedValue == null)
                {
                    MessageBox.Show("Выберите категорию", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, что изображение было загружено (для нового товара)
                if (_isNewItem && _selectedPhotoBytes == null)
                {
                    MessageBox.Show("Загрузите изображение товара размером 300x200 пикселей", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isNewItem)
                {
                    _currentItem.ArticuleItem = ArticuleTB.Text.Trim();
                }

                _currentItem.DescriptionItem = DescriptionTB.Text.Trim();
                _currentItem.MeasurementItem = MeasurementTB.Text.Trim();
                _currentItem.PriceItem = price;
                _currentItem.IdSupplierItem = (int)SupplierCB.SelectedValue;
                _currentItem.IdManufactureItem = (int)ManufactureCB.SelectedValue;
                _currentItem.CategoryItem = (int)CategoryCB.SelectedValue;
                _currentItem.DiscountItem = discount;
                _currentItem.CountInStorageItem = count;

                if (_selectedPhotoBytes != null)
                {
                    _currentItem.PhotoItem = _selectedPhotoBytes;
                }

                if (_currentItem.TypeItem == 0)
                {
                    _currentItem.TypeItem = DEFAULT_TYPE_ITEM;
                }

                using (var saveContext = new ShoeContext())
                {
                    if (_isNewItem)
                    {
                        var existingItem = saveContext.Items
                            .FirstOrDefault(i => i.ArticuleItem == _currentItem.ArticuleItem);

                        if (existingItem != null)
                        {
                            MessageBox.Show("Товар с таким артикулом уже существует", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        saveContext.Items.Add(_currentItem);
                    }
                    else
                    {
                        _currentItem.CategoryItemNavigation = null;
                        _currentItem.IdManufactureItemNavigation = null;
                        _currentItem.IdSupplierItemNavigation = null;

                        saveContext.Items.Update(_currentItem);
                    }

                    try
                    {
                        saveContext.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        string errorMessage = "Ошибка базы данных:\n";

                        if (ex.InnerException != null)
                        {
                            errorMessage += ex.InnerException.Message;

                            if (ex.InnerException.Message.Contains("FOREIGN KEY"))
                            {
                                errorMessage = "Ошибка внешнего ключа. Проверьте, что выбранные поставщик, производитель и категория существуют в базе данных.";
                            }
                            else if (ex.InnerException.Message.Contains("UNIQUE") || ex.InnerException.Message.Contains("Duplicate"))
                            {
                                errorMessage = "Нарушение уникальности. Возможно, товар с таким артикулом уже существует.";
                            }
                            else if (ex.InnerException.Message.Contains("Data truncated"))
                            {
                                errorMessage = "Слишком длинное значение в одном из полей. Проверьте:\n" +
                                              $"- Артикул (макс. {MAX_ARTICULE_LENGTH} символов)\n" +
                                              $"- Описание (макс. {MAX_DESCRIPTION_LENGTH} символов)\n" +
                                              $"- Единица измерения (макс. {MAX_MEASUREMENT_LENGTH} символов)";
                            }
                        }
                        else
                        {
                            errorMessage += ex.Message;
                        }

                        MessageBox.Show($"Ошибка при сохранении: {errorMessage}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show("Товар успешно сохранен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ReturnToUserWindow();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                }

                MessageBox.Show($"Ошибка при сохранении: {errorMessage}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ReturnToUserWindow();
        }

        private void ReturnToUserWindow()
        {
            UserWindow userWindow = new UserWindow();
            userWindow.Show();
            this.Close();
        }
    }
}