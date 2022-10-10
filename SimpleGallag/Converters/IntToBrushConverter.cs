using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SimpleGallag.Converters
{
    public class IntToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int xPos = (int)value;
            int parm = Int32.Parse(parameter.ToString());

            if (xPos == parm)
                return Brushes.Red;
            else
                return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
