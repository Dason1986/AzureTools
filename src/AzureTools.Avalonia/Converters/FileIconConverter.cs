using Avalonia.Data.Converters;
using FluentIcons.Common;
using System;
using System.Globalization;
using System.IO;

namespace AzureTools.Converters
{
    /// <summary>
    /// 根据文件名扩展名返回对应的 FluentIcons Icon 枚举值
    /// </summary>
    public class FileIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string name) return Icon.Document;

            string ext = Path.GetExtension(name).ToLowerInvariant();
            return ext switch
            {
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".svg" or ".ico" or ".webp" => Icon.Image,
                ".cs" or ".py" or ".js" or ".ts" or ".java" or ".c" or ".cpp" or ".h" or ".go" or ".rs" or ".php" or ".rb" or ".json" or ".xml" or ".config" or ".yml" or ".yaml" => Icon.DocumentCode,
                ".txt" or ".md" or ".log" => Icon.DocumentText,
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => Icon.Archive,
                ".pdf" => Icon.DocumentPdf,
                ".exe" or ".dll" => Icon.AppGeneric,
                _ => Icon.Document
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
