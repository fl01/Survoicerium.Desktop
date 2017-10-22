using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Survoicerium.Client.Converters
{
    public class ApiKeyStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (int)value;
            int iconId = System.Convert.ToInt32(parameter);

            return status == iconId ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
