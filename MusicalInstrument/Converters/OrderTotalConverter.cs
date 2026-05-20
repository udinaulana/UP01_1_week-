using MusicalInstrument.DataBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MusicalInstrument.Converters
{
    public class OrderTotalConverter : IValueConverter
    {
        // Создаем подключение к базе данных
        DBEntities db = new DBEntities();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, что пришел объект заказа
            if (value is PrkOrders order)
            {
                try
                {
                    // Получаем все детали заказа
                    var orderDetails = db.PrkOrderDetails
                        .Where(x => x.IdOrder == order.IdOrder)
                        .ToList();

                    decimal totalSum = 0;

                    // Вычисляем сумму по каждому товару
                    foreach (var detail in orderDetails)
                    {
                        // Получаем товар
                        var product = db.PrlProducts.Find(detail.IdProduct);

                        if (product != null)
                        {
                            // Учитываем скидку
                            decimal price = product.Price;
                            decimal discount = product.Discount;
                            decimal priceWithDiscount = price - (price * discount / 100);

                            // Добавляем к общей сумме
                            totalSum += priceWithDiscount * detail.Quantity;
                        }
                    }

                    // Возвращаем отформатированную сумму
                    return totalSum.ToString("F2") + " руб.";
                }
                catch (Exception ex)
                {
                    return "0 руб.";
                }
            }

            return "0 руб.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
