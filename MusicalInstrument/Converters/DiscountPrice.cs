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
     public class DiscountPrice : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return "0 руб";
            if (values[0] == null || values[1] == null) return "0 руб";
            
            try
            {
                int price = System.Convert.ToInt32(values[0]);
                int discount = System.Convert.ToInt32(values[1]);
                
                if (discount > 0)
                {
                    double finalPrice = price - (price * discount / 100.0);
                    return finalPrice.ToString("F2") + " руб";
                }
                return price.ToString() + " руб";
            }
            catch
            {
                return "0 руб";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}