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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicalInstrument.DataBase;
using MusicalInstrument.Windows;

namespace MusicalInstrument.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        DBEntities db;
        PrkUsers user;
        Random random = new Random();

        public ClientPage(PrkUsers user)
        {
            db = new DBEntities();
            db.Configuration.ProxyCreationEnabled = false;  // ДОБАВИТЬ ЭТУ СТРОКУ

            InitializeComponent();
            this.user = user;

            RefreshProductsList();

            FIOBlok.Text = $"{user.Surname} {user.Name} {user.Patronymic}";

            var role = db.PrkRoles.FirstOrDefault(r => r.IdRole == user.IdRole);
            RoleBlok.Text = role?.Role ?? "Клиент";

            visibleButton();
        }

        public void RefreshProductsList()
        {
            ProductsList.ItemsSource = null;
            ProductsList.ItemsSource = db.PrlProducts.ToList();
        }

        public void visibleButton()
        {
            try
            {
                var activeOrder = db.PrkOrders.FirstOrDefault(x => x.IdUser == user.IdUser && x.IdOrderStatus == 2);

                if (activeOrder != null)
                {
                    var itemsCount = db.PrkOrderDetails.Count(x => x.IdOrder == activeOrder.IdOrder);

                    if (itemsCount > 0)
                    {
                        cartButton.Visibility = Visibility.Visible;
                        viewOrdersButton.Visibility = Visibility.Visible;
                        return;
                    }
                }

                cartButton.Visibility = Visibility.Collapsed;
                viewOrdersButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menuItem = sender as MenuItem;
                PrlProducts product = menuItem.Tag as PrlProducts;

                if (product == null)
                {
                    MessageBox.Show("Ошибка: товар не найден");
                    return;
                }

                if (product.QuantityInStock == "0")
                {
                    MessageBox.Show("Товар отсутствует на складе!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Ищем активный заказ
                var order = db.PrkOrders.FirstOrDefault(x => x.IdUser == user.IdUser && x.IdOrderStatus == 2);

                if (order == null)
                {
                    // Ручное присвоение IdOrder
                    int newOrderId = 1;
                    if (db.PrkOrders.Any())
                        newOrderId = db.PrkOrders.Max(x => x.IdOrder) + 1;

                    order = new PrkOrders
                    {
                        IdOrder = newOrderId,
                        DateOrder = DateTime.Now,
                        DateDelivery = DateTime.Now.AddDays(3),
                        IdPickUpPoint = 1,
                        IdUser = user.IdUser,
                        CodeOrder = random.Next(1000, 9999).ToString(),
                        IdOrderStatus = 2
                    };
                    db.PrkOrders.Add(order);
                    db.SaveChanges();
                }

                // Проверяем, есть ли уже этот товар в корзине
                var existingItem = db.PrkOrderDetails
                    .FirstOrDefault(x => x.IdOrder == order.IdOrder && x.IdProduct == product.IdProduct);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    // Ручное присвоение IdOrderDetail
                    int newDetailId = 1;
                    if (db.PrkOrderDetails.Any())
                        newDetailId = db.PrkOrderDetails.Max(x => x.IdOrderDetail) + 1;

                    PrkOrderDetails orderDetail = new PrkOrderDetails
                    {
                        IdOrderDetail = newDetailId,
                        IdOrder = order.IdOrder,
                        IdProduct = product.IdProduct,
                        Quantity = 1
                    };
                    db.PrkOrderDetails.Add(orderDetail);
                }

                db.SaveChanges();
                visibleButton();

                MessageBox.Show($"Товар \"{product.NameOrder}\" добавлен в корзину!",
                    "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }

        private void viewOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new DBEntities())
            {
                var activeOrder = db.PrkOrders
                    .FirstOrDefault(x => x.IdUser == user.IdUser && x.IdOrderStatus == 2);

                if (activeOrder == null)
                {
                    MessageBox.Show("Корзина пуста!", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var itemsCount = db.PrkOrderDetails.Count(x => x.IdOrder == activeOrder.IdOrder);
                if (itemsCount == 0)
                {
                    MessageBox.Show("Корзина пуста!", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    cartButton.Visibility = Visibility.Collapsed;
                    viewOrdersButton.Visibility = Visibility.Collapsed;
                    return;
                }

                var cartWindow = new CartWindow(user, activeOrder);
                cartWindow.Owner = Window.GetWindow(this);
                cartWindow.ShowDialog();
            }

            // Обновляем после закрытия окна
            visibleButton();
            RefreshProductsList();
        }

        private void cartButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new DBEntities())
            {
                var activeOrder = db.PrkOrders
                    .FirstOrDefault(x => x.IdUser == user.IdUser && x.IdOrderStatus == 2);

                if (activeOrder == null)
                {
                    MessageBox.Show("Корзина пуста!", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var itemsCount = db.PrkOrderDetails.Count(x => x.IdOrder == activeOrder.IdOrder);
                if (itemsCount == 0)
                {
                    MessageBox.Show("Корзина пуста!", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    cartButton.Visibility = Visibility.Collapsed;
                    viewOrdersButton.Visibility = Visibility.Collapsed;
                    return;
                }

                var cartWindow = new CartWindow(user, activeOrder);
                cartWindow.Owner = Window.GetWindow(this);
                cartWindow.ShowDialog();
            }

            // Обновляем после закрытия окна
            visibleButton();
            RefreshProductsList();
        }

        private void Border_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border != null && border.ContextMenu != null)
            {
                border.ContextMenu.IsOpen = true;
            }
        }
        public void Dispose()
        {
            db?.Dispose();
        }
    }
}