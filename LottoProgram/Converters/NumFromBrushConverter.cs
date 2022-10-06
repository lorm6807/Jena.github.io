using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace LottoProgram.Converters
{
    public class NumFromBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = null;

            int num = (int)value;

            if (num < 10)
                brush = Brushes.Yellow;
            else if (num >= 10 && num < 20)
                brush = Brushes.SkyBlue;
            else if (num >= 20 && num < 30)
                brush = Brushes.IndianRed;
            else if (num >= 30 && num < 40)
                brush = Brushes.Gray;
            else if (num >= 40 && num < 50)
                brush = Brushes.ForestGreen;

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
