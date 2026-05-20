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
    /// Логика взаимодействия для CartWindow.xaml
    /// </summary>
    public partial class CartWindow : Window
    {
        DBEntities db;
        PrkUsers user;
        PrkOrders order;

        public CartWindow(PrkUsers user, PrkOrders order)
        {
            db = new DBEntities();
            db.Configuration.ProxyCreationEnabled = false;  // ДОБАВИТЬ ЭТУ СТРОКУ

            InitializeComponent();
            this.user = user;
            this.order = order;

            FIOBlock.Text = $"{user.Surname} {user.Name} {user.Patronymic}";
            LoadCart();
        }

        private void LoadCart()
        {
            var items = db.PrkOrderDetails
                .Include("PrlProducts")
                .Include("PrlProducts.PrkCategotyOrders")
                .Include("PrlProducts.PrkManufacturers")
                .Include("PrlProducts.PrkSuppliers")
                .Include("PrlProducts.PrkUunitOfMeasurements")
                .Where(x => x.IdOrder == order.IdOrder)
                .ToList();

            CartList.ItemsSource = items;
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            double totalOriginal = 0;
            double totalDiscounted = 0;

            var items = db.PrkOrderDetails.Where(x => x.IdOrder == order.IdOrder).ToList();

            foreach (var item in items)
            {
                var product = db.PrlProducts.Find(item.IdProduct);
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

            TotalOriginalBlock.Text = $"Сумма: {totalOriginal:F2} руб.";
            DiscountBlock.Text = $"Скидка: {discountSum:F2} руб.";
            TotalBlock.Text = $"Итого: {totalDiscounted:F2} руб.";
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var detail = btn?.Tag as PrkOrderDetails;

            if (detail != null)
            {
                var freshDetail = db.PrkOrderDetails.Find(detail.IdOrderDetail);
                if (freshDetail != null)
                {
                    freshDetail.Quantity++;
                    db.SaveChanges();
                    LoadCart();
                }
            }
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var detail = btn?.Tag as PrkOrderDetails;

            if (detail != null)
            {
                var freshDetail = db.PrkOrderDetails.Find(detail.IdOrderDetail);
                if (freshDetail != null && freshDetail.Quantity > 1)
                {
                    freshDetail.Quantity--;
                    db.SaveChanges();
                    LoadCart();
                }
                else if (freshDetail != null && freshDetail.Quantity == 1)
                {
                    RemoveItem(freshDetail);
                }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var detail = btn?.Tag as PrkOrderDetails;

            if (detail != null)
            {
                var freshDetail = db.PrkOrderDetails.Find(detail.IdOrderDetail);
                if (freshDetail != null)
                {
                    RemoveItem(freshDetail);
                }
            }
        }

        private void RemoveItem(PrkOrderDetails detail)
        {
            try
            {
                int orderId = detail.IdOrder;

                db.PrkOrderDetails.Remove(detail);
                db.SaveChanges();

                var remainingItems = db.PrkOrderDetails
                    .Where(x => x.IdOrder == orderId)
                    .Count();

                if (remainingItems == 0)
                {
                    var orderToDelete = db.PrkOrders.Find(orderId);
                    if (orderToDelete != null)
                    {
                        db.PrkOrders.Remove(orderToDelete);
                        db.SaveChanges();
                    }

                    DialogResult = true;
                    Close();
                }
                else
                {
                    LoadCart();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var freshOrder = db.PrkOrders.Find(order.IdOrder);
                if (freshOrder == null)
                {
                    MessageBox.Show("Заказ не найден!");
                    return;
                }

                var items = db.PrkOrderDetails.Where(x => x.IdOrder == freshOrder.IdOrder).ToList();

                if (items.Count == 0)
                {
                    MessageBox.Show("Корзина пуста!");
                    return;
                }

                double totalQuantity = items.Sum(x => x.Quantity);
                if (totalQuantity < 3)
                    freshOrder.DateDelivery = DateTime.Now.AddDays(6);
                else
                    freshOrder.DateDelivery = DateTime.Now.AddDays(3);

                freshOrder.IdOrderStatus = 1;

                foreach (var item in items)
                {
                    var product = db.PrlProducts.Find(item.IdProduct);
                    if (product != null && int.TryParse(product.QuantityInStock, out int currentStock))
                    {
                        product.QuantityInStock = (currentStock - item.Quantity).ToString();
                    }
                }

                db.SaveChanges();

                var orderWindow = new OrderWindow(user, freshOrder);
                orderWindow.Owner = this;
                orderWindow.ShowDialog();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}");
            }
        }
        public void Dispose()
        {
            db?.Dispose();
        }

    }
}