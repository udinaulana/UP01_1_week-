using Microsoft.Win32;
using MusicalInstrument.DataBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicalInstrument.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProductEditWindow.xaml
    /// </summary>
    public partial class ProductEditWindow : Window
    {
        DBEntities db = new DBEntities();
        PrlProducts editingProduct;
        private byte[] productPhoto;
        private static bool isWindowOpen = false;

        public ProductEditWindow(PrlProducts product = null)
        {
            // Защита от открытия нескольких окон
            if (isWindowOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            InitializeComponent();
            isWindowOpen = true;
            this.Closed += (s, e) => isWindowOpen = false;

            editingProduct = product;

            LoadComboBoxes();

            if (editingProduct != null)
            {
                // Режим редактирования
                TitleBlock.Text = "Редактирование товара";
                IdPanel.Visibility = Visibility.Visible;
                LoadProductData();
            }
            else
            {
                // Режим добавления
                TitleBlock.Text = "Добавление товара";
                IdPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadComboBoxes()
        {
            CategoryBox.ItemsSource = db.PrkCategotyOrders.ToList();
            ManufacturerBox.ItemsSource = db.PrkManufacturers.ToList();
            SupplierBox.ItemsSource = db.PrkSuppliers.ToList();
            UnitBox.ItemsSource = db.PrkUunitOfMeasurements.ToList();
        }

        private void LoadProductData()
        {
            IdBox.Text = editingProduct.IdProduct.ToString();
            NameBox.Text = editingProduct.NameOrder;
            DescriptionBox.Text = editingProduct.Description;
            PriceBox.Text = editingProduct.Price.ToString();
            QuantityBox.Text = editingProduct.QuantityInStock;
            DiscountBox.Text = editingProduct.Discount.ToString();

            CategoryBox.SelectedValue = editingProduct.IdCategotyOrder;
            ManufacturerBox.SelectedValue = editingProduct.IdManufacturer;
            SupplierBox.SelectedValue = editingProduct.IdSupplier;
            UnitBox.SelectedValue = editingProduct.IdUnitOfMeasurements;

            if (editingProduct.Photo != null && editingProduct.Photo.Length > 0)
            {
                productPhoto = editingProduct.Photo;
                PhotoImage.Source = ByteArrayToImage(editingProduct.Photo);
            }
            else
            {
                PhotoImage.Source = new BitmapImage(new Uri("/Images/picture.png", UriKind.Relative));
            }
        }

        private void LoadPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";
            dialog.Title = "Выберите фото товара";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Загружаем изображение с изменением размера до 300x200
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(dialog.FileName);
                    bitmap.DecodePixelWidth = 300;
                    bitmap.DecodePixelHeight = 200;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    PhotoImage.Source = bitmap;
                    productPhoto = ImageToByteArray(bitmap);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            PhotoImage.Source = new BitmapImage(new Uri("/Images/picture.png", UriKind.Relative));
            productPhoto = null;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(NameBox.Text))
                {
                    MessageBox.Show("Введите наименование товара!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(PriceBox.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Введите корректную цену (неотрицательное число)!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity < 0)
                {
                    MessageBox.Show("Введите корректное количество (неотрицательное число)!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(DiscountBox.Text, out int discount) || discount < 0 || discount > 100)
                {
                    MessageBox.Show("Введите корректную скидку (от 0 до 100)!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CategoryBox.SelectedValue == null)
                {
                    MessageBox.Show("Выберите категорию товара!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ManufacturerBox.SelectedValue == null)
                {
                    MessageBox.Show("Выберите производителя!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SupplierBox.SelectedValue == null)
                {
                    MessageBox.Show("Выберите поставщика!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (UnitBox.SelectedValue == null)
                {
                    MessageBox.Show("Выберите единицу измерения!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (editingProduct == null)
                {
                    // ДОБАВЛЕНИЕ: автоматически вычисляем ID (+1 к существующему)
                    int newId = 1;
                    if (db.PrlProducts.Any())
                        newId = db.PrlProducts.Max(x => x.IdProduct) + 1;

                    var newProduct = new PrlProducts
                    {
                        IdProduct = newId,
                        NameOrder = NameBox.Text,
                        Description = DescriptionBox.Text,
                        Article = $"ART{newId:D4}",
                        Price = (int)price,
                        QuantityInStock = quantity.ToString(),
                        Discount = discount,
                        IdCategotyOrder = (int)CategoryBox.SelectedValue,
                        IdManufacturer = (int)ManufacturerBox.SelectedValue,
                        IdSupplier = (int)SupplierBox.SelectedValue,
                        IdUnitOfMeasurements = (int)UnitBox.SelectedValue,
                        Photo = productPhoto
                    };
                    db.PrlProducts.Add(newProduct);
                }
                else
                {
                    // РЕДАКТИРОВАНИЕ
                    editingProduct.NameOrder = NameBox.Text;
                    editingProduct.Description = DescriptionBox.Text;
                    editingProduct.Price = (int)price;
                    editingProduct.QuantityInStock = quantity.ToString();
                    editingProduct.Discount = discount;
                    editingProduct.IdCategotyOrder = (int)CategoryBox.SelectedValue;
                    editingProduct.IdManufacturer = (int)ManufacturerBox.SelectedValue;
                    editingProduct.IdSupplier = (int)SupplierBox.SelectedValue;
                    editingProduct.IdUnitOfMeasurements = (int)UnitBox.SelectedValue;

                    if (productPhoto != null)
                        editingProduct.Photo = productPhoto;
                }

                db.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private byte[] ImageToByteArray(BitmapImage bitmap)
        {
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        private BitmapImage ByteArrayToImage(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }
    }
}