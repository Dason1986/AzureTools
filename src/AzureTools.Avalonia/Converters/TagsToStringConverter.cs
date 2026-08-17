using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AzureTools.Converters
{
    public class TagsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string[] tags && tags.Length > 0)
                return string.Join(" ", tags);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Array.Empty<string>();
            if (value is string[] arr) return arr;
            if (value is string str) return str.Split(' ');
            return Array.Empty<string>();

        }
    }
}
