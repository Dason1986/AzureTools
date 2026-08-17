using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AzureTools.Converters
{
    public class DateTimeFormatConverter : IValueConverter
    {
        const string DefaultFormat = "yyyy-MM-dd HH:mm";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return string.Empty;

            string format = parameter as string ?? DefaultFormat;
            if (value is DateTime dt) return dt.ToString(format, culture);
            if (value is DateTimeOffset dto) return dto.ToString(format, culture);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string str || string.IsNullOrEmpty(str)) return null;

            string format = parameter as string ?? DefaultFormat;
            if ((targetType == typeof(DateTime) || targetType == typeof(DateTime?)) && DateTime.TryParseExact(str, format, culture, DateTimeStyles.None, out DateTime dt)) return dt;
            if ((targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?)) && DateTimeOffset.TryParseExact(str, format, culture, DateTimeStyles.None, out DateTimeOffset dto)) return dto;
            return null;
        }
    }
}
