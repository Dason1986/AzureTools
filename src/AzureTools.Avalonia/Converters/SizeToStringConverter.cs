using Avalonia.Data.Converters;
using AzureTools.Extensions;
using System;
using System.Globalization;

namespace AzureTools.Converters
{
    public class SizeToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            if (StructExtensions.TryCase<double>(value, out double size)) return StructExtensions.ToDisylay(size);
            return string.Empty;
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
