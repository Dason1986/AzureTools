using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AzureTools.Converters
{
    public class StringBooleanConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                string target = parameter as string;
                return string.Equals(str, target);
            }

            return false;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                string target = parameter as string;
                return string.Equals(str, target);
            }
            return false;
        }
    }
}
