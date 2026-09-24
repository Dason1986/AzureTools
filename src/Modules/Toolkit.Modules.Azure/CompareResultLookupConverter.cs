using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Toolkit.Modules.Azure.KeyVaults;

namespace Toolkit.Modules.Azure
{
    public class CompareResultLookupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SecretCompareReport result && parameter is string param)
            {
                string[] parts = param.Split(':');
                if (parts.Length == 2)
                {
                    string envName = parts[1];
                    if (parts[0] == "status" && result.EnvStatusMap.TryGetValue(envName, out string status))
                        return status;
                    if (parts[0] == "value" && result.EnvValueMap.TryGetValue(envName, out string val))
                        return val;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}