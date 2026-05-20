using MusicalInstrument.DataBase;
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
    /// Логика взаимодействия для OrderDetailsWindow.xaml
    /// </summary>
    public partial class OrderDetailsWindow : Window
    {
        DBEntities db = new DBEntities();
        PrkOrders order;
        private static bool isStatusDialogOpen = false;

        public OrderDetailsWindow(PrkOrders selectedOrder)
        {
            InitializeComponent();
            order = selectedOrder;

            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            db.Configuration.ProxyCreationEnabled = false;

            // Заголовок
            OrderHeaderBlock.Text = $"ЗАКАЗ № {order.CodeOrder}";

            // Информация о заказе
            OrderCodeBlock.Text = order.CodeOrder;
            OrderDateBlock.Text = order.DateOrder.ToString("dd.MM.yyyy");

            // Статус
            var status = db.PrkOrderStatuses.Find(order.IdOrderStatus);
            OrderStatusBlock.Text = status?.OrderStatus ?? "Неизвестно";

            // Цвет статуса
            if (order.IdOrderStatus == 1) // Завершен
                OrderStatusBlock.Foreground = System.Windows.Media.Brushes.Green;
            else if (order.IdOrderStatus == 2) // Новый
                OrderStatusBlock.Foreground = System.Windows.Media.Brushes.Orange;
            else
                OrderStatusBlock.Foreground = System.Windows.Media.Brushes.Gray;

            // Информация о клиенте
            var user = db.PrkUsers.Find(order.IdUser);
            if (user != null)
            {
                ClientFIOBlock.Text = $"{user.Surname} {user.Name} {user.Patronymic}";
            }

            // Товары в заказе
            var items = db.PrkOrderDetails
                .Include("PrlProducts")
                .Include("PrlProducts.PrkCategotyOrders")
                .Include("PrlProducts.PrkManufacturers")
                .Include("PrlProducts.PrkSuppliers")
                .Where(x => x.IdOrder == order.IdOrder)
                .ToList();

            ProductsList.ItemsSource = items;

            // Подсчет итогов
            double totalOriginal = 0;
            double totalDiscounted = 0;

            foreach (var item in items)
            {
                var product = item.PrlProducts;
                if (product != null)
                {
                    double price = product.Price;
                    double discount = product.Discount;
                    double priceWithDiscount = price - (price * discount / 100);

                    totalOriginal += price * item.Quantity;
                    totalDiscounted += priceWithDiscount * item.Quantity;
                }
            }

            double discountSum = totalOriginal - totalDiscounted;

            TotalOriginalBlock.Text = $"Сумма без скидки: {totalOriginal:F2} руб.";
            DiscountBlock.Text = $"Скидка: {discountSum:F2} руб.";
            TotalBlock.Text = $"Итого: {totalDiscounted:F2} руб.";
        }

        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            // Защита от открытия нескольких окон
            if (isStatusDialogOpen)
            {
                MessageBox.Show("Окно изменения статуса уже открыто!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var statuses = db.PrkOrderStatuses.ToList();

            // Создаем диалог выбора статуса
            var statusDialog = new Window
            {
                Title = "Изменение статуса заказа",
                Width = 350,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = System.Windows.Media.Brushes.White,
                ResizeMode = ResizeMode.NoResize
            };

            isStatusDialogOpen = true;
            statusDialog.Closed += (s, args) => isStatusDialogOpen = false;

            var stackPanel = new StackPanel { Margin = new Thickness(15) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Выберите новый статус:",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.DarkBlue,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var comboBox = new ComboBox
            {
                ItemsSource = statuses,
                DisplayMemberPath = "OrderStatus",
                SelectedValuePath = "IdOrderStatus",
                SelectedValue = order.IdOrderStatus,
                Height = 35,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

            var saveButton = new Button
            {
                Content = "Сохранить",
                Width = 100,
                Height = 35,
                Margin = new Thickness(5),
                Background = System.Windows.Media.Brushes.DarkViolet,
                Foreground = System.Windows.Media.Brushes.White,
                FontWeight = FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 100,
                Height = 35,
                Margin = new Thickness(5),
                Background = System.Windows.Media.Brushes.DarkGoldenrod,
                Foreground = System.Windows.Media.Brushes.White,
                FontWeight = FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(comboBox);
            stackPanel.Children.Add(buttonPanel);
            statusDialog.Content = stackPanel;

            saveButton.Click += (s, args) =>
            {
                if (comboBox.SelectedValue != null)
                {
                    order.IdOrderStatus = (int)comboBox.SelectedValue;
                    db.SaveChanges();

                    // Обновляем отображение статуса
                    var newStatus = db.PrkOrderStatuses.Find(order.IdOrderStatus);
                    OrderStatusBlock.Text = newStatus?.OrderStatus ?? "Неизвестно";

                    if (order.IdOrderStatus == 1)
                        OrderStatusBlock.Foreground = System.Windows.Media.Brushes.Green;
                    else if (order.IdOrderStatus == 2)
                        OrderStatusBlock.Foreground = System.Windows.Media.Brushes.Orange;
                    else
                        OrderStatusBlock.Foreground = System.Windows.Media.Brushes.Gray;

                    statusDialog.Close();
                    MessageBox.Show("Статус заказа изменен!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            cancelButton.Click += (s, args) => statusDialog.Close();

            statusDialog.ShowDialog();
        }
    }
}