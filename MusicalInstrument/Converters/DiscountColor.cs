using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using MusicalInstrument.DataBase;

namespace MusicalInstrument.Converters
{
    public class DiscountColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PrlProducts product)
            {
                // Проверяем количество на складе
                if (product.QuantityInStock == "0")
                {
                    return new SolidColorBrush(Colors.LightGray);
                }

                // Проверяем скидку больше 15% - цвет #B8860B (Тёмное золото)
                if (product.Discount > 15)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B8860B"));
                }
            }

            // Обычный фон - белый
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}