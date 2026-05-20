using MusicalInstrument.DataBase;
using MusicalInstrument.Pages;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для OrdersListWindow.xaml
    /// </summary>
    public partial class OrdersListWindow : Window
    {
        DBEntities db = new DBEntities();
        private PrkOrders selectedOrder;

        public OrdersListWindow()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            db.Configuration.ProxyCreationEnabled = false;

            var orders = db.PrkOrders
                .Include("PrkUsers")
                .Include("PrkOrderStatuses")
                .ToList();

            OrdersGrid.ItemsSource = orders;
            ApplyRowColors();
        }

        private void ApplyRowColors()
        {
            OrdersGrid.LoadingRow += (sender, e) =>
            {
                var order = e.Row.DataContext as PrkOrders;
                if (order != null)
                {
                    var items = db.PrkOrderDetails.Where(x => x.IdOrder == order.IdOrder).ToList();
                    bool hasOutOfStock = false;
                    int totalQuantity = 0;

                    foreach (var item in items)
                    {
                        var product = db.PrlProducts.Find(item.IdProduct);
                        totalQuantity += item.Quantity;
                        if (product != null && product.QuantityInStock == "0")
                        {
                            hasOutOfStock = true;
                        }
                    }

                    if (hasOutOfStock)
                    {
                        e.Row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff8c00"));
                        e.Row.Foreground = Brushes.White;
                    }
                    else if (totalQuantity > 3)
                    {
                        e.Row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20b2aa"));
                        e.Row.Foreground = Brushes.White;
                    }
                    else
                    {
                        e.Row.Background = Brushes.White;
                        e.Row.Foreground = Brushes.Black;
                    }
                }
            };
        }

        private decimal GetOrderTotal(int orderId)
        {
            var items = db.PrkOrderDetails.Where(x => x.IdOrder == orderId).ToList();
            decimal total = 0;
            foreach (var item in items)
            {
                var product = db.PrlProducts.Find(item.IdProduct);
                if (product != null)
                {
                    decimal price = product.Price;
                    decimal discount = product.Discount;
                    decimal priceWithDiscount = price - (price * discount / 100);
                    total += priceWithDiscount * item.Quantity;
                }
            }
            return total;
        }

        private void sortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var orders = OrdersGrid.ItemsSource as System.Collections.IEnumerable;
            if (orders == null) return;

            var orderList = orders.Cast<PrkOrders>().ToList();

            switch (sortBox.SelectedIndex)
            {
                case 1:
                    OrdersGrid.ItemsSource = orderList.OrderBy(x => GetOrderTotal(x.IdOrder)).ToList();
                    break;
                case 2:
                    OrdersGrid.ItemsSource = orderList.OrderByDescending(x => GetOrderTotal(x.IdOrder)).ToList();
                    break;
                default:
                    OrdersGrid.ItemsSource = orderList;
                    break;
            }
            ApplyRowColors();
        }

        private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedOrder = OrdersGrid.SelectedItem as PrkOrders;
        }

        private void OrderDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для просмотра деталей!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Открываем новое окно с деталями
            var detailsWindow = new OrderDetailsWindow(selectedOrder);
            detailsWindow.Owner = this;
            detailsWindow.ShowDialog();
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем страницу редактирования товаров для администратора
            // Нужно определить текущего пользователя
            var user = db.PrkUsers.FirstOrDefault(u => u.IdRole == 1); // Администратор

            if (user != null)
            {
                var adminPage = new AdministratorPage(user);
                var window = new Window
                {
                    Title = "Редактирование товаров",
                    Content = adminPage,
                    Width = 1200,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Icon = this.Icon
                };
                window.ShowDialog();
            }
            else
            {
                MessageBox.Show("Ошибка: пользователь-администратор не найден!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}