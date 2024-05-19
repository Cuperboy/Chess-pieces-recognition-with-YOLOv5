using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfApp1
{
    public class PrecisionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double precision)
            {
                return precision.ToString();
            }
            return Binding.DoNothing;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number;
            bool isValid = double.TryParse((string)value, out number);

            return isValid ? number : Binding.DoNothing;
        }
    }
}
